using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class ruleFitness : IFitness
    {
        private readonly IFitness.FitnessResolver _resolver;
        public ruleFitness(IFitness.FitnessResolver resolver)
        {
            _resolver = resolver;
        }
        public double getFitness(int[] input, Part part)
        {
            int len = input.Length;
            string fitnessStr = "IOC";            

            if(part == Part.Plugboard)
            {
                if (len < 405)
                {
                    if (len >= 285)
                    {
                        fitnessStr = "S";
                    }
                    else if (len >= 200)
                    {
                        fitnessStr = "QUAD";
                    }
                    else if (len >= 120)
                    {
                        fitnessStr = "BI";
                    }
                    else
                    {
                        fitnessStr = "QUAD";
                    }
                }
            }            

            IFitness newFit = _resolver(fitnessStr);
            return newFit.getFitness(input, Part.None);
        }
    }
}
