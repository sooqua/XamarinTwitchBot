// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Shared preferences manager
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using Android.App;
    using Android.Content;
    using Android.Util;

    using Common.Exceptions;

    internal static class SharedPreferences
    {
        public static void SaveObject(object obj, string prefskey, string filename)
        {
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                PutString(prefskey, Base64.EncodeToString(memoryStream.ToArray(), Base64Flags.Default), filename);
            }
        }

        public static object LoadObject(string prefskey, string filename)
        {
            using (var memoryStream = new MemoryStream(Base64.Decode(GetString(prefskey, filename), Base64Flags.Default)))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memoryStream);
            }
        }

        public static void Delete(string filename, FileCreationMode mode = FileCreationMode.Private)
        {
            if (!Application.Context.GetSharedPreferences(filename, mode).Edit().Clear().Commit())
            {
                throw new CommitingSharedPrefsException("Deleting shared preferences failed.");
            }
        }

        public static void PutString(string prefskey, string value, string filename, FileCreationMode mode = FileCreationMode.Private)
        {
            using (var sharedPreferences = Application.Context.GetSharedPreferences(filename, mode))
            using (var preferencesEditor = sharedPreferences.Edit())
            {
                preferencesEditor.PutString(prefskey, value);
                if (!preferencesEditor.Commit())
                {
                    throw new CommitingSharedPrefsException("Commiting shared preferences failed.");
                }
            }
        }

        public static string GetString(string prefskey, string filename, FileCreationMode mode = FileCreationMode.Private)
        {
            using (var sharedPreferences = Application.Context.GetSharedPreferences(filename, mode))
            {
                return sharedPreferences.GetString(prefskey, "");
            }
        }
    }
}