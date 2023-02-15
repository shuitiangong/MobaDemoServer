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

        public void Destroy()
        {
            Debug.Log("用户断开连接");
        }
    }


}
