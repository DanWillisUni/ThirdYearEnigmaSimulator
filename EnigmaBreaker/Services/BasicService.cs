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
        private readonly SharedUtilities _sharedUtilities;
        
        private List<Rotor> allRotors { get; set; }
        private List<Rotor> allReflectors { get; set; }
        private PhysicalConfiguration _physicalConfiguration { get; set; }
        private FitnessConfiguration _fc { get; set; }
        public BasicService(ILogger<BasicService> logger, BasicConfiguration bc, EncodingService encodingService, IFitness.FitnessResolver fitnessResolver,PhysicalConfiguration physicalConfiguration,FitnessConfiguration fc,SharedUtilities sharedUtilities)
        {
            _logger = logger;
            _bc = bc;
            _encodingService = encodingService;
            _resolver = fitnessResolver;
            _physicalConfiguration = physicalConfiguration;
            _fc = fc;
            _sharedUtilities = sharedUtilities;

            setNumOfRotors(_bc.numberOfRotorsInUse);
            setNumOfReflectors(_bc.numberOfReflectorsInUse);
        }
       /// <summary>
       /// The root method for this service which is the only one that should be called externally
       /// </summary>
        public void root()
        {
            if(_bc.inputFormat == "RAND")//if the input format is random
            {
                _logger.LogDebug("Selected Random Input");
                testRandom();
            }
            else if (_bc.inputFormat == "USER")//if the input format is user
            {
                _logger.LogDebug("Selected User Input");
                getUserInputCipherText();
            }
        }
        /// <summary>
        /// Tests a random peice of ciphertext
        /// </summary>
        private void testRandom()
        {
            string plaintext = getText();//get a random plaintext as string
            EnigmaModel em = EnigmaModel.randomizeEnigma(_physicalConfiguration, _bc.numberOfRotorsInUse, _bc.numberOfReflectorsInUse, _bc.maxPlugboardSettings);//get a new random enigma model
            _logger.LogInformation($"Plaintext: {plaintext.Length}\n" + plaintext);//print the plaintext
            _logger.LogInformation(em.ToString());//print the enigma model
            string ciphertext = _encodingService.encode(plaintext, em);//get the ciphertext
            decryption(ciphertext,true);
        }

        /// <summary>
        /// The user is asked to enter the ciphertext and it then  decypts it
        /// </summary>
        private void getUserInputCipherText()
        {
            Console.WriteLine("Please enter a ciphertext:\n");
            string cipherText = Console.ReadLine();
            string plainText = decryption(cipherText);
            Console.WriteLine("Here is the plain text\n");
            Console.WriteLine(plainText,true);
        }

        /// <summary>
        /// Decrypts the ciphertext
        /// </summary>
        /// <param name="ciphertext">text to decypher</param>
        /// <param name="includeLogging"></param>
        /// <returns></returns>
        public string decryption(string ciphertext,bool includeLogging = false)            
        {             
            int[] cipherArr = EncodingService.preProccessCiphertext(ciphertext);//convert the ciphertext into an array of integers            
            _logger.LogInformation($"Ciphertext: {cipherArr.Length}\n" + EncodingService.addSpacesEveryFive(EncodingService.getStringFromIntArr(cipherArr)));//print the ciphertext
            Stopwatch timer = new Stopwatch();//create new timer
            timer.Start();//start timer
            BreakerConfiguration breakerConfiguration = new BreakerConfiguration(cipherArr.Length,_fc.indexFiles);
            breakerConfiguration = sortBreakerRules(breakerConfiguration, cipherArr.Length);
            List<BreakerResult> rotorResults = getRotorResults(cipherArr,breakerConfiguration);//get the top results for rotor configurations
            if (includeLogging)
            {
                foreach (BreakerResult br in rotorResults)
                {
                    _logger.LogInformation(br.enigmaModel.ToString() + " : " + br.score);
                }
            }
            List<BreakerResult> offsetResults = getRotationOffsetResult(rotorResults,cipherArr, breakerConfiguration);//using the top rotor results get the top offset settings
            if (includeLogging)
            {
                foreach (BreakerResult br in offsetResults)
                {
                    _logger.LogInformation(br.enigmaModel.ToString() + " : " + br.score);
                }
            }
            List<BreakerResult> plugboardResults = getPlugboardResults(offsetResults, cipherArr, breakerConfiguration);//using the top offset settings get the top plugboard settings
            if (includeLogging)
            {
                foreach (BreakerResult br in plugboardResults)
                {
                    _logger.LogInformation(br.enigmaModel.ToString() + " : " + br.score);
                }
            }
            List<string> attemptedPlainText = new List<string>();//create list for the attempted plaintext
            foreach (BreakerResult result in plugboardResults)//for all the end results
            {
                attemptedPlainText.Add(_encodingService.encode(ciphertext, result.enigmaModel));//decode the ciphertext using the suggested enigma configuration
            }            
            timer.Stop();//stop the timer

            string attemptPlainText = "";
            if (plugboardResults.Count > 1)//if there is more than one plugboard setting suggested
            {
                Console.WriteLine("Please type in the number corrisponding to the best match");//get the user to chose which end sounds most english
                for (int i = 1;i<attemptedPlainText.Count + 1; i++)//for each attempt at the plain text
                {
                    Console.WriteLine($"{i}: {attemptedPlainText[i].Substring(Math.Max(0, attemptedPlainText[i].Length - _bc.numberOfEndCharsToDisplay))}");//read the list out to the user
                }
                int index = Convert.ToInt32(Console.ReadLine());//get the user input (no error handeling)
                attemptPlainText = attemptedPlainText[index - 1];//set the plaintext to the users input
            }
            else
            {
                attemptPlainText = attemptedPlainText[0];//set the attempt at plaintext to the only one left
            }
            _logger.LogInformation($"Plaintext: \n{attemptPlainText}");//print the plaintext       
            TimeSpan ts = timer.Elapsed;//get the elapsed time as a TimeSpan value
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds);//format the timespan value
            _logger.LogInformation("Run time: " + elapsedTime);//print the time taken to crack the enigma
            _logger.LogInformation(plugboardResults[attemptedPlainText.IndexOf(attemptPlainText)].enigmaModel.ToString());
            return attemptPlainText;
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
        public List<BreakerResult> getRotorResults(int[] cipherArr,BreakerConfiguration breakerConfiguration)
        {            
            IFitness fitness = _resolver(breakerConfiguration.RotorFitness);//get the fitness function from the resolver string
            List<BreakerResult> results = new List<BreakerResult>();//create new results list
            List<EnigmaModel> rotorConfigurationsToCheck = new List<EnigmaModel>();//create new list for the rotor combinations to check

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
                                    rotorConfigurationsToCheck.Add(new EnigmaModel(rotors, new RotorModel(refl), new Dictionary<int, int>()));//add the serilised string of the enigma settings to the settings to check list
                                }
                            }
                        }
                    }
                }
            }

            Parallel.For<List<BreakerResult>>(0, rotorConfigurationsToCheck.Count, () => new List<BreakerResult>(), (i, loop, threadResults) => //multithreaded for loop
            {
                threadResults.AddRange(getIndividualRotorResults(cipherArr, rotorConfigurationsToCheck[(int)i], fitness, breakerConfiguration.numberOfSettingsPerRotorCombinationToKeep));//add to the thread results the top results for that configuration of rotors
                return threadResults;//return the thread results
            },
            (threadResults) => {
                lock (rotorListLock)//lock the list
                {
                    results.AddRange(threadResults);//add the thread results to the results list
                }
            });

            results = sortBreakerList(results);
            if (results.Count > breakerConfiguration.numberOfRotorsToKeep)//if the list is longer than I would like
            {
                results = results.GetRange(0, breakerConfiguration.numberOfRotorsToKeep);//shorten the list
            }
            return results;//return the top few enigma configurations
        }
        /// <summary>
        /// Loops through each rotation configuration for this rotor combination
        /// Evaluates them all using the fitness function
        /// </summary>
        /// <param name="cipherArr"></param>
        /// The ciphertext in integer array
        /// <param name="emToTest"></param>
        /// The enigma configuration to check
        /// <param name="fitness"></param>
        /// Fitness function to use when checking
        /// <returns>The top few rotation settings for this rotor combination</returns>
        public List<BreakerResult> getIndividualRotorResults(int[] cipherArr, EnigmaModel emToTest, IFitness fitness,int n)
        {
            List<BreakerResult> results = new List<BreakerResult>();//create results list
            double lowestResult = double.MinValue;//store the lowest rank in as the minimum value            
            for (int l = 0; l <= 25; l++)//for each left rotor rotation
            {
                for (int m = 0; m <= 25; m++)//for each middle rotor rotation
                {
                    for (int r = 0; r <= 25; r++)//for each right rotor rotation
                    {
                        EnigmaModel em = new EnigmaModel(new List<RotorModel>() { new RotorModel(emToTest.rotors[0].rotor, l), new RotorModel(emToTest.rotors[1].rotor, m), new RotorModel(emToTest.rotors[2].rotor, r) },emToTest.reflector,new Dictionary<int, int>());
                        int[] attemptPlainText = _encodingService.encode(cipherArr, em);//get the integer array of the attempt at decoding with the current enigma setup
                        double rating = fitness.getFitness(attemptPlainText,IFitness.Part.Rotor);//rate how english the attempt is using the fitness function

                        if (rating > lowestResult)//if the rating is higher than the current lowest result scored
                        {
                            em.rotors[0].rotation = l;//reset the rotation for the left rotor
                            em.rotors[1].rotation = m;//reset the rotation for the middle rotor
                            em.rotors[2].rotation = r;//reset the rotation of the right rotor
                            BreakerResult br = new BreakerResult(attemptPlainText, rating, em);//create a result object
                            bool stillSubtract = results.Count + 1 > n;//if there is to many objects in the list
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
                                if (results.Count + 1 <= n || lowest == null)//if the list is still too long or no object could be removed
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
        /*
        public List<BreakerResult> getIndividualRotorResultsFast(int[] cipherArr, string emStr, IFitness fitness, int n)
        {
            int toGoTo = n < 5 ? n : 5;
            List<BreakerResult> leftResults = new List<BreakerResult>();//create results list
            double lowestResult = double.MinValue;//store the lowest rank in as the minimum value
            for (int l = 0; l <= 25; l++)//for each left rotor rotation
            {                
                EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                em.rotors[0].rotation = l;//set the rotation of the left rotor
                em.rotors[1].rotation = 0;//set the rotation of the middle rotor
                em.rotors[2].rotation = 0;//set the rotation of the right rotor
                int[] attemptPlainText = _encodingService.encode(cipherArr, em);//get the integer array of the attempt at decoding with the current enigma setup
                double rating = fitness.getFitness(attemptPlainText, IFitness.Part.Rotor);//rate how english the attempt is using the fitness function

                if (rating > lowestResult)//if the rating is higher than the current lowest result scored
                {
                    em.rotors[0].rotation = l;//reset the rotation for the left rotor
                    em.rotors[1].rotation = 0;//reset the rotation for the middle rotor
                    em.rotors[2].rotation = 0;//reset the rotation of the right rotor
                    BreakerResult br = new BreakerResult(attemptPlainText, rating, em);//create a result object
                    bool stillSubtract = leftResults.Count + 1 > n;//if there is to many objects in the list
                    while (stillSubtract)//while there is too many objects in the list
                    {
                        double nextLowest = rating;
                        BreakerResult lowest = null;
                        foreach (BreakerResult result in leftResults)//for all the results
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
                        leftResults.Remove(lowest);//remove the lowest result from the list
                        lowestResult = nextLowest;//set the lowest result score to the new lowest score
                        if (leftResults.Count + 1 <= n || lowest == null)//if the list is still too long or no object could be removed
                        {
                            stillSubtract = false;//break the while loop
                        }
                    }
                    leftResults.Add(br);//add the new result to the list
                }                  
            }
            leftResults = sortBreakerList(leftResults);
            List<BreakerResult> middleResults = new List<BreakerResult>();//create results list
            lowestResult = double.MinValue;
            for (int i = 0; i < toGoTo; i++)
            {
                BreakerResult currentResult = leftResults[i];
                emStr = JsonConvert.SerializeObject(currentResult.enigmaModel);
                for (int m = 0; m <= 25; m++)//for each left rotor rotation
                {
                    EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                    em.rotors[1].rotation = m;//set the rotation of the middle rotor
                    em.rotors[2].rotation = 0;//set the rotation of the right rotor
                    int[] attemptPlainText = _encodingService.encode(cipherArr, em);//get the integer array of the attempt at decoding with the current enigma setup
                    double rating = fitness.getFitness(attemptPlainText, IFitness.Part.Rotor);//rate how english the attempt is using the fitness function

                    if (rating > lowestResult)//if the rating is higher than the current lowest result scored
                    {
                        em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                        em.rotors[1].rotation = m;//set the rotation of the middle rotor
                        em.rotors[2].rotation = 0;//set the rotation of the right rotor
                        BreakerResult br = new BreakerResult(attemptPlainText, rating, em);//create a result object
                        bool stillSubtract = middleResults.Count + 1 > n;//if there is to many objects in the list
                        while (stillSubtract)//while there is too many objects in the list
                        {
                            double nextLowest = rating;
                            BreakerResult lowest = null;
                            foreach (BreakerResult result in middleResults)//for all the results
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
                            middleResults.Remove(lowest);//remove the lowest result from the list
                            lowestResult = nextLowest;//set the lowest result score to the new lowest score
                            if (middleResults.Count + 1 <= n || lowest == null)//if the list is still too long or no object could be removed
                            {
                                stillSubtract = false;//break the while loop
                            }
                        }
                        middleResults.Add(br);//add the new result to the list
                    }
                }
            }
            middleResults = sortBreakerList(middleResults);
            List<BreakerResult> results = new List<BreakerResult>();//create results list
            lowestResult = double.MinValue;
            for (int i = 0; i < toGoTo; i++)
            {
                BreakerResult currentResult = middleResults[i];
                emStr = JsonConvert.SerializeObject(currentResult.enigmaModel);
                for (int m = 0; m <= 25; m++)//for each left rotor rotation
                {
                    EnigmaModel em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                    em.rotors[1].rotation = m;//set the rotation of the middle rotor
                    em.rotors[2].rotation = 0;//set the rotation of the right rotor
                    int[] attemptPlainText = _encodingService.encode(cipherArr, em);//get the integer array of the attempt at decoding with the current enigma setup
                    double rating = fitness.getFitness(attemptPlainText, IFitness.Part.Rotor);//rate how english the attempt is using the fitness function

                    if (rating > lowestResult)//if the rating is higher than the current lowest result scored
                    {
                        em = JsonConvert.DeserializeObject<EnigmaModel>(emStr);//get the Rotors to check from the Json string
                        em.rotors[1].rotation = m;//set the rotation of the middle rotor
                        em.rotors[2].rotation = 0;//set the rotation of the right rotor
                        BreakerResult br = new BreakerResult(attemptPlainText, rating, em);//create a result object
                        bool stillSubtract = results.Count + 1 > n;//if there is to many objects in the list
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
                            if (results.Count + 1 <= n || lowest == null)//if the list is still too long or no object could be removed
                            {
                                stillSubtract = false;//break the while loop
                            }
                        }
                        results.Add(br);//add the new result to the list
                    }
                }
            }
            return results;//return the results list
        }*/
        #endregion

        #region offset and Rotation
        private readonly object offsetListLock = new object();//mutex for the offset result list

        /// <summary>
        /// Gets the top few rotor offset results from the top few rotor configuration setting
        /// 
        /// Loops through the top few rotor configuration 
        /// Adds a variance of +/- 1 to each rotor rotation
        /// Uses multi threading to check all these variance combinations with the top few rotor combinations
        /// </summary>
        /// <param name="breakerResults"></param>
        /// The top few rotor configuration results
        /// <param name="cipherArr"></param>
        /// cipher text as an integer array
        /// <param name="fitnessStr"></param>
        /// (Optional and for testing purposes) The fitness function resolver string to use
        /// <returns>The top few rotor offset configurations</returns>
        public List<BreakerResult> getRotationOffsetResult(List<BreakerResult> breakerResults,int[] cipherArr,BreakerConfiguration breakerConfiguration)
        {
            IFitness fitness = _resolver(breakerConfiguration.OffsetFitness);//get the fitness class from the fitness string
            List<BreakerResult> results = new List<BreakerResult> ();//create a list for the results
            List<string> offsetConfigurationsToCheck = new List<string>();//create a list for the rotor configurations to check
            foreach (BreakerResult br in breakerResults)//for each result in the top few rotor configurations
            {
                int lbase = br.enigmaModel.rotors[0].rotation;//get the rotation of the left rotor
                int mbase = br.enigmaModel.rotors[1].rotation;//get the rotation of the middle rotor
                int rbase = br.enigmaModel.rotors[2].rotation;//get the rotation of the right rotor
                for (int lchange = -1; lchange < 2; lchange++)//for varience in left rotor rotation
                {
                    br.enigmaModel.rotors[0].rotation = lbase + lchange;//set the rotation of the left rotor
                    for (int mchange = -1; mchange < 2; mchange++)//for varience in the middle rotor rotation
                    {
                        br.enigmaModel.rotors[1].rotation = mbase + mchange;//set the middle rotor rotation
                        for (int rchange = -1; rchange < 2; rchange++)//for varience in the right rotor
                        {
                            br.enigmaModel.rotors[2].rotation = rbase + rchange;//set the right rotor rotation
                            offsetConfigurationsToCheck.Add(br.enigmaModel.toStringRotors());//add the string of rotor to the ones to check
                            //TODO make this take a EM or atleast serilised EM
                        }
                    }
                }
            }

            Parallel.For<List<BreakerResult>>(0, offsetConfigurationsToCheck.Count, () => new List<BreakerResult>(), (i, loop, threadResults) => //multi threaded for loop
            {
                threadResults.AddRange(getOffsetResultPerChange(cipherArr, fitness, offsetConfigurationsToCheck[(int)i],breakerConfiguration.numberOfSettingsPerRotationCombinationToKeep));//add the top few results of the individual range to check to the thread results
                return threadResults;//return the results for this thread
            },
            (threadResults) => {
                lock (offsetListLock)//lock the list
                {
                    results.AddRange(threadResults);//add the thread results to the list
                }
            });

            results = sortBreakerList(results);//sort the list
            if (results.Count > breakerConfiguration.numberOfOffsetToKeep)//if the list is longer than I would like
            {
                results=results.GetRange(0, breakerConfiguration.numberOfOffsetToKeep);//shorten the list
            }
            return results;//return the results
        }
        /// <summary>
        /// Get the top few results per change of the rotor combination and rotation
        /// </summary>
        /// <param name="cipherArr"> Cipher text as an integer array</param>
        /// <param name="fitness"> fitness function</param>
        /// <param name="currentRotors">string of the current reflector, rotors, rotation and offset of each rotor</param>
        /// <param name="n">Max number of results to return</param>
        /// <returns>The top few rotor offset and rotation combinations given the rotor rotation</returns>
        public List<BreakerResult> getOffsetResultPerChange(int[] cipherArr, IFitness fitness, string currentRotors,int n)
        {
            List<string> rotorNames = new List<string>();//create the new list of rotor names
            List<string> rotorDetails = new List<string>();//create the new list of rotor details
            foreach (string rotorDetailsStr in currentRotors.Split("/"))//for each rotor in the currentRotor string
            {
                if (rotorDetailsStr.Contains(","))//if it isnt the reflector
                {
                    rotorNames.Add(rotorDetailsStr.Split(",")[0]);//add the name to the rotor names array
                    rotorDetails.Add(rotorDetailsStr);//add the rotor details to the rotor details list
                }
            }
            RotorModel emRefl = null;
            foreach (Rotor refl in allReflectors)//for each relfector
            {
                if (refl.name == currentRotors.Split("/")[0])//if its name is the same as the one passed in
                {
                    emRefl = new RotorModel(refl);//set the relfector
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
                    double rating = fitness.getFitness(attemptPlainText,IFitness.Part.Offset);

                    if (rating > lowestResult)
                    {
                        em.rotors[0].rotation = lbase;
                        em.rotors[1].rotation = m;
                        em.rotors[2].rotation = r;
                        BreakerResult br = new BreakerResult(attemptPlainText, rating, em);
                        bool stillSubtract = results.Count + 1 > n;
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
                            if(results.Count + 1 <= n || lowest == null)
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
        private readonly object plugboardListLock = new object();
        public List<BreakerResult> getPlugboardResults(List<BreakerResult> offsetResults, int[] cipherArr, BreakerConfiguration breakerConfiguration)
        {
            IFitness fitness = _resolver(breakerConfiguration.PlugboardFitness);
            
            List<BreakerResult> results = new List<BreakerResult>();
            foreach(BreakerResult br in offsetResults)
            {
                List<BreakerResult> onePairResults = new List<BreakerResult>() { br };
                int currentPlugboardLength = 0;
                while (currentPlugboardLength <= _bc.maxPlugboardSettings)
                {
                    results.AddRange(onePairResults);
                    currentPlugboardLength++;
                    List<BreakerResult> newOPR = new List<BreakerResult>();

                    Parallel.For<List<BreakerResult>>(0, onePairResults.Count, () => new List<BreakerResult>(), (i, loop, threadResults) => //multi threaded for loop
                    {
                        threadResults.AddRange(onePairPlugboard(onePairResults[i], cipherArr, fitness, breakerConfiguration.numberOfSinglePlugboardSettingsToKeep));
                        return threadResults;//return the results for this thread
                    },
                    (threadResults) => {
                        lock (plugboardListLock)//lock the list
                        {
                            newOPR.AddRange(threadResults);//add the thread results to the list
                        }
                    });

                    onePairResults = new List<BreakerResult>();
                    onePairResults.AddRange(newOPR);
                }
            }
            results = sortBreakerList(results);
            if(results.Count > breakerConfiguration.numberOfPlugboardSettingsToKeep)
            {
                results = results.GetRange(0, breakerConfiguration.numberOfPlugboardSettingsToKeep);
            }
            return results;
        }       

        public List<BreakerResult> onePairPlugboard(BreakerResult br,int[] cipherArr,IFitness fitness,int n)
        {
            List<int> ignoreCurrent = new List<int>();
            foreach (KeyValuePair<int, int> entry in br.enigmaModel.plugboard)
            {
                ignoreCurrent.Add(entry.Key);
                ignoreCurrent.Add(entry.Value);
            }
            string emJson = JsonConvert.SerializeObject(br.enigmaModel);

            List<BreakerResult> results = new List<BreakerResult>();

            for (int a = 0; a < 26; a++)
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
                            double rating = fitness.getFitness(attemptPlainText,IFitness.Part.Plugboard);
                            EnigmaModel em2 = JsonConvert.DeserializeObject<EnigmaModel>(emJson);
                            em2.plugboard.Add(a, b);
                            results.Add(new BreakerResult(attemptPlainText, rating, em2));
                        }
                    }
                }
            }

            results = sortBreakerList(results);
            if (results.Count > n)
            {
                results = results.GetRange(0, n);
            }
            return results;
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

        public void setNumOfRotors(int num)
        {
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
            allRotors = rotors.GetRange(0, num);
        }
        public void setNumOfReflectors(int num)
        {
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
            if (num == 1)
            {
                reflectorStartIndex = 1;
            }
            allReflectors = reflectors.GetRange(reflectorStartIndex, num);
        }

        public BreakerConfiguration sortBreakerRules(BreakerConfiguration breakerConfiguration,int cipherLength)
        {
            if (breakerConfiguration.RotorFitness == "RULE")
            {
                breakerConfiguration.RotorFitness = _sharedUtilities.getRes(cipherLength, IFitness.Part.Rotor);
            }
            if (breakerConfiguration.OffsetFitness == "RULE")
            {
                breakerConfiguration.OffsetFitness = _sharedUtilities.getRes(cipherLength, IFitness.Part.Offset);
            }
            if (breakerConfiguration.PlugboardFitness == "RULE")
            {
                breakerConfiguration.PlugboardFitness = _sharedUtilities.getRes(cipherLength, IFitness.Part.Plugboard);
            }
            return breakerConfiguration;
        }
        #endregion
    }
}
