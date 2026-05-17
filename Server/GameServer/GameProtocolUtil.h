#pragma once
#include "Protocol.pb.h"
#include "Monster.h"

namespace GameProtocolUtil
{
	void FillObjectInfo(const PlayerRef& player, Protocol::ObjectInfo* info);
	void FillObjectInfo(const MonsterRef& monster, Protocol::ObjectInfo* info);
}
