#include "GraphicsDevice.hpp"
#include "Logging/Error.hpp"
//include shader types

#include <cassert>
#include "Graphics/VertexBuffer.hpp"
#include "Graphics/IndexBuffer.hpp"

#ifdef  _DX11
#pragma comment(lib,"d3d11.lib")
#endif //  _DX11

using namespace Engine3DRadSpace;
using namespace Engine3DRadSpace::Graphics;
using namespace Engine3DRadSpace::Logging;

Engine3DRadSpace::GraphicsDevice::GraphicsDevice(void* nativeWindowHandle, unsigned width, unsigned height) :
	EnableVSync(true),
	resolution(width, height)
{
#ifdef _DX11
	DXGI_SWAP_CHAIN_DESC swapChainDesc{};
	swapChainDesc.BufferCount = 2;
	swapChainDesc.BufferDesc.Width = width;
	swapChainDesc.BufferDesc.Height = height;
	swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R16G16B16A16_FLOAT;
	swapChainDesc.BufferDesc.RefreshRate = { 1,60 }; // 1/60
	swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_STRETCHED;
	swapChainDesc.Windowed = true;
	swapChainDesc.SampleDesc = { 1, 0 }; //count, quality
	swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapChainDesc.OutputWindow = static_cast<HWND>(nativeWindowHandle);
	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;

#if _DEBUG
	UINT flags = D3D11_CREATE_DEVICE_DEBUG;
#else
	UINT flags = 0;
#endif

	HRESULT r = D3D11CreateDeviceAndSwapChain(
		nullptr,
		D3D_DRIVER_TYPE_HARDWARE,
		nullptr,
		flags,
		nullptr,
		0,
		D3D11_SDK_VERSION,
		&swapChainDesc,
		&this->swapChain,
		&this->device,
		nullptr,
		&this->context
	);
	RaiseFatalErrorIfFailed(r, "D3D11CreateDeviceAndSwapChain failed!");

	r = swapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), static_cast<void**>(&screenTexture));
	RaiseFatalErrorIfFailed(r, "Failed to get the back buffer texture!");

	r = device->CreateRenderTargetView(screenTexture.Get(), nullptr, &mainRenderTarget);
	RaiseFatalErrorIfFailed(r, "Failed to create the main render target!");

	createDepthStencil();
	context->OMSetDepthStencilState(this->depthState.Get(), 1);

	D3D11_BLEND_DESC blendDesc;
	blendDesc.AlphaToCoverageEnable = true;
	blendDesc.IndependentBlendEnable = true;

	blendDesc.RenderTarget[0].BlendEnable = true;
	blendDesc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
	blendDesc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
	blendDesc.RenderTarget[0].DestBlend = D3D11_BLEND_ZERO;
	blendDesc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ZERO;
	blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
	blendDesc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
	blendDesc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;

	for(int i = 1; i < 8; i++)
		blendDesc.RenderTarget[i] = blendDesc.RenderTarget[0];

	r = device->CreateBlendState(&blendDesc, blendState.GetAddressOf());
	RaiseFatalErrorIfFailed(r, "Failed to create a default blend state!", "");

	r = CoInitializeEx(nullptr, COINIT::COINIT_APARTMENTTHREADED);
	RaiseFatalErrorIfFailed(r, "Failed to initialize COM!");
#endif
}

void Engine3DRadSpace::GraphicsDevice::createDepthStencil()
{
#ifdef WIN32
	createDepthTexture();

	D3D11_DEPTH_STENCIL_DESC dsDesc;

	dsDesc.DepthEnable = true;
	dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
	dsDesc.DepthFunc = D3D11_COMPARISON_LESS;

	dsDesc.StencilEnable = true;
	dsDesc.StencilReadMask = 0xFF;
	dsDesc.StencilWriteMask = 0xFF;

	dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
	dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_INCR;
	dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
	dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

	dsDesc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
	dsDesc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_DECR;
	dsDesc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
	dsDesc.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

	HRESULT r = device->CreateDepthStencilState(&dsDesc, &depthState);
	RaiseFatalErrorIfFailed(r, "Failed to create a depth stencil state with default settings");

	createDepthView();
#endif
}

