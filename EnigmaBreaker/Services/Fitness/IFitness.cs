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
        public double getFitness(int[] input,Part part = 0);
    }
}
