using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;
using System.Collections.Generic;

namespace Monster_Health_Bars
{
    public class HealthBarConfig : OptionInterface
    {
        public static Configurable<bool> EnableHealthBars;
        public static Configurable<bool> ShowPlayerHealthBar;
        public static Configurable<bool> HideWhenFullHealth;
        public static Configurable<int> BarWidth;
        public static Configurable<int> BarHeight;
        public static Configurable<int> MaxDistance;

        // 多语言文本
        private static new string Translate(string key)
        {
            var lang = Custom.rainWorld.inGameTranslator.currentLanguage;

            var translations = new Dictionary<InGameTranslator.LanguageID, Dictionary<string, string>>
            {
                {
                    InGameTranslator.LanguageID.English, new Dictionary<string, string>
                    {
                        {"mod_name", "Monster Health Bars Settings"},
                        {"enable_bars", "Enable Health Bars:"},
                        {"show_player", "Show Player Health Bar:"},
                        {"hide_full", "Hide When Full Health:"},
                        {"bar_width", "Bar Width (20-100):"},
                        {"bar_height", "Bar Height (2-10):"},
                        {"max_distance", "Max Distance (400-1600):"},
                        {"desc1", "Health bars will appear above all creatures in the game."},
                        {"desc2", "The color changes from green to yellow to red based on health."},
                        {"desc3", "Health bars fade out as creatures move away from the player."},
                        {"settings", "Settings"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Chinese, new Dictionary<string, string>
                    {
                        {"mod_name", "怪物血条设置"},
                        {"enable_bars", "启用血条:"},
                        {"show_player", "显示玩家血条:"},
                        {"hide_full", "满血时隐藏:"},
                        {"bar_width", "血条宽度 (20-100):"},
                        {"bar_height", "血条高度 (2-10):"},
                        {"max_distance", "最大距离 (400-1600):"},
                        {"desc1", "血条会显示在游戏中所有生物的上方。"},
                        {"desc2", "颜色会根据血量从绿色变为黄色再变为红色。"},
                        {"desc3", "当生物远离玩家时,血条会逐渐淡出。"},
                        {"settings", "设置"}
                    }
                }
            };

            // 默认使用英文
            if (!translations.ContainsKey(lang))
            {
                lang = InGameTranslator.LanguageID.English;
            }

            return translations[lang].ContainsKey(key) ? translations[lang][key] : key;
        }

        public HealthBarConfig()
        {
            // 初始化配置项
            EnableHealthBars = config.Bind("EnableHealthBars", true, new ConfigurableInfo(
                "Enable health bars display for all creatures",
                null, "", "Enable Health Bars"));

            ShowPlayerHealthBar = config.Bind("ShowPlayerHealthBar", false, new ConfigurableInfo(
                "Show health bar above the player",
                null, "", "Show Player Health Bar"));

            HideWhenFullHealth = config.Bind("HideWhenFullHealth", true, new ConfigurableInfo(
                "Hide health bars when creature is at full health",
                null, "", "Hide When Full Health"));

            BarWidth = config.Bind("BarWidth", 40, new ConfigurableInfo(
                "Width of the health bar in pixels",
                new ConfigAcceptableRange<int>(20, 100), "", "Bar Width"));

            BarHeight = config.Bind("BarHeight", 4, new ConfigurableInfo(
                "Height of the health bar in pixels",
                new ConfigAcceptableRange<int>(2, 10), "", "Bar Height"));

            MaxDistance = config.Bind("MaxDistance", 800, new ConfigurableInfo(
                "Maximum distance to show health bars (fade out beyond this)",
                new ConfigAcceptableRange<int>(400, 1600), "", "Max Distance"));
        }

        public override void Initialize()
        {
            base.Initialize();

            var opTab = new OpTab(this, Translate("settings"));
            Tabs = new[] { opTab };

            float yPos = 550f;

            // 标题
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("mod_name"), true)
            );
            yPos -= 40f;

            // 启用/禁用复选框
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("enable_bars")),
                new OpCheckBox(EnableHealthBars, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            // 显示玩家血条复选框
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("show_player")),
                new OpCheckBox(ShowPlayerHealthBar, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            // 满血时隐藏复选框
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("hide_full")),
                new OpCheckBox(HideWhenFullHealth, new Vector2(250f, yPos))
            );
            yPos -= 60f;

            // 血条宽度滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("bar_width")),
                new OpSlider(BarWidth, new Vector2(250f, yPos), 200)
            );
            yPos -= 40f;

            // 血条高度滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("bar_height")),
                new OpSlider(BarHeight, new Vector2(250f, yPos), 200)
            );
            yPos -= 40f;

            // 最大距离滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("max_distance")),
                new OpSlider(MaxDistance, new Vector2(250f, yPos), 200)
            );
            yPos -= 60f;

            // 使用说明
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("desc1"))
            );
            yPos -= 20f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("desc2"))
            );
            yPos -= 20f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("desc3"))
            );
        }
    }
}
