using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models;
using EngimaSimulator.Models.Enigma;
using EngimaSimulator.Models.EnigmaConfiguration;
using EngimaSimulator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EngimaSimulator.Controllers
{
    public class EnigmaController : Controller
    {
        private readonly ILogger<EnigmaController> _logger;
        private readonly BasicConfiguration _basicConfiguration;
        private readonly EncodingService _encodingService;

        public EnigmaController(ILogger<EnigmaController> logger, BasicConfiguration basicConfiguration, EncodingService encodingService)
        {
            _logger = logger;
            _basicConfiguration = basicConfiguration;
            _encodingService = encodingService;
        }        
        public IActionResult Index()
        {
            _logger.LogInformation("Load main page");
            EnigmaModel enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
            MainViewModel model = new MainViewModel(enigmaModel);
            return View(model);
        }
        [HttpPost]
        public IActionResult Index(MainViewModel modelIn)
        {
            _logger.LogInformation("Post index");
            EnigmaModel enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
            MainViewModel modelOut = new MainViewModel(enigmaModel);
            modelOut.inputTextBox = modelIn.inputTextBox;
            switch (modelIn.Command)
            {
                case "Convert":
                    modelOut.outputTextBox = _encodingService.encode(modelIn.inputTextBox, enigmaModel);
                    modelOut.enigmaModel = enigmaModel;
                    foreach (char c in modelIn.inputTextBox)
                    {
                        modelOut.enigmaModel = _encodingService.stepRotors(modelOut.enigmaModel);
                    }
                    //modelOut.enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(modelOut.enigmaModel,Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    break;
                default:
                    break;
            }
            return View(modelOut);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
