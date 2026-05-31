#include "pch.h"
#include "Monster.h"

void Monster::Init(int32 objectId, int32 zoneId, Protocol::MonsterType monsterType, int32 level)
{
	SetObjectId(objectId);
	SetZoneId(zoneId);
	_monsterType = monsterType;

	CreatureStat& stat = GetStat();
	stat.SetLevel(level);
	// todo : creatureType에 따른 데이터
	stat.SetMaxHp(50);
	stat.SetHp(50);
	switch (monsterType)
	{
	case Protocol::MONSTER_TYPE_FROG:
		_name = "Frog";
		break;
	case Protocol::MONSTER_TYPE_WOOD:
		_name = "Wood";
		break;
	default:
		_name = "Monster";
		break;
	}
}
