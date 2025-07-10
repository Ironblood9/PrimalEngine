#pragma once

// C/C++
#include <stdint.h>
#include <assert.h>
#include<typeinfo>
#include<memory>
#include<unordered_map>
#include<string>

#if defined(_WIN64)
#include <DirectXMath.h>
#endif


//Common headers
#include "..\Utilities\Utilities.h"
#include "PrimitiveTypes.h"
#include "Id.h"
#include "..\Utilities\MathTypes.h"

#ifdef _DEBUG
#define DEBUG_OP(x) x
#else
#define DEBUG_OP(x) (void(0))
#endif // _DEBUG















