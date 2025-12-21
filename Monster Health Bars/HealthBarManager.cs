using System.Collections.Generic;
using UnityEngine;

namespace Monster_Health_Bars
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> _healthBars = new Dictionary<Creature, HealthBarData>();

        // 缓存反射字段以提高性能
        private static System.Reflection.FieldInfo _enteringShortCutField;
        private static System.Reflection.FieldInfo _shortcutDelayField;
        private static bool _reflectionInitialized = false;

        // 检查生物是否应该显示血条（无敌或特殊类型不显示）
        private static bool ShouldShowHealthBar(Creature creature)
        {
            if (creature == null) return false;

            // 过滤无敌的特殊生物类型
            if (creature is Overseer) return false; // 观察者

            // 检查是否完全无敌（所有伤害类型抗性都是无限大）
            try
            {
                if (creature.Template != null && creature.Template.damageRestistances != null && creature.Template.damageRestistances.Length > 0)
                {
                    bool allInvincible = true;
                    foreach (var resistance in creature.Template.damageRestistances)
                    {
                        // 如果任何一种伤害类型的抗性不是无限大，说明不是完全无敌
                        if (resistance < 999f) // 999 以下认为不是无敌
                        {
                            allInvincible = false;
                            break;
                        }
                    }
                    if (allInvincible) return false; // 完全无敌，不显示血条
                }
            }
            catch
            {
                // 忽略异常
            }

            return true; // 默认显示
        }

        // 初始化反射字段缓存
        private static void InitializeReflection()
        {
            if (_reflectionInitialized) return;
            _reflectionInitialized = true;

            try
            {
                _enteringShortCutField = typeof(Creature).GetField("enteringShortCut");
                _shortcutDelayField = typeof(Player).GetField("shortcutDelay");
            }
            catch
            {
                // 如果反射失败，字段保持 null
            }
        }

        // 检查生物是否应该隐藏血条（在管道中等状态）
        private static bool IsCreatureHidden(Creature creature)
        {
            if (creature == null) return true;

            // 初始化反射缓存
            InitializeReflection();

            // 检查是否在管道中（已经完全进入）
            if (creature.inShortcut) return true;

            // 检查是否正在进入管道（enteringShortCut 不为 null 表示正在进入）
            if (_enteringShortCutField != null)
            {
                try
                {
                    var value = _enteringShortCutField.GetValue(creature);
                    if (value != null)
                    {
                        return true; // 正在进入管道，隐藏血条
                    }
                }
                catch
                {
                    // 忽略反射错误
                }
            }

            // 检查玩家特定的管道延迟状态
            if (creature is Player player && _shortcutDelayField != null)
            {
                try
                {
                    var delay = _shortcutDelayField.GetValue(player);
                    if (delay != null && (int)delay > 0)
                    {
                        return true; // 管道延迟期间隐藏
                    }
                }
                catch
                {
                    // 忽略反射错误
                }
            }

            // 检查所有身体块是否都不可见或位置异常
            if (creature.bodyChunks != null)
            {
                bool anyVisible = false;
                foreach (var chunk in creature.bodyChunks)
                {
                    if (chunk != null && chunk.pos.x >= 0 && chunk.pos.y >= 0)
                    {
                        anyVisible = true;
                        break;
                    }
                }
                if (!anyVisible) return true;
            }

            // 检查生物是否在房间外（有时候进管道后位置会变成极端值）
            if (creature.room != null && creature.bodyChunks != null && creature.bodyChunks.Length > 0)
            {
                var pos = creature.bodyChunks[0].pos;
                // 如果位置远超房间边界，可能在管道中
                if (pos.x < -100f || pos.y < -100f ||
                    pos.x > creature.room.PixelWidth + 100f ||
                    pos.y > creature.room.PixelHeight + 100f)
                {
                    return true;
                }
            }

            return false;
        }

        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            // 添加更严格的空值检查
            if (room == null || camera == null) return;
            if (room.abstractRoom == null || room.abstractRoom.creatures == null) return;
            if (camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

            // 清理已死亡或离开房间的生物
            List<Creature> toRemove = new List<Creature>();
            foreach (var kvp in _healthBars)
            {
                // 离开房间或被删除 - 立即清理
                if (kvp.Key == null || kvp.Key.room != room || kvp.Key.slatedForDeletetion)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.RemoveSprites();
                    }
                    toRemove.Add(kvp.Key);
                }
                // 死亡 - 启动渐隐动画并继续更新
                else if (kvp.Key.dead || !kvp.Key.State.alive)
                {
                    if (kvp.Value != null)
                    {
                        // 标记为死亡状态，开始计时
                        kvp.Value.StartDeathFade();

                        // 继续更新位置和渐隐动画（关键！）
                        kvp.Value.Update(camera, timeStacker);

                        // 如果渐隐动画已完成，才清理
                        if (kvp.Value.IsDeathFadeComplete())
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }
                }
                // 检查生物是否在管道中或不可见
                else if (kvp.Key.room == room && IsCreatureHidden(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.SetVisible(false);
                    }
                }
                else if (kvp.Key.room == room)
                {
                    // 生物可见且存活,确保血条显示
                    if (kvp.Value != null)
                    {
                        kvp.Value.SetVisible(true);
                    }
                }
            }
            foreach (var creature in toRemove)
            {
                if (_healthBars.ContainsKey(creature))
                {
                    if (_healthBars[creature] != null)
                    {
                        _healthBars[creature].RemoveSprites();
                    }
                    _healthBars.Remove(creature);
                }
            }

            // 更新并绘制当前房间的生物血条
            foreach (var abstractCreature in room.abstractRoom.creatures)
            {
                if (abstractCreature == null) continue;
                if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.State.alive)
                {
                    Creature creature = abstractCreature.realizedCreature;

                    // 检查是否应该显示血条（过滤无敌生物）
                    if (!ShouldShowHealthBar(creature))
                    {
                        continue;
                    }

                    // 如果是玩家,检查配置是否允许显示玩家血条
                    if (creature is Player)
                    {
                        if (HealthBarConfig.ShowPlayerHealthBar == null || !HealthBarConfig.ShowPlayerHealthBar.Value)
                        {
                            // 不显示玩家血条,跳过
                            continue;
                        }
                    }

                    if (!_healthBars.ContainsKey(creature))
                    {
                        _healthBars[creature] = new HealthBarData(creature);
                    }
                    _healthBars[creature].Update(camera, timeStacker);
                }
            }
        }

        public static void ClearAll()
        {
            foreach (var kvp in _healthBars)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.RemoveSprites();
                }
            }
            _healthBars.Clear();
        }
    }

    public class HealthBarData
    {
        private Creature _creature;
        private float _currentHealth;
        private float _maxHealth;

        // Sprite 组件
        private FSprite _backgroundSprite;
        private FSprite _healthSprite;
        private FSprite[] _cornerSprites; // 四个圆角
        private FContainer _container;

        // 位置缓动
        private Vector2 _smoothedPosition;
        private bool _positionInitialized;
        private const float SmoothFactor = 0.3f; // 缓动系数,越小越平滑

        // 死亡动画
        private bool _isDying; // 是否处于死亡渐隐状态
        private float _deathTimer; // 死亡计时器
        private const float DeathFadeTime = 1f; // 死亡渐隐持续时间（秒）
        private float _deathAlphaMultiplier = 1f; // 死亡透明度倍数

        // 血条显示参数 - 现在从配置读取
        private const float OffsetY = 20f;
        private const float CornerRadius = 2f; // 圆角半径

        private float BarWidth
        {
            get { return HealthBarConfig.BarWidth != null ? HealthBarConfig.BarWidth.Value : 40f; }
        }

        private float BarHeight
        {
            get { return HealthBarConfig.BarHeight != null ? HealthBarConfig.BarHeight.Value : 4f; }
        }

        private float MaxDistance
        {
            get { return HealthBarConfig.MaxDistance != null ? HealthBarConfig.MaxDistance.Value : 800f; }
        }

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

        public void Update(RoomCamera camera, float timeStacker)
        {
            if (_creature == null || _creature.State == null) return;

            // 初始化 sprites
            if (_container == null && camera != null && camera.hud != null && camera.hud.fContainers != null && camera.hud.fContainers.Length > 0)
            {
                InitSprites(camera);
            }

            // 根据生物状态更新血量
            if (_creature.dead || !_creature.State.alive)
            {
                _currentHealth = 0f;
            }
            else if (_creature.stun > 0)
            {
                // 被眩晕时显示血量下降
                _currentHealth = _maxHealth * Mathf.Clamp01(1f - (_creature.stun / 100f));
            }
            else
            {
                _currentHealth = _maxHealth;
            }

            // 如果处于死亡渐隐状态，更新计时器
            if (_isDying)
            {
                _deathTimer += timeStacker / 40f; // Rain World runs at 40 FPS
                float fadeProgress = Mathf.Clamp01(_deathTimer / DeathFadeTime);
                _deathAlphaMultiplier = 1f - fadeProgress; // 从 1 渐变到 0
            }

            // 检查是否满血且配置为满血时隐藏（只在非死亡状态下检查）
            bool shouldHide = false;
            if (!_isDying && HealthBarConfig.HideWhenFullHealth != null && HealthBarConfig.HideWhenFullHealth.Value)
            {
                float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);
                if (healthPercent >= 0.99f) // 99% 以上视为满血
                {
                    shouldHide = true;
                }
            }

            // 更新 sprite 位置和颜色
            if (_backgroundSprite != null && _healthSprite != null && _creature.bodyChunks != null && _creature.bodyChunks.Length > 0)
            {
                if (shouldHide)
                {
                    // 隐藏血条
                    _backgroundSprite.isVisible = false;
                    _healthSprite.isVisible = false;
                    if (_cornerSprites != null)
                    {
                        foreach (var corner in _cornerSprites)
                        {
                            if (corner != null) corner.isVisible = false;
                        }
                    }
                }
                else
                {
                    // 显示血条
                    _backgroundSprite.isVisible = true;
                    _healthSprite.isVisible = true;
                    if (_cornerSprites != null)
                    {
                        foreach (var corner in _cornerSprites)
                        {
                            if (corner != null) corner.isVisible = true;
                        }
                    }
                    DrawHealthBar(camera, timeStacker);
                }
            }
        }

        private void InitSprites(RoomCamera camera)
        {
            try
            {
                if (camera == null || camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

                _container = new FContainer();

                // 创建背景 sprite (稍微缩小以便显示圆角)
                _backgroundSprite = new FSprite("pixel")
                {
                    scaleX = BarWidth - CornerRadius,
                    scaleY = BarHeight - CornerRadius,
                    color = new Color(0f, 0f, 0f),
                    alpha = 0.7f
                };

                // 创建血条 sprite
                _healthSprite = new FSprite("pixel")
                {
                    scaleX = BarWidth - CornerRadius,
                    scaleY = BarHeight - CornerRadius,
                    color = Color.green,
                    alpha = 0.9f
                };

                // 创建四个圆角
                _cornerSprites = new FSprite[4];
                for (int i = 0; i < 4; i++)
                {
                    _cornerSprites[i] = new FSprite("pixel")
                    {
                        scaleX = CornerRadius,
                        scaleY = CornerRadius,
                        color = new Color(0f, 0f, 0f),
                        alpha = 0.7f
                    };
                    _container.AddChild(_cornerSprites[i]);
                }

                _container.AddChild(_backgroundSprite);
                _container.AddChild(_healthSprite);

                camera.hud.fContainers[1].AddChild(_container);
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to initialize sprites: {e.Message}");
            }
        }

        private void DrawHealthBar(RoomCamera camera, float timeStacker)
        {
            if (_creature == null || _creature.bodyChunks == null || _creature.bodyChunks.Length == 0)
                return;

            if (_backgroundSprite == null || _healthSprite == null) return;
            if (camera == null) return;

            // 获取生物头部位置
            Vector2 targetPos = Vector2.Lerp(
                _creature.bodyChunks[0].lastPos,
                _creature.bodyChunks[0].pos,
                timeStacker
            );

            // 转换为屏幕坐标
            Vector2 targetScreenPos = targetPos - camera.pos + new Vector2(0f, OffsetY);

            // 应用位置缓动以减少抖动
            if (!_positionInitialized)
            {
                _smoothedPosition = targetScreenPos;
                _positionInitialized = true;
            }
            else
            {
                // 平滑插值
                _smoothedPosition = Vector2.Lerp(_smoothedPosition, targetScreenPos, SmoothFactor);
            }

            Vector2 screenPos = _smoothedPosition;

            // 计算血量百分比
            float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);

            // 死亡时强制显示最小宽度的红色血条
            if (_isDying && healthPercent <= 0.01f)
            {
                healthPercent = 0.05f; // 显示 5% 的红色血条
            }

            // 更新背景位置
            _backgroundSprite.SetPosition(screenPos);

            // 更新血条位置和宽度
            float healthWidth = (BarWidth - CornerRadius) * healthPercent;
            _healthSprite.scaleX = healthWidth;
            _healthSprite.SetPosition(screenPos + new Vector2((healthWidth - (BarWidth - CornerRadius)) / 2f, 0f));
            _healthSprite.color = GetHealthColor(healthPercent);

            // 更新圆角位置
            if (_cornerSprites != null && _cornerSprites.Length == 4)
            {
                float halfWidth = (BarWidth - CornerRadius) / 2f;
                float halfHeight = (BarHeight - CornerRadius) / 2f;

                // 左上角
                _cornerSprites[0].SetPosition(screenPos + new Vector2(-halfWidth, halfHeight));
                // 右上角
                _cornerSprites[1].SetPosition(screenPos + new Vector2(halfWidth, halfHeight));
                // 左下角
                _cornerSprites[2].SetPosition(screenPos + new Vector2(-halfWidth, -halfHeight));
                // 右下角
                _cornerSprites[3].SetPosition(screenPos + new Vector2(halfWidth, -halfHeight));
            }

            // 根据距离调整透明度
            float distanceToPlayer = 99999f;
            if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
            {
                distanceToPlayer = Vector2.Distance(targetPos, camera.followAbstractCreature.realizedCreature.mainBodyChunk.pos);
            }

            float alpha = Mathf.Clamp01(1f - (distanceToPlayer / MaxDistance));

            // 应用死亡渐隐倍数
            alpha *= _deathAlphaMultiplier;

            _backgroundSprite.alpha = alpha * 0.7f;
            _healthSprite.alpha = alpha * 0.9f;

            // 更新圆角透明度
            if (_cornerSprites != null)
            {
                foreach (var corner in _cornerSprites)
                {
                    if (corner != null)
                    {
                        corner.alpha = alpha * 0.7f;
                    }
                }
            }
        }

        private Color GetHealthColor(float percent)
        {
            // 死亡状态强制显示纯红色
            if (_isDying)
                return Color.red;

            if (percent > 0.6f)
                return Color.Lerp(Color.yellow, Color.green, (percent - 0.6f) / 0.4f);
            else if (percent > 0.3f)
                return Color.Lerp(Color.red, Color.yellow, (percent - 0.3f) / 0.3f);
            else
                return Color.red;
        }

        public void StartDeathFade()
        {
            if (!_isDying)
            {
                _isDying = true;
                _deathTimer = 0f;
            }
        }

        public bool IsDeathFadeComplete()
        {
            return _isDying && _deathTimer >= DeathFadeTime;
        }

        public void SetVisible(bool visible)
        {
            if (_backgroundSprite != null)
            {
                _backgroundSprite.isVisible = visible;
            }
            if (_healthSprite != null)
            {
                _healthSprite.isVisible = visible;
            }
            if (_cornerSprites != null)
            {
                foreach (var corner in _cornerSprites)
                {
                    if (corner != null)
                    {
                        corner.isVisible = visible;
                    }
                }
            }
        }

        public void RemoveSprites()
        {
            try
            {
                if (_container != null)
                {
                    _container.RemoveFromContainer();
                    _container = null;
                }
                _backgroundSprite = null;
                _healthSprite = null;
                _cornerSprites = null;
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to remove sprites: {e.Message}");
            }
        }
    }
}
