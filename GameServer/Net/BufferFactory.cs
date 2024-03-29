﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Net
{
    class BufferFactory
    {
        enum MessageType
        {
            ACK = 0, //确认报文
            Logic = 1, //业务逻辑的报文
        }
        /// <summary>
        /// 创建并且发送报文
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="message"></param>
        public static BufferEntity CreateAndSendPackage(UClient uCLient, int messageID, IMessage message)
        {
            if (uCLient.isConnect)
            {
                //按json格式打印json
                Debug.Log(messageID, message);
                BufferEntity buffer = new BufferEntity(uCLient.endPoint, uCLient.session,
                0, 0, MessageType.Logic.GetHashCode(), messageID, ProtobufHelper.ToBytes(message));
                uCLient.Send(buffer);
                return buffer;
            }
            
            return null;
        }

        public static BufferEntity CreateAndSendPackage(BufferEntity req, IMessage message)
        {
            UClient client = GameMgr.uSocket.GetClient(req.session);
            return CreateAndSendPackage(client, req.messageID, message);
        }
    }
}
