using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace d5vhealthbar
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class HealthBarMod : BaseUnityPlugin
    {
        public const string PluginGuid = "d5vhealthbar";
        public const string PluginName = "d5vhealthbar";
        public const string PluginVersion = "1.5.0";

        public static new ManualLogSource Logger;

        private bool _isInit;
        private bool _hooksRegistered;

        public void OnEnable()
        {
            Logger = base.Logger;

            if (_isInit) return;
            _isInit = true;

            Logger.LogInfo("Health Bar Mod loaded!");

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (_hooksRegistered) return;
            _hooksRegistered = true;

            try
            {
                MachineConnector.SetRegisteredOI(PluginGuid, new HealthBarConfig());

                On.HUD.HUD.Draw += HUD_Draw;
                On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;

                Logger.LogInfo("Health Bar hooks registered successfully!");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Failed to register hooks: {e.Message}");
            }
        }

        public void OnDisable()
        {
            if (!_isInit) return;
            _isInit = false;

            try
            {
                On.RainWorld.OnModsInit -= RainWorld_OnModsInit;

                if (_hooksRegistered)
                {
                    On.HUD.HUD.Draw -= HUD_Draw;
                    On.RainWorldGame.ShutDownProcess -= RainWorldGame_ShutDownProcess;
                    _hooksRegistered = false;
                }

                HealthBarManager.ClearAll();
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error during cleanup: {e.Message}");
            }
        }

        private void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);

            try
            {
                HealthBarManager.ClearAll();
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error in ShutDownProcess: {e.Message}");
            }
        }

        private void HUD_Draw(On.HUD.HUD.orig_Draw orig, HUD.HUD self, float timeStacker)
        {
            orig(self, timeStacker);

            try
            {
                if (HealthBarConfig.EnableHealthBars == null || !HealthBarConfig.EnableHealthBars.Value) return;

                if (self == null || self.owner == null) return;

                Player player = self.owner as Player;
                if (player == null || player.room == null) return;
                if (player.room.game == null || player.room.game.cameras == null || player.room.game.cameras.Length == 0) return;

                RoomCamera camera = player.room.game.cameras[0];
                if (camera == null || camera.hud == null) return;

                HealthBarManager.DrawHealthBars(player.room, camera, timeStacker);
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error in HUD_Draw: {e.Message}");
            }
        }
    }
}
