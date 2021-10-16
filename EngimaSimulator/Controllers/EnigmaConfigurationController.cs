using EngimaSimulator.Configuration.Models;
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
        private ILogger<EnigmaConfigurationController> _logger;
        public EnigmaConfigurationController(ILogger<EnigmaConfigurationController> logger,PhysicalConfiguration physicalConfiguration)
        {
            _logger = logger;
            _physicalConfiguration = physicalConfiguration;
        }
        public IActionResult Plugboard()
        {
            PlugboardViewModel pvm = new PlugboardViewModel();
            return View(pvm);
        }
        public IActionResult Rotors()
        {
            RotorViewModel rvm = new RotorViewModel();
            return View(rvm);
        }
    }
}
