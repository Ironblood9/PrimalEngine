#pragma once

#define USE_STL_VECTOR 1
#define USE_STL_DEQUE 1

#if USE_STL_VECTOR
#include <vector>

namespace primal::utl {
	template<typename T>
	using vector = vector<T>;
}
#endif

#if USE_STL_DEQUE
#include <deque>

namespace primal::utl {
	template<typename T>
	using deque = deque<T>;
}
#endif

namespace primal::utl {
	//TODO:implement our own containers

}











