﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Runtime.Valuables;

namespace Tests.Runtime
{
    public class ValuableDatabaseTests
    {
        [Test]
        public void AllTypesPresent()
        {
            foreach (ValuableType valuableType in Enum.GetValues(typeof(ValuableType)))
            {
                Assert.That(VTypes(), Contains.Item(valuableType));
            }
        }

        [Test]
        public void NoTypeDuplicates()
        {
            Assert.That(VTypes(), Is.Unique);
        }

        private static IEnumerable<ValuableType> VTypes()
        {
            return ValuableDatabase.Models.Keys.Select(key => ValuableDatabase.Models[key].Type);
        }
    }
}