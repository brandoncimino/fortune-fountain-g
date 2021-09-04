namespace Runtime.Valuables {
    public static class ValuableTypeExtensions {
        public static double FaceValue(this ValuableType valuableType) {
            return ValuableDatabase.Models[valuableType].ImmutableValue;
        }
    }
}