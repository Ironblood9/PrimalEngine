#include "MeshPrimitives.h"
#include "GeometryCommon.h"

namespace primal::tools {
namespace {
	using primitive_mash_creator = void(*)(scene&, const primitive_init_info& info);

	void create_plane(scene& scene, const primitive_init_info& info);
	void create_cube(scene& scene, const primitive_init_info& info);
	void create_uv_sphere(scene& scene, const primitive_init_info& info);
	void create_ico_sphere(scene& scene, const primitive_init_info& info);
	void create_cylinder(scene& scene, const primitive_init_info& info);
	void create_capsule(scene& scene, const primitive_init_info& info);

	primitive_mash_creator creators[]
	{
		 create_plane,
		 create_cube,
		 create_uv_sphere,
		 create_ico_sphere,
		 create_cylinder,
		 create_capsule,
	};
	static_assert(_countof(creators) == primitive_mash_type::count);

	struct axis
	{
		enum : u32
		{
			x = 0,
			y = 1,
			z = 2,
		};
	};

	mesh
	create_plane(const primitive_init_info& info, u32 horizontal_index = axis::x, u32 vertical_index = axis::z, bool flip_winding = false,
			     math::v3 offset = { -0.5f, 0.f, -0.5f }, math::v2 u_range = { 0.f, 1.f }, math::v2 v_range = { 0.f, 1.f }) 
	{
		assert(horizontal_index < 3 && vertical_index < 3);
		assert(horizontal_index != vertical_index);

		const u32 horizontal_count{ clamp(info.segments[horizontal_index], 1u, 10u) };
		const u32 vertical_count{ clamp(info.segments[vertical_index], 1u, 10u) };
	}

	void create_plane(scene& scene, const primitive_init_info& info)
	{
		lod_group lod{};
		lod.name = "plane";
		lod.meshes.emplace_back(create_plane(info));
		scene.lod_groups.emplace_back(lod);
	}
	void create_cube(scene& scene, const primitive_init_info& info) 
	{

	}
	void create_uv_sphere(scene& scene, const primitive_init_info& info)
	{ }
	void create_ico_sphere(scene& scene, const primitive_init_info& info)
	{}
	void create_cylinder(scene& scene, const primitive_init_info& info)
	{ }
	void create_capsule(scene& scene, const primitive_init_info& info)
	{ }

}//anonymous

EDITOR_INTERFACE void
CreatePrimitiveMesh(scene_data* data, primitive_init_info* info)
{
	assert(data && info);
	assert(info->type < primitive_mash_type::count);
	scene scene{};
	creators[info->type](scene, *info);
}
}