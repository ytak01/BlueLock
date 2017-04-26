using System;
using System.Runtime.InteropServices;

namespace Win32_API
{
	internal struct LASTINPUTINFO 
	{
		public uint cbSize;
		public uint dwTime;
	}

	/// <summary>
	/// Summary description for Win32.
	/// </summary>
	public class Win32
	{
        public const int WM_MyMessage1 = 0x8000 + 0x01;
        public const int WM_MyMessage2 = 0x8000 + 0x02;

        [DllImport("User32.dll")]
		public static extern bool LockWorkStation();

		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);		

		[DllImport("Kernel32.dll")]
		private static extern uint GetLastError();


        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string className, string windowName);

        public static uint GetIdleTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
			GetLastInputInfo(ref lastInPut);

			return ( (uint)Environment.TickCount - lastInPut.dwTime);
		}

		public static long GetTickCount()
		{
			return Environment.TickCount;
		}

		public static long GetLastInputTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
			if (!GetLastInputInfo(ref lastInPut))
			{
				throw new Exception(GetLastError().ToString());
			}
							
			return lastInPut.dwTime;
		}


	}

}
