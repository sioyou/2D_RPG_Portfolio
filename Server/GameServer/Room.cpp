#include "pch.h"
#include "Room.h"
#include <algorithm>
#include "GameSession.h"
#include "GameProtocolUtil.h"
#include "ClientPacketHandler.h"
#include "CreatureManager.h"
#include "PlayerManager.h"
#include "Player.h"

namespace
{
	constexpr float MAX_SYNC_INTERVAL_SEC = 0.15f;
	constexpr float MAX_ALLOWED_DELTA_SEC = 0.5f;
	constexpr float MOVE_TOLERANCE = 0.35f;
}

Room::Room(int32 roomId, const RoomData& config)
	: _roomId(roomId)
	, _originX(config.originX)
	, _originY(config.originY)
	, _zoneSize(config.zoneSize > 0.f ? config.zoneSize : 10.f)
	, _viewRadius(config.viewRadius > 0.f ? config.viewRadius : _zoneSize * 0.45f)
	, _spawnPosX(config.spawnPosX)
	, _spawnPosY(config.spawnPosY)
	, _zoneCountX(config.zoneCountX > 0 ? config.zoneCountX : 1)
	, _zoneCountY(config.zoneCountY > 0 ? config.zoneCountY : 1)
{
	const int32 zoneCount = _zoneCountX * _zoneCountY;
	_zones.reserve(zoneCount);

	for (int32 i = 0; i < zoneCount; ++i)
		_zones.push_back(make_shared<Zone>(i));
}

int32 Room::WorldToZoneIndex(float worldX, float worldY) const
{
	int32 zoneX = static_cast<int32>(floorf((worldX - _originX) / _zoneSize));
	int32 zoneY = static_cast<int32>(floorf((worldY - _originY) / _zoneSize));

	zoneX = std::clamp(zoneX, 0, _zoneCountX - 1);
	zoneY = std::clamp(zoneY, 0, _zoneCountY - 1);

	return zoneY * _zoneCountX + zoneX;
}

void Room::ZoneIndexToCoord(int32 zoneIndex, int32& outX, int32& outY) const
{
	outX = zoneIndex % _zoneCountX;
	outY = zoneIndex / _zoneCountX;
}

bool Room::IsValidZoneIndex(int32 zoneIndex) const
{
	return zoneIndex >= 0 && zoneIndex < static_cast<int32>(_zones.size());
}

ZoneRef Room::GetZone(int32 zoneIndex) const
{
	if (IsValidZoneIndex(zoneIndex) == false)
		return nullptr;

	return _zones[zoneIndex];
}

void Room::CollectVisibleZoneIndices(float worldX, float worldY, Vector<int32>& out) const
{
	out.clear();

	const float left = worldX - _viewRadius;
	const float right = worldX + _viewRadius;
	const float bottom = worldY - _viewRadius;
	const float top = worldY + _viewRadius;

	int32 minZoneX = static_cast<int32>(floorf((left - _originX) / _zoneSize));
	int32 maxZoneX = static_cast<int32>(floorf((right - _originX) / _zoneSize));
	int32 minZoneY = static_cast<int32>(floorf((bottom - _originY) / _zoneSize));
	int32 maxZoneY = static_cast<int32>(floorf((top - _originY) / _zoneSize));

	minZoneX = std::clamp(minZoneX, 0, _zoneCountX - 1);
	maxZoneX = std::clamp(maxZoneX, 0, _zoneCountX - 1);
	minZoneY = std::clamp(minZoneY, 0, _zoneCountY - 1);
	maxZoneY = std::clamp(maxZoneY, 0, _zoneCountY - 1);

	for (int32 y = minZoneY; y <= maxZoneY; ++y)
	{
		for (int32 x = minZoneX; x <= maxZoneX; ++x)
			out.push_back(y * _zoneCountX + x);
	}
}

