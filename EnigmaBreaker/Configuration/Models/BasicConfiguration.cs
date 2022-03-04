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
        
        public string textDir { get; set; }
        public List<string> textFileNames { get; set; }

        public int numberOfEndCharsToDisplay { get; set; }
    }
}
