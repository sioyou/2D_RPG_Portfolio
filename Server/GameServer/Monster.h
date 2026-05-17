#pragma once
#include "CreatureStat.h"

class Monster : public enable_shared_from_this<Monster>
{
public:
	int32 GetObjectId() const { return _objectId; }
	int32 GetZoneId() const { return _zoneId; }
	int32 GetTemplateId() const { return _templateId; }
	const string& GetName() const { return _name; }

	const CreatureStat& GetStat() const { return _stat; }
	CreatureStat& GetStat() { return _stat; }

	void Init(int32 objectId, int32 zoneId, int32 templateId, const string& name);

private:
	int32 _objectId = 0;
	int32 _zoneId = 0;
	int32 _templateId = 0;
	string _name;

	CreatureStat _stat;
};

using MonsterRef = shared_ptr<Monster>;
