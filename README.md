# SoundSwap
This mod is created to work with [BepInEx](https://github.com/BepInEx/BepInEx), you would unzip the .zip file from [Releases](https://github.com/ru-mii/ADGAC-SoundSwap/releases) and then copy paste into your plugins folder in BepInEx, should like like this...

![image](https://github.com/ru-mii/ADGAC-SoundSwap/assets/118167137/148570f5-5f02-4023-b639-27c15ffeeca6)

# How to use?

In this example I will show you how to replace the falling sound.
Download the default sound package for the game, it has the required structure and all the sounds with their names so you would know which one is the one you want to replace.
Unzip the .zip file and place "Sounds" into "SoundSwap" folder plugin, should look like this... 

![image](https://github.com/ru-mii/ADGAC-SoundSwap/assets/118167137/92b02b27-7f68-400a-9706-4d62a9b2f14c)

From now on all of the sounds in the "Sounds" folder will be loaded and used accordingly. For our example we wanted to replace the falling sound,
I have checked and it's in Sounds/PlayerSoundManager, the folders that will have falling sound are "VoiceFall1", "VoiceFall2", "VoiceBigFall1".
If you want to replace the sound with something else you go into that folder, then remove the old sound file and place your in there,
in our example I am replacing the falling sound with a can sound (don't ask why), so what I removed the old file and placed my can.mp3 into the folder.

![image](https://github.com/ru-mii/ADGAC-SoundSwap/assets/118167137/23234204-5ad5-4f25-aa70-651d830710ff)

The only thing that matters when it comes to naming stuff is the folders and "config.ini" (DO NOT RENAME THEM),
the actual sound file can be named as whatever, make sure to have only one sound file in the folder though.
