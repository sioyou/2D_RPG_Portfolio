#pragma once
#include "Creature.h"
#include "Monster.h"

/*----------------------
	CreatureManager
	- Room 내 Player / Monster 등 Creature 통합 관리 (objectId 기준)
-----------------------*/
class CreatureManager
{
public:
	void Add(CreatureRef creature);
	void Remove(int32 objectId);

	CreatureRef Find(int32 objectId);
	MonsterRef FindMonster(int32 objectId);

	MonsterRef SpawnMonster(int32 roomId, Protocol::MonsterType monsterType, int32 level, float posX, float posY);

	template<typename Func>
	void ForEachInRoom(int32 roomId, Func&& func)
	{
		READ_LOCK;
		for (const auto& pair : _creatures)
		{
			if (pair.second->GetRoomId() != roomId)
				continue;

			func(pair.second);
		}
	}

	template<typename Func>
	void ForEachMonsterInRoom(int32 roomId, Func&& func)
	{
		ForEachInRoom(roomId, [&func](CreatureRef creature)
		{
			if (creature->GetObjectType() != Protocol::OBJECT_TYPE_MONSTER)
				return;

			func(static_pointer_cast<Monster>(creature));
		});
	}

	int32 GetCountInRoom(int32 roomId);

private:
	USE_LOCK;
	HashMap<int32, CreatureRef> _creatures;
};

extern CreatureManager GCreatureManager;
