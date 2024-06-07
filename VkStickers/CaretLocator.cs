using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static VkStickers.MainWindow;

namespace VkStickers
{
    public class CaretLocation {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CaretLocator
    {
        const uint OBJID_CARET = 0xFFFFFFF8;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetGUIThreadInfo(uint hTreadID, ref GUITHREADINFO lpgui);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

        public static CaretLocation Locate() {
            var guiInfo = new GUITHREADINFO();
            guiInfo.cbSize = Marshal.SizeOf(guiInfo);
            GetGUIThreadInfo(0, ref guiInfo);

            Guid guid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
            dynamic o = new object();
            var obj = AccessibleObjectFromWindow(guiInfo.hwndFocus, OBJID_CARET, ref guid, ref o);
            var varChild = new object();
            if (o == null)
                return null;

            o.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, 0);
            Debug.WriteLine("{0}, {1}, {2}, {3}", pxLeft, pyTop, pcxWidth, pcyHeight);

            return new CaretLocation()
            {
                Left = pxLeft,
                Top = pyTop,
                Width = pcxWidth,
                Height = pcyHeight,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int iLeft;
        public int iTop;
        public int iRight;
        public int iBottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public RECT rectCaret;
    }
}
