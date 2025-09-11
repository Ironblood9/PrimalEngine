#include "GeometryCommon.h"
#include "..\Utilities\IOStream.h"

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

		u64	get_vertex_element_size(elements::elements_type::type elements_type)
		{
			using namespace elements;
			switch (elements_type)
			{
			case elements_type::static_normal:               return sizeof(static_normal);
			case elements_type::static_normal_texture:       return sizeof(static_normal_texture);
			case elements_type::static_color:                return sizeof(static_color);
			case elements_type::skeletal:                    return sizeof(skeletal);
			case elements_type::skeletal_color:              return sizeof(skeletal_color);
			case elements_type::skeletal_normal:             return sizeof(skeletal_normal);
			case elements_type::skeletal_normal_color:       return sizeof(skeletal_normal_color);
			case elements_type::skeletal_normal_texture:     return sizeof(skeletal_normal_texture);
			case elements_type::skeletal_normal_texture_color:return sizeof(skeletal_normal_texture_color);
			}

			return 0;
		}

		void pack_vertices(mesh& m)
		{
			const u32 num_vertices{ (u32)m.vertices.size() };
			assert(num_vertices);

			m.position_buffer.resize(sizeof(math::v3) * num_vertices);
			math::v3 *const position_buffer{ (math::v3 *const)m.position_buffer.data() };

			for (u32 i{ 0 }; i < num_vertices; ++i)
			{
				position_buffer[i] = m.vertices[i].position;
			}

			struct u16v2 { u16 x, y; };
			struct u8v3 { u8 x, y, z; };

			utl::vector<u8> t_signs(num_vertices);
			utl::vector<u16v2> normals(num_vertices);
			utl::vector<u16v2> tangents(num_vertices);
			utl::vector<u8v3> joint_weights(num_vertices);

			if (m.elements_type & elements::elements_type::static_normal)
			{
				// normals only
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					t_signs[i] = (u8)((v.normal.z > 0.f) << 1);
					normals[i] = { (u16)pack_float<16>(v.normal.x, -1.f, 1.f),
								   (u16)pack_float<16>(v.normal.y, -1.f, 1.f) };
				}
			}

			if (m.elements_type & elements::elements_type::static_normal_texture)
			{
				// full T-space
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					t_signs[i] |= (u8)((v.tangent.w > 0.f) && (v.tangent.z > 0.f));
					tangents[i] = { (u16)pack_float<16>(v.tangent.x, -1.f, 1.f),
									(u16)pack_float<16>(v.tangent.y, -1.f, 1.f) };
				}
			}

			if (m.elements_type & elements::elements_type::skeletal)
			{
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					// pack joint weights (from [0.0, 1.0] to [0..255])
					joint_weights[i] = {
						(u8)pack_unit_float<8>(v.joint_weights.x),
						(u8)pack_unit_float<8>(v.joint_weights.y),
						(u8)pack_unit_float<8>(v.joint_weights.z)
					};
				}
			}

			m.element_buffer.resize(get_vertex_element_size(m.elements_type) * num_vertices);
			using namespace elements;

			switch (m.elements_type)
			{
			case elements_type::static_color:
			{
				static_color *const element_buffer{ (static_color *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					element_buffer[i] = { {v.red, v.green, v.blue}, {} };
				}
			}
			break;
			case elements_type::static_normal:
			{
				static_normal *const element_buffer{ (static_normal *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					element_buffer[i] = { {v.red, v.green, v.blue}, t_signs[i],
										  {normals[i].x, normals[i].y} };
				}
			}
			break;
			case elements_type::static_normal_texture:
			{
				static_normal_texture *const element_buffer{ (static_normal_texture *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					element_buffer[i] = { {v.red, v.green, v.blue}, t_signs[i],
										  {normals[i].x, normals[i].y}, {tangents[i].x, tangents[i].y},
										   v.uv };
				}
			}
			break;
			case elements_type::skeletal:
			{
				skeletal *const element_buffer{ (skeletal *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y , (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, {},
										   {indices[0], indices[1], indices[2], indices[3]} };
				}
			}
			break;
			case elements_type::skeletal_color:
			{
				skeletal_color *const element_buffer{ (skeletal_color *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y , (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, {},
										   {indices[0], indices[1], indices[2], indices[3]},
											{v.red, v.green, v.blue}, {} };
				}
			}
			break;

			case elements_type::skeletal_normal:
			{
				skeletal_normal *const element_buffer{ (skeletal_normal *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y,
										  (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, t_signs[i],
										  {indices[0], indices[1], indices[2], indices[3]},
										  {normals[i].x, normals[i].y} };
				}
			}
			break;
			case elements_type::skeletal_normal_color:
			{
				skeletal_normal_color *const element_buffer{ (skeletal_normal_color *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y,
										  (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, t_signs[i],
										  {indices[0], indices[1], indices[2], indices[3]},
										  {normals[i].x, normals[i].y}, {v.red, v.green, v.blue}, {} };
				}
			}
			break;
			case elements_type::skeletal_normal_texture:
			{
				skeletal_normal_texture *const element_buffer{ (skeletal_normal_texture *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y,
										  (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, t_signs[i],
										  {indices[0], indices[1], indices[2], indices[3]},
										  {normals[i].x, normals[i].y}, { tangents[i].x, tangents[i].y }, v.uv };
				}
			}
			break;
			case elements_type::skeletal_normal_texture_color:
			{
				skeletal_normal_texture_color *const element_buffer{ (skeletal_normal_texture_color *const)m.element_buffer.data() };
				for (u32 i{ 0 }; i < num_vertices; ++i)
				{
					vertex& v{ m.vertices[i] };
					const u16 indices[4]{ (u16)v.joint_indices.x, (u16)v.joint_indices.y,
										  (u16)v.joint_indices.z, (u16)v.joint_indices.w };
					element_buffer[i] = { {joint_weights[i].x, joint_weights[i].y, joint_weights[i].z}, t_signs[i],
										  {indices[0], indices[1], indices[2], indices[3]},
										  {normals[i].x, normals[i].y}, { tangents[i].x, tangents[i].y }, v.uv,
										  {v.red, v.green, v.blue}, {} };
				}
			}
			break;
			}
		}



		void determine_elements_type(mesh& m)
		{
			using namespace elements;
			if (m.normals.size())
			{
				if (m.uv_sets.size() && m.uv_sets[0].size())
				{
					m.elements_type = elements_type::static_normal_texture;
				}
				else
				{
					m.elements_type = elements_type::static_normal;
				}
			}
			else if (m.colors.size())
			{
				m.elements_type = elements_type::static_color;
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

			determine_elements_type(mesh);
			pack_vertices(mesh);
		}

		u64 get_mesh_size(const mesh& mesh)
		{
			const u64 num_vertices{ mesh.vertices.size() };
			const u64 position_buffer_size{ mesh.position_buffer.size() };
			assert(position_buffer_size == sizeof(math::v3) * num_vertices);
			const u64 element_buffer_size{ mesh.element_buffer.size() };
			assert(element_buffer_size == get_vertex_element_size(mesh.elements_type) * num_vertices);
			const u64 index_size{ (num_vertices < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			const u64 index_buffer_size{ index_size * mesh.indices.size() };
			constexpr u64 su32{ sizeof(u32) };
			const u64 size
			{
				su32 + mesh.name.size() + // room for mesh name string and mesh name lenght
				su32 + // lod id
				su32 + // vertex element size
				su32+  // element type enumaration
				su32 + // number of verticies
				su32 + // index size (16 or 32 bit)
				su32 + // number of indices
				sizeof(f32) + // LOD threshold
				position_buffer_size + // room for vertex positions
				element_buffer_size + // room for vertex elements
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

		void pack_mesh_data(const mesh& mesh, utl::blob_stream_writer& blob)
		{
			// mesh name
			blob.write((u32)mesh.name.size());
			blob.write(mesh.name.c_str(), mesh.name.size());
			// lod id
			blob.write(mesh.lod_id);
			// vertex element size
			const u32 elements_size{ (u32)get_vertex_element_size(mesh.elements_type) };
			blob.write(elements_size);
		    // elements type numeration
			blob.write((u32)mesh.elements_type);
			// number of vertices
			const u32 num_vertices{ (u32)mesh.vertices.size() };
			blob.write(num_vertices);
			// index size(16 or 32 bit)
			const u32 index_size{ (num_vertices < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			blob.write(index_size);
			// number of indices
			const u32 num_indices{ (u32)mesh.indices.size() };
			blob.write(num_indices);
			// LOD threshold
			blob.write(mesh.lod_threshold);
			// position buffer
			assert(mesh.position_buffer.size() == sizeof(math::v3) * num_vertices);
			blob.write(mesh.position_buffer.data(), mesh.position_buffer.size());
			// element buffer
			assert(mesh.element_buffer.size() == elements_size * num_vertices);
			blob.write(mesh.element_buffer.data(), mesh.element_buffer.size());
			// index data
			const u32 index_buffer_size{ index_size * num_indices };
			const u8* data{ (const u8*)mesh.indices.data() };
			utl::vector<u16> indices;

			if (index_size == sizeof(u16))
			{
				indices.resize(num_indices);
				for (u32 i{ 0 }; i < num_indices; i++) indices[i] = (u16)mesh.indices[i];
				data = (const u8*)indices.data();
			}
			blob.write(data, index_buffer_size);

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

		utl::blob_stream_writer blob{ data.buffer, data.buffer_size };
		//scene name
		blob.write((u32)scene.name.size());
		blob.write(scene.name.c_str(), scene.name.size());
		//number of LODs
		blob.write((u32)scene.lod_groups.size());
		

		for (auto& lod : scene.lod_groups)
		{
			// LOD name
			blob.write((u32)lod.name.size());
			blob.write(lod.name.c_str(), lod.name.size());
			// number of meshes in this LOD
			blob.write((u32)lod.meshes.size());

			for (auto& mesh : lod.meshes)
			{
				pack_mesh_data(mesh, blob);
			}
		}
		assert(scene_size == blob.offset());
	}
}






