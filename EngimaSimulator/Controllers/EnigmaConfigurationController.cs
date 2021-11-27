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
                    foreach(int i in newRotorOrder)
                    {
                        tempRotors.Add(modelOut.liveRotorsNames[i - 1]);
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
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    return View(modelOut);
                case "Enigma":
                    _logger.LogInformation("Go to the simulator from rotors");
                    enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    /*foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }*/
                    _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    break;
            }
            return View(modelOut);
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
                    break;
            }
            return View(modelOut);
        }
    }
}
