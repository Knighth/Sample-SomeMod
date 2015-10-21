using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework.Threading;
using ICities;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace SomeMod
{
	public class Loader : LoadingExtensionBase
	{
        public static UIView parentGuiView;     //this holds our refference to the game main UIView object.
        public static SomeModNameGUI guiPanel;  //this holds our refference to our actual gui object.
        internal static bool isGuiRunning = false; //this var is set to know if our gui is actually running\is setup
        
        // We store the loadmode here for later use by the gui, frankly you could use look it up over and over again
        // from simulation manager but this is more handy and useful later during mapunload\release to know what mode
        // we were loaded under... though technically for this example mod it doesn't actually matter.
        internal static LoadMode CurrentLoadMode; 

        //again our constructor which we don't actually use, nor is it required.
        public Loader() { }


        /// <summary>
        /// Optional
        /// This function gets called by the game during the loading process when the loading thread gets created.
        /// This happens BEFORE deserialization of the file begins. So while we're not doing anything interesting
        /// here this would be the place to Detour functions that need to be replaced\Detoured before deserialization
        /// begin. Look at Unlimited Trees project for just one example. For basic mods you can wait and do that
        /// during OnLevelLoaded but for those that need to screw with stuff before a map gets loaded just keep
        /// OnCreated in mind.
        /// </summary>
        /// <param name="loading">the games 'loading' object which doesn't have much use this early</param>
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading); //Since we're overriding base object here go run the base objects version first.

            // Try\Catch error handling - I'm assume the reader here knows the basics of this and why it's used in C#
            // However I want to note ALWAYS wrap code that the game is going to call back into in Try\Catches
            // ESPECIALLY above all else in OnCreated OnLevelLoaded OnLevelUnloaded and OnReleased.
            // Colosal order does not have a try\catch around each master call to these of it's own
            // so if your mod shits the bed in one of these call everyone else's mod after you will NOT
            // have these calls invoked.  Unless they're running the Isolated Failures mod of course.

            try
            {
                if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("Reloading config before mapload."); }
                // *reload config values again after map load. This should not be problem atm.
                // *So long as we do this before OnLevelLoaded we should be ok;
                // *In theory this allows someone to go make some manual adjustments in your
                //  config file that don't have options screen settings and still let them be used
                //  without the user having to exit the game and come back just to have them get used.
                Helper.ReloadConfigValues(false, false);
            }
            catch (Exception ex)
            { Logger.dbgLog("Error:", ex, true); }
        }


        /// <summary>
        /// Optional
        /// This core function will get called just after the Level - aka your map has been fully loaded.
        /// That means the game data has all been read from your file and the simulation is ready to go.
        /// </summary>
        /// <param name="mode">a LoadMode enum (ie newgame,newmap,loadgame,loadmap,newasset,loadassett)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);  //call the original implemenation first if does anything... it doesn't actually but whatever maybe some day it might.
            CurrentLoadMode = mode; //save this guy for later.
            try
            {
                if (SomeModName.DEBUG_LOG_ON && SomeModName.DEBUG_LOG_LEVEL > 0) { Logger.dbgLog("LoadMode:" + mode.ToString()); }
                if (SomeModName.isEnabled == true)
                {
                    // only setup gui when in a real game, not in the asset editor
                    if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode==LoadMode.LoadMap ||mode==LoadMode.NewMap )
                    {
                        if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("Asset modes not detcted, setting up gui."); }
                        SetupGui();  //setup gui 
                    }
                }
                else
                {
                    //This should technically never happen, but why not account for it anyway.
                    if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("We fired when we were not even enabled active??"); }
                    RemoveGui(); 
                }
            }
            catch(Exception ex)
            { Logger.dbgLog("Error:", ex, true); }
        }


        /// <summary>
        /// Optional
        /// This function gets called by the game when you've asked to unload a map. Either because you are going to the main menu
        /// or because you are in a map, but have asked to load another map.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();  //see prior comments same concept.
            try
            {
                //This is just an example totally not need for this actual sample mod.
                if (!(CurrentLoadMode == LoadMode.NewAsset || CurrentLoadMode == LoadMode.LoadAsset) & SomeModName.config.DebugLogging == true) 
                {
                    Logger.dbgLog("OnLevelUnloading we've been asked to something when unloading from non-asset related mode"); 
                }

                if(SomeModName.isEnabled & isGuiRunning)
                {
                    RemoveGui(); //go remove our gui.
                }
            }
            catch (Exception ex1)
            {
                Logger.dbgLog("Error: \r\n", ex1, true);
            }


        }

        /// <summary>
        /// This is called by the game when the map as fully unloaded and released, 
        /// it's basically the opposite\counterpart to OnCreated()
        /// </summary>
        public override void OnReleased()
        {
            base.OnReleased();
            if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog ("Releasing Completed."); }
        }


        /// <summary>
        /// Our private little function to do whatever we need to setup and initialize our Gui screen object.
        /// </summary>
        private static void SetupGui()
        {
            if (SomeModName.DEBUG_LOG_ON) Logger.dbgLog(" Setting up Gui panel.");
            try
            {
                parentGuiView = null; //make sure we start fresh, even though we could just assume that was set during the last map unload. 
                parentGuiView = UIView.GetAView(); //go get the root screen\view object from Unity via Colosalframework.ui function.

                //if our object is null (it should be) then lets create one, have the game ADD it, and store it and set our isGUIrunning flag. 
                if (guiPanel == null)
                {
                    guiPanel = (SomeModNameGUI)parentGuiView.AddUIComponent(typeof(SomeModNameGUI));
                    if (SomeModName.DEBUG_LOG_ON) Logger.dbgLog(" GUI Created.");
                }
                isGuiRunning = true;
            }
            catch (Exception ex)
            {
                Logger.dbgLog("Error: \r\n", ex,true);
            }

        }

        /// <summary>
        /// Our private little function to do whatever we need to un-setup and un-initialize our Gui screen object.
        /// </summary>
        private static void RemoveGui()
        {

            if (SomeModName.DEBUG_LOG_ON) Logger.dbgLog(" Removing Gui.");
            try
            {
                if (guiPanel != null)
                {
                    // I've seen some people try to clean up their gui objects with code like this.
                    // I could be wrong but it seem unneccessary in most cases, but I leave it as an example
                    // of something to do in 'removing' your gui object. 
                    // Frankly it seems to me the game cleans these up for you anyway, but in theory doesn't
                    // hurt to do it yourself to maybe trigger garbage collection faster.
                    guiPanel.gameObject.SetActive(false);
                    GameObject.DestroyImmediate(guiPanel.gameObject);
                    guiPanel = null;
                    if (SomeModName.DEBUG_LOG_ON) Logger.dbgLog("Destroyed GUI objects.");
                }
            }
            catch (Exception ex)
            {
                Logger.dbgLog("Error: ",ex,true);
            }

            isGuiRunning = false;
            if (parentGuiView != null) { parentGuiView = null; } //destroy our reference to primary guiview
        }

	}
}
