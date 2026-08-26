using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerServer
{
    public class MainClass
    {
        public static void Main()
        {
            if (!DbManager.Connect("Game", "127.0.0.1", 3306, "root", "apple"))
                return;
            NetManager.Connect("127.0.0.1", 8888);
        }        
    }
}