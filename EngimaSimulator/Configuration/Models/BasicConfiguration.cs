using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Configuration.Models
{
    public class BasicConfiguration
    {
        public FilePath tempConfig { get; set; }  //I need a temp filepath to store a temp file that stores the enigma configuration while the simularot is running
    }
    public class FilePath
    {
        public string dir { get; set; }  //directory of the filepath
        public string fileName { get; set; } //filename
    }
}
