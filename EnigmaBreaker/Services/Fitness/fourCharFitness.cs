using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services.Fitness
{
    public class fourCharFitness : IFitness
    {
        private readonly float[] quadgrams;
        public fourCharFitness(FitnessConfiguration gc)
        {
            quadgrams = new float[845626];//set the new array size
            Array.Fill(quadgrams, (float)Math.Log10(float.Epsilon));//set the array to empty
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(gc.gramFiles.gramDataDir, gc.gramFiles.quadgramFileName)))//for each line in the file
            {
                quadgrams[quadIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65, line.Split(",")[0][2] - 65, line.Split(",")[0][3] - 65)] = float.Parse(line.Split(",")[1]);//set the value in the quadgram array
            }
        }
        /// <summary>
        /// Get the index of the game
        /// </summary>
        /// <param name="a">First letter</param>
        /// <param name="b">Second letter</param>
        /// <param name="c">Third letter</param>
        /// <param name="d">Fourth letter</param>
        /// <returns>index of the array that corrisponds to the location</returns>
        private static int quadIndex(int a, int b, int c, int d)
        {
            return (a << 15) | (b << 10) | (c << 5) | d;//uses logical shifts because it is faster
        }
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int a = 0;
            int b = input[0];
            int c = input[1];
            int d = input[2];
            for (int i = 3; i < input.Length; i++)//for each group for 4 letters
            {
                a = b;
                b = c;
                c = d;
                d = input[i];
                fitness += quadgrams[quadIndex(a, b, c,d)];//add the value of the quadgram to that
            }
            return fitness;
        }
    }
}
