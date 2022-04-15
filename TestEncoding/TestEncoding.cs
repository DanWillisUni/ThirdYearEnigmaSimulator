using EngimaSimulator.Configuration.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    public class TestEncoding
    {
        PhysicalConfiguration pc { get; set; }
        EncodingService encodingService { get; set; }
        /// <summary>
        /// Sets up the Physical configuration and encoding service for all the tests
        /// </summary>
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

            encodingService = new EncodingService();
        }

        /// <summary>
        /// Double encodes a pangram with all possible rotations
        /// </summary>
        [Test]
        public void MirrorForAllRotations()
        {
            const string qbfjold = "THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG";            
            for (int l = 0; l <= 25; l++)
            {
                for (int m = 0; m <= 25; m++)
                {
                    for (int r = 0; r <= 25; r++)
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(pc);
                        em.rotors[0].rotation = l;
                        em.rotors[1].rotation = m;
                        em.rotors[2].rotation = r;
                        string json = JsonConvert.SerializeObject(em);//deep copy
                        EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(json);
                        string outFirst = encodingService.encode(qbfjold, em);//use first enigma model to do 1 encoding
                        string outFromDoubleEncode = encodingService.encode(outFirst, em2);//put the output of the first encoding into the second and use the copy of the enigma model
                        Assert.AreEqual(qbfjold, outFromDoubleEncode);//check the output from the second encoding and the input of the first are the same
                    }
                }
            }
        }
        /// <summary>
        /// Double encode a pangram with all possible rotor ring offsets
        /// </summary>
        [Test]
        public void MirrorForAllRingSettings()
        {
            const string qbfjold = "THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG";
            for (int l = 0; l <= 25; l++)
            {
                for (int m = 0; m <= 25; m++)
                {
                    for (int r = 0; r <= 25; r++)
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//create a random enigma as we only care about the offset
                        //set each offset
                        em.rotors[0].ringOffset = l;
                        em.rotors[1].ringOffset = m;
                        em.rotors[2].ringOffset = r;
                        string json = JsonConvert.SerializeObject(em);//deep copy
                        EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(json);
                        string outFirst = encodingService.encode(qbfjold, em);//use first enigma model to do 1 encoding
                        string outFromDoubleEncode = encodingService.encode(outFirst, em2);//put the output of the first encoding into the second and use the copy of the enigma model
                        Assert.AreEqual(qbfjold, outFromDoubleEncode);//check the output from the second encoding and the input of the first are the same
                    }
                }
            }
        }
        /// <summary>
        /// Tests encoding each individual character on every ringsetting of random rotors
        /// </summary>
        [Test]
        public void EncodeOneCharRingSettings()
        {
            //for every ring setting
            for (int l = 0; l <= 25; l++)
            {
                for (int m = 0; m <= 25; m++)
                {
                    for (int r = 0; r <= 25; r++)
                    {
                        for (int i = 0; i <= 25; i++)
                        {
                            EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//create a random enigma model
                            //set the offset
                            em.rotors[0].ringOffset = l;
                            em.rotors[1].ringOffset = m;
                            em.rotors[2].ringOffset = r;
                            string json = JsonConvert.SerializeObject(em);//deep copy
                            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(json);
                            int outFirst = encodingService.encodeOneChar(em, i);//encode on char
                            int outFromDoubleEncode = encodingService.encodeOneChar(em2, outFirst);//encode out from first encode
                            Assert.AreEqual(i, outFromDoubleEncode);//check the output from the second encoding and the input of the first are the same
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Tests encoding each character on every rotor rotation of random rotors
        /// </summary>
        [Test]
        public void EncodeOneCharRotation()
        {
            //For every combinaiton of rotor rotation
            for (int l = 0; l <= 25; l++)
            {
                for (int m = 0; m <= 25; m++)
                {
                    for (int r = 0; r <= 25; r++)
                    {
                        for (int i = 0; i <= 25; i++)//for every char
                        {
                            EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomise enigma
                            //set the rotations
                            em.rotors[0].rotation = l;
                            em.rotors[1].rotation = m;
                            em.rotors[2].rotation = r;
                            string json = JsonConvert.SerializeObject(em);//deep copy
                            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(json);
                            int outFirst = encodingService.encodeOneChar(em, i);//encode one char
                            int outFromDoubleEncode = encodingService.encodeOneChar(em2, outFirst);//encode output from first with copy
                            Assert.AreEqual(i, outFromDoubleEncode);//check the output from the second encoding and the input of the first are the same
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Test encode and inverse are opposites
        /// </summary>
        [Test]
        public void EncodeAndInverse()
        {            
            foreach (Rotor rotor in pc.rotors)//for each rotor
            {
                for (int r = 0; r <= 25; r++)//for each  rotation
                {
                    for (int o = 0; o <= 25; o++)//for each offset
                    {
                        for (int i = 0; i <= 25; i++)//for each character
                        {
                            RotorModel rm = new RotorModel(rotor, r, o);//create rotor
                            RotorModel rm2 = new RotorModel(rotor, r, o);//copy
                            int outFirst = encodingService.rotorEncode(rm, i);//encode
                            int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2, outFirst);//encode inverse
                            Assert.AreEqual(i, outFromDoubleEncode);//is the out from the inverse the same as the input
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

        /// <summary>
        /// Test all plugboard pairs swap as intended
        /// </summary>
        [Test]
        public void AllPlugboardLetters()
        {            
            for (int a = 0; a <= 25; a++)//for each letter
            {
                for (int b = a + 1; b <= 25; b++)//for each letter above a
                {
                    Dictionary<int, int> pb1 = new Dictionary<int, int>();//create a new plugboard
                    pb1.Add(a, b);//add the pair to plugboard
                    Dictionary<int, int> pb2 = new Dictionary<int, int>();//create a new plugboard
                    pb2.Add(b, a);//add the pair to plugboard
                    for (int i = 0; i <= 25; i++)//for every letter
                    {
                        int expectedResult = i == a ? b : i == b?a:i;//set expected result                        
                        int actual1 = encodingService.plugboardSwap(pb1, i);//perform plugboard swap
                        int actual2 = encodingService.plugboardSwap(pb2, i);//perform plugboard swap
                        Assert.AreEqual(expectedResult, actual1);//test highest last
                        Assert.AreEqual(expectedResult, actual2);//test highest first
                    }
                }
            }
        }

        /// <summary>
        /// Test Step rotors 1 position
        /// </summary>
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
                                for (int r = 0; r <= 25; r++)
                                {
                                    //TODO look at offset here
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    rotors.Add(new RotorModel(left));
                                    rotors.Add(new RotorModel(middle));
                                    rotors.Add(new RotorModel(right, r));
                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);
                                    em.rotors = rotors;
                                    em = encodingService.stepRotors(em);
                                    Assert.AreEqual((r + 1) % 26, em.rotors[2].rotation);
                                    if (right.turnoverNotchA == r)
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
                        foreach (Rotor right in pc.rotors)
                        {
                            if (left.name != right.name && middle.name != right.name)
                            {
                                for (int m = 0; m <= 25; m++)
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    rotors.Add(new RotorModel(left, 0));
                                    rotors.Add(new RotorModel(middle, m));
                                    rotors.Add(new RotorModel(right, right.turnoverNotchA));
                                    EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
                                    em = encodingService.stepRotors(em);
                                    Assert.AreEqual((right.turnoverNotchA + 1) % 26, em.rotors[2].rotation);
                                    Assert.AreEqual((m + 1) % 26, em.rotors[1].rotation);
                                    if (middle.turnoverNotchA == m)
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
        [Test]
        public void StepTurnOver3()
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
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    rotors.Add(new RotorModel(left, l));
                                    rotors.Add(new RotorModel(middle, middle.turnoverNotchA));
                                    rotors.Add(new RotorModel(right, right.turnoverNotchA));
                                    EnigmaModel em = new EnigmaModel(rotors, new RotorModel(pc.reflectors[0]), new Dictionary<int, int>());
                                    em = encodingService.stepRotors(em);
                                    Assert.AreEqual((right.turnoverNotchA + 1) % 26, em.rotors[2].rotation);
                                    Assert.AreEqual((middle.turnoverNotchA + 1) % 26, em.rotors[1].rotation);
                                    Assert.AreEqual((l + 1) % 26, em.rotors[0].rotation);
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
            for(int i = 0;i<= 256; i++)//for all unicode
            {
                s += Convert.ToChar(i);//convert to char and add to string
            }
            EnigmaModel em = EnigmaModel.randomizeEnigma(pc);
            string output = encodingService.encode(s,em);
            Assert.AreEqual(52, output.Length);
        }
        [Test]
        public void TextTransformations()
        {
            for (int i = 0;i<= 1000; i++)
            {
                string expected = "";
                for (int l = 0; l <= 100; l++)
                {
                    Random rand = new Random();
                    expected += Convert.ToChar(rand.Next(26) + 65);
                }
                int[] preProcessed = EncodingService.preProccessCiphertext(expected);
                string actual = EncodingService.getStringFromIntArr(preProcessed);
                Assert.AreEqual(expected, actual);
            }            
        }
    }    
}