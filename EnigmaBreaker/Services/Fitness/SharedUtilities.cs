using System;
using System.Collections.Generic;
using System.Text;
using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;

namespace EnigmaBreaker.Services.Fitness
{
    public class SharedUtilities
    {
        private readonly FitnessConfiguration _fc;
        private readonly CSVReaderService<WeightFile> _csvR;
        private List<WeightFile> rotorWeightFile;
        private List<WeightFile> offsetWeightFile;
        private List<WeightFile> plugboardWeightFile;
        public SharedUtilities(FitnessConfiguration fc, CSVReaderService<WeightFile> csvR)
        {
            _fc = fc;
            _csvR = csvR;
            rotorWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.rotorFileName);
            offsetWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.offsetFileName);
            plugboardWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.plugboardFileName);
        }
        /// <summary>
        /// Get the accuracy of a given fitness function for a specific part of the at a set length
        /// </summary>
        /// <param name="length">length of ciphertext</param>
        /// <param name="fitnessString">fitness function string</param>
        /// <param name="part">part of deciphering it is for</param>
        /// <returns>The accuracy from the file</returns>
        public double getHitRate(int length, string fitnessString, IFitness.Part part = IFitness.Part.None)
        {
            List<WeightFile> wf = new List<WeightFile>();//setting the weightfile for the correct part
            if (part == IFitness.Part.Rotor)
            {
                wf = rotorWeightFile;
            }
            else if (part == IFitness.Part.Offset)
            {
                wf = offsetWeightFile;
            }
            else if (part == IFitness.Part.Plugboard)
            {
                wf = plugboardWeightFile;
            }
            int previous = 0;
            double r = -1;
            foreach (WeightFile w in wf)//for every row in the file
            {
                if (length <= Convert.ToInt32(w.length) && length > previous)//if the length is between the previous and current
                {
                    r = w.weights[fitnessString];//get the correct column
                    break;
                }
                previous = Convert.ToInt32(w.length);//set the previous to current
            }
            if (r == -1)
            {
                r = wf[wf.Count - 1].weights[fitnessString];//if the file doesnt go high enough use the last one in the file
            }
            return r;
        }
    }
}
