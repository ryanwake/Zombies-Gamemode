using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace ZombiesBaseClient
{
    class HUD : BaseScript
    {
        private static int ping = 0;
        public HUD()
        {
            Tick += HUDTick;
            Tick += PingTick;
            EventHandlers.Add("basezombies:setPing", new Action<int>(SetPing));
        }

        private async Task PingTick()
        {
            await Delay(5000);
            TriggerServerEvent("basezombies:getPing");
        }

        private void SetPing(int obj)
        {
            ping = obj;
        }

        private async Task HUDTick()
        {
            await Delay(100);
            
            while (true)
            {
                
                await Delay(10);
                if (!(MainClient.Waiting))
                {
                    DrawTxt(0.675, 1.45, 1.0, 1.0, 0.45f, String.Format("ID: ~b~{0}~b~ ~w~Ping:~w~ ~b~{1}~b~", Game.Player.ServerId, ping), 255, 255, 255, 255);
                    Vector3 coords = new Vector3(-456.28f, -1714.55f, 18.64f);
                    Vector3 playerCoords = GetEntityCoords(GetPlayerPed(-1), false);

                    if (Vector3.Distance(coords, playerCoords) < 40) {
                        DrawTxt(0.555, 1.275, 1.0, 1.0, 0.45f, "INSIDE SAFEZONE", 255, 255, 255, 255);
                    }
                }


            }
        }

        private void DrawTxt(double x, double y, double width, double height, float scale, string text, int r, int g, int b, int a)
        {
            SetTextFont(4);
            SetTextProportional(false);
            SetTextScale(scale, scale);
            SetTextColour(r, g, b, a);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(2, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            AddTextComponentString(text);
            DrawText((float)(x - width / 2), (float)(y - height / 2 + 0.005));
        }
    }
}
