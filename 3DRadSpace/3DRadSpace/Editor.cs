﻿using _3DRadSpaceDll;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace _3DRadSpace
{
	public partial class Editor : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Form GameWindow;
		Camera Editor_View;
		Model Axis;
		static DiscordRichPresence discordRichPresence;
		Matrix View, Projection;
		public string OpenedFile = null;
		static bool[] Settings;
		float CameraRotationSpeed = 0.001f;

		Vector2 CameraRotationCoords = new Vector2(-2.105f, 2.946f);
		float CameraSpeed = 0.1f;

		//SkyColor specific
		Color ClearColor = Color.Black;

		//Fog specifig
		Vector3 FogColor;
		float FogStart, FogEnd;
		bool FogEnabled = false;

		SpriteFont D_Font;

		Texture2D I_MovX, I_MovY, I_MovZ, I_RotX, I_RotY, I_RotZ, I_ScalX, I_ScalY, I_ScalZ;
		Rectangle R_MovX, R_MovY, R_MovZ, R_RotX, R_RotY, R_RotZ, R_ScalX, R_ScalY, R_ScalZ;
		Point M_MovX, M_MovY, M_MovZ, M_RotX, M_RotY, M_RotZ, M_ScalX, M_ScalY, M_ScalZ;
		int sX, sY;

		public Editor()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			GameWindow = (Form)Control.FromHandle(Window.Handle);
			InitializeComponent();
			toolStripStatusLabel1.Text = "Status: Ready";
			GameWindow_SizeChanged(GameWindow, null);
			discordRichPresence = new DiscordRichPresence();
			Editor_View = new Camera(null, true, new Vector3(5, 10, 5), new Vector3(0, 0, 0), Vector3.Up, 75, 0.01f, 500f);
			Main.GameObjects = new List<GameObject>();
			Settings = Settings_Load();
		}
		protected override void Initialize()
		{
			Window.AllowUserResizing = true;
			Window.Title = "3DRadSpace - Editor v0.0.7 alpha";
			GameWindow.WindowState = FormWindowState.Maximized;
			IsMouseVisible = true;
			if(Settings[0])
			{
				checkforUpdatesEvent(null, null);
			}
			if(OpenedFile != null)
			{
				_3DRadSpaceDll.Main.GameObjects = Project.Open(OpenedFile);
				LoadAllObjects();
				UpdateObjects();
			}
			Editor_View.CameraTarget = Editor_View.Position + Vector3.Transform(Vector3.UnitZ + Vector3.Up, Matrix.CreateFromYawPitchRoll(CameraRotationCoords.X, 0, CameraRotationCoords.Y));

			sX = graphics.PreferredBackBufferWidth; //these will be needed when I will make the coordinates compatible with multiple screen resolutions
			sY = graphics.PreferredBackBufferHeight;
			R_MovX = new Rectangle(170, 35, 80, 80);
			R_MovY = new Rectangle(250, 35, 80, 80);
			R_MovZ = new Rectangle(330, 35, 80, 80);
			R_RotX = new Rectangle(410, 35, 80, 80);
			R_RotY = new Rectangle(490, 35, 80, 80);
			R_RotZ = new Rectangle(570, 35, 80, 80);
			R_ScalX = new Rectangle(650, 35, 80, 80);
			R_ScalY = new Rectangle(730, 35, 80, 80);
			R_ScalZ = new Rectangle(810, 35, 80, 80);

			M_MovX = InterpolateRectangle(R_MovX, 0.5f);
			M_MovY = InterpolateRectangle(R_MovY, 0.5f);
			M_MovZ = InterpolateRectangle(R_MovZ, 0.5f);
			M_RotX = InterpolateRectangle(R_RotX, 0.5f);
			M_RotY = InterpolateRectangle(R_RotY, 0.5f);
			M_RotZ = InterpolateRectangle(R_RotZ, 0.5f);
			M_ScalX = InterpolateRectangle(R_ScalX, 0.5f);
			M_ScalY = InterpolateRectangle(R_ScalY, 0.5f);
			M_ScalZ = InterpolateRectangle(R_ScalZ, 0.5f);

			base.Initialize();
		}
		Point InterpolateRectangle(Rectangle r, float lerp)
		{
			return new Point((int)MathHelper.Lerp(r.X, r.Width + r.X, lerp), (int)MathHelper.Lerp(r.Y, r.Y + r.Height, lerp));
		}
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Camera.model = Content.Load<Model>("Camera/camera");
			Camera.ScreenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
			Axis = Content.Load<Model>("Axis");
			D_Font = Content.Load<SpriteFont>("Font");
			EventOnLocation.LoadModels(Content);
			SoundSource.ModelMarker = Content.Load<Model>("SoundEffect_Model");
			I_MovX = Content.Load<Texture2D>("Editor//MovmentX");
			I_MovY = Content.Load<Texture2D>("Editor//MovmentY");
			I_MovZ = Content.Load<Texture2D>("Editor//MovmentZ");
			I_RotX = Content.Load<Texture2D>("Editor//RotationX");
			I_RotY = Content.Load<Texture2D>("Editor//Rotationy");
			I_RotZ = Content.Load<Texture2D>("Editor//RotationZ");
			I_ScalX = Content.Load<Texture2D>("Editor//ScaleX");
			I_ScalY = Content.Load<Texture2D>("Editor//ScaleY");
			I_ScalZ = Content.Load<Texture2D>("Editor//ScaleZ");
		}

		protected override void UnloadContent()
		{

		}
		Vector3 _3dcursor_loc = Vector3.Zero;
		int selected_object_index = -1;

		MouseState oldState;
		bool _viewportfocus = true;

		bool _allowmov = false;
		bool _allowrot = false;
		bool _allowscal = false;

		protected override void Update(GameTime gameTime)
		{
			if(Form.ActiveForm == GameWindow && _viewportfocus)
			{
				bool mv = true;
				KeyboardState keyboard = Keyboard.GetState();
				MouseState mouse = Mouse.GetState();
				//keyboard shortcuts
				if(GetKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.N)) newProject(null, null);
				if(GetKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.O)) openProject(null, null);
				if(GetKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.S)) saveProject(null, null);
				if(GetAltKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.S)) saveProjectAs(null, null);
				if(GetKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.P)) playProject(null, null);
				if(GetKeyShortcut(keyboard, Microsoft.Xna.Framework.Input.Keys.A)) addObject(null, null);

				//camera movement

				if(mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
				{
					if(mouse.X >= 150 && mouse.X <= graphics.PreferredBackBufferWidth &&
					   mouse.Y >= 105 && mouse.Y <= graphics.PreferredBackBufferHeight - 25)
					{
						Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
						mv = false;
						CameraRotationCoords += new Vector2((mouse.X - (graphics.PreferredBackBufferWidth / 2)) * (-CameraRotationSpeed), (mouse.Y - (graphics.PreferredBackBufferHeight / 2)) * CameraRotationSpeed);
						if(CameraRotationCoords.Y > (MathHelper.Pi - 0.1f)) CameraRotationCoords.Y = (MathHelper.Pi - 0.1f);
						if(CameraRotationCoords.Y < 0) CameraRotationCoords.Y = 0.1f;
					}
					else
						mv = true;
				}
				Vector3 UnitV = Vector3.Transform(Vector3.UnitZ + Vector3.Up, Matrix.CreateFromYawPitchRoll(CameraRotationCoords.X, 0, CameraRotationCoords.Y));
				Editor_View.Position = _3dcursor_loc + (UnitV * (mouse.ScrollWheelValue - 500f) * 0.01f);
				Editor_View.CameraTarget = _3dcursor_loc;

				if(mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
					oldState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
				{
					float d = float.MinValue;
					Ray finder = GetMouseRay(new Vector2(mouse.X, mouse.Y), GraphicsDevice.Viewport, View, Projection);
					for(int i = 0; i < Main.GameObjects.Count; i++)
					{
						GameObject o = Main.GameObjects[i];
						if(o is Skinmesh sk)
						{
							for(int j = 0; j < sk.Model.Meshes.Count; j++)
							{
								BoundingSphere sph = sk.Model.Meshes[j].BoundingSphere;
								sph.Center += sk.Position;
								if(RayI(finder, sph, out float? cdst))
								{
									Vector3? val = RayMeshCollision(finder, sk.Model, sk.TranslationMatrix);
									if(val == null) continue;
									if(_3dcursor_loc != null)
									{
										if(d < cdst)
										{
											d = (float)cdst;
											_3dcursor_loc = val.Value;
											selected_object_index = i;
											AllowTr(3);
										}
									}
								}
							}
						}
						if(o is Camera c)
						{
							if(RayI(finder, new BoundingSphere(c.Position, 2), out float? cdst))
							{
								if(d < cdst)
								{
									d = (float)cdst;
									_3dcursor_loc = c.Position;
									selected_object_index = i;
									AllowTr(2);
								}
							}
						}
						if(o is EventOnLocation eol)
						{
							switch(eol.BoundingType)
							{
								case BoundingObject.Box:
									if(RayI(finder, eol.BoundingBox, out float? cdst))
									{
										if(d < cdst)
										{
											d = (float)cdst;
											_3dcursor_loc = finder.Position + finder.Direction * d;
											selected_object_index = i;
											AllowTr(4);
										}
									}
									break;
								case BoundingObject.Sphere:
									if(RayI(finder, eol.BoundingSphere, out float? cdsts))
									{
										if(d < cdsts)
										{
											d = (float)cdsts;
											_3dcursor_loc = finder.Position + finder.Direction * d;
											selected_object_index = i;
											AllowTr(4);
										}
									}
									break;
								default: break;
							}
						}
						if(o is SoundSource sound)
						{
							if(RayI(finder, new BoundingSphere(sound.Position, 2), out float? cdst))
							{
								if(d < cdst)
								{
									d = (float)cdst;
									_3dcursor_loc = sound.Position;
									selected_object_index = i;
									AllowTr(1);
								}
							}
						}
					}
				}
				oldState = mouse;
				if(selected_object_index != -1)
				{
					//edit/remove 3d selected object keyboard shortcuts
					if(keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
						EditObj(selected_object_index);
					if(keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
						DeleteObject(selected_object_index);
					//move/rotate/scale objects
					if(_allowmov)
					{
						if(R_MovX.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_MovX.X, M_MovX.Y);
							Point offset = mouse.Position - M_MovX;
							mv = false;
							Vector3 tr = Vector3.UnitX * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 1);
							_3dcursor_loc += tr; 
						}
						else if(R_MovY.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_MovY.X, M_MovY.Y);
							Point offset = mouse.Position - M_MovY;
							mv = false;
							Vector3 tr = Vector3.UnitY * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 1);
							_3dcursor_loc += tr;
						}
						else if(R_MovZ.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_MovZ.X, M_MovZ.Y);
							Point offset = mouse.Position - M_MovZ;
							mv = false;
							Vector3 tr = Vector3.UnitZ * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 1);
							_3dcursor_loc += tr;
						}
						else if(mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released) mv = true;
					}
					if(_allowrot)
					{
						if(R_RotX.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_RotX.X, M_RotX.Y);
							Point offset = mouse.Position - M_RotX;
							mv = false;
							Vector3 tr = Vector3.UnitX * (offset.X + offset.Y) * 0.01f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 2);
						}
						else if(R_RotY.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_RotY.X, M_RotY.Y);
							Point offset = mouse.Position - M_RotY;
							mv = false;
							Vector3 tr = Vector3.UnitY * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 2);
						}
						else if(R_RotZ.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_RotZ.X, M_RotZ.Y);
							Point offset = mouse.Position - M_RotZ;
							mv = false;
							Vector3 tr = Vector3.UnitZ * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 2);
						}
						else if(mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
							mv = true;
					}
					if(_allowscal)
                    {
						if(R_ScalX.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_ScalX.X, M_ScalX.Y);
							Point offset = mouse.Position - M_ScalX;
							mv = false;
							Vector3 tr = Vector3.UnitX * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 3);
						}
						if(R_ScalY.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_ScalY.X, M_ScalY.Y);
							Point offset = mouse.Position - M_ScalY;
							mv = false;
							Vector3 tr = Vector3.UnitY * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 3);
						}
						if(R_ScalZ.Contains(new Point(mouse.X, mouse.Y)) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
						{
							Mouse.SetPosition(M_ScalZ.X, M_ScalZ.Y);
							Point offset = mouse.Position - M_ScalZ;
							mv = false;
							Vector3 tr = Vector3.UnitZ * (offset.X + offset.Y) * 0.1f;
							ApplyTransformation(Main.GameObjects[selected_object_index], tr, 3);
						}
						else if(mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
							mv = true;
					}
				}
				IsMouseVisible = mv;
			}
			base.Update(gameTime);
		}
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(ClearColor);
			GraphicsDevice.BlendState = BlendState.AlphaBlend;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
			Editor_View.Draw(out View, out Projection);

			//Draws the axis: Rotating it 3/2*pi rad because the model is wrong lol
			Main.DrawModel(Axis, Matrix.CreateRotationY(MathHelper.Pi * 1.5f) * Matrix.CreateTranslation(_3dcursor_loc), View, Projection, FogEnabled, FogColor, FogStart, FogEnd);

			for(int i = 0; i < Main.GameObjects.Count; i++)
			{
				object gameObject = Main.GameObjects[i];
				if(gameObject is FPVCamera fc) fc.EditorDraw(null, View, Projection);
				else if(gameObject is Camera c) c.EditorDraw(null, View, Projection);
				if(gameObject is SkyColor s) ClearColor = s.Color;
				if(gameObject is Fog f)
				{
					FogEnabled = f.Enabled;
					FogColor = f.FogColor;
					FogStart = f.FogStart;
					FogEnd = f.FogEnd;
				}
				if(gameObject is Skinmesh sk)
				{
					if(FogEnabled)
					{
						sk.FogExists = FogEnabled;
						sk.FogColor = FogColor;
						sk.FogEnd = FogEnd;
						sk.FogStart = FogStart;
					}
					else
					{
						sk.FogExists = false;
						sk.FogEnabled = false;
					}
					sk.EditorDraw(null, View, Projection);
					//DrawModelRed(sk.Model);
				}
				if(gameObject is Skybox sb)
				{
					GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
					sb.EditorDraw(Editor_View.Position, View, Projection);
					GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
				}
				if(gameObject is EventOnLocation eol) eol.EditorDraw(spriteBatch, View, Projection);
				if(gameObject is SoundSource ss) ss.EditorDraw(spriteBatch, View, Projection);
			}
			spriteBatch.Begin();
			spriteBatch.DrawString(D_Font, "CamRot: " + CameraRotationCoords + " CamPos " + Editor_View.Position, new Vector2(170, graphics.PreferredBackBufferHeight - 50), Color.White);
			if(_allowmov)
			{
				spriteBatch.Draw(I_MovX, R_MovX, Color.White);
				spriteBatch.Draw(I_MovY, R_MovY, Color.White);
				spriteBatch.Draw(I_MovZ, R_MovZ, Color.White);
			}
			if(_allowrot)
			{
				spriteBatch.Draw(I_RotX, R_RotX, Color.White);
				spriteBatch.Draw(I_RotY, R_RotY, Color.White);
				spriteBatch.Draw(I_RotZ, R_RotZ, Color.White);
			}
			if(_allowscal)
			{
				spriteBatch.Draw(I_ScalX, R_ScalX, Color.White);
				spriteBatch.Draw(I_ScalY, R_ScalY, Color.White);
				spriteBatch.Draw(I_ScalZ, R_ScalZ, Color.White);
			}
			for(int i = 0; i < Main.GameObjects.Count; i++)
			{
				object gameObject = Main.GameObjects[i];
				if(gameObject is Sprite sp) sp.EditorDraw(spriteBatch, null, null);
				if(gameObject is TextPrint tp) tp.EditorDraw(spriteBatch, null, null);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}
		// maybe will find use in the future.
		//bool _first_use; <- currently disabled.
		private bool[] Settings_Load()
		{
			if(!Directory.Exists(Environment.ExpandEnvironmentVariables("%appdata%\\3DRadSpace")))
			{
				//_first_use = true;
				Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%appdata%\\3DRadSpace"));
			}
			string appd = Environment.ExpandEnvironmentVariables("%AppData%\\3DRadSpace");
			if(!File.Exists(appd + "\\Config.cfg"))
			{
				File.WriteAllText(appd + "\\Config.cfg", "1 1 1 1");
				return new[] { true, true, true };
			}
			string[] split = File.ReadAllText(appd + "\\Config.cfg").Split(' ');
			if(split.Length != 5)
			{
				File.WriteAllText(appd + "\\Config.cfg", "1 1 1 1 1");
				return new[] { true, true, true };
			}
			bool[] result = { Convert.ToBoolean(split[0]), Convert.ToBoolean(split[1]), Convert.ToBoolean(split[2]) };
			CameraRotationSpeed = 0.001f * Convert.ToInt32(split[3], Main.CultureInfo);
			CameraSpeed = 0.1f * Convert.ToInt32(split[4], Main.CultureInfo);
			return result;
		}
		bool GetKeyShortcut(KeyboardState keyboard, Microsoft.Xna.Framework.Input.Keys key)
		{
			if(keyboard.IsKeyDown(key) && (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)))
			{
				return true;
			}
			return false;
		}
		bool GetAltKeyShortcut(KeyboardState keyboard, Microsoft.Xna.Framework.Input.Keys key)
		{
			if(keyboard.IsKeyDown(key) && (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
			&& (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)))
			{
				return true;
			}
			return false;
		}
		public static Ray GetMouseRay(Vector2 mousePosition, Viewport viewport, Matrix view, Matrix projection)
		{
			Vector3 nearPoint = new Vector3(mousePosition, 0);
			Vector3 farPoint = new Vector3(mousePosition, 1);
			nearPoint = viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
			farPoint = viewport.Unproject(farPoint, projection, view, Matrix.Identity);
			Vector3 direction = farPoint - nearPoint;
			direction.Normalize();
			return new Ray(nearPoint, direction);
		}
		public bool RayI(Ray finder, BoundingBox obj, out float? dist)
		{
			dist = finder.Intersects(obj);
			if(dist != null)
			{
				return true;
			}
			else return false;
		}
		public bool RayI(Ray finder, BoundingSphere obj, out float? dist)
		{
			dist = finder.Intersects(obj);
			if(dist != null)
			{
				return true;
			}
			else return false;
		}
		public static Vector3 GetBoxCenter(BoundingBox box)
		{
			float x = MathHelper.Lerp(box.Min.X, box.Max.X, 0.5f);
			float y = MathHelper.Lerp(box.Min.Y, box.Max.Y, 0.5f);
			float z = MathHelper.Lerp(box.Min.Z, box.Max.Z, 0.5f);
			return new Vector3(x, y, z);
		}
		public Vector3? RayMeshCollision(Ray r, Model m, Matrix translation)
		{
			for(int i = 0; i < m.Meshes.Count; i++)
			{
				for(int j = 0; j < m.Meshes[i].MeshParts.Count; j++)
				{
					Vector3[] verts = new Vector3[m.Meshes[i].MeshParts[j].NumVertices];
					m.Meshes[i].MeshParts[j].VertexBuffer.GetData(m.Meshes[i].MeshParts[j].VertexOffset + (m.Meshes[i].MeshParts[j].VertexOffset * m.Meshes[i].MeshParts[j].VertexBuffer.VertexDeclaration.VertexStride),
						verts, 0, m.Meshes[i].MeshParts[j].NumVertices, m.Meshes[i].MeshParts[j].VertexBuffer.VertexDeclaration.VertexStride);

					short[] indicies = new short[m.Meshes[i].MeshParts[j].IndexBuffer.IndexCount];
					m.Meshes[i].MeshParts[j].IndexBuffer.GetData(m.Meshes[i].MeshParts[j].StartIndex * 2, indicies, 0, m.Meshes[i].MeshParts[j].PrimitiveCount * 3);

					for(int k = 0; k < verts.Length; k++)
					{
						verts[k] = Vector3.Transform(verts[k], translation);
					}

					for(int k = 0; k < indicies.Length; k += 3)
					{
						Triangle tri = new Triangle(verts[indicies[k]], verts[indicies[k + 1]], verts[indicies[k + 2]]);
						if(MollerTrumboreIntersection(r, tri, out Vector3? intersection)) return intersection;
					}
				}
			}
			return null;
		}
		/// <summary>
		///  Ray-Triangle intersection algorithm
		/// </summary>
		/// <param name="r"></param>
		/// <param name="tri"></param>
		/// <param name="intersectionP"></param>
		/// <returns></returns>
		public static bool MollerTrumboreIntersection(Ray r, Triangle tri, out Vector3? intersectionP)
		{
			//Source : https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
			const float EPSILON = 0.0000001f;
			Vector3 vertex0 = tri.vertex0;
			Vector3 vertex1 = tri.vertex1;
			Vector3 vertex2 = tri.vertex2;
			Vector3 edge1, edge2, h, s, q;
			float a, f, u, v;
			edge1 = vertex1 - vertex0;
			edge2 = vertex2 - vertex0;
			h = Vector3.Cross(r.Direction, edge2);
			a = Vector3.Dot(edge1, h);
			if(a > -EPSILON && a < EPSILON)
			{
				intersectionP = null;
				return false;    // This ray is parallel to this triangle.
			}
			f = 1.0f / a;
			s = r.Position - vertex0;
			u = f * Vector3.Dot(s, h);
			if(u < 0.0 || u > 1.0)
			{
				intersectionP = null;
				return false;
			}
			q = Vector3.Cross(s, edge1);
			v = f * Vector3.Dot(r.Direction, q);
			if(v < 0.0 || u + v > 1.0)
			{
				intersectionP = null;
				return false;
			}
			// At this stage we can compute t to find out where the intersection point is on the line.
			float t = f * Vector3.Dot(edge2, q);
			if(t > EPSILON) // ray intersection
			{
				intersectionP = r.Position + r.Direction * t;
				return true;
			}
			else // This means that there is a line intersection but not a ray intersection.
			{
				intersectionP = null;
				return false;
			}
		}
		void AllowTr(byte flag)
		{
			DisallowTr();
			if(flag == 4)
			{
				_allowmov = true;
				_allowrot = false;
				_allowscal = true;
				return;
			}
			if(flag >= 3) _allowscal = true;
			if(flag >= 2) _allowrot = true;
			if(flag >= 1) _allowmov = true;
		}
		void DisallowTr()
		{
			_allowmov = false;
			_allowrot = false;
			_allowscal = false;
		}
		void ApplyTransformation(GameObject obj,Vector3 tr,byte flag)
		{
			switch(obj)
			{
				case SoundSource ss:
					obj.Position += tr;
					break;
				case Camera cam:
				{
					switch(flag)
					{
						case 1:
						{
							obj.Position += tr;
							break;
						}
						case 2:
						{
							obj.Rotation += tr;
							break;
						}
					}
					break;
				}
				case Skinmesh mesh:
				{
					switch(flag)
					{
						case 1:
						{
							obj.Position += tr;
							break;
						}
						case 2:
						{
							obj.Rotation += tr;
							break;
						}
						case 3:
						{
							mesh.Scale += tr;
							break;
						}
					}
					break;
				}
				case EventOnLocation eol:
				{
					switch(flag)
					{
						case 1:
						{
							switch(eol.BoundingType)
							{
								case BoundingObject.Box:
								{
									eol.BoundingBox.Min += tr;
									eol.BoundingBox.Max += tr;
									break;
								}
								case BoundingObject.Sphere:
								{
									eol.BoundingSphere.Center += tr;
									break;
								}
								default: break;
							}
							break;
						}
						case 3:
						{
							switch(eol.BoundingType)
							{
								case BoundingObject.Box:
								{
									eol.BoundingBox.Max += tr;
									break;
								}
								case BoundingObject.Sphere:
								{
									eol.BoundingSphere.Radius += tr.Length();
									break;
								}
								default:
									break;
							}
							break;
						}
						default: break;
					}
					break;
				}
				default: break;
			}
		}
	}
	public struct Triangle
	{
		public Vector3 vertex0;
		public Vector3 vertex1;
		public Vector3 vertex2;
		public Triangle(Vector3 a, Vector3 b, Vector3 c)
		{
			vertex0 = a;
			vertex1 = b;
			vertex2 = c;
		}
	}
}
