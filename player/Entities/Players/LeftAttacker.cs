using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoboCup.Entities.Players
{
    public class LeftAttacker : Attacker
    {
        public LeftAttacker(Team team, ICoach coach) : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 40, m_sideFactor * 10);

            m_status = new Status();
        }
        private const int WAIT_FOR_MSG_TIME = 10;
        Status m_status;


        public override void play()
        {

            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);
            m_memory.waitForNewInfo();

            SeenCoachObject ballObj;
            var angle = 5;
            while (!m_timeOver)
            {
                var bodyInfo = GetBodyInfo();
                angle = -angle;
                ballObj = m_coach.GetSeenCoachObject("ball");
               // GetData();
                var myPlayer = m_coach.GetSeenCoachObject($"player Brazil {m_number}");
                Quarter quarter = m_side == 'l' ? Quarter.RightUp : Quarter.LeftDown;
                if (CheckQuerter(quarter, ballObj))
                {
                    var distance = myPlayer.DistanceTo(ballObj);
                    if (distance > 1.5)
                    {
                        // If ball is too far then
                        // turn to ball or 
                        // if we have correct direction then go to ball
                        var angleToBall = myPlayer.AngleToKick(ballObj);
                        if (Math.Abs(angleToBall) > 0.5)
                        {
                            m_robot.Turn(angleToBall);
                        }
                        else
                        {
                            m_robot.Dash(150);
                        }
                    }
                    else
                    {
                        // We know where is ball and we can kick it
                        // so look for goal
                        if (m_side == 'l')
                            ballObj = m_coach.GetSeenCoachObject("goal r");
                        else
                            ballObj = m_coach.GetSeenCoachObject("goal l");

                        var atk = myPlayer.AngleToKick(ballObj);
                        if (atk > 120)
                        {
                            m_robot.Kick(5, 40);
                        }

                        else if (myPlayer.DistanceTo(ballObj) > 24)
                            m_robot.Kick(40, myPlayer.AngleToKick(ballObj) + angle);
                        else
                            m_robot.Kick(140, myPlayer.AngleToKick(ballObj) + 10);

                    }


                }
                else
                {
                    Quarter quarter2 = m_side == 'l' ? Quarter.RightDown : Quarter.LeftUp;
                    if (CheckQuerter(quarter2, ballObj))
                    {
                        var angleToBall = myPlayer.AngleToBallX(ballObj.Pos.Value.X, myPlayer.Pos.Value.Y);
                        if (Math.Abs(angleToBall) > 0.5)
                        {
                            m_robot.Turn(angleToBall);
                        }
                        else
                        {
                            m_robot.Dash(50);
                        }
                    }
                }

                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                try
                {
                    Thread.Sleep(2 * SoccerParams.simulator_step);
                }
                catch (Exception e)
                {

                }

            }
        }

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
    }


}
