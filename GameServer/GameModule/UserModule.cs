using GameServer.Net;
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
            throw new NotImplementedException();
        }

        private void HandleUserLoginC2S(BufferEntity req)
        {
            throw new NotImplementedException();
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
