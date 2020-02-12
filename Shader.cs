using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace AnimatedWallpaper
{
	public class Shader
	{
		private readonly int ID;

		public Shader(string vertexPath, string fragmentPath, string geometryPath = null)
		{
			#region Vertex
			string vertexSource = File.ReadAllText(vertexPath) + "\r\n";

			int vertex = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(vertex, vertexSource);
			GL.CompileShader(vertex);

			GL.GetShader(vertex, ShaderParameter.CompileStatus, out var status);
			if (status == 0)
			{
				string info = GL.GetShaderInfoLog(vertex);
				GL.DeleteShader(vertex);

				throw new Exception($"Failed to compile '{vertexPath}'\n{info}");
			}
			#endregion

			#region Fragment
			string fragmentSource = File.ReadAllText(fragmentPath) + "\r\n";

			int fragment = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(fragment, fragmentSource);
			GL.CompileShader(fragment);

			GL.GetShader(fragment, ShaderParameter.CompileStatus, out status);
			if (status == 0)
			{
				string info = GL.GetShaderInfoLog(fragment);
				GL.DeleteShader(fragment);

				throw new Exception($"Failed to compile '{fragmentPath}'\n{info}");
			}
			#endregion

			#region Geometry
			int? geo = null;
			if (!string.IsNullOrWhiteSpace(geometryPath))
			{
				string geoSource = File.ReadAllText(geometryPath) + "\r\n";

				geo = GL.CreateShader(ShaderType.GeometryShader);
				GL.ShaderSource(geo.Value, geoSource);
				GL.CompileShader(geo.Value);

				GL.GetShader(geo.Value, ShaderParameter.CompileStatus, out status);
				if (status == 0)
				{
					string info = GL.GetShaderInfoLog(geo.Value);
					GL.DeleteShader(geo.Value);

					throw new Exception($"Failed to compile '{geometryPath}'\n{info}");
				}
			}
			#endregion

			ID = GL.CreateProgram();
			GL.AttachShader(ID, vertex);
			GL.AttachShader(ID, fragment);
			if (geo != null) GL.AttachShader(ID, geo.Value);

			GL.LinkProgram(ID);
			GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out status);
			if (status == 0)
			{
				GL.DeleteShader(vertex);
				GL.DeleteShader(fragment);
				if (geo != null) GL.DeleteShader(geo.Value);

				throw new Exception($"Failed to link program\n{GL.GetProgramInfoLog(ID)}");
			}

			GL.DetachShader(ID, vertex);
			GL.DetachShader(ID, fragment);
			if (geo != null) GL.DetachShader(ID, geo.Value);
			GL.DeleteShader(vertex);
			GL.DeleteShader(fragment);
			if (geo != null) GL.DeleteShader(geo.Value);
		}

		public void Bind() => GL.UseProgram(ID);

		public void Unbind() => GL.UseProgram(0);

		public void UploadUniformFloat(string name, float value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform1(location, value);
		}

		public void UploadUniformFloat2(string name, Vector2 value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform2(location, value);
		}

		public void UploadUniformFloat3(string name, Vector3 value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform3(location, value);
		}

		public void UploadUniformFloat4(string name, Vector4 value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform4(location, value);
		}

		public void UploadUniformFloat4(string name, Color4 value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform4(location, value);
		}

		public void UploadUniformMat3(string name, Matrix3 matrix)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.UniformMatrix3(location, false, ref matrix);
		}

		public void UploadUniformMat4(string name, Matrix4 matrix)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.UniformMatrix4(location, false, ref matrix);
		}

		public void UploadUniformInt(string name, int value)
		{
			int location = GL.GetUniformLocation(ID, name);
			GL.Uniform1(location, value);
		}
	}
}