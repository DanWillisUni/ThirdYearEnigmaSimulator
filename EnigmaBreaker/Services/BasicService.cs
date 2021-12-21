using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;
using EnigmaBreaker.Services.Fitness;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;

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
    ]";
            allRotors = JsonConvert.DeserializeObject<List<Rotor>>(Rotorjson);
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

        public void root()
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
            _logger.LogInformation(toStringRotors(em));
            string ciphertext = _encodingService.encode(plaintext, em);
            List<BreakerResult> rotorResults = sortBreakerList(getRotorResultsWithRotation(ciphertext, _resolver("IOC")));
            int counter = 1;
            foreach (BreakerResult br in rotorResults)
            {
                _logger.LogDebug($"{br.score} - {toStringRotors(br.enigmaModel)}");
                if (toStringRotors(br.enigmaModel).Split("/")[0].Equals(toStringRotors(em).Split("/")[0]))
                {
                    _logger.LogInformation($"{counter} - {br.score} - {toStringRotors(br.enigmaModel)}");
                    break;
                }
                counter += 1;
            }            
        }
        public List<BreakerResult> getRotorResultsWithRotation(string cipherText,IFitness fitness)
        {
            List<BreakerResult> results = new List<BreakerResult>();

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

                                                string attemptPlainText = _encodingService.encode(cipherText, em);
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
                                                    results.Add(new BreakerResult(attemptPlainText, rating, em));
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
            string r = enigmaModel.reflector.rotor.name + " ";
            foreach (RotorModel rotor in enigmaModel.rotors)
            {
                r += rotor.rotor.name + " ";
            }
            r += "/ ";
            foreach (RotorModel rotor in enigmaModel.rotors)
            {
                r += rotor.rotation + " ";
            }
            r += "/ ";
            foreach (RotorModel rotor in enigmaModel.rotors)
            {
                r += rotor.ringOffset + " ";
            }
            return r;
        }
        #endregion
    }
}
