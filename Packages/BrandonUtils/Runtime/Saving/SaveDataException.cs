using System;
using System.Collections.Generic;
using Packages.BrandonUtils.Runtime.Collections;
using Packages.BrandonUtils.Runtime.Exceptions;

namespace Packages.BrandonUtils.Runtime.Saving {
    /// <summary>
    /// A special <see cref="BrandonException"/> for exceptions caused by <see cref="SaveData{t}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SaveDataException<T> : BrandonException where T : SaveData<T>, new() {
        private SaveData<T> saveData;

        public SaveDataException(SaveData<T> saveData) : base(GetMessage(saveData)) {
            this.saveData = saveData;
        }

        public SaveDataException(SaveData<T> saveData, string message) : base(GetMessage(saveData, message)) {
            this.saveData = saveData;
        }

        public SaveDataException(SaveData<T> saveData, Exception innerException) : base(
            GetMessage(saveData),
            innerException
        ) {
            this.saveData = saveData;
        }

        public SaveDataException(Exception innerException) : base(GetMessage(), innerException) { }

        public SaveDataException(string message, Exception innerException) : base(
            GetMessage(message: message),
            innerException
        ) { }

        public SaveDataException(string message) : base(GetMessage(message: message)) { }

        public SaveDataException(SaveData<T> saveData, string message, Exception innerException) : base(
            GetMessage(saveData, message), innerException
        ) {
            this.saveData = saveData;
        }

        private static string GetMessage(
            SaveData<T> saveData = null,
            string message = "Something went wrong with save data management!"
        ) {
            var lines = new List<string> {
                $"Type: {typeof(T).Name} ({typeof(T)})",
                message
            };

            if (saveData != null) {
                lines.Add($"{nameof(saveData.nickName)}: {saveData.nickName}");
                lines.Add($"{typeof(T)}:\n{saveData}");
            }

            return lines.JoinString("\n");
        }
    }
}