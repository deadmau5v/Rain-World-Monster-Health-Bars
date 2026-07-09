using System.Collections.Generic;
using UnityEngine;

namespace d5vhealthbar
{
    public class HealthBarData
    {
        private readonly Creature _creature;

        private const float MaxHealthRatio = 1f;
        private float _currentHealth = MaxHealthRatio;

        private readonly bool _isOverseer;
        private readonly bool _isInvincible;

        private FContainer _container;
        private List<FSprite> _heartPixels;
        private FSprite _containerBorder;
        private FSprite _containerBackground;
        private List<FSprite> _healthSegments;
        private const int DefaultSegments = 10;
        private readonly int _maxSegments;

        private bool _isDying;
        private float _deathTimer;
        private const float DeathFadeTime = 1f;
        private float _deathAlphaMultiplier = 1f;

        private static readonly int[,] HeartPattern =
        {
            {0, 1, 1, 0, 1, 1, 0},
            {1, 1, 1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1, 1, 1},
            {0, 1, 1, 1, 1, 1, 0},
            {0, 0, 1, 1, 1, 0, 0},
            {0, 0, 0, 1, 0, 0, 0}
        };
        private const int HeartCols = 7;
        private const int HeartRows = 6;
        private List<Vector2> _heartOffsets;

        private const float OffsetY = 20f;
        private const float BaseSegmentWidth = 4f;
        private const float BaseSegmentHeight = 4f;
        private const float BaseSegmentSpacing = 1f;
        private const float BaseHeartSize = 5f;
        private const float BaseHeartSpacing = 1.5f;
        private const float BaseBorderThickness = 0.5f;
        private const float BaseContainerPadding = 1f;

        private float HealthBarScale => HealthBarConfig.HealthBarScale != null ? HealthBarConfig.HealthBarScale.Value / 100f : 1f;
        private float HealthBarOpacity => HealthBarConfig.HealthBarOpacity != null ? HealthBarConfig.HealthBarOpacity.Value / 100f : 0.7f;
        private float MaxDistance => HealthBarConfig.MaxDistance != null ? HealthBarConfig.MaxDistance.Value : 800f;

        private float SegmentWidth => BaseSegmentWidth * HealthBarScale;
        private float SegmentHeight => BaseSegmentHeight * HealthBarScale;
        private float SegmentSpacing => BaseSegmentSpacing * HealthBarScale;
        private float HeartSize => BaseHeartSize * HealthBarScale;
        private float HeartSpacing => BaseHeartSpacing * HealthBarScale;
        private float BorderThickness => BaseBorderThickness * HealthBarScale;
        private float ContainerPadding => BaseContainerPadding * HealthBarScale;

        public HealthBarData(Creature creature)
        {
            _creature = creature;
            _isOverseer = creature is Overseer;
            _isInvincible = ComputeInvincible(creature);
            _maxSegments = ComputeMaxSegments();
        }

        private int ComputeMaxSegments()
        {
            if (_creature.State is HealthState) return DefaultSegments;
            if (_creature is Player player)
            {
                int maxFood = player.MaxFoodInStomach;
                return maxFood > 0 ? maxFood : DefaultSegments;
            }
            return 1;
        }

