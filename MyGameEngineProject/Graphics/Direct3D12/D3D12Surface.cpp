#include "D3D12Surface.h"
#include "D3D12Core.h"
namespace primal::graphics::d3d12
{
	namespace {
		constexpr DXGI_FORMAT
		to_non_srgb(DXGI_FORMAT format)
		{
			if (format == DXGI_FORMAT_R8G8B8A8_UNORM_SRGB) return DXGI_FORMAT_R8G8B8A8_UNORM;
			return format;
		}
	}//anonyomus

	void d3d12_surface::create_swap_chain(IDXGIFactory7* factory, ID3D12CommandQueue* cmd_queue, DXGI_FORMAT format)
	{
		assert(factory && cmd_queue);
		release();

		DXGI_SWAP_CHAIN_DESC1 desc{};
		desc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;
		desc.BufferCount = frame_buffer_count;
		desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		desc.Flags = 0;
		desc.Format = to_non_srgb(format);
		desc.Height = _window.height();
		desc.Width = _window.width();
		desc.SampleDesc.Count = 1;
		desc.SampleDesc.Quality = 0;
		desc.Scaling = DXGI_SCALING_STRETCH;
		desc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
		desc.Stereo = false;

		IDXGISwapChain1* swap_chain;
		HWND hwnd{ (HWND)_window.handle() };
		DXCall(factory->CreateSwapChainForHwnd(cmd_queue, hwnd, &desc, nullptr, nullptr, &swap_chain));
		DXCall(factory->MakeWindowAssociation(hwnd, DXGI_MWA_NO_ALT_ENTER));
		DXCall(swap_chain->QueryInterface(IID_PPV_ARGS(&_swap_chain)));
		core::release(swap_chain);

		_swap_chain->GetCurrentBackBufferIndex();

	}
}
