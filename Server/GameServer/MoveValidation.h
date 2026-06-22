#pragma once

namespace MoveValidation
{
	inline constexpr float MAX_SYNC_INTERVAL_SEC = 0.15f;
	inline constexpr float MAX_ALLOWED_DELTA_SEC = 0.5f;
	inline constexpr float MOVE_TOLERANCE = 0.35f;
	inline constexpr float CHEAT_REJECT_MULTIPLIER = 2.5f;

	inline float CalcMaxDistance(float moveSpeed, float deltaSeconds)
	{
		return moveSpeed * deltaSeconds + MOVE_TOLERANCE;
	}

	inline float CalcCheatRejectDistance(float maxDistance)
	{
		return maxDistance * CHEAT_REJECT_MULTIPLIER;
	}
}
