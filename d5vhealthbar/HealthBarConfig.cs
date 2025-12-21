using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;
using System.Collections.Generic;

namespace d5vhealthbar
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
                        {"mod_name", "d5vhealthbar Settings"},
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
                },
                {
                    InGameTranslator.LanguageID.Japanese, new Dictionary<string, string>
                    {
                        {"mod_name", "モンスター体力バー設定"},
                        {"enable_bars", "体力バーを有効化:"},
                        {"show_player", "プレイヤーの体力バーを表示:"},
                        {"hide_full", "満タン時に非表示:"},
                        {"bar_width", "バー幅 (20-100):"},
                        {"bar_height", "バー高さ (2-10):"},
                        {"max_distance", "最大距離 (400-1600):"},
                        {"desc1", "ゲーム内のすべてのクリーチャーの上に体力バーが表示されます。"},
                        {"desc2", "体力に応じて緑から黄色、赤へと色が変わります。"},
                        {"desc3", "クリーチャーがプレイヤーから離れるにつれてバーがフェードアウトします。"},
                        {"settings", "設定"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Korean, new Dictionary<string, string>
                    {
                        {"mod_name", "몬스터 체력 바 설정"},
                        {"enable_bars", "체력 바 활성화:"},
                        {"show_player", "플레이어 체력 바 표시:"},
                        {"hide_full", "체력 만땅일 때 숨기기:"},
                        {"bar_width", "바 너비 (20-100):"},
                        {"bar_height", "바 높이 (2-10):"},
                        {"max_distance", "최대 거리 (400-1600):"},
                        {"desc1", "모든 생명체 위에 체력 바가 표시됩니다."},
                        {"desc2", "체력에 따라 녹색에서 노란색, 빨간색으로 색이 변합니다."},
                        {"desc3", "생명체가 플레이어로부터 멀어질수록 바가 점점 사라집니다."},
                        {"settings", "설정"}
                    }
                },
                {
                    InGameTranslator.LanguageID.French, new Dictionary<string, string>
                    {
                        {"mod_name", "Paramètres des Barres de Vie"},
                        {"enable_bars", "Activer les barres de vie:"},
                        {"show_player", "Afficher la barre du joueur:"},
                        {"hide_full", "Masquer à pleine vie:"},
                        {"bar_width", "Largeur de barre (20-100):"},
                        {"bar_height", "Hauteur de barre (2-10):"},
                        {"max_distance", "Distance maximale (400-1600):"},
                        {"desc1", "Les barres de vie apparaissent au-dessus de toutes les créatures."},
                        {"desc2", "La couleur change du vert au jaune puis au rouge selon la vie."},
                        {"desc3", "Les barres s'estompent lorsque les créatures s'éloignent du joueur."},
                        {"settings", "Paramètres"}
                    }
                },
                {
                    InGameTranslator.LanguageID.German, new Dictionary<string, string>
                    {
                        {"mod_name", "Monster-Lebensbalken Einstellungen"},
                        {"enable_bars", "Lebensbalken aktivieren:"},
                        {"show_player", "Spieler-Lebensbalken anzeigen:"},
                        {"hide_full", "Bei voller Gesundheit ausblenden:"},
                        {"bar_width", "Balkenbreite (20-100):"},
                        {"bar_height", "Balkenhöhe (2-10):"},
                        {"max_distance", "Maximale Entfernung (400-1600):"},
                        {"desc1", "Lebensbalken erscheinen über allen Kreaturen im Spiel."},
                        {"desc2", "Die Farbe wechselt von Grün über Gelb zu Rot je nach Gesundheit."},
                        {"desc3", "Balken verblassen, wenn sich Kreaturen vom Spieler entfernen."},
                        {"settings", "Einstellungen"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Spanish, new Dictionary<string, string>
                    {
                        {"mod_name", "Configuración de Barras de Vida"},
                        {"enable_bars", "Activar barras de vida:"},
                        {"show_player", "Mostrar barra del jugador:"},
                        {"hide_full", "Ocultar con vida completa:"},
                        {"bar_width", "Ancho de barra (20-100):"},
                        {"bar_height", "Alto de barra (2-10):"},
                        {"max_distance", "Distancia máxima (400-1600):"},
                        {"desc1", "Las barras de vida aparecen sobre todas las criaturas del juego."},
                        {"desc2", "El color cambia de verde a amarillo y luego a rojo según la vida."},
                        {"desc3", "Las barras se desvanecen cuando las criaturas se alejan del jugador."},
                        {"settings", "Configuración"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Italian, new Dictionary<string, string>
                    {
                        {"mod_name", "Impostazioni Barre Vita"},
                        {"enable_bars", "Attiva barre vita:"},
                        {"show_player", "Mostra barra del giocatore:"},
                        {"hide_full", "Nascondi a vita piena:"},
                        {"bar_width", "Larghezza barra (20-100):"},
                        {"bar_height", "Altezza barra (2-10):"},
                        {"max_distance", "Distanza massima (400-1600):"},
                        {"desc1", "Le barre vita appaiono sopra tutte le creature nel gioco."},
                        {"desc2", "Il colore cambia da verde a giallo a rosso in base alla vita."},
                        {"desc3", "Le barre svaniscono quando le creature si allontanano dal giocatore."},
                        {"settings", "Impostazioni"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Portuguese, new Dictionary<string, string>
                    {
                        {"mod_name", "Configurações de Barras de Vida"},
                        {"enable_bars", "Ativar barras de vida:"},
                        {"show_player", "Mostrar barra do jogador:"},
                        {"hide_full", "Ocultar com vida cheia:"},
                        {"bar_width", "Largura da barra (20-100):"},
                        {"bar_height", "Altura da barra (2-10):"},
                        {"max_distance", "Distância máxima (400-1600):"},
                        {"desc1", "Barras de vida aparecem acima de todas as criaturas no jogo."},
                        {"desc2", "A cor muda de verde para amarelo e depois vermelho conforme a vida."},
                        {"desc3", "As barras desaparecem quando as criaturas se afastam do jogador."},
                        {"settings", "Configurações"}
                    }
                },
                {
                    InGameTranslator.LanguageID.Russian, new Dictionary<string, string>
                    {
                        {"mod_name", "Настройки Полосок Здоровья"},
                        {"enable_bars", "Включить полоски здоровья:"},
                        {"show_player", "Показать полоску игрока:"},
                        {"hide_full", "Скрывать при полном здоровье:"},
                        {"bar_width", "Ширина полоски (20-100):"},
                        {"bar_height", "Высота полоски (2-10):"},
                        {"max_distance", "Максимальное расстояние (400-1600):"},
                        {"desc1", "Полоски здоровья появляются над всеми существами в игре."},
                        {"desc2", "Цвет меняется с зелёного на жёлтый и красный в зависимости от здоровья."},
                        {"desc3", "Полоски исчезают, когда существа удаляются от игрока."},
                        {"settings", "Настройки"}
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
