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
            quadgrams = new float[845626];
            Array.Fill(quadgrams, (float)Math.Log10(float.Epsilon));
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(gc.gramFiles.gramDataDir, gc.gramFiles.quadgramFileName)))
            {
                quadgrams[quadIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65, line.Split(",")[0][2] - 65, line.Split(",")[0][3] - 65)] = float.Parse(line.Split(",")[1]);
            }
        }
        private static int quadIndex(int a, int b, int c, int d)
        {
            return (a << 15) | (b << 10) | (c << 5) | d;
        }
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int current = 0;
            int next1 = input[0];
            int next2 = input[1];
            int next3 = input[2];
            for (int i = 3; i < input.Length; i++)
            {
                current = next1;
                next1 = next2;
                next2 = next3;
                next3 = input[i];
                fitness += quadgrams[quadIndex(current, next1, next2,next3)];
            }
            return fitness;
        }
    }
}
