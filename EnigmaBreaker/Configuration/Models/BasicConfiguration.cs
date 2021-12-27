using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class BasicConfiguration
    {
        public int topRotorsToSearch { get; set; }
        public int topAllRotationAndOffset { get; set; }
        public int topSingleRotationAndOffset { get; set; }
        public string dataDir { get; set; }
        public string bigramFileName { get; set; }
        public string trigramFileName { get; set; }
        public string quadgramFileName { get; set; }
    }
}
