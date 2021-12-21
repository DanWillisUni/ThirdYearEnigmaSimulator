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
            Dictionary<char, int> charFrequency = new Dictionary<char, int>();
            foreach(char c in withoutSpaces)
            {
                if (charFrequency.ContainsKey(c))
                {
                    charFrequency[c] = charFrequency[c] + 1;
                }
                else
                {
                    charFrequency.Add(c, 1);
                }
            }

            int sumOfFrequencyCalc = 0;
            foreach(KeyValuePair<char,int> entry in charFrequency)
            {
                sumOfFrequencyCalc += entry.Value * (entry.Value - 1);
            }

            double textLengthMultiplication = withoutSpaces.Length * (withoutSpaces.Length - 1);
            double ioc = sumOfFrequencyCalc * (26/textLengthMultiplication);
            return ioc;

        }
    }
}
