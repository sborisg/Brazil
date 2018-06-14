using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup.Entities
{
    public interface ICoach
    {
        SeenCoachObject GetSeenCoachObject(string name);
        Dictionary<String, SeenCoachObject> GetSeenCoachObjects();
        Status GetPlayersData();
        PlayersData GetPlayersFullData(SeenCoachObject me);
		SeenCoachObject GetNearestPlayerAvialible(SeenCoachObject me);
        double GetAngle(SeenCoachObject me, SeenCoachObject player);
        double GetDistance(SeenCoachObject me, SeenCoachObject player);
    }
}
