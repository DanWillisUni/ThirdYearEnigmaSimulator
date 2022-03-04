using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public class weightFitness : IFitness
    {
        private readonly FitnessConfiguration _fc;
        private readonly IFitness.FitnessResolver _resolver;
        public weightFitness(FitnessConfiguration fc,IFitness.FitnessResolver resolver)
        {
            _resolver = resolver;
            _fc = fc;
        }
        public double getFitness(int[] input, IFitness.Part part = IFitness.Part.None)
        {
            double r = getMissRate(input.Length, "IOC", part) * _resolver("IOC").getFitness(input);
            foreach (string fitnessStr in new List<string>() { })
                r += _fc.fitnessWeights[fitnessStr] * getMissRate(input.Length,fitnessStr,part) * _resolver(fitnessStr).getFitness(input) / input.Length;
            return r;
        }

        private double getMissRate(int length, string fitnessString, IFitness.Part part = IFitness.Part.None)
        {
            return 1;
        }
    }
}
