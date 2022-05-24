using EngimaSimulator.Configuration.Models;
using EnigmaBreaker.Services;
using EnigmaBreaker.Services.Fitness;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //https://planetcalc.com/7944/
            indexOfCoincidence ioc = new indexOfCoincidence();
            Dictionary<string,double> inputs = new Dictionary<string, double>()
            {
                {"abcdefghijklmnopqrstuvwxyz",0.0 },
                {"abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz",0.51 },
                {"a",26.0 },
                {"aa",26.0 },
                { "aaabbc",6.93},
                {"aab",8.67 }
            };            
            foreach(KeyValuePair<string,double> entry in inputs)
            {                
                double actual = ioc.getFitness(EncodingService.preProccessCiphertext(entry.Key));
                Assert.AreEqual(entry.Value, Math.Round(actual, 2));
            }
        }
        /*[Test]
        public void S()
        {

        }
        [Test]
        public void BI()
        {

        }
        [Test]
        public void TRI()
        {

        }
        [Test]
        public void QUAD()
        {

        }*/

        //Test Comparisons
        [Test]
        public void testRotorComparisonTrue()
        {
            foreach (Rotor refl in pc.reflectors)
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
                                                List<RotorModel> actualRotors = new List<RotorModel>();
                                                actualRotors.Add(new RotorModel(left, l));
                                                actualRotors.Add(new RotorModel(middle, m));
                                                actualRotors.Add(new RotorModel(right, r));
                                                EnigmaModel emActual = new EnigmaModel(actualRotors, new RotorModel(refl), new Dictionary<int, int>());

                                                for (int lv = -1; lv <= 1; lv++)
                                                {
                                                    for (int mv = -1; mv <= 1; mv++)
                                                    {
                                                        for (int rv = -1; rv <= 1; rv++)
                                                        {
                                                            List<RotorModel> attemptRotors = new List<RotorModel>();
                                                            attemptRotors.Add(new RotorModel(left, EncodingService.mod26(l + lv)));
                                                            attemptRotors.Add(new RotorModel(middle, EncodingService.mod26(m + mv)));
                                                            attemptRotors.Add(new RotorModel(right, EncodingService.mod26(r + rv)));
                                                            EnigmaModel emAttempt = new EnigmaModel(attemptRotors, new RotorModel(refl), new Dictionary<int, int>());

                                                            Assert.IsTrue(Measuring.compareRotors(emActual, emAttempt));

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
            }
        }
        [Test]
        public void testOffsetComparisonTrue()
        {
            foreach (Rotor refl in pc.reflectors)
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
                                    for (int lr = 0; lr <= 25; lr++)
                                    {
                                        for (int mr = 0; mr <= 25; mr++)
                                        {
                                            for (int rr = 0; rr <= 25; rr++)
                                            {
                                                for (int mo = 0; mo <= 25; mo++)
                                                {
                                                    for (int ro = 0; ro <= 26; ro++)
                                                    {
                                                        List<RotorModel> actualRotors = new List<RotorModel>();
                                                        actualRotors.Add(new RotorModel(left, lr, 0));
                                                        actualRotors.Add(new RotorModel(middle, mr, mo));
                                                        actualRotors.Add(new RotorModel(right, rr, ro));
                                                        EnigmaModel emActual = new EnigmaModel(actualRotors, new RotorModel(refl), new Dictionary<int, int>());

                                                        EnigmaModel emAttempt = new EnigmaModel(actualRotors, new RotorModel(refl), new Dictionary<int, int>());
                                                        Assert.IsTrue(Measuring.compareOffset(emActual, emAttempt));

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
        }
        [Test]
        public void testPlugboardComparisonTrue()
        {
            for (int aa = 0; aa < 25; aa++)
            {
                for (int ab = aa; ab < 25; ab++)
                {
                    if (ab != aa)
                    {
                        for (int ba = ab; ba < 25; ba++)
                        {
                            if (ba != aa && ba != ab)
                            {
                                for (int bb = ba; bb < 25; bb++)
                                {
                                    if (bb != aa && bb != ab && bb != ba)
                                    {
                                        for (int ca = bb; ca < 25; ca++)
                                        {
                                            if (ca != aa && ca != ab && ca != ba && ca != bb)
                                            {
                                                for (int cb = ca; cb < 25; cb++)
                                                {
                                                    if (cb != aa && cb != ab && cb != ba && cb != bb && cb != ca)
                                                    {
                                                        for (int da = cb; da < 25; da++)
                                                        {
                                                            if (da != aa && da != ab && da != ba && da != bb && da != ca && da != cb)
                                                            {
                                                                for (int db = da; db < 25; db++)
                                                                {
                                                                    if (db != aa && db != ab && db != ba && db != bb && db != ca && db != cb && db != da)
                                                                    {
                                                                        for (int ea = db; ea < 25; ea++)
                                                                        {
                                                                            if (ea != aa && ea != ab && ea != ba && ea != bb && ea != ca && ea != cb && ea != da && ea != db)
                                                                            {
                                                                                for (int eb = ea; eb < 25; eb++)
                                                                                {
                                                                                    if (eb != aa && eb != ab && eb != ba && eb != bb && eb != ca && eb != cb && eb != da && eb != db && eb != ea)
                                                                                    {
                                                                                        for (int fa = eb; fa < 25; fa++)
                                                                                        {
                                                                                            if (fa != aa && fa != ab && fa != ba && fa != bb && fa != ca && fa != cb && fa != da && fa != db && fa != ea && fa != eb)
                                                                                            {
                                                                                                for (int fb = fa; fb < 25; fb++)
                                                                                                {
                                                                                                    if (fb != aa && fb != ab && fb != ba && fb != bb && fb != ca && fb != cb && fb != da && fb != db && fb != ea && fb != eb && fb != fa)
                                                                                                    {
                                                                                                        for (int ga = fb; ga < 25; ga++)
                                                                                                        {
                                                                                                            if (ga != aa && ga != ab && ga != ba && ga != bb && ga != ca && ga != cb && ga != da && ga != db && ga != ea && ga != eb && ga != fa && ga != fb)
                                                                                                            {
                                                                                                                for (int gb = ga; gb < 25; gb++)
                                                                                                                {
                                                                                                                    if (gb != aa && gb != ab && gb != ba && gb != bb && gb != ca && gb != cb && gb != da && gb != db && gb != ea && gb != eb && gb != fa && gb != fb && gb != ga)
                                                                                                                    {
                                                                                                                        for (int ha = gb; ha < 25; ha++)
                                                                                                                        {
                                                                                                                            if (ha != aa && ha != ab && ha != ba && ha != bb && ha != ca && ha != cb && ha != da && ha != db && ha != ea && ha != eb && ha != fa && ha != fb && ha != ga && ha != gb)
                                                                                                                            {
                                                                                                                                for (int hb = ha; hb < 25; hb++)
                                                                                                                                {
                                                                                                                                    if (hb != aa && hb != ab && hb != ba && hb != bb && hb != ca && hb != cb && hb != da && hb != db && hb != ea && hb != eb && hb != fa && hb != fb && hb != ga && hb != gb && hb != ha)
                                                                                                                                    {
                                                                                                                                        for (int ia = hb; ia < 25; ia++)
                                                                                                                                        {
                                                                                                                                            if (ia != aa && ia != ab && ia != ba && ia != bb && ia != ca && ia != cb && ia != da && ia != db && ia != ea && ia != eb && ia != fa && ia != fb && ia != ga && ia != gb && ia != ha && ia != hb)
                                                                                                                                            {
                                                                                                                                                for (int ib = ia; ib < 25; ib++)
                                                                                                                                                {
                                                                                                                                                    if (ib != aa && ib != ab && ib != ba && ib != bb && ib != ca && ib != cb && ib != da && ib != db && ib != ea && ib != eb && ib != fa && ib != fb && ib != ga && ib != gb && ib != ha && ib != hb && ib != ia)
                                                                                                                                                    {
                                                                                                                                                        for (int ja = ib; ja < 25; ja++)
                                                                                                                                                        {
                                                                                                                                                            if (ja != aa && ja != ab && ja != ba && ja != bb && ja != ca && ja != cb && ja != da && ja != db && ja != ea && ja != eb && ja != fa && ja != fb && ja != ga && ja != gb && ja != ha && ja != hb && ja != ia && ja != ib)
                                                                                                                                                            {
                                                                                                                                                                for (int jb = ja; jb < 25; jb++)
                                                                                                                                                                {
                                                                                                                                                                    if (jb != aa && jb != ab && jb != ba && jb != bb && jb != ca && jb != cb && jb != da && jb != db && jb != ea && jb != eb && jb != fa && jb != fb && jb != ga && jb != gb && jb != ha && jb != hb && jb != ia && jb != ib && jb != ja)
                                                                                                                                                                    {
                                                                                                                                                                        string actualPlugboardString = "";
                                                                                                                                                                        Dictionary<int, int> actualPlugboard = new Dictionary<int, int>() { { aa, ab }, { ba, bb }, { ca, cb }, { da, db }, { ea, eb }, { fa, fb }, { ga, gb }, { ha, hb }, { ia, ib }, { ja, jb } };
                                                                                                                                                                        foreach (KeyValuePair<int, int> entry in actualPlugboard)//for all the entries in the dictionary
                                                                                                                                                                        {
                                                                                                                                                                            actualPlugboardString += $"{Convert.ToChar(entry.Key + 65)}{Convert.ToChar(entry.Value + 65)} ";//add key value chars
                                                                                                                                                                        }

                                                                                                                                                                        Random rand = new Random();
                                                                                                                                                                        Dictionary<int, int> attemptPlugboard = actualPlugboard.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);
                                                                                                                                                                        string attemptPlugboardString = "";
                                                                                                                                                                        foreach (KeyValuePair<int, int> entry in attemptPlugboard)//for all the entries in the dictionary
                                                                                                                                                                        {
                                                                                                                                                                            attemptPlugboardString += $"{Convert.ToChar(entry.Key + 65)}{Convert.ToChar(entry.Value + 65)} ";//add key value chars
                                                                                                                                                                        }

                                                                                                                                                                        Assert.IsTrue(Measuring.comparePlugboard(actualPlugboardString, attemptPlugboardString));

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
                                }
                            }
                        }
                    }
                }
            }
        }
        [Test]
        public void testRotorComparisonFalse()
        {
            int iterations = 100000;
            for (int i = 0; i < iterations; i++)
            {
                EnigmaModel actual = EnigmaModel.randomizeEnigma(pc);
                //actual.plugboard = new Dictionary<int, int>();
                actual.rotors[0].ringOffset = 0;
                actual.rotors[1].ringOffset = 0;
                actual.rotors[2].ringOffset = 0;
                string emJson = JsonConvert.SerializeObject(actual);

                EnigmaModel changeRefl = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//copy enigma
                foreach (Rotor refl in pc.reflectors)
                {
                    if (refl.name != actual.reflector.rotor.name)
                    {
                        RotorModel currentReflector = new RotorModel(refl);
                        changeRefl.reflector = currentReflector;
                        Assert.IsFalse(Measuring.compareRotors(actual, changeRefl));
                    }
                }

                EnigmaModel changeRotor = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//copy enigma
                for (int rIndex = 0; rIndex < 3; rIndex++)
                {
                    foreach (Rotor rotor in pc.rotors)
                    {
                        if (rotor.name != actual.rotors[rIndex].rotor.name)
                        {
                            RotorModel currentRotor = new RotorModel(rotor, changeRotor.rotors[rIndex].rotation);
                            changeRotor.rotors[rIndex] = currentRotor;
                            Assert.IsFalse(Measuring.compareRotors(actual, changeRotor));
                        }
                    }
                }

                EnigmaModel changeRotation = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//copy enigma
                for (int rIndex = 0; rIndex < 3; rIndex++)
                {
                    for (int rotationChange = 2; rotationChange < 24; rotationChange++)
                    {
                        changeRotation.rotors[rIndex].rotation = EncodingService.mod26(actual.rotors[rIndex].rotation - rotationChange);
                        Assert.IsFalse(Measuring.compareRotors(actual, changeRotation));
                    }
                }
            }
        }
        [Test]
        public void testOffsetComparisonFalse()
        {
            int iterations = 100000;
            for (int i = 0; i < iterations; i++)
            {
                EnigmaModel actual = EnigmaModel.randomizeEnigma(pc);
                actual.plugboard = new Dictionary<int, int>();                
                for (int lv = 1; lv <= 25; lv++)
                {
                    for (int mv = 1; mv <= 25; mv++)
                    {
                        for (int rv = 1; rv <= 25; rv++)
                        {
                            List<RotorModel> attemptRotors = new List<RotorModel>();
                            attemptRotors.Add(new RotorModel(actual.rotors[0].rotor, actual.rotors[0].rotation ,lv));
                            attemptRotors.Add(new RotorModel(actual.rotors[1].rotor, actual.rotors[1].rotation,EncodingService.mod26(actual.rotors[1].ringOffset + mv)));
                            attemptRotors.Add(new RotorModel(actual.rotors[2].rotor, actual.rotors[2].rotation,EncodingService.mod26(actual.rotors[2].ringOffset + rv)));
                            EnigmaModel emAttempt = new EnigmaModel(attemptRotors, actual.reflector,new Dictionary<int, int>());

                            Assert.IsFalse(Measuring.compareOffset(actual, emAttempt));

                        }
                    }
                }            
            }
        }
        [Test]
        public void testPlugboardComparisonFalse()
        {
            for (int aa = 0; aa < 25; aa++)
            {
                for (int ab = aa; ab < 25; ab++)
                {
                    if (ab != aa)
                    {
                        for (int ba = ab; ba < 25; ba++)
                        {
                            if (ba != aa && ba != ab)
                            {
                                for (int bb = ba; bb < 25; bb++)
                                {
                                    if (bb != aa && bb != ab && bb != ba)
                                    {
                                        for (int ca = bb; ca < 25; ca++)
                                        {
                                            if (ca != aa && ca != ab && ca != ba && ca != bb)
                                            {
                                                for (int cb = ca; cb < 25; cb++)
                                                {
                                                    if (cb != aa && cb != ab && cb != ba && cb != bb && cb != ca)
                                                    {
                                                        for (int da = cb; da < 25; da++)
                                                        {
                                                            if (da != aa && da != ab && da != ba && da != bb && da != ca && da != cb)
                                                            {
                                                                for (int db = da; db < 25; db++)
                                                                {
                                                                    if (db != aa && db != ab && db != ba && db != bb && db != ca && db != cb && db != da)
                                                                    {
                                                                        for (int ea = db; ea < 25; ea++)
                                                                        {
                                                                            if (ea != aa && ea != ab && ea != ba && ea != bb && ea != ca && ea != cb && ea != da && ea != db)
                                                                            {
                                                                                for (int eb = ea; eb < 25; eb++)
                                                                                {
                                                                                    if (eb != aa && eb != ab && eb != ba && eb != bb && eb != ca && eb != cb && eb != da && eb != db && eb != ea)
                                                                                    {
                                                                                        for (int fa = eb; fa < 25; fa++)
                                                                                        {
                                                                                            if (fa != aa && fa != ab && fa != ba && fa != bb && fa != ca && fa != cb && fa != da && fa != db && fa != ea && fa != eb)
                                                                                            {
                                                                                                for (int fb = fa; fb < 25; fb++)
                                                                                                {
                                                                                                    if (fb != aa && fb != ab && fb != ba && fb != bb && fb != ca && fb != cb && fb != da && fb != db && fb != ea && fb != eb && fb != fa)
                                                                                                    {
                                                                                                        for (int ga = fb; ga < 25; ga++)
                                                                                                        {
                                                                                                            if (ga != aa && ga != ab && ga != ba && ga != bb && ga != ca && ga != cb && ga != da && ga != db && ga != ea && ga != eb && ga != fa && ga != fb)
                                                                                                            {
                                                                                                                for (int gb = ga; gb < 25; gb++)
                                                                                                                {
                                                                                                                    if (gb != aa && gb != ab && gb != ba && gb != bb && gb != ca && gb != cb && gb != da && gb != db && gb != ea && gb != eb && gb != fa && gb != fb && gb != ga)
                                                                                                                    {
                                                                                                                        for (int ha = gb; ha < 25; ha++)
                                                                                                                        {
                                                                                                                            if (ha != aa && ha != ab && ha != ba && ha != bb && ha != ca && ha != cb && ha != da && ha != db && ha != ea && ha != eb && ha != fa && ha != fb && ha != ga && ha != gb)
                                                                                                                            {
                                                                                                                                for (int hb = ha; hb < 25; hb++)
                                                                                                                                {
                                                                                                                                    if (hb != aa && hb != ab && hb != ba && hb != bb && hb != ca && hb != cb && hb != da && hb != db && hb != ea && hb != eb && hb != fa && hb != fb && hb != ga && hb != gb && hb != ha)
                                                                                                                                    {
                                                                                                                                        for (int ia = hb; ia < 25; ia++)
                                                                                                                                        {
                                                                                                                                            if (ia != aa && ia != ab && ia != ba && ia != bb && ia != ca && ia != cb && ia != da && ia != db && ia != ea && ia != eb && ia != fa && ia != fb && ia != ga && ia != gb && ia != ha && ia != hb)
                                                                                                                                            {
                                                                                                                                                for (int ib = ia; ib < 25; ib++)
                                                                                                                                                {
                                                                                                                                                    if (ib != aa && ib != ab && ib != ba && ib != bb && ib != ca && ib != cb && ib != da && ib != db && ib != ea && ib != eb && ib != fa && ib != fb && ib != ga && ib != gb && ib != ha && ib != hb && ib != ia)
                                                                                                                                                    {
                                                                                                                                                        for (int ja = ib; ja < 25; ja++)
                                                                                                                                                        {
                                                                                                                                                            if (ja != aa && ja != ab && ja != ba && ja != bb && ja != ca && ja != cb && ja != da && ja != db && ja != ea && ja != eb && ja != fa && ja != fb && ja != ga && ja != gb && ja != ha && ja != hb && ja != ia && ja != ib)
                                                                                                                                                            {
                                                                                                                                                                for (int jb = ja; jb < 25; jb++)
                                                                                                                                                                {
                                                                                                                                                                    if (jb != aa && jb != ab && jb != ba && jb != bb && jb != ca && jb != cb && jb != da && jb != db && jb != ea && jb != eb && jb != fa && jb != fb && jb != ga && jb != gb && jb != ha && jb != hb && jb != ia && jb != ib && jb != ja)
                                                                                                                                                                    {
                                                                                                                                                                        string actualPlugboardString = "";
                                                                                                                                                                        Dictionary<int, int> actualPlugboard = new Dictionary<int, int>() { { aa, ab }, { ba, bb }, { ca, cb }, { da, db }, { ea, eb }, { fa, fb }, { ga, gb }, { ha, hb }, { ia, ib }, { ja, jb } };
                                                                                                                                                                        foreach (KeyValuePair<int, int> entry in actualPlugboard)//for all the entries in the dictionary
                                                                                                                                                                        {
                                                                                                                                                                            actualPlugboardString += $"{Convert.ToChar(entry.Key + 65)}{Convert.ToChar(entry.Value + 65)} ";//add key value chars
                                                                                                                                                                        }

                                                                                                                                                                        List<int> l = new List<int>() { aa, ab, ba, bb, ca, cb, da, db, ea, eb, fa, fb, ga, gb, ha, hb, ia, ib, ja, jb };
                                                                                                                                                                        Random rand = new Random();
                                                                                                                                                                        Dictionary<int, int> attemptPlugboard = new Dictionary<int, int>();
                                                                                                                                                                        int a = 2 * rand.Next(10);
                                                                                                                                                                        int b = 2 * rand.Next(10);
                                                                                                                                                                        attemptPlugboard.Add(l[a], l[b]);
                                                                                                                                                                        l.RemoveAt(a);
                                                                                                                                                                        l.RemoveAt(b);
                                                                                                                                                                        for (int i = 0; i < 9; i++)
                                                                                                                                                                        {
                                                                                                                                                                            attemptPlugboard.Add(l[0], l[1]);
                                                                                                                                                                            l.RemoveRange(0, 2);
                                                                                                                                                                        }
                                                                                                                                                                        attemptPlugboard = attemptPlugboard.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);
                                                                                                                                                                        string attemptPlugboardString = "";
                                                                                                                                                                        foreach (KeyValuePair<int, int> entry in attemptPlugboard)//for all the entries in the dictionary
                                                                                                                                                                        {
                                                                                                                                                                            attemptPlugboardString += $"{Convert.ToChar(entry.Key + 65)}{Convert.ToChar(entry.Value + 65)} ";//add key value chars
                                                                                                                                                                        }

                                                                                                                                                                        Assert.IsFalse(Measuring.comparePlugboard(actualPlugboardString, attemptPlugboardString));

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
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