bool Room::CanSee(float viewerX, float viewerY, float targetX, float targetY) const
{
	const float dx = targetX - viewerX;
	const float dy = targetY - viewerY;
	const float distSq = dx * dx + dy * dy;
	const float viewRadiusSq = _viewRadius * _viewRadius;
	if (distSq > viewRadiusSq)
		return false;

	Vector<int32> visible;
	CollectVisibleZoneIndices(viewerX, viewerY, visible);

	const int32 targetZone = WorldToZoneIndex(targetX, targetY);
	return std::find(visible.begin(), visible.end(), targetZone) != visible.end();
}

void Room::CollectSessionsWhoCanSee(float targetX, float targetY, Vector<GameSessionRef>& out, GameSessionRef excludeSession)
{
	out.clear();

	READ_LOCK;
	for (const auto& pair : _players)
	{
		PlayerRef player = pair.second;
		if (player == nullptr)
			continue;

		GameSessionRef session = player->GetSession();
		if (session == nullptr || session == excludeSession)
			continue;

		const float viewerX = player->GetStat().GetPosX();
		const float viewerY = player->GetStat().GetPosY();
		if (CanSee(viewerX, viewerY, targetX, targetY) == false)
			continue;

		out.push_back(session);
	}
}

void Room::SendToSessions(const Vector<GameSessionRef>& sessions, SendBufferRef sendBuffer)
{
	if (sendBuffer == nullptr)
		return;

	for (GameSessionRef session : sessions)
	{
		if (session != nullptr)
			session->Send(sendBuffer);
	}
}

void Room::EnterZone(PlayerRef player, int32 zoneIndex)
{
	ZoneRef zone = GetZone(zoneIndex);
	if (zone == nullptr || player == nullptr)
		return;

	zone->AddPlayer(player);
	player->SetZoneIndex(zoneIndex);
}

void Room::LeaveZone(PlayerRef player, int32 zoneIndex)
{
	ZoneRef zone = GetZone(zoneIndex);
	if (zone == nullptr || player == nullptr)
		return;

	zone->RemovePlayer(player->GetObjectId());
}

void Room::SendSpawnToSession(CreatureRef creature, GameSessionRef session)
{
	if (creature == nullptr || session == nullptr)
		return;

	Protocol::S_C_SPAWN spawnPkt;
	GameProtocolUtil::FillSpawnEntry(creature, spawnPkt.mutable_spawn());
	session->Send(ClientPacketHandler::MakeSendBuffer(spawnPkt));
}

void Room::SendDespawnToSession(int32 objectId, GameSessionRef session)
{
	if (objectId <= 0 || session == nullptr)
		return;

	Protocol::S_C_DESPAWN despawnPkt;
	despawnPkt.set_objectid(objectId);
	session->Send(ClientPacketHandler::MakeSendBuffer(despawnPkt));
}

void Room::SyncOtherPlayersViewOfMover(PlayerRef player, float oldX, float oldY, float newX, float newY)
{
	if (player == nullptr)
		return;

	READ_LOCK;
	for (const auto& pair : _players)
	{
		PlayerRef other = pair.second;
		if (other == nullptr || other == player)
			continue;

		GameSessionRef otherSession = other->GetSession();
		if (otherSession == nullptr)
			continue;

		const float viewerX = other->GetStat().GetPosX();
		const float viewerY = other->GetStat().GetPosY();

		const bool couldSee = CanSee(viewerX, viewerY, oldX, oldY);
		const bool canSeeNow = CanSee(viewerX, viewerY, newX, newY);

		if (canSeeNow && couldSee == false)
			SendSpawnToSession(player, otherSession);

		if (canSeeNow == false && couldSee)
			SendDespawnToSession(player->GetObjectId(), otherSession);
	}
}

