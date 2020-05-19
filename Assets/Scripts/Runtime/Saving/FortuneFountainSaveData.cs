using Packages.BrandonUtils.Runtime.Saving;

namespace Runtime.Saving
{
    /// <inheritdoc />
    public class FortuneFountainSaveData : SaveData<FortuneFountainSaveData>
    {
        public readonly Hand Hand;
        public double Karma;

        public FortuneFountainSaveData()
        {
            Hand = new Hand(this);
        }

        public void AddKarma(double amount)
        {
            Karma += amount;
        }
    }
}