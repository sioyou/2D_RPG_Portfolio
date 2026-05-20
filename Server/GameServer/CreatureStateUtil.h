#pragma once
#include "Enum.pb.h"

namespace CreatureStateUtil
{
	constexpr int32 kMoveBit = 1 << 0;
	constexpr int32 kAttackBit = 1 << 1;
	constexpr int32 kSkillBit = 1 << 2;
	constexpr int32 kAllActionBits = kMoveBit | kAttackBit | kSkillBit;

	inline int32 ToBitMask(Protocol::CreatureState state)
	{
		switch (state)
		{
		case Protocol::CREATURE_STATE_MOVE:
			return kMoveBit;
		case Protocol::CREATURE_STATE_ATTACK:
			return kAttackBit;
		case Protocol::CREATURE_STATE_SKILL:
			return kSkillBit;
		default:
			return 0;
		}
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
		int32 flags = clientFlags & kAllActionBits;

		const bool dirMoving = (dirX != 0.f || dirY != 0.f);
		if (HasFlag(flags, Protocol::CREATURE_STATE_MOVE) == false && dirMoving)
			flags = AddFlag(flags, Protocol::CREATURE_STATE_MOVE);

		if (HasFlag(flags, Protocol::CREATURE_STATE_MOVE) && dirMoving == false)
			flags = RemoveFlag(flags, Protocol::CREATURE_STATE_MOVE);

		return flags;
	}
}
