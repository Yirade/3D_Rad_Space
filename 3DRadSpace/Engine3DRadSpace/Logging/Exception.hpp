#pragma once
#include <string>
#include "../Libs.hpp"

namespace Engine3DRadSpace::Logging
{
	/// <summary>
	/// Replaces std::exception.
	/// </summary>
	class DLLEXPORT Exception
	{
	protected:
		std::string _str;
	public:
		explicit Exception(const std::string &err);
		virtual std::string What() const;
		virtual ~Exception() = default;
	};
}

