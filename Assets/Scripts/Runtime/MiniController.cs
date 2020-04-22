using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
// ReSharper disable CheckNamespace

namespace DefaultNamespace
{
    public class MiniController : MonoBehaviour
    {
        public Text Hand;
        public Text Well;
        public Text Penny;

        public Text PennyPercent;
        
        private double _wellNum = 0;
        
        //List of Item Types that can be grabbed
        List<Grabbable> ItemsToGrab = new List<Grabbable>();
        //List of current Items in the Hand
        List<GrabbedItems> ItemsInHand = new List<GrabbedItems>();
        //List of the numbers of each Item currently in the Hand
        List<int> GrabbedItemCount = new List<int>();
        //List of time elapsed towards Grabbing the new Item
        List<long> ItemGrabProgress = new List<long>();
        void Start()
        {
            // TODO: The creation of the Grabbable Items should definitely be moved to the class
            // TODO: These three Lists should always have the same number of items, and each position within the list is referring to the same Item. This should probably be converted into one singular List. 
            // Adding Pennies
            ItemsToGrab.Add(new Grabbable("Penny", 1, 5, TimeSpan.TicksPerSecond));
            GrabbedItemCount.Add(0);
            ItemGrabProgress.Add(0);
            
            // Setting Defaults to all text fields
            RefreshAll();
        }

        void Update()
        {
            //Debug.Log((Time.deltaTime * TimeSpan.TicksPerSecond) + "f converts to " + (Convert.ToInt32(Time.deltaTime * TimeSpan.TicksPerSecond)) + "L");
            //In the future, we may need to send something other than deltaTime
            GrabItemCheck(Convert.ToInt64(Time.deltaTime * TimeSpan.TicksPerSecond));
        }

        private void GrabItemCheck(long changeInTime)
        {
            for (int i = 0; i < ItemsToGrab.Count; i++)
            {
                if(GrabbedItemCount[i] >= ItemsToGrab[i].maxStack)
                    continue;
                //Debug.Log("Number of Grabs: Checking if " + ItemsToGrab[i].grabInterval + " plus " + changeInTime + "is >= 1");
                var itemsToAdd = IncrementalUtils.NumberOfGrabs(ItemsToGrab[i].grabInterval, changeInTime + ItemGrabProgress[i]);
                //Debug.Log(itemsToAdd > 0);
                if (itemsToAdd > 0)
                {
                    for (int j = 0; j < itemsToAdd; j++)
                    {
                        //TODO: Currently looks at base price. Needs to look at the modified value of the item (after Store and Upgrades)
                        ItemsInHand.Add(new GrabbedItems(ItemsToGrab[i], ItemsToGrab[i].basePrice));
                        GrabbedItemCount[i]++;
                        if (GrabbedItemCount[i] >= ItemsToGrab[i].maxStack)
                            break;
                    }
                    RefreshAll();
                }
                //Debug.Log("Progress of Grab: Adding " + changeInTime + " to " + ItemGrabProgress[i] + " getting " + (changeInTime + ItemGrabProgress[i]));

                ItemGrabProgress[i] = IncrementalUtils.ProgressOfGrab(ItemsToGrab[i].grabInterval, changeInTime + ItemGrabProgress[i]);
                //Debug.Log("Progress of Grab: Got " + ItemGrabProgress[i]);
            }
            RefreshPennyPercent(ItemGrabProgress[0]);
        }

        /// <summary>
        /// Testing variable: Should progression towards the next Grab be discarded when Hand is Thrown?
        /// </summary>
        public enum RolloverResult
        {
            /// <summary>
            /// Progression should be retained. Allows long-term items to be grabbed earlier than anticipated. Causes Rapid-Hand-Throwing to become optimal
            /// </summary>
            Retain,
            /// <summary>
            /// Progression should be discarded. May cause player anger if a large amount of a long-term item's progression is wasted. Identical to OG Fortune Fountain
            /// </summary>
            Discard,
            /// <summary>
            /// Untested Theory: Progression should be reduced, but not to zero. May become possible upgrade
            /// </summary>
            Halved
        }

        public void YeetDiscard()
        {
            YeetItems(RolloverResult.Discard);
        }
        public void YeetItems(RolloverResult rolloverResult)
        {
            _wellNum += ObtainHandSumValue();
            ItemsInHand.Clear();
            GrabbedItemCount = GrabbedItemCount.Select(c => 0).ToList();
            if (rolloverResult == RolloverResult.Discard)
                ItemGrabProgress[0] = 0;
            else if (rolloverResult == RolloverResult.Retain)
            {
                //Intentionally left blank; Retain does not modify existing progress
            }
            else if (rolloverResult == RolloverResult.Halved)
            {
                ItemGrabProgress[0] /= 2;
            }
            else
            {
                throw new NotImplementedException("Unreachable code reached: pls gib me rulovr ahpshon");;
            }
            RefreshAll();
        }
        
        /*
        [Obsolete("GrabItem is deprecated, please use GrabItemCheck instead")]
        private void GrabItem()
        {
            if (Math.Abs(_handNum - HandMax) < .0000001)
                return;
            _handNum = Math.Min(_handNum + Time.deltaTime, HandMax);
            RefreshText(Hand, _handNum);
        }

        public void CelebratoryHandToss()
        {
            _wellNum += _handNum;
            _handNum = 0;
            RefreshAll();
        }
        */

        
        private void RefreshAll()
        {
            RefreshAllItems();
            RefreshAllContainers();
        }

        private void RefreshAllItems()
        {
            //TODO: Add additional calls for each new Item added
            RefreshText(Penny, GrabbedItemCount[0]);
        }

        private void RefreshAllContainers()
        {
            RefreshHand();
            RefreshText(Well, _wellNum);
        }

        private static void RefreshText(Text textField, double newVal)
        {
            textField.text = newVal.ToString(CultureInfo.InvariantCulture);
        }

        private void RefreshHand()
        {
            RefreshText(Hand, ObtainHandSumValue());
        }

        private int ObtainHandSumValue()
        {
            var sumValue = ItemsInHand.Sum(grabbedItems => grabbedItems.value);
            return sumValue;
        }

        //[debug]
        private void RefreshPennyPercent(float newPercent)
        {
            RefreshText(PennyPercent, newPercent);
        }
    }
}