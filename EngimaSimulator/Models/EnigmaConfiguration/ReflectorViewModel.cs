using EngimaSimulator.Configuration.Models;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class ReflectorViewModel
    {
        public PhysicalConfiguration _physicalConfiguration { get; set; }
        public string Command { get; set; }
        public string liveReflectorName { get; set; }

        public ReflectorViewModel()
        {
            liveReflectorName = "";//set  the reflector name to empty
        }
        public ReflectorViewModel(RotorModel reflector, PhysicalConfiguration physicalConfiguration)
        {
            if (reflector != null)//if the reflector is not null
            {
                liveReflectorName = reflector.rotor.name;//set the reflector name to the reflector passed ins name
            }
            else
            {
                liveReflectorName = "";
            }
            _physicalConfiguration = physicalConfiguration;//set the physical configuration
        }
    }
}
