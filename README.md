# S7Patcher
A simple application for the original release of the game "Settlers 7 - Paths to a kingdom".

# Usage
Simply drag&drop your game executable (Settlers7R.exe) onto the application. Look in your installation folder. The application has only been tested on the Steam Gold Edition, it may or may not work on other versions.

# Features
- Enables the Development-Mode (Skippable intros).
- Unlocks all the DLC that became unavailable after the servers got shut down. This includes the DLC maps, the DLC campaign (which can also be played in skirmish now), the "Enormous Advantage" in the ingame map editor, new prestige decoration, 3 new victory points, and (possibly) more.

# Tech
The application modifies offsets in the game executable in the following way:
- "0xD40E" -> Replace 0x1D with 0x2D.
- "0x1A978E" -> Replace 0x74 with 0xEB.
- "0x1A977C" -> Replace 0x75 0x12 with 0x90 0x90.
- "0x195C34" -> Replace with 0xEB 0x15.
- "0x64477C", "0x21929C" and "0x219224" -> Replace with 0xB0 0x00.
