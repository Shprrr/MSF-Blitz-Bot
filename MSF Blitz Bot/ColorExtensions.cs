using System;
using System.Drawing;

namespace MSFBlitzBot
{
    public static class ColorExtensions
    {
        public static bool IsCloseTo(this Color c1, Color c2, byte diff = 10)
        {
            if (Math.Abs(c1.R - c2.R) <= diff && Math.Abs(c1.G - c2.G) <= diff)
            {
                return Math.Abs(c1.B - c2.B) <= diff;
            }
            return false;
        }

        public static bool IsBetween(this Color c, Color cA, Color cB)
        {
            if ((c.R >= cA.R || c.R >= cB.R) && (c.R <= cA.R || c.R <= cB.R) && (c.G >= cA.G || c.G >= cB.G) && (c.G <= cA.G || c.G <= cB.G) && (c.B >= cA.B || c.B >= cB.B))
            {
                if (c.B > cA.B)
                {
                    return c.B <= cB.B;
                }
                return true;
            }
            return false;
        }
    }
}
