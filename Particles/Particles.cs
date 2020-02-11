using OpenTK;
using System;
using System.Drawing;

namespace AnimatedWallpaper
{
	internal class Particles
	{
		public static IntPtr desktop;

		public Particles()
		{
			using Game game = new Game(1920, 1080, "LiveWallpaper")
			{
				Location = new Point(0, 0),
				WindowBorder = WindowBorder.Hidden,
				WindowState = WindowState.Maximized
			};

			IntPtr progman = WinAPI.FindWindow("Progman", null);

			WinAPI.SendMessageTimeout(progman, 1324, new IntPtr(0), IntPtr.Zero, WinAPI.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out _);

			IntPtr workerw = IntPtr.Zero;

			WinAPI.EnumWindows((tophandle, topparamhandle) =>
			{
				IntPtr p = WinAPI.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

				if (p != IntPtr.Zero)
				{
					desktop = tophandle;

					workerw = WinAPI.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
				}

				return true;
			}, IntPtr.Zero);

			WinAPI.SetParent(game.WindowInfo.Handle, workerw);

			game.Run(60, 60);

			WinAPI.SetParent(game.WindowInfo.Handle, IntPtr.Zero);
		}
	}
}