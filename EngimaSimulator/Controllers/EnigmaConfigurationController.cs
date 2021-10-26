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
        private PhysicalConfiguration _physicalConfiguration;
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
        public IActionResult Rotors(RotorViewModel rvm)
        {
            switch (rvm.Command)
            {
                case "rotorOrderSave":
                    int counter = 1;
                    List<int> orderOfPreferanceIds = new List<int>();
                    List<string> SupplierList = new List<string>();
                    //populate supplier list                    
                    break;
                default:
                    break;
            }
            MainViewModel mvm = new MainViewModel(_enigmaModel);
            return View("Enigma/Index", mvm);
        }
    }
}
