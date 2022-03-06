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
            trigrams = new float[26426];
            Array.Fill(trigrams, (float)Math.Log10(float.Epsilon));
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(gc.gramFiles.gramDataDir, gc.gramFiles.trigramFileName)))
            {
                trigrams[triIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65, line.Split(",")[0][2] - 65)] = float.Parse(line.Split(",")[1]);
            }
        }
        private static int triIndex(int a, int b, int c)
        {
            return (a << 10) | (b << 5) | c;
        }
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int current = 0;
            int next1 = input[0];
            int next2 = input[1];
            for (int i = 2; i < input.Length; i++)
            {
                current = next1;
                next1 = next2;
                next2 = input[i];
                fitness += trigrams[triIndex(current, next1, next2)];
            }
            return fitness;
        }
    }
}
