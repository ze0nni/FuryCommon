using System;
using System.Runtime.Serialization;

namespace Fury
{
    [Serializable]
    public struct Ver<T> : ISerializable
    {
        int V;

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


        public Ver(SerializationInfo info, StreamingContext context)
        {
            V = info.GetInt32("V");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("V", V);
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

        public override int GetHashCode()
        {
            return 1133784017 + V.GetHashCode();
        }
    }
}