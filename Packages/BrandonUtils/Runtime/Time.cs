using System;

namespace Packages.BrandonUtils.Runtime {
    public static class Time {
        /// <summary>
        /// Mimics .NET Core's <a href="https://docs.microsoft.com/en-us/dotnet/api/system.timespan.divide">TimeSpan.Divide</a>.
        ///
        /// Does this by converting the given <see cref="TimeSpan"/>s into ticks and performing the division on those.
        /// </summary>
        /// <returns></returns>
        public static double Divide(this TimeSpan dividend, TimeSpan divisor) {
            //cast of dividend.Ticks to (double) is to prevent loss of fractions
            return (double) dividend.Ticks / divisor.Ticks;
        }

        public static double Modulus(this TimeSpan dividend, TimeSpan divisor) {
            return Divide(dividend, divisor) % 1;
        }

        public static double Quotient(this TimeSpan dividend, TimeSpan divisor) {
            return Math.Floor(Divide(dividend, divisor));
        }

        public static TimeSpan ModSpan(this TimeSpan dividend, TimeSpan divisor) {
            return TimeSpan.FromTicks(dividend.Ticks % divisor.Ticks);
        }

        public static TimeSpan DivSpan(this TimeSpan dividend, TimeSpan divisor) {
            return TimeSpan.FromTicks(dividend.Ticks / divisor.Ticks);
        }
    }
}