using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace Monster_Health_Bars
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> healthBars = new Dictionary<Creature, HealthBarData>();

        // 已弃用 - 使用 DrawHealthBars 代替
        // public static void UpdateCreature(Creature creature)
        // {
        //     if (creature == null || creature.room == null) return;
        //
        //     // 检查生物是否存活
        //     if (!creature.State.alive) return;
        //
        //     if (!healthBars.ContainsKey(creature))
        //     {
        //         healthBars[creature] = new HealthBarData(creature);
        //     }
        //
        //     healthBars[creature].Update();
        // }

        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            // 添加更严格的空值检查
            if (room == null || camera == null) return;
            if (room.abstractRoom == null || room.abstractRoom.creatures == null) return;
            if (camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

            // 清理已死亡或离开房间的生物
            List<Creature> toRemove = new List<Creature>();
            foreach (var kvp in healthBars)
            {
                if (kvp.Key == null || kvp.Key.room != room || !kvp.Key.State.alive || kvp.Key.slatedForDeletetion)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.RemoveSprites();
                    }
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var creature in toRemove)
            {
                healthBars.Remove(creature);
            }

            // 更新并绘制当前房间的生物血条
            foreach (var abstractCreature in room.abstractRoom.creatures)
            {
                if (abstractCreature == null) continue;
                if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.State.alive)
                {
                    Creature creature = abstractCreature.realizedCreature;

                    // 如果是玩家,检查配置是否允许显示玩家血条
                    if (creature is Player)
                    {
                        if (HealthBarConfig.ShowPlayerHealthBar == null || !HealthBarConfig.ShowPlayerHealthBar.Value)
                        {
                            // 不显示玩家血条,跳过
                            continue;
                        }
                    }

                    if (!healthBars.ContainsKey(creature))
                    {
                        healthBars[creature] = new HealthBarData(creature);
                    }
                    healthBars[creature].Update(camera, timeStacker);
                }
            }
        }

        public static void ClearAll()
        {
            foreach (var kvp in healthBars)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.RemoveSprites();
                }
            }
            healthBars.Clear();
        }
    }

    public class HealthBarData
    {
        private Creature creature;
        private float currentHealth;
        private float maxHealth;

        // Sprite 组件
        private FSprite backgroundSprite;
        private FSprite healthSprite;
        private FContainer container;
        private RoomCamera lastCamera;

        // 血条显示参数 - 现在从配置读取
        private const float OFFSET_Y = 20f;

        private float BAR_WIDTH
        {
            get { return HealthBarConfig.BarWidth != null ? (float)HealthBarConfig.BarWidth.Value : 40f; }
        }

        private float BAR_HEIGHT
        {
            get { return HealthBarConfig.BarHeight != null ? (float)HealthBarConfig.BarHeight.Value : 4f; }
        }

        private float MAX_DISTANCE
        {
            get { return HealthBarConfig.MaxDistance != null ? (float)HealthBarConfig.MaxDistance.Value : 800f; }
        }

        public HealthBarData(Creature creature)
        {
            this.creature = creature;

            // 使用模板数据计算最大血量
            this.maxHealth = 1f;

            // 根据生物类型设定不同的血量
            if (creature.Template.baseDamageResistance > 0f)
            {
                maxHealth = creature.Template.baseDamageResistance;
            }

            if (creature.Template.baseStunResistance > 0f)
            {
                maxHealth *= creature.Template.baseStunResistance;
            }

            // 使用 alive 状态作为当前血量指示器
            this.currentHealth = maxHealth;
        }

        public void Update(RoomCamera camera, float timeStacker)
        {
            if (creature == null || creature.State == null) return;

            // 初始化 sprites
            if (container == null && camera != null && camera.hud != null && camera.hud.fContainers != null && camera.hud.fContainers.Length > 0)
            {
                InitSprites(camera);
            }

            // 根据生物状态更新血量
            if (creature.dead || !creature.State.alive)
            {
                currentHealth = 0f;
            }
            else if (creature.stun > 0)
            {
                // 被眩晕时显示血量下降
                currentHealth = maxHealth * Mathf.Clamp01(1f - (creature.stun / 100f));
            }
            else
            {
                currentHealth = maxHealth;
            }

            // 检查是否满血且配置为满血时隐藏
            bool shouldHide = false;
            if (HealthBarConfig.HideWhenFullHealth != null && HealthBarConfig.HideWhenFullHealth.Value)
            {
                float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
                if (healthPercent >= 0.99f) // 99% 以上视为满血
                {
                    shouldHide = true;
                }
            }

            // 更新 sprite 位置和颜色
            if (backgroundSprite != null && healthSprite != null && creature.bodyChunks != null && creature.bodyChunks.Length > 0)
            {
                if (shouldHide)
                {
                    // 隐藏血条
                    backgroundSprite.isVisible = false;
                    healthSprite.isVisible = false;
                }
                else
                {
                    // 显示血条
                    backgroundSprite.isVisible = true;
                    healthSprite.isVisible = true;
                    DrawHealthBar(camera, timeStacker);
                }
            }
        }

        private void InitSprites(RoomCamera camera)
        {
            try
            {
                if (camera == null || camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

                lastCamera = camera;
                container = new FContainer();

                // 创建背景 sprite
                backgroundSprite = new FSprite("pixel")
                {
                    scaleX = BAR_WIDTH,
                    scaleY = BAR_HEIGHT,
                    color = new Color(0f, 0f, 0f),
                    alpha = 0.7f
                };

                // 创建血条 sprite
                healthSprite = new FSprite("pixel")
                {
                    scaleX = BAR_WIDTH,
                    scaleY = BAR_HEIGHT,
                    color = Color.green,
                    alpha = 0.9f
                };

                container.AddChild(backgroundSprite);
                container.AddChild(healthSprite);

                camera.hud.fContainers[1].AddChild(container);
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to initialize sprites: {e.Message}");
            }
        }

        private void DrawHealthBar(RoomCamera camera, float timeStacker)
        {
            if (creature == null || creature.bodyChunks == null || creature.bodyChunks.Length == 0)
                return;

            if (backgroundSprite == null || healthSprite == null) return;
            if (camera == null || camera.pos == null) return;

            // 获取生物头部位置
            Vector2 pos = Vector2.Lerp(
                creature.bodyChunks[0].lastPos,
                creature.bodyChunks[0].pos,
                timeStacker
            );

            // 转换为屏幕坐标
            Vector2 screenPos = pos - camera.pos + new Vector2(0f, OFFSET_Y);

            // 计算血量百分比
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

            // 更新背景位置
            backgroundSprite.SetPosition(screenPos);

            // 更新血条位置和宽度
            float healthWidth = BAR_WIDTH * healthPercent;
            healthSprite.scaleX = healthWidth;
            healthSprite.SetPosition(screenPos + new Vector2((healthWidth - BAR_WIDTH) / 2f, 0f));
            healthSprite.color = GetHealthColor(healthPercent);

            // 根据距离调整透明度
            float distanceToPlayer = 99999f;
            if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
            {
                distanceToPlayer = Vector2.Distance(pos, camera.followAbstractCreature.realizedCreature.mainBodyChunk.pos);
            }

            float alpha = Mathf.Clamp01(1f - (distanceToPlayer / MAX_DISTANCE));
            backgroundSprite.alpha = alpha * 0.7f;
            healthSprite.alpha = alpha * 0.9f;
        }

        private Color GetHealthColor(float percent)
        {
            if (percent > 0.6f)
                return Color.Lerp(Color.yellow, Color.green, (percent - 0.6f) / 0.4f);
            else if (percent > 0.3f)
                return Color.Lerp(Color.red, Color.yellow, (percent - 0.3f) / 0.3f);
            else
                return Color.red;
        }

        public void RemoveSprites()
        {
            try
            {
                if (container != null)
                {
                    container.RemoveFromContainer();
                    container = null;
                }
                backgroundSprite = null;
                healthSprite = null;
                lastCamera = null;
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to remove sprites: {e.Message}");
            }
        }
    }
}
