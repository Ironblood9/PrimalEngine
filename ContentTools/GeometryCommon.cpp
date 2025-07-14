#include "GeometryCommon.h"

namespace primal::tools {
	namespace {
		using namespace math;
		using namespace DirectX;

		void recalculate_normals(mesh& mesh)
		{
			const u32 num_indices{ (u32)mesh.raw_indices.size() };
			mesh.normals.reserve(num_indices);

			for (u32 i{ 0 }; i < num_indices; i++)
			{
				const u32 i0{ mesh.raw_indices[i] };
				const u32 i1{ mesh.raw_indices[++i] };
				const u32 i2{ mesh.raw_indices[++i] };

				XMVECTOR v0{ XMLoadFloat3(&mesh.positions[i0]) };
				XMVECTOR v1{ XMLoadFloat3(&mesh.positions[i1]) };
				XMVECTOR v2{ XMLoadFloat3(&mesh.positions[i2]) };

				XMVECTOR e0{ v1 - v0 };
				XMVECTOR e1{ v2 - v0 };
				XMVECTOR n{ XMVector3Normalize(XMVector3Cross(e0, e1)) };

				XMStoreFloat3(&mesh.normals[i], n);
				mesh.normals[i - 1] = mesh.normals[i];
				mesh.normals[i - 2] = mesh.normals[i];
			}
		}
		

		void process_vertices(mesh& mesh, const geometry_import_settings& settings)
		{
			assert((mesh.raw_indices.size() % 3) == 0);
			if (settings.calculate_normals || mesh.normals.empty())
			{
				recalculate_normals(mesh);
			}
			process_normals(mesh, settings.smoothing_angle);

			if (!mesh.uv_sets.empty())
			{
				process_uvs(mesh);
			}
			pack_vertices_static(mesh);
		}
	}//anonymous

	void process_scene(scene& scene, const geometry_import_settings& settings)
	{
		for(auto& lod: scene.lod_groups)
			for (auto& mesh : lod.meshes)
			{
				process_vertices(mesh, settings);
			}
	}
	void pack_data(const scene& scene, scene_data& data)
	{ }
}






