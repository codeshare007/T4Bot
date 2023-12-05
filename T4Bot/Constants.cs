using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4Bot
{
    internal class Constants
    {
        public const String FIRM = "CTS";
        public const String USERNAME = "akazmar2";
        public const String PASSWORD = "Trader12$";

        public const String MARKET = "XCME_EqOp EX1 (Z23P 460500)";
        public const int SELL_FIRST = 0;
        public const int SELL_SECOND = 4;  // 40 / 4 = 10
        public const int SELL_THIRD = 8;   // 80 / 4 = 20
        public const int SELL_FOURTH = 12; // 160 / 4 = 40 
        public const int SELL_FIFTH = 16; // 400 / 4 = 40 
        public const int PT2 = -2;   // 8 / 4 = 2
        public const int PT3 = -4;  // 12 / 4 = 3
        public const int PT4 = -6;  // 32 / 4 = 8
        public const int PT5 = -8;  // 32 / 4 = 8
        public const int VOLUME = 1;

    }
}
