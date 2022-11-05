using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Fury
{
    public static class Storage
    {
        public static readonly MemoryStream _ms = new MemoryStream();
        public static T Load<T>() where T : class, new()
        {
            if (TryLoad<T>(out var data))
            {
                return data;
            }
            return new T();
        }

        public static bool TryLoad<T>(out T data) where T : class
        {
            if (!Exists<T>())
            {
                data = default;
                return false;
            }

            try
            {
                var raw = PlayerPrefs.GetString(typeof(T).FullName);
                _ms.Position = 0;
                _ms.Write(Convert.FromBase64String(raw));
                _ms.Position = 0;
                var bf = new BinaryFormatter();
                data = (T)bf.Deserialize(_ms);
                return true;
            }
            catch
            {
                data = default;
                return false;
            }
        }

        public static void Save<T>(T state) where T : class
        {
            var bf = new BinaryFormatter();
            _ms.Position = 0;
            bf.Serialize(_ms, state);
            var size = (int)_ms.Position;
            var raw = Convert.ToBase64String(_ms.ToArray(), 0, size);
            PlayerPrefs.SetString(typeof(T).FullName, raw);
        }

        public static void Drop<T>()
        {
            PlayerPrefs.DeleteKey(typeof(T).FullName);
        }

        public static bool Exists<T>()
        {
            return PlayerPrefs.HasKey(typeof(T).FullName);
        }
    }
}
