using System;
using System.Collections.Generic;
using Packages.BrandonUtils.Runtime.Exceptions;
using Packages.BrandonUtils.Runtime.Logging;
using UnityEngine;

namespace Packages.BrandonUtils.Runtime.Time {
    /// <summary>
    /// Utilities that interact with Unity's <see cref="Time"/> system.
    /// </summary>
    public static class RealTime {
        public static readonly Dictionary<RuntimeInitializeLoadType, DateTime> LoadTime = new Dictionary<RuntimeInitializeLoadType, DateTime>();

        /// <summary>
        /// The difference between the game starting and the first trigger of <see cref="SetLoadTime_AfterSceneLoad"/>.
        /// </summary>
        /// <remarks>
        /// <li>Used to help calculate <see cref="Now"/>.</li>
        /// <li>Stored as a <c>float</c> as it is a direct record of Unity <see cref="Time"/> values (which are all <c>float</c>s).</li>
        /// </remarks>
        public static float InitialLoadDuration;

        /// <summary>
        /// The <see cref="Time.unscaledTime"/> that has elapsed since the game <b>first finished loading</b>.
        /// </summary>
        /// <remarks>
        /// By default, Unity's <see cref="Time.unscaledTime"/> (and <see cref="Time.realtimeSinceStartup"/>) return the elapsed time <b>since the application started</b>.
        /// <p/>
        /// Meanwhile, Unity's <see cref="Time.time"/>, etc. return the elapsed time <b>since the first scene was loaded</b>.
        /// <p/>
        /// As a result, <see cref="Time.time"/> and <see cref="Time.unscaledTime"/> return <b>different amounts</b>, even when the <see cref="Time.timeScale"/> has never changed, resulting in <see cref="Time.unscaledTime"/> always having a <b>larger value</b> than <see cref="Time.time"/>.
        /// <p/>
        /// This "extra time" added to <see cref="Time.unscaledTime"/> is the time taken for <b>the first level to load</b>, which is stored in <see cref="InitialLoadDuration"/>.
        /// </remarks>
        public static float UnscaledTimeSinceInitialLoad => UnityEngine.Time.unscaledTime - InitialLoadDuration;

        /// <summary>
        /// The <see cref="DateTime"/> at the <b>start of this frame</b>.
        /// </summary>
        /// <remarks>
        ///    <li>Independent of <see cref="Time.timeScale"/>.</li>
        ///    <li>Will be the same <b>throughout the current frame</b>.</li>
        /// </remarks>
        public static DateTime Now => LoadTime[RuntimeInitializeLoadType.AfterSceneLoad] + TimeSpan.FromSeconds(UnscaledTimeSinceInitialLoad);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetRealtime_AfterAssembliesLoaded() {
            SetLoadTime(RuntimeInitializeLoadType.AfterAssembliesLoaded);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SetLoadTime_AfterSceneLoad() {
            SetLoadTime(RuntimeInitializeLoadType.AfterSceneLoad);

            //Set the InitialLoadDuration
            InitialLoadDuration = UnityEngine.Time.realtimeSinceStartup;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void SetLoadTime_BeforeSplashScreen() {
            SetLoadTime(RuntimeInitializeLoadType.BeforeSplashScreen);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetLoadTime_SubsystemRegistration() {
            SetLoadTime(RuntimeInitializeLoadType.SubsystemRegistration);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetLoadTime_BeforeSceneLoad() {
            SetLoadTime(RuntimeInitializeLoadType.BeforeSceneLoad);
        }

        private static void SetLoadTime(RuntimeInitializeLoadType loadType) {
            if (!LoadTime.ContainsKey(loadType)) {
                LoadTime.Add(loadType, DateTime.Now);
                LogUtils.Log($"Set {loadType} time to {LoadTime[loadType]}");
            }
            else {
                throw new TimeParadoxException($"Attempting to set the time for {loadType} twice!!");
            }
        }

        /// <summary>
        /// Prints the core values from <see cref="Time"/> for debugging purposes.
        /// </summary>
        public static void PrintUnityTimes() {
            LogUtils.Log(
                $"Overall {nameof(UnityEngine.Time)} values:",
                $"{nameof(UnityEngine.Time.time)}                 = {UnityEngine.Time.time}",
                $"{nameof(UnityEngine.Time.fixedTime)}            = {UnityEngine.Time.fixedTime}",
                $"{nameof(UnityEngine.Time.unscaledTime)}         = {UnityEngine.Time.unscaledTime}",
                $"{nameof(UnityEngine.Time.fixedUnscaledTime)}    = {UnityEngine.Time.fixedUnscaledTime}",
                $"{nameof(UnityEngine.Time.realtimeSinceStartup)} = {UnityEngine.Time.realtimeSinceStartup}",
                $"{nameof(UnityEngine.Time.timeSinceLevelLoad)}   = {UnityEngine.Time.timeSinceLevelLoad}",
                $"",
                $"Single-frame {nameof(UnityEngine.Time)} values:",
                $"{nameof(UnityEngine.Time.deltaTime)}              = {UnityEngine.Time.deltaTime}",
                $"{nameof(UnityEngine.Time.fixedDeltaTime)}         = {UnityEngine.Time.fixedDeltaTime}",
                $"{nameof(UnityEngine.Time.unscaledDeltaTime)}      = {UnityEngine.Time.unscaledDeltaTime}",
                $"{nameof(UnityEngine.Time.fixedUnscaledDeltaTime)} = {UnityEngine.Time.fixedUnscaledDeltaTime}"
            );
        }
    }
}