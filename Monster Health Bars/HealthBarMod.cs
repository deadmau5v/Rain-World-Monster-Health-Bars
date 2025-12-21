using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Monster_Health_Bars
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class HealthBarMod : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "d5v.healthbar";
        public const string PLUGIN_NAME = "Monster Health Bars";
        public const string PLUGIN_VERSION = "1.0.0";

        public static new ManualLogSource Logger;
        public static bool showHealthBars = true;
        
        private bool isInit = false;

        public void OnEnable()
        {
            Logger = base.Logger;
            
            if (isInit) return;
            isInit = true;

            Logger.LogInfo("Health Bar Mod loaded!");

            // 订阅钩子
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            
            // 注册更新和绘制钩子
            On.RoomCamera.Update += RoomCamera_Update;
            On.HUD.HUD.Draw += HUD_Draw;
        }

        public void OnDisable()
        {
            if (!isInit) return;
            isInit = false;

            // 取消订阅
            On.RainWorldGame.ShutDownProcess -= RainWorldGame_ShutDownProcess;
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            On.RoomCamera.Update -= RoomCamera_Update;
            On.HUD.HUD.Draw -= HUD_Draw;
        }

        // 修正方法签名 - RoomCamera.Update 不接受参数
        private void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);
            
            // 检测 Tab 键切换显示
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                showHealthBars = !showHealthBars;
                Logger.LogInfo($"Health bars: {(showHealthBars ? "Visible" : "Hidden")}");
            }
        }

        private void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            HealthBarManager.ClearAll();
        }

        private void HUD_Draw(On.HUD.HUD.orig_Draw orig, HUD.HUD self, float timeStacker)
        {
            orig(self, timeStacker);
            
            if (showHealthBars && self.owner is Player player && player.room != null)
            {
                HealthBarManager.DrawHealthBars(player.room, player.room.game.cameras[0], timeStacker);
            }
        }
    }
}
