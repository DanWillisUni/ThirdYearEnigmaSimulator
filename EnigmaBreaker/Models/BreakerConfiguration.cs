using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class BreakerConfiguration
    {
        public string RotorFitness { get; set; }
        public int numberOfRotorsToKeep { get; set; }
        public int numberOfSettingsPerRotorCombinationToKeep { get; set; }
        public string OffsetFitness { get; set; }
        public int numberOfOffsetToKeep { get; set; }
        public int numberOfSettingsPerRotationCombinationToKeep { get; set; }
        public string PlugboardFitness { get; set; }
        public int numberOfPlugboardSettingsToKeep { get; set; }
        public int numberOfSinglePlugboardSettingsToKeep { get; set; }
        public int maxNumberOfNewSinglePlugboardSettings { get; set; }
        public BreakerConfiguration(int len)
        {
            RotorFitness = "IOC";
            numberOfRotorsToKeep = 5;
            numberOfSettingsPerRotorCombinationToKeep = 3;
            OffsetFitness = "IOC";
            numberOfOffsetToKeep = 5;
            numberOfSettingsPerRotationCombinationToKeep = 5;
            //getting plugboard fitness
            PlugboardFitness = "IOC";
            if (len < 330)
            {
                if (len >= 280)
                {
                    PlugboardFitness = "QUAD";
                }
                else if (len >= 250)
                {
                    PlugboardFitness = "TRI";
                }
                else if (len >= 180)
                {
                    PlugboardFitness = "BI";
                }
                else if (len >= 130)
                {
                    PlugboardFitness = "QUAD";
                }
                else if (len >= 50)
                {
                    PlugboardFitness = "BI";
                }
                else
                {
                    PlugboardFitness = "TRI";
                }
            }
            numberOfSinglePlugboardSettingsToKeep = 2;            
            numberOfPlugboardSettingsToKeep = 1;
            maxNumberOfNewSinglePlugboardSettings = 10;
        }
    }
}
