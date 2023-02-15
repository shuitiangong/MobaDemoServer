using GameServer.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Room
{
    class RoomMgr:Singleton<RoomMgr>
    {
        public void Add(List<MatchEntity> teamA, List<MatchEntity> teamB)
        {

        }

        internal void Remove(int roomID)
        {
            throw new NotImplementedException();
        }
    }
}