void Room::SyncMoverViewOfEntities(PlayerRef player, float oldX, float oldY, float newX, float newY)
{
	if (player == nullptr)
		return;

	GameSessionRef playerSession = player->GetSession();
	if (playerSession == nullptr)
		return;

	Vector<int32> oldVisible;
	Vector<int32> newVisible;
	CollectVisibleZoneIndices(oldX, oldY, oldVisible);
	CollectVisibleZoneIndices(newX, newY, newVisible);

	Set<int32> zonesToCheck;
	for (int32 idx : oldVisible)
		zonesToCheck.insert(idx);
	for (int32 idx : newVisible)
		zonesToCheck.insert(idx);

	for (int32 idx : zonesToCheck)
	{
		ZoneRef zone = GetZone(idx);
		if (zone == nullptr)
			continue;

		zone->ForEachCreature([&](CreatureRef creature)
			{
				if (creature->GetObjectId() == player->GetObjectId())
					return;

				const float cx = creature->GetStat().GetPosX();
				const float cy = creature->GetStat().GetPosY();

				const bool couldSee = CanSee(oldX, oldY, cx, cy);
				const bool canSeeNow = CanSee(newX, newY, cx, cy);

				if (canSeeNow && couldSee == false)
					SendSpawnToSession(creature, playerSession);

				if (canSeeNow == false && couldSee)
					SendDespawnToSession(creature->GetObjectId(), playerSession);
			});
	}
}

void Room::SyncPlayerVisibility(PlayerRef player, float oldX, float oldY, float newX, float newY)
{
	if (player == nullptr)
		return;

	SyncOtherPlayersViewOfMover(player, oldX, oldY, newX, newY);
	SyncMoverViewOfEntities(player, oldX, oldY, newX, newY);
}

bool Room::HasPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	READ_LOCK;
	return _players.find(player->GetObjectId()) != _players.end();
}

bool Room::EnterPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	GameSessionRef session = player->GetSession();
	if (session == nullptr)
		return false;

	{
		WRITE_LOCK;

		auto it = _players.find(player->GetObjectId());
		if (it != _players.end())
		{
			if (it->second != player)
				return false;

			GameSessionRef oldSession = player->GetSession();
			if (oldSession != session)
				GPlayerManager.UpdateSession(player, session);

			return true;
		}

		player->GetStat().SetPosition(_spawnPosX, _spawnPosY);
		player->SetRoomId(_roomId);
		_players[player->GetObjectId()] = player;
	}

	const float posX = player->GetStat().GetPosX();
	const float posY = player->GetStat().GetPosY();
	const int32 zoneIndex = WorldToZoneIndex(posX, posY);
	EnterZone(player, zoneIndex);

	return true;
}

void Room::LeavePlayer(PlayerRef player)
{
	if (player == nullptr)
		return;

	const int32 zoneIndex = player->GetZoneIndex();

	{
		WRITE_LOCK;
		_players.erase(player->GetObjectId());
	}

	if (IsValidZoneIndex(zoneIndex))
		LeaveZone(player, zoneIndex);

	player->SetRoomId(0);
	player->SetZoneIndex(-1);
}

void Room::OnPlayerMoved(PlayerRef player, float oldX, float oldY, float newX, float newY)
{
	if (player == nullptr)
		return;

	const int32 oldIndex = WorldToZoneIndex(oldX, oldY);
	const int32 newIndex = WorldToZoneIndex(newX, newY);
	if (oldIndex != newIndex)
	{
		LeaveZone(player, oldIndex);
		EnterZone(player, newIndex);
	}

	SyncPlayerVisibility(player, oldX, oldY, newX, newY);
}

