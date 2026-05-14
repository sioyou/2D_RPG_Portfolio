#pragma once

class Session;

enum class EventType : uint8
{
	Connect,
	Disconnect,
	Accept,
	//PreRecv, 0 byte Recv
	Recv,
	Send,
};

/*----------------------
		IocpEvent
 ----------------------*/
// 이렇게 부모자식으로 만들면 virtual 함수를 만들면 안됨
// 지금은 OVERLAPPED 관련 변수가 메모리 제일 앞에 있지만
// virtual을 붙이면 가상 함수테이블이 offset 0번으로 들어가면서 맨 앞이 OVERLAPPED관련된게 아님
class IocpEvent : public OVERLAPPED
{
public:
	IocpEvent(EventType type);

	void Init();

public:
	EventType		eventType;
	IocpObjectRef	owner;
};

/*----------------------
		ConnectEvent
----------------------*/
class ConnectEvent : public IocpEvent
{
public:
	ConnectEvent():IocpEvent(EventType::Connect){}
};


/*----------------------
	  DisconnectEvent
----------------------*/
class DisconnectEvent : public IocpEvent
{
public:
	DisconnectEvent() :IocpEvent(EventType::Disconnect) {}
};

/*----------------------
		AcceptEvent
----------------------*/
class AcceptEvent : public IocpEvent
{
public:
	AcceptEvent() :IocpEvent(EventType::Accept) {}

public:
	SessionRef	session = nullptr;
};

/*----------------------
		RecvEvent
----------------------*/
class RecvEvent : public IocpEvent
{
public:
	RecvEvent() :IocpEvent(EventType::Recv) {}
};

/*----------------------
		SendEvent
----------------------*/
class SendEvent : public IocpEvent
{
public:
	SendEvent() :IocpEvent(EventType::Send) {}

	Vector<SendBufferRef> sendBuffers;
};