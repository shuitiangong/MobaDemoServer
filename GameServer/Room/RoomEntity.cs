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
        public int selectHeroTime = 30000;
        public RoomInfo roomInfo;
        ConcurrentDictionary<int, PlayerInfo> playerList = new ConcurrentDictionary<int, PlayerInfo>();
        ConcurrentDictionary<int, UClient> clientList = new ConcurrentDictionary<int, UClient>();
        public int lockCount;
        ConcurrentDictionary<int, int> playerProgress = new ConcurrentDictionary<int, int>();
        bool isLoadComplete = false;

        public void HandleBattleUserInputC2S(BattleUserInputC2S c2sMSG)
        {
            BattleUserInputS2C s2cMSG = new BattleUserInputS2C();
            s2cMSG.CMD = c2sMSG;
            Broadcast(1500, s2cMSG); //可以定一个延时
        }

        public RoomEntity(RoomInfo roomInfo)
        {
            this.roomID = roomInfo.ID;
            this.roomInfo = roomInfo;
            Init();

        }
        /// <summary>
        /// 角色初始化
        /// </summary>
        private void PlayerInit()
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
                //加载进度
                playerProgress.TryAdd(playerInfo.RolesInfo.RolesID, 0);

            }
            for (int i = 0; i < roomInfo.TeamB.Count; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.RolesInfo = roomInfo.TeamB[i];
                playerInfo.SkillA = 103;
                playerInfo.SkillB = 106;
                playerInfo.HeroID = 0; //表示未选择
                playerInfo.TeamID = 1;
                playerInfo.PosID = i+5; //5-9
                playerList.TryAdd(playerInfo.RolesInfo.RolesID, playerInfo);
                UClient client = GameMgr.uSocket.GetClient(PlayerMgr.GetPlayerEntityFromRolesID(playerInfo.RolesInfo.RolesID).session);
                clientList.TryAdd(playerInfo.RolesInfo.RolesID, client);
                //加载进度
                playerProgress.TryAdd(playerInfo.RolesInfo.RolesID, 0);
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
        public void Broadcast(int messageID, IMessage s2cMSG)
        {
            foreach (var client in clientList.Values)
            {
                BufferFactory.CreateAndSendPackage(client, messageID, s2cMSG);
            }
        }

        public void Broadcast(int teamID, int messageID, IMessage s2cMSG)
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
        #endregion
        public void LockHero(int rolesID, int heroID)
        {
            lockCount++;
            playerList[rolesID].HeroID = heroID;
        }
        /// <summary>
        /// 更新召唤师技能
        /// </summary>
        /// <param name="rolesID"></param>
        /// <param name="skillID"></param>
        /// <param name="gridID"></param>
        /// 
        public void UpdateSkill(int rolesID, int skillID, int gridID)
        {
            if (gridID==0)
            {
                playerList[rolesID].SkillA = skillID;
            }
            else
            {
                playerList[rolesID].SkillB = skillID;
            }
        }
        /// <summary>
        /// 更新所有用户的进度
        /// </summary>
        /// <param name="rolesID"></param>
        /// <param name="progress"></param>
        public bool UpdateLoadProgress(int rolesID, int progress)
        {
            if (isLoadComplete)
            {
                return true;
            }
            playerProgress[rolesID] = progress;
            if (isLoadComplete==false)
            {
                foreach(var player in playerProgress.Values)
                {
                    if (player<100)
                    {
                        return false;
                    }
                }
                isLoadComplete = true;
                //告诉所有客户端都加载完成了
                RoomLoadProgressS2C s2cMSG = new RoomLoadProgressS2C();
                s2cMSG.IsBattleStart = true;
                foreach (var item in playerProgress.Keys)
                {
                    s2cMSG.RolesID.Add(item);
                    s2cMSG.LoadProgress.Add(playerProgress[item]);
                }
                Broadcast(1406, s2cMSG);
            }
            return true;
        }

        /// <summary>
        /// 获取所有用户加载进度
        /// </summary>
        /// <param name="s2cMSG"></param>
        public void GetLoadProgress(ref RoomLoadProgressS2C s2cMSG)
        {
            foreach (var item in playerProgress.Keys)
            {
                s2cMSG.RolesID.Add(item);
                s2cMSG.LoadProgress.Add(playerProgress[item]);
            }
        }

        /// <summary>
        /// 房间关闭时销毁
        /// </summary>
        public void Close()
        {

        }
    }
}
