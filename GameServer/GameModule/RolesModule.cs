using GameServer.Net;
using GameServer.Player;
using MobaServer.MySql;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.GameModule
{
    class RolesModule : GameModuleBase<RolesModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1201, HandleRolesCreateC2S);
        }

        private void HandleRolesCreateC2S(BufferEntity req)
        {
            //数据库查询下角色表有没有相同名称的
            RolesCreateC2S c2sMSG = ProtobufHelper.FromBytes<RolesCreateC2S>(req.proto);
            RolesCreateS2C s2cMSG = new RolesCreateS2C();
            if (DBRolesInfo.Instance.Select(MySqlCMD.Where("NickName", c2sMSG.NickName)) == null)
            {
                PlayerEntity player = PlayerMgr.GetPlayerEntityFromSession(req.session);

                RolesInfo rolesInfo = new RolesInfo();
                rolesInfo.NickName = c2sMSG.NickName;
                rolesInfo.ID = player.userInfo.ID;
                rolesInfo.RolesID = player.userInfo.ID;

                bool result = DBRolesInfo.Instance.Insert(rolesInfo);
                if (result)
                {
                    s2cMSG.Result = 0;
                    s2cMSG.RolesInfo = rolesInfo;
                    //缓存角色信息到服务器本地
                    player.rolesInfo = rolesInfo;
                }
                else
                {
                    s2cMSG.Result = 2;
                    Debug.Log($"插入角色数据存在异常， 昵称：{rolesInfo.NickName}");
                }
            }
            else
            {
                s2cMSG.Result = 1;
            }
            BufferFactory.CreateAndSendPackage(req, s2cMSG);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Realease()
        {
            base.Realease();
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
        }
    }
}
