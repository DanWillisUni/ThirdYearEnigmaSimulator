using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;
using EnigmaBreaker.Services.Fitness;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EnigmaBreaker.Services
{
    public class BasicService
    {
        private readonly ILogger<BasicService> _logger;
        private readonly BasicConfiguration _bc;
        private readonly EncodingService _encodingService;
        private readonly IFitness.FitnessResolver _resolver;
        
        private List<Rotor> allRotors { get; set; }
        private List<Rotor> allReflectors { get; set; }
        public BasicService(ILogger<BasicService> logger, BasicConfiguration bc, EncodingService encodingService, IFitness.FitnessResolver fitnessResolver)
        {
            _logger = logger;
            _bc = bc;
            _encodingService = encodingService;
            _resolver = fitnessResolver;

            string Rotorjson = @"[
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
    ]";
            List<Rotor> rotors = JsonConvert.DeserializeObject<List<Rotor>>(Rotorjson);
            allRotors = rotors.GetRange(0, _bc.numberOfRotorsInUse);
            string Reflectorjson = @"[      
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
    ]";
            List<Rotor> reflectors = JsonConvert.DeserializeObject<List<Rotor>>(Reflectorjson);
            int reflectorStartIndex = 0;
            if (_bc.numberOfReflectorsInUse == 1)
            {
                reflectorStartIndex = 1;
            }
            allReflectors = reflectors.GetRange(reflectorStartIndex, _bc.numberOfReflectorsInUse);

        }

        public void root()
        {
            //new ciphertext and enigma
            string plaintext = getText();
            EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
            _logger.LogInformation($"Plaintext: {plaintext.Length}\n" + plaintext);
            _logger.LogInformation(toStringRotors(em) + "/" + toStringPlugboard(em));
            string ciphertext = _encodingService.encode(plaintext, em);
            _logger.LogInformation($"Ciphertext: {ciphertext.Length}\n" + ciphertext);
            int[] cipherArr = _encodingService.preProccessCiphertext(ciphertext);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();            
            //rotors
            List<BreakerResult> rotorResults = getRotorResults(cipherArr);
            //rotorOffset
            List<BreakerResult> offsetResults = getRotationOffsetResult(rotorResults,cipherArr);
            //plugboard
            List<BreakerResult> plugboardResults = new List<BreakerResult>();
            foreach (BreakerResult offsetResult in offsetResults)
            {
                plugboardResults.Add(getPlugboardResults(offsetResult, cipherArr));
            }
            plugboardResults = sortBreakerList(plugboardResults);

            string attemptPlaintext = _encodingService.encode(ciphertext, plugboardResults[0].enigmaModel);
            stopWatch.Stop();
            _logger.LogInformation($"Plaintext: \n{attemptPlaintext}");            
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            _logger.LogInformation("RunTime " + elapsedTime);
        }
        

        #region rotors
        private readonly object rotorListLock = new object();
        public List<BreakerResult> getRotorResults(int[] cipherArr,string fitnessStr = "")
        {
            if (fitnessStr == "") { fitnessStr = "IOC"; }
            IFitness fitness = _resolver(fitnessStr);
            List<BreakerResult> results = new List<BreakerResult>();
            List<string> rotorConfigurationsToCheck = new List<string>();

            foreach (Rotor refl in allReflectors)
            {
                foreach (Rotor left in allRotors)
                {
                    foreach (Rotor middle in allRotors)
                    {
                        if (middle.name != left.name)
                        {
                            foreach (Rotor right in allRotors)
                            {
                                if (left.name != right.name && middle.name != right.name)
                                {
                                    List<RotorModel> rotors = new List<RotorModel>();
                                    rotors.Add(new RotorModel(left));
                                    rotors.Add(new RotorModel(middle));
                                    rotors.Add(new RotorModel(right));
                                    rotorConfigurationsToCheck.Add(JsonConvert.SerializeObject(new EnigmaModel(rotors, new RotorModel(refl), new Dictionary<int, int>())));
                                }
                            }
                        }
                    }
                }
            }

            Parallel.For<List<BreakerResult>>(0, rotorConfigurationsToCheck.Count, () => new List<BreakerResult>(), (i, loop, threadResults) =>
            {
                threadResults.AddRange(getIndividualRotorResults(cipherArr, rotorConfigurationsToCheck[(int)i], fitness));
                return threadResults;
            },
            (threadResults) => { 
                    lock (rotorListLock)
                    {
                        results.AddRange(threadResults);
                    }
            });                 
            return sortBreakerList(results).GetRange(0, _bc.topRotorsToSearch);
        }
        public List<BreakerResult> getIndividualRotorResults(int[] cipherArr,string emStr,IFitness fitness)
        {
            List<BreakerResult> results = new List<BreakerResult>();
            double lowestResult = double.MinValue;
            for (int l = 0; l <= 25; l++)
            {
                for (int m = 0; m <= 25; m++)
                {
                    for (int r = 0; r <= 25; r++)
                    {
                        EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);
                        em.rotors[0].rotation = l;
                        em.rotors[1].rotation = m;
                        em.rotors[2].rotation = r;
                        int[] attemptPlainText = _encodingService.encode(cipherArr, em);
                        double rating = fitness.getFitness(attemptPlainText);

                        if (rating > lowestResult)
                        {
                            em.rotors[0].rotation = l;
                            em.rotors[1].rotation = m;
                            em.rotors[2].rotation = r;
                            BreakerResult br = new BreakerResult(attemptPlainText, rating, em);
                            bool stillSubtract = results.Count + 1 > _bc.topSingleRotors;
                            while (stillSubtract)
                            {
                                double nextLowest = rating;
                                BreakerResult lowest = null;
                                foreach (BreakerResult result in results)
                                {
                                    if (result.score <= lowestResult && lowest == null)
                                    {
                                        lowest = result;
                                    }
                                    else if (result.score < nextLowest)
                                    {
                                        nextLowest = result.score;
                                    }
                                }
                                results.Remove(lowest);
                                lowestResult = nextLowest;
                                if (results.Count + 1 <= _bc.topSingleRotors || lowest == null)
                                {
                                    stillSubtract = false;
                                }
                            }
                            results.Add(br);
                        }
                    }
                }
            }
            return results;
        }
        #endregion

        #region offset and Rotation
        private readonly object offsetListLock = new object();

        public List<BreakerResult> getRotationOffsetResult(List<BreakerResult> breakerResults,int[] cipherArr,string fitnessStr = "")
        {
            if (fitnessStr == "") { fitnessStr = "IOC"; }
            IFitness fitness = _resolver(fitnessStr);
            List<BreakerResult> results = new List<BreakerResult> ();
            List<string> offsetConfigurationsToCheck = new List<string>();
            foreach (BreakerResult br in breakerResults)
            {
                int lbase = br.enigmaModel.rotors[0].rotation;
                int mbase = br.enigmaModel.rotors[1].rotation;
                int rbase = br.enigmaModel.rotors[2].rotation;
                for (int lchange = -1; lchange < 2; lchange++)
                {
                    br.enigmaModel.rotors[0].rotation = lbase + lchange;
                    for (int mchange = -1; mchange < 2; mchange++)
                    {
                        br.enigmaModel.rotors[1].rotation = mbase + mchange;
                        for (int rchange = -1; rchange < 2; rchange++)
                        {
                            br.enigmaModel.rotors[2].rotation = rbase + rchange;
                            offsetConfigurationsToCheck.Add(toStringRotors(br.enigmaModel));
                        }
                    }
                }
            }

            Parallel.For<List<BreakerResult>>(0, offsetConfigurationsToCheck.Count, () => new List<BreakerResult>(), (i, loop, threadResults) =>
            {
                threadResults.AddRange(getOffsetResultPerChange(cipherArr, fitness, offsetConfigurationsToCheck[(int)i]));
                return threadResults;
            },
            (threadResults) => {
                lock (offsetListLock)
                {
                    results.AddRange(threadResults);
                }
            });

            results = sortBreakerList(results);
            if (results.Count > _bc.topAllRotorRotationAndOffset)
            {
                results=results.GetRange(0, _bc.topAllRotorRotationAndOffset);
            }
            return results;
        }
        public List<BreakerResult> getOffsetResultPerChange(int[] cipherArr, IFitness fitness, string currentRotors)
        {
            List<string> rotorNames = new List<string>();
            List<string> rotorDetails = new List<string>();
            foreach (string s in currentRotors.Split("/"))
            {
                if (s.Contains(","))
                {
                    rotorNames.Add(s.Split(",")[0]);
                    rotorDetails.Add(s);
                }
            }
            RotorModel emRefl = null;
            foreach (Rotor refl in allReflectors)
            {
                if (refl.name == currentRotors.Split("/")[0])
                {
                    emRefl = new RotorModel(refl);
                }
            }
            List<Rotor> emRotors = new List<Rotor>();
            foreach (string name in rotorNames)
            {
                foreach (Rotor rotor in allRotors)
                {
                    if (name == rotor.name)
                    {
                        emRotors.Add(rotor);
                    }
                }
            }

            int lbase = Convert.ToInt16(rotorDetails[0].Split(",")[1]);
            int mbase = Convert.ToInt16(rotorDetails[1].Split(",")[1]);
            int rbase = Convert.ToInt16(rotorDetails[2].Split(",")[1]);

            double lowestResult = 0.0;
            List<BreakerResult> results = new List<BreakerResult>();
            for (int m = 0; m < 26; m++)
            {
                for (int r = 0; r < 26; r++)
                {
                    List<RotorModel> emRotorModel = new List<RotorModel>();
                    emRotorModel.Add(new RotorModel(emRotors[0], lbase, 0));
                    emRotorModel.Add(new RotorModel(emRotors[1], m, EncodingService.mod26(m - mbase)));
                    emRotorModel.Add(new RotorModel(emRotors[2], r, EncodingService.mod26(r - rbase)));
                    EnigmaModel em = new EnigmaModel(emRotorModel, emRefl, new Dictionary<int, int>());

                    int[] attemptPlainText = _encodingService.encode(cipherArr, em);
                    double rating = fitness.getFitness(attemptPlainText);

                    if (rating > lowestResult)
                    {
                        em.rotors[0].rotation = lbase;
                        em.rotors[1].rotation = m;
                        em.rotors[2].rotation = r;
                        BreakerResult br = new BreakerResult(attemptPlainText, rating, em);
                        bool stillSubtract = results.Count + 1 > _bc.topSingleVarianceRotationAndOffset;
                        while (stillSubtract)
                        {
                            double nextLowest = rating;
                            BreakerResult lowest = null;
                            foreach (BreakerResult result in results)
                            {
                                if (result.score <= lowestResult && lowest == null)
                                {
                                    lowest = result;
                                }
                                else if (result.score < nextLowest)
                                {
                                    nextLowest = result.score;
                                }
                            }
                            results.Remove(lowest);
                            lowestResult = nextLowest;
                            if(results.Count + 1 <= _bc.topSingleVarianceRotationAndOffset || lowest == null)
                            {
                                stillSubtract = false;
                            }
                        }
                        results.Add(br);
                    }
                }
            }
            
            return results;
        }
        #endregion

        #region plugboard
        public BreakerResult getPlugboardResults(BreakerResult br, int[] cipherArr, string fitnessStr = "")
        {
            if (fitnessStr == "")
            {
                fitnessStr = "IOC";
                if (cipherArr.Length < 300)
                {
                    if (cipherArr.Length < 200)
                    {
                        fitnessStr = "TRI";
                    }
                    else
                    {
                        fitnessStr = "QUAD";
                    }
                }
            }
            IFitness fitness = _resolver(fitnessStr);
            
            List<BreakerResult> allPlugboardResults = new List<BreakerResult>();
            List<BreakerResult> onePairResults = new List<BreakerResult>() { br };
            while (onePairResults[0].enigmaModel.plugboard.Count < _bc.maxPlugboardSettings)
            {
                foreach(BreakerResult opr in onePairResults)
                {
                    onePairResults = onePairPlugboard(opr, cipherArr, fitness);
                }                
                allPlugboardResults.AddRange(onePairResults);
            }
            allPlugboardResults = sortBreakerList(allPlugboardResults);
            return allPlugboardResults[0];
        }

        public List<BreakerResult> onePairPlugboard(BreakerResult br,int[] cipherArr,IFitness fitness)
        {
            List<int> ignoreCurrent = new List<int>();
            foreach (KeyValuePair<int, int> entry in br.enigmaModel.plugboard)
            {
                ignoreCurrent.Add(entry.Key);
                ignoreCurrent.Add(entry.Value);
            }
            string emJson = JsonConvert.SerializeObject(br.enigmaModel);

            List<BreakerResult> results = new List<BreakerResult>();
            for(int a = 0;a < 25; a++)
            {
                if (!ignoreCurrent.Contains(a))
                {
                    for (int b = a + 1; b < 26; b++)
                    {
                        if (!ignoreCurrent.Contains(b))
                        {
                            EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                            em.plugboard.Add(a, b);
                            int[] attemptPlainText = _encodingService.encode(cipherArr, em);
                            double rating = fitness.getFitness(attemptPlainText);
                            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                            em2.plugboard.Add(a, b);
                            results.Add(new BreakerResult(attemptPlainText,rating,em2));
                        }
                    }
                }                
            }
            return sortBreakerList(results).GetRange(0,_bc.topSinglePlugboardPairs);
        }
        #endregion

        #region helper
        public List<BreakerResult> sortBreakerList(List<BreakerResult> input)
        {
            List<BreakerResult> r = new List<BreakerResult>();
            while (input.Count > 0)
            {
                double highestScore = Double.MinValue;
                foreach(BreakerResult result in input)
                {
                    if (result.score > highestScore)
                    {
                        highestScore = result.score;
                    }
                }
                foreach (BreakerResult result in input)
                {
                    if (result.score == highestScore)
                    {
                        r.Add(result);
                    }
                }
                input.Remove(r[r.Count-1]);
            }
            return r;
        }
        public string toStringRotors(EnigmaModel enigmaModel)
        {
            string r = enigmaModel.reflector.rotor.name;
            foreach (RotorModel rotor in enigmaModel.rotors)
            {
                r += "/";
                r += rotor.rotor.name + ",";
                r += rotor.rotation + ",";
                r += rotor.ringOffset;
            }
            return r;
        }
        public string toStringPlugboard(EnigmaModel enigmaModel)
        {
            string r = "";
            foreach(KeyValuePair<int,int> entry in enigmaModel.plugboard)
            {
                if(entry.Key < entry.Value)
                {
                    r += Convert.ToChar(entry.Key + 65);
                    r += Convert.ToChar(entry.Value + 65);
                }
                else
                {                    
                    r += Convert.ToChar(entry.Value + 65);
                    r += Convert.ToChar(entry.Key + 65);
                }                
                r += " ";
            }
            return r;
        }
        public bool comparePlugboard(string actual,string attempt)
        {
            if (actual.Length == attempt.Length)
            {
                foreach (string a in actual.Split(" "))
                {
                    bool found = false;
                    foreach (string r in attempt.Split(" "))
                    {
                        if (r == a)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public string getText(int length = -1)
        {
            Random rnd = new Random();
            string r = System.IO.File.ReadAllText(System.IO.Path.Combine(_bc.textDir, _bc.textFileNames[rnd.Next(_bc.textFileNames.Count)] + ".txt"));
            if (length != -1)
            {
                while (r.Length < length)
                {
                    r += System.IO.File.ReadAllText(System.IO.Path.Combine(_bc.textDir, _bc.textFileNames[rnd.Next(_bc.textFileNames.Count)] + ".txt"));
                }
                r = r.Substring(0, length);
            }
            return r;
        }
        #endregion
    }
}
