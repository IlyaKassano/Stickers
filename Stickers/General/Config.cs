using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Stickers.General
{
    public class Config
    {
        public TargetProcess[] TargetProcesses { get; set; }
        public int MaxHeightForCaretLocation { get; set; } = 100;
        public int SendEnterWaitTime { get; set; } = 200;
        public int SendCtrlWaitTime { get; set; } = 50;
        public int CaretLocationInterval { get; set; } = 10;
    }

    public class TargetProcess
    {
        public string Name { get; set; }
        public string? BackgroundColor { get; set; }
        public int BottomOffset { get; set; } = -500;
        public int RightOffset { get; set; } = -50;
    }
}
