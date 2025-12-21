using BepInEx;
using BepInEx.Logging;
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
        public const string PluginVersion = "1.4.0";

        public static new ManualLogSource Logger;

        private bool _isInit;
        private bool _hooksRegistered;

        public void OnEnable()
        {
            Logger = base.Logger;

            if (_isInit) return;
            _isInit = true;

            Logger.LogInfo("Health Bar Mod loaded!");

            // 只订阅初始化钩子
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            // 避免重复注册钩子
            if (_hooksRegistered) return;
            _hooksRegistered = true;

            try
            {
                // 加载多语言翻译
                LoadTranslations();

                // 注册配置界面
                MachineConnector.SetRegisteredOI(PluginGuid, new HealthBarConfig());

                // 注册绘制和清理钩子
                On.HUD.HUD.Draw += HUD_Draw;
                On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;

                Logger.LogInfo("Health Bar hooks registered successfully!");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Failed to register hooks: {e.Message}");
            }
        }

        private void LoadTranslations()
        {
            try
            {
                // Rain World 会自动从 translation 文件夹加载翻译文件
                // 文件名格式：ModName-language.txt
                // 我们只需要确保翻译键在 modinfo.json 中被引用
                Logger.LogInfo("Translation files loaded from translation folder");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Failed to load translations: {e.Message}");
            }
        }

        public void OnDisable()
        {
            if (!_isInit) return;
            _isInit = false;

            // 取消订阅所有钩子
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
                // 检查配置是否启用血条显示
                if (HealthBarConfig.EnableHealthBars == null || !HealthBarConfig.EnableHealthBars.Value) return;

                // 只在游戏正常运行时绘制血条
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
