using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private void HandlePointerInput()
        {
            fireRequestedThisFrame = false;

            if (targetCamera == null || battleResolved)
            {
                return;
            }

            Vector2 screenPoint = default;
            var pointerHeld = false;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                screenPoint = mouse.position.ReadValue();
                pointerHeld = true;
            }

            var touchscreen = Touchscreen.current;
            if (!pointerHeld && touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                screenPoint = touchscreen.primaryTouch.position.ReadValue();
                pointerHeld = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (!pointerHeld && Input.GetMouseButton(0))
            {
                screenPoint = Input.mousePosition;
                pointerHeld = true;
            }

            if (!pointerHeld && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    screenPoint = touch.position;
                    pointerHeld = true;
                }
            }
#endif

            if (!pointerHeld)
            {
                return;
            }

            var world = targetCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Mathf.Abs(targetCamera.transform.position.z)));
            world.z = 0f;
            world.x = Mathf.Clamp(world.x, 0f, fieldSize.x - 1f);
            world.y = Mathf.Clamp(world.y, 0f, fieldSize.y - 1f);
            currentAttackPoint = world;
            fireRequestedThisFrame = true;
        }

        private void UpdateMarkerPosition()
        {
            if (inputTargetAnchor != null)
            {
                inputTargetAnchor.position = currentAttackPoint;
            }
        }

        private void ClearEnemyRuntime()
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy != null && enemy.GameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(enemy.GameObject);
                    }
                    else
                    {
                        DestroyImmediate(enemy.GameObject);
                    }
                }
            }

            enemies.Clear();
        }

        private void ClearProjectileRuntime()
        {
            for (var i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile == null || projectile.GameObject == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(projectile.GameObject);
                }
                else
                {
                    DestroyImmediate(projectile.GameObject);
                }
            }

            projectiles.Clear();
        }

        private void DrawHud()
        {
            GUILayout.BeginArea(new Rect(14f, 14f, 380f, 220f), GUI.skin.window);
            GUILayout.Label($"Monster: {selectedMonsterName}");
            GUILayout.Label($"Skill A: {selectedActiveSkillName}");
            GUILayout.Label($"Stage {stageIndex} / Day {dayIndex}");
            GUILayout.Label($"Encounter: {encounterLabel}");
            GUILayout.Label($"Nexus HP: {nexusCurrentHealth:0} / {nexusMaxHealth:0}");
            GUILayout.Label($"Unit HP: {unitCurrentHealth:0} / {unitMaxHealthConfigured:0}");
            GUILayout.Label($"Magazine: {currentShotsRemaining} / {magazineCapacityConfigured}");
            GUILayout.Label(reloadRemaining > 0f
                ? $"Reloading: {reloadRemaining:0.00}s"
                : $"Shot Interval: {shotIntervalConfigured:0.00}s");
            GUILayout.Label($"Projectiles Alive: {projectiles.Count}");
            GUILayout.Label($"Enemies Alive: {enemies.Count}");
            GUILayout.Label($"Focus: ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0})");
            GUILayout.Space(6f);
            GUILayout.Label(statusLabel);
            GUILayout.EndArea();
        }

        private void DrawVictoryPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 240f, 80f, 480f, 400f), GUI.skin.window);
            GUILayout.Label("Victory");
            GUILayout.Label($"Reward Gold: {rewardGold}");
            GUILayout.Label($"Dark Trace: {rewardDarkTrace}");
            GUILayout.Label($"Prisoners: {rewardPrisonerCount} (Boss prisoner guaranteed: {guaranteedPrisonerName})");
            GUILayout.Space(10f);

            if (waitingForRewardChoice)
            {
                GUILayout.Label($"Choose one {selectedMonsterName} reward to continue the prototype loop.");
                for (var i = 0; i < rewardOptions.Count; i++)
                {
                    var option = rewardOptions[i];
                    if (GUILayout.Button(option.Title + "\n" + option.Description, GUILayout.Height(58f)))
                    {
                        ApplyRewardChoice(i);
                    }
                }
            }
            else
            {
                GUILayout.Label(rewardApplied ? appliedRewardSummary : "Reward choice pending.");
                GUILayout.Space(8f);
                if (GUILayout.Button("Next Prototype Day", GUILayout.Height(34f)))
                {
                    BeginPrototypeDay(dayIndex + 1);
                }

                if (GUILayout.Button("Replay Current Day", GUILayout.Height(30f)))
                {
                    BeginPrototypeDay(dayIndex);
                }
            }

            GUILayout.EndArea();
        }

        private void DrawDefeatPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 180f, 100f, 360f, 180f), GUI.skin.window);
            GUILayout.Label("Defeat");
            GUILayout.Label("The prototype battle ends when the Nexus HP reaches zero.");
            GUILayout.Space(8f);
            if (GUILayout.Button("Retry Day", GUILayout.Height(34f)))
            {
                BeginPrototypeDay(dayIndex);
            }

            GUILayout.EndArea();
        }
    }
}
