using GameServer.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Net
{
    class UClient
    {
        private USocket uSocket;
        public IPEndPoint endPoint;
        private int sendSN;
        private int handleSN;
        public int session;
        public bool isConnect = true; //是否处于连接的状态
        private int overtime = 150;
        private Action<BufferEntity> dispatchNetEvent;
        private ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();
        private ConcurrentDictionary<int, BufferEntity> waitHandle = new ConcurrentDictionary<int, BufferEntity>();
        public UClient(USocket uSocket, IPEndPoint endPoint, int sendSN, int handleSN, int session, Action<BufferEntity> dispatchNetEvent)
        {
            this.uSocket = uSocket;
            this.endPoint = endPoint;
            this.sendSN = sendSN;
            this.handleSN = handleSN;
            this.session = session;
            this.dispatchNetEvent = dispatchNetEvent;
            CheckOutTime();
        }

        //对已发送的消息进行超时检测
        public async void CheckOutTime()
        {
            await Task.Delay(overtime);
            foreach (var package in sendPackage.Values)
            {
                if (TimeHelper.Now()-package.time>=overtime*10)
                {
                    Debug.LogError($"重发十次还是失败，协议ID：{package.messageID}");
                    uSocket.RemoveClient(session);
                    return;
                }
                if (TimeHelper.Now()-package.time>=(package.recurCount+1)*overtime)
                {
                    package.recurCount++;
                    Debug.Log($"超时重发， 序号是：{package.sn}");
                    uSocket.Send(package.buffer, endPoint);
                }
            }
            CheckOutTime();
        }
        internal void Handle(BufferEntity buffer)
        {
            //移除掉已经发送的bufferEntity
            switch (buffer.messageType)
            {
                case 0: //ACK确认报文
                    BufferEntity bufferEntity;
                    if (sendPackage.TryRemove(buffer.sn, out bufferEntity))
                    {
                        Debug.Log($"报文已确认，序号：{buffer.sn}");
                    }
                    else
                    {
                        Debug.Log($"要确认的报文不存在， 序号：{buffer.sn}");
                    }
                    break;
                case 1: //业务报文
                    BufferEntity ackPackage = new BufferEntity(buffer);
                    uSocket.SendACK(ackPackage, endPoint);
                    Debug.Log("收到业务报文！");
                    HandleLogicPackage(buffer);

                    break;
                default:
                    break;
            }
        }

        private void HandleLogicPackage(BufferEntity buffer)
        {
            if (buffer.sn <= handleSN)
            {
                //可能有超时重新发过来的
                Debug.Log($"已经处理过的消息了， 序号：{buffer.sn}");
                return;
            }
            if (buffer.sn-handleSN>1)
            {
                if (waitHandle.TryAdd(buffer.sn, buffer))
                {
                    Debug.Log($"错序的报文，进行缓存，序号是：{buffer.sn}");
                }
            }
            handleSN = buffer.sn;
            if (dispatchNetEvent!=null)
            {
                dispatchNetEvent(buffer);
                Debug.Log("分发消息");
            }
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN+1, out nextBuffer))
            {
                HandleLogicPackage(nextBuffer);
            }
        }

        public void Send(BufferEntity package)
        {
            if (isConnect == false)
            {
                return;
            }
            package.time = TimeHelper.Now();
            sendSN++;
            package.sn = sendSN;
            package.Encoder(false);
            uSocket.Send(package.buffer, endPoint);
            if (session!=0)
            {
                //已经发送的数据
                sendPackage.TryAdd(package.sn, package);

            }
        }

        internal void Close()
        {
            isConnect = false;
            //客户端断开的时候，清理掉缓存
            if (PlayerMgr.GetPlayerEntityFromSession(session)!=null)
            {
                int rolesID = PlayerMgr.GetPlayerEntityFromSession(session).rolesInfo.RolesID;
                PlayerMgr.RemoveFromRolesID(rolesID);
                PlayerMgr.RemoveFromSession(session);
            }
        }
    }
}