void Room::AppendOtherCreaturesInZones(Protocol::S_C_ENTER_GAME& pkt, PlayerRef player, const Vector<int32>& zoneIndices) const
{
	if (player == nullptr)
		return;

	const float posX = player->GetStat().GetPosX();
	const float posY = player->GetStat().GetPosY();

	for (int32 idx : zoneIndices)
	{
		ZoneRef zone = GetZone(idx);
		if (zone == nullptr)
			continue;

		zone->ForEachCreature([&](CreatureRef creature)
			{
				if (creature->GetObjectId() == player->GetObjectId())
					return;

				const float cx = creature->GetStat().GetPosX();
				const float cy = creature->GetStat().GetPosY();
				if (CanSee(posX, posY, cx, cy) == false)
					return;

				Protocol::SpawnEntry* spawn = pkt.add_spawns();
				GameProtocolUtil::FillSpawnEntry(creature, spawn);
			});
	}
}

void Room::FillEnterGameSpawns(Protocol::S_C_ENTER_GAME& pkt, PlayerRef player)
{
	if (player == nullptr)
		return;

	GameProtocolUtil::FillPlayerSpawn(player, pkt.add_spawns());

	const float posX = player->GetStat().GetPosX();
	const float posY = player->GetStat().GetPosY();

	Vector<int32> visible;
	CollectVisibleZoneIndices(posX, posY, visible);
	AppendOtherCreaturesInZones(pkt, player, visible);
}

void Room::BroadcastToView(float worldX, float worldY, SendBufferRef sendBuffer, GameSessionRef excludeSession)
{
	Vector<GameSessionRef> sessions;
	CollectSessionsWhoCanSee(worldX, worldY, sessions, excludeSession);
	SendToSessions(sessions, sendBuffer);
}

void Room::ValidateClientPosition(PlayerRef player, float clientX, float clientY, float& outX, float& outY)
{
	CreatureStat& stat = player->GetStat();
	const float serverX = stat.GetPosX();
	const float serverY = stat.GetPosY();

	const uint64 now = ::GetTickCount64();
	uint64 lastTick = player->GetLastMoveTick();
	player->SetLastMoveTick(now);

	float deltaSeconds = MAX_SYNC_INTERVAL_SEC;
	if (lastTick > 0 && now > lastTick)
		deltaSeconds = static_cast<float>(now - lastTick) / 1000.f;

	if (deltaSeconds > MAX_ALLOWED_DELTA_SEC)
		deltaSeconds = MAX_ALLOWED_DELTA_SEC;
	if (deltaSeconds < 0.001f)
		deltaSeconds = MAX_SYNC_INTERVAL_SEC;

	const float moveSpeed = player->GetMoveSpeed();
	const float maxDistance = moveSpeed * deltaSeconds + MOVE_TOLERANCE;

	const float dx = clientX - serverX;
	const float dy = clientY - serverY;
	const float distanceSq = dx * dx + dy * dy;

	if (distanceSq <= maxDistance * maxDistance || distanceSq < 0.0001f)
	{
		outX = clientX;
		outY = clientY;
		return;
	}

	const float distance = sqrtf(distanceSq);
	const float ratio = maxDistance / distance;
	outX = serverX + dx * ratio;
	outY = serverY + dy * ratio;
}

PlayerRef Room::FindPlayer(int32 objectId)
{
	READ_LOCK;
	auto it = _players.find(objectId);
	if (it == _players.end())
		return nullptr;
	return it->second;
}

void Room::RegisterCreature(CreatureRef creature)
{
	if (creature == nullptr)
		return;

	const int32 zoneIndex = WorldToZoneIndex(creature->GetStat().GetPosX(), creature->GetStat().GetPosY());
	creature->SetRoomId(_roomId);
	creature->SetZoneIndex(zoneIndex);

	ZoneRef zone = GetZone(zoneIndex);
	if (zone != nullptr)
		zone->AddCreature(creature);
}

void Room::UnregisterCreature(CreatureRef creature)
{
	if (creature == nullptr)
		return;

	const int32 zoneIndex = creature->GetZoneIndex();
	ZoneRef zone = GetZone(zoneIndex);
	if (zone != nullptr)
		zone->RemoveCreature(creature->GetObjectId());

	creature->SetRoomId(0);
	creature->SetZoneIndex(-1);
}
