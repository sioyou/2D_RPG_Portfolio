#include "pch.h"
#include "DataManager.h"

#include <filesystem>
#include <fstream>
#include <nlohmann/json.hpp>

DataManager GDataManager;

namespace
{
	const std::vector<SpawnEntryData> EMPTY_SPAWNS;
	namespace fs = std::filesystem;

	bool LoadJsonArrayFromFile(const std::string& filePath, const char* label, nlohmann::json& outRoot)
	{
		std::ifstream file(filePath);
		if (file.is_open() == false)
		{
			cout << "[DataManager] Failed to open " << filePath << endl;
			return false;
		}

		try
		{
			file >> outRoot;
		}
		catch (const std::exception& e)
		{
			cout << "[DataManager] " << label << " parse error: " << e.what() << endl;
			return false;
		}

		if (outRoot.is_array() == false)
		{
			cout << "[DataManager] " << label << " must be an array." << endl;
			return false;
		}

		return true;
	}
}

std::string DataManager::ResolveDataRoot(const std::string& dataRootName)
{
	std::vector<fs::path> candidates;
	candidates.push_back(fs::path(dataRootName));

	char exePath[MAX_PATH] = {};
	if (::GetModuleFileNameA(nullptr, exePath, MAX_PATH) > 0)
	{
		const fs::path exeDir = fs::path(exePath).parent_path();
		candidates.push_back(exeDir / dataRootName);
		candidates.push_back(exeDir / ".." / ".." / ".." / dataRootName);
	}

	for (const fs::path& candidate : candidates)
	{
		const fs::path monstersPath = candidate / "Monsters.json";
		std::error_code ec;
		if (fs::exists(monstersPath, ec) && ec.value() == 0)
			return candidate.lexically_normal().string();
	}

	return "";
}

bool DataManager::Init(const std::string& dataRootName)
{
	const std::string dataRootPath = ResolveDataRoot(dataRootName);
	if (dataRootPath.empty())
	{
		cout << "[DataManager] Data root not found. name=" << dataRootName << endl;
		return false;
	}

	_dataRootPath = dataRootPath;

	if (LoadFromDirectory(_dataRootPath) == false)
		return false;

	cout << "[DataManager] Init complete. root=" << _dataRootPath
		<< " monsters=" << _monsters.size()
		<< " players=" << _players.size()
		<< " rooms=" << _rooms.size()
		<< " roomSpawns=" << _roomSpawns.size() << endl;
	return true;
}

bool DataManager::LoadFromDirectory(const std::string& dataRootPath)
{
	const fs::path rootPath(dataRootPath);

	if (LoadMonsters((rootPath / "Monsters.json").string()) == false)
		return false;

	if (LoadPlayers((rootPath / "Players.json").string()) == false)
		return false;

	if (LoadRooms((rootPath / "Rooms.json").string()) == false)
		return false;

	if (LoadRoomSpawns((rootPath / "RoomSpawns.json").string()) == false)
		return false;

	return true;
}

bool DataManager::LoadMonsters(const std::string& filePath)
{
	nlohmann::json root;
	if (LoadJsonArrayFromFile(filePath, "Monsters.json", root) == false)
		return false;

	try
	{
		_monsters.clear();
		for (const nlohmann::json& entry : root)
		{
			MonsterData data = entry.get<MonsterData>();

			if (_monsters.find(data.id) != _monsters.end())
			{
				cout << "[DataManager] Duplicate monster id=" << data.id << endl;
				return false;
			}

			_monsters[data.id] = std::move(data);
		}
	}
	catch (const std::exception& e)
	{
		cout << "[DataManager] Monsters.json parse error: " << e.what() << endl;
		return false;
	}

	return true;
}

bool DataManager::LoadPlayers(const std::string& filePath)
{
	nlohmann::json root;
	if (LoadJsonArrayFromFile(filePath, "Players.json", root) == false)
		return false;

	try
	{
		_players.clear();
		for (const nlohmann::json& entry : root)
		{
			PlayerData data = entry.get<PlayerData>();

			if (_players.find(data.id) != _players.end())
			{
				cout << "[DataManager] Duplicate player id=" << data.id << endl;
				return false;
			}

			_players[data.id] = std::move(data);
		}
	}
	catch (const std::exception& e)
	{
		cout << "[DataManager] Players.json parse error: " << e.what() << endl;
		return false;
	}

	return true;
}

bool DataManager::LoadRooms(const std::string& filePath)
{
	nlohmann::json root;
	if (LoadJsonArrayFromFile(filePath, "Rooms.json", root) == false)
		return false;

	try
	{
		_rooms.clear();
		for (const nlohmann::json& entry : root)
		{
			RoomData data = entry.get<RoomData>();

			if (_rooms.find(data.roomId) != _rooms.end())
			{
				cout << "[DataManager] Duplicate room id=" << data.roomId << endl;
				return false;
			}

			_rooms[data.roomId] = std::move(data);
		}
	}
	catch (const std::exception& e)
	{
		cout << "[DataManager] Rooms.json parse error: " << e.what() << endl;
		return false;
	}

	return true;
}

bool DataManager::LoadRoomSpawns(const std::string& filePath)
{
	nlohmann::json root;
	if (LoadJsonArrayFromFile(filePath, "RoomSpawns.json", root) == false)
		return false;

	try
	{
		_roomSpawns.clear();
		for (const nlohmann::json& roomEntry : root)
		{
			RoomSpawnData roomData = roomEntry.get<RoomSpawnData>();
			_roomSpawns[roomData.roomId] = std::move(roomData.spawns);
		}
	}
	catch (const std::exception& e)
	{
		cout << "[DataManager] RoomSpawns.json parse error: " << e.what() << endl;
		return false;
	}

	return true;
}

const MonsterData* DataManager::GetMonster(int32 monsterTypeId) const
{
	auto it = _monsters.find(monsterTypeId);
	if (it == _monsters.end())
		return nullptr;

	return &it->second;
}

const MonsterData* DataManager::GetMonster(Protocol::MonsterType monsterType) const
{
	return GetMonster(static_cast<int32>(monsterType));
}

const PlayerData* DataManager::GetPlayer(int32 playerDataId) const
{
	auto it = _players.find(playerDataId);
	if (it == _players.end())
		return nullptr;

	return &it->second;
}

const PlayerData* DataManager::GetDefaultPlayer() const
{
	return GetPlayer(DEFAULT_PLAYER_DATA_ID);
}

const RoomData* DataManager::GetRoom(int32 roomId) const
{
	auto it = _rooms.find(roomId);
	if (it == _rooms.end())
		return nullptr;

	return &it->second;
}

const std::vector<SpawnEntryData>& DataManager::GetRoomSpawns(int32 roomId) const
{
	auto it = _roomSpawns.find(roomId);
	if (it == _roomSpawns.end())
		return EMPTY_SPAWNS;

	return it->second;
}
