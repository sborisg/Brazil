using RoboCup.Entities;
using RoboCup.Entities.Players;
using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoboCup
{
    public class AttackerExample2 : Attacker
    {
        private const int WAIT_FOR_MSG_TIME = 10;
      
        public AttackerExample2(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 10, 0);
        }

        public override void play()
        {
            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);

            while (!m_timeOver)
            {
                Quarter quarter = m_side == 'l' ? Quarter.RightDown : Quarter.LeftUp;
                Quarter q2 = m_side == 'l' ? Quarter.RightUp : Quarter.LeftDown;
                var ballObj1 = m_coach.GetSeenCoachObject("ball");
                if (CheckQuerter(quarter, ballObj1) || CheckQuerter(q2, ballObj1))
                {
                    SeenObject ball = null;
                    SeenObject goal = null;

                    //Get current player's info:
                    var bodyInfo = GetBodyInfo();
                    Console.WriteLine($"Kicks so far : {bodyInfo.Kick}");

                    while (ball == null || ball.Distance > 1.5)
                    {
                        SeenCoachObject myPlayer = m_coach.GetSeenCoachObject($"player Brazil {m_number}");
                        //Get field information from god (coach).
                        

                        m_memory.waitForNewInfo();
                        ball = m_memory.GetSeenObject("ball");
                        if (ball == null)
                        {
                            // If you don't know where is ball then find it
                            m_robot.Turn(40);
                            m_memory.waitForNewInfo();
                        }
                        else if (ball.Distance > 1.5)
                        {

                            //   var player2 = m_coach.GetSeenCoachObject($"player Brazil 4");
                            //   var ballobj = m_coach.GetSeenCoachObject("ball");

                            //  if (Math.Abs(myPlayer.AngleTo(ballobj) - myPlayer.AngleTo(player2)) < 3)
                            //{
                            //    var angleToBall = myPlayer.AngleToBallX(ballobj.Pos.Value.X, myPlayer.Pos.Value.Y);
                            //    if (Math.Abs(angleToBall) > 0.5)
                            //    {
                            //        m_robot.Turn(angleToBall);
                            //    }
                            //    else
                            //    {
                            //        m_robot.Dash(50);
                            //    }

                            //}

                            if (Math.Abs(ball.Direction.Value) > 0.7)
                                m_robot.Turn(ball.Direction.Value);
                            else
                            {

                                    m_robot.Dash(120 * ball.Distance.Value);
                                
                                
                            }

                        }
                    }

                    // We know where is ball and we can kick it
                    // so look for goal
                    var goalstr = m_side == 'l' ? "goal r" : "goal l";
                    var angle = 10;
                    while (goal == null)
                    {
                        m_memory.waitForNewInfo();
                        goal = m_memory.GetSeenObject(goalstr);

                        if (goal == null)
                        {
                            m_robot.Turn(40);
                        }
                    }

                    var myPlayer2 = m_coach.GetSeenCoachObject($"player Brazil {m_number}");
                    var ballObj = m_coach.GetSeenCoachObject("ball");
                    var atk = myPlayer2.AngleToKick(ballObj);
                    if (Math.Abs(atk) > 120)
                    {
                        m_robot.Kick(7, 90);
                    }

                    //var player2 = m_coach.GetSeenCoachObject($"player Brazil 4");
                    //var coachGoalObject = m_coach.GetSeenCoachObject(goalstr);
                    //if (player2.DistanceTo(coachGoalObject) < myPlayer2.DistanceTo(coachGoalObject))
                    //{
                    //    m_robot.Kick(20, myPlayer2.AngleToKick(player2));

                    //}
                    else
                    {
                       
                        if (goal.Distance > 25)
                            m_robot.Kick(20, goal.Direction.Value);
                        else m_robot.Kick(150, goal.Direction.Value + 12);
                    }

                }

                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                try
                {
                    Thread.Sleep(SoccerParams.simulator_step);
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
        private bool AvoidPlayer()
        {
            m_memory.waitForNewInfo();
            var players = from x in m_memory.m_info.m_seenObjects
                          where x.Name.Contains("player")
                          select x;
            foreach(var i in players)
            {
                if(i.Distance.Value < 2 && Math.Abs(i.Direction.Value) < 5)
                {
                    m_robot.Turn(i.Direction.Value + 40);
                    Thread.Sleep(SoccerParams.simulator_step);
                    m_robot.Dash(50);

                    return true;
                }
            }
            return false;
        }
    }
}
