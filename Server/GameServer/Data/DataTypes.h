#pragma once

#include <nlohmann/json.hpp>
#include <string>
#include <vector>

struct MonsterData
{
	int32 id = 0;
	std::string name;
	int32 maxHp = 0;
	float moveSpeed = 0.f;
	int32 attackDamage = 0;
	float attackRange = 0.f;
};

struct PlayerData
{
	int32 id = 0;
	std::string name;
	int32 level = 1;
	int32 maxHp = 100;
	float moveSpeed = 5.f;
	int32 attackDamage = 10;
	float attackRange = 1.5f;
	uint64 attackCooldownMs = 500;
};

struct SpawnEntryData
{
	int32 monsterType = 0;
	int32 level = 1;
	float posX = 0.f;
	float posY = 0.f;
};

struct MapCollisionData
{
	int32 mapId = 0;
	float originX = -10.f;
	float originY = -10.f;
	float tileSize = 1.f;
	int32 width = 0;
	int32 height = 0;
	std::string cells;
};

struct RoomData
{
	int32 roomId = 0;
	int32 mapId = 1;
	float originX = -10.f;
	float originY = -10.f;
	float zoneSize = 10.f;
	float viewRadius = 4.5f;
	float spawnPosX = -5.f;
	float spawnPosY = -5.f;
	int32 zoneCountX = 4;
	int32 zoneCountY = 4;
};

struct RoomSpawnData
{
	int32 roomId = 0;
	std::vector<SpawnEntryData> spawns;
};

inline constexpr int32 DEFAULT_PLAYER_DATA_ID = 1;

NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MonsterData, id, name, maxHp, moveSpeed, attackDamage, attackRange)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(PlayerData, id, name, level, maxHp, moveSpeed, attackDamage, attackRange, attackCooldownMs)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SpawnEntryData, monsterType, level, posX, posY)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MapCollisionData, mapId, originX, originY, tileSize, width, height, cells)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RoomData, roomId, mapId, originX, originY, zoneSize, viewRadius, spawnPosX, spawnPosY, zoneCountX, zoneCountY)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RoomSpawnData, roomId, spawns)
