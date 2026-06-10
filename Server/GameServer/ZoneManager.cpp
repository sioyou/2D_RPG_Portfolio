#include "pch.h"
#include "ZoneManager.h"
#include "ClientPacketHandler.h"
#include "GameProtocolUtil.h"
#include "CreatureManager.h"
#include "Data/DataManager.h"

ZoneManager GZoneManager;

void ZoneManager::Init()
{
	ZoneRef zone = CreateZone(DEFAULT_ZONE_ID);
	InitDefaultZone(zone);
	cout << "[ZoneManager] Init complete. defaultZoneId=" << DEFAULT_ZONE_ID << endl;
}

ZoneRef ZoneManager::GetZone(int32 zoneId)
{
	READ_LOCK;
	auto it = _zones.find(zoneId);
	if (it == _zones.end())
		return nullptr;
	return it->second;
}

bool ZoneManager::EnterGame(PlayerRef player, Protocol::S_C_ENTER_GAME& outPkt)
{
	outPkt.set_success(false);
	outPkt.set_myobjectid(0);

	if (player == nullptr)
		return false;

	ZoneRef zone = GetZone(DEFAULT_ZONE_ID);
	if (zone == nullptr)
		return false;

	if (zone->HasPlayer(player) == false)
	{
		Protocol::S_C_SPAWN spawnPkt;
		GameProtocolUtil::FillPlayerSpawn(player, spawnPkt.mutable_spawn());
		SendBufferRef spawnBuffer = ClientPacketHandler::MakeSendBuffer(spawnPkt);
		zone->Broadcast(spawnBuffer);
	}

	if (zone->EnterPlayer(player) == false)
		return false;

	player->SetState(EPlayerState::InGame);
	player->SetLastMoveTick(0);

	outPkt.set_success(true);
	outPkt.set_myobjectid(player->GetObjectId());
	zone->FillEnterGameSpawns(outPkt);
	return true;
}

void ZoneManager::LeaveGame(PlayerRef player)
{
	if (player == nullptr)
		return;

	ZoneRef zone = GetZone(player->GetZoneId());
	if (zone == nullptr)
	{
		player->SetState(EPlayerState::Lobby);
		return;
	}

	if (player->GetState() == EPlayerState::InGame)
		BroadcastDespawn(zone, player->GetObjectId());

	player->SetMoveDirection(0.f, 0.f);
	player->SetLastMoveTick(0);
	zone->LeavePlayer(player);
	player->SetState(EPlayerState::Lobby);
}

void ZoneManager::BroadcastDespawn(ZoneRef zone, int32 objectId)
{
	if (zone == nullptr || objectId <= 0)
		return;

	Protocol::S_C_DESPAWN despawnPkt;
	despawnPkt.set_objectid(objectId);
	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(despawnPkt);
	zone->Broadcast(sendBuffer);
}

void ZoneManager::HandleCreatureDeath(ZoneRef zone, CreatureRef creature)
{
	if (zone == nullptr || creature == nullptr)
		return;

	Protocol::S_C_DIE diePkt;
	diePkt.set_objectid(creature->GetObjectId());
	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(diePkt);
	zone->Broadcast(sendBuffer);
}

ZoneRef ZoneManager::CreateZone(int32 zoneId)
{
	WRITE_LOCK;

	auto it = _zones.find(zoneId);
	if (it != _zones.end())
		return it->second;

	ZoneRef zone = make_shared<Zone>(zoneId);
	_zones[zoneId] = zone;
	return zone;
}

void ZoneManager::InitDefaultZone(ZoneRef zone)
{
	if (zone == nullptr)
		return;

	const int32 zoneId = zone->GetZoneId();
	const std::vector<SpawnEntryData>& spawns = GDataManager.GetZoneSpawns(zoneId);

	for (const SpawnEntryData& entry : spawns)
	{
		GCreatureManager.SpawnMonster(
			zoneId,
			static_cast<Protocol::MonsterType>(entry.monsterType),
			entry.level,
			entry.posX,
			entry.posY);
	}
}
