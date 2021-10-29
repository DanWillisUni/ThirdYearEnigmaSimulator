using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models;
using EngimaSimulator.Models.Enigma;
using EngimaSimulator.Models.EnigmaConfiguration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Controllers
{
    public class EnigmaConfigurationController : Controller
    {
        private readonly PhysicalConfiguration _physicalConfiguration;
        private readonly ILogger<EnigmaConfigurationController> _logger;
        private EnigmaModel _enigmaModel;
        public EnigmaConfigurationController(ILogger<EnigmaConfigurationController> logger,PhysicalConfiguration physicalConfiguration)
        {
            _logger = logger;
            _physicalConfiguration = physicalConfiguration;
            _enigmaModel = new EnigmaModel();
        }
        public IActionResult Plugboard()
        {
            PlugboardViewModel pvm = new PlugboardViewModel(_enigmaModel.plugboard);
            return View(pvm);
        }
        public IActionResult Rotors()
        {
            RotorViewModel rvm = new RotorViewModel(_enigmaModel.rotors,_enigmaModel.reflector,_physicalConfiguration);
            return View(rvm);
        }
        [HttpPost]
        public IActionResult Rotors(RotorViewModel modelIn)
        {
            RotorViewModel modelOut = new RotorViewModel();
            switch (modelIn.Command)
            {
                case "rotorSave":
                    modelOut._physicalConfiguration = this._physicalConfiguration;
                    modelOut.liveRotorsNames = modelIn.liveRotorsNames;
                    foreach(string rn in modelIn.liveRotorsNames)
                    {
                        foreach(Rotor r in _physicalConfiguration.rotors)
                        {
                            if(r.name == rn)
                            {
                                _enigmaModel.rotors.Add(new RotorModel(r));
                                break;
                            }
                        }                        
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
                    //order swap
                    List<RotorModel> tempRotors = new List<RotorModel>();
                    foreach(int i in newRotorOrder)
                    {
                        tempRotors.Add(_enigmaModel.rotors[i]);
                    }
                    _enigmaModel.rotors = tempRotors;
                    break;
                default:
                    break;
            }
            MainViewModel mvm = new MainViewModel(_enigmaModel);
            return View("../Enigma/Index", mvm);
        }
    }
}
