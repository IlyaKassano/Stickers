using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkStickers.General
{
    public class Config
    {
        public TargetProcess[] TargetProcesses { get; set; }
    }

    public class TargetProcess
    {
        public string Name { get; set; }
        public string? BackgroundColor { get; set; }
    }
}
