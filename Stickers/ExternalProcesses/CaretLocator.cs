using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Stickers.General;
using static Stickers.General.WinApi;

namespace Stickers.ExternalProcesses
{
    public class CaretLocation
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CaretLocator
    {
        const uint OBJID_CARET = 0xFFFFFFF8;

        static ConcurrentDictionary<IntPtr, object> _accessibleObjects = new ConcurrentDictionary<IntPtr, object>();

        public static CaretLocation? Locate()
        {
            var guiInfo = new GUITHREADINFO();
            guiInfo.cbSize = Marshal.SizeOf(guiInfo);
            GetGUIThreadInfo(0, ref guiInfo);
            if (guiInfo.hwndFocus == 0)
                return null;

            _accessibleObjects.TryGetValue(guiInfo.hwndFocus, out dynamic? ao);
            if (ao == null)
            {
                Guid guid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
                dynamic o = new object();
                AccessibleObjectFromWindow(guiInfo.hwndFocus, OBJID_CARET, ref guid, ref o); // TODO Fix memory leak
                if (o == null)
                    return null;

                _accessibleObjects.TryAdd(guiInfo.hwndFocus, o);
                ao = o;
            }

            ao.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, 0);
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
}
