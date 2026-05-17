#include "pch.h"
#include "ObjectIdGenerator.h"

Atomic<int32> ObjectIdGenerator::_nextObjectId{ 1 };

int32 ObjectIdGenerator::Generate()
{
	return _nextObjectId.fetch_add(1);
}
