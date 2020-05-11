using System.Collections.Generic;
using System.Linq;

namespace Packages.BrandonUtils.Runtime
{
    public static class ListUtils
    {
        public static T Random<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count() - 1)];
        }

        public static T GrabRandom<T>(this List<T> list)
        {
            var random = list.Random();
            list.Remove(random);
            return random;
        }

        public static List<T> Randomize<T>(this List<T> oldList)
        {
            List<T> backupList = oldList.Clone();
            oldList.Clear();
            while (backupList.Count > 0)
            {
                oldList.Add(GrabRandom(backupList));
            }

            return oldList;
        }

        public static List<T> RandomCopy<T>(this List<T> oldList)
        {
            return oldList.Clone().Randomize();
        }

        public static List<T> Clone<T>(this List<T> oldList)
        {
            return oldList.Select(it => it).ToList();
        }

        public static string Pretty<T>(this List<T> toPrint, string separator = "\n")
        {
            return string.Join(separator, toPrint.Select(it => $"[{it}]").ToList());
        }
    }
}