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
        /// <summary>
        /// All tests are run from here and uncommented when needed to be run
        /// </summary>
        public void root()
        {
            //test();
            //testText(new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" });//3 seconds
            //These are all run unrefined
            //testLength(100, 4000, 100, Part.Plugboard, 250, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/plugboardLengthTest", true);//R 3.3 hours perfect
            //testLength(5, 500, 5, Part.Plugboard, 500, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/plugboardLengthTestClose", true);//R 9 hours perfect
            //testLength(100, 4000, 100, Part.Offset, 500, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/offsetLengthTest", true);//R 2.9 hours perfect
            //testLength(100, 4000, 100, Part.Rotor, 100, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/rotorLengthTest", true);//R perfect            
            //testLength(10, 500, 10, Part.Rotor, 100, new List<string>() { "IOC", "S", "BI", "TRI", "QUAD" }, "Results/rotorLengthTestClose", true);//R perfect

            //Then I take modify the breaker configuration to make the rules
            testLength(100, 4000, 100, Part.Plugboard, 250, new List<string>() { "RULE", "WEIGHT" },"Results/plugboardComparison");//R 3.5 hour perfect
            testLength(5, 500, 5, Part.Plugboard, 500, new List<string>() { "RULE", "WEIGHT" }, "Results/plugboardComparisonClose");//
            testLength(100, 4000, 100, Part.Offset, 500, new List<string>() { "RULE", "WEIGHT" },"Results/offsetComparison");//R 3 hour
            testLength(100, 4000, 100, Part.Rotor, 100, new List<string>() { "RULE", "WEIGHT" },"Results/rotorComparison");//
            testLength(10, 500, 10, Part.Rotor, 100, new List<string>() { "RULE","WEIGHT" }, "Results/rotorLengthTestClose");//R perfect

            //Modify breaker configuration model, after that I set the indexes for every part
            testIndex(100, 4000, 100, Part.Plugboard, 250, "Results/plugboardIndexSingleTest",1,2,1, "S");//R 13.8 hours perfect
            testIndex(100, 4000, 100, Part.Plugboard, 250, "Results/plugboardIndexTest", 1, 3, 1, "F");//R 1 hour perfect     
            testIndex(100, 4000, 100, Part.Offset, 250, "Results/offsetIndexSingleTest", 1, 20, 1, "S");//R 12 hours perfect
            testIndex(100, 4000, 100, Part.Offset, 250, "Results/offsetIndexTest", 1, 20, 1, "F");//R 10.5 hours perfect
            testIndex(100, 4000, 100, Part.Rotor, 100, "Results/rotorsIndexSingleTest", 1, 10, 1, "S"); // perfect
            testIndex(100, 4000, 100, Part.Rotor, 100, "Results/rotorsIndexTest", 1, 10, 1, "F"); // perfect

            //Now it is ready to test how it varies based on plugboard length
            //testPlugboardLength(100, 4000, 100, Part.Plugboard, 250, "Results/plugboardPlugboardLengthTest", 0, 10, 1); //15 hours
            //testPlugboardLength(100, 4000, 100, Part.Offset, 500, "Results/offsetPlugboardLengthTest", 0, 10, 1);//10 hour
            //testPlugboardLength(100, 4000, 100, Part.Rotor, 100, "Results/rotorPlugboardLengthTest",0,10,1);//16 hour

            //testSpeed(100, 4000, 100, Part.Plugboard, 5, "Results/plugboardSpeedTest", 1, 2, 1);//R 16mins perfect
            //testSpeed(100, 4000, 100, Part.Offset, 5, "Results/offsetSpeedTest", 1, 20, 1);//R 1.5 perfect
            //testSpeed(100, 4000, 100, Part.Rotor, 5, "Results/rotorsSpeedTest", 1, 10, 1);//R 18 hours perfect

            //measureFullRunthrough(100, 4000, 100, 100, "Results/fullMeasureRefined");
            //measureFullRunthrough(100, 4000, 100, 100, "Results/fullMeasureUnrefined", true);//40 hours
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
            
            foreach (string fileName in _bc.textFileNames)//for each file
            {
                string newLine = fileName;
                string plaintext = System.IO.File.ReadAllText(System.IO.Path.Combine(_bc.textDir, fileName + ".txt"));//read the file
                List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();//proccess the file into an integer array
                newLine += $",{plainArr.Count}";
                foreach (string fitnessStr in fitnessList)//for each fitness function
                {
                    IFitness fitnessFunc = _resolver(fitnessStr);
                    double fitness = fitnessFunc.getFitness(plainArr.ToArray());//get the fitness
                    newLine += "," + fitness;//add the fitness to the line to write
                }                
                linesToWriteToFile.Add(newLine);//add the new line to the output
            }
            File.WriteAllLines("Results/TextTest_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToWriteToFile);//write all lines to file
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
            string plaintext = _bs.getText(to * 2);//gets the random text
            
            List<string> linesToFile = new List<string>() { ",RotorSuccess,OffsetSuccess,PlugboardSuccess,FullSuccess,RotorTime,OffsetTime,PlugboardTime" };
            for (int lengthOfText = from; lengthOfText <= to; lengthOfText += step)//for each length of text
            {                
                int[] plainArr = EncodingService.preProccessCiphertext(plaintext).ToList().GetRange(0, lengthOfText).ToArray();//cut the text to size required
                //set the success counts and timers to 0
                int rotorSuccess = 0;
                int offsetSuccess = 0;
                int plugboardSuccess = 0;
                TimeSpan rotorTS = TimeSpan.Zero;
                TimeSpan offsetTS = TimeSpan.Zero;
                TimeSpan plugboardTS = TimeSpan.Zero;
                for (int currentIteration = 0; currentIteration < iterations; currentIteration++)//for each iteration
                {
                    EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration,_bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//randomize and enigma
                    string emJson = JsonConvert.SerializeObject(em);
                    EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//get copy of the enigma model to compare to

                    _logger.LogDebug(em.ToString());
                    int[] cipherArr = _encodingService.encode(plainArr, em);//encode to get the cipher array
                    BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);//get the configuration
                    Stopwatch stopWatchRotor = new Stopwatch();
                    stopWatchRotor.Start();//start the timer
                    List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, breakerConfiguration));//get the rotors
                    stopWatchRotor.Stop();//stop the timer
                    TimeSpan tsRotorSingle = stopWatchRotor.Elapsed;
                    rotorTS = rotorTS.Add(tsRotorSingle);
                    bool found = false;
                    foreach (BreakerResult br in initialRotorSetupResults)//search for correcy rotors
                    {
                        if (compareRotors(em2, br.enigmaModel))
                        {
                            found = true;
                            rotorSuccess++;
                            break;
                        }
                    }
                    if (found)//if the rotors were correct move onto the next
                    {
                        Stopwatch stopWatchOffset = new Stopwatch();
                        stopWatchRotor.Start();//start stopwatch
                        List<BreakerResult> fullRotorResultOfAll = _bs.getRotationOffsetResult(initialRotorSetupResults, cipherArr, breakerConfiguration);//get the offset
                        stopWatchOffset.Stop();//stop stopwatch
                        TimeSpan tsOffsetSingle = stopWatchOffset.Elapsed;
                        offsetTS = offsetTS.Add(tsOffsetSingle);
                        found = false;
                        foreach (BreakerResult brr in fullRotorResultOfAll)//search for offset result
                        {
                            if (compareOffset(em2, brr.enigmaModel))
                            {
                                found = true;
                                offsetSuccess++;
                                break;
                            }
                        }
                        if (found)//if the offset was correct in the array
                        {
                            Stopwatch stopWatchPlugboard = new Stopwatch();
                            stopWatchPlugboard.Start();//start stop watch
                            List<BreakerResult> finalResult = _bs.getPlugboardResults(fullRotorResultOfAll, cipherArr, breakerConfiguration);//get the plugboard
                            stopWatchPlugboard.Stop();//stop timer
                            TimeSpan tsPlugboardSingle = stopWatchPlugboard.Elapsed;
                            plugboardTS = plugboardTS.Add(tsPlugboardSingle);
                            if (comparePlugboard(em2.toStringPlugboard(), finalResult[0].enigmaModel.toStringPlugboard()))//search for correct plugboard
                            {
                                plugboardSuccess++;
                                break;
                            }                            
                        }
                    }
                }
                //getting timing and accuracy results ready for the file
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
                string lineToFile = $"{lengthOfText},{rotorSuccessRate},{offsetSuccessRate},{plugboardSuccessRate},{fullSuccessRate},{tsRotor.Hours}:{tsRotor.Minutes}:{tsRotor.Seconds}.{tsRotor.Milliseconds},{tsOffset.Hours}:{tsOffset.Minutes}:{tsOffset.Seconds}.{tsOffset.Milliseconds},{tsPlugboard.Hours}:{tsPlugboard.Minutes}:{tsPlugboard.Seconds}.{tsPlugboard.Milliseconds}";//set line to write to the csv file
                linesToFile.Add(lineToFile);
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);//write line to csv file
            _logger.LogInformation("Writing to file");
        }
        
        #region tests
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
        public void testLength(int from, int to, int step, Part toTest, int iterations, List<string> fitnessStrToTest, string filePathAndName,bool withoutRefinement = false)
        {
            int trueNumOfRotors = _bc.numberOfRotorsInUse;
            int trueNumOfRelfectors = _bc.numberOfReflectorsInUse;            
            if (toTest == Part.Rotor)//if the part to test is rotors
            {                
                //set the number of rotors in use to 3 and the number of reflectors to 1
                _bc.numberOfRotorsInUse = 3;
                _bc.numberOfReflectorsInUse = 1;
                _bs.setNumOfRotors(3);
                _bs.setNumOfReflectors(1);
            }
            string plaintext = _bs.getText(to * 2);//get text
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();//convert plaintext to int list
            List<string> linesToFile = new List<string>() { "," + string.Join(",", fitnessStrToTest) };
            for (int lengthOfPlainText = from; lengthOfPlainText < to + 1; lengthOfPlainText += step)//for each length of text
            {
                List<int> correctCount = new List<int>();
                foreach (string s in fitnessStrToTest) { correctCount.Add(0); }//fill the list with 0 for each fitness string
                string lineToFile = $"{lengthOfPlainText}";
                for (int i = 0; i < iterations; i++)//for each iteration
                {
                    EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//randomize enigma model
                    foreach (string fitnessStr in fitnessStrToTest)//for each fitness string
                    {
                        bool wasCorrect = false;
                        switch (toTest)//run test on correct part to test
                        {
                            case Part.Plugboard:                                
                                wasCorrect = testPlugboard(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr, withoutRefinement);
                                break;
                            case Part.Offset:
                                wasCorrect = testOffset(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr, withoutRefinement);
                                break;
                            case Part.Rotor:                                
                                wasCorrect = testRotor(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em, fitnessStr, withoutRefinement);                                
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        } 
                        if (wasCorrect)//if it was correct
                        {
                            correctCount[fitnessStrToTest.FindIndex(a => a.Contains(fitnessStr))]++;//add some to the correct count
                        }
                    }
                }
                _logger.LogInformation($"Finished {toTest} {lengthOfPlainText}");
                foreach(int i in correctCount) { lineToFile += $", {100 * (double)i/iterations}"; }//add the amount correct to the line to write in the file
                linesToFile.Add(lineToFile);
            }
            
            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);//write to file
            _logger.LogInformation("Writing to file");

            if (toTest == Part.Rotor)//if the part to test was rotors
            {
                //set the rotor number back
                _bs.setNumOfRotors(_bc.numberOfRotorsInUse);
                _bs.setNumOfReflectors(_bc.numberOfReflectorsInUse);
                _bc.numberOfRotorsInUse = trueNumOfRotors;
                _bc.numberOfReflectorsInUse = trueNumOfRelfectors;
            }
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
        public void testIndex(int from, int to, int step, Part toTest, int iterations, string filePathAndName, int singleFrom, int singleTo, int singleStep, string isTotal)
        {
            string plaintext = _bs.getText(to * 2);//get some plain text
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();//convert text to integer list
            string s = "";
            //write headers
            for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)
            {
                s += $", {combinationValue}";
            }
            List<string> linesToFile = new List<string>() { s };
            for (int messageLength = from; messageLength < to + 1; messageLength += step)//for eahc message length
            {
                string lineToFile = $"{messageLength}";
                for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)//for each index value
                {
                    int missNumber = 0;
                    for (int currentIteration = 0; currentIteration < iterations; currentIteration++)//for each iteration
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//randomize enigma model
                        string emJson = JsonConvert.SerializeObject(em);
                        int[] cipherArr = _encodingService.encode(plainArr.GetRange(0, messageLength).ToArray(), em);//encode message at correct length
                        BreakerConfiguration bc = new BreakerConfiguration(cipherArr.Length, true);//get unaltered breaker configuration
                        BreakerConfiguration bcv2 = new BreakerConfiguration(cipherArr.Length);//get altered configuration
                        //set the fitness strings to the altered
                        bc.RotorFitness = bcv2.RotorFitness;
                        bc.OffsetFitness = bcv2.OffsetFitness;
                        bc.PlugboardFitness = bcv2.PlugboardFitness;
                        EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//get copy of enigma model

                        switch (toTest)
                        {
                            case Part.Plugboard://testing plugboard
                                em2.plugboard = new Dictionary<int, int>();//reset plugboard
                                if (isTotal == "F")
                                {
                                    bc.numberOfPlugboardSettingsToKeep = combinationValue;//set full index to combination value
                                }
                                else if (isTotal == "S")
                                {
                                    bc.numberOfSinglePlugboardSettingsToKeep = combinationValue;//set single index to combination value
                                }

                                List<BreakerResult> finalResults = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, bc);//get plugboard results
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                bool foundP = false;//search for correct result
                                foreach (BreakerResult br in finalResults)
                                {
                                    if (comparePlugboard(em2.toStringPlugboard(), br.enigmaModel.toStringPlugboard()))
                                    {
                                        foundP = true;
                                        break;
                                    }
                                }
                                if (!foundP)//if it wasnt found 
                                {
                                    missNumber++;//add to missfile
                                }
                                break;
                            case Part.Offset://test offset
                                em2.plugboard = new Dictionary<int, int>();//reset plugboard
                                Random rnd = new Random();
                                //randomise new rotations
                                for (int ri = 0; ri < 3; ri++)
                                {
                                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;//set rotation
                                    em2.rotors[ri].ringOffset = 0;//set offset
                                }
                                //set the correct value depending on what is being tested
                                if (isTotal == "F")
                                {
                                    bc.numberOfOffsetToKeep = combinationValue;//set the full index
                                }
                                else
                                {
                                    bc.numberOfSettingsPerRotationCombinationToKeep = combinationValue;//set the single index
                                }

                                List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, bc);//get offset results
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                                bool foundO = false;//search for the corretc offset in the results
                                foreach (BreakerResult br in fullRotorOffset)
                                {
                                    if (compareOffset(em2, br.enigmaModel))
                                    {
                                        foundO = true;
                                        break;
                                    }
                                }
                                if (!foundO)//if not found
                                {
                                    missNumber++;//add 1 to miss count
                                }
                                break;
                            case Part.Rotor://test rotor                          
                                if (isTotal == "F")
                                {
                                    bc.numberOfRotorsToKeep = combinationValue;//set full index
                                }
                                else
                                {
                                    bc.numberOfSettingsPerRotorCombinationToKeep = combinationValue;//set single index
                                }

                                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, bc));//get results
                                bool foundR = false;//search for correct settings
                                foreach (BreakerResult br in initialRotorSetupResults)
                                {
                                    if (compareRotors(em2, br.enigmaModel))
                                    {
                                        foundR = true;
                                        break;
                                    }
                                }
                                if (!foundR)//if the correct setting was found
                                {
                                    missNumber++;//add to the miss count
                                }

                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }
                    }
                    lineToFile += "," + Convert.ToDouble((iterations - missNumber) * 100 / iterations);//add the success rate to file                
                }
                linesToFile.Add(lineToFile);
                _logger.LogInformation($"Finished {toTest} {messageLength}");
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);//write to file
            _logger.LogInformation("Writing to file");
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
            string plaintext = _bs.getText(to * 2);//get text
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();//convert text to integer list
            string s = "";
            for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)//for each index size
            {
                s += $", {combinationValue}";//add it to the string to write
            }
            List<string> linesToFile = new List<string>() { s };
            for (int lengthOfText = from; lengthOfText < to + 1; lengthOfText += step)//for each length of text
            {
                string lineToFile = $"{lengthOfText}";
                string elapsedTime = "";
                for (int combinationValue = singleFrom; combinationValue <= singleTo; combinationValue += singleStep)//for each combination values
                {
                    TimeSpan total = TimeSpan.Zero;
                    for (int i = 0; i < iterations; i++)//for iterations
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//randomize an enigma model
                        string emJson = JsonConvert.SerializeObject(em);
                        int[] cipherArr = _encodingService.encode(plainArr.GetRange(0, lengthOfText).ToArray(), em);//encipher
                        
                        BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length);//get the breaker configuration

                        switch (toTest)//check which part of the enigma I am testing
                        {
                            case Part.Plugboard://plugboard
                                EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//get a copy of enigma model
                                em2.plugboard = new Dictionary<int, int>();//remove the plugboard from the copy                                
                                breakerConfiguration.numberOfSinglePlugboardSettingsToKeep = combinationValue;//set the single index
                                
                                Stopwatch stopWatchPlugboard = new Stopwatch();
                                stopWatchPlugboard.Start();//start timer
                                List<BreakerResult> finalResult = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, breakerConfiguration);//get the plugboard
                                stopWatchPlugboard.Stop();//stop timer
                                TimeSpan tsPlugboard = stopWatchPlugboard.Elapsed;
                                total = total.Add(tsPlugboard);//add the timespan to total time
                                break;
                            case Part.Offset:
                                em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//get
                                em2.plugboard = new Dictionary<int, int>();//reset plugboard
                                Random rnd = new Random();
                                for (int ri = 0; ri < 3; ri++)//for each rotor
                                {
                                    em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;//set the rotation
                                    em2.rotors[ri].ringOffset = 0;//set offset
                                }                                
                                breakerConfiguration.numberOfSettingsPerRotationCombinationToKeep = combinationValue;//set single index
                                
                                Stopwatch stopWatchOffset = new Stopwatch();
                                stopWatchOffset.Start();//start timer
                                List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr, breakerConfiguration);
                                stopWatchOffset.Stop();//stop timer
                                TimeSpan tsOffset = stopWatchOffset.Elapsed;
                                total = total.Add(tsOffset);//add the time elapsed to the total
                                break;
                            case Part.Rotor://testing rotor                    
                                breakerConfiguration.numberOfSettingsPerRotorCombinationToKeep = combinationValue;//set the single index
                                
                                Stopwatch stopWatchRotors = new Stopwatch();
                                stopWatchRotors.Start();//start timer
                                List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr, breakerConfiguration));//get rotors
                                stopWatchRotors.Stop();//stop timer
                                TimeSpan tsRotors = stopWatchRotors.Elapsed;
                                total = total.Add(tsRotors);//add the time elapsed to the total
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }                        
                    }
                    TimeSpan ts = new TimeSpan(total.Ticks / iterations);//divide to get the average
                    elapsedTime += $",{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";//add to line of file
                }
                lineToFile += elapsedTime;
                linesToFile.Add(lineToFile);//add to file list
                _logger.LogInformation($"Finished {toTest} {lengthOfText}");
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);//write to file
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
            if (toTest == Part.Rotor)//if the part being tested is rotors
            {
                //set rotors in use to 3 and reflectors in use to 1
                _bc.numberOfRotorsInUse = 3;
                _bc.numberOfReflectorsInUse = 1;
                _bs.setNumOfRotors(3);
                _bs.setNumOfReflectors(1);
            }

            string plaintext = _bs.getText(to * 2);//get text
            List<int> plainArr = EncodingService.preProccessCiphertext(plaintext).ToList();//get text as integer list
            string s = "";
            for(int i = plugboardFrom; i <= plugboardTo; i+= plugbopardStep)//set headers
            {
                s += $",{i}";
            }
            List<string> linesToFile = new List<string>() { s };
            for (int lengthOfPlainText = from; lengthOfPlainText < to + 1; lengthOfPlainText += step)//for eahc length of text
            {
                string lineToFile = $"{lengthOfPlainText}";
                for (int plugboardLength = plugboardFrom; plugboardLength <= plugboardTo; plugboardLength += plugbopardStep)//for each length of the plugboard
                {
                    int correctCount = 0;
                    for (int i = 0; i < iterations; i++)//for each iteration
                    {
                        EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, plugboardLength,true);//randomize enigma
                        
                        bool wasCorrect = false;
                        switch (toTest)
                        {
                            case Part.Plugboard://test plugboard
                                wasCorrect = testPlugboard(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);//get if the plugboard result was correct
                                break;
                            case Part.Offset:
                                wasCorrect = testOffset(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);//get if the offset result was correct
                                break;
                            case Part.Rotor:
                                wasCorrect = testRotor(plainArr.GetRange(0, lengthOfPlainText).ToArray(), em);//get if the rotor result was correct
                                break;
                            default:
                                _logger.LogWarning($"Unsure what to test: {toTest}");
                                break;
                        }
                        if (wasCorrect)//if it was correct
                        {
                            correctCount++;//increase the correct count
                        }
                        
                    }
                    lineToFile += $", {100*((double)correctCount / iterations)}";//add the value to line
                }
                _logger.LogInformation($"Finished {toTest} {lengthOfPlainText}");                
                linesToFile.Add(lineToFile);//add line to other lines
            }

            File.WriteAllLines(filePathAndName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", linesToFile);//write to file
            _logger.LogInformation("Writing to file");

            if (toTest == Part.Rotor)//if testing rotor
            {
                //reset the rotors and reflectors back to what they were
                _bc.numberOfRotorsInUse = trueNumOfRotors;
                _bc.numberOfReflectorsInUse = trueNumOfRelfectors;
                _bs.setNumOfRotors(_bc.numberOfRotorsInUse);
                _bs.setNumOfReflectors(_bc.numberOfReflectorsInUse);               
            }
        }
        #endregion

        #region testing individual sections
        /// <summary>
        /// Returns a bool of if the plaintext encoded by the enigma model was decoded correctly
        /// </summary>
        /// <param name="plaintext"></param> The plaintext to test on as 
        /// <param name="em"></param> (Optional) Enigma model to encode with
        /// <param name="fitnessStr"></param> (Optional) String of fitness model to decode with
        /// <returns>If the rotors got the settings correct</returns>
        public bool testRotor(int[] plaintext, EnigmaModel em = null, string fitnessStr = "",bool withoutRefinement=false)
        {
            em = em == null ? EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings) : em;//if the enigma model is null set it to randomized                       
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//copy enigma model

            _logger.LogDebug(em.ToString());
            int[] cipherArr = _encodingService.encode(plaintext, em);//encode

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);//get configuration
            if (fitnessStr != "") { breakerConfiguration.RotorFitness = fitnessStr; }//set the fitness string if specified
            List<BreakerResult> initialRotorSetupResults = _bs.sortBreakerList(_bs.getRotorResults(cipherArr,breakerConfiguration));//get results from rotor

            foreach (BreakerResult br in initialRotorSetupResults)//for each result
            {
                bool compareRotor = compareRotors(em2, br.enigmaModel);//check if it is correct
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
            em = em == null ? EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings) : em;//if the enigma model is null set it to randomized            
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//get 2 copies of the enigma model
            EnigmaModel em3 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);

            _logger.LogDebug(em.ToString());
            int[] cipherArr = _encodingService.encode(plaintext, em);//encode to cipher text

            em2.plugboard = new Dictionary<int, int>();//set the plugboard to nothing
            Random rnd = new Random();
            //randomise the undoing of the offset
            for (int ri = 0; ri < 3; ri++)
            {
                em2.rotors[ri].rotation = EncodingService.mod26(em2.rotors[ri].rotation - em2.rotors[ri].ringOffset) + rnd.Next(3) - 1;
                em2.rotors[ri].ringOffset = 0;
            }
            _logger.LogDebug($"Input: {em2.toStringRotors()}");

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);//get the configuration to break with
            if (fitnessStr != "") { breakerConfiguration.OffsetFitness = fitnessStr; }//if the fitness string was specified set it

            List<BreakerResult> fullRotorOffset = _bs.getRotationOffsetResult(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr,breakerConfiguration);//get the offset results
            
            foreach (BreakerResult brr in fullRotorOffset)//search results
            {
                bool correctOffset = compareOffset(em3, brr.enigmaModel);//was the result found
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
            em = em == null ? EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings) : em;//if the enigma model is null set it to randomized            
            string emJson = JsonConvert.SerializeObject(em);
            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);//copy enigma

            _logger.LogDebug(em.ToString());
            int[] cipherArr = _encodingService.encode(plaintext, em);//encode

            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,withoutRefinement);//get breaker configuration
            if (fitnessStr != "") { breakerConfiguration.PlugboardFitness = fitnessStr; }//if fitness string is specified set it
            em2.plugboard = new Dictionary<int, int>();//set plugboard to empty             
            List<BreakerResult> finalResult = _bs.getPlugboardResults(new List<BreakerResult>() { new BreakerResult(cipherArr, double.MinValue, em2) }, cipherArr,breakerConfiguration);//get plugboard results
            foreach (BreakerResult br in finalResult)//for each result
            {
                bool correctPB = comparePlugboard(em.toStringPlugboard(), br.enigmaModel.toStringPlugboard());//is the plugboard correct
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
        public static bool compareRotors(EnigmaModel actual,EnigmaModel attempt)
        {
            if (attempt.reflector.rotor.name == actual.reflector.rotor.name && attempt.rotors[0].rotor.name == actual.rotors[0].rotor.name && attempt.rotors[1].rotor.name == actual.rotors[1].rotor.name && attempt.rotors[2].rotor.name == actual.rotors[2].rotor.name)//if all rotors and reflector are in the correct order
            {
                if (EncodingService.mod26(attempt.rotors[0].rotation - 1) == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) || attempt.rotors[0].rotation == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) || EncodingService.mod26(attempt.rotors[0].rotation + 1) == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset))//if the rotation of the first rotor is +/- 1 out from the offset and rotation of the actual
                {
                    if (EncodingService.mod26(attempt.rotors[1].rotation - 1) == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset) || attempt.rotors[1].rotation == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset) || EncodingService.mod26(attempt.rotors[1].rotation + 1) == EncodingService.mod26(actual.rotors[1].rotation - actual.rotors[1].ringOffset))//if the rotation of the second rotor is +/- 1 out from the offset and rotation of the actual
                    {
                        if (EncodingService.mod26(attempt.rotors[2].rotation - 1) == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset) || attempt.rotors[2].rotation == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset) || EncodingService.mod26(attempt.rotors[2].rotation + 1) == EncodingService.mod26(actual.rotors[2].rotation - actual.rotors[2].ringOffset))//if the rotation of the third rotor is +/- 1 out from the offset and rotation of the actual
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
        public static bool compareOffset(EnigmaModel actual,EnigmaModel attempt)
        {
            if (attempt.rotors[0].rotation == EncodingService.mod26(actual.rotors[0].rotation - actual.rotors[0].ringOffset) && attempt.rotors[0].ringOffset == 0)//if the right rotor is correct
            {
                string[] actualSplit = actual.toStringRotors().Split("/");
                string actualStr = actualSplit[0]  + "/" + actualSplit[2] + "/" + actualSplit[3];//add the relfector and second and third rotor to actual string
                string[] attemptSplit = attempt.toStringRotors().Split("/");
                string attemptStr = attemptSplit[0] + "/" + attemptSplit[2] + "/" + attemptSplit[3];//add the relfector and second and third rotor to attempt string
                if (actualStr == attemptStr)//if the attempt and actual are the same
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
        public static bool comparePlugboard(string actual, string attempt)
        {
            if (actual != attempt) 
            {
                if (actual.Length == attempt.Length)//if the lengths are the same
                {
                    foreach (string actualPairChars in actual.Split(" "))//for each pair
                    {
                        bool found = false;
                        foreach (string attemptPairChars in attempt.Split(" "))//for each attempt pair
                        {
                            if (attemptPairChars == actualPairChars)//if they are equal
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)//if not found
                        {
                            return false;//return false
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public double accuracyPercentage(string plaintext,string attempt)
        {
            int correctCount = 0;//set the original number correct to 0
            for(int i = 0; i < plaintext.Length; i++)//for each letter in the plaintext
            {
                if (attempt.Length > i)//if the attempt is long enough
                {
                    if (plaintext[i] == attempt[i])//if the plaintext is equal
                    {
                        correctCount++;//increase the correct count
                    }
                }
                else
                {
                    break;//break the for if the attempt isnt long enough
                }
            }
            return 100 * Convert.ToDouble(correctCount/plaintext.Length);//return the accuracy
        }
        #endregion
    }
}
