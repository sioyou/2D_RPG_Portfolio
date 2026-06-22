#pragma once
#include "Room.h"
#include "Creature.h"

class RoomManager
{
public:
	static constexpr int32 DEFAULT_ROOM_ID = 1;

	void Init();

	RoomRef GetRoom(int32 roomId);
	bool EnterGame(PlayerRef player, Protocol::S_C_ENTER_GAME& outPkt);
	void LeaveGame(PlayerRef player);

	void HandleCreatureDeath(RoomRef room, CreatureRef creature);

private:
	void BroadcastDespawn(RoomRef room, float worldX, float worldY, int32 objectId);

	RoomRef CreateRoom(int32 roomId);
	void InitDefaultRoom(RoomRef room);

private:
	USE_LOCK;
	HashMap<int32, RoomRef> _rooms;
};

extern RoomManager GRoomManager;
