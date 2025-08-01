#pragma once
#include "D3D12CommonHeaders.h"


namespace primal::graphics::d3d12
{
	class d3d12_surface
	{
	public:
		explicit d3d12_surface(platform::window window) : _window{ window }
		{
			assert(_window.handle());
		}

		~d3d12_surface() { release(); }

		void create_swap_chain(IDXGIFactory7* factory, ID3D12CommandQueue* cmd_queue, DXGI_FORMAT format);
		void present() const;
		void resize();

		u32 width() const {}
		u32 height() const {}

	private:
		void release();

		IDXGISwapChain4* _swap_chain{ nullptr };
		platform::window _window{};
	};
}





