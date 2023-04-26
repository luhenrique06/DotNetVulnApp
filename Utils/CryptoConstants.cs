using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brokenaccesscontrol.Utils
{
    public class CryptoConstants
    {
        public static readonly byte[] key = System.Convert.FromBase64String("7Bt8uHdHmSnq4Zq71xJ7uw==");
        public static readonly byte[] IV = System.Convert.FromBase64String("N2x93aNb5JDPafmTAuiv/w==");
        public static readonly byte[] DesKey = System.Text.Encoding.UTF8.GetBytes("password"); 
    }
}