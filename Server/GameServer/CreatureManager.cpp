#include "pch.h"
#include "CreatureManager.h"
#include "ObjectIdGenerator.h"

CreatureManager GCreatureManager;

void CreatureManager::Add(CreatureRef creature)
{
	if (creature == nullptr)
		return;

	const int32 objectId = creature->GetObjectId();
	if (objectId <= 0)
		return;

	WRITE_LOCK;
	_creatures[objectId] = creature;

	cout << "[CreatureManager] Add. objectId=" << objectId
		<< " type=" << static_cast<int32>(creature->GetObjectType())
		<< " name=" << creature->GetDisplayName() << endl;
}

void CreatureManager::Remove(int32 objectId)
{
	WRITE_LOCK;
	auto it = _creatures.find(objectId);
	if (it == _creatures.end())
		return;

	cout << "[CreatureManager] Remove. objectId=" << objectId
		<< " name=" << it->second->GetDisplayName() << endl;
	_creatures.erase(it);
}

CreatureRef CreatureManager::Find(int32 objectId)
{
	READ_LOCK;
	auto it = _creatures.find(objectId);
	if (it == _creatures.end())
		return nullptr;
	return it->second;
}

MonsterRef CreatureManager::FindMonster(int32 objectId)
{
	CreatureRef creature = Find(objectId);
	if (creature == nullptr)
		return nullptr;

	if (creature->GetObjectType() != Protocol::OBJECT_TYPE_MONSTER)
		return nullptr;

	return static_pointer_cast<Monster>(creature);
}

MonsterRef CreatureManager::SpawnMonster(int32 zoneId, Protocol::MonsterType monsterType, int32 level, float posX, float posY)
{
	const int32 objectId = ObjectIdGenerator::Generate();

	MonsterRef monster = make_shared<Monster>();
	monster->Init(objectId, zoneId, monsterType, level);

	CreatureStat& stat = monster->GetStat();
	stat.SetPosition(posX, posY);

	Add(monster);

	cout << "[CreatureManager] SpawnMonster. zoneId=" << zoneId
		<< " objectId=" << objectId << " name=" << monster->GetDisplayName() << endl;
	return monster;
}

int32 CreatureManager::GetCountInZone(int32 zoneId)
{
	int32 count = 0;

	READ_LOCK;
	for (const auto& pair : _creatures)
	{
		if (pair.second->GetZoneId() == zoneId)
			++count;
	}

	return count;
}
