#pragma once
#include "Enum.pb.h"

namespace CreatureStateUtil
{
	inline int32 ToBitMask(Protocol::CreatureState state)
	{
		if (state == Protocol::CREATURE_STATE_NONE)
			return 0;

		return 1 << static_cast<int>(state);
	}

	inline bool HasFlag(int32 flags, Protocol::CreatureState state)
	{
		const int32 bit = ToBitMask(state);
		if (bit == 0)
			return flags == 0;

		return (flags & bit) != 0;
	}

	inline int32 AddFlag(int32 flags, Protocol::CreatureState state)
	{
		return flags | ToBitMask(state);
	}

	inline int32 RemoveFlag(int32 flags, Protocol::CreatureState state)
	{
		return flags & ~ToBitMask(state);
	}

	inline int32 SanitizeStateFlags(int32 clientFlags, float dirX, float dirY)
	{
		const bool dirMoving = (dirX != 0.f || dirY != 0.f);
		if (HasFlag(clientFlags, Protocol::CREATURE_STATE_MOVE) == false && dirMoving)
			clientFlags = AddFlag(clientFlags, Protocol::CREATURE_STATE_MOVE);

		if (HasFlag(clientFlags, Protocol::CREATURE_STATE_MOVE) && dirMoving == false)
			clientFlags = RemoveFlag(clientFlags, Protocol::CREATURE_STATE_MOVE);

		return clientFlags;
	}
}
