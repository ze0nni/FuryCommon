using Newtonsoft.Json;
using System;

namespace Fury
{
    [Serializable]
    [JsonConverter(typeof(VerJsonConverter))]
    public struct Ver<T>
        : IEquatable<Ver<T>>,
        IComparable<Ver<T>>
    {
        public int V;

        public Ver(int v) => V = v;

        public static Ver<T> operator ++(Ver<T> v)
        {
            v.V++;
            return v;
        }

        public static bool operator ==(Ver<T> l, Ver<T> r)
        {
            return l.V == r.V;
        }

        public static bool operator !=(Ver<T> l, Ver<T> r)
        {
            return l.V != r.V;
        }

        public static bool operator >(Ver<T> l, Ver<T> r)
        {
            return l.V > r.V;
        }

        public static bool operator <(Ver<T> l, Ver<T> r)
        {
            return l.V < r.V;
        }

        public static bool operator >=(Ver<T> l, Ver<T> r)
        {
            return l.V >= r.V;
        }

        public static bool operator <=(Ver<T> l, Ver<T> r)
        {
            return l.V <= r.V;
        }

        public override string ToString()
        {
            return $"Ver<{typeof(T).Name}>({V})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Ver<T>))
            {
                return false;
            }

            var version = (Ver<T>)obj;
            return V == version.V;
        }

        public bool Equals(Ver<T> other)
        {
            return this.V == other.V;
        }

        public int CompareTo(Ver<T> other)
        {
            return this.V.CompareTo(other.V);
        }

        public override int GetHashCode()
        {
            return 1133784017 + V.GetHashCode();
        }
    }
}