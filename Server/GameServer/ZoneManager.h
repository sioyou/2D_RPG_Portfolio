#pragma once
#include "Zone.h"
#include "Creature.h"

class ZoneManager
{
public:
	static constexpr int32 DEFAULT_ZONE_ID = 1;

	void Init();

	ZoneRef GetZone(int32 zoneId);
	bool EnterGame(PlayerRef player, Protocol::S_C_ENTER_GAME& outPkt);
	void LeaveGame(PlayerRef player);

	void HandleCreatureDeath(ZoneRef zone, CreatureRef creature);

private:
	void BroadcastDespawn(ZoneRef zone, int32 objectId);

	ZoneRef CreateZone(int32 zoneId);
	void InitDefaultZone(ZoneRef zone);

private:
	USE_LOCK;
	HashMap<int32, ZoneRef> _zones;
};

extern ZoneManager GZoneManager;
