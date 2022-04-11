using EngimaSimulator.Configuration.Models;
using EnigmaBreaker.Services.Fitness;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnitTests
{
    public class TestBreaker
    {
        PhysicalConfiguration pc { get; set; }

        [SetUp]
        public void Setup()
        {
            string json = @"{
    'rotors': [
      {
        'name': 'I',
        'order': 'EKMFLGDQVZNTOWYHXUSPAIBRCJ',
        'turnoverNotchA': 7

      },
      {
        'name': 'II',
        'order': 'AJDKSIRUXBLHWTMCQGZNPYFVOE',
        'turnoverNotchA': 25
      },
      {
        'name': 'III',
        'order': 'BDFHJLCPRTXVZNYEIWGAKMUSQO',
        'turnoverNotchA': 11
      },
      {
        'name': 'IV',
        'order': 'ESOVPZJAYQUIRHXLNFTGKDCMWB',
        'turnoverNotchA': 6
      },
      {
        'name': 'V',
        'order': 'VZBRGITYUPSDNHLXAWMJQOFECK',
        'turnoverNotchA': 1
      }
    ],
    'reflectors': [      
      {
                'name': 'A',
        'order': 'EJMZALYXVBWFCRQUONTSPIKHGD'
      },
      {
                'name': 'B',
        'order': 'YRUHQSLDPXNGOKMIEBFZCWVJAT'
      },
      {
                'name': 'C',
        'order': 'FVPJIAOYEDRZXWGCTKUQSBNMHL'
      }
    ]
  }";
            pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);
        }

        [Test]
        public void IOC()
        {
            indexOfCoincidence ioc = new indexOfCoincidence();
            string input = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.";
            double actual = ioc.getFitness(preProccessCiphertext(input));//https://www.dcode.fr/index-coincidence

            Assert.AreEqual(1.88, Math.Round(actual,2));
        }

        //Test other fitness functions

        private int[] preProccessCiphertext(string ciphertext)
        {
            string formattedInput = Regex.Replace(ciphertext.ToUpper(), @"[^A-Z]", string.Empty);
            int[] r = new int[formattedInput.Length];
            for (int i = 0; i < formattedInput.Length; i++)
            {
                r[i] = Convert.ToInt16(formattedInput[i]) - 65;
            }
            return r;
        }
    }
}
