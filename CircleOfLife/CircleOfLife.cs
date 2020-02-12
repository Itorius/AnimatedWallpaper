using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Threading.Tasks;

namespace AnimatedWallpaper.CircleOfLife
{
	// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true

	public struct Particle
	{
		public Vector2 position;
	}

	internal class CircleOfLife : GameWindow
	{
		private const float DegToRad = MathF.PI / 180f;

		private const int BatchSize = 200;

		private double TotalTime;
		private Vector2 MousePosition;

		private unsafe Particle* particles;

		private Shader shader;
		private int drawIndex;
		private const double smoothing = 0.9;
		private double frameTime;
		private Matrix4 matrix;

		public CircleOfLife(int width = 1280, int height = 720, string title = "Game") : base(width, height, GraphicsMode.Default, title)
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

			shader = new Shader("CircleOfLife/Assets/basic.vert", "CircleOfLife/Assets/basic.frag");

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			const BufferAccessMask fMap = BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit;
			const BufferStorageFlags fCreate = BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.DynamicStorageBit;

			GL.GenBuffers(1, out int vertexPosVBO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexPosVBO);

			GL.BufferStorage(BufferTarget.ArrayBuffer, (BatchSize + 1) * sizeof(Vector2), IntPtr.Zero, fCreate);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

			float* pVertexPosBufferData = (float*)GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, (BatchSize + 1) * sizeof(Vector2), fMap);
			particles = (Particle*)pVertexPosBufferData;

			if (particles == null) throw new Exception();

			for (int i = 0; i < BatchSize; i++)
			{
				ref Particle particle = ref particles[drawIndex];
				particle.position = new Vector2(0f, 0f);
				drawIndex++;
			}
		}

		private float angle;

		protected override unsafe void OnUpdateFrame(FrameEventArgs e)
		{
			var state = Mouse.GetCursorState();
			MousePosition = new Vector2(state.X, Height - state.Y);

			angle += DegToRad * 0.25f;
			TotalTime += e.Time;

			Quaternion q = Quaternion.FromAxisAngle(Vector3.UnitZ, angle*2f);

			Parallel.For(0, BatchSize, i =>
			{
				ref Particle particle = ref particles[i];
				ref Vector2 position = ref particle.position;

				float a = angle + i * (360f / BatchSize) * DegToRad;
				position = new Vector2(MathF.Cos(a), MathF.Sin(a)) * (Width / 5f);

				position *= 1f+MathF.Sin(a * 15f) * 0.15f;

				position = Vector2.Transform(position, q);

				// position*=Vector2.Normalize(position)*100f*MathF.Sin(a*12f);
			});

			particles[BatchSize] = particles[0];
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			frameTime = frameTime * smoothing + e.Time * (1.0 - smoothing);
			// Console.WriteLine(1 / frameTime);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.Bind();
			shader.UploadUniformMat4("u_ViewProjection", matrix);
			var color = Color4.FromHsv(new Vector4((float)(Math.Sin(TotalTime * 0.15f) + 1f) * 0.5f, 1.0f, 1.0f, 0.0f));
			color.A = 1f;
			shader.UploadUniformFloat4("u_Color", color);

			GL.LineWidth(3f);
			GL.PointSize(10f);
			GL.DrawArrays(PrimitiveType.Points, 0, drawIndex + 1);
			GL.DrawArrays(PrimitiveType.LineStrip, 0, drawIndex + 1);

			SwapBuffers();
		}
	}
}