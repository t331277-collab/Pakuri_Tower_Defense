using System;
using System.Collections.Generic;
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

    [Test]
    public void TriggerNodesGenerateFinalRuntimeOutcomes()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;
        var triggers = new List<SkillTriggerDefinition>();
        foreach (var monster in catalog.Monsters)
        {
            triggers.AddRange(monster.SkillTriggers);
        }

        Assert.That(triggers, Has.Count.EqualTo(158));
        Assert.That(
            triggers.FindAll(trigger => trigger.TriggeredSkill != null),
            Has.Count.EqualTo(55));
        Assert.That(
            triggers.FindAll(trigger => trigger.Command != null),
            Has.Count.EqualTo(22));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.RecastZone),
            Has.Count.EqualTo(1));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.RefundCooldown),
            Has.Count.EqualTo(14));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.ReduceReload),
            Has.Count.EqualTo(6));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.ExtendStatusDuration),
            Has.Count.EqualTo(1));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill == null && trigger.Command == null),
            Has.Count.EqualTo(81));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed),
            Has.Count.EqualTo(7));
        Assert.That(
            triggers.FindAll(trigger => trigger.UsesExistingSkillRuntime),
            Has.Count.EqualTo(4));
        Assert.That(
            triggers.FindAll(trigger => trigger.PublishSkillLifecycleEvents),
            Has.Count.EqualTo(4));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill != null
                && !trigger.UsesExistingSkillRuntime
                && trigger.PublishSkillLifecycleEvents),
            Is.Empty);
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is SingleSkillDefinition),
            Has.Count.EqualTo(27));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is BuffSkillDefinition),
            Has.Count.EqualTo(21));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is BuffShieldSkillDefinition),
            Has.Count.EqualTo(3));
    }
}
