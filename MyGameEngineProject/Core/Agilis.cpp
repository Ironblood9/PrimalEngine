
#if !defined(SHIPPING)
#include"..\Content\LoadContent.h"
#include"..\Components\Script.h"
#include<thread>

bool agilis_initialize()
{
	bool result{ primal::content::load_game() };
	return result;
}

void agilis_update()
{
	primal::script::update(10.f);
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void agilis_shutdown()
{
	primal::content::unload_game();
}
#endif // !defined(SHIPPING)




















