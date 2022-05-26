using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models;
using EngimaSimulator.Models.Enigma;
using EngimaSimulator.Models.EnigmaConfiguration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Controllers
{
    public class EnigmaConfigurationController : Controller
    {
        private readonly PhysicalConfiguration _physicalConfiguration;//physical configurations of all the rotor configured
        private readonly ILogger<EnigmaConfigurationController> _logger;//logger to log the events
        private readonly BasicConfiguration _basicConfiguration;//configuration of the application
        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="physicalConfiguration"></param>
        /// <param name="basicConfiguration"></param>
        public EnigmaConfigurationController(ILogger<EnigmaConfigurationController> logger,PhysicalConfiguration physicalConfiguration,BasicConfiguration basicConfiguration)
        {
            _logger = logger;
            _physicalConfiguration = physicalConfiguration;
            _basicConfiguration = basicConfiguration;
            _logger.LogDebug("Physical Configuration: " + JsonConvert.SerializeObject(_physicalConfiguration));
            _logger.LogDebug("Basic Configuration: " + JsonConvert.SerializeObject(_basicConfiguration));
        }

        #region rotors
        /// <summary>
        /// Get the rotors view page
        /// </summary>
        /// <returns>Rotors view page</returns>
        public IActionResult Rotors()
        {
            _logger.LogInformation("Get rotors");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//load the current state of the enigma model
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            RotorViewModel rvm = new RotorViewModel(currentSave.rotors,_physicalConfiguration);//construct a new rotor model from the physical configuration and the enigma model
            return View(rvm);//return Rotors view
        }
        /// <summary>
        /// Processes all the post requests from the Rotors View
        /// 
        /// These perform tasks such as saving new rotors, changing rotor order or loading the main view page
        /// </summary>
        /// <param name="modelIn">Model posted</param>
        /// <returns>View for next page</returns>
        [HttpPost]
        public IActionResult Rotors(RotorViewModel modelIn)
        {
            _logger.LogInformation("Post rotors");
            RotorViewModel modelOut = new RotorViewModel();//construct new model
            modelOut._physicalConfiguration = this._physicalConfiguration;//set the Physical configuration
            EnigmaModel enigmaModel = new EnigmaModel();//create Enigma model
            switch (modelIn.Command)//switchcase for the command
            {
                case "rotorSave"://edit which rotors are selected
                    _logger.LogInformation("Save the rotors");
                    _logger.LogInformation("Saving rotors to: " + String.Join(", ", modelIn.liveRotorsNames.ToArray()));
                    foreach (string rn in modelIn.liveRotorsNames)//for each rotor name
                    {
                        foreach (Rotor r in _physicalConfiguration.rotors) //for each rotor in physical configurations
                        {
                            if (r.name == rn)//if the physical configuration name is the same as the name
                            {
                                enigmaModel.rotors.Add(new RotorModel(r));//add the rotor from the physical configuration
                                break;//break the for loop of physical configuration
                            }
                        }
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//merge the rotors with the last configuration saved
                    foreach (RotorModel r in enigmaModel.rotors) //for each new rotor
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);//set the rotor names on out model
                        modelOut.rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65 + r.rotation)));//set teh rotation on the out model
                        modelOut.rotorStepOffset.Add(r.ringOffset.ToString());//set the offset on the out model
                    }
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);//return the view of the configuration
                case "rotorSaveOrder"://save the new order of rotors
                    _logger.LogInformation("ReOrder rotors");
                    //get new order
                    bool continueOn = true;
                    int counter = 1;
                    List<int> newRotorOrder = new List<int>();//construct new list of order
                    do
                    {//do while previous was not null
                        var newItem = Request.Form[$"rotorOrder_{counter}"].ToString();//request item
                        if (String.IsNullOrEmpty(newItem))//if that item in the form didnt exist or is null
                        {
                            continueOn = false;//do not continue
                        }
                        else
                        {
                            newRotorOrder.Add(Convert.ToInt32(newItem));//add the string to new order
                        }
                        counter++;//increase the counter
                    } while (continueOn);
                    //get previous order
                    EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//load the current save of the enigma model
                    foreach (RotorModel r in currentSave.rotors)// for each rotor in the current model
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);//add the names to the model out
                        modelOut.rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65 + r.rotation)));//set teh rotation on the out model
                        modelOut.rotorStepOffset.Add(r.ringOffset.ToString());//set the offset on the out model
                    }
                    _logger.LogInformation("Previous Rotor order: " + String.Join(", ", modelOut.liveRotorsNames.ToArray()));
                    //order swap
                    List<string> tempRotors = new List<string>();
                    List<string> tempRotation = new List<string>();
                    List<string> tempOffset = new List<string>();
                    for(int newRotorOrderIndex = 0; newRotorOrderIndex<=newRotorOrder.Count - 1; newRotorOrderIndex++)//for every new rotor
                    {
                        int counterSwap = 0;
                        foreach (int newRotorOrderItem in newRotorOrder)//for each item in the new rotor order
                        {
                            if(newRotorOrderItem-1 == newRotorOrderIndex)//if the item -1 is equal to the index
                            {
                                tempRotors.Add(modelOut.liveRotorsNames[counterSwap]);//add the swap that needs to happen to the temp rotors
                                tempRotation.Add(modelOut.rotorStepOffset[2 * counterSwap]);//add the item that needs to swap to the temp
                                tempOffset.Add(modelOut.rotorStepOffset[2 * counterSwap + 1]);//add the offset that needs to swap to the temp
                                break;//break the for each item
                            }
                            counterSwap++;//increase the counter swap
                        }
                    }                    
                    modelOut.liveRotorsNames = tempRotors;//set the model out rotors to the new order
                    modelOut.rotorStepOffset = new List<string>();
                    for(int i = 0;i < tempRotation.Count; i++)//for the nunber of rotors
                    {
                        modelOut.rotorStepOffset.Add(tempRotation[i]);//add the rotation
                        modelOut.rotorStepOffset.Add(tempOffset[i]);//then add the offset
                    }
                    _logger.LogInformation("New Rotor order: " + String.Join(", ", modelOut.liveRotorsNames.ToArray()));
                    //update the enigma model rotor order
                    for(int i = 0; i< modelOut.liveRotorsNames.Count;i++)//for each rotor name in the new order
                    {
                        foreach (Rotor r in _physicalConfiguration.rotors)//for each rotor in the physical configuration
                        {
                            if (r.name == modelOut.liveRotorsNames[i])//if the rotor name matches
                            {
                                enigmaModel.rotors.Add(new RotorModel(r, (int)Convert.ToChar(modelOut.rotorStepOffset[2 * i]) - 65, Convert.ToInt16(modelOut.rotorStepOffset[2 * i + 1])));//add the rotor model to the enigma rotors
                                break;//break the for
                            } 
                        }
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//merge the changes of the enigma model
                    
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "rotorSaveEdit"://edit offset and step
                    _logger.LogInformation("Edit Rotors");
                    enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//load the latest enigma model
                    _logger.LogInformation("Before: " + JsonConvert.SerializeObject(enigmaModel));
                    foreach (RotorModel r in enigmaModel.rotors)//for each rotor
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);//add the r
                        var offset = Request.Form[$"{r.rotor.name} offset"].ToString();//got the ring offset from form
                        var rotation = Request.Form[$"{r.rotor.name} step"].ToString();//get the rotation from the form
                        r.ringOffset = Convert.ToInt32(offset);//update the offset on enigma model
                        r.rotation = Convert.ToInt32(Convert.ToChar(rotation) - 65);//update the rotation on the enigma model
                        modelOut.rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65 + r.rotation)));//add the rotation to the out model
                        modelOut.rotorStepOffset.Add(r.ringOffset.ToString());//add the ringoffset to the out model
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel,Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//merge with previous save state
                    _logger.LogInformation("After: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "Enigma":
                    _logger.LogInformation("Go to the simulator from rotors");
                    enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//get the current model                    
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);//creare a new MainView model from the enigma model
                    return View("../Enigma/Index", mainviewmodel);//return the enigma main page
                default://unknown command
                    return View(modelOut);
            }            
        }
        #endregion

        #region relflector
        /// <summary>
        /// Get reflector
        /// </summary>
        /// <returns>Reflector view</returns>
        public IActionResult Reflector()
        {
            _logger.LogInformation("Get reflector");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//get the current save
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            ReflectorViewModel rvm = new ReflectorViewModel(currentSave.reflector, _physicalConfiguration);//construct new Reflector view model from physical configuraion and current save
            return View(rvm);//return the view
        }
        /// <summary>
        /// Post function for Reflector
        /// </summary>
        /// <param name="modelIn">The model past in</param>
        /// <returns>View of next page</returns>
        [HttpPost]
        public IActionResult Reflector(ReflectorViewModel modelIn)
        {
            _logger.LogInformation("Post reflector");
            ReflectorViewModel modelOut = new ReflectorViewModel();
            modelOut._physicalConfiguration = this._physicalConfiguration;//set the physical configuration of the out model
            EnigmaModel enigmaModel = new EnigmaModel();
            foreach (Rotor r in _physicalConfiguration.reflectors)//for each reflector
            {
                if (r.name == modelIn.liveReflectorName)//if the reflector name is the same as the new reflector name
                {
                    _logger.LogInformation("New reflector: " + modelIn.liveReflectorName);
                    enigmaModel.reflector = new RotorModel(r);//set the new reflector
                    break;//breaking searching
                }
            }
            EnigmaModel mergedEnigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//merge the new reflector in
            modelOut.liveReflectorName = mergedEnigmaModel.reflector.rotor.name;//set the model our reflector name
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(mergedEnigmaModel));
            switch (modelIn.Command)
            {              
                case "Enigma"://go to main page
                    _logger.LogInformation("Go to simulator from Reflector");
                    MainViewModel mainviewmodel = new MainViewModel(mergedEnigmaModel);//construct main model
                    return View("../Enigma/Index", mainviewmodel);//return the view
                default://unrecognised command
                    return View(modelOut);
            }            
        }
        #endregion

        #region plugboard
        /// <summary>
        /// Get a View the current plugboard
        /// </summary>
        /// <returns>View of Plugboard.html</returns>
        public IActionResult Plugboard()
        {
            _logger.LogInformation("Get Plugboard");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));  //Loads the current enigma state from file
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            PlugboardViewModel pvm = new PlugboardViewModel(currentSave.plugboard);//Creates a new View model from the current plugboard
            return View(pvm);
        }
        /// <summary>
        /// Performs tasks based on posts from the plugboard page
        /// 
        /// These tasks are to clear the plugboard or go back to the main enigma page (saving the current plugboard)
        /// </summary>
        /// <param name="modelIn">The model passed in from the View</param>
        /// <returns>View of the next page</returns>
        [HttpPost]
        public IActionResult Plugboard(PlugboardViewModel modelIn)
        {
            //TODO log this more
            _logger.LogInformation("Post Plugboard");
            PlugboardViewModel modelOut = new PlugboardViewModel();
            EnigmaModel enigmaModel = new EnigmaModel();
            switch (modelIn.Command)
            {
                case "clear":
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//TODO could be load rather than merge
                    enigmaModel.plugboard = new Dictionary<int, int>();//set the plugboard to empty
                    Services.FileHandler.overwrite(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//overwrite the existing
                    return View(modelOut);
                case "Enigma":
                    for (int i = 1; i <= 10; i++)
                    {
                        var a = Request.Form[$"Pair {i} A"].ToString();
                        var b = Request.Form[$"Pair {i} B"].ToString();
                        if (a != "" && b != "")//if there is a pair entered
                        {
                            enigmaModel.plugboard.Add(Convert.ToInt16(Convert.ToChar(a)) - 65, Convert.ToInt16(Convert.ToChar(b)) - 65);//add the letters converted to integers
                        }
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//merge the changes to the plugboard with the current state
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);//construct a new Main View Model from the enigma model
                    return View("../Enigma/Index", mainviewmodel);//load the main view
                default://Unknown command
                    return View(modelOut);
            }
        }
        #endregion
    }
}
