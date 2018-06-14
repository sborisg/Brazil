using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoboCup.Entities.Players
{
    public class Attacker : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;
        Status m_status;

        public Attacker(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 40, 0);
            m_status = new Status();
        }

        //public override void play()
        //{

        //    // first ,ove to start position
        //    m_robot.Move(m_startPosition.X, m_startPosition.Y);

        //    SeenCoachObject ballObj;
        //    var angle = 15;
        //    while (!m_timeOver)
        //    {
        //        var bodyInfo = GetBodyInfo();
        //        angle = -angle;
        //        m_memory.waitForNewInfo();
        //        ballObj = m_coach.GetSeenCoachObject("ball");
        //        GetData();
        //        var myPlayer = m_status.MyPlayers[0];
        //        if (CheckQuerter(Quarter.LeftDown, ballObj))
        //            {
        //            if (myPlayer.DistanceTo(ballObj) > 1.5)
        //            {
        //                // If ball is too far then
        //                // turn to ball or 
        //                // if we have correct direction then go to ball
        //                var angleToBall = myPlayer.AngleToKick(ballObj);
        //                if (Math.Abs(angleToBall) > 0.5)
        //                {
        //                    m_robot.Turn(angleToBall);
        //                }
        //                else
        //                {
        //                    m_robot.Dash(100 * myPlayer.DistanceTo(ballObj));
        //                }
        //            }
        //            else
        //            {
        //                // We know where is ball and we can kick it
        //                // so look for goal
        //                if (m_side == 'l')
        //                    ballObj = m_coach.GetSeenCoachObject("goal r");
        //                else
        //                    ballObj = m_coach.GetSeenCoachObject("goal l");

        //                if (ballObj == null)
        //                {
        //                    m_robot.Turn(40);
        //                    m_memory.waitForNewInfo();
        //                }
        //                else
        //                { if (m_status.MyPlayers[0].DistanceTo(ballObj) > 24)
        //                        m_robot.Kick(20, m_status.MyPlayers[0].AngleToKick(ballObj) + angle);
        //                    else
        //                        m_robot.Kick(140, m_status.MyPlayers[0].AngleToKick(ballObj) + 10);
        //                }
        //            }


        //            // sleep one step to ensure that we will not send
        //            // two commands in one cycle.
        //            try
        //            {
        //                Thread.Sleep(2 * SoccerParams.simulator_step);
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }
        //    }
        //}

        private SenseBodyInfo GetBodyInfo()
        {
            m_robot.SenseBody();
            SenseBodyInfo bodyInfo = null;
            while (bodyInfo == null)
            {
                Thread.Sleep(WAIT_FOR_MSG_TIME);
                bodyInfo = m_memory.getBodyInfo();
            }

            return bodyInfo;
        }
        private void GetData()
        {
            m_status.MyPlayers = (from ob in m_coach.GetSeenCoachObjects()
                                  where ob.Key.Contains("player") && ob.Key.Contains("Brazil")
                                  select ob.Value).ToList();
            m_status.OpponentPlayers = (from ob in m_coach.GetSeenCoachObjects()
                                        where ob.Key.Contains("player") && !ob.Key.Contains("Brazil")
                                        select ob.Value).ToList();

        }

        public bool CheckQuerter(Quarter quarter,SeenCoachObject obj)
        {
             int XBOUNDRY = -20;
            bool result=false;
            switch(quarter)
            {
                case Quarter.LeftUp:
                    if ((obj.Pos.Value.X) <= XBOUNDRY && (obj.Pos.Value.X) >= -50 &&
                        (obj.Pos.Value.Y) <= 0 && (obj.Pos.Value.Y) >= -30)
                    {
                        result = true;
                    }
                    break;
                case Quarter.LeftDown:
                    if ((obj.Pos.Value.X) <= XBOUNDRY && (obj.Pos.Value.X) >= -50 &&
                        (obj.Pos.Value.Y) >= 0 && (obj.Pos.Value.Y) <= 30)
                    {
                        result = true;
                    }
                    break;
                case Quarter.RightUp:
                    if ((obj.Pos.Value.X) >= XBOUNDRY && (obj.Pos.Value.X) <= 50 &&
                        (obj.Pos.Value.Y) <= 0 && (obj.Pos.Value.Y) >= -30)
                    {
                        result = true;
                    }
                    break;
                case Quarter.RightDown:
                      if ((obj.Pos.Value.X) >= XBOUNDRY && (obj.Pos.Value.X) <= 50 &&
                          (obj.Pos.Value.Y) >= 0 && (obj.Pos.Value.Y) <= 30)
                        {
                            result = true;
                        }                                     
                 
                        break;
            }
            return result;
        }

        public enum Quarter
        {
            LeftUp,
            RightUp,
            LeftDown,
            RightDown
        }
    }
  

    class Status
    {
        public List<SeenCoachObject> MyPlayers { get; set; }

        public List<SeenCoachObject> OpponentPlayers { get; set; }
    }
}