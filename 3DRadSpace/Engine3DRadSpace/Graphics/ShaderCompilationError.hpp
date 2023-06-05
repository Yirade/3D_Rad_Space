#pragma once
#include "../Logging/Exception.hpp"

namespace Engine3DRadSpace::Graphics
{
	class ShaderCompilationError : public Engine3DRadSpace::Logging::Exception
	{
	public:
		ShaderCompilationError(const std::string &err) :
			Exception(err)
		{
		}
	};
}

