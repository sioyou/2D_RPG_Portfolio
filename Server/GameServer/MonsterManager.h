#pragma once
#include "Monster.h"

/*----------------------
	MonsterManager
	- 몬스터 생성/제거 및 objectId / zoneId 조회
-----------------------*/
class MonsterManager
{
public:
	MonsterRef Spawn(int32 zoneId, int32 templateId, const string& name, int32 level, int32 maxHp, float posX, float posY);
	void Despawn(int32 objectId);

	MonsterRef FindByObjectId(int32 objectId);

	template<typename Func>
	void ForEachInZone(int32 zoneId, Func&& func)
	{
		READ_LOCK;
		for (const auto& pair : _monsters)
		{
			if (pair.second->GetZoneId() != zoneId)
				continue;

			func(pair.second);
		}
	}

	int32 GetCountInZone(int32 zoneId);

private:
	USE_LOCK;
	HashMap<int32, MonsterRef> _monsters;
};

extern MonsterManager GMonsterManager;
