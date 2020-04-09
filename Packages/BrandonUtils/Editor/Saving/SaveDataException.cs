using System;
using BrandonUtils.Exceptions;

namespace BrandonUtils.Saving
{
    public class SaveDataException<T> : BrandonException where T : SaveData<T>, new()
    {
        private SaveData<T> saveData;

        public SaveDataException(SaveData<T> saveData) : base(GetMessage(saveData))
        {
            this.saveData = saveData;
        }

        public SaveDataException(SaveData<T> saveData, string message) : base(GetMessage(saveData, message))
        {
            this.saveData = saveData;
        }

        public SaveDataException(SaveData<T> saveData, Exception innerException) : base(GetMessage(saveData),
            innerException)
        {
            this.saveData = saveData;
        }

        public SaveDataException(Exception innerException) : base(GetMessage(), innerException)
        {
        }

        public SaveDataException(string message, Exception innerException) : base(GetMessage(message: message),
            innerException)
        {
        }

        public SaveDataException(string message) : base(GetMessage(message: message))
        {
        }

        public SaveDataException(SaveData<T> saveData, string message, Exception innerException) : base(
            GetMessage(saveData, message), innerException)
        {
            this.saveData = saveData;
        }

        private static string GetMessage(SaveData<T> saveData = null,
            string message = "Something went wrong with save data management!")
        {
            if (saveData != null)
            {
                message += $"\nnickName: {saveData.nickName}\nData:\n{saveData}";
            }

            return message;
        }
    }
}