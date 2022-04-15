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
using static EnigmaBreaker.Services.Fitness.IFitness;

namespace EnigmaBreaker.Services
{
    public class Measuring
    {
        private readonly ILogger<Measuring> _logger;
        private readonly BasicConfiguration _bc;
        private readonly EncodingService _encodingService;
        private readonly IFitness.FitnessResolver _resolver;
        private readonly BasicService _bs;
        private readonly CSVReaderService<WeightFile> _csvReader;
        private readonly PhysicalConfiguration _physicalConfiguration;
        public Measuring(ILogger<Measuring> logger, BasicConfiguration bc, EncodingService encodingService, IFitness.FitnessResolver fitnessResolver,BasicService bs,CSVReaderService<WeightFile> csvReader,PhysicalConfiguration physicalConfiguration) 
        {
            _logger = logger;
            _bc = bc;
            _encodingService = encodingService;
            _resolver = fitnessResolver;
            _bs = bs;
            _csvReader = csvReader;
            _physicalConfiguration = physicalConfiguration;
        }

        public void root()
        {
            //test();
            //testText(new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" });//3 seconds

            testLength(100, 4000, 100, Part.Plugboard, 500, new List<string>() { "IOC", "S", "BI", "TRI","QUAD" },"Results/plugboardLengthTest");//R 3.3 hours perfect
            testLength(5, 500, 5, Part.Plugboard, 1000, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/plugboardLengthTestClose");//R 9 hours perfect
            testLength(100, 4000, 100, Part.Offset, 1000, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" },"Results/offsetLengthTest");//R 2.9 hours perfect
            testLength(100, 4000, 100, Part.Rotor, 200, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" },"Results/rotorLengthTest");//R perfect
            testLength(10, 500, 10, Part.Rotor, 500, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" },"Results/rotorLengthTestClose");//R perfect

            //testPlugboardLength(100, 2000, 100, Part.Plugboard, 100, "Results/plugboardPlugboardLengthTest", 0, 10, 1); //15 hours
            //testPlugboardLength(100, 2000, 100, Part.Offset, 100, "Results/offsetPlugboardLengthTest", 0, 10, 1);//10 hour
            //testPlugboardLength(100, 4000, 100, Part.Rotor, 100, "Results/rotorPlugboardLengthTest",0,10,1);//16 hour

            //testIndex(100, 2000, 100, Part.Plugboard, 250, "Results/plugboardIndexSingleTest",1,2,1, "S");//R 13.8 hours perfect
            //testIndex(100, 2000, 100, Part.Plugboard, 100, "Results/plugboardIndexTest", 1, 3, 1, "F");//R 1 hour perfect     
            //testIndex(100, 2000, 100, Part.Offset, 100, "Results/offsetIndexSingleTest", 1, 20, 1, "S");//R 12 hours perfect
            //testIndex(100, 2000, 100, Part.Offset, 100, "Results/offsetIndexTest", 1, 20, 1, "F");//R 10.5 hours perfect
            //testIndex(100, 2000, 100, Part.Rotor, 100, "Results/rotorsIndexSingleTest", 1, 10, 1, "S"); // perfect
            //testIndex(100, 2000, 100, Part.Rotor, 100, "Results/rotorsIndexTest", 1, 10, 1, "F"); // perfect

            testSpeed(100, 4000, 100, Part.Plugboard, 5, "Results/plugboardSpeedTest", 1, 3, 1);//R 16mins perfect
            testSpeed(100, 4000, 100, Part.Offset, 5, "Results/offsetSpeedTest", 1, 20, 1);//R 1.5 perfect
            testSpeed(100, 4000, 100, Part.Rotor, 5, "Results/rotorsSpeedTest",1,10,1);//R 18 hours perfect

            //testLength(100, 4000, 100, Part.Plugboard, 1000, new List<string>() { "RULE", "WEIGHT" },"Results/plugboardComparison");//R 3.5 hour perfect
            //testLength(5, 500, 5, Part.Plugboard, 1000, new List<string>() { "RULE", "WEIGHT" }, "Results/plugboardComparisonClose");//
            //testLength(100, 4000, 100, Part.Offset, 1000, new List<string>() { "RULE", "WEIGHT" },"Results/offsetComparison");//R 3 hour
            //testLength(100, 4000, 100, Part.Rotors, 1000, new List<string>() { "RULE", "WEIGHT" },"Results/rotorComparison");//

            //measureFullRunthrough(100, 2000, 100,10, "Results/fullMeasureRefined");
            measureFullRunthrough(100, 4000, 100,1000, "Results/fullMeasureUnrefined",true);//40 hours
        }

        public void test()
        {
            var weights = _csvReader.readFromFile("C:/Users/danny/source/repos/EnigmaSimulator/EnigmaBreaker/bin/Release/netcoreapp3.1/Results/ToUse", "plugboardLengthTest_20220123_005753");
            foreach (var w in weights)
            {
                _logger.LogInformation(w.length + ": " + string.Join(Environment.NewLine, w.weights));
            }            
        }
        /// <summary>
        /// This function tests the texts that I have and saves the results to a file
        /// 
        /// It checks the IOC, letter frequency, bigrams, trigrams, quadragrams scores of all the files
        /// From these I can see which texts have the highest "Englishness" score.
        /// I also made the fitness weights based upon these values
        /// </summary>
        public void testText(List<string> fitnessList)
        {            
            List<string> linesToWriteToFile = new List<string>() { "Name,CharCount," + string.Join(",", fitnessList) };
            
            foreach (string fileName in _bc.textFileNames)
            {
                string newLine = fileName;
                string plaintext = System.IO.File.ReadAllText(System.IO.Path.Combine(_bc.textDir, fileName + ".txt"));
                List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();
                newLine += $",{plainArr.Count}";
                foreach (string fitnessStr in fitnessList)
                {
                    IFitness fitnessFunc = _resolver(fitnessStr);
                    double fitness = fitnessFunc.getFitness(plainArr.ToArray());
                    newLine += "," + fitness;
                }                
                linesToWriteToFile.Add(newLine);
            }
            File.WriteAllLines("Results/TextTest_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToWriteToFile);
        }
        /// <summary>
        /// Measure the accuracy of the full run through of the breaker
        /// Used to track the accuracy asI attempted to improve it
        /// </summary>
        /// <param name="from"></param> starting ciphertext length
        /// <param name="to"></param> finishing ciphertext length
        /// <param name="step"></param> step in cipertext length
        /// <param name="iterations"></param> number of iteration to do to get the accuracy
        /// <param name="filePathAndName"></param> location to save the results
        /// <param name="withoutRefinement"></param> True if you want to use the most recent version else false
        public void measureFullRunthrough(int from, int to, int step, int iterations, string filePathAndName,bool withoutRefinement)
        {
            string plaintext = _bs.getText(to * 2);
            
            List<string> linesToFile = new List<string>() { ",RotorSuccess,OffsetSuccess,PlugboardSuccess,FullSuccess,RotorTime,OffsetTime,PlugboardTime" };
            for (int i = from; i <= to; i += step)
            {                
                int[] plainArr = EncodingService.preProccessCiphertext(plaintext).ToList().GetRange(0, i).ToArray();

                int rotorSuccess = 0;
                int offsetSuccess = 0;
                int plugboardSuccess = 0;
                TimeSpan rotorTS = TimeSpan.Zero;
                TimeSpan offsetTS = TimeSpan.Zero;
                TimeSpan plugboardTS = TimeSpan.Zero;
                for (int currentIteration = 0; currentIteration < iterations; currentIteration++)
                {
                    EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration,_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                    string emJson = JsonConvert.SerializeObject(em);
                    EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                    _logger.LogDebug(em.ToString());
                    int[] cipherArr = _encodingService.encode(plainArr, em);
                    BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);
                    Stopwatch stopWatchRotor = new Stopwatch();
                    stopWatchRotor.Start();
                    List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, breakerConfiguration));
                    stopWatchRotor.Stop();
                    TimeSpan tsRotorSingle = stopWatchRotor.Elapsed;
                    rotorTS = rotorTS.Add(tsRotorSingle);
                    bool found = false;
                    foreach (BreakerResult br in initialRotorSetupResults)
                    {
                        if (compareRotors(em2, br.enigmaModel))
                        {
                            found = true;
                            rotorSuccess++;
                            break;
                        }
                    }
                    if (found)
                    {
                        Stopwatch stopWatchOffset = new Stopwatch();
                        stopWatchRotor.Start();
                        List<BreakerResult> fullRotorResultOfAll = _bs.getRotationOffsetResult(initialRotorSetupResults, cipherArr, breakerConfiguration);
                        stopWatchOffset.Stop();
                        TimeSpan tsOffsetSingle = stopWatchOffset.Elapsed;
                        offsetTS = offsetTS.Add(tsOffsetSingle);
                        found = false;
                        foreach (BreakerResult brr in fullRotorResultOfAll)
                        {
                            if (compareOffset(em2, brr.enigmaModel))
                            {
                                found = true;
                                offsetSuccess++;
                                break;
                            }
                        }
                        if (found)
                        {
                            Stopwatch stopWatchPlugboard = new Stopwatch();
                            stopWatchPlugboard.Start();
                            List<BreakerResult> finalResult = _bs.getPlugboardResults(fullRotorResultOfAll, cipherArr, breakerConfiguration);
                            stopWatchPlugboard.Stop();
                            TimeSpan tsPlugboardSingle = stopWatchPlugboard.Elapsed;
                            plugboardTS = plugboardTS.Add(tsPlugboardSingle);
                            if (comparePlugboard(em2.toStringPlugboard(), finalResult[0].enigmaModel.toStringPlugboard()))
                            {
                                plugboardSuccess++;
                                break;
                            }                            
                        }
                    }
                }
                TimeSpan tsRotor = new TimeSpan(rotorTS.Ticks / iterations);
                TimeSpan tsOffset = TimeSpan.Zero;
                TimeSpan tsPlugboard = TimeSpan.Zero;
                if (iterations - (iterations - rotorSuccess) > 0)
                {
                    tsOffset = new TimeSpan(rotorTS.Ticks / (iterations - (iterations - rotorSuccess)));
                }
                if ((iterations - (iterations - rotorSuccess) - ((iterations - (iterations - rotorSuccess)) - offsetSuccess)) > 0)
                {
                    tsPlugboard = new TimeSpan(rotorTS.Ticks / (iterations - (iterations - rotorSuccess) - ((iterations - (iterations - rotorSuccess)) - offsetSuccess)));
                }              
                
                double rotorSuccessRate = (double)(rotorSuccess * 100 / iterations);
                double offsetSuccessRate = (double)(offsetSuccess * 100 / (iterations - (iterations - rotorSuccess)));
                double plugboardSuccessRate = (double)(plugboardSuccess * 100 / (iterations - (iterations - rotorSuccess) - ((iterations - (iterations - rotorSuccess)) - offsetSuccess)));
                double fullSuccessRate = (double)(plugboardSuccess * 100 / iterations);
                _logger.LogDebug($"Rotor Success: {rotorSuccessRate}%");//RotorMiss = iteration - rotorSuccess
                _logger.LogDebug($"Offset Success: {offsetSuccessRate}%");//OffsetMiss = (iterations - (iterations-rotorSuccess)) - offsetSuccess
                _logger.LogDebug($"Plugboard Success: {plugboardSuccessRate}%");
                _logger.LogDebug($"Success: {fullSuccessRate}%");
                string lineToFile = $"{i},{rotorSuccessRate},{offsetSuccessRate},{plugboardSuccessRate},{fullSuccessRate},{tsRotor.Hours}:{tsRotor.Minutes}:{tsRotor.Seconds}.{tsRotor.Milliseconds},{tsOffset.Hours}:{tsOffset.Minutes}:{tsOffset.Seconds}.{tsOffset.Milliseconds},{tsPlugboard.Hours}:{tsPlugboard.Minutes}:{tsPlugboard.Seconds}.{tsPlugboard.Milliseconds}";
                linesToFile.Add(lineToFile);
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);
            _logger.LogInformation("Writing to file");
        }
        /// <summary>
        /// Tests different fitness functions accuracies at different lengths of ciphertext
        /// </summary>
        /// <param name="from"></param> starting ciphertext length
        /// <param name="to"></param> finishing ciphertext length
        /// <param name="step"></param> step in cipertext length 
        /// <param name="toTest"></param> Area to test
        /// <param name="iterations"></param> number of iteration to do to get the accuracy
        /// <param name="fitnessStrToTest"></param> List of the different fitness function keys to test
        /// <param name="filePathAndName"></param> Location to save results
        public void testLength(int from, int to, int step, Part toTest, int iterations, List<string> fitnessStrToTest, string filePathAndName)
        {
            int trueNumOfRotors = _bc.numberOfRotorsInUse;
            int trueNumOfRelfectors = _bc.numberOfReflectorsInUse;            
            if (toTest == Part.Rotor)
            {                
                _bc.numberOfRotorsInUse = 3;
                _bc.numberOfReflectorsInUse = 1;
                _bs.setNumOfRotors(3);
                _bs.setNumOfReflectors(1);
            }
            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();
            List<string> linesToFile = new List<string>() { "," + string.Join(",", fitnessStrToTest) };
            for (int lengthOfPlainText = from; lengthOfPlainText < to + 1; lengthOfPlainText += step)
            {
                List<int> correctCount = new List<int>();
                foreach (string s in fitnessStrToTest)
                {
                    correctCount.Add(0);
                }
                string lineToFile = $"{lengthOfPlainText}";
                for (int i = 0; i < iterations; i++)
                {
                    EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                    foreach (string fitnessStr in fitnessStrToTest)
                    {
                        bool wasCorrect = false;
                        switch (toTest)
                        {
                            case Part.Plugboard:                                
                                wasCorrect = testPlugboard(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr,true);
                                break;
                            case Part.Offset:
                                wasCorrect = testOffset(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr,true);
                                break;
                            case Part.Rotor:                                
                                wasCorrect = testRotor(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr,true);                                
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        } 
                        if (wasCorrect)
                        {
                            correctCount[fitnessStrToTest.FindIndex(a => a.Contains(fitnessStr))]++;
                        }
                    }
                }
                _logger.LogInformation($"Finished {toTest} {lengthOfPlainText}");
                foreach(int i in correctCount)
                {
                    lineToFile += $", {100 * (double)i/iterations}";
                }
                linesToFile.Add(lineToFile);
            }
            
            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);
            _logger.LogInformation("Writing to file");
            if (toTest == Part.Rotor)
            {
                _bs.setNumOfRotors(_bc.numberOfRotorsInUse);
                _bs.setNumOfReflectors(_bc.numberOfReflectorsInUse);
                _bc.numberOfRotorsInUse = trueNumOfRotors;
                _bc.numberOfReflectorsInUse = trueNumOfRelfectors;
            }
        }
        /// <summary>
        /// Tests the speed of decryption over different lengths of text and different single index sizes
        /// </summary>
        /// <param name="from"></param> starting ciphertext length
        /// <param name="to"></param> finishing ciphertext length
        /// <param name="step"></param> step in cipertext length 
        /// <param name="toTest"></param> Area to test
        /// <param name="iterations"></param> number of iteration to do to get the accuracy
        /// <param name="filePathAndName"></param> Location to save results
        /// <param name="singleFrom"></param> single index from value
        /// <param name="singleTo"></param> single index to value
        /// <param name="singleStep"></param> single index step value
        public void testSpeed(int from, int to, int step, Part toTest, int iterations, string filePathAndName, int singleFrom, int singleTo, int singleStep)
        {
            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();
            string s = "";
            for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)
            {
                s += $", {combinationValue}";
            }
            List<string> linesToFile = new List<string>() { s };
            for (int i = from; i < to + 1; i += step)
            {
                string lineToFile = $"{i}";
                string elapsedTime = "";
                for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)
                {
                    TimeSpan total = TimeSpan.Zero;
                    for (int j = 0; j < iterations; j++)
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                        string emJson = JsonConvert.SerializeObject(em);
                        int[] cipherArr = _encodingService.encode(plainArr.GetRange(0, i).ToArray(), em);
                        
                        BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length);

                        switch (toTest)
                        {
                            case Part.Plugboard:
                                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                em2.plugboard = new Dictionary<int, int>();
                                
                                breakerConfiguration.numberOfSinglePlugboardSettingsToKeep = combinationValue;
                                
                                Stopwatch stopWatchPlugboard = new Stopwatch();
                                stopWatchPlugboard.Start();
                                List<BreakerResult> finalResult = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, breakerConfiguration);
                                stopWatchPlugboard.Stop();
                                TimeSpan tsPlugboard = stopWatchPlugboard.Elapsed;
                                total = total.Add(tsPlugboard);
                                break;
                            case Part.Offset:
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                em2.plugboard = new Dictionary<int, int>();
                                Random rnd = new Random();
                                for (int ri = 0; ri < 3; ri++)
                                {
                                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                                    em2.rotors[ri].ringOffset = 0;
                                }                                
                                breakerConfiguration.numberOfSettingsPerRotationCombinationToKeep = combinationValue;
                                
                                Stopwatch stopWatchOffset = new Stopwatch();
                                stopWatchOffset.Start();
                                List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, breakerConfiguration);
                                stopWatchOffset.Stop();
                                TimeSpan tsOffset = stopWatchOffset.Elapsed;
                                total = total.Add(tsOffset);
                                break;
                            case Part.Rotor:                                
                                breakerConfiguration.numberOfSettingsPerRotorCombinationToKeep = combinationValue;
                                
