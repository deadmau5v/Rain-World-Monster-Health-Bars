using Menu.Remix.MixedUI;
using UnityEngine;

namespace Monster_Health_Bars
{
    public class HealthBarConfig : OptionInterface
    {
        public static Configurable<bool> EnableHealthBars;
        public static Configurable<bool> ShowPlayerHealthBar;
        public static Configurable<int> BarWidth;
        public static Configurable<int> BarHeight;
        public static Configurable<int> MaxDistance;

        // 多语言文本
        private static bool IsChinese => Custom.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese;

        private static string Translate(string english, string chinese)
        {
            return IsChinese ? chinese : english;
        }

        public HealthBarConfig()
        {
            // 初始化配置项
            EnableHealthBars = config.Bind("EnableHealthBars", true, new ConfigurableInfo(
                Translate("Enable health bars display for all creatures", "为所有生物启用血条显示"),
                null, "", Translate("Enable Health Bars", "启用血条")));

            ShowPlayerHealthBar = config.Bind("ShowPlayerHealthBar", false, new ConfigurableInfo(
                Translate("Show health bar above the player", "在玩家上方显示血条"),
                null, "", Translate("Show Player Health Bar", "显示玩家血条")));

            BarWidth = config.Bind("BarWidth", 40, new ConfigurableInfo(
                Translate("Width of the health bar in pixels", "血条宽度(像素)"),
                new ConfigAcceptableRange<int>(20, 100), "", Translate("Bar Width", "血条宽度")));

            BarHeight = config.Bind("BarHeight", 4, new ConfigurableInfo(
                Translate("Height of the health bar in pixels", "血条高度(像素)"),
                new ConfigAcceptableRange<int>(2, 10), "", Translate("Bar Height", "血条高度")));

            MaxDistance = config.Bind("MaxDistance", 800, new ConfigurableInfo(
                Translate("Maximum distance to show health bars (fade out beyond this)", "显示血条的最大距离(超过后淡出)"),
                new ConfigAcceptableRange<int>(400, 1600), "", Translate("Max Distance", "最大距离")));
        }

        public override void Initialize()
        {
            base.Initialize();

            var opTab = new OpTab(this, Translate("Settings", "设置"));
            Tabs = new[] { opTab };

            float yPos = 550f;

            // 标题
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Monster Health Bars Settings", "怪物血条设置"), true)
            );
            yPos -= 40f;

            // 启用/禁用复选框
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Enable Health Bars:", "启用血条:")),
                new OpCheckBox(EnableHealthBars, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            // 显示玩家血条复选框
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Show Player Health Bar:", "显示玩家血条:")),
                new OpCheckBox(ShowPlayerHealthBar, new Vector2(250f, yPos))
            );
            yPos -= 60f;

            // 血条宽度滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Bar Width (20-100):", "血条宽度 (20-100):")),
                new OpSlider(BarWidth, new Vector2(250f, yPos), 200, false)
            );
            yPos -= 40f;

            // 血条高度滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Bar Height (2-10):", "血条高度 (2-10):")),
                new OpSlider(BarHeight, new Vector2(250f, yPos), 200, false)
            );
            yPos -= 40f;

            // 最大距离滑块
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Max Distance (400-1600):", "最大距离 (400-1600):")),
                new OpSlider(MaxDistance, new Vector2(250f, yPos), 200, false)
            );
            yPos -= 60f;

            // 使用说明
            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Health bars will appear above all creatures in the game.", "血条会显示在游戏中所有生物的上方。"))
            );
            yPos -= 20f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("The color changes from green to yellow to red based on health.", "颜色会根据血量从绿色变为黄色再变为红色。"))
            );
            yPos -= 20f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("Health bars fade out as creatures move away from the player.", "当生物远离玩家时,血条会逐渐淡出。"))
            );
        }
    }
}
