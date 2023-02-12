using GameServer.Net;
using System;

namespace GameServer
{
    class Program
    {
        public static USocket uSocket;
        static void Main(string[] args)
        {
            Console.WriteLine("启动服务器。。。。。。。");
            NetSystemInit();

            Console.ReadLine();
        }

        static void NetSystemInit()
        {
            uSocket = new USocket(DispatchNetEvent);
            Debug.Log("网络系统初始化完成!");
        }

        static void DispatchNetEvent(BufferEntity buffer)
        {

            //进行报文分发
        }
    }
}
