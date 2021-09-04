using System;
using BrandonUtils.Standalone.Chronic;
using BrandonUtils.Standalone.Exceptions;
using BrandonUtils.Standalone.Optional;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Runtime.Utils {
    /// <summary>
    /// Represents a value manipulated in 2 steps:
    /// <ul>
    /// <li>An <see cref="AddIncrements"/> step, where the <see cref="ExactIncrements"/> increases</li>
    /// <li>A <see cref="Reduce"/> step, which returns the <see cref="FullIncrements"/> and reduces the <see cref="ExactIncrements"/> down to <see cref="PartialIncrements"/></li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// This class is intended to handle extreme discrepancies in the frequency of incrementation vs. the amount incremented, i.e.:
    /// <ul>
    /// <li>One "full increment" takes ~1m, but we update the value every 1ms</li>
    /// <li>One "full increment" takes ~1ms, but we update the value every 10m</li>
    /// </ul>
    /// </remarks>
    /// <example>
    /// <ul>
    /// <li>We are constructing a tower</li>
    /// <li>Each builder can build 1/20 of a tower per second</li>
    /// <li>The number of available builders changes constantly</li>
    /// </ul>
    /// <code><![CDATA[
    /// Incrementer towerProgress = new Incrementer();
    /// double ratePerBuilder = 0.05;
    ///
    /// void Update(){
    ///     // step 1: increase the score                               // Example values
    ///     var rateThisFrame = availableBuilders * ratePerBuilder;     // 0.6 towers/sec           =   12 builders             *   0.05 towers/sec/builder
    ///     var progressThisFrame = rateThisFrame * Time.deltaTime;     // 0.3 towers               =   0.3 towers/sec          *   0.5 sec
    ///     towerProgress.Increment(progressThisFrame);                 // {ExactIncrements: 1.1}   =   {ExactIncrements: 0.8}  +   {progressThisFrame: 0.3}
    ///
    ///     // step 2: spawn an object for each finished tower,
    ///     // preserving any leftover "partial" towers                 // {ExactIncrements: 1.1,   FullIncrements: 1,  PartialIncrements: 0.1}
    ///     var finishedTowers = towerProgress.Reduce();                // {ExactIncrements: 0.1,   FullIncrements: 0,  PartialIncrements: 0.1}
    ///     spawnTowers(finishedTowers);                                // {finishedTowers: 1}
    /// }
    /// ]]></code>
    /// </example>
    [Serializable]
    public class Incrementer {
        /// <summary>
        /// The <b>exact</b> value of this incrementer, including "partial" increments.
        /// </summary>
        [JsonProperty]
        public double ExactIncrements { get; private set; }

        /// <summary>
        /// The integer component of <see cref="ExactIncrements"/>.
        /// </summary>
        /// <remarks>
        /// Derived from <see cref="ExactIncrements"/>, and therefore does not need to be serialized.
        /// </remarks>
        [JsonIgnore]
        public int FullIncrements => (int)Math.Floor(ExactIncrements);

        /// <summary>
        /// The decimal component of <see cref="ExactIncrements"/>.
        /// </summary>
        /// <remarks>
        /// Derived from <see cref="ExactIncrements"/>, and therefore does not need to be serialized.
        /// </remarks>
        [JsonIgnore]
        public double PartialIncrements => ExactIncrements - FullIncrements;

        private double _hertz;

        /// <summary>
        /// The number of <see cref="FullIncrements"/> per second.
        /// </summary>
        /// <remarks>
        /// Because the <see cref="Period"/> and <see cref="Hertz"/> can be derived from each other, only <b>one</b> of them needs to be serialized.
        /// We have chosen to serialize <see cref="Hertz"/> and <b>not</b> <see cref="Period"/>.
        /// <p/>
        /// <a href="https://en.wikipedia.org/wiki/Hertz">Hertz</a>
        /// </remarks>
        /// <exception cref="TimeParadoxException">if set to a negative value</exception>
        [JsonProperty]
        public double Hertz {
            get => _hertz;
            set {
                if (value < 0) {
                    throw new TimeParadoxException(
                        $"{nameof(Incrementer)}.{nameof(Hertz)} cannot be set to {value}, because we cannot increment backwards!");
                }

                _hertz = value;
            }
        }

        /// <summary>
        /// The <see cref="TimeSpan"/> duration of one increment.
        /// </summary>
        /// <remarks>
        /// Derived from the <see cref="Hertz"/>, and therefore <b>does not</b> need to be serialized.
        /// <br/>
        /// <see cref="set_Period"/> will <b>also</b> call <see cref="set_Hertz"/>.
        /// <p/>
        /// <a href="https://en.wikipedia.org/wiki/Period_(physics)">Period (physics)</a></remarks>
        /// <exception cref="TimeParadoxException">If set to a negative value</exception>
        [JsonIgnore]
        public virtual TimeSpan? Period {
            get => HertzToPeriod(Hertz);
            set {
                if (value < TimeSpan.Zero) {
                    throw new TimeParadoxException(
                        $"Cannot set the {nameof(Incrementer)}.{nameof(Period)} to the negative {nameof(TimeSpan)} {value}!");
                }

                Hertz = PeriodToHertz(value);
            }
        }

        /// <summary>
        /// Calculates the number of times <paramref name="period"/> occurs in 1 second.
        /// </summary>
        /// <remarks>
        /// If <paramref name="period"/> is <see cref="TimeSpan.Zero"/>, then 0 is returned.
        /// While it would be more mathematically correct to return <see cref="double.PositiveInfinity"/>, 0 was chosen
        /// for practicality.
        /// </remarks>
        /// <param name="period"></param>
        /// <returns>the number of times <paramref name="period"/> occurs in 1 second</returns>
        private double PeriodToHertz(TimeSpan? period) {
            if (period == null) {
                return 0;
            }

            return period == TimeSpan.Zero ? double.PositiveInfinity : TimeSpan.FromSeconds(1).Divide(period.Value);
        }

        /// <summary>
        /// Calculates the <see cref="TimeSpan"/> that would occur <paramref name="hertz"/> times in 1 second.
        /// </summary>
        /// <remarks>
        /// If <paramref name="hertz"/> is 0, then null is returned.
        /// </remarks>
        /// <param name="hertz"></param>
        /// <returns>the <see cref="TimeSpan"/> that would occur <paramref name="hertz"/> times in 1 second</returns>
        private TimeSpan? HertzToPeriod(double hertz) {
            if (hertz == 0) {
                return null;
            }

            return TimeSpan.FromSeconds(1).Divide(hertz);
        }

        /// <summary>
        /// Adds some amount to <see cref="ExactIncrements"/>, where "1" is the equivalent of <b>one "cycle"</b>.
        /// </summary>
        /// <remarks>
        /// This method should <b>never</b> reduce the value of <see cref="ExactIncrements"/>.
        /// <see cref="ExactIncrements"/> should only be reduced via <see cref="Reduce"/>.
        /// </remarks>
        /// <param name="amount">the value to add to <see cref="ExactIncrements"/>. Cannot be negative.</param>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="amount"/> is negative</exception>
        public void AddIncrements(double amount) {
            if (amount < 0) {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            ExactIncrements += amount;
        }

        /// <summary>
        /// Updates the value of <see cref="ExactIncrements"/> based on an elapsed <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// This method should <b>never</b> reduce the value of <see cref="ExactIncrements"/>.
        /// <see cref="ExactIncrements"/> should only be reduced via <see cref="Reduce"/>.
        /// </remarks>
        /// <param name="elapsedTime">the <see cref="TimeSpan"/> to add. Cannot be negative.</param>
        /// <exception cref="TimeParadoxException">if <paramref name="elapsedTime"/> is negative</exception>
        public void AddTime(TimeSpan elapsedTime) {
            if (elapsedTime < TimeSpan.Zero) {
                throw new TimeParadoxException(
                    $"Cannot add a negative amount of time ({elapsedTime}) to an {this.GetType().Name}!");
            }

            AddIncrements(ComputeElapsedIncrements(elapsedTime));
        }

        /**
         * <inheritdoc cref="AddTime"/>
         */
        public void AddSeconds(double seconds) {
            AddTime(TimeSpan.FromSeconds(seconds));
        }

        internal double ComputeElapsedIncrements(TimeSpan elapsedTime) {
            if (elapsedTime == TimeSpan.Zero) {
                return 0;
            }

            // ReSharper disable once PossibleInvalidOperationException
            return Period.OrElseThrow() == TimeSpan.Zero ? double.PositiveInfinity : elapsedTime.Divide(Period.Value);
        }

        [NotNull]
        private TimeParadoxException PeriodNullException(TimeSpan elapsedTime) {
            return new TimeParadoxException(
                $"Couldn't {nameof(ComputeElapsedIncrements)} for the [{elapsedTime.GetType().Name}]{elapsedTime} because the {nameof(Period)} is {Period}!");
        }

        /// <summary>
        /// Reduces <see cref="ExactIncrements"/> to only its decimal component (i.e. <see cref="PartialIncrements"/>),
        /// returning the <see cref="int"/> amount reduced (i.e. <see cref="FullIncrements"/>) in the process.
        /// </summary>
        /// <remarks>
        /// This method should be <b>only</b> way to lower the value of <see cref="ExactIncrements"/>.
        /// </remarks>
        /// <returns>the <see cref="int"/> amount that <see cref="ExactIncrements"/> was reduced by</returns>
        public int Reduce() {
            var fullIncrements = FullIncrements;
            var partialIncrements = PartialIncrements;

            ExactIncrements = partialIncrements;
            return fullIncrements;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}