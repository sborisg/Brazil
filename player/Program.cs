using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using TinMan;
using RoboCup.Entities;

namespace RoboCup
{
    class Program
    {
       static  void Main(string[] args)
        {
            var team1 = new Team(args);
          //  var opponentTeam = new OpponentTeam(args);
            
            
            Console.ReadKey();
        }

    }
}
