#include "Script.h"
#include "Entity.h"

namespace primal::script{
	namespace {

		utl::vector<detail::script_ptr>             entity_scripts;
		utl::vector<id::generation_type>            generations;
		utl::vector<script_id>                      free_ids;
	}// anonymous 

	component
	create(init_info info, game_entity::entity entity)
	{
		return component{};
	}

	void remove(component c)
	{

	}
}