        private static bool ComputeInvincible(Creature creature)
        {
            try
            {
                var resist = creature.Template?.damageRestistances;
                if (resist == null || resist.Length == 0) return false;
                for (int i = 0; i < resist.GetLength(0); i++)
                {
                    for (int j = 0; j < resist.GetLength(1); j++)
                    {
                        if (resist[i, j] < 999f) return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Update(RoomCamera camera, float timeStacker)
        {
            if (_creature == null || _creature.State == null) return;

            if (_container == null && camera != null && camera.hud != null && camera.hud.fContainers != null && camera.hud.fContainers.Length > 0)
            {
                InitSprites(camera);
            }

            UpdateHealth();
            UpdateDeathFade(timeStacker);

            bool inPipe = _creature.enteringShortCut.HasValue || _creature.inShortcut;

            bool shouldHide = !_isDying
                && (inPipe
                    || (HealthBarConfig.HideWhenFullHealth != null
                        && HealthBarConfig.HideWhenFullHealth.Value
                        && Mathf.Clamp01(_currentHealth / MaxHealthRatio) >= 0.99f));

            if (_containerBorder == null || _healthSegments == null || _creature.bodyChunks == null || _creature.bodyChunks.Length == 0)
                return;

            if (shouldHide)
            {
                SetVisible(false);
            }
            else
            {
                SetVisible(true);
                DrawHealthBar(camera, timeStacker);
            }
        }

        private void UpdateHealth()
        {
            if (_creature.dead || !_creature.State.alive)
            {
                _currentHealth = 0f;
                return;
            }

            if (_creature.State is HealthState healthState)
            {
                _currentHealth = Mathf.Clamp01(healthState.health);
                return;
            }

            if (_creature is Player player)
            {
                int maxFood = player.MaxFoodInStomach;
                _currentHealth = maxFood > 0 ? Mathf.Clamp01((float)player.FoodInStomach / (float)maxFood) : 1f;
                return;
            }

            _currentHealth = _creature.stun > 0
                ? Mathf.Clamp01(1f - (_creature.stun / 100f))
                : 1f;
        }

        private void UpdateDeathFade(float timeStacker)
        {
            if (!_isDying) return;
            _deathTimer += timeStacker / 40f;
            float fadeProgress = Mathf.Clamp01(_deathTimer / DeathFadeTime);
            _deathAlphaMultiplier = 1f - fadeProgress;
        }

        private void InitSprites(RoomCamera camera)
        {
            try
            {
                if (camera == null || camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

                _container = new FContainer();
                _healthSegments = new List<FSprite>();
                _heartPixels = new List<FSprite>();

                float totalSegmentWidth = _maxSegments * SegmentWidth + (_maxSegments - 1) * SegmentSpacing;
                float containerWidth = HeartSize + HeartSpacing + totalSegmentWidth + ContainerPadding * 2f;
                float containerHeight = Mathf.Max(HeartSize, SegmentHeight) + ContainerPadding * 2f;

                _containerBorder = new FSprite("pixel")
                {
                    scaleX = containerWidth + BorderThickness * 2f,
                    scaleY = containerHeight + BorderThickness * 2f,
                    color = new Color(1f, 1f, 1f),
                    alpha = 1f
                };
                _container.AddChild(_containerBorder);

                _containerBackground = new FSprite("pixel")
                {
                    scaleX = containerWidth,
                    scaleY = containerHeight,
                    color = new Color(0f, 0f, 0f),
                    alpha = 1f
                };
                _container.AddChild(_containerBackground);

                float pixelSize = HeartSize / (float)HeartCols;
                for (int row = 0; row < HeartRows; row++)
                {
                    for (int col = 0; col < HeartCols; col++)
                    {
                        if (HeartPattern[row, col] != 1) continue;
                        FSprite pixel = new FSprite("pixel")
                        {
                            scaleX = pixelSize,
                            scaleY = pixelSize,
                            color = new Color(0.95f, 0.15f, 0.15f),
                            alpha = 1f
                        };
                        _heartPixels.Add(pixel);
                        _container.AddChild(pixel);
                    }
                }

                for (int i = 0; i < _maxSegments; i++)
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

                RebuildHeartOffsets();
                camera.hud.fContainers[1].AddChild(_container);
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to initialize sprites: {e.Message}");
            }
        }

        private void RebuildHeartOffsets()
        {
            _heartOffsets = new List<Vector2>();
            float pixelSize = HeartSize / (float)HeartCols;
            float heartWidth = HeartCols * pixelSize;
            float heartHeight = HeartRows * pixelSize;
            float heartCenterX = -ComputeContainerWidth() / 2f + ContainerPadding + HeartSize / 2f;

            for (int row = 0; row < HeartRows; row++)
            {
                for (int col = 0; col < HeartCols; col++)
                {
                    if (HeartPattern[row, col] != 1) continue;
                    float x = heartCenterX - heartWidth / 2f + col * pixelSize + pixelSize / 2f;
                    float y = heartHeight / 2f - row * pixelSize - pixelSize / 2f;
                    _heartOffsets.Add(new Vector2(x, y));
                }
            }
        }

        private float ComputeContainerWidth()
        {
            float totalSegmentWidth = _maxSegments * SegmentWidth + (_maxSegments - 1) * SegmentSpacing;
            return HeartSize + HeartSpacing + totalSegmentWidth + ContainerPadding * 2f;
        }

        private void DrawHealthBar(RoomCamera camera, float timeStacker)
        {
            if (_creature == null || _creature.bodyChunks == null || _creature.bodyChunks.Length == 0) return;
            if (_containerBorder == null || _healthSegments == null || camera == null) return;

            Vector2 targetPos = Vector2.Lerp(
                _creature.bodyChunks[0].lastPos,
                _creature.bodyChunks[0].pos,
                timeStacker
            );
            Vector2 screenPos = targetPos - camera.pos + new Vector2(0f, OffsetY);

            float healthPercent = Mathf.Clamp01(_currentHealth / MaxHealthRatio);
            int filledSegments = Mathf.CeilToInt(healthPercent * _maxSegments);
            if (_isDying && filledSegments == 0) filledSegments = 1;

            _containerBorder.SetPosition(screenPos);
            _containerBackground.SetPosition(screenPos);

            for (int i = 0; i < _heartPixels.Count && i < _heartOffsets.Count; i++)
            {
                _heartPixels[i].SetPosition(screenPos + _heartOffsets[i]);
            }

            float containerWidth = ComputeContainerWidth();
            float segmentStartX = -containerWidth / 2f + ContainerPadding + HeartSize + HeartSpacing + SegmentWidth / 2f;

            for (int i = 0; i < _maxSegments; i++)
            {
                FSprite segment = _healthSegments[i];
                float xOffset = segmentStartX + i * (SegmentWidth + SegmentSpacing);
                segment.SetPosition(screenPos + new Vector2(xOffset, 0f));

                if (i < filledSegments)
                {
                    segment.isVisible = true;
                    segment.color = GetSegmentColor(healthPercent);
                }
                else
                {
                    segment.isVisible = true;
                    segment.color = new Color(0.2f, 0.2f, 0.2f);
                }
            }

            float distanceToPlayer = 99999f;
            if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
            {
                distanceToPlayer = Vector2.Distance(targetPos, camera.followAbstractCreature.realizedCreature.mainBodyChunk.pos);
            }

            float distanceAlpha = Mathf.Clamp01(1f - (distanceToPlayer / MaxDistance)) * _deathAlphaMultiplier;
            float configOpacity = HealthBarOpacity;

            _containerBorder.alpha = distanceAlpha * configOpacity;
            _containerBackground.alpha = distanceAlpha * configOpacity * 0.9f;

            for (int i = 0; i < _heartPixels.Count; i++) _heartPixels[i].alpha = distanceAlpha * configOpacity;
            for (int i = 0; i < _healthSegments.Count; i++) _healthSegments[i].alpha = distanceAlpha * configOpacity;
        }

        private Color GetSegmentColor(float healthPercent)
        {
            if (_isDying) return new Color(0.9f, 0.1f, 0.1f);
            if (healthPercent > 0.6f) return new Color(0.2f, 0.9f, 0.2f);
            if (healthPercent > 0.3f) return new Color(0.9f, 0.9f, 0.2f);
            return new Color(0.9f, 0.1f, 0.1f);
        }

        public void StartDeathFade()
        {
            if (!_isDying)
            {
                _isDying = true;
                _deathTimer = 0f;
            }
        }

        public bool IsDeathFadeComplete() => _isDying && _deathTimer >= DeathFadeTime;

        public bool IsOverseer => _isOverseer;
        public bool IsInvincible => _isInvincible;

        public void SetVisible(bool visible)
        {
            if (_containerBorder != null) _containerBorder.isVisible = visible;
            if (_containerBackground != null) _containerBackground.isVisible = visible;
            if (_heartPixels != null)
            {
                for (int i = 0; i < _heartPixels.Count; i++)
                    if (_heartPixels[i] != null) _heartPixels[i].isVisible = visible;
            }
            if (_healthSegments != null)
            {
                for (int i = 0; i < _healthSegments.Count; i++)
                    if (_healthSegments[i] != null) _healthSegments[i].isVisible = visible;
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
                _heartOffsets = null;
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger.LogError($"Failed to remove sprites: {e.Message}");
            }
        }
    }
}
