using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace DocumentSerials
{
    public class SerialCode
    {
        private List<string> documentCodes = new List<string> { };

        private const char groupSeparator = '-';
        private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public int NumBlocks { get; set; }
        public int Size { get; set; }

        public SerialCode()
        {
            Console.WriteLine("Creating a new SerialCode object");
            // Setting default values
            NumBlocks = 4;
            Size = 16;
        }

        private void checkDocument(string document)
        {
            if (!documentCodes.Contains(document))
            {
                documentCodes.Add(document);
            }
            return;
        }

        private string HashString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[Size];

            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

        public string Generate(string document, int duration, int sequenceNumber)
        {
            this.checkDocument(document);

            string text = document + groupSeparator + duration + groupSeparator + Convert.ToString(sequenceNumber);

            string code = this.HashString(text);
            string groupedCode = "";
            for(int i = 0, block_size = Convert.ToInt32(code.Length / NumBlocks); i < code.Length; i += block_size)
            {
                string s = (i + block_size < code.Length) ? groupSeparator.ToString() : "";
                groupedCode += code.Substring(i, block_size) + s;
            }
            return groupedCode;
        }
    }
}
