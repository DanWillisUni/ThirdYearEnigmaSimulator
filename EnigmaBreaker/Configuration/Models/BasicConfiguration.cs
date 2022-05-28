using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Configuration.Models
{
    public class BasicConfiguration
    {
        public int numberOfRotorsInUse { get; set; }//the number of possible rotors being used for encryption
        public int numberOfReflectorsInUse { get; set; }//the number of possible relfectors being used for encryption
        public int maxPlugboardSettings { get; set; }//the number of max plugboard pairs being used for encryption

        public string textDir { get; set; }//where the text is stored
        public List<string> textFileNames { get; set; }//list of all the textfile names

        public int numberOfEndCharsToDisplay { get; set; }

        public bool isMeasure { get; set; }//run experiments
        public string inputFormat { get; set; }//RAND or USER for different inputs
    }
}
