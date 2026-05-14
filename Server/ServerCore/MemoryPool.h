#pragma once

enum
{
	SLIST_ALIGNMENT = 16
};

/*-----------------------
		MemoryHeader
 -----------------------*/
// 상속을 받으면 맴버 변수 제일 앞에 부모의 맴버변수가 들어감
DECLSPEC_ALIGN(SLIST_ALIGNMENT)
struct MemoryHeader : public SLIST_ENTRY
{
	//[MemoryHeader][Data]

	MemoryHeader(int32 size) : allocSize(size){}

	static void* AttachHeader(MemoryHeader* header, int32 size)
	{
		new(header)MemoryHeader(size); //placement new
		return reinterpret_cast<void*>(++header);	// c++ 특성상 memoryHeader만큼을 건너뛴 값을 리턴함->데이터주소 리턴
	}

	static MemoryHeader* DetachHeader(void* ptr)
	{
		MemoryHeader* header = reinterpret_cast<MemoryHeader*>(ptr) - 1;
		return header;
	}

	int32 allocSize;
	// TODO : 필요한 추가 정보
};

/*-----------------------
		MemoryPool
 -----------------------*/

DECLSPEC_ALIGN(SLIST_ALIGNMENT)
class MemoryPool
{
public:
	MemoryPool(int32 allocSize);
	~MemoryPool();

	void Push(MemoryHeader* ptr);
	MemoryHeader* Pop();

private:
	SLIST_HEADER _header;
	int32 _allocSize = 0;
	atomic<int32> _useCount = 0;
	atomic<int32> _reserveCount = 0;

};

