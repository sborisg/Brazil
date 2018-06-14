using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup.Entities
{
   public class OpponentTeam : Team
    {
        public OpponentTeam(string[] args)
        {
            m_teamName = "BadGuys";
            m_coach = new Coach();
            m_teamFormation = new OpponentFormation();
            m_playerList = m_teamFormation.InitTeam(this, m_coach);
        }
    }
}
