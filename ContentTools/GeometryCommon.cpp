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
		
		void process_normals(mesh& mesh, f32 smoothing_angle)
		{
			const f32 cos_angle{ XMScalarCos(pi - smoothing_angle * pi / 180.f) };
			const bool is_hard_edge{ XMScalarNearEqual(smoothing_angle, 180.f, epsilon) };
			const bool is_soft_edge{ XMScalarNearEqual(smoothing_angle, 0.f, epsilon) };
			const u32 num_indices{ (u32)mesh.raw_indices.size() };
			const u32 num_vertices{ (u32)mesh.positions.size() };
			assert(num_indices && num_vertices);

			mesh.indices.resize(num_indices);

			utl::vector<utl::vector<u32>> idx_ref(num_vertices);
			for (u32 i{ 0 }; i < num_indices; i++)
				idx_ref[mesh.raw_indices[i]].emplace_back(i);

			for (u32 i{ 0 }; i < num_vertices; i++)
			{
				auto& refs{ idx_ref[i] };
				u32 num_refs{ (u32)refs.size() };
				for (u32 j{ 0 }; j < num_refs; j++)
				{
					mesh.indices[refs[j]] = (u32)mesh.vertices.size();
					vertex& vertex{ mesh.vertices.emplace_back() };
					vertex.position = mesh.positions[mesh.raw_indices[refs[j]]];

					XMVECTOR n1{ XMLoadFloat3(&mesh.normals[refs[j]]) };
					if (!is_hard_edge)
					{
						for (u32 k{ j + 1 }; k < num_refs; k++)
						{
						   // this value represents the cosine of the angle between normals
							f32 n{ 0.f };
							XMVECTOR n2{ XMLoadFloat3(&mesh.normals[refs[k]]) };
							if (!is_soft_edge)
							{
								// accounting for the lenght of n1.We assume unit lenght for n2
								XMStoreFloat(&n, XMVector3Dot(n1, n2) * XMVector3ReciprocalLength(n1));
							}
							if (is_soft_edge || n >= cos_angle)
							{
								n1 += n2;
								mesh.indices[refs[k]] = mesh.indices[refs[j]];
								refs.erase(refs.begin() + k);
								--num_refs;
								--k;
							}
						}
					}
					XMStoreFloat3(&vertex.normal, XMVector3Normalize(n1));
				}
			}
		}

		void process_uvs(mesh& mesh)
		{
			utl::vector<vertex> old_vertices;
			old_vertices.swap(mesh.vertices);
			utl::vector<u32> old_indices(mesh.indices.size());
			old_indices.swap(mesh.indices);

			const u32 num_vertices{ (u32)old_vertices.size() };
			const u32 num_indices{ (u32)old_indices.size() };
			assert(num_vertices && num_indices);

			utl::vector<utl::vector<u32>> idx_ref(num_vertices);
			for (u32 i{ 0 }; i < num_indices; i++)
				idx_ref[old_indices[i]].emplace_back(i);

			for (u32 i{ 0 }; i < num_indices; i++)
			{
				auto& refs{ idx_ref[i] };
				u32 num_refs{ (u32)refs.size() };
				for (u32 j{ 0 }; j < num_refs; j++)
				{
					mesh.indices[refs[j]] = (u32)mesh.vertices.size();
					vertex& vertex{ old_vertices[old_indices[refs[j]]] };
					vertex.uv = mesh.uv_sets[0][refs[j]];
					mesh.vertices.emplace_back(vertex);

					for (u32 k{ j + 1 }; k < num_refs; k++)
					{
						v2& uv1{ mesh.uv_sets[0][refs[k]] };
						if (XMScalarNearEqual(vertex.uv.x, uv1.x, epsilon) && XMScalarNearEqual(vertex.uv.y, uv1.y, epsilon))
						{
							mesh.indices[refs[k]] = mesh.indices[refs[j]];
							refs.erase(refs.begin() + k);
							--num_refs;
							--k;
						}
					}
				}
			}

		}

		void pack_vertices_static(mesh& mesh)
		{
			const u32 num_vertices{ (u32)mesh.vertices.size() };
			assert(num_vertices);
			mesh.packed_vertices_static.reserve(num_vertices);

			for (u32 i{ 0 }; i < num_vertices; i++)
			{
				vertex& vertex{ mesh.vertices[i] };
				const u8 signs{ (u8)((vertex.normal.z > 0.f) << 1) };
				const u8 normal_x{ (u16)pack_float<16>(vertex.normal.x, -1.f, 1.f) };
				const u8 normal_y{ (u16)pack_float<16>(vertex.normal.y, -1.f, 1.f) };
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






