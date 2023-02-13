using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.DBCMD
{
    public class DBBase<T> where T:new()
    {
        public static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }


    }
}
