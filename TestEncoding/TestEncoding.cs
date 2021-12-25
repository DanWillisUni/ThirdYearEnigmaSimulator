using EngimaSimulator.Configuration.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    public class Tests
    {
        PhysicalConfiguration pc { get; set; }
        EncodingService encodingService { get; set; }

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

            encodingService = new EncodingService();
        }

        [Test]
        public void MirrorForAllRotations()
        {
            const string qbfjold = "THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG";
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
            const string qbfjold = "THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG";
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
        public void EncodeOneCharRingSettings()
        {
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
        public void EncodeOneCharRotation()
        {
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
        public void EncodeAndInverse()
        {
            foreach (Rotor reflector in pc.reflectors)
            {
                for (int r = 0; r <= 25; r++)
                {
                    for (int o = 0; o <= 25; o++)
                    {
                        for (int i = 0; i <= 25; i++)
                        {
                            RotorModel rm = new RotorModel(reflector, r, o);
                            RotorModel rm2 = new RotorModel(reflector, r, o);
                            int outFirst = encodingService.rotorEncode(rm, i);
                            int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2, outFirst);
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
                            int outFirst = encodingService.rotorEncode(rm, i);
                            int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2, outFirst);
                            Assert.AreEqual(i, outFromDoubleEncode);
                        }
                    }
                }
            }
        }


        [Test]
        public void AllPlugboardSettings()
        {
            for (int a = 0; a <= 25; a++)
            {
                for (int b = a + 1; b <= 25; b++)
                {
                    Dictionary<int, int> pb = new Dictionary<int, int>();
                    pb.Add(a, b);
                    for (int i = 0; i <= 25; i++)
                    {
                        int expectedResult = i;
                        if (i == a)
                        {
                            expectedResult = b;
                        }
                        else if (i == b)
                        {
                            expectedResult = a;
                        }
                        int actual = encodingService.plugboardSwap(pb, i);
                        Assert.AreEqual(expectedResult, actual);
                    }
                }
            }
        }

        [Test]
        public void Step()
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
                                List<int> indexOfNotches = new List<int>();
                                foreach (char c in right.turnoverNotches)
                                {
                                    indexOfNotches.Add(right.order.IndexOf(c));
                                }
                                for (int r = 0; r <= 25; r++)
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    rotors.Add(new RotorModel(left));
                                    rotors.Add(new RotorModel(middle));
                                    rotors.Add(new RotorModel(right, r));
                                    EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
                                    em = encodingService.stepRotors(em);
                                    Assert.AreEqual((r + 1) % 26, em.rotors[2].rotation);
                                    if (indexOfNotches.Contains(r))
                                    {
                                        Assert.AreEqual(1, em.rotors[1].rotation);
                                    }
                                    else
                                    {
                                        Assert.AreEqual(0, em.rotors[1].rotation);
                                    }
                                    Assert.AreEqual(0, em.rotors[0].rotation);
                                }
                            }
                        }
                    }
                }
            }
        }
        [Test]
        public void StepTurnOver2()
        {
            foreach (Rotor left in pc.rotors)
            {
                foreach (Rotor middle in pc.rotors)
                {
                    if (middle.name != left.name)
                    {
                        List<int> indexOfNotches = new List<int>();
                        foreach (char c in middle.turnoverNotches)
                        {
                            indexOfNotches.Add(middle.order.IndexOf(c));
                        }
                        foreach (Rotor right in pc.rotors)
                        {
                            if (left.name != right.name && middle.name != right.name)
                            {
                                foreach (char notch in right.turnoverNotches)
                                {
                                    int r = right.order.IndexOf(notch);
                                    for (int m = 0; m <= 25; m++)
                                    {
                                        List<RotorModel> rotors = new List<RotorModel>();
                                        rotors.Add(new RotorModel(left, 0));
                                        rotors.Add(new RotorModel(middle, m));
                                        rotors.Add(new RotorModel(right, r));
                                        EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
                                        em = encodingService.stepRotors(em);
                                        Assert.AreEqual((r + 1) % 26, em.rotors[2].rotation);
                                        Assert.AreEqual((m + 1) % 26, em.rotors[1].rotation);
                                        if (indexOfNotches.Contains(m))
                                        {
                                            Assert.AreEqual(1, em.rotors[0].rotation);
                                        }
                                        else
                                        {
                                            Assert.AreEqual(0, em.rotors[0].rotation);
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
        public void StepTurnOver3()
        {
            foreach (Rotor left in pc.rotors)
            {
                foreach (Rotor middle in pc.rotors)
                {
                    if (middle.name != left.name)
                    {
                        foreach (char mNotch in middle.turnoverNotches)
                        {
                            int m = middle.order.IndexOf(mNotch);
                            foreach (Rotor right in pc.rotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    foreach (char notch in right.turnoverNotches)
                                    {
                                        int r = right.order.IndexOf(notch);
                                        for (int l = 0; l <= 25; l++)
                                        {
                                            List<RotorModel> rotors = new List<RotorModel>();
                                            rotors.Add(new RotorModel(left, l));
                                            rotors.Add(new RotorModel(middle, m));
                                            rotors.Add(new RotorModel(right, r));
                                            EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
                                            em = encodingService.stepRotors(em);
                                            Assert.AreEqual((r + 1) % 26, em.rotors[2].rotation);
                                            Assert.AreEqual((m + 1) % 26, em.rotors[1].rotation);
                                            Assert.AreEqual((l + 1) % 26, em.rotors[0].rotation);
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
        public void InvalidCharacters()
        {
            string s = "";
            for(int i = 0;i<= 256; i++)
            {
                s += Convert.ToChar(i);
            }
            List<RotorModel> rotors = new List<RotorModel>();
            rotors.Add(new RotorModel(pc.rotors[0]));
            EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
            string output = encodingService.encode(s,em);
            Assert.AreEqual(52, output.Length);
        }
    }    
}