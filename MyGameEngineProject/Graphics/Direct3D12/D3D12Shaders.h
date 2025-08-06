#pragma once
#include "D3D12CommonHeaders.h"

namespace primal::graphics::d3d12::shaders {

    struct shader_type {
        enum type : u32 {
            vertex = 0,
            hull,
            domain,
            geometry,
            pixel,
            compute,
            amplification,
            mesh,
            count
        };
    };

    struct agilis_shader {
        enum id : u32 {
            fullscreen_triangle_vs = 0,
            count
        };
    };

    bool initialize();
    void shutdown();

    D3D12_SHADER_BYTECODE get_agilis_shader(agilis_shader::id id);

}