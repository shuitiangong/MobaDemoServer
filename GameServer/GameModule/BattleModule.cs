using GameServer.Net;
using GameServer.Room;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.GameModule
{
    class BattleModule : GameModuleBase<BattleModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1500, HandleBattleUserInputC2S);
        }

        private void HandleBattleUserInputC2S(BufferEntity res)
        {
            BattleUserInputC2S c2sMSG = ProtobufHelper.FromBytes<BattleUserInputC2S>(res.proto);
            RoomEntity roomEntity = RoomMgr.Instance.Get(c2sMSG.RoomID);
            if (roomEntity!=null)
            {
                roomEntity.HandleBattleUserInputC2S(c2sMSG);
            }

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
