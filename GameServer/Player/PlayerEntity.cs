using GameServer.Match;
using GameServer.Room;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Player
{
    public class PlayerEntity
    {
        public int session;
        public UserInfo userInfo;
        public RolesInfo rolesInfo;
        public MatchEntity matchEntity;
        public RoomEntity roomEntity;
        public int TeamID;

        public void Destroy()
        {
            Debug.Log("用户断开连接");
        }
    }


}
