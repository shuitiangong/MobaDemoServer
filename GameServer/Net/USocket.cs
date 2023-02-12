using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.Net
{
    class USocket
    {
        UdpClient socket;
        string ip = "192.168.56.1";
        int port = 8899;
        int sessionID = 1000;
        Action<BufferEntity> dispatchNetEvent;
        ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        ConcurrentDictionary<int, UClient> clients = new ConcurrentDictionary<int, UClient>();

        public USocket(Action<BufferEntity> dispatchNetEvent)
        {
            this.dispatchNetEvent = dispatchNetEvent;
            socket = new UdpClient(port);
            Receive();
            Task.Run(Handle, ct.Token);
        }

        public async void Send(byte[] data, IPEndPoint endPoint)
        {
            if (socket != null)
            {
                try
                {
                    int length = await socket.SendAsync(data, data.Length);
                    if (data.Length == length)
                    {
                        //完整的发送
                    }
                }
                catch (Exception ex)
                {

                    Debug.LogError($"发送异常：{ex.Message}");
                    Close();
                }

            }
        }

        public async void SendACK(BufferEntity ackPackage, IPEndPoint endPoint)
        {
            Send(ackPackage.buffer, endPoint);
        }

        public async void Receive()
        {
            if (socket!=null)
            {
                try
                {
                    UdpReceiveResult result = await socket.ReceiveAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"接受异常：{ex.Message}");
                    Close();
                }
            }
        }

        CancellationTokenSource ct = new CancellationTokenSource();
        async Task Handle()
        {
            while(!ct.IsCancellationRequested)
            {
                if (awaitHandle.Count<=0)
                {
                    continue;
                }
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    BufferEntity bufferEntity = new BufferEntity(data.RemoteEndPoint, data.Buffer);
                    if (bufferEntity.isFull)
                    {
                        //会话ID来进行查询
                        if (bufferEntity.session == 0)
                        {
                            //客户端还未建立连接 分配会话ID
                            sessionID++;
                            //创建客户端 给这个客户端分配会话ID
                            bufferEntity.session = sessionID;
                            CreateUClient(bufferEntity);
                        }
                        UClient targetClient;
                        //获取到客户端
                        if (clients.TryGetValue(bufferEntity.session, out targetClient))
                        {
                            targetClient.Handle(bufferEntity);
                        }
                    }
                }
            }

        }

        //创建客户端建立虚拟连接的端口
        void CreateUClient(BufferEntity buffer)
        {
            UClient client;
            EndPoint endPoint = null;
            if (clients.TryGetValue(buffer.session, out client))
            {
                client = new UClient(this, buffer.endPoint, 0, 0, buffer.session, dispatchNetEvent);
                clients.TryAdd(buffer.session, client);
            }
        }

        public UClient GetClient(int sessionID)
        {
            UClient client;
            if (clients.TryGetValue(sessionID, out client))
            {
                return client;
            }
            return null;
        }

        public void RemoveClient(int sessionID)
        {
            UClient client;
            if (clients.TryRemove(sessionID, out client))
            {
                client.Close();
                client = null;
            }
        }

        public void Close()
        {
            //取消任务的信号
            ct.Cancel();
            if (socket!=null)
            {
                socket.Close();
                socket = null;
            }
            if (dispatchNetEvent!=null)
            {
                dispatchNetEvent = null;
            }
            //所有的客户端都清理掉
            foreach(var client in clients.Values)
            {
                client.Close();
            }
            clients.Clear();
        }
    }
}
