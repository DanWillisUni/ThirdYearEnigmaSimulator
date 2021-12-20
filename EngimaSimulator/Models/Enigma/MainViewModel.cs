using Microsoft.AspNetCore.Mvc;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models.Enigma
{
    public class MainViewModel
    {
        public EnigmaModel enigmaModel { get; set; }
        public string Command { get; set; }
        public string outputTextBox { get; set; }
        public string inputTextBox { get; set; }
        public string plugboardPairs { get {
                string r = "";
                foreach(KeyValuePair<int, int> entry in enigmaModel.plugboard)
                {
                    r += Convert.ToChar(entry.Key + 65);
                    r += Convert.ToChar(entry.Value + 65);
                    r += " ";
                }
                return r;
            } }
        public MainViewModel()
        {
            this.enigmaModel = new EnigmaModel();
            outputTextBox = "";
        }
        public MainViewModel(EnigmaModel enigmaModel)
        {
            this.enigmaModel = enigmaModel;
            outputTextBox = "";
        }
        
    }
}
