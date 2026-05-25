#include "pch.h"
#include "Zone.h"
#include "GameSession.h"
#include "GameProtocolUtil.h"
#include "MonsterManager.h"

namespace 
{
	constexpr float MOVE_SPEED = 5.f;
	constexpr float MAX_SYNC_INTERVAL_SEC = 0.15f;
	constexpr float MOVE_TOLERANCE = 0.35f;
}

Zone::Zone(int32 zoneId)
	: _zoneId(zoneId)
{
}

bool Zone::HasPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	READ_LOCK;
	return _players.find(player->GetObjectId()) != _players.end();
}

bool Zone::EnterPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	GameSessionRef session = player->GetSession();
	if (session == nullptr)
		return false;

	WRITE_LOCK;

	auto it = _players.find(player->GetObjectId());
	if (it != _players.end())
		return it->second == player;

	player->SetZoneId(_zoneId);
	_players[player->GetObjectId()] = player;
	_sessions.insert(session);
	return true;
}

void Zone::LeavePlayer(PlayerRef player)
{
	if (player == nullptr)
		return;

	GameSessionRef session = player->GetSession();

	WRITE_LOCK;
	_players.erase(player->GetObjectId());

	if (session != nullptr)
		_sessions.erase(session);

	player->SetZoneId(0);
}

void Zone::FillEnterGameSpawns(Protocol::S_C_ENTER_GAME& pkt)
{
	{
		READ_LOCK;
		for (const auto& pair : _players)
		{
			Protocol::SpawnEntry* spawn = pkt.add_spawns();
			GameProtocolUtil::FillPlayerSpawn(pair.second, spawn);
		}
	}

	GMonsterManager.ForEachInZone(_zoneId, [&pkt](MonsterRef monster)
		{
			Protocol::SpawnEntry* spawn = pkt.add_spawns();
			GameProtocolUtil::FillMonsterSpawn(monster, spawn);
		});
}

void Zone::Broadcast(SendBufferRef sendBuffer)
{
	if (sendBuffer == nullptr)
		return;

	Vector<GameSessionRef> targets;
	{
		READ_LOCK;
		targets.reserve(_sessions.size());
		for (GameSessionRef session : _sessions)
			targets.push_back(session);
	}

	for (GameSessionRef session : targets)
		session->Send(sendBuffer);
}

void Zone::ValidateClientPosition(PlayerRef player, float clientX, float clientY, float& outX, float& outY)
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

	if (deltaSeconds > MAX_SYNC_INTERVAL_SEC)
		deltaSeconds = MAX_SYNC_INTERVAL_SEC;
	if (deltaSeconds < 0.001f)
		deltaSeconds = MAX_SYNC_INTERVAL_SEC;

	const float maxDistance = MOVE_SPEED * deltaSeconds + MOVE_TOLERANCE;

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

PlayerRef Zone::FindPlayer(int32 objectId)
{
	READ_LOCK;
	auto it = _players.find(objectId);
	if (it == _players.end())
		return nullptr;
	return it->second;
}
