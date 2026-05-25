#include "pch.h"
#include "MonsterManager.h"
#include "ObjectIdGenerator.h"

MonsterManager GMonsterManager;

MonsterRef MonsterManager::Spawn(int32 zoneId, Protocol::MonsterType monsterType, int32 level, float posX, float posY)
{
	const int32 objectId = ObjectIdGenerator::Generate();

	MonsterRef monster = make_shared<Monster>();
	monster->Init(objectId, zoneId, monsterType, level);

	CreatureStat& stat = monster->GetStat();
	stat.SetPosition(posX, posY);

	WRITE_LOCK;
	_monsters[objectId] = monster;

	cout << "[MonsterManager] Spawn. zoneId=" << zoneId
		<< " objectId=" << objectId << " name=" << monster->GetName() << endl;
	return monster;
}

void MonsterManager::Despawn(int32 objectId)
{
	WRITE_LOCK;
	_monsters.erase(objectId);
}

MonsterRef MonsterManager::FindByObjectId(int32 objectId)
{
	READ_LOCK;
	auto it = _monsters.find(objectId);
	if (it == _monsters.end())
		return nullptr;
	return it->second;
}

int32 MonsterManager::GetCountInZone(int32 zoneId)
{
	int32 count = 0;

	READ_LOCK;
	for (const auto& pair : _monsters)
	{
		if (pair.second->GetZoneId() == zoneId)
			++count;
	}

	return count;
}
