/*
things to do to create a game project

Generate an MSVC project
Add files that contain the script
Set include and libary directories C:\Users\Msý\source\repos\PrimalEngine\x64\Debug\net8.0-windows\MyGameEngineProject.lib þeklinde nokta atýþý ver sadece file ismini verme
Set force include file
Set c++ 17 language version and calling convension
*/

#ifdef _WIN64
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN
#include<Windows.h>
#include<crtdbg.h>
#ifndef USE_WITH_EDITOR

extern bool agilis_initialize();
extern void agilis_update();
extern void agilis_shutdown();

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif
	if (agilis_initialize())
	{
		MSG msg{};
		bool is_running{ true };
		while (is_running)
		{
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
				is_running &= (msg.message != WM_QUIT);
			}
			agilis_update();
		}
	}
	agilis_shutdown();
	return 0;
}
#endif // !USE_WITH_EDITOR
#endif // _WIN64
