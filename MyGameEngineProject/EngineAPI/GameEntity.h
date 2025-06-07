#pragma once

#include "..\Components\ComponentsCommon.h"

namespace primal::game_entity 
{
	DEFINE_TYPE_ID(entity_id);
	class entity{
	public:
	
		constexpr explicit entity(entity_id id) : _id { id }{}
		constexpr explicit entity() : _id { id::invalid_id } {}
	
			 
	private:
		entity_id _id;
	};



}






























