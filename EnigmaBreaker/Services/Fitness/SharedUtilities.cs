﻿using System;
using System.Collections.Generic;
using System.Text;
using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;

namespace EnigmaBreaker.Services.Fitness
{
    public class SharedUtilities
    {
        private readonly FitnessConfiguration _fc;
        private readonly IFitness.FitnessResolver _resolver;
        private readonly CSVReaderService<WeightFile> _csvR;
        private List<WeightFile> rotorWeightFile;
        private List<WeightFile> offsetWeightFile;
        private List<WeightFile> plugboardWeightFile;
        public SharedUtilities(FitnessConfiguration fc, IFitness.FitnessResolver resolver, CSVReaderService<WeightFile> csvR)
        {
            _resolver = resolver;
            _fc = fc;
            _csvR = csvR;
            rotorWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.rotorFileName);
            offsetWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.offsetFileName);
            plugboardWeightFile = _csvR.readFromFile(_fc.weightFiles.dir, _fc.weightFiles.plugboardFileName);
        }
        public double getHitRate(int length, string fitnessString, IFitness.Part part = IFitness.Part.None)
        {
            List<WeightFile> wf = new List<WeightFile>();
            if (part == IFitness.Part.Rotor)
            {
                wf = rotorWeightFile;
            }
            else if (part == IFitness.Part.Offset)
            {
                wf = offsetWeightFile;
            }
            else if (part == IFitness.Part.Plugboard)
            {
                wf = plugboardWeightFile;
            }
            int previous = 0;
            double r = -1;
            foreach (WeightFile w in wf)
            {
                if (length <= Convert.ToInt32(w.length) && length > previous)
                {
                    r = w.weights[fitnessString];
                    break;
                }
                previous = Convert.ToInt32(w.length);
            }
            if (r == -1)
            {
                r = wf[wf.Count - 1].weights[fitnessString];
            }
            return r;
        }
    }
}
