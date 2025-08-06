#pragma once
#include"CommonHeaders.h"

#if !defined(SHIPPING)
namespace primal::content {

bool load_game();	
void unload_game();

bool load_agilis_shaders(std::unique_ptr<u8[]>& shaders, u64& size);
}
#endif // !defined(SHIPPING)









