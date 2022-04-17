using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class doubleCharFitness : IFitness
    {
        private readonly float[] bigrams;
        /// <summary>
        /// Constructs the bigram array from the file
        /// </summary>
        /// <param name="fc">Fitness configuration</param>
        public doubleCharFitness(FitnessConfiguration fc)
        {
            bigrams = new float[826];//creates new array
            Array.Fill(bigrams, (float)Math.Log10(float.Epsilon));//fills the array
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(fc.gramFiles.gramDataDir,fc.gramFiles.bigramFileName)))//for each line in the file
            {
                bigrams[biIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65)] = float.Parse(line.Split(",")[1]);//get the index in the array to put it and place the value there
            }            
        }        
        /// <summary>
        /// Get the index of the array based on the values of the two chars
        /// </summary>
        /// <param name="a">first letter</param>
        /// <param name="b">second letter</param>
        /// <returns></returns>
        private static int biIndex(int a, int b)
        {
            return (a << 5) | b;//return index
        }        
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int current = 0;
            int next = input[0];
            for (int i = 1; i < input.Length; i++)//for each letter
            {
                current = next;//set the previous
                next = input[i];//set the next
                fitness += bigrams[biIndex(current, next)];//add the value from the bigrams to the fitness to return
            }
            return fitness;
        }
    }
}
