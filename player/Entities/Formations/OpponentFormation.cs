using player.Entities.Players;
using RoboCup.Entities;
using RoboCup.Entities.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup
{
    public class OpponentFormation : IFormation
    {
        public List<Player> InitTeam(Team team, ICoach coach)
        {
            var players = new List<Player>();
         //   players.Add(new AttackerExample2(team, coach));
 //           players.Add(new DefenderBottom(team, coach));
 //           players.Add(new DefenderTop(team, coach));
 //           players.Add(new Goalkeeper(team, coach));
 //           players.Add(new LeftAttacker(team, coach));
            players.Add(new AttackerExample2(team, coach));
            players.Add(new AttackerExample2(team, coach));

            team.m_teamName = "BadGuys";
            return players;
        }
    }
}
