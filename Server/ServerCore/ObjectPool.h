#pragma once
#include "Types.h"
#include "MemoryPool.h"

template<typename Type>
class ObjectPool
{
public:
	// 생성자가 필요하면 넣어주기
	template<typename... Args>
	static Type* Pop(Args&&... args)
	{
#if defined _STOMP
		MemoryHeader* ptr = reinterpret_cast<MemoryHeader*>(StompAllocator::Alloc(s_allocSize));
		Type* memory = static_cast<Type*>(MemoryHeader::AttachHeader(ptr, s_allocSize));
#else
		Type* memory = static_cast<Type*>(MemoryHeader::AttachHeader(s_pool.Pop(), s_allocSize));
#endif
		new(memory)Type(forward<Args>(args)...);	// placement new
		return memory;
	}

	static void Push(Type* obj)
	{
		obj->~Type();
#if defined _STOMP
		StompAllocator::Release(MemoryHeader::DetachHeader(obj));
#else
		s_pool.Push(MemoryHeader::DetachHeader(obj));
#endif
	}

	template<typename... Args>
	static shared_ptr<Type> MakeShared(Args&&... args)
	{
		shared_ptr<Type> ptr = { Pop(forward<Args>(args)...),Push };
		return ptr;
	}

private:
	// static이지만 template이라서 Type에 따라 각각의 클래스가 생김-> 각각의 static변수가 생김
	static int32		s_allocSize;
	static MemoryPool	s_pool;
};

template<typename Type>
int32 ObjectPool<Type>::s_allocSize = sizeof(Type) + sizeof(MemoryHeader);

template<typename Type>
MemoryPool ObjectPool<Type>::s_pool{ s_allocSize };
