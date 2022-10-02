using System;
using System.Runtime.Serialization;

namespace Fury
{
    [Serializable]
    public readonly struct Identity<T>
        : IEquatable<Identity<T>>,
        IComparable<Identity<T>>,
        ISerializable
        where T : class
    {
        readonly int _value;
        public Identity(int value) => _value = value;

        public static Identity<T> Null => new Identity<T>(0);
        public bool IsNull => _value == 0;

        public static bool operator ==(Identity<T> l, Identity<T> r)
        {
            return l._value == r._value;
        }

        public static bool operator !=(Identity<T> l, Identity<T> r)
        {
            return l._value != r._value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Identity<T> other)
            {
                return other._value == _value;
            }
            return false;
        }

        public bool Equals(Identity<T> other)
        {
            return other._value == _value;
        }

        public int CompareTo(Identity<T> other)
        {
            return _value - other._value;
        }        

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Identity<{typeof(T).Name}>({_value})";
        }

        public Identity(SerializationInfo info, StreamingContext context)
        {
            _value = info.GetInt32("value");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", _value);
        }
    }
}
