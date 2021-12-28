using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class BasicConfiguration
    {
        public int numberOfRotorsInUse { get; set; }
        public int numberOfReflectorsInUse { get; set; }
        public int topRotorsToSearch { get; set; }
        public int topSingleRotorRotationAndOffset { get; set; }
        public int topSingleVarianceRotationAndOffset { get; set; }
        public int topAllRotorRotationAndOffset { get; set; }
        public string dataDir { get; set; }
        public string bigramFileName { get; set; }
        public string trigramFileName { get; set; }
        public string quadgramFileName { get; set; }
    }
}
