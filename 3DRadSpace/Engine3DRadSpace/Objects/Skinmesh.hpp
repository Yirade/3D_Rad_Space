#pragma once
#include "../IObject3D.hpp"
#include "../Graphics/Model3D.hpp"
#include "../Reflection/Reflection.hpp"

namespace Engine3DRadSpace::Objects
{
	class DLLEXPORT Skinmesh : public IObject3D
	{
		Engine3DRadSpace::Graphics::Model3D *_model;
		std::unique_ptr<std::string> _path;
	public:
		Skinmesh();

		Skinmesh(const std::string &name, bool visible, const std::string &tag, Reflection::RefModel3D model, const Math::Vector3 &pos,
			const Math::Quaternion &rot, const Math::Vector3 &pivot, const Math::Vector3 &scale);

		Skinmesh(const std::string &name, bool visible, const std::string &tag, const std::filesystem::path &path,const Math::Vector3 &pos = Math::Vector3::Zero(),
			const Math::Quaternion &rot = Math::Quaternion(), const Math::Vector3 &pivot = Math::Vector3::Zero(), const Math::Vector3 &scale = Math::Vector3::One());

		Reflection::RefModel3D Model;

		Engine3DRadSpace::Graphics::Model3D *GetModel();

		virtual void Initialize() override;
		virtual void Update(Input::Keyboard &keyboard, Input::Mouse &mouse, double dt) override;
		virtual void Load(Content::ContentManager *content) override;
		virtual void EditorInitialize() override;
		virtual void EditorLoad(Content::ContentManager *content) override;
		virtual Reflection::UUID GetUUID() override;
		virtual void Draw(Engine3DRadSpace::Math::Matrix &view, Engine3DRadSpace::Math::Matrix &projection, double dt) override;
		virtual void EditorDraw(const Engine3DRadSpace::Math::Matrix &view, const Engine3DRadSpace::Math::Matrix &projection, double dt, bool selected) override;
		virtual std::optional<float> Intersects(const Math::Ray &r) override;

		virtual ~Skinmesh() = default;
	};
}

REFL_DEF(Skinmesh)