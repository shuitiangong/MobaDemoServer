using GameServer.Match;
using GameServer.Net;
using GameServer.Player;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.GameModule
{
    class LobbyModule : GameModuleBase<LobbyModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1300, HandleLobbyToMatchC2S);
            NetEvent.Instance.AddEventListener(1302, HandleLobbyQuitMatchC2S);

        }

        private void HandleLobbyToMatchC2S(BufferEntity req)
        {
            LobbyToMatchC2S c2sMSG = ProtobufHelper.FromBytes<LobbyToMatchC2S>(req.proto);
            LobbyToMatchS2C s2cMSG = new LobbyToMatchS2C();
            s2cMSG.Result = 0;
            MatchEntity matchEntity = new MatchEntity();
            PlayerEntity player = PlayerMgr.GetPlayerEntityFromSession(req.session);
            player.matchEntity = matchEntity;
            matchEntity.TeamID = player.rolesInfo.RolesID;
            matchEntity.player = player;
            BufferFactory.CreateAndSendPackage(req, s2cMSG);
        }

        private void HandleLobbyQuitMatchC2S(BufferEntity req)
        {
            LobbyQuitMatchC2S c2sMSG = ProtobufHelper.FromBytes<LobbyQuitMatchC2S>(req.proto);
            LobbyQuitMatchS2C s2cMSG = new LobbyQuitMatchS2C();
            PlayerEntity player = PlayerMgr.GetPlayerEntityFromSession(req.session);
            if (player!=null)
            {
                bool result = MatchMgr.Instance.Remove(player.matchEntity);
                if (result)
                {
                    player.matchEntity = null;
                    s2cMSG.Result = 0; //移除成功
                }
                else
                {
                    s2cMSG.Result = 1; //不在匹配状态
                }
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
            NetEvent.Instance.RemoveEventListener(1300, HandleLobbyToMatchC2S);
            NetEvent.Instance.RemoveEventListener(1302, HandleLobbyQuitMatchC2S);
        }
    }
}
