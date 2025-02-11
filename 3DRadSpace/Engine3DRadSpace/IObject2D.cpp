#include "IObject2D.hpp"

using namespace Engine3DRadSpace;
using namespace Engine3DRadSpace::Math;

Engine3DRadSpace::IObject2D::IObject2D(const std::string &name, const std::string &tag, bool enabled, bool visible, const Math::Vector2 &pos, const Math::Vector2 &scale, 
	float rotation, const Math::Vector2 &pivot, float depth) :
	IObject(name, tag, false, false),
	Position(pos),
	Rotation(rotation),
	RotationCenter(pivot),
	Scale(scale),
	Depth(depth)
{
}

Engine3DRadSpace::IObject2D::IObject2D(GraphicsDevice *device, const std::string &name, const std::string &tag, bool enabled, bool visible, const Vector2 &pos,
	const Vector2& scale, float rotation, const Math::Vector2 &pivot, float depth): 
	IObject(device, name, tag, false, false),
	Position(pos),
	Rotation(rotation),
	RotationCenter(pivot),
	Scale(scale),
	Depth(depth)
{
}
