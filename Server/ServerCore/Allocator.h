#pragma once
/*------------------
	BaseAllocator
-------------------*/
class BaseAllocator
{
public:
	static void* Alloc(int32 size);
	static void Release(void* ptr);
};

/*------------------
	StompAllocator
-------------------*/
class StompAllocator
{
	enum { PAGE_SIZE = 0x1000 };	//VirtualAlloc()를 해도 dwSize가 아니라 페이지 사이즈만큼 메모리를 할당해줌
public:
	static void* Alloc(int32 size);
	static void Release(void* ptr);
};

/*------------------
	PoolAllocator
-------------------*/
class PoolAllocator
{
	enum { PAGE_SIZE = 0x1000 };	//VirtualAlloc()를 해도 dwSize가 아니라 페이지 사이즈만큼 메모리를 할당해줌
public:
	static void* Alloc(int32 size);
	static void Release(void* ptr);
};

/*------------------
	STLAllocator
-------------------*/
template<typename T>
class StlAllocator
{
public:
	using value_type = T;

	StlAllocator(){}

	template<typename Other>
	StlAllocator(const StlAllocator<Other>&){}

	T* allocate(size_t count)
	{
		const int32 size = static_cast<int32>(count * sizeof(T));
		return static_cast<T*>(PoolAllocator::Alloc(size));
	}

	void deallocate(T* ptr, size_t count)
	{
		PoolAllocator::Release(ptr);
	}

};