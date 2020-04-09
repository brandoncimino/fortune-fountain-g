using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Packages.BrandonUtils.Editor
{
    public static class TestUtils
    {
        public static void AreEqual<T>(IList<T> expectedList, IList<T> actualList)
        {
            Assert.AreEqual(expectedList.Count, actualList.Count, "The lists weren't the same length!");
            for (int i = 0; i < expectedList.Count; i++)
            {
                Debug.Log($"Comparing {expectedList[i]} == {actualList[i]}");
                Assert.AreEqual(expectedList[i], actualList[i], $"The lists differ at index [{i}]!");
            }
        }
    }
}