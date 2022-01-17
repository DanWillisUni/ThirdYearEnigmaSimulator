using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Models;
using EnigmaBreaker.Services.Fitness;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EnigmaBreaker.Services
{
    public class Measuring
    {
        private readonly ILogger<Measuring> _logger;
        private readonly BasicConfiguration _bc;
        private readonly EncodingService _encodingService;
        private readonly IFitness.FitnessResolver _resolver;
        private readonly BasicService _bs;
        public Measuring(ILogger<Measuring> logger, BasicConfiguration bc, EncodingService encodingService, IFitness.FitnessResolver fitnessResolver,BasicService bs) 
        {
            _logger = logger;
            _bc = bc;
            _encodingService = encodingService;
            _resolver = fitnessResolver;
            _bs = bs;
        }

        public void root()
        {
            //testLength(100, 2000, 100, "Plugboard", 100, new List<string>() { "IOC", "BI", "TRI","QUAD" },"Results/plugboardLengthTest.csv");//1.6 hours
            //testLength(10, 500, 10, "Plugboard", 100, new List<string>() { "IOC", "BI", "TRI", "QUAD" }, "Results/plugboardLengthTestClose.csv");//2.3 hours
            //testLength(100, 2000, 100, "Offset", 25, new List<string>() { "IOC", "BI", "TRI", "QUAD" },"Results/offsetLengthTest.csv");//3 hours
            //testLength(100, 2000, 100, "Rotors", 5, new List<string>() { "IOC", "BI", "TRI", "QUAD" },"Results/rotorLengthTest.csv");//15 hours

            //testSpeed(100, 2000, 100, "Plugboard", 500, "Results/plugboardSpeedTest.csv");//3.9 hours
            //testSpeed(100, 2000, 100, "Offset", 100, "Results/offsetSpeedTest.csv");//3.7 hours
            //testSpeed(100, 2000, 100, "Rotors", 5, "Results/rotorsSpeedTest.csv");//15 hours

            //measureFullRunthrough(100);

            testLength(100, 2000, 100, "Offset", 1, new List<string>() { "IOC", "BI", "TRI", "QUAD" },"Results/offsetLengthTest.csv");//3 hours
        }
        public void measureFullRunthrough(int iterations)
        {
            int rotorMiss = 0;
            int offsetMiss = 0;
            int plugboardMiss = 0;
            double rotorFoundPositionSum = 0.0;
            double offsetFoundPositionSum = 0.0;
            int success = 0;
            for (int i = 0; i < iterations; i++)
            {
                string plaintext = _bs.getText();
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogDebug(em.toStringRotors());
                string ciphertext = _encodingService.encode(plaintext, em);
                int[] cipherArr = _encodingService.preProccessCiphertext(ciphertext);

                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr));

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
                    List<BreakerResult> fullRotorResultOfAll = _bs.getRotationOffsetResult(initialRotorSetupResults, cipherArr);
                    found = false;
                    foreach (BreakerResult brr in fullRotorResultOfAll)
                    {
                        /*if (_bs.toStringRotors(brr.enigmaModel).Split("/")[2] == _bs.toStringRotors(em2).Split("/")[2] && _bs.toStringRotors(brr.enigmaModel).Split("/")[3] == _bs.toStringRotors(em2).Split("/")[3] && brr.enigmaModel.rotors[0].rotation == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset))
                        {
                            found = true;
                            _logger.LogDebug($"O1 - {fullRotorResultOfAll.IndexOf(brr)}");
                            offsetFoundPositionSum += fullRotorResultOfAll.IndexOf(brr) + 1;
                            break;
                        }*/
                    }
                    if (!found)
                    {
                        offsetMiss += 1;
                    }
                    else
                    {
                        found = false;
                        foreach (BreakerResult brrr in fullRotorResultOfAll)
                        {
                            BreakerResult finalResult = _bs.getPlugboardResults(brrr, cipherArr);
                            if (comparePlugboard(em2.toStringPlugboard(), finalResult.enigmaModel.toStringPlugboard()))
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
            _logger.LogDebug($"Rotor Fail: {(double)(rotorMiss * 100 / iterations)}%");
            _logger.LogDebug($"Offset Fail: {(double)(offsetMiss * 100 / iterations)}%");
            _logger.LogDebug($"Plugboard Fail: {(double)(plugboardMiss * 100 / iterations)}%");
            _logger.LogDebug($"Success: {(double)(success * 100 / iterations)}%");
            _logger.LogDebug($"Average Rotor Index when found: {(double)(rotorFoundPositionSum / (iterations - rotorMiss))}");
            _logger.LogDebug($"Average Offset Index when found: {(double)(offsetFoundPositionSum / (iterations - (rotorMiss + offsetMiss)))}");
        }
        public void testLength(int from, int to, int step, string toTest, int iterations, List<string> fitnessStrToTest, string filePathAndName)
        {
            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = _encodingService.preProccessCiphertext(plaintext).ToList();
            List<string> linesToFile = new List<string>() { "," + string.Join(",", fitnessStrToTest) };
            for (int i = from; i < to + 1; i += step)
            {
                string lineToFile = $"{i}";
                foreach (string fitnessStr in fitnessStrToTest)
                {
                    switch (toTest)
                    {
                        case "Plugboard":
                            lineToFile += $", {testPlugboard(plainArr.GetRange(0, i).ToArray(), iterations, fitnessStr)}";
                            break;
                        case "Offset":
                            lineToFile += $", {testOffset(plainArr.GetRange(0, i).ToArray(), iterations, fitnessStr)}";
                            break;
                        case "Rotors":
                            lineToFile += $", {testRotor(plainArr.GetRange(0, i).ToArray(), iterations, fitnessStr)}";
                            break;
                        default:
                            _logger.LogWarning($"Unsure what to test: {toTest}");
                            break;
                    }
                    _logger.LogInformation($"Finished {fitnessStr}");
                }
                _logger.LogInformation($"Finished {toTest} {i}");
                linesToFile.Add(lineToFile);
            }

            File.WriteAllLines(filePathAndName, linesToFile);
            _logger.LogInformation("Writing to file");
        }

        public void testSpeed(int from, int to, int step, string toTest, int iterations, string filePathAndName)
        {
            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = _encodingService.preProccessCiphertext(plaintext).ToList();
            List<string> linesToFile = new List<string>() { ",Speed" };
            for (int i = from; i < to + 1; i += step)
            {
                string lineToFile = $"{i}";
                string elapsedTime = "";
                for (int j = 0; j < iterations; j++)
                {
                    EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                    string emJson = JsonConvert.SerializeObject(em);
                    int[] cipherArr = _encodingService.encode(plainArr.GetRange(0, i).ToArray(), em);
                    TimeSpan total = TimeSpan.Zero;

                    switch (toTest)
                    {
                        case "Plugboard":
                            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                            em2.plugboard = new Dictionary<int, int>();

                            Stopwatch stopWatchPlugboard = new Stopwatch();
                            stopWatchPlugboard.Start();
                            BreakerResult finalResult = _bs.getPlugboardResults(new BreakerResult(cipherArr, double.MinValue, em2), cipherArr);
                            stopWatchPlugboard.Stop();
                            TimeSpan tsPlugboard = stopWatchPlugboard.Elapsed;
                            total = total.Add(tsPlugboard);
                            break;
                        case "Offset":
                            em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                            em2.plugboard = new Dictionary<int, int>();
                            Random rnd = new Random();
                            for (int ri = 0; ri < 3; ri++)
                            {
                                em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                                em2.rotors[ri].ringOffset = 0;
                            }

                            Stopwatch stopWatchOffset = new Stopwatch();
                            stopWatchOffset.Start();
                            List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr);
                            stopWatchOffset.Stop();
                            TimeSpan tsOffset = stopWatchOffset.Elapsed;
                            total = total.Add(tsOffset);
                            break;
                        case "Rotors":
                            Stopwatch stopWatchRotors = new Stopwatch();
                            stopWatchRotors.Start();
                            List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr));
                            stopWatchRotors.Stop();
                            TimeSpan tsRotors = stopWatchRotors.Elapsed;
                            break;
                        default:
                            _logger.LogWarning($"Unsure what to test: {toTest}");
                            break;
                    }

                    TimeSpan ts = new TimeSpan(total.Ticks / iterations);
                    elapsedTime = String.Format("{0:00}:{1:00}.{2:00}",ts.Minutes, ts.Seconds,ts.Milliseconds);
                    
                }
                lineToFile += "," + elapsedTime;
                linesToFile.Add(lineToFile);
                _logger.LogInformation($"Finished {toTest} {i}");
            }

            File.WriteAllLines(filePathAndName, linesToFile);
        }

        #region testing individual sections
        public double testRotor(int[] plaintext, int iterations, string fitnessStr = "")
        {
            double success = 0.0;
            for (int i = 0; i < iterations; i++)
            {
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogDebug(em.ToString());
                int[] cipherArr = _encodingService.encode(plaintext, em);

                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr,fitnessStr));

                foreach (BreakerResult br in initialRotorSetupResults)
                {
                    if (br.enigmaModel.reflector.rotor.name == em.reflector.rotor.name && br.enigmaModel.rotors[0].rotor.name == em.rotors[0].rotor.name && br.enigmaModel.rotors[1].rotor.name == em.rotors[1].rotor.name && br.enigmaModel.rotors[2].rotor.name == em.rotors[2].rotor.name)
                    {
                        _logger.LogDebug($"Correct Rotors {initialRotorSetupResults.IndexOf(br)}: {br.enigmaModel.toStringRotors()}");
                        if (br.enigmaModel.rotors[0].rotation - 1 == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset) || br.enigmaModel.rotors[0].rotation == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset) || br.enigmaModel.rotors[0].rotation + 1 == EncodingService.mod26(em2.rotors[0].rotation - em2.rotors[0].ringOffset))
                        {
                            if (br.enigmaModel.rotors[1].rotation - 1 == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset) || br.enigmaModel.rotors[1].rotation == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset) || br.enigmaModel.rotors[1].rotation + 1 == EncodingService.mod26(em2.rotors[1].rotation - em2.rotors[1].ringOffset))
                            {
                                if (br.enigmaModel.rotors[2].rotation - 1 == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset) || br.enigmaModel.rotors[2].rotation == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset) || br.enigmaModel.rotors[2].rotation + 1 == EncodingService.mod26(em2.rotors[2].rotation - em2.rotors[2].ringOffset))//this line is a cheat
                                {
                                    success += 1.0;
                                    _logger.LogDebug($"Rotor result {initialRotorSetupResults.IndexOf(br)}: {br.enigmaModel.toStringRotors()}");
                                    break;
                                }
                            }
                        }

                    }
                }
            }
            _logger.LogDebug($"Success rate: {success * 100 / iterations}%");
            return success * 100 / iterations;
        }
        public double testOffset(int[] plaintext,int iterations,string fitnessStr = "")
        {
            double success = 0.0;
            for (int i = 0; i < iterations; i++)
            {
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                EnigmaModel em3 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogDebug(em.ToString());
                int[] cipherArr = _encodingService.encode(plaintext, em);

                em2.plugboard = new Dictionary<int, int>();
                Random rnd = new Random();
                for (int ri = 0; ri < 3; ri++)
                {
                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                    em2.rotors[ri].ringOffset = 0;
                }
                _logger.LogDebug($"Input: {em2.toStringRotors()}");

                List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr,fitnessStr);
                bool found = false;
                foreach (BreakerResult brr in fullRotorOffset)
                {
                    bool correctOffset = compareOffset(em3, brr.enigmaModel);
                    if (correctOffset)
                    {
                        found = true;
                        _logger.LogDebug($"Result {fullRotorOffset.IndexOf(brr)}: {brr.enigmaModel.toStringRotors()}");
                    }
                }
                if (found)
                {
                    success += 1.0;
                }
                else
                {
                    _logger.LogDebug("Incorrect");
                }
            }
            _logger.LogDebug($"Success rate: {success * 100 / iterations}%");
            return (success * 100) / iterations;
        }
        public double testPlugboard(int[] plaintext,int iterations,string fitnessStr = "")
        {
            double success = 0.0;
            for (int i = 0; i < iterations; i++)
            {
                EnigmaModel em = EnigmaModel.randomizeEnigma(_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                string emJson = JsonConvert.SerializeObject(em);
                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                _logger.LogDebug(em.ToString());
                int[] cipherArr = _encodingService.encode(plaintext, em);

                em2.plugboard = new Dictionary<int, int>();                
                BreakerResult finalResult = _bs.getPlugboardResults(new BreakerResult(cipherArr, double.MinValue, em2), cipherArr,fitnessStr);
                _logger.LogDebug($"Final Result: {finalResult.enigmaModel.ToString()}");

                bool correctPB = comparePlugboard(em.toStringPlugboard(), finalResult.enigmaModel.toStringPlugboard());
                if (correctPB)
                {
                    success += 1.0;
                }
                else
                {
                    _logger.LogDebug("Incorrect");
                }
            }
            _logger.LogDebug($"Success rate: {success * 100 / iterations}%");
            return (success * 100) / iterations;
        }
        #endregion

        #region helper
        public bool compareRotors(string actual,string attempt)
        {
            return true;
        }
        public bool compareOffset(EnigmaModel actual,EnigmaModel attempt)
        {
            if (attempt.rotors[0].rotation == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset))
            {
                if (attempt.reflector.ToString() == actual.reflector.ToString())
                {
                    if (attempt.rotors.Count == actual.rotors.Count)
                    {
                        for (int i = 1; i < attempt.rotors.Count; i++)
                        {
                            if (attempt.rotors[i] != actual.rotors[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }                
            }
            return false;
        }
        public bool comparePlugboard(string actual, string attempt)
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
