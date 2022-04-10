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

        public List<string> getNotAllowedOptions()
        {
            List<string> notAllowedOptions = new List<string>();
            foreach (KeyValuePair<int, int> entry in this.plugboard)
            {
                notAllowedOptions.Add(Convert.ToString(Convert.ToChar(65+entry.Key)));
                notAllowedOptions.Add(Convert.ToString(Convert.ToChar(65 + entry.Value)));
            }
            return notAllowedOptions;
        }
    }
}
