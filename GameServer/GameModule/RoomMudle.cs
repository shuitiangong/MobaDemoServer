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
    class RoomMudle : GameModuleBase<RoomMudle>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1400, HandleRoomSelectHeroC2S);
            NetEvent.Instance.AddEventListener(1401, HandleRoomSelectHeroSkillC2S);
            NetEvent.Instance.AddEventListener(1404, HandleRoomSendMsgC2S);
            NetEvent.Instance.AddEventListener(1405, HandleRoomLockHeroC2S);
            NetEvent.Instance.AddEventListener(1406, HandleRoomLoadProgressC2S);
        }

        private void HandleRoomSelectHeroC2S(BufferEntity req)
        {
            RoomSelectHeroC2S c2sMSG = ProtobufHelper.FromBytes<RoomSelectHeroC2S>(req.proto);
            RoomSelectHeroS2C s2cMSG = new RoomSelectHeroS2C();
            s2cMSG.HeroID = c2sMSG.HeroID;
            PlayerEntity p = PlayerMgr.GetPlayerEntityFromSession(req.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;
            p.roomEntity.Broadcast(req.messageID, s2cMSG);
        }

        private void HandleRoomSelectHeroSkillC2S(BufferEntity req)
        {
            RoomSelectHeroSkillC2S c2sMSG = ProtobufHelper.FromBytes<RoomSelectHeroSkillC2S>(req.proto);
            RoomSelectHeroSkillS2C s2cMSG = new RoomSelectHeroSkillS2C();
            s2cMSG.GridID = c2sMSG.GridID;
            s2cMSG.SkillID = c2sMSG.SkillID;
            PlayerEntity p = PlayerMgr.GetPlayerEntityFromSession(req.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;
            //缓存角色技能
            p.roomEntity.UpdateSkill(s2cMSG.RolesID, s2cMSG.SkillID, s2cMSG.GridID);
            p.roomEntity.Broadcast(req.messageID, s2cMSG);
        }
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="req"></param>
        private void HandleRoomSendMsgC2S(BufferEntity req)
        {
            RoomSendMsgC2S c2sMSG = ProtobufHelper.FromBytes<RoomSendMsgC2S>(req.proto);
            RoomSendMsgS2C s2cMSG = new RoomSendMsgS2C();
            PlayerEntity p = PlayerMgr.GetPlayerEntityFromSession(req.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;
            s2cMSG.Text = c2sMSG.Text;
            //广播给同一队玩家
            //p.roomEntity.Broadcast(p.TeamID, req.messageID, s2cMSG);
            //广播给所有玩家
            p.roomEntity.Broadcast(req.messageID, s2cMSG);
        }

        private void HandleRoomLockHeroC2S(BufferEntity req)
        {
            RoomLockHeroC2S c2sMSG = ProtobufHelper.FromBytes<RoomLockHeroC2S>(req.proto);
            RoomLockHeroS2C s2cMSG = new RoomLockHeroS2C();
            s2cMSG.HeroID = c2sMSG.HeroID;
            PlayerEntity p = PlayerMgr.GetPlayerEntityFromSession(req.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;
            p.roomEntity.LockHero(s2cMSG.RolesID, s2cMSG.HeroID);
            p.roomEntity.Broadcast(req.messageID, s2cMSG);
        }

        private void HandleRoomLoadProgressC2S(BufferEntity req)
        {
            RoomLoadProgressC2S c2sMSG = ProtobufHelper.FromBytes<RoomLoadProgressC2S>(req.proto);
            RoomLoadProgressS2C s2cMSG = new RoomLoadProgressS2C();
            s2cMSG.IsBattleStart = false;
            PlayerEntity p = PlayerMgr.GetPlayerEntityFromSession(req.session);
            bool result = p.roomEntity.UpdateLoadProgress(p.rolesInfo.RolesID, c2sMSG.LoadProgress);
            if (result)
            {
                //所有玩家都已经加载完成了
                
            }
            else
            {
                p.roomEntity.GetLoadProgress(ref s2cMSG);
                BufferFactory.CreateAndSendPackage(req, s2cMSG);
            }
            p.roomEntity.Broadcast(req.messageID, s2cMSG);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Realease()
        {
            base.Realease();
            NetEvent.Instance.RemoveEventListener(1400, HandleRoomSelectHeroC2S);
            NetEvent.Instance.RemoveEventListener(1401, HandleRoomSelectHeroSkillC2S);
            NetEvent.Instance.RemoveEventListener(1404, HandleRoomSendMsgC2S);
            NetEvent.Instance.RemoveEventListener(1405, HandleRoomLockHeroC2S);
            NetEvent.Instance.RemoveEventListener(1406, HandleRoomLoadProgressC2S);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
        }
    }
}
