using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;

namespace plagiarism
{
    public class Shingles
    {
        private const string Delims = " -=.,_{}*\\\n\r\"/?[];:()!\'’”„";
        public static bool Checkinputfile = true;

        /// <summary>
        /// Проверка двух файлов на схожесть
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static double CompareStrings(ref string s1, ref string s2, int length)
        {
            HashSet<string> shingles1, shingles2;
            if (Checkinputfile)
            {
                shingles1 = GetShingles(ref s1, length);
                shingles2 = GetShingles(ref s2, length);
            }
            else
            {
                shingles1 = GetShingles(ref s2, length);
                shingles2 = GetShingles(ref s1, length);
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
        private static HashSet<string> GetShingles(ref string source, int shingleLength)
        {
            var stemmer = new EnglishStemmer();
            var split = source.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < split.Length; index++)
            {
                split[index] = stemmer.Stem(split[index]);
            }
            var shingles = new HashSet<string>();
            var tmp = "";
            if (split.Length < shingleLength)
            {
                return shingles;
            }
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
