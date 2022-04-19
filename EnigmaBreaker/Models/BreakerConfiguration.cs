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
        /// <summary>
        /// Provides a full configuration on the ideal way to break the enigma the fastest and most accurate based upon all my tests
        /// </summary>
        /// <param name="len">length of the ciphertext</param>
        /// <param name="withoutRefinement">This is useful for measuring test how much difference was made by the tests</param>
        public BreakerConfiguration(int len,bool withoutRefinement=false)
        {            
            if (!withoutRefinement)//if it is refined
            {
                //set the fitness strings
                RotorFitness = "RULE";
                OffsetFitness = "RULE";
                PlugboardFitness = "RULE";

                //string rotorFileName = "../resources/data/rotorWeights.csv";
                //string offsetFileName = "../resources/data/offsetWeights.csv";
                //string plugboardFileName = "../resources/data/plugboardWeights.csv";

                numberOfRotorsToKeep = 20; // higher because it makes very little difference to the computing time as 1 iteration of offset is under 4 seconds at 2000 chars
                numberOfSettingsPerRotorCombinationToKeep = 3;
                
                numberOfOffsetToKeep = 5; //altered because it increases the computing time as the plugboard is searching through a few of them
                if(len < 2000)
                {
                    if (len > 1800)
                    {
                        numberOfOffsetToKeep = 7;
                    }
                    else if (len > 1700)
                    {
                        numberOfOffsetToKeep = 9;
                    }
                    else if (len > 1600)
                    {
                        numberOfOffsetToKeep = 12;
                    }
                    else if (len > 1400)
                    {
                        numberOfOffsetToKeep = 14;
                    }
                    else if (len > 1300)
                    {
                        numberOfOffsetToKeep = 15;
                    }
                    else if (len > 1200)
                    {
                        numberOfOffsetToKeep = 16;
                    }
                    else 
                    {
                        numberOfOffsetToKeep = 20;
                    }
                }
                numberOfSettingsPerRotationCombinationToKeep = 20;//set high because it makes little differnece to the timing
                numberOfSinglePlugboardSettingsToKeep = 1;//set to 2 as it is only adds a few seconds        
                numberOfPlugboardSettingsToKeep = 1;//keep only top 1 else the user would have to pick and only makes an average of 1.4% differnce changing it to 3
            }
            else//else set my original guess at a decent configuration in there
            {
                RotorFitness = "IOC";
                OffsetFitness = "IOC";
                PlugboardFitness = "IOC";
                numberOfRotorsToKeep = 3;
                numberOfSettingsPerRotorCombinationToKeep = 3;
                numberOfOffsetToKeep = 10;
                numberOfSettingsPerRotationCombinationToKeep = 3;
                numberOfSinglePlugboardSettingsToKeep = 1;
                numberOfPlugboardSettingsToKeep = 1;
            }
        }

        private int getIndex(IndexFile indexFile,int length)
        {
            int prevLength = 0;
            indexFileItem fileItemToUse = indexFile.IndexFiles[indexFile.IndexFiles.Count - 1];
            foreach (indexFileItem i in indexFile.IndexFiles)
            {
                if (i.length >= length && prevLength < length)
                {
                    fileItemToUse = i;
                    break;
                }
            }

            int r = -1;
            foreach (KeyValuePair<int,double> entry in fileItemToUse.data)
            {
                r = entry.Key;
                if(entry.Value > 90)
                {
                    break;
                }
            }
            return r;
        }
    }
}
