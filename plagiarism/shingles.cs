using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using plagiarism;

namespace dataAnalyze.Algorithms
{
    public class Shingles
    {
        static public bool Checkinputfile = true;
        private char[] _stopSymbols;
        private int _shingleLength;

        public Shingles(string delims, int shinglelength)
        {
            _stopSymbols = delims.ToCharArray();
            _shingleLength = shinglelength;

        }
        public void Length(int x)
        {
            _shingleLength = x;
        }
        public int Length()
        {
            return _shingleLength;
        }

        public double CompareStrings(string s1, string s2, int length)
        {
            _shingleLength = length;
            RemoveStopSymbols(ref s1);
            RemoveStopSymbols(ref s2);
            HashSet<string> shingles1, shingles2;
            if (Checkinputfile)
            {
                shingles1 = GetShingles(ref s1, _shingleLength);
                shingles2 = GetShingles(ref s2, _shingleLength);
            }
            else
            {
                shingles1 = GetShingles(ref s2, _shingleLength);
                shingles2 = GetShingles(ref s1, _shingleLength);
            }
            var same = shingles1.Count(shingles2.Contains);
            return same /((double) (shingles1.Count()))*100;
        }

        /// <summary>
        /// get shingles and calculate hash for everyone
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shingleLength"></param>
        /// <returns></returns>
        private HashSet<string> GetShingles(ref string source, int shingleLength)
        {
            var shingles =new HashSet<string>();
            var shift = 0;
            for (var i = 0; i < source.Length - (shingleLength - 1); i++)
            {

                shingles.Add(
                    (source.Length >= shift + shingleLength)
                        ? source.Substring(shift, shingleLength)
                        : source.Substring(shift, source.Length - (shift + shingleLength)));
                shift++;
            }
            return shingles;

        }

        /// <summary>
        /// delete some inappropriate chars from the string
        /// </summary>
        /// <param name="source"></param>
        private void RemoveStopSymbols(ref string source)
        {
            var positionForRemove = new int[source.Length];
            var arrayCounter = 0;
            FindIndexOfSymbols(ref source, ref positionForRemove, ref arrayCounter, ref _stopSymbols);
            Array.Resize(ref positionForRemove, arrayCounter);
            Array.Sort(positionForRemove);
            //Array.Reverse(positionForRemove);
            var shift = 0;
            var result = new StringBuilder(source.Length - arrayCounter);
            for (var i = 0; i < source.Length; i++)
            {
                if (i == positionForRemove[shift])
                {
                    if (positionForRemove.Length > shift + 1)
                        shift++;
                }
                else
                    result.Append(source[i]);
            }

            //positionForRemove = null;
            source = result.ToString();

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"> link for original string</param>
        /// <param name="positionsForRemove">array of indexes</param>
        /// <param name="arrayCounter">point to next element in array</param>
        /// <param name="symbols"></param>
        private void FindIndexOfSymbols(ref string source, ref int[] positionsForRemove, ref int arrayCounter, ref char[] symbols)
        {
            for (int i = 0; i < source.Length; i++)
            {
                foreach (var t in symbols)
                    if (source[i] == t)
                    {
                        positionsForRemove[arrayCounter] = i;
                        arrayCounter++;

                    }
            }
        }
    }
}
 



