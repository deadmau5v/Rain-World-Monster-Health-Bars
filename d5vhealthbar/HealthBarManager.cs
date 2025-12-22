using System.Collections.Generic;
using UnityEngine;

namespace d5vhealthbar
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> _healthBars = new Dictionary<Creature, HealthBarData>();

        // 检查生物是否对玩家有敌意
        private static bool IsCreatureHostile(Creature creature, Room room)
        {
            if (creature == null || room == null || room.game == null) return false;

            try
            {
                // 查找玩家
                Player player = null;
                foreach (var abstractCreature in room.abstractRoom.creatures)
                {
                    if (abstractCreature?.realizedCreature is Player p)
                    {
                        player = p;
                        break;
                    }
                }

                if (player == null) return false;

                // 使用 Rain World 的关系系统检查敌意
                // CreatureTemplate.Relationship 返回生物之间的关系
                var relationship = creature.abstractCreature.creatureTemplate.CreatureRelationship(player.abstractCreature.creatureTemplate);

                // relationship.type 可以是：
                // - Eats: 会吃掉对方（敌对）
                // - Attacks: 会攻击对方（敌对）
                // - Antagonizes: 敌对（敌对）
                // - Afraid: 害怕对方（非敌对）
                // - Ignores: 忽略对方（非敌对）
                // - etc.

                // 检查敌对关系类型
                bool isHostile = relationship.type == CreatureTemplate.Relationship.Type.Eats ||
                                relationship.type == CreatureTemplate.Relationship.Type.Attacks ||
                                relationship.type == CreatureTemplate.Relationship.Type.Antagonizes;

                // 额外检查：如果关系强度（intensity）大于0，也可能表示敌意
                // 强度越高，敌意越强
                if (!isHostile && relationship.intensity > 0f)
                {
                    isHostile = true;
                }

                return isHostile;
            }
            catch (System.Exception e)
            {
                // 记录错误，方便调试
                HealthBarMod.Logger?.LogWarning($"IsCreatureHostile failed for {creature?.abstractCreature?.creatureTemplate?.type}: {e.Message}");

                // 如果检测失败，保守地返回 true（显示血条）
                return true;
            }
        }

        // 检查生物是否受伤（血量不满）
        private static bool IsCreatureInjured(Creature creature)
        {
            if (creature == null || creature.State == null) return false;

            try
            {
                // 尝试使用 HealthState 检查血量
                if (creature.State is HealthState healthState)
                {
                    // 如果血量小于1.0（满血），认为受伤
                    return healthState.health < 1f;
                }

                // 如果没有 HealthState，检查 stun 值
                if (creature.stun > 0)
                {
                    return true;
                }
            }
            catch
            {
                // 忽略异常
            }

            return false;
        }

        // 检查生物是否应该显示血条（无敌或特殊类型不显示）
        private static bool ShouldShowHealthBar(Creature creature, Room room)
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

            // 如果启用了"只显示敌对生物"选项
            if (HealthBarConfig.OnlyShowHostile != null && HealthBarConfig.OnlyShowHostile.Value)
            {
                // 检查是否敌对 或 是否受伤
                // 受伤的生物即使不敌对也要显示血条
                bool isHostile = IsCreatureHostile(creature, room);
                bool isInjured = IsCreatureInjured(creature);

                if (!isHostile && !isInjured)
                {
                    return false; // 既不敌对也没受伤，不显示
                }
            }

            return true; // 默认显示
        }


        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            // 添加更严格的空值检查
            if (room == null || camera == null) return;
            if (room.abstractRoom == null || room.abstractRoom.creatures == null) return;
            if (camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

            // 检查游戏是否已结束（玩家死亡）
            bool gameOver = false;
            if (room.game != null && room.game.GameOverModeActive)
            {
                gameOver = true;
            }

            // 游戏结束后隐藏所有血条
            if (gameOver)
            {
                foreach (var kvp in _healthBars)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.SetVisible(false);
                    }
                }
                return;
            }

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

                    // 如果是玩家,单独处理（不受"只显示敌对生物"选项影响）
                    if (creature is Player)
                    {
                        if (HealthBarConfig.ShowPlayerHealthBar == null || !HealthBarConfig.ShowPlayerHealthBar.Value)
                        {
                            // 不显示玩家血条,跳过
                            continue;
                        }
                        // 玩家血条通过了检查，继续显示
                    }
                    else
                    {
                        // 对于非玩家生物，检查是否应该显示血条（过滤无敌生物和非敌对生物）
                        if (!ShouldShowHealthBar(creature, room))
                        {
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

        // Sprite 组件 - 分段式血条
        private FContainer _container;
        private List<FSprite> _heartPixels;   // 心形图标像素组（多个像素组成心形）
        private FSprite _containerBorder;     // 容器边框
        private FSprite _containerBackground; // 容器背景
        private List<FSprite> _healthSegments; // 血量方块列表
        private const int MaxSegments = 10;    // 最大分段数量


        // 死亡动画
        private bool _isDying; // 是否处于死亡渐隐状态
        private float _deathTimer; // 死亡计时器
        private const float DeathFadeTime = 1f; // 死亡渐隐持续时间（秒）
        private float _deathAlphaMultiplier = 1f; // 死亡透明度倍数

        // 血条显示参数 - 基础尺寸（会被缩放倍数修改）
        private const float OffsetY = 20f;
        private const float BaseSegmentWidth = 4f;   // 单个方块基础宽度
        private const float BaseSegmentHeight = 4f;  // 单个方块基础高度
        private const float BaseSegmentSpacing = 1f; // 方块基础间距
        private const float BaseHeartSize = 5f;      // 心形图标基础大小
        private const float BaseHeartSpacing = 1.5f;   // 心形图标与血条基础间距
        private const float BaseBorderThickness = 0.5f; // 边框基础厚度
        private const float BaseContainerPadding = 1f; // 容器基础内边距

        // 获取缩放后的尺寸
        private float SegmentWidth
        {
            get { return BaseSegmentWidth * HealthBarScale; }
        }

        private float SegmentHeight
        {
            get { return BaseSegmentHeight * HealthBarScale; }
        }

        private float SegmentSpacing
        {
            get { return BaseSegmentSpacing * HealthBarScale; }
        }

        private float HeartSize
        {
            get { return BaseHeartSize * HealthBarScale; }
        }

        private float HeartSpacing
        {
            get { return BaseHeartSpacing * HealthBarScale; }
        }

        private float BorderThickness
        {
            get { return BaseBorderThickness * HealthBarScale; }
        }

        private float ContainerPadding
        {
            get { return BaseContainerPadding * HealthBarScale; }
        }

        private float HealthBarScale
        {
            get { return HealthBarConfig.HealthBarScale != null ? HealthBarConfig.HealthBarScale.Value / 100f : 1f; }
        }

        private float HealthBarOpacity
        {
            get { return HealthBarConfig.HealthBarOpacity != null ? HealthBarConfig.HealthBarOpacity.Value / 100f : 0.7f; }
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

            // 特殊处理：如果是 HealthState，可能有更精确的 health 字段
            try
            {
                if (creature.State is HealthState healthState)
                {
                    _maxHealth = healthState.health;
                }
            }
            catch
            {
                // 忽略异常
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
            else
            {
                // 尝试使用 HealthState 的 health 字段（更准确）
                bool healthUpdated = false;
                try
                {
                    if (_creature.State is HealthState healthState)
                    {
                        _currentHealth = healthState.health;
                        healthUpdated = true;
                    }
                }
                catch
                {
                    // 忽略异常
                }

                // 如果没有 HealthState，回退到旧的眩晕值估算
                if (!healthUpdated)
                {
                    if (_creature.stun > 0)
                    {
                        // 被眩晕时显示血量下降（估算）
                        _currentHealth = _maxHealth * Mathf.Clamp01(1f - (_creature.stun / 100f));
                    }
                    else
                    {
                        _currentHealth = _maxHealth;
                    }
                }
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
            if (_containerBorder != null && _healthSegments != null && _creature.bodyChunks != null && _creature.bodyChunks.Length > 0)
            {
                if (shouldHide)
                {
                    // 隐藏血条
                    SetVisible(false);
                }
                else
                {
                    // 显示血条
                    SetVisible(true);
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
                _healthSegments = new List<FSprite>();
                _heartPixels = new List<FSprite>();

                // 计算容器总宽度和高度
                float totalSegmentWidth = MaxSegments * SegmentWidth + (MaxSegments - 1) * SegmentSpacing;
                float containerWidth = HeartSize + HeartSpacing + totalSegmentWidth + ContainerPadding * 2;
                float containerHeight = Mathf.Max(HeartSize, SegmentHeight) + ContainerPadding * 2;

                // 创建容器边框（白色，先添加以便在底层）
                _containerBorder = new FSprite("pixel")
                {
                    scaleX = containerWidth + BorderThickness * 2,
                    scaleY = containerHeight + BorderThickness * 2,
                    color = new Color(1f, 1f, 1f),
                    alpha = 1f
                };
                _container.AddChild(_containerBorder);

                // 创建容器背景（黑色，在边框之上）
                _containerBackground = new FSprite("pixel")
                {
                    scaleX = containerWidth,
                    scaleY = containerHeight,
                    color = new Color(0f, 0f, 0f),
                    alpha = 1f
                };
                _container.AddChild(_containerBackground);

                // 创建心形图标 - 使用多个像素组合成心形
                // 心形图案 (5x5 像素网格):
                //   XX XX
                //  XXXXXX
                //  XXXXXX
                //   XXXX
                //    XX
                float pixelSize = HeartSize / 5f; // 每个像素的大小
                int[,] heartPattern = new int[,]
                {
                    {0, 1, 1, 0, 1, 1, 0},  // 第1行
                    {1, 1, 1, 1, 1, 1, 1},  // 第2行
                    {1, 1, 1, 1, 1, 1, 1},  // 第3行
                    {0, 1, 1, 1, 1, 1, 0},  // 第4行
                    {0, 0, 1, 1, 1, 0, 0},  // 第5行
                    {0, 0, 0, 1, 0, 0, 0}   // 第6行
                };

                for (int row = 0; row < heartPattern.GetLength(0); row++)
                {
                    for (int col = 0; col < heartPattern.GetLength(1); col++)
                    {
                        if (heartPattern[row, col] == 1)
                        {
                            FSprite pixel = new FSprite("pixel")
                            {
                                scaleX = pixelSize,
                                scaleY = pixelSize,
                                color = new Color(0.95f, 0.15f, 0.15f), // 鲜艳的红色
                                alpha = 1f
                            };
                            _heartPixels.Add(pixel);
                            _container.AddChild(pixel);
                        }
                    }
                }

                // 创建血量分段
                for (int i = 0; i < MaxSegments; i++)
                {
                    FSprite segment = new FSprite("pixel")
                    {
                        scaleX = SegmentWidth,
                        scaleY = SegmentHeight,
                        color = Color.green,
                        alpha = 1f
                    };
                    _healthSegments.Add(segment);
                    _container.AddChild(segment);
                }

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

            if (_containerBorder == null || _healthSegments == null) return;
            if (camera == null) return;

            // 获取生物头部位置
            Vector2 targetPos = Vector2.Lerp(
                _creature.bodyChunks[0].lastPos,
                _creature.bodyChunks[0].pos,
                timeStacker
            );

            // 转换为屏幕坐标
            Vector2 screenPos = targetPos - camera.pos + new Vector2(0f, OffsetY);

            // 计算血量百分比和填充的方块数量
            float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);
            int filledSegments = Mathf.CeilToInt(healthPercent * MaxSegments);

            // 死亡时至少显示一个红色方块
            if (_isDying && filledSegments == 0)
            {
                filledSegments = 1;
            }

            // 计算容器总宽度
            float totalSegmentWidth = MaxSegments * SegmentWidth + (MaxSegments - 1) * SegmentSpacing;
            float containerWidth = HeartSize + HeartSpacing + totalSegmentWidth + ContainerPadding * 2;

            // 更新容器边框和背景位置
            _containerBorder.SetPosition(screenPos);
            _containerBackground.SetPosition(screenPos);

            // 更新心形图标位置（左侧）
            float heartCenterX = -containerWidth / 2f + ContainerPadding + HeartSize / 2f;

            // 定位心形的每个像素
            float pixelSize = HeartSize / 5f;
            int[,] heartPattern = new int[,]
            {
                {0, 1, 1, 0, 1, 1, 0},  // 第1行
                {1, 1, 1, 1, 1, 1, 1},  // 第2行
                {1, 1, 1, 1, 1, 1, 1},  // 第3行
                {0, 1, 1, 1, 1, 1, 0},  // 第4行
                {0, 0, 1, 1, 1, 0, 0},  // 第5行
                {0, 0, 0, 1, 0, 0, 0}   // 第6行
            };

            int pixelIndex = 0;
            float heartWidth = 7 * pixelSize; // 7列
            float heartHeight = 6 * pixelSize; // 6行

            for (int row = 0; row < heartPattern.GetLength(0); row++)
            {
                for (int col = 0; col < heartPattern.GetLength(1); col++)
                {
                    if (heartPattern[row, col] == 1 && pixelIndex < _heartPixels.Count)
                    {
                        float xOffset = heartCenterX - heartWidth / 2f + col * pixelSize + pixelSize / 2f;
                        float yOffset = heartHeight / 2f - row * pixelSize - pixelSize / 2f;
                        _heartPixels[pixelIndex].SetPosition(screenPos + new Vector2(xOffset, yOffset));
                        pixelIndex++;
                    }
                }
            }

            // 更新血量方块位置和颜色
            float segmentStartX = heartCenterX + HeartSize / 2f + HeartSpacing + SegmentWidth / 2f;
            for (int i = 0; i < MaxSegments; i++)
            {
                FSprite segment = _healthSegments[i];
                float xOffset = segmentStartX + i * (SegmentWidth + SegmentSpacing);
                segment.SetPosition(screenPos + new Vector2(xOffset, 0f));

                // 根据是否填充设置颜色和可见性
                if (i < filledSegments)
                {
                    segment.isVisible = true;
                    // 根据血量百分比设置颜色
                    if (_isDying)
                    {
                        segment.color = new Color(0.9f, 0.1f, 0.1f); // 深红色
                    }
                    else if (healthPercent > 0.6f)
                    {
                        segment.color = new Color(0.2f, 0.9f, 0.2f); // 绿色
                    }
                    else if (healthPercent > 0.3f)
                    {
                        segment.color = new Color(0.9f, 0.9f, 0.2f); // 黄色
                    }
                    else
                    {
                        segment.color = new Color(0.9f, 0.1f, 0.1f); // 红色
                    }
                }
                else
                {
                    // 未填充的方块显示为深灰色
                    segment.isVisible = true;
                    segment.color = new Color(0.2f, 0.2f, 0.2f);
                }
            }

            // 根据距离调整透明度
            float distanceToPlayer = 99999f;
            if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
            {
                distanceToPlayer = Vector2.Distance(targetPos, camera.followAbstractCreature.realizedCreature.mainBodyChunk.pos);
            }

            float distanceAlpha = Mathf.Clamp01(1f - (distanceToPlayer / MaxDistance));

            // 应用死亡渐隐倍数
            distanceAlpha *= _deathAlphaMultiplier;

            // 获取配置的透明度
            float configOpacity = HealthBarOpacity;

            // 应用透明度到所有元素（结合距离透明度和配置透明度）
            _containerBorder.alpha = distanceAlpha * configOpacity;
            _containerBackground.alpha = distanceAlpha * configOpacity * 0.9f; // 背景稍微更透明

            foreach (var heartPixel in _heartPixels)
            {
                heartPixel.alpha = distanceAlpha * configOpacity;
            }

            foreach (var segment in _healthSegments)
            {
                segment.alpha = distanceAlpha * configOpacity;
            }
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
            if (_containerBorder != null)
            {
                _containerBorder.isVisible = visible;
            }
            if (_containerBackground != null)
            {
                _containerBackground.isVisible = visible;
            }
            if (_heartPixels != null)
            {
                foreach (var heartPixel in _heartPixels)
                {
                    if (heartPixel != null)
                    {
                        heartPixel.isVisible = visible;
                    }
                }
            }
            if (_healthSegments != null)
            {
                foreach (var segment in _healthSegments)
                {
                    if (segment != null)
                    {
                        segment.isVisible = visible;
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
                _containerBorder = null;
                _containerBackground = null;
                _heartPixels = null;
                _healthSegments = null;
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to remove sprites: {e.Message}");
            }
        }
    }
}
