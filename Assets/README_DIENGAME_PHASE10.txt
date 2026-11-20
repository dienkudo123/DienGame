DIENGAME - GIAI DOAN 10 / PHASE 10 - QUY TANG INSPIRED
Base: DienGame_Phase09_FINAL_TEST_FULL.zip

GOAL
- Enable a FOURTH playable class: DAU SI / Warrior, separate scene.
- Keep the 3 other classes' combat, scenes, skill trees and existing art intact.
- Reuse 4 Warrior sword equipment IDs already present in Phase09.
- 7 Warrior-only passive skills with 5 levels; uses the LevelUpUI that already works for the other classes.
- Remain grounded in the existing sci-fi / Einstein-field-inspired equipment setting. Names are GAMEPLAY INTERPRETATIONS, not newly established story canon.

HOW TO INSTALL
1. FIRST BACK UP your existing Unity project.
2. Apply PHASE10 PATCH onto a project already running PHASE09 FINAL TEST (copy the top-level Assets folder into the project root).
3. Open the Unity Editor and wait for script compilation. New scene GameplayTest_Warrior is present in Assets/_Game/Scenes.
4. The Editor installer auto-configures that scene once, disables old Assassin behaviours copied only into this new scene, and registers all five scenes in Build Settings (Lobby and four gameplay scenes). Existing scenes are not rewritten.
5. If the Editor had UNSAVED SCENE CHANGES and auto installation was skipped, save the current scene and run:
   DienGame > Phase10 > 01 - Cai dat Dau si (scene + build settings)
6. Run:
   DienGame > Phase10 > 02 - Kiem tra 4 class va scene
7. Open Lobby -> CHON NHAN VAT -> DAU SI -> XAC NHAN -> VAO AI.
8. For instant gear unlock in Unity Editor/Development Build, use:
   DienGame > Equipment > 09 FINAL TEST > 01 - Mo khoa tat ca 19 trang bi
   (QA overrides are separate from real saved gear.)

WHAT TO TEST IN PLAY MODE
- All four class cards visible (no clipped text at 360x800, 393x852, 1080x1920).
- DAU SI loads GameplayTest_Warrior, not the Assassin or Marksman scene.
- Press joystick and run in ANY angle; character displays 8-direction animation.
- Auto melee hits closest enemy with one combo sequence of 3 swings.
- Switch to KIEM HOP KIM: standard melee, no status.
- Switch to DAI KIEM DONG NANG: reach 2.45, 225-degree arc, slower attacks, up to 4 targets.
- Switch to KIEM BUONG NHIET: short burn refresh; no stacking.
- Switch to KIEM KET TINH X: slow with limited boss potency.
- Try 7 Warrior passives and confirm they do NOT appear for Marksman/Assassin/Mage.
- Guard reduces damage, no permanent invincibility; shockwave never passes terrain obstacles.
- Pause/restart/return to Lobby; retained selected Warrior + equipment (in QA sandbox).
- Confirm old Chimera, biomech pets, guns and Assassin skills remain functional.

BALANCE BASELINE (TUNABLE; NOT PLAYTESTED)
- Warrior: +25 maximum HP, -6% move speed, +12% base swing interval.
- Default arc 150 degrees, reach 1.85 world units, up to 2 enemies per swing.
- Combo finisher (3rd hit): +16% force before passive modifiers.
- Greatsword: base item multiplier 0.86; slow interval x1.18; 225-degree arc and reach 2.45 (4-target cap).
- Fire sword: base item multiplier 0.84, burn refresh (no stacking).
- Frost sword: base item multiplier 0.90, slow refresh; bosses have reduced effect.
- Energy field calibration stays +4-gated and its existing cost/interval tradeoffs are preserved.
- Warrior passives are class isolated. Active skills are not transplanted from Assassin.

IMPORTANT VISUAL LIMITATION
- The standalone Warrior's animated BODY currently reuses the existing 8-direction Assassin run/slash sprite data as an explicitly labelled temporary fallback, with a distinct sword renderer, gold-steel tint and live sword energy trails.
- This DOES NOT overwrite the original Assassin sprites or animation data. To reach final production art, a dedicated 8-direction Warrior body sheet must be authored and assigned to WarriorVisual. The separate sword visual already switches based on equipment and upgrade tiers.
- Unity Editor and Android Play Mode were NOT available in the packaging environment. Static source / asset / zip integrity checks DO NOT substitute for actual compilation or device QA.

PACKAGE NOTES
- PATCH contains complete copies of all modified C# files, newly added warrior scripts, scene and README. No partial code snippets.
- FULL contains the cumulative ASSETS folder, not ProjectSettings/Packages; use it inside an existing Unity project (it is NOT a standalone Unity project).
- No existing image, original scene, map, enemy or animation was deleted or overwritten; only necessary shared scripts were modified to connect Warrior.
