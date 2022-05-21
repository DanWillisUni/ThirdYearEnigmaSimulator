using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class tripleCharFitness : IFitness
    {
        private readonly float[] trigrams;
        public tripleCharFitness(FitnessConfiguration gc)
        {
            trigrams = new float[26426];//set the size of the array
            Array.Fill(trigrams, (float)Math.Log10(float.Epsilon));//set the array to all epsilon
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(gc.gramFiles.gramDataDir, gc.gramFiles.trigramFileName)))//for each line in the file
            {
                trigrams[triIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65, line.Split(",")[0][2] - 65)] = float.Parse(line.Split(",")[1]);//set the entry in the correct place in the arrya
            }
        }
        /// <summary>
        /// Get the index of the score for the combination of letters
        /// </summary>
        /// <param name="a">first letter</param>
        /// <param name="b">second letter</param>
        /// <param name="c">third letter</param>
        /// <returns>index in the array for that combination</returns>
        private static int triIndex(int a, int b, int c)
        {
            return (a << 10) | (b << 5) | c;//using left shift because it is faster
        }
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int a = 0;
            int b = input[0];
            int c = input[1];
            for (int i = 2; i < input.Length; i++)//for every 3 consecutive letters
            {
                a = b;
                b = c;
                c = input[i];
                fitness += trigrams[triIndex(a, b, c)];//get the score for those letter and add to the sum
            }
            return fitness;
        }
    }
}
