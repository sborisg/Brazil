using RoboCup.Entities;
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
    public class DefenderBottom : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;

        public DefenderBottom(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 30, 0);

        }

        public bool IsDefending { get; set; }

        public override void play()
        {
            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);

            IsDefending = true;
            while (!m_timeOver)
            {
                //if (IsDefending)
                //    GoToDefence();
                // else
                GoToDefence();
                //var ball = m_coach.GetSeenCoachObject("ball");
                //while (CheckBallInMyHalf(ball))
                //{
                SearchBall();
                if (!GoForBall())
                    continue;
                PassToAttacker();

                //m_memory.waitForNewInfo();
                //    ball = m_coach.GetSeenCoachObject("ball");
                //}
                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                //try
                //{
                //    Thread.Sleep(SoccerParams.simulator_step);
                //}
                //catch (Exception e)
                //{

                //}
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

        void GoToDefence()
        {
            //  if (!IsDefending)
            //       return;
            SeenObject corner_obj;
            string c_flag = "flag p r b";
            if (m_side == 'l')
                c_flag = "flag p l b";
            while (true)
            {
                //m_memory.waitForNewInfo();
                //var ball = m_coach.GetSeenCoachObject("ball");
                //if (!CheckBallInMyHalf(ball))
                //    return;
                corner_obj = FindMovingObj(c_flag);
                if (corner_obj.Distance.Value > 1.5)
                {
                    m_robot.Dash(100);
                }
                else
                {
                    return;
                }
            }


            //   else
            //       SearchBall();
        }

        void PassToAttacker()
        {
            //while(true)
            //{
            var attacker4 = m_coach.GetSeenCoachObject("player Brazil 4");
            var attacker5 = m_coach.GetSeenCoachObject("player Brazil 5");
            var me = m_coach.GetSeenCoachObject($"player Brazil {m_number}");

            var passTo = me.DistanceTo(attacker4) < me.DistanceTo(attacker5) ? attacker4 : attacker5;

            var angle = me.AngleToKick(passTo);
            m_robot.Turn(angle);

            Thread.Sleep(2 * SoccerParams.simulator_step);
            m_robot.Kick(1000, angle);
            //  }
        }

        private double GetAngle(SeenCoachObject me, SeenCoachObject attacker)
        {
            var angle = Math.Atan2(me.Pos.Value.Y - attacker.Pos.Value.Y, me.Pos.Value.X - attacker.Pos.Value.X) * 180 / Math.PI;
            return angle - me.BodyAngle.Value;
        }

        bool GoForBall()
        {
            while (true)
            {
                //m_memory.waitForNewInfo();
                var ball_obj = FindMovingObj("ball");

                //m_robot.Dash(100);
                //m_memory.waitForNewInfo();
                var ball = m_coach.GetSeenCoachObject($"ball");
                var coachBall = m_coach.GetSeenCoachObject($"ball");
                if (!CheckBallInMyHalf(coachBall))
                    return false;
                if (ball_obj.Distance > 1)
                {
                    m_robot.Dash(100);
                }
                else
                    return true;
            }
        }

        void SearchBall()
        {
            while (true)
            {
                var ball_obj = FindMovingObj("ball");
                var coachBall = m_coach.GetSeenCoachObject("ball");

                if ( ball_obj.Distance.Value < 40)
                {
                    return;
                }
            }

        }

        private SeenObject FindMovingObj(string str)
        {
            var movingObj = m_memory.GetSeenObject(str);

            while (movingObj == null)
            {
                m_robot.Turn(90);
                m_memory.waitForNewInfo();
                movingObj = m_memory.GetSeenObject(str);
            }
            m_robot.Turn(movingObj.Direction.Value);
            return movingObj;
        }

        private SeenObject FindMovingObjNoTurn(string str)
        {
            var movingObj = m_memory.GetSeenObject(str);

            while (movingObj == null)
            {
                m_robot.Turn(90);
                m_memory.waitForNewInfo();
                movingObj = m_memory.GetSeenObject(str);
            }
            return movingObj;
        }

        private bool CheckBallInMyHalf(SeenCoachObject ball)
        {
            if (m_side == 'r')
                return (ball.Pos.Value.X >= 0);
            else
                return (ball.Pos.Value.X <= 0);
        }

    }
}
