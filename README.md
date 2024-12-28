# S7Patcher
A simple application for the original release of the game "Settlers 7 - Paths to a Kingdom".

# Usage
Simply drag&drop your game executable (Settlers7R.exe) onto the S7Patcher application. Look in your installation folder. The S7Patcher has only been tested on the Steam Gold Edition, it may or may not work on other versions.

*Find the Settlers7R.exe here:* 
```
<Steam>\Settlers 7 Gold\Data\Base\_Dbg\Bin\Release\Settlers7R.exe
```

# Features
- Enables the Development-Mode (Skippable intros).
- Unlocks all the DLC and UPlay Rewards that became unavailable when the servers got shut down. This includes the DLC maps, the DLC campaign (which can also be played in skirmish now), the "Enormous Advantage" in the ingame map editor, new prestige decoration, 3 new victory points, and (possibly) more.
- Enables all local features (like the castle forge) that required network access.

# Tech
The application modifies offsets in the game executable in the following way:
```
- "0x00D40E" -> Replace 0x1D with 0x2D.
- "0x1A978E" -> Replace 0x74 with 0xEB.
- "0x1A977C" -> Replace 0x75 0x12 with 0x90 0x90.
- "0x195C34" -> Replace with 0xEB 0x15.
- "0x69000F" -> Replace 0x95 with 0x94.
- "0x58BC2E" -> Replace 0x00 with 0x01.
- "0x64477C", "0x21929C" and "0x219224" -> Replace with 0xB0 0x00.
- "0x696D83" -> Replace with 0x90 0x90 0x90 0x90 0x90.
- "0x696DC8" -> Replace with 0xE9 0x0B 0x03 0x00 0x00 0x90.
```
