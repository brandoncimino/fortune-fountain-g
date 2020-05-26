using System;

namespace Packages.BrandonUtils.Runtime.Saving {
    /// <summary>
    /// A special <see cref="SaveDataException{T}"/> thrown when a <see cref="SaveData{T}"/> is <see cref="SaveData{T}.Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string,bool)"/>-ed before <see cref="SaveData{T}.ReSaveDelay"/> has elapsed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReSaveDelayException<T> : SaveDataException<T> where T : SaveData<T>, new() {
        public ReSaveDelayException(SaveData<T> saveData) : base(saveData) { }
        public ReSaveDelayException(SaveData<T> saveData, string message) : base(saveData, message) { }
        public ReSaveDelayException(SaveData<T> saveData, Exception innerException) : base(saveData, innerException) { }
        public ReSaveDelayException(Exception innerException) : base(innerException) { }
        public ReSaveDelayException(string message, Exception innerException) : base(message, innerException) { }
        public ReSaveDelayException(string message) : base(message) { }
        public ReSaveDelayException(SaveData<T> saveData, string message, Exception innerException) : base(saveData, message, innerException) { }
    }
}