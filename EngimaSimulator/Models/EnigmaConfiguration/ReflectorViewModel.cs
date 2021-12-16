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
            liveReflectorName = "";
        }
        public ReflectorViewModel(RotorModel reflector, PhysicalConfiguration physicalConfiguration)
        {
            if (reflector != null)
            {
                liveReflectorName = reflector.rotor.name;
            }
            else
            {
                liveReflectorName = "";
            }
            _physicalConfiguration = physicalConfiguration;
        }
    }
}
