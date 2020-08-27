using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace ZombiesBaseServer.MySQL
{
    class MySQLHandler : BaseScript
    {
        public static MySQLHandler Instance { get; set; }

        public MySQLHandler()
        {
            Instance = this;
        }

        public void FetchAll(string query, Dictionary<string, object> pars, Action<List<dynamic>> action)
        {
            Exports["mysql-async"].mysql_fetch_all(query, pars, action);
        }

        public void Execute(string query, Dictionary<string, object> pars, Action<int> action)
        {
            Exports["mysql-async"].mysql_execute(query, pars, action);
        }
    }
}
