using System;
using Runtime.Saving;

namespace Runtime.Utils
{
    public static class Constants
    {
        /// <summary>
        /// Used as a pseudo-"null" value used to indicate that something has explicitly <b>never happened</b>, such as <see cref="Hand.lastGrabTime"/>.
        /// Use this to be more explicit than relying on the default initialized value of 0.
        /// <p/>
        /// <b>NOTE:</b>
        /// <see cref="DateTime"/> is nullable by default, so this wouldn't be necessary, but for reliability we are serializing <see cref="DateTime"/> fields's <see cref="DateTime.Ticks"/> values instead.
        /// We could save the <see cref="DateTime.Ticks"/> values as nullable longs, but that makes conversions between the accessible <see cref="DateTime"/> fields - e.g. <see cref="Hand.LastGrabTime"/> - and the backing long fields - e.g. <see cref="Hand.lastGrabTime"/> - a pain in the butt.
        /// </summary>
        public const long NeverTime = -1;
    }
}