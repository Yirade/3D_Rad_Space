#include "BasicTextured_NBT.hpp"
using namespace Engine3DRadSpace::Math;
using namespace Engine3DRadSpace::Graphics;
using namespace Engine3DRadSpace::Graphics::Shaders;

BasicTextured_NBT::VertexShader::VertexShader(GraphicsDevice* device) :
	IVertexShader(device, elements, std::filesystem::path("Data\\Shaders\\PositionNormalTangentUV.hlsl"), "VS_Main")
{
}

std::span<InputLayoutElement> BasicTextured_NBT::VertexShader::InputLayout()
{
	return elements;
}

BasicTextured_NBT::PixelShader::PixelShader(GraphicsDevice* device) :
	IPixelShader(device, std::filesystem::path("Data\\Shaders\\PositionNormalTangentUV.hlsl"), "PS_Main")
{
}


BasicTextured_NBT::BasicTextured_NBT(GraphicsDevice* device) : 
	ShaderPipeline(device, new BasicTextured_NBT::VertexShader(device), new BasicTextured_NBT::PixelShader(device))
{
}

void BasicTextured_NBT::SetTransform(const Matrix& m)
{
	_vertex->SetData(0, &m, sizeof(Matrix));
}

void BasicTextured_NBT::SetTexture(Engine3DRadSpace::Graphics::Texture2D* texture)
{
	_vertex->SetTexture(0, texture);
}

void BasicTextured_NBT::SetSampler(Engine3DRadSpace::Graphics::SamplerState* state)
{
	_pixel->SetSampler(0, state);
}
