using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Security.Policy;

namespace DocumentSerials
{
    public class SerialCode
    {
        private List<string> documentCodes;
        private Random _random;

        private const char groupSeparator = '-';
        public int NumBlocks { get; set; }
        public int Size { get; set; }

        public SerialCode()
        {
            documentCodes = new List<string>();
            _random = new Random();
            // Setting default values
            NumBlocks = 4;
            Size = 20;
        }

        private void checkDocument(string document)
        {
            if (!documentCodes.Contains(document))
            {
                documentCodes.Add(document);
            }
            return;
        }

        private T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + _random.Next(n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }
        // OLD DEPRECATED ALGORITHM
        private string SHA256DownsampledHash(string text)
        {
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
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
        /*
         * This new algorithm merges and shuffles randomly two hashes of the same code text, and returns
         * a substring with the specified length
         */
        private string HashingAlgorithm(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed sha256 = new SHA256Managed();
            MD5 md5 = MD5.Create();

            byte[] hash1 = sha256.ComputeHash(bytes);
            byte[] hash2 = md5.ComputeHash(bytes);

            StringBuilder hash1String = new StringBuilder();
            StringBuilder hash2String = new StringBuilder();
            // Convert the hash byte arrays into hexadecimal strings
            for (int i = 0; i < hash1.Length; i++)
                hash1String.Append(hash1[i].ToString("X2"));
            for (int i = 0; i < hash2.Length; i++)
                hash2String.Append(hash2[i].ToString("X2"));
            // Random shuffle of the concatenation of the two hashes
            byte[] code = Shuffle(Encoding.UTF8.GetBytes(hash1String.ToString() + hash2String.ToString()));
            return Encoding.UTF8.GetString(code).Substring(0, Size);
        }

        public string Generate(string document, string duration, int sequenceNumber)
        {
            this.checkDocument(document);

            string text = document + groupSeparator + duration + groupSeparator + Convert.ToString(sequenceNumber);

            string code = this.HashingAlgorithm(text);
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
