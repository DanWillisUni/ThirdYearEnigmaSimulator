using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class BasicConfiguration
    {
        public int numberOfRotorsInUse { get; set; }
        public int numberOfReflectorsInUse { get; set; }
        public int maxPlugboardSettings { get; set; }
        public int topRotorsToSearch { get; set; }
        public int topSingleRotors { get; set; }
        public int topSingleRotorRotationAndOffset { get; set; }
        public int topSingleVarianceRotationAndOffset { get; set; }
        public int topAllRotorRotationAndOffset { get; set; }
        public int topSinglePlugboardPairs { get; set; }
        public string gramDataDir { get; set; }
        public string bigramFileName { get; set; }
        public string trigramFileName { get; set; }
        public string quadgramFileName { get; set; }
        public string textDir { get; set; }
        public List<string> textFileNames { get; set; }
    }
}
