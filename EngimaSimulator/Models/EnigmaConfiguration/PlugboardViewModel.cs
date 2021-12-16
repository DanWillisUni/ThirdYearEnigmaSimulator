using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class PlugboardViewModel
    {
        public Dictionary<int, int> plugboard;
        public string Command { get; set; }
        public PlugboardViewModel(Dictionary<int, int> plugboard)
        {
            this.plugboard = plugboard;
        }

        public PlugboardViewModel()
        {
            plugboard = new Dictionary<int, int>();
        }
    }
}
