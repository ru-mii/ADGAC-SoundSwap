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

# config.ini

Each sound has their own config file, here are possible options in the config file you might encounter,
if you're seeing one option in one config file but the same option is not in the other one then do not
add that option in there, it means that this specific option is not supported for that specific sound.

volume -> as name suggests and volume of the sound  
pitch -> the speed of the sound from 0 to 1.0, if you set it to 0.5 it will slow it down and play twice as long  
random pitch -> ```pitch = (Random.Range(-randomPitch, randomPitch) + 1f) * pitch;```  
random volume -> ```volume = (Random.Range(-randomVolume, randomVolume) + 1f) * volume;```  
loop -> will repeat the sound over and over when played once, it won't stop unless game makes it  

If you don't want volume or pitch to be random with each time it plays then change these values to 0, so it will look like this.  

![image](https://github.com/ru-mii/ADGAC-SoundSwap/assets/118167137/be4e2273-e12d-4abd-8ea6-cb08ec9f558e)

# FAQ

1. Is there a hotkey I could use to reload sounds without restarting the game?
- Yes! On your keyboard press M + F5.  

2. Can I remove other folders if I'm not replacing sounds in them?
- Yes! If you remove a folder that has a sound for example "kallsup1" in "SoundManager" it will simply leave the default game sound as it is,
you can also remove the folders in higher hierarchy like "PlayerBodySoundManager", "PlayerSoundManager" or "SoundManager" if you're not replacing
any sounds that fall in their structure, plugin will then just leave the default sound in the game.
