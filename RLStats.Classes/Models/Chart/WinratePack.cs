
using System;

namespace RLStats_Classes.Models.Chart
{
    public class WinratePack : IComparable
    {
        public string Name { get; set; }
        public string WinrateString => $"{Math.Round(Winrate * 100, MidpointRounding.ToZero)}%";
        public double Winrate => Won / Played;
        public double Won { get; set; }
        public double Played { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is WinratePack mw)
            {
                if (Winrate == mw.Winrate)
                    return 0;
                if (Winrate > mw.Winrate)
                    return 1;
                else
                    return -1;
            }
            else
            {
                throw new Exception("cant compare");
            }
        }
    }
}
