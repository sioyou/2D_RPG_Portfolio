#pragma once
#include "Creature.h"

class Monster : public Creature
{
public:
	Protocol::GameObjectType GetObjectType() const override { return Protocol::OBJECT_TYPE_MONSTER; }
	const string& GetDisplayName() const override { return _name; }

	Protocol::MonsterType GetTemplateId() const { return _monsterType; }

	void Init(int32 objectId, int32 zoneId, Protocol::MonsterType monsterType, int32 level);

private:
	Protocol::MonsterType _monsterType = Protocol::MONSTER_TYPE_NONE;
	string _name;
};

using MonsterRef = shared_ptr<Monster>;
