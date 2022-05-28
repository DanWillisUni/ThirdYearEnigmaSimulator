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
        /// <summary>
        /// Gets hte fitness using the rules set out by my csv
        /// </summary>
        /// <param name="input">input array of integers to represent ciphertext</param>
        /// <param name="part">part of decryption process</param>
        /// <returns>fitness determined by the rules</returns>
        public double getFitness(int[] input, Part part)
        {
            int len = input.Length;
            string highestString = _sharedUtilities.getRes(len, part);//get the highest fitness of string
            IFitness newFit = _resolver(highestString);//resolve to get the highest
            return newFit.getFitness(input, Part.None);//get and return fitness of the other fitness function
        }

        
    }
}
