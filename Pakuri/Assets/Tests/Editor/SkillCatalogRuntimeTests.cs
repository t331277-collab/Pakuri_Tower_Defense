using System;
using NUnit.Framework;
using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

public sealed class SkillCatalogRuntimeTests
{
    [Test]
    public void ChoiceNodesApplyOnlyToTheirTargetSkill()
    {
        var skill = new SkillDefinition { SkillId = "skill-a" };
        var choice = new SkillChoice
        {
            Nodes = new[]
            {
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 2f), "skill-a"),
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 3f), "skill-b")
            }
        };
        var data = new SkillExecutionData(skill);

        data.ApplyChoiceSpec(choice);

        Assert.That(data.DamageModifierOps, Has.Count.EqualTo(1));
        Assert.That(data.DamageModifierOps[0].Multiplier, Is.EqualTo(2f));
    }

    [Test]
    public void CatalogAndRebuildReuseFinalDefinition()
    {
        var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
        var monster = ScriptableObject.CreateInstance<MonsterDefinition>();
        var skill = new SkillDefinition { SkillId = "skill-a", Slot = SkillSlot.A };

        try
        {
            monster.MonsterId = "monster-a";
            monster.ActiveSkills = new[] { skill };
            catalog.Monsters = new[] { monster };
            catalog.RebuildLookup();

            Assert.That(catalog.GetData<SkillDefinition>("skill-a"), Is.SameAs(skill));
            Assert.That(catalog.GetActiveSkill("monster-a", SkillSlot.A), Is.SameAs(skill));

            var owner = new UnitCombatState();
            SkillExecution.RebuildAssignedSkillState(
                owner,
                new[] { skill },
                Array.Empty<SkillTriggerDefinition>());
            var firstState = owner.SkillState.FindBySkillId("skill-a");
            SkillExecution.RebuildAssignedSkillState(
                owner,
                new[] { skill },
                Array.Empty<SkillTriggerDefinition>());
            var secondState = owner.SkillState.FindBySkillId("skill-a");

            Assert.That(secondState, Is.Not.SameAs(firstState));
            Assert.That(firstState.Data, Is.SameAs(skill));
            Assert.That(secondState.Data, Is.SameAs(skill));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(monster);
            UnityEngine.Object.DestroyImmediate(catalog);
        }
    }
}