void Engine3DRadSpace::GraphicsDevice::createDepthTexture()
{
#ifdef WIN32
	D3D11_TEXTURE2D_DESC desc{};
	desc.Format = DXGI_FORMAT_R32_TYPELESS;
	desc.ArraySize = 1;
	desc.Width = resolution.X;
	desc.Height = resolution.Y;
	desc.MipLevels = 1;
	desc.SampleDesc.Count = 1;
	desc.SampleDesc.Quality = 0;
	desc.BindFlags = D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_SHADER_RESOURCE;

	HRESULT r = device->CreateTexture2D(&desc, nullptr, &depthTexture);
	RaiseFatalErrorIfFailed(r, "Failed to create the depth stencil texture!");
#endif
}

void Engine3DRadSpace::GraphicsDevice::createDepthView()
{
#ifdef WIN32
	D3D11_DEPTH_STENCIL_VIEW_DESC viewDesc{};
	viewDesc.Format = DXGI_FORMAT_D32_FLOAT;
	viewDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;

	HRESULT r = device->CreateDepthStencilView(this->depthTexture.Get(), &viewDesc, &this->depthView); //the & operator release the previous stencil view object
	RaiseFatalErrorIfFailed(r, "Failed to create a depth stencil view!");
#endif
}

void Engine3DRadSpace::GraphicsDevice::Clear(const Color& clearColor)
{
#ifdef _DX11
	context->OMSetRenderTargets(1, mainRenderTarget.GetAddressOf(), this->depthView.Get());

	float color[4] = { clearColor.R,clearColor.G,clearColor.B,clearColor.A };
	context->ClearRenderTargetView(mainRenderTarget.Get(), color);
	context->ClearDepthStencilView(depthView.Get(), D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1.0f, 0xFF);

	float blendFactor[4] = {1,1,1,1};
	context->OMSetBlendState(blendState.Get(), blendFactor, 0xFF);
#endif
}

void Engine3DRadSpace::GraphicsDevice::SetViewport(const Viewport& viewport)
{
#ifdef _DX11
	D3D11_VIEWPORT vp{};
	vp.TopLeftX = viewport.ScreenRectangle.X;
	vp.TopLeftY = viewport.ScreenRectangle.Y;
	vp.Width = viewport.ScreenRectangle.Width;
	vp.Height = viewport.ScreenRectangle.Height;

	vp.MinDepth = viewport.MinDepth;
	vp.MaxDepth = viewport.MaxDepth;

	context->RSSetViewports(1, &vp);
#endif
}

void Engine3DRadSpace::GraphicsDevice::SetViewports(std::span<Viewport> viewports)
{
#ifdef _DX11
	context->RSSetViewports(static_cast<UINT>(viewports.size()), reinterpret_cast<D3D11_VIEWPORT*>(&viewports[0]));
#endif // _DX11
}

void Engine3DRadSpace::GraphicsDevice::SetNewDepthStencil(const Graphics::DepthStencilState& state)
{
#ifdef _DX11
	//create a description matching the state parameter.
	D3D11_DEPTH_STENCIL_DESC dsDesc;

	dsDesc.DepthEnable = state.EnableDepthCheck;
	dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK(state.WriteMask); //REFACTORING-NOTE: don't use initialization, may prefer a switch-case.
	dsDesc.DepthFunc = D3D11_COMPARISON_FUNC(state.Function);

	dsDesc.StencilEnable = state.EnableStencilCheck;
	dsDesc.StencilReadMask = state.ReadMask;
	dsDesc.StencilWriteMask = state.WriteMask;

	dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP(state.FrontFace.StencilFail);
	dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP(state.FrontFace.DepthFail);
	dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP(state.FrontFace.PassOp);
	dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_FUNC(state.FrontFace.Function);

	dsDesc.BackFace.StencilFailOp = D3D11_STENCIL_OP(state.BackFace.StencilFail);
	dsDesc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP(state.BackFace.DepthFail);
	dsDesc.BackFace.StencilPassOp = D3D11_STENCIL_OP(state.BackFace.PassOp);
	dsDesc.BackFace.StencilFunc = D3D11_COMPARISON_FUNC(state.BackFace.Function);

	HRESULT r = device->CreateDepthStencilState(&dsDesc, &depthState);
	RaiseFatalErrorIfFailed(r, "Failed to create a depth stencil state");

	//recreate depth view resource.
	createDepthView();
#endif
}

