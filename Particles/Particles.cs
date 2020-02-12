using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Threading.Tasks;

namespace AnimatedWallpaper.Particles
{
	// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true

	public struct Particle
	{
		public Vector2 position;
	}

	internal class Particles : GameWindow
	{
		private const int NumX = 100;
		private const int NumY = 100;

		private const int BatchSize = NumX * NumY;
		private const float FrictionFactor = 0.01f;

		private unsafe Particle* particles;
		private Vector2[] velocities = new Vector2[BatchSize];

		private Shader shader;
		private int drawIndex;
		private const double smoothing = 0.9;
		private double frameTime;
		private Vector2 Mouse;
		private int direction;
		private Matrix4 matrix;

		public Particles(int width = 1280, int height = 720, string title = "Game") : base(width, height, GraphicsMode.Default, title)
		{
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			matrix = Matrix4.CreateOrthographic(Width, Height, -1, 1);
		}

		protected override unsafe void OnLoad(EventArgs e)
		{
			Console.WriteLine(GL.GetString(StringName.Vendor) + " " + GL.GetString(StringName.Renderer));
			Console.WriteLine(GL.GetString(StringName.Version));

			shader = new Shader("Particles/Assets/basic.vert", "Particles/Assets/basic.frag", "Particles/Assets/basic.geom");

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			const BufferAccessMask fMap = BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit;
			const BufferStorageFlags fCreate = BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.DynamicStorageBit;

			GL.GenBuffers(1, out int vertexPosVBO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexPosVBO);

			GL.BufferStorage(BufferTarget.ArrayBuffer, BatchSize * sizeof(Vector2), IntPtr.Zero, fCreate);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

			float* pVertexPosBufferData = (float*)GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, BatchSize * sizeof(Vector2), fMap);
			particles = (Particle*)pVertexPosBufferData;

			if (particles == null) throw new Exception();

			float strideX = (float)Width / (NumX - 1);
			float strideY = (float)Height / (NumY - 1);

			for (int x = -NumX / 2; x < NumX / 2; x++)
			{
				for (int y = -NumY / 2; y < NumY / 2; y++)
				{
					ref Particle particle = ref particles[drawIndex];
					particle.position = new Vector2(x * strideX, y * strideY);
					drawIndex++;
				}
			}
		}

		private static bool DesktopFocused()
		{
			var activatedHandle = WinAPI.GetForegroundWindow();
			return activatedHandle == Program.desktop;
		}

		protected override unsafe void OnUpdateFrame(FrameEventArgs e)
		{
			var state = OpenTK.Input.Mouse.GetCursorState();
			MousePosition = new Vector2(state.X, Height - state.Y);
			
			if (DesktopFocused())
			{
				Mouse = new Vector2(state.X - X - Width / 2, Height / 2 - (state.Y - Y));

				if (state.LeftButton == ButtonState.Pressed) direction = 1;
				else if (state.RightButton == ButtonState.Pressed) direction = -1;
				else direction = 0;
			}

			totalTime += e.Time;

			Vector2 min = new Vector2(-Width * 0.5f, -Height * 0.5f);
			Vector2 max = new Vector2(Width * 0.5f, Height * 0.5f);

			const int parts = 10;
			const int partSize = BatchSize / parts;
			float speed = (float)(10f * e.Time);

			Parallel.For(0, BatchSize, i =>
			{
				ref Particle particle = ref particles[i];
				ref Vector2 position = ref particle.position;

				float distance = Vector2.Distance(Mouse, position);
				if (distance < 10f) distance = 10f;

				Vector2 velocity = direction * Vector2.Normalize(Mouse - position) * -speed / (distance * 0.1f);
				velocities[i] += velocity;

				velocities[i] = velocities[i] - FrictionFactor * velocities[i];

				position += velocities[i];

				Vector2.Clamp(ref position, ref min, ref max, out position);
			});
		}

		private double totalTime;
		private Vector2 MousePosition;
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			frameTime = frameTime * smoothing + e.Time * (1.0 - smoothing);
			//Console.WriteLine(1 / frameTime);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.Bind();
			shader.UploadUniformMat4("u_ViewProjection", matrix);
			var color = Color4.FromHsv(new Vector4((float)(Math.Sin(totalTime * 0.15f) + 1f) * 0.5f, 1.0f, 1.0f, 0.0f));
			color.A = 1f;
			shader.UploadUniformFloat4("u_Color", color);
			shader.UploadUniformInt("size", 2);
			shader.UploadUniformFloat2("u_MousePos", MousePosition);
			
			GL.DrawArrays(PrimitiveType.Points, 0, drawIndex);

			SwapBuffers();
		}
	}
}