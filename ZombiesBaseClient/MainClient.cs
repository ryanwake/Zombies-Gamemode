using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Linq;
using System.Dynamic;
using System.ComponentModel;

namespace ZombiesBaseClient
{
    public class MainClient : BaseScript
    {
        private bool waiting = true;
        private List<dynamic> playerData = new List<dynamic>();
        
        public static bool Waiting { get; set; }

        public MainClient()
        {
            RegisterNuiCallbackType("LoadGame");
            EventHandlers.Add("onClientResourceStart", new Action<string>(OnClientResourceStart));
            EventHandlers.Add("basezombies:serverReady", new Action<List<dynamic>>(OnServerReady));
            EventHandlers["baseevents:onPlayerKilled"] += new Action<int, ExpandoObject>(OnPlayerKilled);
            EventHandlers["baseevents:onPlayerDied"] += new Action<int, List<dynamic>>(OnPlayerKilled);
            EventHandlers["baseevents:onPlayerWasted"] += new Action<int, List<dynamic>>(OnPlayerKilled);
            EventHandlers["__cfx_nui:LoadGame"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                LoadGame();
                cb("ok");
            });

        }

        private void OnPlayerKilled(int killedby, List<dynamic> data)
        {
            Vector3 coords = GetEntityCoords(GetPlayerPed(-1), false);
            NetworkResurrectLocalPlayer(coords.X, coords.Y, coords.Z, 0, false, false);
        }

        private void OnPlayerKilled(int killedby, ExpandoObject data)
        {
            Vector3 coords = GetEntityCoords(GetPlayerPed(-1), false);
            NetworkResurrectLocalPlayer(coords.X, coords.Y, coords.Z, 0, false, false);
        }

        private void OnServerReady(List<dynamic> data)
        {
            playerData = data.ToList();
            
            //Debug.WriteLine(playerData.FirstOrDefault().coords);
            /*foreach (var obj in playerData)
            {
                Debug.WriteLine(obj.coords.ToString());

                //Debug.WriteLine(obj.ToString());
            }*/
            waiting = false;
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            Exports["spawnmanager"].setAutoSpawn(false);
            RegisterCommand("car", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                var model = "adder";
                if (args.Count > 0)
                {
                    model = args[0].ToString();
                }

                var hash = (uint)GetHashKey(model);
                if(!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                {
                    TriggerEvent("chat:addMessage", new
                    {
                        color = new[] { 255, 0, 0 },
                        args = new[] { "[CarSpawner]", $"It might have been a good thing that you tried to spawn a {model}. Who even wants their spawning to actually ^*succeed?" }
                    });
                    return;
                }

                var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);

                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);

                TriggerEvent("chat:addMessage", new
                {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "[CarSpawner]", $"Woohoo! Enjoy your new ^*{model}!" }
                });

            }), false);
            SetNuiFocus(true, true);
            LoadingSetup();
            StartWelcomeUI();
            HUD h = new HUD();
            waiting = true;
            Tick += OnTick;
        }

        private void StartWelcomeUI()
        {
            Debug.WriteLine("load ui");
            SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openui"
            }));
        }

        private void LoadingSetup()
        {
            TriggerServerEvent("basezombies:playerJoined");
            SetTimecycleModifier("hud_def_blur");
            FreezeEntityPosition(GetPlayerPed(-1), true);
            var cam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", -100.92f, -1000.78f, 320.75f, 300.00f, 0.00f, 100.00f, 120.00f, false, 0);
            SetCamActive(cam, true);
            RenderScriptCams(true, false, 1, true, true);
        }

        private void LoadGame()
        {
            SetNuiFocus(false, false);
            DoScreenFadeOut(500);

            string coords = playerData.FirstOrDefault().coords;
            float[] newCoords = coords.Split(new string[] { ", " }, StringSplitOptions.None).Select(x => float.Parse(x)).ToArray();
            
            SetTimecycleModifier("default");
            SetEntityCoords(GetPlayerPed(-1), newCoords[0], newCoords[1], newCoords[2], true, false, false, true);
            DoScreenFadeIn(500);
            var cam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", -100.92f, -1000.78f, 320.75f, 300.00f, 0.00f, 100.00f, 120.00f, false, 0);
            var cam2 = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", -100.93f, -1000.78f, 320.75f, 300.00f, 0.00f, 100.00f, 120.00f, false, 0);
            PointCamAtCoord(cam2, 42.0f, 266.4f, 109.6f + 200);
            SetCamActiveWithInterp(cam2, cam, 900, 1, 1);
            PlaySoundFrontend(-1, "Zoom_Out", "DLC_HEIST_PLANNING_BOARD_SOUNDS", true);
            RenderScriptCams(false, true, 500, true, true);
            PlaySoundFrontend(-1, "CAR_BIKE_WHOOSH", "MP_LOBBY_SOUNDS", true);
            FreezeEntityPosition(GetPlayerPed(-1), false);
            SetCamActive(cam, false);
            DestroyCam(cam, true);
            DestroyCam(cam2, true);
            DisplayHud(true);
            DisplayRadar(true);
            SetupSafeZones();

            Tick += PlayerSyncTick;
            
            return;
        }

        private void SetupSafeZones()
        {
            var blip = AddBlipForRadius(-456.28f, -1714.55f, 18.64f, 45.0f);
            SetBlipHighDetail(blip, true);
            SetBlipColour(blip, 2);
            SetBlipAlpha(blip, 128);
        }

        private async Task PlayerSyncTick()
        {
            await Delay(5*60*1000);
            Vector3 coords = GetEntityCoords(GetPlayerPed(-1), false);
            TriggerServerEvent("basezombies:syncPlayerPos", coords);
        }

        private async Task OnTick()
        {
            if (waiting)
            {
                do
                {
                    await Delay(100);
                    Debug.WriteLine(waiting.ToString());
                    DisplayHud(false);
                    DisplayRadar(false);
                    SetNuiFocus(true, true);
                } while (waiting);
            }

            var ped = GetPlayerPed(-1);
            Vector3 coords = new Vector3(-456.28f, -1714.55f, 18.64f);
            float distance = Vector3.Distance(coords, GetEntityCoords(ped, false));

            if (distance < 40.0f)
            {
                SetPlayerInvincible(Game.Player.ServerId, true);
                //Debug.WriteLine("inside");
            } else
            {
                SetPlayerInvincible(Game.Player.ServerId, false);
                //Debug.WriteLine("outside");
            }

            await Delay(1000);
        }
    }
}
