#pragma once
#include "Protocol.pb.h"
#include "Monster.h"

namespace GameProtocolUtil
{
	void FillObjectInfo(const PlayerRef& player, Protocol::ObjectInfo* info);
	void FillObjectInfo(const MonsterRef& monster, Protocol::ObjectInfo* info);

	void FillPlayerSpawn(const PlayerRef& player, Protocol::SpawnEntry* entry);
	void FillMonsterSpawn(const MonsterRef& monster, Protocol::SpawnEntry* entry);
}
