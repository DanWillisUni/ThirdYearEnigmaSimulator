using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public class weightFitness : IFitness
    {
        private readonly FitnessConfiguration _fc;
        private readonly IFitness.FitnessResolver _resolver;
        private readonly SharedUtilities _sharedUtilities;
        public weightFitness(FitnessConfiguration fc,IFitness.FitnessResolver resolver, SharedUtilities sharedUtilities)
        {
            _resolver = resolver;
            _fc = fc;
            _sharedUtilities = sharedUtilities;
        }
        public double getFitness(int[] input, IFitness.Part part = IFitness.Part.None)
        {
            double r = _sharedUtilities.getHitRate(input.Length, "IOC", part) * _resolver("IOC").getFitness(input);
            foreach (string fitnessStr in new List<string>() { "S", "BI","TRI","QUAD" })
            {
                r += _fc.fitnessWeights[fitnessStr] * _sharedUtilities.getHitRate(input.Length, fitnessStr, part) * _resolver(fitnessStr).getFitness(input) / input.Length;
            }                
            return r;
        }

        
    }
}
