#include "D3D12Shaders.h"
#include "Content/LoadContent.h"

namespace primal::graphics::d3d12::shaders {
    namespace {

        typedef struct compiled_shader
        {
            u64         size;
            const u8*   byte_code;
        } const *compiled_shader_ptr;

        // Each element in this array points to an offset withing the shaders blob.
        compiled_shader_ptr agilis_shaders[agilis_shader::count]{};

        // This is a chunk of memory that contains all compiled engine shaders.
        // The blob is an array of shader byte code consisting of a u64 size and
        // an array of bytes.
        std::unique_ptr<u8[]> shaders_blob{};

        bool
            load_agilis_shaders()
        {
            assert(!shaders_blob);
            u64 size{ 0 };
            bool result{ content::load_agilis_shaders(shaders_blob, size) };
            assert(shaders_blob && size);

            u64 offset{ 0 };
            u32 index{ 0 };
            while (offset < size && result)
            {
                assert(index < agilis_shader::count);
                compiled_shader_ptr& shader{ agilis_shaders[index] };
                assert(!shader);
                result &= index < agilis_shader::count && !shader;
                if (!result) break;
                shader = reinterpret_cast<const compiled_shader_ptr>(&shaders_blob[offset]);
                offset += sizeof(u64) + shader->size;
                ++index;
            }

            assert(offset == size && index == agilis_shader::count);
            return result;
        }
    }

    bool initialize()
    {
        return load_agilis_shaders();
    }

    void shutdown()
    {
        for (u32 i{ 0 }; i < agilis_shader::count; i++)
        {
            agilis_shaders[i] = {};
        }
        shaders_blob.reset();
    }

    D3D12_SHADER_BYTECODE 
    get_agilis_shader(agilis_shader::id id)
    {
        assert(id < agilis_shader::count);
        const compiled_shader_ptr shader{ agilis_shaders[id] };
        assert(shader && shader->size);
        return { &shader->byte_code, shader->size };
    }



}