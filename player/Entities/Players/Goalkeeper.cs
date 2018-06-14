using RoboCup;
using RoboCup.Entities;
using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace player.Entities.Players
{
    /// <summary>
    /// Also called goalie, or keeper
    /// The goalkeeper is simply known as the guy with gloves who keeps the opponents from scoring. He has a special position because only him can play the ball with his hands (provided that he is inside his own penalty area and the ball was not deliberately passed to him by a team mate).
    /// Aside from being the last line of defense, the goalkeeper is the first person in attack. That is why keepers who can make good goal kicks and strategic ball throws to team mates are valuable.
    /// The goalie has four main roles: saving, clearing, directing the defense, and distributing the ball. Saving is the act of preventing the ball from entering the net while clearing means keeping the ball far from the goal area.
    /// The goalkeeper has the role of directing the defense since he is the farthest player at the back and he can see where the defenders should position themselves.
    /// Distributing the ball happens when a goalkeeper decides whether to kick the ball or throw it after making a save. Where the keeper throws or kicks the ball is the first instance of attack
    /// </summary>
    public class Goalkeeper : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;

        object syncObj = new object();
        bool breakScan;
        //ICoach m_coach;
        Status m_status;

        bool BreakScan
        {
            get
            {
                lock (syncObj)
                {
                    return breakScan;
                }
            }
            set
            {
                lock (syncObj)
                {
                    breakScan = value;
                }
            }
        }

        public Goalkeeper(Team team, ICoach coach)
        {
            m_coach = coach;
            m_memory = new Memory();
            m_team = team;
            m_robot = new Robot(m_memory);
           // (init TeamName[(version VerNum)] [(goalie)])

            m_robot.Init(team.m_teamName, out m_side, out m_number, out m_playMode, "(init " + m_team.m_teamName + " (version 6) (goalie)");

            Console.WriteLine("New Player - Team: " + m_team.m_teamName + " Side:" + m_side + " Num:" + m_number);

            m_strategy = new Thread(new ThreadStart(play));
            m_strategy.Start();

        }

       
        public override void play()
        {
            SeenObject obj;
            // first move to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);
            int i = 0;
            while (true)
            {
                GotoGoal();
                ScanForBall();
                GoForBall();
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

        void GotoGoal()
        {
            var myGoalStr = m_side == 'l' ? "goal l" : "goal r";

            SeenObject myGoal = FindGoal(myGoalStr);

            var ballObj = FindMovingObj("ball");
            var tolerance = ballObj.Distance.Value > 1.5 ? 0.8 : 5.0;

            while (myGoal.Distance.Value > tolerance)
            {           
                m_robot.Dash(50 * myGoal.Distance.Value);
                myGoal = FindGoal(myGoalStr);
            }

            if (ballObj.Distance.Value < 1.0)
            {
                var otherGoalStr = m_side == 'l' ? "goal r" : "goal l";
                var goalObj = FindMovingObj(otherGoalStr);
                Thread.Sleep(2 * SoccerParams.simulator_step);
                m_robot.Kick(100, goalObj.Direction.Value);
                Thread.Sleep(2000);
            }

        }

        void ScanForBall()
        {
            while (true)
            {
                var ball = FindMovingObj("ball");
                if (ball.Distance.Value < 15)
                    break;
            }
        }

        void GoForBall()
        {
            var myGoalStr = m_side == 'l' ? "goal l" : "goal r";

            var ballObj = FindMovingObj("ball");
            if (ballObj.Distance.Value > 20)
                return;

            SeenObject myGoal;
            while (ballObj.Distance.Value > 1.0)
            {
                m_robot.Dash(100 * ballObj.Distance.Value);

                ballObj = FindMovingObj("ball");
                if (ballObj.Distance.Value > 20)
                    return;

                var me = m_coach.GetSeenCoachObject($"player Brazil {m_number}");
                var goal = m_coach.GetSeenCoachObject(myGoalStr);
                var d = m_coach.GetDistance(me, goal);

                if (d > 20)
                    return;
            }

            Thread.Sleep(2 * SoccerParams.simulator_step);
            m_robot.Catch(ballObj.Direction.Value);
        }

        private SeenObject FindGoal(string myGoalStr)
        {
            var myGoal = m_memory.GetSeenObject(myGoalStr);

            while (myGoal == null)
            {
                m_robot.Turn(40);
                myGoal = m_memory.GetSeenObject(myGoalStr);
            }
            m_robot.Turn(myGoal.Direction.Value);
            return myGoal;
        }

        private SeenObject FindMovingObj(string str)
        {
            var movingObj = m_memory.GetSeenObject(str);

            while (movingObj == null)
            {
                m_robot.Turn(40);
                m_memory.waitForNewInfo();
                movingObj = m_memory.GetSeenObject(str);
            }
            m_robot.Turn(movingObj.Direction.Value);
            return movingObj;
        }

       

    }

    
}


