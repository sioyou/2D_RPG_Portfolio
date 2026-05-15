using Google.Protobuf;
using Protocol;
using ServerCore;
using UnityEngine;

public static class PacketHandler
{
    public static void S_C_LOGINHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_LOGIN;
        Debug.Log($"S_C_LOGIN success={pkt?.Success}");
    }

    public static void S_C_ENTER_GAMEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_ENTER_GAME;
        Debug.Log($"S_C_ENTER_GAME success={pkt?.Success}");
    }

    public static void S_C_LEAVE_GAMEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_LEAVE_GAME;
        Debug.Log($"S_C_LEAVE_GAME playerId={pkt?.PlayerId}");
    }

    public static void S_C_SPAWNHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_C_SPAWN");
    }

    public static void S_C_MOVEHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_C_MOVE");
    }

    public static void S_C_ATTACKHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_C_ATTACK");
    }

    public static void S_C_DIEHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_C_DIE");
    }

    public static void S_C_CHATHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_CHAT;
        Debug.Log($"S_C_CHAT objectId={pkt?.ObjectId} msg={pkt?.Msg}");
    }
}
