using EngimaSimulator.Models.NonView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Configuration.Models
{
    public class PhysicalConfiguration
    {
        public List<Rotor> rotors { get; set; }
        public List<Rotor> reflectors { get; set; }
    }
}
