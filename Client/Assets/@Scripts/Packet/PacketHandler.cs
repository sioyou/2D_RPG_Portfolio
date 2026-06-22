using Google.Protobuf;
using Protocol;
using ServerCore;
using UnityEngine;

public static class PacketHandler
{
    public static void S_C_LOGINHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_LOGIN;
        if (pkt == null)
        {
            Managers.Network.HandleLoginResponse(false);
            return;
        }

        Debug.Log($"S_C_LOGIN success={pkt.Success}");
        Managers.Network.HandleLoginResponse(pkt.Success);
    }

    public static void S_C_ENTER_GAMEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_ENTER_GAME;
        if (pkt == null)
            return;

        Debug.Log($"S_C_ENTER_GAME success={pkt.Success} myObjectId={pkt.MyObjectId} roomId={pkt.RoomId} mapId={pkt.MapId} spawns={pkt.Spawns.Count}");
        Managers.Network.HandleEnterGameResponse(pkt.Success, pkt.MyObjectId, pkt.RoomId, pkt.MapId, pkt.Spawns);
    }

    public static void S_C_LEAVE_GAMEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_LEAVE_GAME;
        if (pkt == null)
            return;

        Debug.Log($"S_C_LEAVE_GAME success={pkt.Success}");
        Managers.Network.HandleLeaveGameResponse(pkt.Success);
    }

    public static void S_C_SPAWNHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_SPAWN;
        if (pkt?.Spawn == null)
            return;

        Managers.Object.Spawn(pkt.Spawn);
    }

    public static void S_C_DESPAWNHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_DESPAWN;
        if (pkt == null)
            return;

        Debug.Log($"S_C_DESPAWN objectId={pkt.ObjectId}");
        Managers.Object.Despawn(pkt.ObjectId);
    }

    public static void S_C_MOVEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_MOVE;
        if (pkt == null)
            return;

        CreatureObject creature= Managers.Object.Find<CreatureObject>(pkt.ObjectId);
        if (creature == null)
            return;

        creature.ApplyDestPosition(new Vector2(pkt.PosX, pkt.PosY));
        creature.ApplyStateFlags(pkt.StateFlags);
        creature.ApplyMoveDirection(pkt.DirX, pkt.DirY);
    }

    public static void S_C_ATTACKHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_ATTACK;
        if (pkt == null)
            return;

        CreatureObject attacker = Managers.Object.Find<CreatureObject>(pkt.AttackerId);
        attacker?.ApplyAttack(pkt.DirX, pkt.DirY);

        if (pkt.TargetId == 0)
            return;

        CreatureObject target = Managers.Object.Find<CreatureObject>(pkt.TargetId);
        if (target == null)
            return;

        float faceDirX = pkt.TargetDirX;
        if (Mathf.Abs(faceDirX) <= 0.0001f && Mathf.Abs(pkt.TargetDirY) <= 0.0001f)
        {
            faceDirX = -pkt.DirX;
            if (attacker != null)
                faceDirX = attacker.transform.position.x - target.transform.position.x;
        }
        
        target.ApplyDamaged(faceDirX, pkt.TargetHp);
    }

    public static void S_C_DIEHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_DIE;
        if (pkt == null)
            return;

        CreatureObject creature = Managers.Object.Find<CreatureObject>(pkt.ObjectId);
        creature?.ApplyDie();
    }

    public static void S_C_CHATHandler(PacketSession session, IMessage packet)
    {
        var pkt = packet as S_C_CHAT;
        Debug.Log($"S_C_CHAT objectId={pkt?.ObjectId} msg={pkt?.Msg}");
    }
}
