using GameServer.Net;
using GameServer.Player;
using Google.Protobuf;
using ProtoMsg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Room
{
    public class RoomEntity
    {
        public int roomID;
        public int selectHeroTime = 10000;
        public RoomInfo roomInfo;
        ConcurrentDictionary<int, PlayerInfo> playerList = new ConcurrentDictionary<int, PlayerInfo>();
        ConcurrentDictionary<int, UClient> clientList = new ConcurrentDictionary<int, UClient>();
        public int lockCount;
        ConcurrentDictionary<int, int> playerProgress = new ConcurrentDictionary<int, int>();
        bool isLoadComplete = false;

        public RoomEntity(RoomInfo roomInfo)
        {
            this.roomID = roomInfo.ID;
            this.roomInfo = roomInfo;
            Init();

        }
        /// <summary>
        /// 角色初始化
        /// </summary>
        void PlayerInit()
        {

            for (int i = 0; i<roomInfo.TeamA.Count; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.RolesInfo = roomInfo.TeamA[i];
                playerInfo.SkillA = 103;
                playerInfo.SkillB = 106;
                playerInfo.HeroID = 0; //表示未选择
                playerInfo.TeamID = 0;
                playerInfo.PosID = i; //0-4
                playerList.TryAdd(playerInfo.RolesInfo.RolesID, playerInfo);
                UClient client = GameMgr.uSocket.GetClient(PlayerMgr.GetPlayerEntityFromRolesID(playerInfo.RolesInfo.RolesID).session);
                clientList.TryAdd(playerInfo.RolesInfo.RolesID, client);

            }
            for (int i = 0; i < roomInfo.TeamB.Count; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.RolesInfo = roomInfo.TeamB[i];
                playerInfo.SkillA = 103;
                playerInfo.SkillB = 106;
                playerInfo.HeroID = 0; //表示未选择
                playerInfo.TeamID = 1;
                playerInfo.PosID = i; //5-9
                playerList.TryAdd(playerInfo.RolesInfo.RolesID, playerInfo);
                UClient client = GameMgr.uSocket.GetClient(PlayerMgr.GetPlayerEntityFromRolesID(playerInfo.RolesInfo.RolesID).session);
                clientList.TryAdd(playerInfo.RolesInfo.RolesID, client);
            }
        }
        /// <summary>
        /// 整个房间的初始化
        /// </summary>
        private async void Init()
        {
            PlayerInit();
            await Task.Delay(selectHeroTime);
            if (lockCount == (roomInfo.TeamA.Count + roomInfo.TeamB.Count))
            {
                //所有人都锁定了选择的英雄
                //可以加载战斗了
                RoomToBattleS2C s2cMSG = new RoomToBattleS2C();
                foreach (var rolesID in playerList.Keys)
                {
                    UClient client = GameMgr.uSocket
                        .GetClient(PlayerMgr.GetPlayerEntityFromRolesID(rolesID).session);
                    clientList.TryAdd(rolesID, client);
                    s2cMSG.PlayerList.Add(playerList[rolesID]);
                }
                Broadcast(1407, s2cMSG);
            }
            else
            {
                //解散房间
                RoomCloseS2C s2cMSG = new RoomCloseS2C();
                Broadcast(1403, s2cMSG);
                RoomMgr.Instance.Remove(roomID);
            }
        }

        #region 广播的接口
        private void Broadcast(int messageID, IMessage s2cMSG)
        {
            foreach (var client in clientList.Values)
            {
                BufferFactory.CreateAndSendPackage(client, messageID, s2cMSG);

            }
        }

        private void Broadcast(int teamID, int messageID, IMessage s2cMSG)
        {
            if (teamID==0)
            {
                //A队伍
                for (int i = 0; i < roomInfo.TeamA.Count; i++)
                {
                    UClient client;
                    if (clientList.TryGetValue(roomInfo.TeamA[i].RolesID, out client))
                    {
                        BufferFactory.CreateAndSendPackage(clientList[roomInfo.TeamA[i].RolesID], messageID, s2cMSG);
                    }
                }
            }
            else
            {
                //B队伍
                for (int i = 0; i < roomInfo.TeamB.Count; i++)
                {
                    UClient client;
                    if (clientList.TryGetValue(roomInfo.TeamB[i].RolesID, out client))
                    {
                        BufferFactory.CreateAndSendPackage(clientList[roomInfo.TeamB[i].RolesID], messageID, s2cMSG);
                    }
                }
            }
        }
    }
    #endregion
}
