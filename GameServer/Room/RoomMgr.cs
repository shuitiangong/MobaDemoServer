using GameServer.Match;
using ProtoMsg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Room
{
    class RoomMgr:Singleton<RoomMgr>
    {
        private int roomID = 0; //房间ID 自增
        ConcurrentDictionary<int, RoomEntity> roomDIC = new ConcurrentDictionary<int, RoomEntity>();

        public void Add(List<MatchEntity> teamA, List<MatchEntity> teamB)
        {
            roomID++;
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.ID = roomID;
            for (int i = 0; i<teamA.Count; i++)
            {
                MatchMgr.Instance.Remove(teamA[i]);
                roomInfo.TeamA.Add(teamA[i].player.rolesInfo);
            }
            for (int i = 0; i < teamB.Count; i++)
            {
                MatchMgr.Instance.Remove(teamB[i]);
                roomInfo.TeamB.Add(teamB[i].player.rolesInfo);
            }
            roomInfo.StartTime = TimeHelper.Now();
            RoomEntity roomEntity = new RoomEntity(roomInfo);
            if (roomDIC.TryAdd(roomInfo.ID, roomEntity)) {
                //告诉每个客户端，匹配成功了，然后进入到房间，选择英雄
                LobbyUpdateMatchStateS2C s2cMSG = new LobbyUpdateMatchStateS2C();
                s2cMSG.Result = 0;
                s2cMSG.RoomInfo = roomInfo;
                roomEntity.Broadcast(1301, s2cMSG);
            }
            for (int i = 0; i<teamA.Count; i++)
            {
                teamA[i].player.matchEntity = null;
                teamA[i].player.roomEntity = null;
                teamA[i].player.TeamID = 0;
            }
            for (int i = 0; i < teamB.Count; i++)
            {
                teamB[i].player.matchEntity = null;
                teamB[i].player.roomEntity = null;
                teamB[i].player.TeamID = 0;
            }
        }

        internal void Remove(int roomID)
        {
            RoomEntity roomEntity;
            if (roomDIC.TryRemove(roomID, out roomEntity))
            {
                roomEntity.Close();
                roomEntity = null;
            }
        }

        public RoomEntity Get(int roomID)
        {
            RoomEntity roomEntity;
            if (roomDIC.TryGetValue(roomID, out roomEntity))
            {
                return roomEntity;
            }
            return null;
        }

        /// <summary>
        /// 清除所有roomEntity
        /// </summary>
        public void Clear()
        {
            foreach(var roomEntity in roomDIC.Values)
            {
                Remove(roomEntity.roomID);
            }
        }
    }
}