void Engine3DRadSpace::GraphicsDevice::DrawVertexBuffer(Engine3DRadSpace::Graphics::VertexBuffer* vertexBuffer, unsigned startSlot)
{
#ifdef _DX11
	UINT strides = UINT(vertexBuffer->structSize);
	UINT offsets = 0;
	context->IASetVertexBuffers(startSlot, 1, vertexBuffer->buffer.GetAddressOf(), &strides, &offsets);
	context->IASetIndexBuffer(nullptr, DXGI_FORMAT_UNKNOWN, 0);
	context->Draw(UINT(vertexBuffer->numVerts), UINT(startSlot));
#endif
}
void Engine3DRadSpace::GraphicsDevice::DrawVertexBufferWithindices(Engine3DRadSpace::Graphics::VertexBuffer* vertexBuffer, Engine3DRadSpace::Graphics::IndexBuffer* indexBuffer)
{
#ifdef _DX11
	UINT strides = UINT(vertexBuffer->structSize);
	UINT offsets = 0;
	context->IASetIndexBuffer(indexBuffer->indexBuffer.Get(), DXGI_FORMAT_R32_UINT, 0);
	context->IASetVertexBuffers(0, 1, vertexBuffer->buffer.GetAddressOf(), &strides, &offsets);
	context->Draw(UINT(vertexBuffer->numVerts), 0u);
#endif
}
void Engine3DRadSpace::GraphicsDevice::Present()
{
#ifdef _DX11
	HRESULT r = swapChain->Present(EnableVSync ? 1 : 0, 0);
	if (SUCCEEDED(r)) return; //if Present call succeded, skip error reporting.

	if (r == DXGI_ERROR_DEVICE_RESET) RaiseFatalError(Error(r, "Graphics device reset"));
	if (r == DXGI_ERROR_DEVICE_REMOVED)
	{
		HRESULT reason = device->GetDeviceRemovedReason();
		switch (reason)
		{
			case DXGI_ERROR_DEVICE_HUNG:
				RaiseFatalError(Error(reason, "Graphics device hung"));
				break;
			case DXGI_ERROR_DEVICE_REMOVED:
				RaiseFatalError(Error(reason, "Graphics device removed"));
				break;
			case DXGI_ERROR_DEVICE_RESET:
				RaiseFatalError(Error(reason, "Graphics device reset"));
				break;
			case DXGI_ERROR_DRIVER_INTERNAL_ERROR:
				RaiseFatalError(Error(reason, "Internal driver error!"));
				break;
			case DXGI_ERROR_INVALID_CALL:
				RaiseFatalError(Error(reason, "Invalid call"));
				break;
			default: break;
		}
	}
	
	RaiseFatalErrorIfFailed(r, "Unknown error when presenting a frame", this);
#endif // _DX11
}

void Engine3DRadSpace::GraphicsDevice::SetShader(Engine3DRadSpace::Graphics::IShader *shader)
{
	shader->SetShader();
}

void Engine3DRadSpace::GraphicsDevice::SetTopology(Graphics::VertexTopology topology)
{
#ifdef _DX11
	context->IASetPrimitiveTopology(static_cast<D3D11_PRIMITIVE_TOPOLOGY>(topology));
#endif
}

void Engine3DRadSpace::GraphicsDevice::DrawAuto()
{
#ifdef _DX11
	this->context->DrawAuto();
#endif
}

void Engine3DRadSpace::GraphicsDevice::SetRasterizerState(const RasterizerState *state)
{
#ifdef _DX11
	context->RSSetState(state->rasterizerState.Get());
#endif
}

Engine3DRadSpace::Math::Point Engine3DRadSpace::GraphicsDevice::Resolution()
{
	return this->resolution;
}

Engine3DRadSpace::GraphicsDevice::~GraphicsDevice()
{
	CoUninitialize();
}
