using Protocol;

public static class CreatureStateUtil
{
    public static int ToBitMask(CreatureState state)
    {
        if (state == CreatureState.None)
            return 0;

        return 1 << (int)state;
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
        bool dirMoving = dirX != 0f || dirY != 0f;
        if (HasFlag(clientFlags, CreatureState.Move) && dirMoving == false)
            clientFlags = RemoveFlag(clientFlags, CreatureState.Move);

        return clientFlags;
    }
}
