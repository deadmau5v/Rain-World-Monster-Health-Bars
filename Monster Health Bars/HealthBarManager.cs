using System.Collections.Generic;
using UnityEngine;

namespace Monster_Health_Bars
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> _healthBars = new Dictionary<Creature, HealthBarData>();

        public static void UpdateCreature(Creature creature)
        {
            if (creature == null || creature.room == null) return;
            
            // 检查生物是否存活
            if (!creature.State.alive) return;

            if (!_healthBars.ContainsKey(creature))
            {
                _healthBars[creature] = new HealthBarData(creature);
            }
            
            _healthBars[creature].Update();
        }

        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            if (room == null || camera == null) return;

            // 清理已死亡或离开房间的生物
            List<Creature> toRemove = new List<Creature>();
            foreach (var kvp in _healthBars)
            {
                if (kvp.Key.room != room || !kvp.Key.State.alive || kvp.Key.slatedForDeletetion)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var creature in toRemove)
            {
                _healthBars.Remove(creature);
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
            foreach (var kvp in _healthBars)
            {
                if (kvp.Key.room == room)
                {
                    kvp.Value.Draw(camera, timeStacker);
                }
            }
        }

        public static void ClearAll()
        {
            _healthBars.Clear();
        }
    }

    public class HealthBarData
    {
        private Creature _creature;
        private float _currentHealth;
        private float _maxHealth;
        
        // 血条显示参数
        private const float BarWidth = 40f;
        private const float BarHeight = 4f;
        private const float OffsetY = 20f;

        public HealthBarData(Creature creature)
        {
            this._creature = creature;
            
            // 使用模板数据计算最大血量
            this._maxHealth = 1f;
            
            // 根据生物类型设定不同的血量
            if (creature.Template.baseDamageResistance > 0f)
            {
                _maxHealth = creature.Template.baseDamageResistance;
            }
            
            if (creature.Template.baseStunResistance > 0f)
            {
                _maxHealth *= creature.Template.baseStunResistance;
            }
            
            // 使用 alive 状态作为当前血量指示器
            this._currentHealth = _maxHealth;
        }

        public void Update()
        {
            if (_creature != null && _creature.State != null)
            {
                // 根据生物状态更新血量
                // 如果生物受伤，可以通过 stunned、dead 等属性判断
                if (_creature.dead || !_creature.State.alive)
                {
                    _currentHealth = 0f;
                }
                else if (_creature.stun > 0)
                {
                    // 被眩晕时显示血量下降
                    _currentHealth = _maxHealth * (1f - (_creature.stun / 100f));
                }
                else
                {
                    _currentHealth = _maxHealth;
                }
            }
        }

        public void Draw(RoomCamera camera, float timeStacker)
        {
            if (_creature == null || _creature.bodyChunks == null || _creature.bodyChunks.Length == 0)
                return;

            // 获取生物头部位置
            Vector2 pos = Vector2.Lerp(
                _creature.bodyChunks[0].lastPos,
                _creature.bodyChunks[0].pos,
                timeStacker
            );

            // 转换为屏幕坐标
            Vector2 screenPos = pos - camera.pos + new Vector2(0f, OffsetY);

            // 计算血量百分比
            float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);

            // 绘制背景 (黑色)
            DrawRect(screenPos, BarWidth, BarHeight, new Color(0f, 0f, 0f, 0.7f));

            // 绘制血条 (渐变颜色:绿->黄->红)
            Color healthColor = GetHealthColor(healthPercent);
            DrawRect(screenPos, BarWidth * healthPercent, BarHeight, healthColor);

            // 绘制边框 (白色)
            DrawRectOutline(screenPos, BarWidth, BarHeight, Color.white);
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
            Color oldColor = GUI.color;
            GUI.color = color;
            
            // 绘制纹理 (使用白色纹理)
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            
            // 恢复颜色
            GUI.color = oldColor;
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
