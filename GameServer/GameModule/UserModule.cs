using GameServer.Net;
using GameServer.Player;
using MobaServer.MySql;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.GameModule
{
    public class UserModule : GameModuleBase<UserModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1000, HandleUserRegisterC2S);
            NetEvent.Instance.AddEventListener(1001, HandleUserLoginC2S);
        }

        private void HandleUserRegisterC2S(BufferEntity req)
        {
            //得到客户端发送的数据
            UserRegisterC2S c2sMSG = ProtobufHelper.FromBytes<UserRegisterC2S>(req.proto);
            UserRegisterS2C s2cMSG = new UserRegisterS2C();
            if (DBUserInfo.Instance.Select(MySqlCMD.Where("Account", c2sMSG.UserInfo.Account))!=null) {
                Debug.Log("账号已被注册");
                s2cMSG.Result = 3;
            }
            else
            {
                bool result = DBUserInfo.Instance.Insert(c2sMSG.UserInfo);
                if (result)
                {
                    s2cMSG.Result = 0; //注册成功
                }
                else
                {
                    s2cMSG.Result = 4; //未知错误
                }

            }

            //返回客户端结果
            BufferFactory.CreateAndSendPackage(req, s2cMSG);
        }

        private void HandleUserLoginC2S(BufferEntity req)
        {
            //得到客户端发送的数据
            UserLoginC2S c2sMSG = ProtobufHelper.FromBytes<UserLoginC2S>(req.proto);
            //匹配记录
            string sqlCMD = MySqlCMD.Where("Account", c2sMSG.UserInfo.Account)
                            + MySqlCMD.And("Password", c2sMSG.UserInfo.Password);
            UserInfo userInfo = DBUserInfo.Instance.Select(sqlCMD);
            UserLoginS2C s2cMSG = new UserLoginS2C();
            if (userInfo!=null)
            {
                s2cMSG.UserInfo = userInfo;
                s2cMSG.Result = 0; //登陆成功
                PlayerMgr.Add(req.session, userInfo.ID, new PlayerEntity() { 
                    userInfo = userInfo,
                    session = req.session,
                });
                RolesInfo rolesInfo = DBRolesInfo.Instance.Select(MySqlCMD.Where("ID", s2cMSG.UserInfo.ID));
                if (rolesInfo!=null)
                {
                    s2cMSG.RolesInfo = rolesInfo;
                    PlayerEntity playerEntity = PlayerMgr.GetPlayerEntityFromSession(req.session);
                    playerEntity.rolesInfo = rolesInfo;
                }
            }
            else
            {
                s2cMSG.Result = 2; //账号和密码不匹配
            }
            //返回客户端结果
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
            NetEvent.Instance.RemoveEventListener(1000, HandleUserRegisterC2S);
            NetEvent.Instance.RemoveEventListener(1001, HandleUserLoginC2S);
        }
    }
}
