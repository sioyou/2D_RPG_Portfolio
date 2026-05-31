#pragma once
#include "Creature.h"
#include "Monster.h"

/*----------------------
	CreatureManager
	- 존 내 Player / Monster 등 Creature 통합 관리 (objectId 기준)
-----------------------*/
class CreatureManager
{
public:
	void Add(CreatureRef creature);
	void Remove(int32 objectId);

	CreatureRef Find(int32 objectId);
	MonsterRef FindMonster(int32 objectId);

	MonsterRef SpawnMonster(int32 zoneId, Protocol::MonsterType monsterType, int32 level, float posX, float posY);

	template<typename Func>
	void ForEachInZone(int32 zoneId, Func&& func)
	{
		READ_LOCK;
		for (const auto& pair : _creatures)
		{
			if (pair.second->GetZoneId() != zoneId)
				continue;

			func(pair.second);
		}
	}

	template<typename Func>
	void ForEachMonsterInZone(int32 zoneId, Func&& func)
	{
		ForEachInZone(zoneId, [&func](CreatureRef creature)
		{
			if (creature->GetObjectType() != Protocol::OBJECT_TYPE_MONSTER)
				return;

			func(static_pointer_cast<Monster>(creature));
		});
	}

	int32 GetCountInZone(int32 zoneId);

private:
	USE_LOCK;
	HashMap<int32, CreatureRef> _creatures;
};

extern CreatureManager GCreatureManager;
