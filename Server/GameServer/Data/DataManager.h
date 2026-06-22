#pragma once

#include "DataTypes.h"
#include <unordered_map>

class DataManager
{
public:
	bool Init(const std::string& dataRootName = "Data");

	const MonsterData* GetMonster(int32 monsterTypeId) const;
	const MonsterData* GetMonster(Protocol::MonsterType monsterType) const;
	const PlayerData* GetPlayer(int32 playerDataId) const;
	const PlayerData* GetDefaultPlayer() const;
	const RoomData* GetRoom(int32 roomId) const;
	const std::vector<SpawnEntryData>& GetRoomSpawns(int32 roomId) const;

private:
	bool LoadFromDirectory(const std::string& dataRootPath);
	bool LoadMonsters(const std::string& filePath);
	bool LoadPlayers(const std::string& filePath);
	bool LoadRooms(const std::string& filePath);
	bool LoadRoomSpawns(const std::string& filePath);

	static std::string ResolveDataRoot(const std::string& dataRootName);

	std::string _dataRootPath;
	std::unordered_map<int32, MonsterData> _monsters;
	std::unordered_map<int32, PlayerData> _players;
	std::unordered_map<int32, RoomData> _rooms;
	std::unordered_map<int32, std::vector<SpawnEntryData>> _roomSpawns;
};

extern DataManager GDataManager;
