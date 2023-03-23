#pragma once
#include "ShaderPipeline.hpp"
#include "Matrix.hpp"

namespace Engine3DRadSpace::Graphics::Shaders
{
	class BlankShader : public ShaderPipeline
	{
		class VertexShader : public IShader
		{
		public:
			explicit VertexShader(GraphicsDevice* device);

			std::span<InputLayoutElement> InputLayout() override;
		};

		class PixelShader : public IShader
		{
		public:
			explicit PixelShader(GraphicsDevice* device);

			std::span<InputLayoutElement> InputLayout() override;
		};

	public:
		explicit BlankShader(GraphicsDevice* device);

		/// <summary>
		/// Sets the linear transformation used to render the primitives.
		/// </summary>
		/// <param name="matrix">A MVP (model-view-projection) linear transformation is required.</param>
		void SetTransformation(Math::Matrix &matrix);
	};
}