using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class indexOfCoincidence : IFitness
    {
        private double getIOC(int[] input)
        {
            int[] charFrequency = new int[26];
            foreach (int c in input)
            {
                charFrequency[c] += 1;
            }

            int sumOfFrequencyCalc = 0;
            foreach (int f in charFrequency)
            {
                sumOfFrequencyCalc += f * (f - 1);
            }

            double textLengthMultiplication = (input.Length * (input.Length - 1));
            double ioc = sumOfFrequencyCalc * (26 / textLengthMultiplication);
            return ioc;
        }
        public double getFitness(int[] input, Part part)
        {            
            double IOC = getIOC(input);
            return Math.Abs(1.73-IOC);
        }
    }
}
