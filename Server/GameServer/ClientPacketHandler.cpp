#include "pch.h"
#include "ClientPacketHandler.h"
#include "GameSession.h"
#include "PlayerManager.h"
#include "RoomManager.h"
#include "CreatureManager.h"
#include "Creature.h"
#include "CreatureStateUtil.h"

PacketHandlerFunc GPacketHandler[UINT16_MAX];

bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len)
{
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
	// TODO : log
	return false;
}


bool Handle_C_S_LOGIN(PacketSessionRef& session, Protocol::C_S_LOGIN& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	Protocol::S_C_LOGIN sendPkt;
	PlayerRef player;
	sendPkt.set_success(GPlayerManager.Login(gameSession, pkt.playerid(), player));

	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(sendPkt);
	session->Send(sendBuffer);
	return true;
}

bool Handle_C_S_ENTER_GAME(PacketSessionRef& session, Protocol::C_S_ENTER_GAME& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = GPlayerManager.FindBySession(gameSession);

	Protocol::S_C_ENTER_GAME enterPkt;
	if (GRoomManager.EnterGame(player, enterPkt) == false)
	{
		enterPkt.set_success(false);
		enterPkt.set_myobjectid(0);
	}

	SendBufferRef enterBuffer = ClientPacketHandler::MakeSendBuffer(enterPkt);
	session->Send(enterBuffer);
	return true;
}

bool Handle_C_S_LEAVE_GAME(PacketSessionRef& session, Protocol::C_S_LEAVE_GAME& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = GPlayerManager.FindBySession(gameSession);

	Protocol::S_C_LEAVE_GAME leavePkt;

	if (player == nullptr)
	{
		leavePkt.set_success(false);
	}
	else
	{
		GRoomManager.LeaveGame(player);
		leavePkt.set_success(true);
	}

	SendBufferRef leaveBuffer = ClientPacketHandler::MakeSendBuffer(leavePkt);
	session->Send(leaveBuffer);
	return true;
}

bool Handle_C_S_MOVE(PacketSessionRef& session, Protocol::C_S_MOVE& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = GPlayerManager.FindBySession(gameSession);
	if (player == nullptr)
		return false;

	if (player->GetState() != EPlayerState::InGame)
		return false;

	RoomRef room = GRoomManager.GetRoom(player->GetRoomId());
	if (room == nullptr)
		return false;

	const float oldX = player->GetStat().GetPosX();
	const float oldY = player->GetStat().GetPosY();

	float validatedX = 0.f;
	float validatedY = 0.f;
	room->ValidateClientPosition(player, pkt.posx(), pkt.posy(), validatedX, validatedY);

	player->GetStat().SetPosition(validatedX, validatedY);
	room->OnPlayerMoved(player, oldX, oldY, validatedX, validatedY);

	if (pkt.dirx() != 0.f || pkt.diry() != 0.f)
		player->SetMoveDirection(pkt.dirx(), pkt.diry());

	const int32 stateFlags = CreatureStateUtil::SanitizeStateFlags(
		pkt.stateflags(), player->GetMoveDirX(), player->GetMoveDirY());
	player->SetStateFlags(stateFlags);

	Protocol::S_C_MOVE movePkt;
	movePkt.set_objectid(player->GetObjectId());
	movePkt.set_posx(validatedX);
	movePkt.set_posy(validatedY);
	movePkt.set_stateflags(stateFlags);
	movePkt.set_dirx(player->GetMoveDirX());
	movePkt.set_diry(player->GetMoveDirY());

	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(movePkt);

	// 본인에게는 매 이동마다 서버 권한 위치를 전달한다.
	gameSession->Send(sendBuffer);
	room->BroadcastToView(validatedX, validatedY, sendBuffer, gameSession);
	return true;
}

bool Handle_C_S_ATTACK(PacketSessionRef& session, Protocol::C_S_ATTACK& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef attacker = GPlayerManager.FindBySession(gameSession);
	if (attacker == nullptr)
		return false;

	if (attacker->GetState() != EPlayerState::InGame)
		return false;

	RoomRef room = GRoomManager.GetRoom(attacker->GetRoomId());
	if (room == nullptr)
		return false;

	const uint64 now = ::GetTickCount64();
	if (now - attacker->GetLastAttackTick() < attacker->GetAttackCooldownMs())
		return true;

	attacker->SetLastAttackTick(now);
	attacker->SetMoveDirection(pkt.dirx(), pkt.diry());

	const float attackRange = attacker->GetAttackRange();
	const int32 attackDamage = attacker->GetAttackDamage();

	const float ax = attacker->GetStat().GetPosX();
	const float ay = attacker->GetStat().GetPosY();

	MonsterRef hitMonster = nullptr;
	float targetDirX = 0.f;
	float targetDirY = 0.f;
	int32 damageDealt = 0;

	room->ForEachMonsterInView(ax, ay, [&](MonsterRef monster)
	{
		if (hitMonster != nullptr)
			return;

		if (monster->IsAlive() == false)
			return;

		const float mx = monster->GetStat().GetPosX();
		const float my = monster->GetStat().GetPosY();
		const float dx = mx - ax;
		const float dy = my - ay;
		if (dx * dx + dy * dy > attackRange * attackRange)
			return;

		damageDealt = monster->TakeDamage(attackDamage);
		if (damageDealt <= 0)
			return;

		hitMonster = monster;
		monster->FaceToward(ax, ay);
		targetDirX = monster->GetMoveDirX();
		targetDirY = monster->GetMoveDirY();
	});

	const int32 hitTargetId = hitMonster != nullptr ? hitMonster->GetObjectId() : 0;

	Protocol::S_C_ATTACK attackPkt;
	attackPkt.set_attackerid(attacker->GetObjectId());
	attackPkt.set_dirx(pkt.dirx());
	attackPkt.set_diry(pkt.diry());
	attackPkt.set_targetid(hitTargetId);
	if (hitMonster != nullptr)
	{
		attackPkt.set_targethp(hitMonster->GetStat().GetHp());
		attackPkt.set_targetdirx(targetDirX);
		attackPkt.set_targetdiry(targetDirY);
	}

	room->BroadcastToView(ax, ay, ClientPacketHandler::MakeSendBuffer(attackPkt));

	if (hitMonster != nullptr && hitMonster->IsDead())
	{
		CreatureRef deadCreature = static_pointer_cast<Creature>(hitMonster);
		GRoomManager.HandleCreatureDeath(room, deadCreature);
		room->UnregisterCreature(deadCreature);
		GCreatureManager.Remove(deadCreature->GetObjectId());
	}

	return true;
}

bool Handle_C_S_CHAT(PacketSessionRef& session, Protocol::C_S_CHAT& pkt)
{

	return true;
}
