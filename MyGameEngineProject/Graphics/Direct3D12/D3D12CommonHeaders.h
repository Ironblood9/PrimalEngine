#pragma once
#include "CommonHeaders.h"
#include "Graphics\Renderer.h"

#include <dxgi1_6.h>
#include <d3d12.h>
#include <wrl.h>

#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3d12.lib")


// Asset that COM call to D3D API succeeded

#ifdef _DEBUG

#ifndef DXCall
#define DXCall(x)                                         \
if(FAILED(x)) {                                           \
   char line_number[32];                                  \
   sprintf_s(line_number, "%u", __LINE__);                \
   OutputDebugStringA("Error in: ");                      \
   OutputDebugStringA(__FILE__);                          \
   OutputDebugStringA("\nLine: ");                        \
   OutputDebugStringA(line_number);                       \
   OutputDebugStringA("\n");                              \
   OutputDebugStringA(#x);                                \
   OutputDebugStringA("\n");                              \
   __debugbreak();                                        \
}
#endif // !DXCall
#else
#ifndef DXCall
#define DXCall(x) x
#endif // !DXCall
#endif // _DEBUG


