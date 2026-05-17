#pragma once

class ObjectIdGenerator
{
public:
	static int32 Generate();

private:
	static Atomic<int32> _nextObjectId;
};
