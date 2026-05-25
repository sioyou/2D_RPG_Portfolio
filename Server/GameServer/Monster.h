#pragma once
#include "CreatureStat.h"
#include "Enum.pb.h"

class Monster : public enable_shared_from_this<Monster>
{
public:
	int32 GetObjectId() const { return _objectId; }
	int32 GetZoneId() const { return _zoneId; }
	Protocol::MonsterType GetTemplateId() const { return _monsterType; }
	const string& GetName() const { return _name; }

	const CreatureStat& GetStat() const { return _stat; }
	CreatureStat& GetStat() { return _stat; }

	int32 GetStateFlags() const { return _stateFlags; }
	void SetStateFlags(int32 stateFlags) { _stateFlags = stateFlags; }

	void Init(int32 objectId, int32 zoneId, Protocol::MonsterType monsterType, int32 level);

private:
	int32 _objectId = 0;
	int32 _zoneId = 0;
	Protocol::MonsterType _monsterType = Protocol::MONSTER_TYPE_NONE;
	string _name;

	CreatureStat _stat;
	int32 _stateFlags = Protocol::CREATURE_STATE_NONE;
};

using MonsterRef = shared_ptr<Monster>;
