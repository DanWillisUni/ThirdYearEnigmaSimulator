using EngimaSimulator.Configuration.Models;
using SharedCL;
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
        public List<string> rotorStepOffset { get; set; }
        public RotorViewModel() 
        {
            liveRotorsNames = new List<string>();//empty list for rotor names
            rotorStepOffset = new List<string>();//empty list for offset and rotaions
        }
        public RotorViewModel(List<RotorModel> rotors, PhysicalConfiguration physicalConfiguration)
        {
            liveRotorsNames = new List<string>();//empty list for rotor names
            rotorStepOffset = new List<string>();//empty list for offset and rotaions
            if (rotors != null)//if the rotors are not null
            {
                foreach (RotorModel r in rotors)//for each rotor
                {
                    liveRotorsNames.Add(r.rotor.name);//add the rotor name
                    rotorStepOffset.Add(Convert.ToString(Convert.ToChar(65+ r.rotation)));//add the rotation as a char
                    rotorStepOffset.Add(r.ringOffset.ToString());//add the offset
                }
            }            
            _physicalConfiguration = physicalConfiguration;//set the physical configuration
        }
    }
}
