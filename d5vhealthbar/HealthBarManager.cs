using System.Collections.Generic;
using UnityEngine;

namespace d5vhealthbar
{
    public static class HealthBarManager
    {
        private static Dictionary<Creature, HealthBarData> _healthBars = new Dictionary<Creature, HealthBarData>();

        private static bool IsCreatureHostile(Creature creature, Creature player)
        {
            if (creature == null || player == null) return false;
            try
            {
                var relationship = creature.Template.CreatureRelationship(player.Template);
                bool isHostile = relationship.type == CreatureTemplate.Relationship.Type.Eats ||
                                relationship.type == CreatureTemplate.Relationship.Type.Attacks ||
                                relationship.type == CreatureTemplate.Relationship.Type.Antagonizes;

                if (!isHostile && relationship.intensity > 0f) isHostile = true;
                return isHostile;
            }
            catch (System.Exception e)
            {
                HealthBarMod.Logger?.LogWarning($"IsCreatureHostile failed for {creature?.Template?.type}: {e.Message}");
                return true;
            }
        }

        private static bool IsCreatureInjured(Creature creature)
        {
            if (creature?.State == null) return false;
            try
            {
                if (creature.State is HealthState healthState)
                    return healthState.health < 1f;
                if (creature.stun > 0) return true;
            }
            catch { }
            return false;
        }

        private static bool ShouldShowHealthBar(Creature creature, Creature player, HealthBarData data)
        {
            if (creature == null) return false;
            if (data.IsOverseer) return false;
            if (data.IsInvincible) return false;

            if (HealthBarConfig.OnlyShowHostile != null && HealthBarConfig.OnlyShowHostile.Value)
            {
                if (!IsCreatureHostile(creature, player) && !IsCreatureInjured(creature))
                    return false;
            }
            return true;
        }

        public static void DrawHealthBars(Room room, RoomCamera camera, float timeStacker)
        {
            if (room == null || camera == null) return;
            if (room.abstractRoom == null || room.abstractRoom.creatures == null) return;
            if (camera.hud == null || camera.hud.fContainers == null || camera.hud.fContainers.Length == 0) return;

            if (room.game != null && room.game.GameOverModeActive)
            {
                foreach (var kvp in _healthBars)
                    if (kvp.Value != null) kvp.Value.SetVisible(false);
                return;
            }

            Creature player = null;
            foreach (var ac in room.abstractRoom.creatures)
            {
                if (ac?.realizedCreature is Player p) { player = p; break; }
            }

            if (player != null && (player.enteringShortCut.HasValue || player.inShortcut))
            {
                foreach (var kvp in _healthBars)
                    if (kvp.Value != null) kvp.Value.SetVisible(false);
                return;
            }

            var toRemove = new List<Creature>();
            foreach (var kvp in _healthBars)
            {
                if (kvp.Key == null || kvp.Key.room != room || kvp.Key.slatedForDeletetion)
                {
                    kvp.Value?.RemoveSprites();
                    toRemove.Add(kvp.Key);
                }
                else if (kvp.Key.dead || !kvp.Key.State.alive)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.StartDeathFade();
                        kvp.Value.Update(camera, timeStacker);
                        if (kvp.Value.IsDeathFadeComplete()) toRemove.Add(kvp.Key);
                    }
                }
            }
            foreach (var c in toRemove)
            {
                if (_healthBars.ContainsKey(c))
                {
                    _healthBars[c]?.RemoveSprites();
                    _healthBars.Remove(c);
                }
            }

            foreach (var abstractCreature in room.abstractRoom.creatures)
            {
                if (abstractCreature?.realizedCreature == null || !abstractCreature.realizedCreature.State.alive) continue;
                Creature creature = abstractCreature.realizedCreature;

                if (creature is Player)
                {
                    if (HealthBarConfig.ShowPlayerHealthBar == null || !HealthBarConfig.ShowPlayerHealthBar.Value)
                        continue;
                }
                else
                {
                    if (!_healthBars.ContainsKey(creature))
                    {
                        _healthBars[creature] = new HealthBarData(creature);
                    }
                    if (!ShouldShowHealthBar(creature, player, _healthBars[creature]))
                        continue;
                }

                if (!_healthBars.ContainsKey(creature))
                    _healthBars[creature] = new HealthBarData(creature);
                _healthBars[creature].Update(camera, timeStacker);
            }
        }

        public static void ClearAll()
        {
            foreach (var kvp in _healthBars)
                if (kvp.Value != null) kvp.Value.RemoveSprites();
            _healthBars.Clear();
        }
    }
}