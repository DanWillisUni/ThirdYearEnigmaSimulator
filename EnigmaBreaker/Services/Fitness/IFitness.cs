using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public interface IFitness
    {
        public delegate IFitness FitnessResolver(string key);
        public double getFitness(string input);
    }
}
