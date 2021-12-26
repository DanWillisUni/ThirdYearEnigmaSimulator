using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;
using EnigmaBreaker.Services.Fitness;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        public BasicService(ILogger<BasicService> logger, BasicConfiguration bc, EncodingService encodingService,IFitness.FitnessResolver fitnessResolver)
        {
            _logger = logger;
            _bc = bc;
            _encodingService = encodingService;
            _resolver = fitnessResolver;
            /*
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
            allReflectors = JsonConvert.DeserializeObject<List<Rotor>>(Reflectorjson);
            */
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
      }
    ]";
            allRotors = JsonConvert.DeserializeObject<List<Rotor>>(Rotorjson);
            string Reflectorjson = @"[      
      {
                'name': 'A',
        'order': 'EJMZALYXVBWFCRQUONTSPIKHGD'
      }
    ]";
            allReflectors = JsonConvert.DeserializeObject<List<Rotor>>(Reflectorjson);
        }

        public string root()
        {
            /*List<RotorModel> emRotors = new List<RotorModel>();
            emRotors.Add(new RotorModel(allRotors[0],2,3));
            emRotors.Add(new RotorModel(allRotors[1],4,6));
            emRotors.Add(new RotorModel(allRotors[2],6,9));
            EnigmaModel em = new EnigmaModel(emRotors, new RotorModel(allReflectors[0]), new Dictionary<int, int>());
            */
            //https://www.gutenberg.org/files/12/12-h/12-h.htm#link2HCH0001
            string plaintext = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.
 
The way Dinah washed her children’s faces was this: first she held the poor thing down by its ear with one paw, and then with the other paw she rubbed its face all over, the wrong way, beginning at the nose: and just now, as I said, she was hard at work on the white kitten, which was lying quite still and trying to purr—no doubt feeling that it was all meant for its good.
 
But the black kitten had been finished with earlier in the afternoon, and so, while Alice was sitting curled up in a corner of the great arm-chair, half talking to herself and half asleep, the kitten had been having a grand game of romps with the ball of worsted Alice had been trying to wind up, and had been rolling it up and down till it had all come undone again; and there it was, spread over the hearth-rug, all knots and tangles, with the kitten running after its own tail in the middle.
 
“Oh, you wicked little thing!” cried Alice, catching up the kitten, and giving it a little kiss to make it understand that it was in disgrace. “Really, Dinah ought to have taught you better manners! You ought, Dinah, you know you ought!” she added, looking reproachfully at the old cat, and speaking in as cross a voice as she could manage—and then she scrambled back into the arm-chair, taking the kitten and the worsted with her, and began winding up the ball again. But she didn’t get on very fast, as she was talking all the time, sometimes to the kitten, and sometimes to herself. Kitty sat very demurely on her knee, pretending to watch the progress of the winding, and now and then putting out one paw and gently touching the ball, as if it would be glad to help, if it might.
";//first 4 paragraphs in alice in wonderland
            EnigmaModel em = EnigmaModel.randomizeEnigma(3,1);
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

            _logger.LogInformation(toStringRotors(em));
            string ciphertext = _encodingService.encode(plaintext, em);
            List<BreakerResult> rotorResults = sortBreakerList(getRotorResults(ciphertext, _resolver("IOC")));
            bool rotorFound = false;
            foreach (BreakerResult br in rotorResults) // for each rotor configuration result
            {                
                if (br.enigmaModel.reflector.rotor.name == em.reflector.rotor.name && br.enigmaModel.rotors[0].rotor.name == em.rotors[0].rotor.name && br.enigmaModel.rotors[1].rotor.name == em.rotors[1].rotor.name && br.enigmaModel.rotors[2].rotor.name == em.rotors[2].rotor.name)
                {
                    _logger.LogInformation(toStringRotors(br.enigmaModel));
                    rotorFound = true;
                    List<BreakerResult> fullRotorResults = getRotationOffsetResult(br,ciphertext, _resolver("IOC"));
                    bool offsetFound = false;
                    foreach (BreakerResult brr in fullRotorResults)
                    {                        
                        if (toStringRotors(brr.enigmaModel).Split("/")[2] == toStringRotors(em2).Split("/")[2] && toStringRotors(brr.enigmaModel).Split("/")[3] == toStringRotors(em2).Split("/")[3] && brr.enigmaModel.rotors[0].rotation == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset))
                        {
                            offsetFound = true;
                            _logger.LogInformation($"{brr.score} - {toStringRotors(brr.enigmaModel)}");
                            //plugboard
                            return "N";
                        }
                        else
                        {
                            _logger.LogDebug($"{brr.score} - {toStringRotors(brr.enigmaModel)}");
                        }
                    }
                    if (!offsetFound)
                    {
                        _logger.LogInformation("Rotor Miss");
                        return "O";
                    }
                }
                else
                {
                    _logger.LogDebug(toStringRotors(br.enigmaModel));
                }
            }
            if (!rotorFound)
            {
                _logger.LogInformation("Miss");
                return "R";
            }
            return "C";
        }
        #region offset and Rotation
        private List<BreakerResult> getRotationOffsetResult(BreakerResult br,string ciphertext,IFitness fitness)
        {
            List<BreakerResult> fullRotorResults = new List<BreakerResult>();
            int lbase = br.enigmaModel.rotors[0].rotation;
            int mbase = br.enigmaModel.rotors[1].rotation;
            int rbase = br.enigmaModel.rotors[2].rotation;
            for (int lchange = -1; lchange < 2; lchange++)
            {
                for (int mchange = -1; mchange < 2; mchange++)
                {
                    for (int rchange = -1; rchange < 2; rchange++)
                    {
                        br.enigmaModel.rotors[0].rotation = lbase + lchange;
                        br.enigmaModel.rotors[1].rotation = mbase + mchange;
                        br.enigmaModel.rotors[2].rotation = rbase + rchange;
                        fullRotorResults.AddRange(getRotorResultsWithRotationAndOffset(ciphertext,fitness, toStringRotors(br.enigmaModel)));
                    }
                }
            }
            fullRotorResults = sortBreakerList(fullRotorResults);     
            if(_bc.topAllRotationAndOffset > fullRotorResults.Count)
            {
                return fullRotorResults;
            }            
            return fullRotorResults.GetRange(0,_bc.topAllRotationAndOffset);
        }
        private List<BreakerResult> getRotorResultsWithRotationAndOffset(string ciphertext, IFitness fitness, string currentRotors)
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
            int[] cipherArr = preProccessCiphertext(ciphertext);
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
                        double newLowest = rating;
                        if (results.Count + 1 < _bc.topSingleRotationAndOffset)
                        {
                            newLowest = 0;
                        }
                        else
                        {
                            BreakerResult toRemove = null;
                            foreach (var result in results)
                            {
                                if (result.score == lowestResult)
                                {
                                    toRemove = result;
                                }
                                else if (result.score < newLowest)
                                {
                                    newLowest = result.score;
                                }
                            }
                            results.Remove(toRemove);
                        }
                        lowestResult = newLowest;
                        List<RotorModel> emRotorModel2 = new List<RotorModel>();
                        emRotorModel2.Add(new RotorModel(emRotors[0], lbase, 0));
                        emRotorModel2.Add(new RotorModel(emRotors[1], m, EncodingService.mod26(m - mbase)));
                        emRotorModel2.Add(new RotorModel(emRotors[2], r, EncodingService.mod26(r - rbase)));
                        EnigmaModel em2 = new EnigmaModel(emRotorModel2, emRefl, new Dictionary<int, int>());
                        results.Add(new BreakerResult(rating,em2));
                    }
                }
            }
            
            return results;
        }
        #endregion
        public List<BreakerResult> getRotorResults(string cipherText,IFitness fitness)
        {
            List<BreakerResult> results = new List<BreakerResult>();
            int[] cipherArr = preProccessCiphertext(cipherText);

            double lowestResult = 0.0;

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
                                    //_logger.LogInformation($"{left.name} {middle.name} {right.name}");

                                    for (int l = 0; l <= 25; l++)
                                    {
                                        for (int m = 0; m <= 25; m++)
                                        {
                                            for (int r = 0; r <= 25; r++)
                                            {
                                                List<RotorModel> rotors = new List<RotorModel>();
                                                rotors.Add(new RotorModel(left, l));
                                                rotors.Add(new RotorModel(middle, m));
                                                rotors.Add(new RotorModel(right, r));
                                                EnigmaModel em = new EnigmaModel(rotors, new RotorModel(refl), new Dictionary<int, int>());

                                                int[] attemptPlainText = _encodingService.encode(cipherArr, em);
                                                double rating = fitness.getFitness(attemptPlainText);

                                                if (rating > lowestResult)
                                                {
                                                    double newLowest = rating;
                                                    if (results.Count + 1 < _bc.topRotorsToSearch)
                                                    {
                                                        newLowest = 0;
                                                    }
                                                    else
                                                    {
                                                        BreakerResult toRemove = null;
                                                        foreach (var result in results)
                                                        {
                                                            if (result.score == lowestResult)
                                                            {
                                                                toRemove = result;
                                                            }
                                                            else if (result.score < newLowest)
                                                            {
                                                                newLowest = result.score;
                                                            }
                                                        }
                                                        results.Remove(toRemove);
                                                    }
                                                    lowestResult = newLowest;
                                                    em.rotors[0].rotation = l;
                                                    em.rotors[1].rotation = m;
                                                    em.rotors[2].rotation = r;
                                                    results.Add(new BreakerResult(rating, em));
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

            return results;
        }

        #region helper
        private List<BreakerResult> sortBreakerList(List<BreakerResult> input)
        {
            List<BreakerResult> r = new List<BreakerResult>();
            while (input.Count > 0)
            {
                double highestScore = 0.0;
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
        private int[] preProccessCiphertext(string ciphertext)
        {            
            string formattedInput = Regex.Replace(ciphertext.ToUpper(), @"[^A-Z]", string.Empty);
            int[] r = new int[formattedInput.Length];
            for (int i = 0; i< formattedInput.Length;i++)
            {
                r[i] = Convert.ToInt16(formattedInput[i]) - 65;
            }
            return r;
        }
        #endregion
    }
}
