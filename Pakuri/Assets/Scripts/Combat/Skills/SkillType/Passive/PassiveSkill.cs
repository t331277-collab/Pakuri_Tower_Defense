using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 패시브 트리거가 사용하는 전투 단위 재사용 대기시간과 누적 횟수를 관리한다.
 * 패시브의 실제 행동은 소유 트리거 노드가 실행한다.
 */
namespace Pakuri.InGame
{
    internal class PassiveSkill
    {
        private readonly Dictionary<string, float> triggerCooldowns =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> triggerCounts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void Reset()
        {
            triggerCooldowns.Clear();
            triggerCounts.Clear();
        }

        public bool ConsumeTriggerCooldown(string key, float cooldownSeconds)
        {
            var now = Time.time;
            if (triggerCooldowns.TryGetValue(key, out var readyAt) && readyAt > now)
            {
                return false;
            }

            if (cooldownSeconds > 0f)
            {
                triggerCooldowns[key] = now + cooldownSeconds;
            }
            else
            {
                triggerCooldowns.Remove(key);
            }

            return true;
        }

        public bool ConsumeTriggerCount(string key, int triggerEveryCount)
        {
            if (triggerEveryCount <= 1)
            {
                return true;
            }

            triggerCounts.TryGetValue(key, out var currentCount);
            currentCount++;
            if (currentCount < triggerEveryCount)
            {
                triggerCounts[key] = currentCount;
                return false;
            }

            triggerCounts[key] = 0;
            return true;
        }

        public void NotifyResourceChanged(InGameResourceChangeResult result)
        {
        }

        public void NotifyRosterChanged()
        {
        }

        public void NotifyStatusChanged(UnitCombatState target)
        {
        }

        public void NotifyShieldChanged(UnitCombatState target, float previousShield, float currentShield)
        {
        }

        public void NotifyHealthChanged(UnitCombatState target, float previousHealth, float currentHealth)
        {
        }

        public void FlushPendingChanges(InGameCombatManager combatManager, CombatUnitRegistry roster)
        {
        }
    }
}
