#pragma once
#include "Player.h"

/*----------------------
	PlayerManager
	- 로그인한 유저 데이터 (인메모리)
	- playerId / objectId / session 으로 조회
-----------------------*/
class PlayerManager
{
public:
	bool Login(GameSessionRef session, const string& playerId, PlayerRef& outPlayer);
	void Logout(GameSessionRef session);
	void LogoutByPlayerId(const string& playerId);

	PlayerRef FindByPlayerId(const string& playerId);
	PlayerRef FindByObjectId(int32 objectId);
	PlayerRef FindBySession(GameSessionRef session);

	bool IsOnline(const string& playerId);
	int32 GetOnlineCount();

private:
	bool Register(PlayerRef player);
	void Unregister(PlayerRef player);

private:
	USE_LOCK;

	HashMap<string, PlayerRef> _playersById;
	HashMap<int32, PlayerRef> _playersByObjectId;
	HashMap<GameSession*, PlayerRef> _playersBySession;
};

extern PlayerManager GPlayerManager;
