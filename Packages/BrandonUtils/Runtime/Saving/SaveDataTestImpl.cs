using System;

namespace Packages.BrandonUtils.Runtime.Saving {
    /// <summary>
    ///     An example implementation of <see cref="SaveData{T}"/>.
    /// </summary>
    /// <remarks>
    ///     Intended only for use by unit tests.
    /// </remarks>
    [Serializable]
    public class SaveDataTestImpl : SaveData<SaveDataTestImpl> {
        public string Word;
    }
}