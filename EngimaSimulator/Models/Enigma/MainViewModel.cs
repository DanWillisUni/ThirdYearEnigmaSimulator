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
        public string plugboardPairs { get {//used for main page to display the plugboard in a readable way
                string r = "";
                foreach(KeyValuePair<int, int> entry in enigmaModel.plugboard)//for each entry in the plugboard
                {
                    r += Convert.ToChar(entry.Key + 65);//add the uppercase char of the key
                    r += Convert.ToChar(entry.Value + 65);//add the uppercase char of the value
                    r += " ";//add a space
                }
                return r;
            } }
        public MainViewModel()
        {
            this.enigmaModel = new EnigmaModel();//new enigma model
            outputTextBox = "";//empty output box
        }
        public MainViewModel(EnigmaModel enigmaModel)
        {
            this.enigmaModel = enigmaModel;//set the enigma model
            outputTextBox = "";//empty output box
        }
        
    }
}
