using Protocol;

public static class CreatureStateUtil
{
    public const int MoveBit = 1 << 0;
    public const int AttackBit = 1 << 1;
    public const int SkillBit = 1 << 2;
    public const int AllActionBits = MoveBit | AttackBit | SkillBit;

    public static int ToBitMask(CreatureState state)
    {
        return state switch
        {
            CreatureState.Move => MoveBit,
            CreatureState.Attack => AttackBit,
            CreatureState.Skill => SkillBit,
            _ => 0,
        };
    }

    public static bool HasFlag(int flags, CreatureState state)
    {
        int bit = ToBitMask(state);
        if (bit == 0)
            return flags == 0;

        return (flags & bit) != 0;
    }

    public static int AddFlag(int flags, CreatureState state)
    {
        return flags | ToBitMask(state);
    }

    public static int RemoveFlag(int flags, CreatureState state)
    {
        return flags & ~ToBitMask(state);
    }

    public static int Sanitize(int clientFlags, float dirX, float dirY)
    {
        int flags = clientFlags & AllActionBits;

        bool dirMoving = dirX != 0f || dirY != 0f;
        if (HasFlag(flags, CreatureState.Move) == false && dirMoving)
            flags = AddFlag(flags, CreatureState.Move);

        if (HasFlag(flags, CreatureState.Move) && dirMoving == false)
            flags = RemoveFlag(flags, CreatureState.Move);

        return flags;
    }
}
