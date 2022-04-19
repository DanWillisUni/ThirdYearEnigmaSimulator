using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class indexOfCoincidence : IFitness
    {
        /// <summary>
        /// Calculate the IC for the text input
        /// </summary>
        /// <param name="input">input array of integers that represent ciphertext</param>
        /// <returns>the index of coincidence</returns>
        private double getIOC(int[] input)
        {            
            int[] charFrequency = new int[26];//set all the frequency to 0
            foreach (int c in input)//for every letter
            {
                charFrequency[c] += 1;//add 1 to the frequency count of that letter
            }

            int sumOfFrequencyCalc = 0;
            foreach (int f in charFrequency)//for each letter
            {
                if (f > 0)
                {                    
                    sumOfFrequencyCalc += f * (f - 1);//calculate numerator of chance of picking that letter twice
                }                
            }

            double textLengthMultiplication = (input.Length * (input.Length - 1));//calculate the demominator
            double ioc = (sumOfFrequencyCalc / textLengthMultiplication);
            return ioc;
        }
        public double getFitness(int[] input, Part part=Part.None)
        {
            double IOC = 1;
            if (input.Length > 1)
            {
               IOC = getIOC(input);//extracted to extra function because I might want to do abs(1.73-IOC) 
            }            
            return 26 * IOC;//normalise with 26
        }
    }
}
