using EngimaSimulator.Configuration.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;

namespace TestEncoding
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void MirrorForAllRotations()
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
            PhysicalConfiguration pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);

            const string qbfjold = "THEQU ICKBR OWNFO XJUMP SOVER THELA ZYDOG ";
            foreach (Rotor reflector in pc.reflectors)
            {
                foreach (Rotor left in pc.rotors)
                {
                    foreach (Rotor middle in pc.rotors)
                    {
                        if (middle.name != left.name)
                        {
                            foreach (Rotor right in pc.rotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                List<RotorModel> rotors = new List<RotorModel>();
                                                Random rnd = new Random();
                                                int lOffset = rnd.Next(26);
                                                int mOffset = rnd.Next(26);
                                                int rOffset = rnd.Next(26);
                                                rotors.Add(new RotorModel(left, l, lOffset));
                                                rotors.Add(new RotorModel(middle, m, mOffset));
                                                rotors.Add(new RotorModel(right, r, rOffset));
                                                EnigmaModel em = new EnigmaModel(rotors, new RotorModel(reflector), new Dictionary<int, int>());
                                                List<RotorModel> rotors2 = new List<RotorModel>();
                                                rotors2.Add(new RotorModel(left, l, lOffset));
                                                rotors2.Add(new RotorModel(middle, m, mOffset));
                                                rotors2.Add(new RotorModel(right, r, rOffset));
                                                EnigmaModel em2 = new EnigmaModel(rotors2, new RotorModel(reflector), new Dictionary<int, int>());
                                                EncodingService encodingService = new EncodingService();
                                                string outFirst = encodingService.encode(qbfjold, em);
                                                string outFromDoubleEncode = encodingService.encode(outFirst, em2);
                                                Assert.AreEqual(qbfjold, outFromDoubleEncode);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        [Test]
        public void MirrorForAllRingSettings()
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
            PhysicalConfiguration pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);

            const string qbfjold = "THEQU ICKBR OWNFO XJUMP SOVER THELA ZYDOG ";
            foreach (Rotor reflector in pc.reflectors)
            {
                foreach (Rotor left in pc.rotors)
                {
                    foreach (Rotor middle in pc.rotors)
                    {
                        if (middle.name != left.name)
                        {
                            foreach (Rotor right in pc.rotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                List<RotorModel> rotors = new List<RotorModel>();
                                                Random rnd = new Random();
                                                int lOffset = rnd.Next(26);
                                                int mOffset = rnd.Next(26);
                                                int rOffset = rnd.Next(26);
                                                rotors.Add(new RotorModel(left, lOffset, l));
                                                rotors.Add(new RotorModel(middle, mOffset, m));
                                                rotors.Add(new RotorModel(right, rOffset, r));
                                                EnigmaModel em = new EnigmaModel(rotors, new RotorModel(reflector), new Dictionary<int, int>());
                                                List<RotorModel> rotors2 = new List<RotorModel>();
                                                rotors2.Add(new RotorModel(left, lOffset, l));
                                                rotors2.Add(new RotorModel(middle, mOffset, m));
                                                rotors2.Add(new RotorModel(right, rOffset, r));
                                                EnigmaModel em2 = new EnigmaModel(rotors2, new RotorModel(reflector), new Dictionary<int, int>());
                                                EncodingService encodingService = new EncodingService();
                                                string outFirst = encodingService.encode(qbfjold, em);
                                                string outFromDoubleEncode = encodingService.encode(outFirst, em2);
                                                Assert.AreEqual(qbfjold, outFromDoubleEncode);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void TestAllPlugboardSettings()
        {
            for (int a = 0; a <= 25; a++)
            {
                for (int b = a + 1; b <= 25; b++)
                {
                    Dictionary<int, int> pb = new Dictionary<int, int>();
                    pb.Add(a, b);
                    EncodingService es = new EncodingService();
                    int actualA = es.plugboardSwap(pb, a);
                    int actualB = es.plugboardSwap(pb, b);
                    Assert.AreEqual(b, actualA);
                    Assert.AreEqual(a, actualB);
                }
            }
        }
        [Test]
        public void TestEncodeOneCharRingSettings() {
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
            PhysicalConfiguration pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);

            foreach (Rotor reflector in pc.reflectors)
            {
                foreach (Rotor left in pc.rotors)
                {
                    foreach (Rotor middle in pc.rotors)
                    {
                        if (middle.name != left.name)
                        {
                            foreach (Rotor right in pc.rotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                for (int i = 0; i <= 25; i++)
                                                {
                                                    List<RotorModel> rotors = new List<RotorModel>();
                                                    Random rnd = new Random();
                                                    int lOffset = rnd.Next(26);
                                                    int mOffset = rnd.Next(26);
                                                    int rOffset = rnd.Next(26);
                                                    rotors.Add(new RotorModel(left, lOffset, l));
                                                    rotors.Add(new RotorModel(middle, mOffset, m));
                                                    rotors.Add(new RotorModel(right, rOffset, r));
                                                    EnigmaModel em = new EnigmaModel(rotors, new RotorModel(reflector), new Dictionary<int, int>());
                                                    List<RotorModel> rotors2 = new List<RotorModel>();
                                                    rotors2.Add(new RotorModel(left, lOffset, l));
                                                    rotors2.Add(new RotorModel(middle, mOffset, m));
                                                    rotors2.Add(new RotorModel(right, rOffset, r));
                                                    EnigmaModel em2 = new EnigmaModel(rotors2, new RotorModel(reflector), new Dictionary<int, int>());
                                                    EncodingService encodingService = new EncodingService();
                                                    char c = Convert.ToChar(i + 65);
                                                    char outFirst = encodingService.encodeOneChar(em, c);
                                                    char outFromDoubleEncode = encodingService.encodeOneChar(em2, outFirst);
                                                    Assert.AreEqual(c, outFromDoubleEncode);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        [Test]
        public void TestEncodeOneCharRotation()
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
            PhysicalConfiguration pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);

            foreach (Rotor reflector in pc.reflectors)
            {
                foreach (Rotor left in pc.rotors)
                {
                    foreach (Rotor middle in pc.rotors)
                    {
                        if (middle.name != left.name)
                        {
                            foreach (Rotor right in pc.rotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                for (int i = 0; i <= 25; i++)
                                                {
                                                    List<RotorModel> rotors = new List<RotorModel>();
                                                    Random rnd = new Random();
                                                    int lOffset = rnd.Next(26);
                                                    int mOffset = rnd.Next(26);
                                                    int rOffset = rnd.Next(26);
                                                    rotors.Add(new RotorModel(left, l, lOffset));
                                                    rotors.Add(new RotorModel(middle, m, mOffset));
                                                    rotors.Add(new RotorModel(right, r, rOffset));
                                                    EnigmaModel em = new EnigmaModel(rotors, new RotorModel(reflector), new Dictionary<int, int>());
                                                    List<RotorModel> rotors2 = new List<RotorModel>();
                                                    rotors2.Add(new RotorModel(left, l, lOffset));
                                                    rotors2.Add(new RotorModel(middle, m, mOffset));
                                                    rotors2.Add(new RotorModel(right, r, rOffset));
                                                    EnigmaModel em2 = new EnigmaModel(rotors2, new RotorModel(reflector), new Dictionary<int, int>());
                                                    EncodingService encodingService = new EncodingService();
                                                    char c = Convert.ToChar(i + 65);
                                                    char outFirst = encodingService.encodeOneChar(em, c);
                                                    char outFromDoubleEncode = encodingService.encodeOneChar(em2, outFirst);
                                                    Assert.AreEqual(c, outFromDoubleEncode);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void TestEncodeAndInverse() {
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
            PhysicalConfiguration pc = JsonConvert.DeserializeObject<PhysicalConfiguration>(json);

            foreach (Rotor reflector in pc.reflectors)
            {
                for (int r = 0; r <= 25; r++)
                {
                    for (int o = 0; o <= 25; o++)
                    {
                        for (int i = 0; i <= 25; i++)
                        {
                            RotorModel rm = new RotorModel(reflector,r,o);
                            RotorModel rm2 = new RotorModel(reflector, r, o);
                            EncodingService encodingService = new EncodingService();
                            int outFirst = encodingService.rotorEncode(rm, i);
                            int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2,outFirst);
                            Assert.AreEqual(i, outFromDoubleEncode);
                        }
                    }
                }
            }
            foreach (Rotor rotors in pc.rotors)
            {
                for (int r = 0; r <= 25; r++)
                {
                    for (int o = 0; o <= 25; o++)
                    {
                        for (int i = 0; i <= 25; i++)
                        {
                            RotorModel rm = new RotorModel(rotors, r, o);
                            RotorModel rm2 = new RotorModel(rotors, r, o);
                            EncodingService encodingService = new EncodingService();
                            int outFirst = encodingService.rotorEncode(rm, i);
                            int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2, outFirst);
                            Assert.AreEqual(i, outFromDoubleEncode);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestStep() { }
        [Test]
        public void TestStepTurnOver2() { }
        [Test]
        public void TestStepTurnOver3() { }
    }
}