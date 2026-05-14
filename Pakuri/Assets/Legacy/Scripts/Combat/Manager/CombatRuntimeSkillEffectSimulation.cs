using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private SkillEffectSimulation skillEffectSimulation;

        private SkillEffectSimulation SkillEffectSimulationBoundary
        {
            get
            {
                if (skillEffectSimulation == null)
                {
                    skillEffectSimulation = new SkillEffectSimulation(this);
                }

                return skillEffectSimulation;
            }
        }

        private void UpdatePersistentSkillEffects()
        {
            SkillEffectSimulationBoundary.Tick();
        }

        private void TickSkillEffect(SkillEffectRuntime effect)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!CanSkillEffectHitEnemy(effect, enemy))
                {
                    continue;
                }

                ApplySkillEffectHit(effect, enemy);
            }
        }

        private bool CanSkillEffectHitEnemy(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            if (effect == null || enemy == null || enemy.CurrentHealth <= 0f)
            {
                return false;
            }

            return IsEnemyInsideSkillEffect(effect, enemy);
        }

        private bool IsEnemyInsideSkillEffect(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            return effect.SkillId == "eve-b" || IsVegaLineSkillEffect(effect)
                ? IsPointInsideBeam(enemy.Transform.position, effect)
                : Vector2.Distance(enemy.Transform.position, effect.Transform.position) <= effect.Radius + GetEnemyHitRadius(enemy);
        }

        private void ApplySkillEffectHit(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            if (IsSeinSkillEffect(effect))
            {
                ApplySeinSkillEffectDamage(effect, enemy);
                return;
            }

            if (IsVegaSkillEffect(effect))
            {
                ApplyVegaSkillEffectDamage(effect, enemy);
                return;
            }

            if (effect.ManifestedSource != null)
            {
                ApplyManifestedSkillEffectDamage(effect, enemy);
                return;
            }

            ApplyEveSkillEffectHit(effect, enemy);
        }

        private void ApplyEveSkillEffectHit(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            ApplyEveSkillDamage(enemy, effect.BaseDamage, effect.Attribute, 1f, effect.SkillId);
            if (effect.SkillId == "eve-b" && effect.SlowChance > 0f && Random.value < effect.SlowChance)
            {
                enemy.SlowMultiplier = 0.65f;
                enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, effect.SlowDuration);
            }
            else if (effect.SkillId == "eve-c")
            {
                ApplyChill(enemy, Mathf.Max(1, effect.StatusStacks), 2.5f);
                if (effect.FreezeDuration > 0f)
                {
                    enemy.FreezeTimer = Mathf.Max(enemy.FreezeTimer, effect.FreezeDuration);
                }
            }
        }

        private void TryHandleSkillEffectExpired(SkillEffectRuntime effect)
        {
            TryHandleSeinSkillEffectExpired(effect);
        }

        private sealed class SkillEffectSimulation
        {
            private readonly CombatRuntimeController owner;

            public SkillEffectSimulation(CombatRuntimeController owner)
            {
                this.owner = owner;
            }

            public void Tick()
            {
                for (var i = owner.skillEffects.Count - 1; i >= 0; i--)
                {
                    var effect = owner.skillEffects[i];
                    if (effect == null || effect.GameObject == null)
                    {
                        owner.skillEffects.RemoveAt(i);
                        continue;
                    }

                    effect.RemainingDuration = Mathf.Max(0f, effect.RemainingDuration - Time.deltaTime);
                    effect.TickRemaining -= Time.deltaTime;
                    if (effect.TickInterval > 0f && effect.TickRemaining <= 0f && effect.BaseDamage > 0f)
                    {
                        effect.HitThisTick.Clear();
                        owner.TickSkillEffect(effect);
                        effect.TickRemaining = effect.TickInterval;
                    }

                    if (effect.RemainingDuration > 0f)
                    {
                        continue;
                    }

                    owner.TryHandleSkillEffectExpired(effect);
                    Object.Destroy(effect.GameObject);
                    owner.skillEffects.RemoveAt(i);
                }
            }
        }
    }
}
