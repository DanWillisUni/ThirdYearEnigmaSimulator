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
        public BreakerConfiguration(int len,bool withoutRefinement=false)
        {
            RotorFitness = "IOC";
            OffsetFitness = "IOC";
            PlugboardFitness = "RULE";
            if (!withoutRefinement)
            {                
                numberOfRotorsToKeep = 5;
                numberOfSettingsPerRotorCombinationToKeep = 3;                
                numberOfOffsetToKeep = 20;
                numberOfSettingsPerRotationCombinationToKeep = 20;//set high because it makes little differnece to the timing
                numberOfSinglePlugboardSettingsToKeep = 2;//set to 2 as it is only adds a few seconds        
                numberOfPlugboardSettingsToKeep = 1;//keep only top 1 else the user would have to pick
            }
            else
            {
                numberOfRotorsToKeep = 3;
                numberOfSettingsPerRotorCombinationToKeep = 3;
                numberOfOffsetToKeep = 10;
                numberOfSettingsPerRotationCombinationToKeep = 3;
                numberOfSinglePlugboardSettingsToKeep = 1;
                numberOfPlugboardSettingsToKeep = 1;
            }
        }
    }
}
