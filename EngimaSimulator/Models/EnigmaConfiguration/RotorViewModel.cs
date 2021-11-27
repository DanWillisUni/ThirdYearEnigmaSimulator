using EngimaSimulator.Configuration.Models;
using EngimaSimulator.Models.NonView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class RotorViewModel
    {        
        public PhysicalConfiguration _physicalConfiguration { get; set; }
        public string Command { get; set; }
        public List<string> liveRotorsNames { get; set; }
        public RotorViewModel() 
        {
            liveRotorsNames = new List<string>();
        }
        public RotorViewModel(List<RotorModel> rotors, PhysicalConfiguration physicalConfiguration)
        {
            liveRotorsNames = new List<string>();
            if (rotors != null)
            {
                foreach (RotorModel r in rotors)
                {
                    liveRotorsNames.Add(r.rotor.name);
                }
            }            
            _physicalConfiguration = physicalConfiguration;
        }
    }
}
