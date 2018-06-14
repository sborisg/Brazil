using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.Threading;
using TinMan;
using RoboCup.Infrastructure;
using System.Text.RegularExpressions;

namespace RoboCup.Entities
{
    public class Coach : ICoach
    {
        // Private members
        private UdpClient udpClient;
        private IPEndPoint endPoint;
        private Thread m_parser;
        private Dictionary<String, SeenCoachObject> m_seenCoachObjects;

        public Coach()
        {
            m_seenCoachObjects = new Dictionary<String, SeenCoachObject>();
            endPoint = new IPEndPoint(IPAddress.Parse(SoccerParams.m_host), SoccerParams.m_coachPort);
            udpClient = new UdpClient();
            m_parser = new Thread(new ThreadStart(parseCoachInformation));
            m_parser.Start();
        }

        //---------------------------------------------------------------------------
        // This function sends via socket message to the server
        private void send(String message)
        {
            Byte[] sendBytes = new byte[SoccerParams.MSG_SIZE_COACH];
            var messageBytes = Encoding.ASCII.GetBytes(message);

            for (int i = 0; i < messageBytes.Length; i++)
            {
                sendBytes[i] = messageBytes[i];
            }
            try
            {
                udpClient.Send(sendBytes, SoccerParams.MSG_SIZE_COACH, endPoint);
            }
            catch (Exception e)
            {
                Debug.Write("socket sending error " + e.Message);
            }
        }

        //---------------------------------------------------------------------------
        // This function waits for new message from server
        private String receive()
        {
            byte[] buffer = new byte[SoccerParams.MSG_SIZE];
            try
            {
                buffer = udpClient.Receive(ref endPoint);
            }
            catch (Exception e)
            {
                Debug.Write("socket receiving error " + e.Message);
            }
            return (Encoding.ASCII.GetString(buffer));
        }

        //---------------------------------------------------------------------------
        // This function parses sensor information
        private void parseCoachInformation()
        {
            String message;
            while (true)
            {
                Look();
                message = receive();
                //Console.WriteLine($"Received: {message}");
                ParseLookMessage(message);
                Thread.Sleep(SoccerParams.simulator_step);
            }
        }

        public Dictionary<String, SeenCoachObject> GetSeenCoachObjects()
        {
            return m_seenCoachObjects;
        }

        public SeenCoachObject GetSeenCoachObject(string name)
        {
            SeenCoachObject result = null;
            var obj = m_seenCoachObjects.TryGetValue(name, out result);
            return result;
        }

        private void Look()
        {
            send("(look)");
        }

        private void ParseLookMessage(string message)
        {
            if (!message.StartsWith("(ok look"))
                return;

            var objectsMatch = Regex.Matches(message, MagicPattern);
            var tmpDictionary = new Dictionary<String, SeenCoachObject>();
            for (int i = 0; i < objectsMatch.Count; i++)
            {
                var obj = objectsMatch[i];
                var innerObjects = obj.Value.Split(')');
                var name = innerObjects[0].Substring(2);
                var parameters = innerObjects[1].Substring(1).Split(' ');
                var floatParams = parameters.Select(strParam => float.Parse(strParam)).ToArray();
                var seenObject = new SeenCoachObject(name, floatParams);
                tmpDictionary.Add(name, seenObject);
            }
            m_seenCoachObjects = tmpDictionary;
        }

        public Status GetPlayersData()
        {
            Status ret = new Status();
            ret.MyPlayers = (from ob in GetSeenCoachObjects()
                                  where ob.Key.Contains("player") && ob.Key.Contains("Brazil")
                                  select ob.Value).ToList();
            ret.OpponentPlayers = (from ob in GetSeenCoachObjects()
                                        where ob.Key.Contains("player") && !ob.Key.Contains("Brazil")
                                        select ob.Value).ToList();

            return ret;
        }


        public PlayersData GetPlayersFullData(SeenCoachObject me)
        {
            var status = GetPlayersData();
            PlayersData ret = new PlayersData()
            {
                MyPlayers = new Dictionary<SeenCoachObject, PlayerData>(),
                OpponentPlayers = new Dictionary<SeenCoachObject, PlayerData>()
            };

            foreach (var player in status.MyPlayers)
            {
                ret.MyPlayers.Add(player,new PlayerData() {
                                                           Angle = GetAngle(me, player),
                                                           Distance = GetDistance(me, player),
                                                           DistanceFromBall = GetDistanceFromBall(player)
                                                          });
            }

            foreach (var player in status.OpponentPlayers)
            {
                ret.OpponentPlayers.Add(player, new PlayerData() {
                                                                    Angle = GetAngle(me, player),
                                                                    Distance = GetDistance(me, player),
                                                                    DistanceFromBall = GetDistanceFromBall(player)
                                                                });
            }

            return ret;
        }

        public double GetDistanceFromBall(SeenCoachObject player)
        {
            var ball = GetSeenCoachObject("ball");
            return GetDistance(player, ball);
        }

        public double GetDistance(SeenCoachObject me, SeenCoachObject player)
        {
           return  Math.Sqrt(Math.Pow(me.Pos.Value.Y - player.Pos.Value.Y, 2) + Math.Pow(me.Pos.Value.X - player.Pos.Value.X, 2));       
        }

        public double GetAngle(SeenCoachObject me, SeenCoachObject player)
        {
            var angle = Math.Atan2(me.Pos.Value.Y - player.Pos.Value.Y, me.Pos.Value.X - player.Pos.Value.X) * 180 / Math.PI - me.BodyAngle.Value;
            if (angle > 180)
                angle -= 360;

            if (angle < -180)
                angle += 360;

            return angle;
        }
		
		public bool PlayerAvialible( double myAngle, Dictionary<SeenCoachObject, PlayerData> Opponents)
        {
            foreach (var o_p in Opponents)
            {
                if (o_p.Value.Angle - myAngle < 3 || myAngle - o_p.Value.Angle < 3)
                    // if (o_p.Key is AttackerExample) // check if he attacker
                    return false;
            }
            return true;
        }

        public SeenCoachObject GetNearestPlayerAvialible(SeenCoachObject me)
        {
            PlayersData dic = GetPlayersFullData(me);
            PlayerData best_player_data = new PlayerData();
            best_player_data.Angle = 0;
            best_player_data.Distance = 100;
            SeenCoachObject best_player_obj = me;
            foreach (var p in dic.MyPlayers)
            {
                if (p.Value.Distance > 0.5 && p.Value.Distance < best_player_data.Distance && PlayerAvialible(p.Value.Angle, dic.OpponentPlayers) )
                {
                    best_player_data = p.Value;
                    best_player_obj = p.Key;
                }
            }
            return best_player_obj;
        }

        private const string MagicPattern = "\\(\\(.*?\\).*?\\)";
    }

    public class Status
    {
        public bool AttackOrDefence;
        public List<SeenCoachObject> MyPlayers { get; set; }

        public List<SeenCoachObject> OpponentPlayers { get; set; }
    }

    public class PlayerAngels
    {
        public Dictionary<SeenCoachObject, double> MyPlayers { get; set; }

        public Dictionary<SeenCoachObject, double> OpponentPlayers { get; set; }
    }

    public class PlayersData
    {
        public Dictionary<SeenCoachObject, PlayerData> MyPlayers { get; set; }

        public Dictionary<SeenCoachObject, PlayerData> OpponentPlayers { get; set; }
    }

    public class PlayerData
    {
        public double Angle { get; set; }
        public double Distance { get; set; }
        public double DistanceFromBall { get; set; }
    }


}
