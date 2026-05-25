#include "pch.h"
#include "GameProtocolUtil.h"
#include "Player.h"
#include "Monster.h"

namespace
{
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

void GameProtocolUtil::FillObjectInfo(const PlayerRef& player, Protocol::ObjectInfo* info)
{
	if (player == nullptr || info == nullptr)
		return;

	info->set_objectid(player->GetObjectId());
	info->set_objecttype(Protocol::OBJECT_TYPE_PLAYER);
	info->set_name(player->GetPlayerId());
	FillStat(player->GetStat(), player->GetStateFlags(), info);
}

void GameProtocolUtil::FillObjectInfo(const MonsterRef& monster, Protocol::ObjectInfo* info)
{
	if (monster == nullptr || info == nullptr)
		return;

	info->set_objectid(monster->GetObjectId());
	info->set_objecttype(Protocol::OBJECT_TYPE_MONSTER);
	info->set_name(monster->GetName());
	FillStat(monster->GetStat(), monster->GetStateFlags(), info);
}

void GameProtocolUtil::FillPlayerSpawn(const PlayerRef& player, Protocol::SpawnEntry* entry)
{
	if (player == nullptr || entry == nullptr)
		return;

	FillObjectInfo(player, entry->mutable_player());
}

void GameProtocolUtil::FillMonsterSpawn(const MonsterRef& monster, Protocol::SpawnEntry* entry)
{
	if (monster == nullptr || entry == nullptr)
		return;

	Protocol::MonsterInfo* monsterInfo = entry->mutable_monster();
	FillObjectInfo(monster, monsterInfo->mutable_objectinfo());
	monsterInfo->set_monstertype(monster->GetTemplateId());
}
