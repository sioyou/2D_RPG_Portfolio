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

struct ZoneSpawnData
{
	int32 zoneId = 0;
	std::vector<SpawnEntryData> spawns;
};

inline constexpr int32 DEFAULT_PLAYER_DATA_ID = 1;

NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MonsterData, id, name, maxHp, moveSpeed, attackDamage, attackRange)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(PlayerData, id, name, level, maxHp, moveSpeed, attackDamage, attackRange, attackCooldownMs)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SpawnEntryData, monsterType, level, posX, posY)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(ZoneSpawnData, zoneId, spawns)
