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
        public doubleCharFitness(BasicConfiguration bc)
        {
            bigrams = new float[826];
            Array.Fill(bigrams, (float)Math.Log10(float.Epsilon));
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(bc.gramDataDir,bc.bigramFileName)))
            {
                bigrams[biIndex(line.Split(",")[0][0] - 65, line.Split(",")[0][1] - 65)] = float.Parse(line.Split(",")[1]);
            }            
        }        
        private static int biIndex(int a, int b)
        {
            return (a << 5) | b;
        }
        public double getFitness(int[] input, Part part)
        {
            double fitness = 0;
            int current = 0;
            int next = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                current = next;
                next = input[i];
                fitness += bigrams[biIndex(current, next)];
            }
            return fitness;
        }
    }
}
