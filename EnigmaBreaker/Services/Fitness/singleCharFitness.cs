using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class singleCharFitness : IFitness
    {
        private readonly float[] singles;
        public singleCharFitness(FitnessConfiguration gc)
        {
            singles = new float[26];//set the new array
            Array.Fill(singles, (float)Math.Log10(float.Epsilon));//fill the new array
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(gc.gramFiles.gramDataDir, gc.gramFiles.singleFileName)))//for every value in the file
            {
                singles[line.Split(",")[0][0] - 65] = float.Parse(line.Split(",")[1]);//set the array value from the file
            }
        }        
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            for (int i = 0; i < input.Length; i++)//for each letter in the input
            {
                fitness += singles[input[i]];//add the value to the sum
            }
            return fitness;
        }
    }
}
