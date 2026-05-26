using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Define
{
    public const char MAP_TOOL_WALL = '0';
    public const char MAP_TOOL_NONE = '1';

	public enum EScene
    {
        Unknown,
        TitleScene,
        GameScene,
    }

    public enum ESound
    {
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum ETouchEvent
    {
        PointerUp,
        PointerDown,
        Click,
        Pressed,
        BeginDrag,
        Drag,
        EndDrag,
    }

	public enum ELayer
    {
    }
}

public static class SortingLayers
{
	public const int PLAYER = 200;
	public const int MONSTER = 300;
	public const int BOSS = 300;
	public const int PROJECTILE = 310;
	public const int DROP_ITEM = 310;
	public const int SKILL_EFFECT = 315;
	public const int DAMAGE_FONT = 410;
}

public static class AnimName
{
    public const string IDLE = "Idle";
    public const string MOVE = "Move";
    public const string HIT = "Hit";
}