# S7Patcher
A simple application for the game "The Settlers 7 - Paths to a Kingdom".
<p align="center">
	<img src="https://github.com/Eisenmonoxid/S7Patcher/blob/9ee91e1bbfae1d9420e88e9cd23f59c6088aadbe/Images/S7.jpg" width="60%" height="60%" alt="Header"/>
</p>

**Should you have any questions, errors or feature requests: [Discord](https://discord.gg/7SGkQtAAET).**

---
## Usage
Before using the S7Patcher, make sure that the game has been launched successfully at least once.  
Afterwards, simply drag&drop your game executable (Settlers7R.exe) onto the S7Patcher application **OR** start the application and input the path to the file
manually. The S7Patcher works with the original release of the game and both History Editions.  
_(In case your History Edition is from Steam, the executable will have to be extracted with the tool "Steamless" first before using the S7Patcher!)_

*Find the Settlers7R.exe here:* 
```
<Settlers7>\Data\Base\_Dbg\Bin\Release\Settlers7R.exe
```

---
## Features
### All Versions
- Enables the Development Mode (e.g. skippable intros).
### Original Release (Gold Edition)
- Unlocks all the DLC and UPlay Rewards that became unavailable when the servers got shut down. This includes the DLC maps, the DLC campaign (which can also be played in skirmish now), the "Enormous Advantage" in the ingame map editor, new prestige decoration, 3 new victory points, and (possibly) more.
- Enables all local features (like the castle forge and the profile system) that required network access.
- Fixes a few crashes that happened in offline-mode when local features were present.

---
## Development Mode Options
Using the following key combinations, you can enable/disable a few special features of the Development Mode:
- `CTRL + G`: Toggle the ingame UI.
- `CTRL + C`: Switch between the free view camera mode and the default camera mode. Control the free view camera using WASD or the arrow keys, 
the right mouse button and Q and E.
- `CTRL + SHIFT + T`: Toggle terrain rendering.
- `CTRL + SHIFT + W`: Toggle water and lava rendering.
- `CTRL + SHIFT + C`: Toggle grass and bush rendering.
- `F11`: Toggle the general development mode menu.
- `F10`: Toggle the AI menu.

<p align="center">
	<img src="https://github.com/Eisenmonoxid/S7Patcher/blob/9ee91e1bbfae1d9420e88e9cd23f59c6088aadbe/Images/F11.jpg" width="50%" height="50%" alt="F11"/>
	<img src="https://github.com/Eisenmonoxid/S7Patcher/blob/9ee91e1bbfae1d9420e88e9cd23f59c6088aadbe/Images/F10.jpg" width="50%" height="50%" alt="F10"/>
</p>

It is also possible to play the campaign and scenario maps in skirmish mode (drop-down menu in the upper left corner of the map selection screen).

---
## Recommendation
To gain better performance in the game, set the process priority to "High" and limit the game to your physical CPU cores (4 at max), since it does not work well with SMT/Hyperthreading.

Utilizing [DXVK](https://github.com/doitsujin/dxvk/releases/latest) can also be beneficial for performance and enables you to tweak some settings in the [configuration file](https://github.com/doitsujin/dxvk/wiki/Configuration).