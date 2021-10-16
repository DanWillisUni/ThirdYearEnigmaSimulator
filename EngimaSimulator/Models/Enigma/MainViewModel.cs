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

        public EnigmaModel enigmaModel { get; set; }
    }
}
