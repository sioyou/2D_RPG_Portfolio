#pragma once
#include "CreatureStat.h"

enum class EPlayerState : uint8
{
	Offline = 0,
	Lobby = 1,
	InGame = 2,
};

class Player : public enable_shared_from_this<Player>
{
public:
	~Player();
	const string& GetPlayerId() const { return _playerId; }
	int32 GetObjectId() const { return _objectId; }
	int32 GetZoneId() const { return _zoneId; }
	EPlayerState GetState() const { return _state; }

	const CreatureStat& GetStat() const { return _stat; }
	CreatureStat& GetStat() { return _stat; }

	void Init(const string& playerId, int32 objectId);
	void SetZoneId(int32 zoneId) { _zoneId = zoneId; }
	void SetState(EPlayerState state) { _state = state; }

	void SetSession(GameSessionRef session);
	GameSessionRef GetSession();
	bool IsOnline() const;

	uint64 GetLoginTick() const { return _loginTick; }
	void SetLoginTick(uint64 tick) { _loginTick = tick; }

private:
	string _playerId;
	int32 _objectId = 0;
	int32 _zoneId = 0;
	EPlayerState _state = EPlayerState::Lobby;

	CreatureStat _stat;

	uint64 _loginTick = 0;
	weak_ptr<class GameSession> _session;
};
