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

        /// <summary>
        /// Load the main page
        /// </summary>
        /// <returns>View of main page</returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Load main page");
            EnigmaModel enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//Get current save
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
            MainViewModel model = new MainViewModel(enigmaModel);//construct view from enigma model
            return View(model);//return view
        }
        /// <summary>
        /// Post from the main page
        /// </summary>
        /// <param name="modelIn">Model passed in</param>
        /// <returns>View for next page</returns>
        [HttpPost]
        public IActionResult Index(MainViewModel modelIn)
        {
            _logger.LogInformation("Post index");
            EnigmaModel enigmaModel = Services.FileHandler.getCurrentSave(Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//get the current save
            _logger.LogInformation("Current save: " + JsonConvert.SerializeObject(enigmaModel));
            MainViewModel modelOut = new MainViewModel(enigmaModel);//construct the next model out
            modelOut.inputTextBox = modelIn.inputTextBox;//set the out text box to the same as the in text box
            switch (modelIn.Command)
            {
                case "Convert"://convert the text
                    modelOut.enigmaModel = enigmaModel;//set the out model enigma model
                    string tempOut = _encodingService.encode(modelIn.inputTextBox, enigmaModel);//encode the text
                    //setting spaces every 5 chars
                    int count = 0;
                    modelOut.outputTextBox = "";//set output to empty string
                    foreach (char c in tempOut) {//for every char encoded
                        modelOut.outputTextBox += c;//add the char to the output
                        count += 1;//increase counter
                        if (count >= 5)//if the counter is greater than or equal to 5
                        {
                            count = 0;//reset count to 0
                            modelOut.outputTextBox += " ";//add a space to the output
                        }
                    }
                    //modelOut.enigmaModel = Services.FileHandler.mergeEnigmaConfiguration(modelOut.enigmaModel,Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));
                    break;
                case "Randomize"://Randomise the enigma model
                    modelOut.enigmaModel = EnigmaModel.randomizeEnigma();//randomise the model
                    Services.FileHandler.overwrite(modelOut.enigmaModel, Path.Combine(_basicConfiguration.tempConfig.dir, _basicConfiguration.tempConfig.fileName));//save the model
                    break;
                default://unrecognised command
                    break;
            }
            return View(modelOut);//return the main view
        }

        /// <summary>
        /// Gets error page
        /// </summary>
        /// <returns>Error view model</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
