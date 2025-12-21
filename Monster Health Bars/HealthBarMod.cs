using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Monster_Health_Bars
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class HealthBarMod : BaseUnityPlugin
    {
        public const string PluginGuid = "d5v.healthbar";
        public const string PluginName = "Monster Health Bars";
        public const string PluginVersion = "1.0.0";

        public static new ManualLogSource Logger;
        public static bool ShowHealthBars = true;
        
        private bool _isInit;

        public void OnEnable()
        {
            Logger = base.Logger;
            
            if (_isInit) return;
            _isInit = true;

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
            if (!_isInit) return;
            _isInit = false;

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
                ShowHealthBars = !ShowHealthBars;
                Logger.LogInfo($"Health bars: {(ShowHealthBars ? "Visible" : "Hidden")}");
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
            
            if (ShowHealthBars && self.owner is Player player && player.room != null)
            {
                HealthBarManager.DrawHealthBars(player.room, player.room.game.cameras[0], timeStacker);
            }
        }
    }
}
