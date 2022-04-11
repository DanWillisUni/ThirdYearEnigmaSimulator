using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedCL
{
    /// <summary>
    /// This class is to represent the physical attributes of the machiene that are interchangeable and available
    /// </summary>
    public class PhysicalConfiguration
    {
        public List<Rotor> rotors { get; set; }
        public List<Rotor> reflectors { get; set; }
    }
}
