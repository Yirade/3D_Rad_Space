#include "IPixelShader.hpp"
#include "ShaderCompilationError.hpp"
#include "../Logging/ResourceLoadingError.hpp"

using namespace Engine3DRadSpace;
using namespace Engine3DRadSpace::Graphics;
using namespace Engine3DRadSpace::Logging;

void IPixelShader::_createShader()
{
#ifdef _DX11
	HRESULT r = _device->_device->CreatePixelShader(
		_shaderBlob->GetBufferPointer(),
		_shaderBlob->GetBufferSize(),
		nullptr,
		_shader.GetAddressOf());

	if(FAILED(r)) throw ShaderCompilationError("Failed to create a pixel shader!");

#ifdef _DEBUG
	const char shaderName[] = "IPixelShader::_shader";
	_shader->SetPrivateData(WKPDID_D3DDebugObjectName, sizeof(shaderName), shaderName);
#endif
#endif
}

const char *Engine3DRadSpace::Graphics::IPixelShader::_determineTarget()
{
	switch(_featureLevel)
	{
		case ShaderFeatureLevel::DX_V4:
			return "ps_4_0";
		case ShaderFeatureLevel::DX_V4_1:
			return "ps_4_1";
		case ShaderFeatureLevel::DX_V5:
			return "ps_5_0";
		case ShaderFeatureLevel::DX_V6:
			return "ps_6_0";
		default:
			return nullptr;
	}
}

Engine3DRadSpace::Graphics::IPixelShader::IPixelShader(GraphicsDevice *device, const char *shaderSource, const char *entryFunction, ShaderFeatureLevel fl):
	IShader(device, shaderSource, entryFunction, fl)
{
	_compileShader(shaderSource, _determineTarget());
	_createShader();
}

Engine3DRadSpace::Graphics::IPixelShader::IPixelShader(GraphicsDevice *device, const std::filesystem::path &path, const char *entryFunction, ShaderFeatureLevel fl):
	IShader(device, path, entryFunction, fl)
{
	_compileShaderFromFile(path.string().c_str(), _determineTarget());
	_createShader();
}

void Engine3DRadSpace::Graphics::IPixelShader::SetTexture(unsigned index, Texture2D *texture)
{
#ifdef _DX11
	_device->_context->PSSetShaderResources(index, 1, texture->_resourceView.GetAddressOf());
#endif // _DX11
}

void Engine3DRadSpace::Graphics::IPixelShader::SetSampler(unsigned index, SamplerState *samplerState)
{
#ifdef _DX11
	_device->_context->PSSetSamplers(0, 1, samplerState->_samplerState.GetAddressOf());
#endif
}

void Engine3DRadSpace::Graphics::IPixelShader::SetShader()
{
#ifdef _DX11
	unsigned i;
	auto validConstantBuffers = this->_validConstantBuffers(i);
	_device->_context->PSSetConstantBuffers(0, i, validConstantBuffers.data());

	_device->_context->PSSetShader(_shader.Get(), nullptr, 0);
#endif
}
