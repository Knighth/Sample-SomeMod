
This is a sample Cities Skylines 1.2.x mod project

It's meant to be a bit of a tutorial for those who want to look at simple mod project that has most of it's code commented so the reader has a better idea of what and why certain things are being done.

This sample shows how implemented a basic options screen, an exmample of how to store those settings and read them back in with a configuration class, one of many ways to create a simple gui panel in the game for a player to see certain information and how access certain data from one of the "Managers" in the game. Also shows one way to implement alternate key-bindings for your mod.

It isn't meant to do very much, it's just basically provides the vehicle count and total amounts of vehicles who are importing\exporting or exchanging certain types of 'material' in the game.  It also counts taxi passengers. 


I'm 100% this is not neccessarly the 'best' code, I don't follow every best practice etc, but the code works and in the grand sceme of things you'll probably will find worse code out there.


This is a Visual Studio Ultimate 2010 project targeted for .net 3.5 (mono level) it should open\auto convert just fine in VS 2013.  You will however need to place the following refference dll's in the "ReferenceFiles" folder of the project:

ICities.dll
Assembly-CSharp.dll
ColassalManaged.dll
UnityEngine.dll

You can find them in your Cities_Data\Managed folder of your game installation.
..Alternatively you can just modifiy the project references to link to where those files are on your computers. Personally I prefer to just keep copies with-in a given project.
 
Additional requirements:
If you're new to Cities Skylines modding you're going to want to have a MSIL decompiler installed so that you can look at the decompiled source code in Colassal's various dlls. Especially Assembly-CSharp. I generally use Telerik's JustDecompile, or ILSpy, DotPeek is another one that will serve you well, and all of them do a good job. 


