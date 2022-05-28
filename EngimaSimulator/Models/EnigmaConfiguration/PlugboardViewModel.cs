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
            this.plugboard = plugboard;//set the plugboard
        }
        
        public PlugboardViewModel() : this(new Dictionary<int, int>())
        {
        }
        /// <summary>
        /// Get the strings of the characters that are already in the plugboard so they cannot be added again 
        /// </summary>
        /// <returns>Current plugboard characters</returns>
        public List<string> getNotAllowedOptions()
        {
            List<string> notAllowedOptions = new List<string>();
            foreach (KeyValuePair<int, int> entry in this.plugboard)//for every plugboard pair
            {
                notAllowedOptions.Add(Convert.ToString(Convert.ToChar(65 + entry.Key)));//add key
                notAllowedOptions.Add(Convert.ToString(Convert.ToChar(65 + entry.Value)));//and value
            }
            return notAllowedOptions;
        }
    }
}
