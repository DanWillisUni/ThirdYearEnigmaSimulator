using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public class indexOfCoincidence : IFitness
    {
        public double getFitness(string input)
        {
            string withoutSpaces = input.Replace(" ", "");
            int[] charFrequency = new int[26];
            foreach(char c in withoutSpaces)
            {
                charFrequency[c-65] += 1;
            }

            int sumOfFrequencyCalc = 0;
            foreach(int f in charFrequency)
            {
                sumOfFrequencyCalc += f * (f - 1);
            }

            double textLengthMultiplication = (withoutSpaces.Length * (withoutSpaces.Length - 1));
            double ioc = sumOfFrequencyCalc * (26/textLengthMultiplication);
            return ioc;

        }
    }
}
