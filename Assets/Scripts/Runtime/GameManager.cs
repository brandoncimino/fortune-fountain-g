﻿using Runtime.Saving;
using UnityEngine;

namespace Runtime {
    /// <summary>
    /// This class is going to be my "brute-force", "old-fashioned" way of just <i>getting shit done</i>.
    ///
    /// This manager will handle basically everything until those things are given a more appropriate home, in order to prevent me from being paralyzed and unable to decide where things should go.
    /// </summary>
    public class GameManager : MonoBehaviour {
        /// <summary>
        /// The name of the current save file to use.
        /// </summary>
        /// <remarks>
        /// Ideally, this would be marked as <c>readonly</c>, but <see cref="SystemInfo.deviceName"/> cannot be used inside of a static initializer and must instead be used inside of <see cref="Awake"/> or <see cref="Start"/>.
        /// </remarks>
        private static string SaveFileName;

        /// <summary>
        /// The <see cref="FortuneFountainSaveData"/> that will be used by the game.
        ///
        /// See <see cref="FortuneFountainSaveData.Load"/> for full details on the behavior.
        ///
        /// As of the implementation here on 8/5/2020, the expected behavior is:
        /// <li> The most recent save file named <see cref="SaveFileName"/> is loaded when the game starts</li>
        /// <li> If no save file named <see cref="SaveFileName"/> exists, one is created</li>
        /// </summary>
        /// <remarks>
        /// This would ideally be <c>readonly</c>, but is limited by <see cref="SaveFileName"/>.
        /// </remarks>
        public static FortuneFountainSaveData SaveData;

        private void Awake() {
            LoadFortuneFountain();
        }

        // Start is called before the first frame update
        void Start() { }

        // Update is called once per frame
        void Update() { }

        /// <summary>
        /// "Starts" the game, doing things such as loading the appropriate save file.
        /// </summary>
        public void LoadFortuneFountain() {
            SaveFileName = GetAppropriateSaveFileName();
            SaveData = FortuneFountainSaveData.Load(SaveFileName);
        }

        /// <summary>
        /// Returns the appropriate name for this user's save file.
        ///
        /// The plan is to have one save file per system, and not have users manage it in any particular way (except maybe to clear their data), so this can be initialized here and then treated as read-only.
        ///
        /// TODO: This is currently the same as <see cref="SystemInfo.deviceName"/>, but in the future will likely include some logic based on Google Play identifiers (whatever those are).
        /// </summary>
        /// <returns>The appropriate name for this user's save file.</returns>
        private string GetAppropriateSaveFileName() {
            return SystemInfo.deviceName;
        }
    }
}