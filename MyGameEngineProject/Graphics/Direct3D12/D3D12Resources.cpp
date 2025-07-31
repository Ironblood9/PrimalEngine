
#include "D3D12Resources.h"


namespace primal::graphics::d3d12
{
	///DESCRIPTOR HEAP///
	bool 
	descriptor_heap::initialize(u32 capacity, bool is_shader_visible)
	{ }
	void 
	descriptor_heap::release()
	{ }

	descriptor_handle 
	descriptor_heap::allocate()
	{ }
	void 
	descriptor_heap::free(descriptor_handle& handle)
	{ }
}