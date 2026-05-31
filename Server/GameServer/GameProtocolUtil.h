#pragma once
#include "Protocol.pb.h"
#include "Creature.h"
#include "Player.h"
#include "Monster.h"

namespace GameProtocolUtil
{
	void FillObjectInfo(const CreatureRef& creature, Protocol::ObjectInfo* info);

	void FillPlayerSpawn(const PlayerRef& player, Protocol::SpawnEntry* entry);
	void FillMonsterSpawn(const MonsterRef& monster, Protocol::SpawnEntry* entry);
	void FillSpawnEntry(const CreatureRef& creature, Protocol::SpawnEntry* entry);
}
