using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.Enigma
{
    public class MainViewModel
    {
        public MainViewModel(EnigmaModel enigmaModel)
        {
            this.enigmaModel = enigmaModel;
        }
        public MainViewModel()
        {
            this.enigmaModel = new EnigmaModel();
        }
        public EnigmaModel enigmaModel { get; set; }
        public string Command { get; set; }
    }
}
