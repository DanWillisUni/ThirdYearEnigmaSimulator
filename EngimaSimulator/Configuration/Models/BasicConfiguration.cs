using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Configuration.Models
{
    public class BasicConfiguration
    {
        public FilePath tempConfig { get; set; }
    }
    public class FilePath
    {
        public string dir { get; set; }
        public string fileName { get; set; }
    }
}
