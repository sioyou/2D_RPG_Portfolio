#include "pch.h"
#include "Monster.h"

void Monster::Init(int32 objectId, int32 zoneId, int32 templateId, const string& name)
{
	_objectId = objectId;
	_zoneId = zoneId;
	_templateId = templateId;
	_name = name;
}
