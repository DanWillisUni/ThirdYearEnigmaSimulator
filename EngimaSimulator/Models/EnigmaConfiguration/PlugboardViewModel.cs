using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class PlugboardViewModel
    {
        private Dictionary<char, char> plugboard;

        public PlugboardViewModel(Dictionary<char, char> plugboard)
        {
            this.plugboard = plugboard;
        }

        public EnigmaModel enigmaModel { get; set; }
    }
}
