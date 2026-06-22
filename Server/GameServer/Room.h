#pragma once
#include "Zone.h"
#include "Monster.h"
#include "Data/DataTypes.h"
#include "Protocol.pb.h"

/* Room: game instance. Zones inside a room handle spatial AOI. */
class Room : public enable_shared_from_this<Room>
{
public:
	Room(int32 roomId, const RoomData& config);

	int32 GetRoomId() const { return _roomId; }
	float GetZoneSize() const { return _zoneSize; }
	float GetViewRadius() const { return _viewRadius; }
	int32 GetZoneCountX() const { return _zoneCountX; }
	int32 GetZoneCountY() const { return _zoneCountY; }

	int32 WorldToZoneIndex(float worldX, float worldY) const;
	void ZoneIndexToCoord(int32 zoneIndex, int32& outX, int32& outY) const;
	bool IsValidZoneIndex(int32 zoneIndex) const;

	void CollectVisibleZoneIndices(float worldX, float worldY, Vector<int32>& out) const;
	bool CanSee(float viewerX, float viewerY, float targetX, float targetY) const;

	bool HasPlayer(PlayerRef player);
	bool EnterPlayer(PlayerRef player);
	void LeavePlayer(PlayerRef player);

	void OnPlayerMoved(PlayerRef player, float oldX, float oldY, float newX, float newY);

	void FillEnterGameSpawns(Protocol::S_C_ENTER_GAME& pkt, PlayerRef player);
	void BroadcastToView(float worldX, float worldY, SendBufferRef sendBuffer, GameSessionRef excludeSession = nullptr);

	void ValidateClientPosition(PlayerRef player, float clientX, float clientY, float& outX, float& outY);
	PlayerRef FindPlayer(int32 objectId);

	void RegisterCreature(CreatureRef creature);
	void UnregisterCreature(CreatureRef creature);

	template<typename Func>
	void ForEachMonsterInView(float worldX, float worldY, Func&& func)
	{
		Vector<int32> visible;
		CollectVisibleZoneIndices(worldX, worldY, visible);

		for (int32 idx : visible)
		{
			ZoneRef zone = GetZone(idx);
			if (zone == nullptr)
				continue;

			zone->ForEachCreature([&](CreatureRef creature)
				{
					if (creature->GetObjectType() != Protocol::OBJECT_TYPE_MONSTER)
						return;

					const float cx = creature->GetStat().GetPosX();
					const float cy = creature->GetStat().GetPosY();
					if (CanSee(worldX, worldY, cx, cy) == false)
						return;

					func(static_pointer_cast<Monster>(creature));
				});
		}
	}

private:
	ZoneRef GetZone(int32 zoneIndex) const;
	void CollectSessionsWhoCanSee(float targetX, float targetY, Vector<GameSessionRef>& out, GameSessionRef excludeSession);

	void EnterZone(PlayerRef player, int32 zoneIndex);
	void LeaveZone(PlayerRef player, int32 zoneIndex);
	void SyncPlayerVisibility(PlayerRef player, float oldX, float oldY, float newX, float newY);

	void SendSpawnToSession(CreatureRef creature, GameSessionRef session);
	void SendDespawnToSession(int32 objectId, GameSessionRef session);
	void SendToSessions(const Vector<GameSessionRef>& sessions, SendBufferRef sendBuffer);

	void AppendOtherCreaturesInZones(Protocol::S_C_ENTER_GAME& pkt, PlayerRef player, const Vector<int32>& zoneIndices) const;
	void SyncOtherPlayersViewOfMover(PlayerRef player, float oldX, float oldY, float newX, float newY);
	void SyncMoverViewOfEntities(PlayerRef player, float oldX, float oldY, float newX, float newY);

	int32 _roomId = 0;
	float _originX = 0.f;
	float _originY = 0.f;
	float _zoneSize = 10.f;
	float _viewRadius = 4.5f;
	float _spawnPosX = 0.f;
	float _spawnPosY = 0.f;
	int32 _zoneCountX = 1;
	int32 _zoneCountY = 1;

	Vector<ZoneRef> _zones;

	USE_LOCK;
	HashMap<int32, PlayerRef> _players;
};

using RoomRef = shared_ptr<Room>;
