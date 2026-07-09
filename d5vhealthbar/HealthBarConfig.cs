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
        public static Configurable<bool> OnlyShowHostile;
        public static Configurable<int> MaxDistance;
        public static Configurable<int> HealthBarScale;
        public static Configurable<int> HealthBarOpacity;

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
                        {"only_hostile", "Only Show Hostile Creatures:"},
                        {"bar_scale", "Health Bar Scale (50-150%):"},
                        {"bar_opacity", "Health Bar Opacity (10-100%):"},
                        {"max_distance", "Max Distance (400-1600):"},
                        {"desc1", "Segmented health bars appear above all creatures."},
                        {"desc2", "Green segments turn yellow/red as health decreases."},
                        {"desc3", "Adjust scale to change size, opacity for transparency."},
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
                        {"only_hostile", "只显示敌对生物:"},
                        {"bar_scale", "血条缩放 (50-150%):"},
                        {"bar_opacity", "血条透明度 (10-100%):"},
                        {"max_distance", "最大距离 (400-1600):"},
                        {"desc1", "分段式血条会显示在所有生物上方。"},
                        {"desc2", "绿色方块会随血量减少变为黄色或红色。"},
                        {"desc3", "调整缩放改变大小，调整透明度改变显示效果。"},
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
                        {"only_hostile", "敵対クリーチャーのみ表示:"},
                        {"bar_scale", "体力バースケール (50-150%):"},
                        {"bar_opacity", "体力バー透明度 (10-100%):"},
                        {"max_distance", "最大距離 (400-1600):"},
                        {"desc1", "セグメント型の体力バーがすべてのクリーチャーの上に表示されます。"},
                        {"desc2", "緑色のセグメントが体力減少に応じて黄色や赤色に変わります。"},
                        {"desc3", "スケールでサイズを調整、透明度で表示効果を調整します。"},
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
                        {"only_hostile", "적대적인 생물만 표시:"},
                        {"bar_scale", "체력 바 크기 (50-150%):"},
                        {"bar_opacity", "체력 바 투명도 (10-100%):"},
                        {"max_distance", "최대 거리 (400-1600):"},
                        {"desc1", "분할된 체력 바가 모든 생명체 위에 표시됩니다."},
                        {"desc2", "녹색 세그먼트가 체력 감소에 따라 노란색/빨간색으로 변합니다."},
                        {"desc3", "크기와 투명도를 조정하여 표시 효과를 변경합니다."},
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
                        {"only_hostile", "Afficher uniquement les créatures hostiles:"},
                        {"bar_scale", "Échelle des barres (50-150%):"},
                        {"bar_opacity", "Opacité des barres (10-100%):"},
                        {"max_distance", "Distance maximale (400-1600):"},
                        {"desc1", "Des barres de vie segmentées apparaissent au-dessus de toutes les créatures."},
                        {"desc2", "Les segments verts deviennent jaunes/rouges lorsque la vie diminue."},
                        {"desc3", "Ajustez l'échelle et l'opacité pour modifier l'affichage."},
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
                        {"only_hostile", "Nur feindliche Kreaturen anzeigen:"},
                        {"bar_scale", "Balkenskalierung (50-150%):"},
                        {"bar_opacity", "Balkendeckkraft (10-100%):"},
                        {"max_distance", "Maximale Entfernung (400-1600):"},
                        {"desc1", "Segmentierte Lebensbalken erscheinen über allen Kreaturen."},
                        {"desc2", "Grüne Segmente werden gelb/rot, wenn die Gesundheit abnimmt."},
                        {"desc3", "Skalierung und Deckkraft für Anzeigeeffekte anpassen."},
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
                        {"only_hostile", "Mostrar solo criaturas hostiles:"},
                        {"bar_scale", "Escala de barras (50-150%):"},
                        {"bar_opacity", "Opacidad de barras (10-100%):"},
                        {"max_distance", "Distancia máxima (400-1600):"},
                        {"desc1", "Barras de vida segmentadas aparecen sobre todas las criaturas."},
                        {"desc2", "Los segmentos verdes se vuelven amarillos/rojos al disminuir la vida."},
                        {"desc3", "Ajuste la escala y opacidad para cambiar la visualización."},
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
                        {"only_hostile", "Mostra solo creature ostili:"},
                        {"bar_scale", "Scala barre (50-150%):"},
                        {"bar_opacity", "Opacità barre (10-100%):"},
                        {"max_distance", "Distanza massima (400-1600):"},
                        {"desc1", "Barre vita segmentate appaiono sopra tutte le creature."},
                        {"desc2", "I segmenti verdi diventano gialli/rossi quando la vita diminuisce."},
                        {"desc3", "Regola scala e opacità per modificare la visualizzazione."},
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
                        {"only_hostile", "Mostrar apenas criaturas hostis:"},
                        {"bar_scale", "Escala das barras (50-150%):"},
                        {"bar_opacity", "Opacidade das barras (10-100%):"},
                        {"max_distance", "Distância máxima (400-1600):"},
                        {"desc1", "Barras de vida segmentadas aparecem acima de todas as criaturas."},
                        {"desc2", "Segmentos verdes ficam amarelos/vermelhos quando a vida diminui."},
                        {"desc3", "Ajuste escala e opacidade para alterar a visualização."},
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
                        {"only_hostile", "Показывать только враждебных существ:"},
                        {"bar_scale", "Масштаб полосок (50-150%):"},
                        {"bar_opacity", "Прозрачность полосок (10-100%):"},
                        {"max_distance", "Максимальное расстояние (400-1600):"},
                        {"desc1", "Сегментированные полоски здоровья появляются над всеми существами."},
                        {"desc2", "Зелёные сегменты становятся жёлтыми/красными при снижении здоровья."},
                        {"desc3", "Настройте масштаб и прозрачность для изменения отображения."},
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
            EnableHealthBars = config.Bind("EnableHealthBars", true, new ConfigurableInfo(
                "Enable health bars display for all creatures",
                null, "", "Enable Health Bars"));

            ShowPlayerHealthBar = config.Bind("ShowPlayerHealthBar", false, new ConfigurableInfo(
                "Show health bar above the player",
                null, "", "Show Player Health Bar"));

            HideWhenFullHealth = config.Bind("HideWhenFullHealth", true, new ConfigurableInfo(
                "Hide health bars when creature is at full health",
                null, "", "Hide When Full Health"));

            OnlyShowHostile = config.Bind("OnlyShowHostile", false, new ConfigurableInfo(
                "Only show health bars for hostile creatures",
                null, "", "Only Show Hostile Creatures"));

            HealthBarScale = config.Bind("HealthBarScale", 125, new ConfigurableInfo(
                "Scale of health bars (50-150%, default 125%)",
                new ConfigAcceptableRange<int>(50, 150), "", "Health Bar Scale"));

            HealthBarOpacity = config.Bind("HealthBarOpacity", 70, new ConfigurableInfo(
                "Opacity of health bars (10-100%, default 70%)",
                new ConfigAcceptableRange<int>(10, 100), "", "Health Bar Opacity"));

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

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("mod_name"), true)
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("enable_bars")),
                new OpCheckBox(EnableHealthBars, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("show_player")),
                new OpCheckBox(ShowPlayerHealthBar, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("hide_full")),
                new OpCheckBox(HideWhenFullHealth, new Vector2(250f, yPos))
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("only_hostile")),
                new OpCheckBox(OnlyShowHostile, new Vector2(250f, yPos))
            );
            yPos -= 60f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("bar_scale")),
                new OpSlider(HealthBarScale, new Vector2(250f, yPos), 200)
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("bar_opacity")),
                new OpSlider(HealthBarOpacity, new Vector2(250f, yPos), 200)
            );
            yPos -= 40f;

            opTab.AddItems(
                new OpLabel(10f, yPos, Translate("max_distance")),
                new OpSlider(MaxDistance, new Vector2(250f, yPos), 200)
            );
            yPos -= 60f;

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
