#include "pch.h"

#include "ThreadManager.h"
#include "Service.h"
#include "GameSession.h"
#include "ClientPacketHandler.h"
#include "ZoneManager.h"

enum
{
	WORKER_TICK = 64,
};

void DoWorkerJob(ServerServiceRef& service)
{
	while (true)
	{
		LEndTickCount = ::GetTickCount64() + WORKER_TICK;

		service->GetIocpCore()->Dispatch(10);

		ThreadManager::DistributeReservedJobs();
		ThreadManager::DoGlobalQueueWork();
	}
}

int main()
{
	ClientPacketHandler::Init();
	GZoneManager.Init();

	ServerServiceRef service = MakeShared<ServerService>(
		NetAddress(L"0.0.0.0", 7777),
		MakeShared<IocpCore>(),
		MakeShared<GameSession>,
		100);

	ASSERT_CRASH(service->Start());

	for (int32 i = 0; i < 1; i++)
	{
		GThreadManager->Launch([&service]()
			{
				DoWorkerJob(service);
			});
	}

	// Main Thread
	DoWorkerJob(service);

	GThreadManager->Join();
}
