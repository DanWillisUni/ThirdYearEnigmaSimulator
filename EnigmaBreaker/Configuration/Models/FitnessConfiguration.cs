using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class FitnessConfiguration
    {
        public string gramDataDir { get; set; }
        public string singleFileName { get; set; }
        public string bigramFileName { get; set; }
        public string trigramFileName { get; set; }
        public string quadgramFileName { get; set; }

        public Dictionary<string, double> fitnessWeights { get; set;}
    }
}
