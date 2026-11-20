# DienGame — Fix CS0101/CS0111 — Lobby V2 & 5 islands

**Why this error recurs:** Unity is compiling old `Assets(8)` class definitions alongside newer `Assets(9)` / Lobby V2 scripts. The original `MetaProgressService` functions at lines 165, 187, 193, and 199 match the old code. Replacing the visible file alone will not work if duplicate `.cs` files still live elsewhere under Assets.

## One-time installation

1. **Close Unity**. Back up your project folder first.
2. Unzip this package **in the Unity project root** (alongside `Assets` and `ProjectSettings`).
3. Open folder `DienGame_AutoFix_V3`, then double-click `FIX_DIENGAME_1_CLICK.cmd`.
4. Read the result, then reopen Unity. Do **not** reapply older patches.

The fixer installs the latest Lobby V2 code for `LobbyUI`, `LobbyStageCatalog`, and `MetaProgressService`; installs 10 missing 5-island scripts only if absent; scans all C# sources below `Assets` for mirrored `_Game` files and 15 known colliding types; quarantines duplicates **outside Assets** in `DienGame_Repair_Backup_<timestamp>`; and writes a `RESULT.txt` report. Existing `.meta`/GUID for canonical scripts is retained.

**Nothing is deleted permanently:** before each removal the original `.cs` and `.meta` are copied to the backup directory. Gameplay implementations outside the three client scripts are kept intact unless missing.

If Unity still reports errors after this, capture the **first errors** in the Console along with the `RESULT.txt` report. Only the actual on-disk project can establish what other scripts or partial definitions are present; ZIP static validation cannot guarantee a clean Unity Play Mode.
