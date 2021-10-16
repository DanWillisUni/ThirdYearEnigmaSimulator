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
    
    public class Rotor
    {
        public string name { get; set; }
        public string order { get; set; }
    }
}
