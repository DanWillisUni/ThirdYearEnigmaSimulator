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
            string plaintext = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.
 
The way Dinah washed her children’s faces was this: first she held the poor thing down by its ear with one paw, and then with the other paw she rubbed its face all over, the wrong way, beginning at the nose: and just now, as I said, she was hard at work on the white kitten, which was lying quite still and trying to purr—no doubt feeling that it was all meant for its good.
 
But the black kitten had been finished with earlier in the afternoon, and so, while Alice was sitting curled up in a corner of the great arm-chair, half talking to herself and half asleep, the kitten had been having a grand game of romps with the ball of worsted Alice had been trying to wind up, and had been rolling it up and down till it had all come undone again; and there it was, spread over the hearth-rug, all knots and tangles, with the kitten running after its own tail in the middle.
 
“Oh, you wicked little thing!” cried Alice, catching up the kitten, and giving it a little kiss to make it understand that it was in disgrace. “Really, Dinah ought to have taught you better manners! You ought, Dinah, you know you ought!” she added, looking reproachfully at the old cat, and speaking in as cross a voice as she could manage—and then she scrambled back into the arm-chair, taking the kitten and the worsted with her, and began winding up the ball again. But she didn’t get on very fast, as she was talking all the time, sometimes to the kitten, and sometimes to herself. Kitty sat very demurely on her knee, pretending to watch the progress of the winding, and now and then putting out one paw and gently touching the ball, as if it would be glad to help, if it might.
";//first 4 paragraphs in alice in wonderland
            EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

            _logger.LogInformation(toStringRotors(em));
            string ciphertext = _encodingService.encode(plaintext, em);

            int[] cipherArr = _encodingService.preProccessCiphertext(ciphertext);

            //rotors
            List<BreakerResult> initialRotorSetupResults = sortBreakerList(getRotorResults(ciphertext, _resolver("IOC")));
            //rotorOffset
            List<BreakerResult> fullRotorResultOfAll = new List<BreakerResult>();
            foreach (BreakerResult rotorResult in initialRotorSetupResults)
            {
                fullRotorResultOfAll.AddRange(getRotationOffsetResult(rotorResult, cipherArr, _resolver("IOC")));
            }
            fullRotorResultOfAll = sortBreakerList(fullRotorResultOfAll);
            if (fullRotorResultOfAll.Count > _bc.topAllRotorRotationAndOffset)
            {
                fullRotorResultOfAll = fullRotorResultOfAll.GetRange(0, _bc.topAllRotorRotationAndOffset);
            }
            //plugboard
            foreach (BreakerResult offsetResults in fullRotorResultOfAll)
            {
                //plugboard
            }
        }
        public void measureSuccessRate()
        {
            int counts = 5;
            int rotorMiss = 0;
            int offsetMiss = 0;
            int plugboardMiss = 0;
            double rotorFoundPositionSum = 0.0;
            double offsetFoundPositionSum = 0.0;
            int success = 0;
            for (int i = 0; i < counts; i++)
            {
                string plaintext = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.
 
The way Dinah washed her children’s faces was this: first she held the poor thing down by its ear with one paw, and then with the other paw she rubbed its face all over, the wrong way, beginning at the nose: and just now, as I said, she was hard at work on the white kitten, which was lying quite still and trying to purr—no doubt feeling that it was all meant for its good.
 
But the black kitten had been finished with earlier in the afternoon, and so, while Alice was sitting curled up in a corner of the great arm-chair, half talking to herself and half asleep, the kitten had been having a grand game of romps with the ball of worsted Alice had been trying to wind up, and had been rolling it up and down till it had all come undone again; and there it was, spread over the hearth-rug, all knots and tangles, with the kitten running after its own tail in the middle.
 
“Oh, you wicked little thing!” cried Alice, catching up the kitten, and giving it a little kiss to make it understand that it was in disgrace. “Really, Dinah ought to have taught you better manners! You ought, Dinah, you know you ought!” she added, looking reproachfully at the old cat, and speaking in as cross a voice as she could manage—and then she scrambled back into the arm-chair, taking the kitten and the worsted with her, and began winding up the ball again. But she didn’t get on very fast, as she was talking all the time, sometimes to the kitten, and sometimes to herself. Kitty sat very demurely on her knee, pretending to watch the progress of the winding, and now and then putting out one paw and gently touching the ball, as if it would be glad to help, if it might.
";//first 4 paragraphs in alice in wonderland
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogInformation(toStringRotors(em));
                string ciphertext = _encodingService.encode(plaintext, em);
                int[] cipherArr = _encodingService.preProccessCiphertext(ciphertext);

                List<BreakerResult> initialRotorSetupResults = sortBreakerList(getRotorResults(ciphertext, _resolver("IOC")));

                bool found = false;
                foreach (BreakerResult br in initialRotorSetupResults)
                {
                    if (br.enigmaModel.reflector.rotor.name == em.reflector.rotor.name && br.enigmaModel.rotors[0].rotor.name == em.rotors[0].rotor.name && br.enigmaModel.rotors[1].rotor.name == em.rotors[1].rotor.name && br.enigmaModel.rotors[2].rotor.name == em.rotors[2].rotor.name)//this line is a cheat
                    {
                        found = true;
                        _logger.LogDebug($"R1 - {initialRotorSetupResults.IndexOf(br)}");
                        rotorFoundPositionSum += initialRotorSetupResults.IndexOf(br) + 1;
                        break;
                    }
                }
                if (!found)
                {
                    rotorMiss += 1;
                }
                else
                {
                    List<BreakerResult> fullRotorResultOfAll = new List<BreakerResult>();
                    foreach (BreakerResult rotorResult in initialRotorSetupResults)
                    {
                        fullRotorResultOfAll.AddRange(getRotationOffsetResult(rotorResult, cipherArr, _resolver("IOC")));
                    }
                    fullRotorResultOfAll = sortBreakerList(fullRotorResultOfAll);
                    if (fullRotorResultOfAll.Count > _bc.topAllRotorRotationAndOffset)
                    {
                        fullRotorResultOfAll = fullRotorResultOfAll.GetRange(0, _bc.topAllRotorRotationAndOffset);
                    }
                    found = false;
                    foreach (BreakerResult brr in fullRotorResultOfAll)
                    {
                        if (toStringRotors(brr.enigmaModel).Split("/")[2] == toStringRotors(em2).Split("/")[2] && toStringRotors(brr.enigmaModel).Split("/")[3] == toStringRotors(em2).Split("/")[3] && brr.enigmaModel.rotors[0].rotation == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset))
                        {
                            found = true;
                            _logger.LogDebug($"O1 - {fullRotorResultOfAll.IndexOf(brr)}");
                            offsetFoundPositionSum += fullRotorResultOfAll.IndexOf(brr) + 1;
                            break;
                        }
                    }
                    if (!found)
                    {
                        offsetMiss += 1;
                    }
                    else
                    {
                        found = false;
                        foreach(BreakerResult brrr in fullRotorResultOfAll)
                        {
                            BreakerResult finalResult = getPlugboardSettings(brrr, cipherArr);
                            if(comparePlugboard(toStringPlugboard(em2), toStringPlugboard(finalResult.enigmaModel)))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            plugboardMiss += 1;
                        }
                        else
                        {
                            success += 1;
                        }
                    }
                }
            }
            _logger.LogInformation($"Rotor Fail: {(double)(rotorMiss * 100 / counts)}%");
            _logger.LogInformation($"Offset Fail: {(double)(offsetMiss * 100 / counts)}%");
            _logger.LogInformation($"Offset Fail: {(double)(plugboardMiss * 100 / counts)}%");
            _logger.LogInformation($"Success: {(double)(success * 100 / counts)}%");
            _logger.LogInformation($"Average Rotor Index when found: {(double)(rotorFoundPositionSum / (counts - rotorMiss))}");
            _logger.LogInformation($"Average Offset Index when found: {(double)(offsetFoundPositionSum / (counts - (rotorMiss + offsetMiss)))}");
        }

        public void testRotor()
        {
            int counts = 100;
            double success = 0.0;
            for (int i = 0; i < counts; i++)
            {
                string plaintext = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.
 
The way Dinah washed her children’s faces was this: first she held the poor thing down by its ear with one paw, and then with the other paw she rubbed its face all over, the wrong way, beginning at the nose: and just now, as I said, she was hard at work on the white kitten, which was lying quite still and trying to purr—no doubt feeling that it was all meant for its good.
 
But the black kitten had been finished with earlier in the afternoon, and so, while Alice was sitting curled up in a corner of the great arm-chair, half talking to herself and half asleep, the kitten had been having a grand game of romps with the ball of worsted Alice had been trying to wind up, and had been rolling it up and down till it had all come undone again; and there it was, spread over the hearth-rug, all knots and tangles, with the kitten running after its own tail in the middle.
 
“Oh, you wicked little thing!” cried Alice, catching up the kitten, and giving it a little kiss to make it understand that it was in disgrace. “Really, Dinah ought to have taught you better manners! You ought, Dinah, you know you ought!” she added, looking reproachfully at the old cat, and speaking in as cross a voice as she could manage—and then she scrambled back into the arm-chair, taking the kitten and the worsted with her, and began winding up the ball again. But she didn’t get on very fast, as she was talking all the time, sometimes to the kitten, and sometimes to herself. Kitty sat very demurely on her knee, pretending to watch the progress of the winding, and now and then putting out one paw and gently touching the ball, as if it would be glad to help, if it might.
";//first 4 paragraphs in alice in wonderland
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogInformation(toStringRotors(em) + "/" + toStringPlugboard(em));
                string ciphertext = _encodingService.encode(plaintext, em);

                List<BreakerResult> initialRotorSetupResults = sortBreakerList(getRotorResults(ciphertext, _resolver("IOC")));

                foreach (BreakerResult br in initialRotorSetupResults)
                {
                    if (br.enigmaModel.reflector.rotor.name == em.reflector.rotor.name && br.enigmaModel.rotors[0].rotor.name == em.rotors[0].rotor.name && br.enigmaModel.rotors[1].rotor.name == em.rotors[1].rotor.name && br.enigmaModel.rotors[2].rotor.name == em.rotors[2].rotor.name)
                    {
                        _logger.LogInformation($"Correct Rotors {initialRotorSetupResults.IndexOf(br)}: {toStringRotors(br.enigmaModel)}");
                        if (br.enigmaModel.rotors[0].rotation - 1 == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset) || br.enigmaModel.rotors[0].rotation == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset) || br.enigmaModel.rotors[0].rotation + 1 == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset))
                        {
                            if (br.enigmaModel.rotors[1].rotation - 1 == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset) || br.enigmaModel.rotors[1].rotation == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset) || br.enigmaModel.rotors[1].rotation + 1 == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset))
                            {
                                if (br.enigmaModel.rotors[2].rotation - 1 == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset) || br.enigmaModel.rotors[2].rotation == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset) || br.enigmaModel.rotors[2].rotation + 1 == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset))//this line is a cheat
                                {
                                    success += 1.0;
                                    _logger.LogInformation($"Rotor result {initialRotorSetupResults.IndexOf(br)}: {toStringRotors(br.enigmaModel)}");
                                    break;
                                }
                            }
                        }
                        
                    }
                }
            }
            _logger.LogInformation($"Success rate: {success * 100 / counts}%");
        }
        public double testOffset(string fitness,int[] plaintext)
        {
            int counts = 50;
            double success = 0.0;
            for (int i = 0; i < counts; i++)
            {
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                EnigmaModel em3 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogInformation(toStringRotors(em) + "/" + toStringPlugboard(em));
                int[] cipherArr = _encodingService.encode(plaintext, em);

                em2.plugboard = new Dictionary<int, int>();
                Random rnd = new Random();
                for(int ri = 0; ri < 3; ri++)
                {
                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                    em2.rotors[ri].ringOffset = 0;
                }
                _logger.LogInformation($"Input: {toStringRotors(em2)}");

                List<BreakerResult> fullRotorOffset = getRotationOffsetResult(new BreakerResult(cipherArr,double.MinValue,em2), cipherArr, _resolver(fitness));
                bool found = false;
                foreach(BreakerResult brr in fullRotorOffset)
                {
                    if (toStringRotors(brr.enigmaModel).Split("/")[2] == toStringRotors(em3).Split("/")[2] && toStringRotors(brr.enigmaModel).Split("/")[3] == toStringRotors(em3).Split("/")[3] && brr.enigmaModel.rotors[0].rotation == EncodingService.mod26(em3.rotors[0].rotation - em3.rotors[0].ringOffset))
                    {
                        found = true;
                        _logger.LogInformation($"Result {fullRotorOffset.IndexOf(brr)}: {toStringRotors(brr.enigmaModel)}");
                    }
                }
                if (found)
                {
                    success += 1.0;
                }
                else
                {
                    _logger.LogInformation("Incorrect");
                }
            }
            _logger.LogInformation($"Success rate: {success * 100 / counts}%");
            return (success * 100) / counts;
        }
        public double testPlugboard(int[] plaintext)
        {
            int counts = 100;
            double success = 0.0;
            for (int i = 0; i < counts; i++)
            {
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogInformation(toStringRotors(em) + "/" + toStringPlugboard(em));
                int[] cipherArr = _encodingService.encode(plaintext, em);

                em2.plugboard = new Dictionary<int, int>();
                BreakerResult brr = new BreakerResult(cipherArr, double.MinValue, em2);
                BreakerResult finalResult = getPlugboardSettings(brr, cipherArr);
                _logger.LogInformation($"Final Result: {toStringRotors(finalResult.enigmaModel)} {toStringPlugboard(finalResult.enigmaModel)}");

                string actPB = toStringPlugboard(em);
                string resultPB = toStringPlugboard(finalResult.enigmaModel);
                bool correctPB = comparePlugboard(actPB, resultPB);
                if (correctPB)
                {
                    success += 1.0;
                }
                else
                {
                    _logger.LogInformation("Incorrect");
                }
            }
            _logger.LogInformation($"Success rate: {success * 100 / counts}%");
            return (success * 100) / counts;
        }

        public List<BreakerResult> getRotorResults(string cipherText, IFitness fitness)
        {
            List<BreakerResult> results = new List<BreakerResult>();
            int[] cipherArr = _encodingService.preProccessCiphertext(cipherText);
            double lowestResult = double.MinValue;

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
                                                    em.rotors[0].rotation = l;
                                                    em.rotors[1].rotation = m;
                                                    em.rotors[2].rotation = r;
                                                    BreakerResult br = new BreakerResult(attemptPlainText, rating, em);
                                                    bool stillSubtract = results.Count + 1 > _bc.topRotorsToSearch;
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
                                                        if(results.Count + 1 <= _bc.topRotorsToSearch || lowest == null)
                                                        {
                                                            stillSubtract = false;
                                                        }
                                                    }
                                                    results.Add(br);
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
        public List<BreakerResult> roundTwoBreakerResults(IFitness fitness,List<BreakerResult> initialResults,int maxCount)
        {
            List<BreakerResult> roundTwoResults = new List<BreakerResult>();
            foreach (BreakerResult br in initialResults) 
            {
                roundTwoResults.Add(new BreakerResult(br.text,fitness.getFitness(br.text),br.enigmaModel));
            }
            roundTwoResults = sortBreakerList(roundTwoResults);
            if (roundTwoResults.Count < maxCount)
            {
                return roundTwoResults;
            }
            return roundTwoResults.GetRange(0, maxCount);
        }

        #region offset and Rotation
        private List<BreakerResult> getRotationOffsetResult(BreakerResult br,int[] ciphertext,IFitness fitness)
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
            if(_bc.topSingleRotorRotationAndOffset > fullRotorResults.Count)
            {
                return fullRotorResults;
            }            
            return fullRotorResults.GetRange(0,_bc.topSingleRotorRotationAndOffset);
        }
        private List<BreakerResult> getRotorResultsWithRotationAndOffset(int[] cipherArr, IFitness fitness, string currentRotors)
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
        public BreakerResult getPlugboardSettings(BreakerResult br, int[] cipherArr)
        {
            IFitness fitness = _resolver("IOC");
            if (cipherArr.Length < 300)
            {
                if (cipherArr.Length < 200)
                {
                    fitness = _resolver("TRI");
                }
                else
                {
                    fitness = _resolver("QUAD");
                }
            }
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
        private List<BreakerResult> sortBreakerList(List<BreakerResult> input)
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
        
        #endregion
    }
}
