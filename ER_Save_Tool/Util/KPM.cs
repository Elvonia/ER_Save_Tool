using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ER_Save_Tool
{
    // Knuth-Morris-Pratt Pattern Matching Algorithm
    public class KPM
    {
        /**
         * Search the data byte array for the first occurrence 
         * of the byte array pattern.
         */
        public static int indexOf(byte[] data, byte[] pattern)
        {
            int[] failure = computeFailure(pattern);

            int j = 0;

            for (int i = 0; i < data.Length; i++)
            {
                while (j > 0 && pattern[j] != data[i])
                {
                    j = failure[j - 1];
                }
                if (pattern[j] == data[i])
                {
                    j++;
                }
                if (j == pattern.Length)
                {
                    return i - pattern.Length + 1;
                }
            }
            return -1;
        }

        /**
         * Computes the failure function using a boot-strapping process,
         * where the pattern is matched against itself.
         */
        private static int[] computeFailure(byte[] pattern)
        {
            int[] failure = new int[pattern.Length];

            int j = 0;
            for (int i = 1; i < pattern.Length; i++)
            {
                while (j > 0 && pattern[j] != pattern[i])
                {
                    j = failure[j - 1];
                }
                if (pattern[j] == pattern[i])
                {
                    j++;
                }
                failure[i] = j;
            }

            return failure;
        }
    }
}
