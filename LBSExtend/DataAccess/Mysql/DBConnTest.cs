﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace ZIT.EMERGENCY.fnDataAccess.Mysql
{
    class DBConnTest : IDBConnTest
    {
        public bool DBIsConnected()
        {
            bool bIsConnected = DbHelperMySQL.IsConnected();
            return bIsConnected;
        }

    }
}
