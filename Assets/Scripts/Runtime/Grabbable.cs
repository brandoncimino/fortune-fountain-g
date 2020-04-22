using System;
// ReSharper disable CheckNamespace

namespace DefaultNamespace
{
    /// <summary>
    /// A class containing the information of a type of item able to be grabbed (pennies, nickles, etc.)
    /// </summary>
    public class Grabbable
    {
        private Grabbable()
        {
        }
        public Grabbable(string name, int basePrice, int maxStack, long grabInterval)
        {
            Name = name;
            this.basePrice = basePrice;
            this.maxStack = maxStack;
            this.grabInterval = grabInterval;
        }
        
        private string Name = "null";
        public int basePrice = 0;
        public int maxStack = Int32.MaxValue;
        public long grabInterval;
    }

    public class GrabbedItems
    {
        private Grabbable Item;
        public int value;

        public GrabbedItems(Grabbable grabbable, int value)
        {
            Item = grabbable;
            this.value = value;
        }
    }
}