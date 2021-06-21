using System;

namespace LionLibrary.Utils
{
    public static class RandomThreadSafe
    {
        private static readonly Random _global = new();

        [ThreadStatic]
        private static Random? _local;

        public static int Next()
        {
            EnsureThreadStaticRandomCreated();
            return _local!.Next();
        }

        public static int Next(int maxValue)
        {
            EnsureThreadStaticRandomCreated();
            return _local!.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            EnsureThreadStaticRandomCreated();
            return _local!.Next(minValue, maxValue);
        }

        private static void EnsureThreadStaticRandomCreated()
        {
            if (_local == null)
            {
                lock (_global)
                {
                    if (_local == null)
                    {
                        int seed = _global.Next();
                        _local = new Random(seed);
                    }
                }
            }
        }
    }
}
