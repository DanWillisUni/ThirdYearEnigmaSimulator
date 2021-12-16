using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedCL
{
    public class EncodingService
    {
        public EncodingService()
        {
        }

        public string encode(string input,EnigmaModel em) 
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
            return r;
        }

        private char encodeOneChar(EnigmaModel em, char input)
        {
            int current = Convert.ToInt32(input) - 65;
            current = plugboardSwap(em.plugboard, current);
            foreach (RotorModel r in em.rotors.Reverse<RotorModel>())
            {
                current = rotorEncode(r, current);
            }
            current = rotorEncode(em.reflector, current);
            foreach (RotorModel r in em.rotors)
            {
                current = rotorEncodeInverse(r, current);
            }
            current = plugboardSwap(em.plugboard, current);
            return Convert.ToChar(current + 65);
        }
        private int plugboardSwap(Dictionary<char, char> plugboard, int input)
        {
            foreach (KeyValuePair<char, char> entry in plugboard)
            {
                if (entry.Key.Equals(input))
                {
                    return entry.Value;
                }
                if (entry.Value.Equals(input))
                {
                    return entry.Key;
                }
            }
            return input;
        }
        private char rotorEncode(RotorModel rm, char input)
        {
            int charNumber = Convert.ToInt32(input) - 65;
            int OrderIndex = charNumber + rm.rotation - rm.ringOffset;
            char encodedChar = rm.rotor.order[mod26(OrderIndex)];
            int encodedCharRotated = Convert.ToInt32(encodedChar) - 65 - rm.rotation + rm.ringOffset;
            char r = Convert.ToChar(65 + mod26(encodedCharRotated));
            return r;
        }
        private char rotorEncodeInverse(RotorModel rm, char input)
        {
            int charNumber = Convert.ToInt32(input) - 65;
            int charRotated = charNumber + rm.rotation - rm.ringOffset;
            int encodedCharNumber = rm.rotor.order.IndexOf(Convert.ToChar(mod26(charRotated) + 65));
            char r = Convert.ToChar(65 + mod26(encodedCharNumber - rm.rotation + rm.ringOffset));
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
        public EnigmaModel stepRotors(EnigmaModel em)
        {
            em.rotors[em.rotors.Count - 1].rotation = (em.rotors[em.rotors.Count - 1].rotation + 1) % 26;
            for (int i = em.rotors.Count - 1; i >= 1; i--)
            {
                RotorModel r = em.rotors[i];
                if (r.rotor.turnoverNotches.Contains(r.rotor.order[mod26(r.rotation - 1)]))
                {
                    em.rotors[i - 1].rotation = (em.rotors[i - 1].rotation + 1) % 26;
                }
                else
                {
                    break;
                }
            }
            return em;
        }
    }
}
