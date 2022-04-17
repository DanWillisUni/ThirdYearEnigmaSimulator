using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public interface IFitness
    {
        enum Part : ushort
        {
            None = 0,
            Rotor = 1,
            Offset = 2,
            Plugboard = 3
        }
        public delegate IFitness FitnessResolver(string key);
        /// <summary>
        /// Get the fitness of the input to determin how english it is
        /// 
        /// The larger the value the more english it is
        /// </summary>
        /// <param name="input">integer array to represent all the chars</param>
        /// <param name="part">Part of the decryption that it is for</param>
        /// <returns>A double score for the text</returns>
        public double getFitness(int[] input,Part part = 0);
    }
}
