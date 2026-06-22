public static class MoveValidation
{
    public const float MaxSyncIntervalSec = 0.15f;
    public const float MaxAllowedDeltaSec = 0.5f;
    public const float MoveTolerance = 0.35f;
    public const float CheatRejectMultiplier = 2.5f;

    /// <summary>서버 보정 스냅 임계값. 이보다 크면 치트/비정상 이동으로 간주.</summary>
    public const float ServerSnapThreshold = 0.25f;

    public static float CalcMaxDistance(float moveSpeed, float deltaSeconds)
    {
        return moveSpeed * deltaSeconds + MoveTolerance;
    }

    public static float CalcCheatRejectDistance(float maxDistance)
    {
        return maxDistance * CheatRejectMultiplier;
    }
}
