using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Player
{
    class PlayerMgr
    {
        public static ConcurrentDictionary<int, PlayerEntity> playerList 
            = new ConcurrentDictionary<int, PlayerEntity>();
        public static ConcurrentDictionary<int, PlayerEntity> playerSession
            = new ConcurrentDictionary<int, PlayerEntity>();
        public static void Add(int session, int rolesID, PlayerEntity player)
        {
            Debug.Log($"Add player: {session}, {rolesID}");
            playerList.TryAdd(rolesID, player);
            playerSession.TryAdd(session, player);
        }

        public static bool RemoveFromSession(int session)
        {
            PlayerEntity player;
            return playerSession.TryRemove(session, out player);
        }

        public static bool RemoveFromRolesID(int rolesID)
        {
            PlayerEntity player;
            return playerList.TryRemove(rolesID, out player);
        }

        public static PlayerEntity GetPlayerEntityFromSession(int session)
        {
            Debug.Log($"PE:        sessionID:{session}");
            PlayerEntity player;
            if (playerSession.TryGetValue(session, out player))
            {
                Debug.Log($"获取到了， {session}");
                return player;
            }
            else
            {
                return null;
            }
        }

        public static PlayerEntity GetPlayerEntityFromRolesID(int rolesID)
        {
            Debug.Log($"PE:        rolesID:{rolesID}");
            PlayerEntity player;
            if (playerList.TryGetValue(rolesID, out player))
            {
                Debug.Log($"获取到了， {rolesID}");
                return player;
            }
            else
            {
                return null;
            }
        }
    }
}
