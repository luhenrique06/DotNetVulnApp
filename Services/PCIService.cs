using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using brokenaccesscontrol.Utils;

namespace brokenaccesscontrol.Services
{
    public class PCIService
    {
       private static FileLog logger = new FileLog("valid.log"); 

       public bool validate(String pan){
        int sum = 0;
        for (int i = pan.Length -1; i >=0; i--){
            int n = (int)Char.GetNumericValue(pan[i]);
            if(i % 2 ==0){
                n *=2;
                if(n >= 10){
                    n = n % 10 + 1;
                }
            }
            sum += n;
        }
        logger.Log(pan);
        return true;
       }
    }
}