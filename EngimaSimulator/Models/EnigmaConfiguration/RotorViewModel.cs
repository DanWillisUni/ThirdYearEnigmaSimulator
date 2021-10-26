using EngimaSimulator.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class RotorViewModel
    {        
        public readonly PhysicalConfiguration _physicalConfiguration;
        public string Command { get; set; }
        public List<string> liveRotorsNames { get; set; }
        public string[] newRotorOrder { get; set; }
        public string liveReflectorName { get; set; }
        public RotorViewModel() { }
        public RotorViewModel(List<RotorModel> rotors, RotorModel reflector, PhysicalConfiguration physicalConfiguration)
        {
            liveRotorsNames = new List<string>();
            if (rotors != null)
            {
                foreach (RotorModel r in rotors)
                {
                    liveRotorsNames.Add(r.rotor.name);
                }
            }

            if(reflector != null)
            {
                liveReflectorName = reflector.rotor.name;
            }            
            _physicalConfiguration = physicalConfiguration;

            liveRotorsNames = new List<string>{"A","B","C"};//test purposes
        }
    }
}
