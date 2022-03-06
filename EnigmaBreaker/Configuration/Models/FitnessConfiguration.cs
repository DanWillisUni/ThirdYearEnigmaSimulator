using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class FitnessConfiguration
    {
        public gramFiles gramFiles { get; set; }
        public Dictionary<string, double> fitnessWeights { get; set;}
        public WeightFiles weightFiles { get; set; }
    }

    public class gramFiles
    {
        public string gramDataDir { get; set; }
        public string singleFileName { get; set; }
        public string bigramFileName { get; set; }
        public string trigramFileName { get; set; }
        public string quadgramFileName { get; set; }
    }

    public class WeightFiles
    {
        public string dir { get;set; }
        public string rotorFileName { get; set; }
        public string offsetFileName { get; set; }
        public string plugboardFileName { get; set; }
    }
}
