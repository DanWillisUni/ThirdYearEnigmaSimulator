using EngimaSimulator.Configuration.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;

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
        'turnoverNotches': [ 'Q' ]

      },
      {
                'name': 'II',
        'order': 'AJDKSIRUXBLHWTMCQGZNPYFVOE',
        'turnoverNotches': [ 'E' ]
      },
      {
                'name': 'III',
        'order': 'BDFHJLCPRTXVZNYEIWGAKMUSQO',
        'turnoverNotches': [ 'V' ]
      },
      {
                'name': 'IV',
        'order': 'ESOVPZJAYQUIRHXLNFTGKDCMWB',
        'turnoverNotches': [ 'J' ]
      },
      {
                'name': 'V',
        'order': 'VZBRGITYUPSDNHLXAWMJQOFECK',
        'turnoverNotches': [ 'Z' ]
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


    }
}
