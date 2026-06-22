#include "pch.h"
#include "RoomManager.h"
#include "ClientPacketHandler.h"
#include "GameProtocolUtil.h"
#include "CreatureManager.h"
#include "Data/DataManager.h"

RoomManager GRoomManager;

void RoomManager::Init()
{
	RoomRef room = CreateRoom(DEFAULT_ROOM_ID);
	InitDefaultRoom(room);
	cout << "[RoomManager] Init complete. defaultRoomId=" << DEFAULT_ROOM_ID << endl;
}

RoomRef RoomManager::GetRoom(int32 roomId)
{
	READ_LOCK;
	auto it = _rooms.find(roomId);
	if (it == _rooms.end())
		return nullptr;
	return it->second;
}

bool RoomManager::EnterGame(PlayerRef player, Protocol::S_C_ENTER_GAME& outPkt)
{
	outPkt.set_success(false);
	outPkt.set_myobjectid(0);

	if (player == nullptr)
		return false;

	RoomRef room = GetRoom(DEFAULT_ROOM_ID);
	if (room == nullptr)
		return false;

	const bool wasInRoom = room->HasPlayer(player);

	if (room->EnterPlayer(player) == false)
		return false;

	if (wasInRoom == false)
	{
		Protocol::S_C_SPAWN spawnPkt;
		GameProtocolUtil::FillPlayerSpawn(player, spawnPkt.mutable_spawn());
		SendBufferRef spawnBuffer = ClientPacketHandler::MakeSendBuffer(spawnPkt);
		room->BroadcastToView(
			player->GetStat().GetPosX(),
			player->GetStat().GetPosY(),
			spawnBuffer,
			player->GetSession());
	}

	player->SetState(EPlayerState::InGame);
	player->SetLastMoveTick(0);

	outPkt.set_success(true);
	outPkt.set_myobjectid(player->GetObjectId());
	room->FillEnterGameSpawns(outPkt, player);
	return true;
}

void RoomManager::LeaveGame(PlayerRef player)
{
	if (player == nullptr)
		return;

	RoomRef room = GetRoom(player->GetRoomId());
	if (room == nullptr)
	{
		player->SetState(EPlayerState::Lobby);
		return;
	}

	if (player->GetState() == EPlayerState::InGame)
	{
		BroadcastDespawn(
			room,
			player->GetStat().GetPosX(),
			player->GetStat().GetPosY(),
			player->GetObjectId());
	}

	player->SetMoveDirection(0.f, 0.f);
	player->SetLastMoveTick(0);
	room->LeavePlayer(player);
	player->SetState(EPlayerState::Lobby);
}

void RoomManager::BroadcastDespawn(RoomRef room, float worldX, float worldY, int32 objectId)
{
	if (room == nullptr || objectId <= 0)
		return;

	Protocol::S_C_DESPAWN despawnPkt;
	despawnPkt.set_objectid(objectId);
	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(despawnPkt);
	room->BroadcastToView(worldX, worldY, sendBuffer);
}

void RoomManager::HandleCreatureDeath(RoomRef room, CreatureRef creature)
{
	if (room == nullptr || creature == nullptr)
		return;

	Protocol::S_C_DIE diePkt;
	diePkt.set_objectid(creature->GetObjectId());
	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(diePkt);
	room->BroadcastToView(
		creature->GetStat().GetPosX(),
		creature->GetStat().GetPosY(),
		sendBuffer);
}

RoomRef RoomManager::CreateRoom(int32 roomId)
{
	WRITE_LOCK;

	auto it = _rooms.find(roomId);
	if (it != _rooms.end())
		return it->second;

	const RoomData* roomData = GDataManager.GetRoom(roomId);
	RoomData config;
	if (roomData != nullptr)
		config = *roomData;
	else
		config.roomId = roomId;

	RoomRef room = make_shared<Room>(roomId, config);
	_rooms[roomId] = room;
	return room;
}

void RoomManager::InitDefaultRoom(RoomRef room)
{
	if (room == nullptr)
		return;

	const int32 roomId = room->GetRoomId();
	const std::vector<SpawnEntryData>& spawns = GDataManager.GetRoomSpawns(roomId);

	for (const SpawnEntryData& entry : spawns)
	{
		MonsterRef monster = GCreatureManager.SpawnMonster(
			roomId,
			static_cast<Protocol::MonsterType>(entry.monsterType),
			entry.level,
			entry.posX,
			entry.posY);

		if (monster != nullptr)
			room->RegisterCreature(monster);
	}
}
