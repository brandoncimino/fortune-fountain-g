using UnityEngine;

namespace Runtime.Valuables
{
    public class Throwable
    {
        [SerializeField] public ValuableType ValuableType;

        [SerializeField] public double ThrowValue;

        public Throwable(ValuableType valuableType, double throwValue)
        {
            ValuableType = valuableType;
            ThrowValue = throwValue;
        }
    }
}