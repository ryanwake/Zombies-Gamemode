using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using System.Runtime.CompilerServices;
using ZombiesBaseServer.MySQL;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace ZombiesBaseServer
{
    public class MainServer : BaseScript
    {
        private readonly List<int> allowedEntities = new List<int>()
        {
            225514697,
            -1883002148,
            -2076478498
        };

        public MainServer()
        {
            EventHandlers.Add("onServerResourceStart", new Action<string>(OnServerResourceStart));
            EventHandlers.Add("basezombies:playerJoined", new Action<Player>(OnPlayerJoined));
            EventHandlers.Add("basezombies:getPlayerData", new Action<Player>(GetPlayerData));
            EventHandlers.Add("basezombies:syncPlayerPos", new Action<Player, Vector3>(SyncPlayerCoords));
            EventHandlers.Add("basezombies:getPing", new Action<Player>(GetPing));
            EventHandlers.Add("entityCreating", new Action<int>(EntityCreating));
            //EventHandlers.Add("baseevents:onPlayerKilled", new Action<Player, string, string>(OnPlayerKilled));
        }

        private void GetPing([FromSource] Player player)
        {
            int ping = player.Ping;
            player.TriggerEvent("basezombies:setPing", ping);
        }

        /*private void OnPlayerKilled([FromSource] Player player, string killedBy, string data)
        {
            Debug.WriteLine(String.Format("{0} killed", player.Name));
        }*/

        private async Task CheckDeadPlayers()
        {
            PlayerList pl = Players;

            foreach (Player ply in pl)
            {
                
            }
            await Delay(100);
        }

        private void EntityCreating(int entity)
        {
            int id = allowedEntities.IndexOf(GetEntityModel(entity));
            //Debug.WriteLine(id.ToString());
            if (id >= 0)
                return;

            int owner = NetworkGetEntityOwner(entity);
            //string player = (owner.ToString());

            //CancelEvent();
            //DropPlayer(owner.ToString(), String.Format("[{0}] Attempting to spawn in item.", GetCurrentResourceName()));
            //Debug.WriteLine(String.Format("{0} tried creating a", GetEntityModel(entity)));
        }

        private void SyncPlayerCoords([FromSource] Player source, Vector3 coords)
        {
            //Vector3 playerCoords = GetEntityCoords(source.Character.NetworkId);
            string formattedCoords = String.Format("{0}, {1}, {2}", coords.X, coords.Y, coords.Z);
            string license = "";

            source.Identifiers.ToList().ForEach(e =>
            {
                if (e.Contains("license:"))
                    license = e;
            });

            MySQLHandler.Instance.Execute("UPDATE users SET coords = @coords WHERE license = @license", new Dictionary<string, object>()
            {
                ["@coords"] = formattedCoords,
                ["@license"] = license
            }, new Action<int>(_ => { }));
        }

        private void GetPlayerData(Player source)
        {
            string license = "";
            string ip = "";
            DateTime currentDateTime = DateTime.Now;
            string sqlFormatted = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            

            source.Identifiers.ToList().ForEach(e =>
            {
                if (e.Contains("license:"))
                    license = e;
                else if (e.Contains("ip:"))
                    ip = e;
            });

            var pars = new Dictionary<string, object>()
            {
                ["@license"] = license
            };

            MySQLHandler.Instance?.FetchAll("SELECT * from users WHERE license=@license", pars, new Action<List<dynamic>>((objs) =>
            {
                if (objs.Count == 0)
                {
                    MySQLHandler.Instance.Execute("INSERT INTO users (name, license, lastip, lastdate, lastdeath, coords) VALUES (@name, @license, @lastip, @lastdate, @lastdeath, @coords)", new Dictionary<string, object>()
                    {
                        ["@name"] = source.Name,
                        ["@license"] = license,
                        ["@lastip"] = ip,
                        ["@lastdate"] = sqlFormatted,
                        ["@lastdeath"] = sqlFormatted,
                        ["@coords"] =  "195.55, -933.36, 29.90"
                    }, new Action<int>(_ => { }));
                } else
                {
                    foreach (var obj in objs)
                    {
                        Debug.WriteLine(obj?.coords.ToString());
                    }
                    MySQLHandler.Instance.Execute("UPDATE users SET lastdate=@lastdate WHERE license=@license", new Dictionary<string, object>()
                    {
                        ["@lastdate"] = sqlFormatted,
                        ["@license"] = license
                    }, new Action<int>(_ => { }));
                    TriggerClientEvent(source, "basezombies:serverReady", objs);
                }
            }));
        }

        private void OnPlayerJoined([FromSource] Player source)
        {
            GetPlayerData(source);
        }

        private void OnServerResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine("[{0}] Loaded!", resourceName);
        }
    }
}
