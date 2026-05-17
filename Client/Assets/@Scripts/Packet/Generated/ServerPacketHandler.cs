using Google.Protobuf;
using ServerCore;
using System;
using System.Collections.Generic;
using Protocol;

public enum MsgId
{
    C_S_LOGIN = 1010,
    S_C_LOGIN = 1011,
    C_S_ENTER_GAME = 1012,
    S_C_ENTER_GAME = 1013,
    C_S_LEAVE_GAME = 1014,
    S_C_LEAVE_GAME = 1015,
    S_C_SPAWN = 1016,
    S_C_DESPAWN = 1017,
    C_S_MOVE = 1018,
    S_C_MOVE = 1019,
    C_S_ATTACK = 1020,
    S_C_ATTACK = 1021,
    S_C_DIE = 1022,
    C_S_CHAT = 1023,
    S_C_CHAT = 1024,
};

class ServerPacketHandler
{
    
    #region Singleton
    
    static ServerPacketHandler
    _instance = new ServerPacketHandler();

    public static ServerPacketHandler Instance { get { return _instance; } }
    #endregion

    ServerPacketHandler()
    {
        Register();
    }

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
    
    public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
    
    public void Register()
    {
        _onRecv.Add((ushort)MsgId.S_C_LOGIN, MakePacket<S_C_LOGIN>);
        _handler.Add((ushort)MsgId.S_C_LOGIN, PacketHandler.S_C_LOGINHandler);
        _onRecv.Add((ushort)MsgId.S_C_ENTER_GAME, MakePacket<S_C_ENTER_GAME>);
        _handler.Add((ushort)MsgId.S_C_ENTER_GAME, PacketHandler.S_C_ENTER_GAMEHandler);
        _onRecv.Add((ushort)MsgId.S_C_LEAVE_GAME, MakePacket<S_C_LEAVE_GAME>);
        _handler.Add((ushort)MsgId.S_C_LEAVE_GAME, PacketHandler.S_C_LEAVE_GAMEHandler);
        _onRecv.Add((ushort)MsgId.S_C_SPAWN, MakePacket<S_C_SPAWN>);
        _handler.Add((ushort)MsgId.S_C_SPAWN, PacketHandler.S_C_SPAWNHandler);
        _onRecv.Add((ushort)MsgId.S_C_DESPAWN, MakePacket<S_C_DESPAWN>);
        _handler.Add((ushort)MsgId.S_C_DESPAWN, PacketHandler.S_C_DESPAWNHandler);
        _onRecv.Add((ushort)MsgId.S_C_MOVE, MakePacket<S_C_MOVE>);
        _handler.Add((ushort)MsgId.S_C_MOVE, PacketHandler.S_C_MOVEHandler);
        _onRecv.Add((ushort)MsgId.S_C_ATTACK, MakePacket<S_C_ATTACK>);
        _handler.Add((ushort)MsgId.S_C_ATTACK, PacketHandler.S_C_ATTACKHandler);
        _onRecv.Add((ushort)MsgId.S_C_DIE, MakePacket<S_C_DIE>);
        _handler.Add((ushort)MsgId.S_C_DIE, PacketHandler.S_C_DIEHandler);
        _onRecv.Add((ushort)MsgId.S_C_CHAT, MakePacket<S_C_CHAT>);
        _handler.Add((ushort)MsgId.S_C_CHAT, PacketHandler.S_C_CHATHandler);
    }
    
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;
    
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
    
        Action<PacketSession, ArraySegment<byte>, ushort> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer, id);
    }
    
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
    {
        T pkt = new T();
        pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
    
        if (CustomHandler != null)
        {
            CustomHandler.Invoke(session, pkt, id);
        }
        else
        {
            Action<PacketSession, IMessage> action = null;
            if (_handler.TryGetValue(id, out action))
                action.Invoke(session, pkt);
        }
    }
    
    public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
    {
        Action<PacketSession, IMessage> action = null;
        if (_handler.TryGetValue(id, out action))
            return action;
        return null;
    }
}