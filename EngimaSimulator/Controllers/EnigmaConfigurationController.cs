using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models;
using EngimaSimulator.Models.Enigma;
using EngimaSimulator.Models.EnigmaConfiguration;
using EngimaSimulator.Models.NonView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Controllers
{
    public class EnigmaConfigurationController : Controller
    {
        private readonly PhysicalConfiguration _physicalConfiguration;
        private readonly ILogger<EnigmaConfigurationController> _logger;
        private readonly BasicConfiguration _basicConfiguration;
        public EnigmaConfigurationController(ILogger<EnigmaConfigurationController> logger,PhysicalConfiguration physicalConfiguration,BasicConfiguration basicConfiguration)
        {
            _logger = logger;
            _physicalConfiguration = physicalConfiguration;
            _basicConfiguration = basicConfiguration;
            _logger.LogDebug("Physical Configuration: " + JsonConvert.SerializeObject(_physicalConfiguration));
            _logger.LogDebug("Basic Configuration: " + JsonConvert.SerializeObject(_basicConfiguration));
        }
        public IActionResult Plugboard()
        {
            _logger.LogInformation("Get Plugboard");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            PlugboardViewModel pvm = new PlugboardViewModel(currentSave.plugboard);
            return View(pvm);
        }
        [HttpPost]
        public IActionResult Plugboard(PlugboardViewModel modelIn)
        {
            _logger.LogInformation("Post Plugboard");
            PlugboardViewModel modelOut = new PlugboardViewModel();
            EnigmaModel enigmaModel = new EnigmaModel();
            switch (modelIn.Command)
            {
                case "clear":
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    enigmaModel.plugboard = new Dictionary<char, char>();
                    Services.FileHandler.overwrite(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    return View(modelOut);
                case "Enigma":
                    //ideally there would be a check here to see if the letter had been selected before but I havent got around to it yet
                    for (int i = 1; i <= 10; i++)
                    {
                        var a = Request.Form[$"Pair {i} A"].ToString();
                        var b = Request.Form[$"Pair {i} B"].ToString();
                        if(a != "" && b != "")
                        {
                            enigmaModel.plugboard.Add(Convert.ToChar(a), Convert.ToChar(b));
                        }                        
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    return View(modelOut);
            }            
        }

        public IActionResult Rotors()
        {
            _logger.LogInformation("Get rotors");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            RotorViewModel rvm = new RotorViewModel(currentSave.rotors,_physicalConfiguration);
            return View(rvm);
        }
        [HttpPost]
        public IActionResult Rotors(RotorViewModel modelIn)
        {
            _logger.LogInformation("Post rotors");
            RotorViewModel modelOut = new RotorViewModel();
            modelOut._physicalConfiguration = this._physicalConfiguration;
            EnigmaModel enigmaModel = new EnigmaModel();
            switch (modelIn.Command)
            {
                case "rotorSave":
                    _logger.LogInformation("Save the rotors");
                    _logger.LogInformation("Saving rotors to: " + String.Join(", ", modelIn.liveRotorsNames.ToArray()));
                    foreach (string rn in modelIn.liveRotorsNames)
                    {
                        foreach (Rotor r in _physicalConfiguration.rotors)
                        {
                            if (r.name == rn)
                            {
                                enigmaModel.rotors.Add(new RotorModel(r));
                                break;
                            }
                        }
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                        modelOut.rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65 + r.rotation)));
                        modelOut.rotorStepOffset.Add(r.ringOffset.ToString());
                    }
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "rotorSaveOrder":
                    _logger.LogInformation("ReOrder rotors");
                    //get new order
                    bool continueOn = true;
                    int counter = 1;
                    List<int> newRotorOrder = new List<int>();
                    do
                    {
                        var newItem = Request.Form[$"rotorOrder_{counter}"].ToString();
                        if (String.IsNullOrEmpty(newItem))
                        {
                            continueOn = false;
                        }
                        else
                        {
                            newRotorOrder.Add(Convert.ToInt32(newItem));
                        }
                        counter++;
                    } while (continueOn);
                    //get previous order
                    EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    foreach (RotorModel r in currentSave.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    _logger.LogInformation("Previous Rotor order: " + String.Join(", ", modelOut.liveRotorsNames.ToArray()));
                    //order swap
                    List<string> tempRotors = new List<string>();
                    for(int i = 0; i<=newRotorOrder.Count - 1; i++)
                    {
                        int counterSwap = 0;
                        foreach (int o in newRotorOrder)
                        {
                            if(o-1 == i)
                            {
                                tempRotors.Add(modelOut.liveRotorsNames[counterSwap]);
                                break;
                            }
                            counterSwap++;
                        }
                    }                    
                    modelOut.liveRotorsNames = tempRotors;
                    _logger.LogInformation("New Rotor order: " + String.Join(", ", modelOut.liveRotorsNames.ToArray()));
                    foreach (string rn in modelOut.liveRotorsNames)
                    {
                        foreach (Rotor r in _physicalConfiguration.rotors)
                        {
                            if (r.name == rn)
                            {
                                enigmaModel.rotors.Add(new RotorModel(r));
                                break;
                            }
                        }
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    modelOut.liveRotorsNames = new List<string>();
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                        modelOut.rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65 + r.rotation)));
                        modelOut.rotorStepOffset.Add(r.ringOffset.ToString());
                    }
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "rotorSaveEdit":
                    _logger.LogInformation("Edit Rotors");
                    enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    _logger.LogInformation("Before: " + JsonConvert.SerializeObject(enigmaModel));
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                        var offset = Request.Form[$"{r.rotor.name} offset"].ToString();
                        var rotation = Request.Form[$"{r.rotor.name} step"].ToString();
                        r.ringOffset = Convert.ToInt32(offset);
                        r.rotation = Convert.ToInt32(Convert.ToChar(rotation) - 65);
                    }
                    enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel,Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    _logger.LogInformation("After: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "Enigma":
                    _logger.LogInformation("Go to the simulator from rotors");
                    enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    return View(modelOut);
            }            
        }

        public IActionResult Reflector()
        {
            _logger.LogInformation("Get reflector");
            EnigmaModel currentSave = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(currentSave));
            ReflectorViewModel rvm = new ReflectorViewModel(currentSave.reflector, _physicalConfiguration);
            return View(rvm);
        }
        [HttpPost]
        public IActionResult Reflector(ReflectorViewModel modelIn)
        {
            _logger.LogInformation("Post reflector");
            ReflectorViewModel modelOut = new ReflectorViewModel();
            modelOut._physicalConfiguration = this._physicalConfiguration;
            EnigmaModel enigmaModel = new EnigmaModel();
            foreach (Rotor r in _physicalConfiguration.reflectors)
            {
                if (r.name == modelIn.liveReflectorName)
                {
                    _logger.LogInformation("New reflector: " + modelIn.liveReflectorName);
                    enigmaModel.reflector = new RotorModel(r);
                    break;
                }
            }
            EnigmaModel mergedEnigmaModel = Services.FileHandler.mergeEnigmaConfiguration(enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            modelOut.liveReflectorName = mergedEnigmaModel.reflector.rotor.name;
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(mergedEnigmaModel));
            switch (modelIn.Command)
            {              
                case "Enigma":
                    _logger.LogInformation("Go to simulator from Reflector");
                    MainViewModel mainviewmodel = new MainViewModel(mergedEnigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    return View(modelOut);
            }            
        }
    }
}
