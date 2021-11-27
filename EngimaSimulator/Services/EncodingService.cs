using EngimaSimulator.Models.NonView;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EngimaSimulator.Services
{
    public class EncodingService
    {
        private readonly ILogger<EncodingService> _logger;
        public EncodingService(ILogger<EncodingService> logger)
        {
            _logger = logger;
        }

        #region encoding
        public string encode(string input,EnigmaModel em,string enigmaFilePath) 
        { 
            string r = "";
            string formattedInput = Regex.Replace(input.ToUpper(), @"[^A-Z]", string.Empty);
            int count = 0;
            foreach (char c in formattedInput)
            {
                em = stepRotors(em);
                r += encodeOneChar(em, c);
                count += 1;
                if (count >= 5)
                {
                    count = 0;
                    r += " ";
                }
            }
            //Services.FileHandler.mergeEnigmaConfiguration(em, enigmaFilePath);
            return r;
        }

        private char encodeOneChar(EnigmaModel em, char input)
        {
            _logger.LogInformation("Encode: " + input);
            char current = input;
            current = plugboardSwap(em.plugboard, current);
            _logger.LogDebug("Encoding rotors right to left");
            foreach (RotorModel r in em.rotors.Reverse<RotorModel>())
            {
                current = rotorEncode(r, current);
            }
            _logger.LogDebug("Reflector");
            current = rotorEncode(em.reflector, current);
            _logger.LogDebug("Encoding rotors left to right");
            foreach (RotorModel r in em.rotors)
            {
                current = rotorEncodeInverse(r, current);
            }
            current = plugboardSwap(em.plugboard, current);
            _logger.LogInformation(input + ":" + current);
            return current;
        }
        private char plugboardSwap(Dictionary<char, char> plugboard, char input)
        {
            _logger.LogDebug("plugboardSwap: " + input);
            foreach (KeyValuePair<char, char> entry in plugboard)
            {
                if (entry.Key.Equals(input))
                {
                    _logger.LogInformation("Swapped " + input + " with " + entry.Value);
                    return entry.Value;
                }
                if (entry.Value.Equals(input))
                {
                    _logger.LogInformation("Swapped " + input + " with " + entry.Key);
                    return entry.Key;
                }
            }
            _logger.LogInformation("No swap was made: " + input);
            return input;
        }
        private char rotorEncode(RotorModel rm, char input)
        {
            _logger.LogDebug("Input: " + input);
            _logger.LogDebug("Rotor rotation: " + rm.rotation);
            _logger.LogDebug("Order: " + rm.rotor.order);

            int charNumber = Convert.ToInt32(input) - 65;
            int OrderIndex = charNumber + rm.rotation - rm.ringOffset;
            char encodedChar = rm.rotor.order[mod26(OrderIndex)];
            int encodedCharRotated = Convert.ToInt32(encodedChar) - 65 - rm.rotation + rm.ringOffset;
            char r = Convert.ToChar(65 + mod26(encodedCharRotated));
            _logger.LogInformation(input + " returns " + r);
            return r;
        }
        private char rotorEncodeInverse(RotorModel rm, char input)
        {
            _logger.LogDebug("Input: " + input);
            _logger.LogDebug("Rotor rotation: " + rm.rotation);
            _logger.LogDebug("Order: " + rm.rotor.order);

            int charNumber = Convert.ToInt32(input) - 65;
            int charRotated = charNumber + rm.rotation - rm.ringOffset;
            int encodedCharNumber = rm.rotor.order.IndexOf(Convert.ToChar(mod26(charRotated) + 65));
            char r = Convert.ToChar(65 + mod26(encodedCharNumber - rm.rotation + rm.ringOffset));
            _logger.LogInformation(input + " returns " + r);
            return r;
        }
        private int mod26(int a)
        {
            while (a >= 26)
            {
                a = a - 26;
            }
            while (a < 0)
            {
                a = a + 26;
            }
            return a;
        }
        private EnigmaModel stepRotors(EnigmaModel em)
        {
            _logger.LogDebug("Step rotors");
            em.rotors[em.rotors.Count - 1].rotation = (em.rotors[em.rotors.Count - 1].rotation + 1) % 26;
            _logger.LogDebug($"Stepped {em.rotors[em.rotors.Count - 1].rotor.name} to {em.rotors[em.rotors.Count - 1].rotor.order[em.rotors[em.rotors.Count - 1].rotation]}");
            for (int i = em.rotors.Count - 1; i >= 1; i--)
            {
                RotorModel r = em.rotors[i];
                if (r.rotor.turnoverNotches.Contains(r.rotor.order[(r.rotation - 1) == -1 ? 25 : (r.rotation - 1)]))
                {
                    em.rotors[i - 1].rotation = (em.rotors[i - 1].rotation + 1) % 26;
                    _logger.LogDebug($"Stepped {em.rotors[i - 1].rotor.name} to {em.rotors[i - 1].rotor.order[em.rotors[i - 1].rotation]}");
                }
                else
                {
                    _logger.LogDebug("Finished stepping");
                    break;
                }
            }
            return em;
        }
        #endregion
    }
}
