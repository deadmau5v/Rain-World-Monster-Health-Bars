using System.Collections.Generic;
using UnityEngine;

namespace Monster_Health_Bars
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> healthBars = new Dictionary<Creature, HealthBarData>();

        public static void UpdateCreature(Creature creature)
        {
            if (creature == null || creature.room == null) return;
            
            // 检查生物是否存活
            if (!creature.State.alive) return;

            if (!healthBars.ContainsKey(creature))
            {
                healthBars[creature] = new HealthBarData(creature);
            }
            
            healthBars[creature].Update();
        }

        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            if (room == null || camera == null) return;

            // 清理已死亡或离开房间的生物
            List<Creature> toRemove = new List<Creature>();
            foreach (var kvp in healthBars)
            {
                if (kvp.Key.room != room || !kvp.Key.State.alive || kvp.Key.slatedForDeletetion)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var creature in toRemove)
            {
                healthBars.Remove(creature);
            }

            // 更新并绘制当前房间的生物血条
            foreach (var creature in room.abstractRoom.creatures)
            {
                if (creature.realizedCreature != null && creature.realizedCreature.State.alive)
                {
                    UpdateCreature(creature.realizedCreature);
                }
            }

            // 绘制血条
            foreach (var kvp in healthBars)
            {
                if (kvp.Key.room == room)
                {
                    kvp.Value.Draw(camera, timeStacker);
                }
            }
        }

        public static void ClearAll()
        {
            healthBars.Clear();
        }
    }

    public class HealthBarData
    {
        private Creature creature;
        private float currentHealth;
        private float maxHealth;
        
        // 血条显示参数
        private const float BAR_WIDTH = 40f;
        private const float BAR_HEIGHT = 4f;
        private const float OFFSET_Y = 20f;

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

        public void Update()
        {
            if (creature != null && creature.State != null)
            {
                // 根据生物状态更新血量
                // 如果生物受伤，可以通过 stunned、dead 等属性判断
                if (creature.dead || !creature.State.alive)
                {
                    currentHealth = 0f;
                }
                else if (creature.stun > 0)
                {
                    // 被眩晕时显示血量下降
                    currentHealth = maxHealth * (1f - (creature.stun / 100f));
                }
                else
                {
                    currentHealth = maxHealth;
                }
            }
        }

        public void Draw(RoomCamera camera, float timeStacker)
        {
            if (creature == null || creature.bodyChunks == null || creature.bodyChunks.Length == 0)
                return;

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

            // 绘制背景 (黑色)
            DrawRect(screenPos, BAR_WIDTH, BAR_HEIGHT, new Color(0f, 0f, 0f, 0.7f));

            // 绘制血条 (渐变颜色:绿->黄->红)
            Color healthColor = GetHealthColor(healthPercent);
            DrawRect(screenPos, BAR_WIDTH * healthPercent, BAR_HEIGHT, healthColor);

            // 绘制边框 (白色)
            DrawRectOutline(screenPos, BAR_WIDTH, BAR_HEIGHT, Color.white);
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

        // 简单的矩形绘制 (使用 GUI)
        private void DrawRect(Vector2 center, float width, float height, Color color)
        {
            Rect rect = new Rect(center.x - width / 2f, Screen.height - center.y - height / 2f, width, height);
            
            // 保存之前的颜色
            Color oldColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = color;
            
            // 绘制纹理 (使用白色纹理)
            UnityEngine.GUI.DrawTexture(rect, Texture2D.whiteTexture);
            
            // 恢复颜色
            UnityEngine.GUI.color = oldColor;
        }

        private void DrawRectOutline(Vector2 center, float width, float height, Color color)
        {
            float thickness = 1f;
            
            // 上
            DrawRect(new Vector2(center.x, center.y + height / 2f), width, thickness, color);
            // 下
            DrawRect(new Vector2(center.x, center.y - height / 2f), width, thickness, color);
            // 左
            DrawRect(new Vector2(center.x - width / 2f, center.y), thickness, height, color);
            // 右
            DrawRect(new Vector2(center.x + width / 2f, center.y), thickness, height, color);
        }
    }
}