                                Stopwatch stopWatchRotors = new Stopwatch();
                                stopWatchRotors.Start();
                                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, breakerConfiguration));
                                stopWatchRotors.Stop();
                                TimeSpan tsRotors = stopWatchRotors.Elapsed;
                                total = total.Add(tsRotors);
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }                        
                    }
                    TimeSpan ts = new TimeSpan(total.Ticks / iterations);
                    elapsedTime += $",{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";
                }
                lineToFile += elapsedTime;
                linesToFile.Add(lineToFile);
                _logger.LogInformation($"Finished {toTest} {i}");
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);
            _logger.LogInformation("Writing to file");
        }
        /// <summary>
        /// Testing how accurate the breaker is with different index parameters so that I can get the optimal amount
        /// </summary>
        /// <param name="from"></param> starting ciphertext length
        /// <param name="to"></param> finishing ciphertext length
        /// <param name="step"></param> step in cipertext length 
        /// <param name="toTest"></param> Area to test
        /// <param name="iterations"></param> number of iteration to do to get the accuracy
        /// <param name="filePathAndName"></param> Location to save results
        /// <param name="singleFrom"></param> single index from value
        /// <param name="singleTo"></param> single index to value
        /// <param name="singleStep"></param> single index step value
        /// <param name="isTotal"></param> True for overall index testing false for single inde testing
        public void testIndex(int from, int to, int step, Part toTest, int iterations, string filePathAndName,int singleFrom,int singleTo,int singleStep, string isTotal)
        {
            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();
            string s = "";
            for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)
            {
                s += $", {combinationValue}";
            }
            List<string> linesToFile = new List<string>() { s };
            for (int messageLength = from; messageLength < to + 1; messageLength += step)
            {
                string lineToFile = $"{messageLength}";
                for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)
                {
                    int missNumber = 0;
                    for (int currentIteration = 0; currentIteration < iterations; currentIteration++)
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
                        string emJson = JsonConvert.SerializeObject(em);
                        int[] cipherArr = _encodingService.encode(plainArr.GetRange(0, messageLength).ToArray(), em);
                        BreakerConfiguration bc = new BreakerConfiguration(cipherArr.Length);
                        bc.numberOfSettingsPerRotorCombinationToKeep = 10;
                        bc.numberOfRotorsToKeep = 5;
                        bc.numberOfPlugboardSettingsToKeep = 1;
                        bc.numberOfSinglePlugboardSettingsToKeep = 1;
                        bc.numberOfOffsetToKeep = 5;
                        bc.numberOfSettingsPerRotationCombinationToKeep = 26;
                        EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

                        switch (toTest)
                        {
                            case Part.Plugboard:
                                em2.plugboard = new Dictionary<int, int>();
                                if (isTotal == "F")
                                {
                                    bc.numberOfPlugboardSettingsToKeep = combinationValue;
                                }
                                else if (isTotal == "S")
                                {
                                    bc.numberOfSinglePlugboardSettingsToKeep = combinationValue;
                                }                                

                                List<BreakerResult> finalResults = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, bc);
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                bool foundP = false;
                                foreach (BreakerResult br in finalResults)
                                {
                                    if (comparePlugboard(em2.toStringPlugboard(), br.enigmaModel.toStringPlugboard()))
                                    {
                                        foundP = true;
                                        break;
                                    }
                                }
                                if (!foundP)
                                {
                                    missNumber++;
                                }
                                break;
                            case Part.Offset:
                                em2.plugboard = new Dictionary<int, int>();
                                Random rnd = new Random();
                                for (int ri = 0; ri < 3; ri++)
                                {
                                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                                    em2.rotors[ri].ringOffset = 0;
                                }
                                
                                if (isTotal == "F")
                                {
                                    bc.numberOfOffsetToKeep = combinationValue;
                                }
                                else
                                {
                                    bc.numberOfSettingsPerRotationCombinationToKeep = combinationValue;
                                }

                                List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, bc);
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                bool foundO = false;
                                foreach (BreakerResult br in fullRotorOffset)
                                {
                                    if (compareOffset(em2, br.enigmaModel))
                                    {
                                        foundO = true;
                                        break;
                                    }
                                }
                                if (!foundO)
                                {
                                    missNumber++;
                                }

                                
                                break;
                            case Part.Rotor:                                
                                if (isTotal == "F")
                                {
                                    bc.numberOfRotorsToKeep = combinationValue;
                                }
                                else
                                {
                                    bc.numberOfSettingsPerRotorCombinationToKeep = combinationValue;
                                }

                                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, bc));
                                bool foundR = false;
                                foreach (BreakerResult br in initialRotorSetupResults)
                                {
                                    if (compareRotors(em2, br.enigmaModel))
                                    {
                                        foundR = true;
                                        break;
                                    }
                                }
                                if (!foundR)
                                {
                                    missNumber++;
                                }
                                
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }
                    }
                    lineToFile += "," + Convert.ToDouble(missNumber * 100 / iterations);                    
                }
                linesToFile.Add(lineToFile);
                _logger.LogInformation($"Finished {toTest} {messageLength}");
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);
            _logger.LogInformation("Writing to file");
        }
        /// <summary>
        /// Test how the accuracy of the different parts varies with different lengths of text and different plugboard lengths
        /// </summary>
        /// <param name="from"></param> starting ciphertext length
        /// <param name="to"></param> finishing ciphertext length
        /// <param name="step"></param> step in cipertext length 
        /// <param name="toTest"></param> Area to test
        /// <param name="iterations"></param> number of iteration to do to get the accuracy
        /// <param name="filePathAndName"></param> Location to save results
        /// <param name="plugboardFrom"></param>
        /// <param name="plugboardTo"></param>
        /// <param name="plugbopardStep"></param>
        public void testPlugboardLength(int from, int to, int step, Part toTest, int iterations, string filePathAndName, int plugboardFrom, int plugboardTo, int plugbopardStep)
        {
            int trueNumOfRotors = _bc.numberOfRotorsInUse;
            int trueNumOfRelfectors = _bc.numberOfReflectorsInUse;
            if (toTest == Part.Rotor)
            {
                _bc.numberOfRotorsInUse = 3;
                _bc.numberOfReflectorsInUse = 1;
                _bs.setNumOfRotors(3);
                _bs.setNumOfReflectors(1);
            }

            string plaintext = _bs.getText(to * 2);
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();
            string s = "";
            for(int i = plugboardFrom; i <= plugboardTo; i+= plugbopardStep)
            {
                s += $",{i}";
            }
            List<string> linesToFile = new List<string>() { s };
            for (int lengthOfPlainText = from; lengthOfPlainText < to + 1; lengthOfPlainText += step)
            {
                string lineToFile = $"{lengthOfPlainText}";
                for (int plugboardLength = plugboardFrom; plugboardLength <= plugboardTo; plugboardLength += plugbopardStep)
                {
                    int correctCount = 0;
                    for (int i = 0; i < iterations; i++)
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, plugboardLength,true);
                        
                        bool wasCorrect = false;
                        switch (toTest)
                        {
                            case Part.Plugboard:
                                wasCorrect = testPlugboard(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);
                                break;
                            case Part.Offset:
                                wasCorrect = testOffset(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);
                                break;
                            case Part.Rotor:
                                wasCorrect = testRotor(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }
                        if (wasCorrect)
                        {
                            correctCount++;
                        }
                        
                    }
                    lineToFile += $", {100*((double)correctCount / iterations)}";
                }
                _logger.LogInformation($"Finished {toTest} {lengthOfPlainText}");                
                linesToFile.Add(lineToFile);
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);
            _logger.LogInformation("Writing to file");

            if (toTest == Part.Rotor)
            {
                _bs.setNumOfRotors(_bc.numberOfRotorsInUse);
                _bs.setNumOfReflectors(_bc.numberOfReflectorsInUse);
                _bc.numberOfRotorsInUse = trueNumOfRotors;
                _bc.numberOfReflectorsInUse = trueNumOfRelfectors;
            }
        }
        #region testing individual sections
        /// <summary>
        /// Returns a bool of if the plaintext encoded by the enigma model was decoded correctly
        /// </summary>
        /// <param name="plaintext"></param> The plaintext to test on as 
        /// <param name="em"></param> (Optional) Enigma model to encode with
        /// <param name="fitnessStr"></param> (Optional) String of fitness model to decode with
        /// <returns></returns>
        public bool testRotor(int[] plaintext, EnigmaModel em = null, string fitnessStr = "",bool withoutRefinement=false)
        {
            if(em == null)
            {
                em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
            }            
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

            _logger.LogDebug(em.ToString());
            int[] cipherArr = _encodingService.encode(plaintext, em);

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);
            if (fitnessStr != "") { breakerConfiguration.RotorFitness = fitnessStr; }
            List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr,breakerConfiguration));

            foreach (BreakerResult br in initialRotorSetupResults)
            {
                bool compareRotor = compareRotors(em2, br.enigmaModel);
                if (compareRotor)
                {
                    _logger.LogDebug($"Rotor result {initialRotorSetupResults.IndexOf(br)}: {br.enigmaModel.toStringRotors()}");
                    return true;
                }                 
            }
            return false;
        }
        /// <summary>
        /// Returns a bool of if the plaintext encoded by the enigma model was decoded correctly
        /// </summary>
        /// <param name="plaintext"></param> The plaintext to test on as 
        /// <param name="em"></param> (Optional) Enigma model to encode with
        /// <param name="fitnessStr"></param> (Optional) String of fitness model to decode with
        /// <returns></returns>
        public bool testOffset(int[] plaintext, EnigmaModel em = null, string fitnessStr = "", bool withoutRefinement = false)
        {
            if (em == null)
            {
                em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
            }
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

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);
            if (fitnessStr != "") { breakerConfiguration.OffsetFitness = fitnessStr; }

            List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr,breakerConfiguration);
            
            foreach (BreakerResult brr in fullRotorOffset)
            {
                bool correctOffset = compareOffset(em3, brr.enigmaModel);
                if (correctOffset)
                {                    
                    _logger.LogDebug($"Result {fullRotorOffset.IndexOf(brr)}: {brr.enigmaModel.toStringRotors()}");
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns a bool of if the plaintext encoded by the enigma model was decoded correctly
        /// </summary>
        /// <param name="plaintext"></param> The plaintext to test on as 
        /// <param name="em"></param> (Optional) Enigma model to encode with
        /// <param name="fitnessStr"></param> (Optional) String of fitness model to decode with
        /// <returns></returns>
        public bool testPlugboard(int[] plaintext,EnigmaModel em = null, string fitnessStr = "", bool withoutRefinement = false)
        {
            if (em == null)
            {
                em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);
            }
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

            _logger.LogDebug(em.ToString());
            int[] cipherArr = _encodingService.encode(plaintext, em);

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);
            if (fitnessStr != "") { breakerConfiguration.PlugboardFitness = fitnessStr; }
            em2.plugboard = new Dictionary<int, int>();                
            List<BreakerResult> finalResult = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr,breakerConfiguration);
            foreach (BreakerResult br in finalResult)
            {
                bool correctPB = comparePlugboard(em.toStringPlugboard(), br.enigmaModel.toStringPlugboard());
                if (correctPB)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region helper
        /// <summary>
        /// Compares the rotors to make sure that they are near enough the same to progress onto the next stage
        /// </summary>
        /// <param name="actual"></param> Actual Enigma model
        /// <param name="attempt"></param> Attempt at the enigma model
        /// <returns> boolean of if the rotors are the "Same"</returns>
        public bool compareRotors(EnigmaModel actual,EnigmaModel attempt)
        {
            if (attempt.reflector.rotor.name == actual.reflector.rotor.name && attempt.rotors[0].rotor.name == actual.rotors[0].rotor.name && attempt.rotors[1].rotor.name == actual.rotors[1].rotor.name && attempt.rotors[2].rotor.name == actual.rotors[2].rotor.name)
            {
                if (attempt.rotors[0].rotation - 1 == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) || attempt.rotors[0].rotation == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) || attempt.rotors[0].rotation + 1 == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset))
                {
                    if (attempt.rotors[1].rotation - 1 == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset) || attempt.rotors[1].rotation == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset) || attempt.rotors[1].rotation + 1 == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset))
                    {
                        if (attempt.rotors[2].rotation - 1 == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset) || attempt.rotors[2].rotation == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset) || attempt.rotors[2].rotation + 1 == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset))//this line is a cheat
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Compares the rotors offsets to make sure that they are near enough the same to progress onto the next stage
        /// </summary>
        /// <param name="actual"></param> Actual Enigma model
        /// <param name="attempt"></param> Attempt at the enigma model
        /// <returns> boolean of if the rotors are the "Same"</returns>
        public bool compareOffset(EnigmaModel actual,EnigmaModel attempt)
        {
            if (attempt.rotors[0].rotation == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) && attempt.rotors[0].ringOffset == 0)
            {
                string[] actualSplit = actual.toStringRotors().Split("/");
                string actualStr = actualSplit[0]  + "/" + actualSplit[2] + "/" + actualSplit[3];
                string[] attemptSplit = attempt.toStringRotors().Split("/");
                string attemptStr = attemptSplit[0] + "/" + attemptSplit[2] + "/" + attemptSplit[3];
                if(actualStr == attemptStr)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Compares the plugboards to make sure that they are the same
        /// </summary>
        /// <param name="actual"></param> Actual Enigma model plugboard string
        /// <param name="attempt"></param> Attempt at the enigma model plugboard string
        /// <returns> boolean of if the plugboards are equivilent</returns>
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

        public double accuracyPercentage(string plaintext,string attempt)
        {
            double accuracy = 0;
            return accuracy;
        }
        #endregion
    }
}
