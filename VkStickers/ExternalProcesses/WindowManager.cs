using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using VkStickers.General;
using static VkStickers.General.WinApi;

namespace VkStickers.ExternalProcesses
{
    internal class WindowManager
    {
        internal static IntPtr LastActiveWindow;
        internal static bool Showing { get; set; }

        internal static bool ShowWindow(CaretLocation caretLocation, TargetProcess[] targetProcesses)
        {
            LastActiveWindow = GetForegroundWindow();

            GetWindowThreadProcessId(LastActiveWindow, out int id);
            var process = Process.GetProcessById(id);
            if (!targetProcesses.Any(tp => tp.Name == process.ProcessName))
                return false;

            TargetProcess? targetProcess = targetProcesses.SingleOrDefault(tp => tp.Name == process.ProcessName);
            MainWindow.StickerBackground = null;
            if (targetProcess?.BackgroundColor != null)
            {
                var color = (Color)ColorConverter.ConvertFromString(targetProcess.BackgroundColor);
                MainWindow.StickerBackground = color;
            }

            if (!GetWindowRect(LastActiveWindow, out RECT rect))
            {
                Debug.WriteLine("Could not get window rect for: " + LastActiveWindow);
                return false;
            }
            if (rect.Bottom - 200 > caretLocation.Top)
            {
                Debug.WriteLine("Caret location is higher than allowed");
                return false;
            }

            Debug.WriteLine("Show");
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            var x = rect.Right + targetProcess?.RightOffset ?? 0;
            var y = rect.Bottom + targetProcess?.BottomOffset ?? 0;
            SetWindowPos(handle, -1, x, y, 0, 0, SWP_ASYNCWINDOWPOS | SWP_NOSIZE | SWP_SHOWWINDOW);

            Showing = true;
            return true;
        }

        internal static void HideWindow(CaretLocation caretLocation)
        {
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            var fore = GetForegroundWindow();
            if (handle != fore)
            {
                Debug.WriteLine("Hide");
                SetWindowPos(handle, 1, 5000, 3000, 0, 0, SWP_ASYNCWINDOWPOS | SWP_NOSIZE | SWP_SHOWWINDOW);

                Showing = false;
            }
        }


        const int SC_MINIMIZE = 0xF020;
        const int SC_RESTORE = 0xF120;
        const int WM_SYSCOMMAND = 0x0112;
        const int SWP_ASYNCWINDOWPOS = 0x4000;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_NOACTIVATE = 0x0010;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_HIDEWINDOW = 0x0080;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    }
}
