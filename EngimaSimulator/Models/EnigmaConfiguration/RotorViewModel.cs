using EngimaSimulator.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.EnigmaConfiguration
{
    public class RotorViewModel
    {
        public List<RotorModel> rotors;
        public RotorModel reflector;
        public readonly PhysicalConfiguration _physicalConfiguration;

        public RotorViewModel(List<RotorModel> rotors, RotorModel reflector, PhysicalConfiguration physicalConfiguration)
        {
            this.rotors = rotors;
            this.reflector = reflector;
            _physicalConfiguration = physicalConfiguration;
        }
    }
}
