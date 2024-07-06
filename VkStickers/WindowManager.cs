using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static VkStickers.WinApi;

namespace VkStickers
{
    internal class WindowManager
    {
        static bool _show;
        internal static IntPtr LastActiveWindow;

        static string[] _allowedProcesses = new string[] { "steamwebhelper", "Discord" };
        internal static void ShowWindow(CaretLocation caretLocation)
        {
            LastActiveWindow = GetForegroundWindow();

            GetWindowThreadProcessId(LastActiveWindow, out int id);
            //var err = GetLastError();
            //Debug.WriteLine(new Win32Exception(err));
            var process = Process.GetProcessById(id);
            //Debug.WriteLine(process.ProcessName);
            if (!_allowedProcesses.Contains(process.ProcessName))
                return;

            Debug.WriteLine("Show");
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            //PostMessage(handle, WM_SYSCOMMAND, SC_RESTORE, 0);
            SetWindowPos(handle, -1, caretLocation.Left + 600, caretLocation.Top - 470, 0, 0, SWP_ASYNCWINDOWPOS | SWP_NOSIZE | SWP_SHOWWINDOW);
            //SetForegroundWindow(handle);

            //ShowWindow(handle, ShowWindowCommands.ShowMinNoActive);

            _show = caretLocation.Width != 0;
        }

        internal static void HideWindow(CaretLocation caretLocation)
        {
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            var fore = GetForegroundWindow();
            if (handle != fore)
            {
                Debug.WriteLine("PostMessage");
                //PostMessage(handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                SetWindowPos(handle, 1, 5000, 3000, 0, 0, SWP_ASYNCWINDOWPOS | SWP_NOSIZE | SWP_SHOWWINDOW);

                //ShowWindow(handle, ShowWindowCommands.Hide);

                _show = caretLocation.Width != 0;
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
