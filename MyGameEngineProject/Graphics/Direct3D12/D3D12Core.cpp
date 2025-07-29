#include "D3D12Core.h"

using namespace Microsoft::WRL;

namespace primal::graphics::d3d12::core
{
	namespace {
		ID3D12Device8* main_device{ nullptr };
		IDXGIFactory7* dxgi_factory{ nullptr };

		constexpr D3D_FEATURE_LEVEL minimum_feature_level{ D3D_FEATURE_LEVEL_11_0 };

		bool failed_init()
		{
			shutdown();
			return false;
		}

		IDXGIAdapter4*
		determine_main_adapter()
		{
			IDXGIAdapter4* adapter{ nullptr };
			// get adapters in descending order of performance
			for (u32 i{ 0 }; dxgi_factory->EnumAdapterByGpuPreference(i, DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE, IID_PPV_ARGS(&adapter)) != DXGI_ERROR_NOT_FOUND;
			++i)
			{
				// pick the first adapter that supports the minimum feature level
				if (SUCCEEDED(D3D12CreateDevice(adapter, minimum_feature_level, __uuidof(ID3D12Device), nullptr)))
				{
					return adapter;
				}
				release(adapter);
			}
			return nullptr;
		}

	}//anonymous

	bool initialize()
	{
		// determine what is the maximum feature level that is supporter
		// create a ID3D12Device
		if (main_device) shutdown();

		u32 dxgi_factory_flags{ 0 };
#ifdef _DEBUG
		dxgi_factory_flags |= DXGI_CREATE_FACTORY_DEBUG;
#endif // _DEBUG

		HRESULT hr{ S_OK };
		hr = CreateDXGIFactory2(dxgi_factory_flags, IID_PPV_ARGS(&dxgi_factory)); // uuid universally unique identifier
		if (FAILED(hr)) return failed_init();

		// determine which adapter to use, if any
		ComPtr<IDXGIAdapter4> main_adapter;
		main_adapter.Attach(determine_main_adapter());

		if (!main_adapter) return failed_init();
	}
}
