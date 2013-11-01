using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OpaqueTaskbar
{
	public static class API
	{
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		private delegate bool CallBackPtr(IntPtr hwnd, int lParam);

		[Flags]
		private enum DWM_BB
		{
			Enable = 1,
			BlurRegion = 2,
			TransitionMaximized = 4
		}

		private struct DWM_BLURBEHIND
		{
			public API.DWM_BB dwFlags;
			public bool fEnable;
			public IntPtr hRgnBlur;
			public bool fTransitionOnMaximized;
		}

		public class AeroGlassCompositionChangedEvenArgs : EventArgs
		{
			private bool availability;
			public bool GlassAvailable
			{
				get
				{
					return this.availability;
				}
			}
			internal AeroGlassCompositionChangedEvenArgs(bool avilability)
			{
				this.availability = avilability;
			}
		}

		public delegate void AeroGlassCompositionChangedEvent(object sender, API.AeroGlassCompositionChangedEvenArgs e);

		public const int WM_DWMCOMPOSITIONCHANGED = 794;
		public const int WM_DWMNCRENDERINGCHANGED = 799;
		public const int WM_THEMECHANGED = 798;
		public const int WM_DWMCOLORIZATIONCOLORCHANGED = 800;

		private static API.CallBackPtr callBackPtr;
		public static uint WM_TaskbarCreated = 0u;
		private static List<IntPtr> hTaskBars = new List<IntPtr>();

		[DllImport("Shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		internal static extern int HashData([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)] [In] byte[] pbData, int cbData, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 3)] [Out] byte[] piet, int outputLen);
		
        public static string GetHashedString()
		{
			string text = Environment.UserName;
			byte[] bytes = API.GetBytes(text);
			int num = bytes.Length;
			byte[] array = new byte[num];
			int num2 = API.HashData(bytes, num, array, num);
			if (num2 != 0)
			{
				text = BitConverter.ToUInt32(bytes, 0).ToString();
			}
			else
			{
				text = BitConverter.ToUInt32(array, 0).ToString();
			}
			return text;
		}

		private static byte[] GetBytes(string str)
		{
			byte[] array = new byte[str.Length * 2];
			Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hwnd, out API.RECT lpRect);

		public static List<API.RECT> GetTaskbarLocations()
		{
			if (API.hTaskBars.Count == 0)
			{
				return new List<API.RECT>();
			}
			List<API.RECT> list = new List<API.RECT>();
			foreach (IntPtr current in API.hTaskBars)
			{
				API.RECT item = default(API.RECT);
				API.GetWindowRect(current, out item);
				list.Add(item);
			}
			return list;
		}

		public static bool IsTaskBarsLocationChanged(List<API.RECT> OldPositions)
		{
			bool result = false;
			List<API.RECT> taskbarLocations = API.GetTaskbarLocations();
			foreach (API.RECT current in OldPositions)
			{
				bool flag = false;
				foreach (API.RECT current2 in taskbarLocations)
				{
					if (current.bottom == current2.bottom && current.left == current2.left && current.right == current2.right && current.top == current2.top)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
		[DllImport("user32.dll")]
		private static extern int EnumWindows(API.CallBackPtr callPtr, int lPar);

		private static bool FindTaskbarCallback(IntPtr hwnd, int lParam)
		{
			string pattern = "Shell_.*TrayWnd";
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
			StringBuilder stringBuilder = new StringBuilder(256);
			API.GetClassName(hwnd, stringBuilder, stringBuilder.Capacity);
			if (regex.IsMatch(stringBuilder.ToString()))
			{
				API.hTaskBars.Add(hwnd);
			}
			return true;
		}

		public static void FindTaskbars()
		{
			API.hTaskBars.Clear();
			API.callBackPtr = new API.CallBackPtr(API.FindTaskbarCallback);
			API.EnumWindows(API.callBackPtr, 0);
		}

		[DllImport("dwmapi.dll")]
		private static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref API.DWM_BLURBEHIND blurBehind);
		
        public static void DisableTaskbarTransparency()
		{
			if (API.hTaskBars.Count == 0)
			{
				MessageBox.Show("Can't find Taskbar", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			API.DWM_BLURBEHIND dWM_BLURBEHIND = default(API.DWM_BLURBEHIND);
			dWM_BLURBEHIND.dwFlags = API.DWM_BB.Enable;
			dWM_BLURBEHIND.fEnable = true;
			foreach (IntPtr current in API.hTaskBars)
			{
				API.DwmEnableBlurBehindWindow(current, ref dWM_BLURBEHIND);
			}
		}

		public static void EnableTaskbarTransparency()
		{
			if (API.hTaskBars.Count == 0)
			{
				MessageBox.Show("Can't find Taskbar", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			API.DWM_BLURBEHIND dWM_BLURBEHIND = default(API.DWM_BLURBEHIND);
			dWM_BLURBEHIND.dwFlags = API.DWM_BB.Enable;
			dWM_BLURBEHIND.fEnable = false;
			foreach (IntPtr current in API.hTaskBars)
			{
				API.DwmEnableBlurBehindWindow(current, ref dWM_BLURBEHIND);
			}
		}
	}
}
