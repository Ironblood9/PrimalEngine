#include "Entity.h"


namespace primal::game_entity {

	namespace {
	
		utl::vector<id::generation_type>              generations;
		utl::deque<entity_id>                         free_ids;


	}// anonymous namespace
	entity
	create_game_entity(const entity_info& info){
	
		assert(info.transform);// all game entities must have a transform componant
		if (!info.transform) return entity{};

		entity_id id;

		if (free_ids.size() > id::min_deleted_elemenst) 
		{
			id = free_ids.front();
			assert(!is_alive(entity{ id }));
			free_ids.pop_front();
			id = entity_id{ id::new_generation(id) };
			++generations[id::index(id)];
		}
		else
		{
			id = entity_id{ (id::id_type)generations.size() };
			generations.push_back(0);
		}
		const entity new_entity{ id };
		const id::id_type index{ id::index(id) };
		return new_entity;
	}
	void remove_game_entity(entity id){}

	bool is_alive(entity id){ }
}












