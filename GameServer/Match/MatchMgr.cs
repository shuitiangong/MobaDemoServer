using GameServer.Room;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Match
{
    class MatchMgr:Singleton<MatchMgr>
    {
        public int Number = 1; // 几V几
        ConcurrentDictionary<int, MatchEntity> pool = new ConcurrentDictionary<int, MatchEntity>();
        public bool Add(MatchEntity matchEntity)
        {
            if (pool.TryAdd(matchEntity.TeamID, matchEntity))
            {
                //判断是否已满足开启一个房间
                if (pool.Count>=Number*2)
                {
                    MatchCompleteEvent();
                }
                return true;
            }
            else
            {
                Debug.Log($"加入匹配池失败, ID:{matchEntity.TeamID}!");
                return false;
            }
        }

        private void MatchCompleteEvent()
        {
            Debug.Log("匹配完成！");
            List<MatchEntity> teamA = new List<MatchEntity>();
            List<MatchEntity> teamB = new List<MatchEntity>();
            for (int i = 0; i<Number*2; i++)
            {
                MatchEntity entity = pool.ElementAt(i).Value;
                if (teamA.Count < Number)
                {
                    teamA.Add(entity);
                }
                else
                {
                    teamB.Add(entity);
                }
            }

            //把这两个队伍传递给房间
            RoomMgr.Instance.Add(teamA, teamB);
        }

        public bool Remove(MatchEntity matchEntity)
        {
            MatchEntity entity;
            return pool.TryRemove(matchEntity.TeamID, out entity);
        }
    }
}
