using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace plagiarism
{
    public class Shingles
    {
        private const string Delims = " =.,_{}*\\\n\r\"/?[];:()!\'’”„";
        public static bool Checkinputfile = true;
        private int _shingleLength;

        public void Length(int x)
        {
            _shingleLength = x;
        }

        public int Length()
        {
            return _shingleLength;
        }

        /// <summary>
        /// Проверка двух файлов на схожесть
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public double CompareStrings(string s1, string s2, int length)
        {
            _shingleLength = length;
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
            return same/((double) (shingles1.Count()))*100;
        }

        /// <summary>
        /// Получение хэшей шинглов
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shingleLength"></param>
        /// <returns></returns>
        private HashSet<string> GetShingles(ref string source, int shingleLength)
        {
            string[] split = source.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var shingles = new HashSet<string>();
            var tmp = "";
            for (int j = 0; j < shingleLength; j++)
            {
                tmp += split[j];
            }
            for (var i = shingleLength; i < split.Length; i++)
            {
                var sb = new StringBuilder(tmp, split[i - shingleLength].Length,
                                           tmp.Length - split[i - shingleLength].Length, 100*shingleLength);
                sb.Append(split[i]);
                shingles.Add(tmp = sb.ToString());
            }
            return shingles;

        }     
    }
}
