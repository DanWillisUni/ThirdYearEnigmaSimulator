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
        /// Tests encoding each character on every rotor rotation of random rotors
        /// </summary>
        [Test]
        public void EncodeOneCharRotation()
        {
            foreach (Rotor refl in pc.reflectors)
            {
                //for each combination of rotors
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
                                    //For every combinaiton of rotor rotation
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                for (int i = 0; i <= 25; i++)//for every char
                                                {
                                                    List<RotorModel> rotors = new List<RotorModel>();
                                                    Random rand = new Random();
                                                    rotors.Add(new RotorModel(left, l, rand.Next(26)));
                                                    rotors.Add(new RotorModel(middle, m, rand.Next(26)));
                                                    rotors.Add(new RotorModel(right, r, rand.Next(26)));
                                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomise enigma model
                                                    em.rotors = rotors;//set the rotors
                                                    em.reflector = new RotorModel(refl);
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
                            }
                        }
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
            foreach (Rotor refl in pc.reflectors)
            {
                //for each combination of rotors
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
                                    //for every offset ring setting combinaiton
                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                for (int i = 0; i <= 25; i++)
                                                {
                                                    List<RotorModel> rotors = new List<RotorModel>();
                                                    Random rand = new Random();
                                                    rotors.Add(new RotorModel(left, rand.Next(26),l));
                                                    rotors.Add(new RotorModel(middle, rand.Next(26),m));
                                                    rotors.Add(new RotorModel(right, rand.Next(26),r));
                                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomise enigma model
                                                    em.rotors = rotors;//set the rotors
                                                    em.reflector = new RotorModel(refl);

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
                            }
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
            foreach (Rotor reflector in pc.reflectors)
            {                
                for (int i = 0; i <= 25; i++)
                {
                    RotorModel rm = new RotorModel(reflector);
                    RotorModel rm2 = new RotorModel(reflector);
                    int outFirst = encodingService.rotorEncode(rm, i);
                    int outFromDoubleEncode = encodingService.rotorEncodeInverse(rm2, outFirst);
                    Assert.AreEqual(i, outFromDoubleEncode);
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
                for (int b = a + 1; b <= 25; b++)//for each letter above first
                {
                    Dictionary<int, int> pb1 = new Dictionary<int, int>();//create a new plugboard
                    pb1.Add(a, b);//add the pair to plugboard
                    Dictionary<int, int> pb2 = new Dictionary<int, int>();//create a new plugboard
                    pb2.Add(b, a);//add the pair to plugboard in reverse
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
            //for each rotor combination
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
                                for (int r = 0; r <= 25; r++)// for each rotoation
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    //randomise offset
                                    Random rand = new Random();
                                    rotors.Add(new RotorModel(left,0,rand.Next(26)));
                                    rotors.Add(new RotorModel(middle, 0, rand.Next(26)));
                                    rotors.Add(new RotorModel(right,r, rand.Next(26)));
                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomise enigma model
                                    em.rotors = rotors;//set the rotors
                                    em = encodingService.stepRotors(em);//step the rotors
                                    Assert.AreEqual((r + 1) % 26, em.rotors[2].rotation);//test the right rotor has moved one place
                                    if (right.turnoverNotchA == r)//if the rotor should have turned the second
                                    {
                                        Assert.AreEqual(1, em.rotors[1].rotation);//check the second rotor has turned
                                    }
                                    else//else
                                    {
                                        Assert.AreEqual(0, em.rotors[1].rotation);//check the second rotor hasn turned
                                    }
                                    Assert.AreEqual(0, em.rotors[0].rotation);//check the third  rotor hasnt changed
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Test stepping 2 rotors
        /// </summary>
        [Test]
        public void StepTurnOver2()
        {
            //for each rotor combinaiton
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
                                for (int m = 0; m <= 25; m++)//for each middle rotaiton
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    Random rand = new Random();//set random offset
                                    rotors.Add(new RotorModel(left, 0,rand.Next(26)));
                                    rotors.Add(new RotorModel(middle, m, rand.Next(26)));
                                    rotors.Add(new RotorModel(right, right.turnoverNotchA, rand.Next(26)));//set the rotation to turnove
                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomize enigma
                                    em.rotors = rotors;//set the rotors
                                    em = encodingService.stepRotors(em);//step the rotors
                                    Assert.AreEqual((right.turnoverNotchA + 1) % 26, em.rotors[2].rotation);//check the right rotor moved one
                                    Assert.AreEqual((m + 1) % 26, em.rotors[1].rotation);//check the middle rotor moved one
                                    if (middle.turnoverNotchA == m)//if the middle should have turned the third
                                    {
                                        Assert.AreEqual(1, em.rotors[0].rotation);//check the third turned
                                    }
                                    else//else
                                    {
                                        Assert.AreEqual(0, em.rotors[0].rotation);//check the third didnt turn
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Test stepping all 3 rotors
        /// </summary>
        [Test]
        public void StepTurnOver3()
        {
            //for each combination of rotors
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
                                for (int l = 0; l <= 25; l++)//for each left rotation position
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    Random rand = new Random();//set random offset
                                    rotors.Add(new RotorModel(left, l,rand.Next(26)));
                                    rotors.Add(new RotorModel(middle, middle.turnoverNotchA, rand.Next(26)));
                                    rotors.Add(new RotorModel(right, right.turnoverNotchA, rand.Next(26)));
                                    EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//randomize enigma
                                    em.rotors = rotors;//set rotors
                                    em = encodingService.stepRotors(em);//step
                                    Assert.AreEqual((right.turnoverNotchA + 1) % 26, em.rotors[2].rotation);//check right turned 1
                                    Assert.AreEqual((middle.turnoverNotchA + 1) % 26, em.rotors[1].rotation);//check middle turned 1
                                    Assert.AreEqual((l + 1) % 26, em.rotors[0].rotation);//check left turned 1
                                }
                                    
                            }
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Test invalid characters being removed
        /// </summary>
        [Test]
        public void InvalidCharacters()
        {
            string s = "";
            for(int i = 0;i<= 256; i++)//for all unicode
            {
                s += Convert.ToChar(i);//convert to char and add to string
            }
            EnigmaModel em = EnigmaModel.randomizeEnigma(pc);//random enigma
            string output = encodingService.encode(s,em);//encode
            Assert.AreEqual(52, output.Length);//check by length that it got rid of bad chars
        }
        /// <summary>
        /// Test the transformation of the string to an integer array and back
        /// </summary>
        [Test]
        public void TextTransformations()
        {
            for (int i = 0;i<= 1000000; i++)//run 1000000 times
            {
                string expected = "";
                for (int l = 0; l < 100; l++)//word length 100
                {
                    Random rand = new Random();
                    expected += Convert.ToChar(rand.Next(26) + 65);//randomize character
                }
                int[] preProcessed = EncodingService.preProccessCiphertext(expected);//get in array
                string actual = EncodingService.getStringFromIntArr(preProcessed);//get string back from int array
                Assert.AreEqual(expected, actual);//expected is equal to double transformed
            }            
        }
    }    
}