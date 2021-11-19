using DAL;
using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models;
using EngimaSimulator.Models.Enigma;
using EngimaSimulator.Models.EnigmaConfiguration;
using FileHandler;
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
        public EnigmaConfigurationController(ILogger<EnigmaConfigurationController> logger,PhysicalConfiguration physicalConfiguration)
        {
            _logger = logger;
            _physicalConfiguration = physicalConfiguration;
        }
        public IActionResult Plugboard()
        {
            EnigmaModel currentSave = EnigmaConfiguration.getCurrentSave("currentConfig.json");
            PlugboardViewModel pvm = new PlugboardViewModel(currentSave.plugboard);
            return View(pvm);
        }
        public IActionResult Rotors()
        {
            EnigmaModel currentSave = EnigmaConfiguration.getCurrentSave("currentConfig.json");
            RotorViewModel rvm = new RotorViewModel(currentSave.rotors,_physicalConfiguration);
            return View(rvm);
        }
        [HttpPost]
        public IActionResult Rotors(RotorViewModel modelIn)
        {
            RotorViewModel modelOut = new RotorViewModel();
            modelOut._physicalConfiguration = this._physicalConfiguration;
            EnigmaModel enigmaModel = new EnigmaModel();
            switch (modelIn.Command)
            {
                case "rotorSave":
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
                    enigmaModel = EnigmaConfiguration.mergeEnigmaConfiguration(enigmaModel);
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    return View(modelOut);
                case "rotorSaveOrder":
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
                    EnigmaModel currentSave = EnigmaConfiguration.getCurrentSave("currentConfig.json");
                    foreach (RotorModel r in currentSave.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    //order swap
                    List<string> tempRotors = new List<string>();
                    foreach(int i in newRotorOrder)
                    {
                        tempRotors.Add(modelOut.liveRotorsNames[i - 1]);
                    }
                    modelOut.liveRotorsNames = tempRotors;

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
                    enigmaModel = EnigmaConfiguration.mergeEnigmaConfiguration(enigmaModel);
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    return View(modelOut);
                case "Enigma":
                    /*foreach (string rn in modelIn.liveRotorsNames)
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
                    enigmaModel = mergeEnigmaConfiguration(enigmaModel);*/
                    enigmaModel = EnigmaConfiguration.getCurrentSave("currentConfig.json");
                    foreach (RotorModel r in enigmaModel.rotors)
                    {
                        modelOut.liveRotorsNames.Add(r.rotor.name);
                    }
                    MainViewModel mainviewmodel = new MainViewModel(enigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    break;
            }
            return View(modelOut);
        }

        public IActionResult Reflector()
        {
            EnigmaModel currentSave = EnigmaConfiguration.getCurrentSave("currentConfig.json");
            ReflectorViewModel rvm = new ReflectorViewModel(currentSave.reflector, _physicalConfiguration);
            return View(rvm);
        }
        [HttpPost]
        public IActionResult Reflector(ReflectorViewModel modelIn)
        {
            ReflectorViewModel modelOut = new ReflectorViewModel();
            modelOut._physicalConfiguration = this._physicalConfiguration;
            EnigmaModel enigmaModel = new EnigmaModel();
            foreach (Rotor r in _physicalConfiguration.reflectors)
            {
                if (r.name == modelIn.liveReflectorName)
                {
                    enigmaModel.reflector = new RotorModel(r);
                    break;
                }
            }
            EnigmaModel mergedEnigmaModel = EnigmaConfiguration.mergeEnigmaConfiguration(enigmaModel);
            modelOut.liveReflectorName = mergedEnigmaModel.reflector.rotor.name;
            switch (modelIn.Command)
            {              
                case "Enigma":
                    MainViewModel mainviewmodel = new MainViewModel(mergedEnigmaModel);
                    return View("../Enigma/Index", mainviewmodel);
                default:
                    break;
            }
            return View(modelOut);
        }
    }
}
