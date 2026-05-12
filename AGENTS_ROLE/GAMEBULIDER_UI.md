# GAMEBULIDER_UI.md

## Unity UI Implementation Checks

When work touches Unity UI, Code Builder must identify which UI path is being used:

- UI Toolkit for screen-space UI, menus, HUD, inventory, settings, dialogs, editor tools, UXML, USS, data binding, and themeable UI.
- UGUI for world-space UI, floating damage numbers, enemy health bars, 3D UI, or complex UI animation cases that require Canvas-based behavior.

UI work should check:

- data binding or command flow instead of UI directly modifying game state;
- screen stack behavior such as Push, Pop, Replace, or ClearTo when navigation is involved;
- mouse, touch, and gamepad input paths;
- focus management for gamepad and modal UI;
- localization keys instead of hardcoded player-facing strings;
- pooling or virtualization for repeated list/grid elements;
- accessibility basics such as text scaling, colorblind-safe indicators, touch target size, and subtitle requirements where relevant.

## Boundary

Do not run Unity Play Mode for gameplay verification. User performs Play Mode verification.

