using System;
using System.Runtime.Serialization;

namespace Fury
{
    [Serializable]
    public readonly struct Identity<T>
        : IEquatable<Identity<T>>,
        IComparable<Identity<T>>
        where T : class
    {
        public readonly int Value;
        public Identity(int value) => Value = value;

        public static Identity<T> Null => new Identity<T>(0);
        public bool IsNull => Value == 0;

        public static bool operator ==(Identity<T> l, Identity<T> r)
        {
            return l.Value == r.Value;
        }

        public static bool operator !=(Identity<T> l, Identity<T> r)
        {
            return l.Value != r.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Identity<T> other)
            {
                return other.Value == Value;
            }
            return false;
        }

        public bool Equals(Identity<T> other)
        {
            return other.Value == Value;
        }

        public int CompareTo(Identity<T> other)
        {
            return Value - other.Value;
        }        

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Identity<{typeof(T).Name}>({Value})";
        }
    }
}
