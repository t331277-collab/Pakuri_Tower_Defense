**Findings**

1. [CombatRuntimeRewards.cs](C:/TowerDefence_Pakuri/Test/Pakuri/Assets/Scripts/Combat/CombatRuntimeRewards.cs:128) can generate duplicate prisoner rewards.
   `BuildRewardPrisoners()` first adds guaranteed prisoners at lines 122-124, then repeatedly samples `currentNormalEnemyPool` at lines 128-134. `AddRewardPrisoner()` at lines 155-163 only checks null/name and always appends the display name. In normal combat, [CombatRuntimeEnemies.cs](C:/TowerDefence_Pakuri/Test/Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:161) selects `currentNormalBossDefinition` from that same normal pool, and [line 185](C:/TowerDefence_Pakuri/Test/Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:185) adds it as the guaranteed prisoner. So the random fill can pick the already-guaranteed boss again, producing duplicate prisoner names/options and duplicate accumulation into `RunSession.PrisonerNames` through [RunCombatUiController.cs:294](C:/TowerDefence_Pakuri/Test/Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:294) and [RunSession.cs:100](C:/TowerDefence_Pakuri/Test/Pakuri/Assets/Scripts/Run/RunSession.cs:100). This conflicts with `combat-reward-system.md` lines 48-50, which define boss + normal prisoner candidates, not repeated copies of the same prisoner.

**Checks Completed**

- Read `AGENTS.md`; local output was encoding-garbled, but the provided instruction block was followed.
- Inspected only the requested BLACKBOARD task block plus directly related combat runtime context.
- Reviewed the requested files only for implementation behavior.
- Confirmed referenced classes/functions/fields compile: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly conflict warnings.
- Confirmed artifacts/offering behavior was not implemented as functional logic: reward construction currently emits `RewardKind = "Prisoner"` and `"Material"` only; artifact appears only as a UI template mapping/label, and offering appears only in explanatory text.
- Did not edit files and did not run Unity Play Mode.

REVIEW_RESULT: NEEDS_CHANGES