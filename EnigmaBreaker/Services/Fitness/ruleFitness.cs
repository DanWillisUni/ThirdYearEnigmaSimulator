using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class ruleFitness : IFitness
    {
        private readonly IFitness.FitnessResolver _resolver;
        private readonly SharedUtilities _sharedUtilities;
        public ruleFitness(IFitness.FitnessResolver resolver, SharedUtilities sharedUtilities)
        {
            _resolver = resolver;
            _sharedUtilities = sharedUtilities;
        }
        public double getFitness(int[] input, Part part)
        {
            int len = input.Length;
            double highest = 0;
            string highestString = null;
            foreach (string fitnessStr in new List<string>() { "S", "BI", "TRI", "QUAD" })
            {
                double current = _sharedUtilities.getHitRate(input.Length, fitnessStr, part);
                if(current > highest) 
                {
                    highest = current;
                    highestString = fitnessStr;
                }
            }
            IFitness newFit = _resolver(highestString);
            return newFit.getFitness(input, Part.None);
        }
    }
}
