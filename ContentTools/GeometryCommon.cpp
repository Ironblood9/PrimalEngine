#include "GeometryCommon.h"

namespace primal::tools {
	namespace {
		using namespace math;
		using namespace DirectX;

		void recalculate_normals(mesh& mesh)
		{
			const u32 num_indices{ (u32)mesh.raw_indices.size() };
			mesh.normals.resize(num_indices);

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
			const f32 cos_alpha{ XMScalarCos(pi - smoothing_angle * pi / 180.f) };
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
							f32 cos_theta{ 0.f };
							XMVECTOR n2{ XMLoadFloat3(&mesh.normals[refs[k]]) };
							if (!is_soft_edge)
							{
								// accounting for the lenght of n1.We assume unit lenght for n2
								XMStoreFloat(&cos_theta, XMVector3Dot(n1, n2) * XMVector3ReciprocalLength(n1));
							}
							if (is_soft_edge || cos_theta >= cos_alpha)
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

			for (u32 i{ 0 }; i < num_vertices; i++)
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
				const u16 normal_x{ (u16)pack_float<16>(vertex.normal.x, -1.f, 1.f) };
				const u16 normal_y{ (u16)pack_float<16>(vertex.normal.y, -1.f, 1.f) };

				mesh.packed_vertices_static.emplace_back(packed_vertex::vertex_static
					{
						vertex.position, {0, 0, 0}, signs,
						{normal_x, normal_y}, {},
						vertex.uv
					});
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
		u64 get_mesh_size(const mesh& mesh)
		{
			const u64 num_vertices{ mesh.vertices.size() };
			const u64 vertex_buffer_size{ sizeof(packed_vertex::vertex_static) * num_vertices };
			const u64 index_size{ (num_vertices < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			const u64 index_buffer_size{ index_size * mesh.indices.size() };
			constexpr u64 su32{ sizeof(u32) };
			const u64 size
			{
				su32 + mesh.name.size() + // room for mesh name string and mesh name lenght
				su32 + // lod id
				su32 + // vertex size
				su32 + // number of verticies
				su32 + // index size (16 or 32 bit)
				su32 + // number of indices
				sizeof(f32) + // LOD threshold
				vertex_buffer_size + // room for vertices
				index_buffer_size  // room for indices
			};
			return size;
		}

		u64 get_scene_size(const scene& scene)
		{
			constexpr u64 su32{ sizeof(u32) };
			u64 size
			{
				su32 +                  // name lenght
				scene.name.size() +     // room for scene name string
				su32                    //number of LODs
			};
			for (auto& lod : scene.lod_groups)
			{
				u64 lod_size
				{
					su32 + lod.name.size() + // room for LPD name string and LOAD name lenght
					su32                     // number of meshes in this LOD  
				};
				for (auto& mesh : lod.meshes)
				{
					lod_size += get_mesh_size(mesh);
				}
				size += lod_size;
			}
			return size;
		}

		void pack_mesh_data(const mesh& mesh, u8* const buffer, u64& at)
		{
			constexpr u64 su32{ sizeof(u32) };
			u32 s{ 0 };
			// mesh name
			s = (u32)mesh.name.size();
			memcpy(&buffer[at], &s, su32); at += su32;
			memcpy(&buffer[at], mesh.name.c_str(), s); at += s;
			// lod id
			s = mesh.lod_id;
			memcpy(&buffer[at], &s, su32); at += su32;
			// vertex size
			constexpr u32 vertex_size{ sizeof(packed_vertex::vertex_static) };
			s = vertex_size;
			memcpy(&buffer[at], &s, su32); at += su32;
			// number of vertices
			const u32 num_vertices{ (u32)mesh.vertices.size() };
			s = num_vertices;
			memcpy(&buffer[at], &s, su32); at += su32;
			// index size(16 or 32 bit)
			const u32 index_size{ (num_vertices < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			s = index_size;
			memcpy(&buffer[at], &s, su32); at += su32;
			// number of indices
			const u32 num_indices{ (u32)mesh.indices.size() };
			s = num_indices;
			memcpy(&buffer[at], &s, su32); at += su32;
			// LOD threshold
			memcpy(&buffer[at], &mesh.lod_threshold, sizeof(f32)); at += sizeof(f32);
			// vertex data
			s = vertex_size * num_vertices;
			memcpy(&buffer[at], mesh.packed_vertices_static.data(), s); at += s;
			// index data
			s = index_size * num_indices;
			void* data{ (void*)mesh.indices.data() };
			utl::vector<u16> indices;

			if (index_size == sizeof(u16))
			{
				indices.resize(num_indices);
				for (u32 i{ 0 }; i < num_indices; i++) indices[i] = (u16)mesh.indices[i];
				data = (void*)indices.data();
			}
			memcpy(&buffer[at], data, s); at += s;

		}

		bool split_meshes_by_material(u32 material_idx, const mesh& m, mesh& submesh)
		{
			submesh.name = m.name;
			submesh.lod_threshold = m.lod_threshold;
			submesh.lod_id = m.lod_id;
			submesh.material_used.emplace_back(material_idx);
			submesh.uv_sets.resize(m.uv_sets.size());

			const u32 num_polys{ (u32)m.raw_indices.size() / 3 };
			utl::vector<u32> vertex_ref(m.positions.size(), u32_invalid_id);

			for (u32 i{ 0 }; i < num_polys; ++i)
			{
				const u32 mtl_idx{ m.material_indices[i] };
				if (mtl_idx != material_idx) continue;

				const u32 index{ i * 3 };
				for (u32 j = index; j < index + 3; ++j)
				{
					const u32 v_idx{ m.raw_indices[j] };
					if (vertex_ref[v_idx] != u32_invalid_id)
					{
						submesh.raw_indices.emplace_back(vertex_ref[v_idx]);
					}
					else
					{
						submesh.raw_indices.emplace_back((u32)submesh.positions.size());
						vertex_ref[v_idx] = submesh.raw_indices.back();
						submesh.positions.emplace_back(m.positions[v_idx]);
					}

					if (m.normals.size())
					{
						submesh.normals.emplace_back(m.normals[j]);
					}

					if (m.tangents.size())
					{
						submesh.tangents.emplace_back(m.tangents[j]);
					}

					for (u32 k{ 0 }; k < m.uv_sets.size(); ++k)
					{
						if (m.uv_sets[k].size())
						{
							submesh.uv_sets[k].emplace_back(m.uv_sets[k][j]);
						}
					}
				}
			}
			assert((submesh.raw_indices.size() % 3) == 0);
			return !submesh.raw_indices.empty();
		}



		void split_meshes_by_material(scene& scene)
		{
			for (auto& lod : scene.lod_groups)
			{
				utl::vector<mesh> new_meshes;
				for (auto& m : lod.meshes)
				{
					const u32 num_materials{ (u32)m.material_used.size() };
					if (num_materials > 1)
					{
						for (u32 i{ 0 }; i < num_materials; i++)
						{
							mesh submesh{};
							if (split_meshes_by_material(m.material_used[i], m, submesh))
							{
								new_meshes.emplace_back(submesh);
							}
						}
					}
					else
					{
						new_meshes.emplace_back(m);
					}
				}
				new_meshes.swap(lod.meshes);
			}
		}

	}//anonymous

	void process_scene(scene& scene, const geometry_import_settings& settings)
	{
		split_meshes_by_material(scene);

		for(auto& lod: scene.lod_groups)
			for (auto& mesh : lod.meshes)
			{
				process_vertices(mesh, settings);
			}
	}

	

	void pack_data(const scene& scene, scene_data& data)
	{ 
		constexpr u64 su32{ sizeof(u32) };
		const u64 scene_size{ get_scene_size(scene) };
		data.buffer_size = (u32)scene_size;
		data.buffer = (u8*)CoTaskMemAlloc(scene_size);
		assert(data.buffer);

		u8* const buffer{ data.buffer };
		u64 at{ 0 };
		u32 s{ 0 };
		//scene name
		s = (u32)scene.name.size();
		memcpy(&buffer[at], &s, su32); at += su32;
		memcpy(&buffer[at], scene.name.c_str(), s); at += s;
		//number of LODs
		s = (u32)scene.lod_groups.size();
		memcpy(&buffer[at], &s, su32); at += su32;

		for (auto& lod : scene.lod_groups)
		{
			// LOD name
			s = (u32)lod.name.size();
			memcpy(&buffer[at], &s, su32); at += su32;
			memcpy(&buffer[at], lod.name.c_str(), s); at += s;

			// number of meshes in this LOD
			s = (u32)lod.meshes.size();
			memcpy(&buffer[at], &s, su32); at += su32;

			for (auto& mesh : lod.meshes)
			{
				pack_mesh_data(mesh, buffer, at);
			}
		}
		assert(scene_size == at);
	}
}






