#include "Platform.h"
#include "PlatformTypes.h"

namespace primal::platform {

#ifdef _WIN64

	namespace 
	{
		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{

		}


	}//anonymous
	window create_window(const window_init_info* const init_info /*nullptr*/)
	{
		window_proc callback{ init_info ? init_info->callback : nullptr };
		window_handle parent{ init_info ? init_info->parent : nullptr };

		// setup a window class
		WNDCLASSEX wc;
		ZeroMemory(&wc, sizeof(wc));
		wc.cbSize= sizeof(WNDCLASSEX);
		wc.style= CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc= internal_window_proc;
		wc.cbClsExtra=0;
		wc.cbWndExtra= callback ? sizeof(callback) : 0;
		wc.hInstance;
		wc.hIcon;
		wc.hCursor;
		wc.hbrBackground;
		wc.lpszMenuName;
		wc.lpszClassName;
		wc.hIconSm;

		//register the window class

		// Create a instance of the window class

	}
#elif
#error "must implement at least one platform"
#endif // _WIN64

}