#pragma once

#include "CommonHeaders.h"
#include "..\Platform\window.h"

namespace primal::graphics {

	class surface
	{
	};
	
	struct render_surface
	{
		platform::window window{};
		surface surface{};
	};
}















