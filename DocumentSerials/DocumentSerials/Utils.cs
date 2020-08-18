using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace DocumentSerials
{
    public class SerialCode
    {
        private List<string> documentCodes = new List<string> { };
        private Dictionary<string, int> sequenceNumber = new Dictionary<string, int> { };

        private const char groupSeparator = '-';
        private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int num_of_blocks = 4;

        public SerialCode()
        {
            Console.WriteLine("Creating a new SerialCode object");
        }

        private void checkDocument(string document)
        {
            if (!documentCodes.Contains(document))
            {
                documentCodes.Add(document);
                sequenceNumber.Add(document, 0);
            }
            return;
        }

        private string HashString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

        public string Generate(string document, int duration)
        {
            this.checkDocument(document);
            sequenceNumber[document] += 1;

            string text = document + groupSeparator + duration + groupSeparator + Convert.ToString(sequenceNumber[document]);

            string code = this.HashString(text);
            string groupedCode = "";
            for(int i = 0; i < code.Length; i += num_of_blocks)
            {
                string s = (i + num_of_blocks < code.Length) ? groupSeparator.ToString() : "";
                groupedCode += code.Substring(i, Convert.ToInt32(code.Length / num_of_blocks)) + s;
            }
            return groupedCode;
        }
    }
}
