#include "pch.h"
#include "Monster.h"
#include "Data/DataManager.h"

void Monster::Init(int32 objectId, int32 zoneId, Protocol::MonsterType monsterType, int32 level)
{
	SetObjectId(objectId);
	SetZoneId(zoneId);
	_monsterType = monsterType;

	CreatureStat& stat = GetStat();
	stat.SetLevel(level);

	const MonsterData* data = GDataManager.GetMonster(monsterType);
	if (data != nullptr)
	{
		stat.SetMaxHp(data->maxHp);
		stat.SetHp(data->maxHp);
		_name = data->name;
	}
	else
	{
		stat.SetMaxHp(50);
		stat.SetHp(50);
		_name = "Monster";
	}
}
