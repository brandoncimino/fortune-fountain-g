namespace Packages.BrandonUtils.Runtime
{
    /// <summary>
    /// A class for handling noun conjugations consistently.
    /// </summary>
    public class Noun
    {
        public string Singular;
        public string Plural;

        public Noun(string singular, string plural=null)
        {
            this.Singular = singular;
            this.Plural = plural == null ? Singular + "s" : plural;
        }

        public string Get(int quantity = 1)
        {
            return quantity == 1 ? Singular : Plural;
        }
    }
}