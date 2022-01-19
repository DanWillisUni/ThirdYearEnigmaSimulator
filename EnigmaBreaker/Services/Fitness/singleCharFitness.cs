using EnigmaBreaker.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services.Fitness
{
    public class singleCharFitness : IFitness
    {
        private readonly float[] singles;
        public singleCharFitness(BasicConfiguration bc)
        {
            singles = new float[26];
            Array.Fill(singles, (float)Math.Log10(float.Epsilon));
            foreach (string line in System.IO.File.ReadLines(System.IO.Path.Combine(bc.gramDataDir, bc.singleFileName)))
            {
                singles[line.Split(",")[0][0] - 65] = float.Parse(line.Split(",")[1]);
            }
        }
        public double getFitness(int[] input)
        {
            double fitness = 0;
            for (int i = 0; i < input.Length; i++)
            {
                fitness += singles[input[i]];
            }
            return fitness;
        }
    }
}
