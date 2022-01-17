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

        /// <summary>
        /// Randomly select a plaintext
        /// Randomly generate an Enigma configuration
        /// Encrypt the plaintext with the Engma configuration
        /// 
        /// Break the ciphertext and print the attempt at plaintext
        /// Print the time taken
        /// </summary>
        public void root()            
        {            
            string plaintext = getText();//get a random plaintext as string
            EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//get a new random enigma model
            _logger.LogInformation($"Plaintext: {plaintext.Length}\n" + plaintext);//print the plaintext
            _logger.LogInformation(em.ToString());//print the enigma model
            string ciphertext = _encodingService.encode(plaintext, em);//get the ciphertext
            _logger.LogInformation($"Ciphertext: {ciphertext.Length}\n" + ciphertext);//print the ciphertext
            int[] cipherArr = _encodingService.preProccessCiphertext(ciphertext);//convert the ciphertext into an array of integers

            Stopwatch timer = new Stopwatch();//create new timer
            timer.Start();//start timer
            List<BreakerResult> rotorResults = getRotorResults(cipherArr);//get the top results for rotor configurations
            List<BreakerResult> offsetResults = getRotationOffsetResult(rotorResults,cipherArr);//using the top rotor results get the top offset settings
            BreakerResult plugboardResult = getPlugboardResults(offsetResults, cipherArr);//using the top offset settings get the top plugboard setting
            
            string attemptPlaintext = _encodingService.encode(ciphertext, plugboardResult.enigmaModel);//decode the ciphertext using the suggested enigma configuration
            timer.Stop();//stop the timer
            _logger.LogInformation($"Plaintext: \n{attemptPlaintext}");//print the plaintext       
            TimeSpan ts = timer.Elapsed;//get the elapsed time as a TimeSpan value
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds);//format the timespan value
            _logger.LogInformation("Run time: " + elapsedTime);//print the time taken to crack the enigma
        }        

        #region rotors
        private readonly object rotorListLock = new object();//mutex object for multi threading
        /// <summary>
        /// Gets all the rotor configurations that need to be checked into list
        /// Loops though the configurations to check on multipule threads
        /// </summary>
        /// <param name="cipherArr"></param>
        /// Ciphertext as an array of integers
        /// <param name="fitnessStr"></param>
        /// (Optional and for testing purposes) The fitness function resolver string to use
        /// <returns>Top few rotor configurations</returns>
        public List<BreakerResult> getRotorResults(int[] cipherArr,string fitnessStr = "")
        {
            if (fitnessStr == "") { fitnessStr = "IOC"; }//if the fitness string is unspecified set it to default
            IFitness fitness = _resolver(fitnessStr);//get the fitness function from the resolver string
            List<BreakerResult> results = new List<BreakerResult>();//create new results list
            List<string> rotorConfigurationsToCheck = new List<string>();//create new list for the rotor combinations to check

            foreach (Rotor refl in allReflectors)//for all reflectors
            {
                foreach (Rotor left in allRotors)//for all rotors
                {
                    foreach (Rotor middle in allRotors)//for all rotors
                    {
                        if (middle.name != left.name)//if the middle rotor is not equal to the left rotor
                        {
                            foreach (Rotor right in allRotors)//for all rotors
                            {
                                if (left.name != right.name && middle.name != right.name)//if the right rotor is not equal to the left or the middle one
                                {
                                    List<RotorModel> rotors = new List<RotorModel>() { new RotorModel(left), new RotorModel(middle), new RotorModel(right) };//set the rotor list for the enigma model
                                    rotorConfigurationsToCheck.Add(JsonConvert.SerializeObject(new EnigmaModel(rotors, new RotorModel(refl), new Dictionary<int, int>())));//add the serilised string of the enigma settings to the settings to check list
                                }
                            }
                        }
                    }
                }
            }

            Parallel.For<List<BreakerResult>>(0, rotorConfigurationsToCheck.Count, () => new List<BreakerResult>(), (i, loop, threadResults) => //multithreaded for loop
            {
                threadResults.AddRange(getIndividualRotorResults(cipherArr, rotorConfigurationsToCheck[(int)i], fitness));//add to the thread results the top results for that configuration of rotors
                return threadResults;//return the thread results
            },
            (threadResults) => { 
                    lock (rotorListLock)//lock the list
                    {
                        results.AddRange(threadResults);//add the thread results to the results list
                    }
            });                 
            return sortBreakerList(results).GetRange(0, _bc.topRotorsToSearch);//return the top few enigma configurations
        }
        /// <summary>
        /// Loops through each rotation configuration for this rotor combination
        /// Evaluates them all using the fitness function
        /// </summary>
        /// <param name="cipherArr"></param>
        /// The ciphertext in integer array
        /// <param name="emStr"></param>
        /// The enigma configuration to check as a json string
        /// <param name="fitness"></param>
        /// Fitness function to use when checking
        /// <returns>The top few rotation settings for this rotor combination</returns>
        public List<BreakerResult> getIndividualRotorResults(int[] cipherArr,string emStr,IFitness fitness)
        {
            List<BreakerResult> results = new List<BreakerResult>();//create results list
            double lowestResult = double.MinValue;//store the lowest rank in as the minimum value
            for (int l = 0; l <= 25; l++)//for each left rotor rotation
            {
                for (int m = 0; m <= 25; m++)//for each middle rotor rotation
                {
                    for (int r = 0; r <= 25; r++)//for each right rotor rotation
                    {
                        EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                        em.rotors[0].rotation = l;//set the rotation of the left rotor
                        em.rotors[1].rotation = m;//set the rotation of the middle rotor
                        em.rotors[2].rotation = r;//set the rotation of the right rotor
                        int[] attemptPlainText = _encodingService.encode(cipherArr, em);//get the integer array of the attempt at decoding with the current enigma setup
                        double rating = fitness.getFitness(attemptPlainText);//rate how english the attempt is using the fitness function

                        if (rating > lowestResult)//if the rating is higher than the current lowest result scored
                        {
                            em.rotors[0].rotation = l;//reset the rotation for the left rotor
                            em.rotors[1].rotation = m;//reset the rotation for the middle rotor
                            em.rotors[2].rotation = r;//reset the rotation of the right rotor
                            BreakerResult br = new BreakerResult(attemptPlainText, rating, em);//create a result object
                            bool stillSubtract = results.Count + 1 > _bc.topSingleRotors;//if there is to many objects in the list
                            while (stillSubtract)//while there is too many objects in the list
                            {
                                double nextLowest = rating;
                                BreakerResult lowest = null;
                                foreach (BreakerResult result in results)//for all the results
                                {
                                    if (result.score <= lowestResult && lowest == null)//if the score is less than or equal to the lowest result
                                    {
                                        lowest = result;//set the lowest result to current
                                    }
                                    else if (result.score < nextLowest)//else if the current score is less than the next lowest
                                    {
                                        nextLowest = result.score;//set the next lowest to the current score
                                    }
                                }
                                results.Remove(lowest);//remove the lowest result from the list
                                lowestResult = nextLowest;//set the lowest result score to the new lowest score
                                if (results.Count + 1 <= _bc.topSingleRotors || lowest == null)//if the list is still too long or no object could be removed
                                {
                                    stillSubtract = false;//break the while loop
                                }
                            }
                            results.Add(br);//add the new result to the list
                        }
                    }
                }
            }
            return results;//return the results list
        }
        #endregion

        #region offset and Rotation
        private readonly object offsetListLock = new object();//mutex for the offset result list

        /// <summary>
        /// 
        /// </summary>
        /// <param name="breakerResults"></param>
        /// <param name="cipherArr"></param>
        /// <param name="fitnessStr"></param>
        /// <returns></returns>
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
                            offsetConfigurationsToCheck.Add(br.enigmaModel.toStringRotors());
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
        public BreakerResult getPlugboardResults(List<BreakerResult> offsetResults, int[] cipherArr, string fitnessStr = "")
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
            foreach(BreakerResult br in offsetResults)
            {
                List<BreakerResult> onePairResults = new List<BreakerResult>() { br };
                while (onePairResults[0].enigmaModel.plugboard.Count < _bc.maxPlugboardSettings)
                {
                    foreach (BreakerResult opr in onePairResults)
                    {
                        onePairResults = onePairPlugboard(opr, cipherArr, fitness);
                    }
                    allPlugboardResults.AddRange(onePairResults);
                }
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
