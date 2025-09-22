﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class ImportedFileInfo
    {
        public int Index { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime ImportTime { get; set; }
    }

}
