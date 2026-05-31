#include "pch.h"
#include "GameProtocolUtil.h"

namespace
{
	void FillFacing(float dirX, float dirY, Protocol::ObjectInfo* info)
	{
		if (dirX == 0.f && dirY == 0.f)
		{
			dirX = 1.f;
			dirY = 0.f;
		}

		info->set_dirx(dirX);
		info->set_diry(dirY);
	}

	void FillStat(const CreatureStat& stat, int32 stateFlags, Protocol::ObjectInfo* info)
	{
		info->set_level(stat.GetLevel());
		info->set_hp(stat.GetHp());
		info->set_maxhp(stat.GetMaxHp());
		info->set_posx(stat.GetPosX());
		info->set_posy(stat.GetPosY());
		info->set_stateflags(stateFlags);
	}
}

void GameProtocolUtil::FillObjectInfo(const CreatureRef& creature, Protocol::ObjectInfo* info)
{
	if (creature == nullptr || info == nullptr)
		return;

	info->set_objectid(creature->GetObjectId());
	info->set_objecttype(creature->GetObjectType());
	info->set_name(creature->GetDisplayName());
	FillStat(creature->GetStat(), creature->GetStateFlags(), info);
	FillFacing(creature->GetMoveDirX(), creature->GetMoveDirY(), info);
}

void GameProtocolUtil::FillPlayerSpawn(const PlayerRef& player, Protocol::SpawnEntry* entry)
{
	if (player == nullptr || entry == nullptr)
		return;

	FillObjectInfo(static_pointer_cast<Creature>(player), entry->mutable_player());
}

void GameProtocolUtil::FillMonsterSpawn(const MonsterRef& monster, Protocol::SpawnEntry* entry)
{
	if (monster == nullptr || entry == nullptr)
		return;

	Protocol::MonsterInfo* monsterInfo = entry->mutable_monster();
	FillObjectInfo(static_pointer_cast<Creature>(monster), monsterInfo->mutable_objectinfo());
	monsterInfo->set_monstertype(monster->GetTemplateId());
}

void GameProtocolUtil::FillSpawnEntry(const CreatureRef& creature, Protocol::SpawnEntry* entry)
{
	if (creature == nullptr || entry == nullptr)
		return;

	switch (creature->GetObjectType())
	{
	case Protocol::OBJECT_TYPE_PLAYER:
		FillPlayerSpawn(static_pointer_cast<Player>(creature), entry);
		break;
	case Protocol::OBJECT_TYPE_MONSTER:
		FillMonsterSpawn(static_pointer_cast<Monster>(creature), entry);
		break;
	default:
		break;
	}
}
