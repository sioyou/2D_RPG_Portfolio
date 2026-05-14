#include "pch.h"
#include "Allocator.h"
#include "Memory.h"

/*------------------
	BaseAllocator
-------------------*/
void* BaseAllocator::Alloc(int32 size)
{
	return ::malloc(size);
}

void BaseAllocator::Release(void* ptr)
{
	::free(ptr);
}

/*------------------
	StompAllocator
-------------------*/
//[						[]]	-> 앞쪽에 하면 오버플로우가 일어나는걸 못잡기에 주소를 맨 뒤에 잡음
//							-> 이러면 언더플로우는 못잡지만 잘 안일어남
void* StompAllocator::Alloc(int32 size)
{
	// 4 + 4095 = 4099, -1를 하는 이유 4096 + 4096 하면 count 2개를 할당해서 -1를 하고 나눠줌
	const int64 pageCount = (size + PAGE_SIZE - 1) / PAGE_SIZE;
	const int64 dataOffset = pageCount * PAGE_SIZE - size;
	void* baseAddress = ::VirtualAlloc(NULL, pageCount * PAGE_SIZE, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	return static_cast<void*>(static_cast<int8*>(baseAddress) + dataOffset);
}

void StompAllocator::Release(void* ptr)
{
	const int64 address = reinterpret_cast<int64>(ptr);
	const int64 baseAddress = address - (address % PAGE_SIZE);
	::VirtualFree(reinterpret_cast<void*>(baseAddress), 0, MEM_RELEASE);
}

/*------------------
	PoolAllocator
-------------------*/
void* PoolAllocator::Alloc(int32 size)
{
	return GMemory->Allocate(size);
}

void PoolAllocator::Release(void* ptr)
{
	GMemory->Release(ptr);
}
