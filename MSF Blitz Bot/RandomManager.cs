using System;

namespace MSFBlitzBot
{
    public static class RandomManager
    {
        private static readonly Random _rand = new((int)DateTime.Now.Ticks);

        public static int Get(int min, int max)
        {
            return max >= min ? _rand.Next(min, max + 1) : _rand.Next(max, min + 1);
        }

        public static int Get(int max)
        {
            return max >= 0 ? _rand.Next(max + 1) : 0;
        }
    }
}
