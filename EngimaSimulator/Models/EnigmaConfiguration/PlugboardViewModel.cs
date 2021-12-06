using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class PlugboardViewModel
    {
        public Dictionary<char, char> plugboard;
        public string Command { get; set; }
        public PlugboardViewModel(Dictionary<char, char> plugboard)
        {
            this.plugboard = plugboard;
        }

        public PlugboardViewModel()
        {
            plugboard = new Dictionary<char, char>();
        }
    }
}
