using System;
using UnityEngine;

namespace Fury
{
    public static class Assets
    {
        public static void RequestEquatable<T>(string name, Type context)
        {
            if (!typeof(IEquatable<T>).IsAssignableFrom(typeof(T)))
            {
                Debug.LogWarning($"[{context.Name}] generic argument {name}={typeof(T).Name} not implements IEquatable<{typeof(T).Name}> will become a source extra memory allocations");
            }
        }
    }
}