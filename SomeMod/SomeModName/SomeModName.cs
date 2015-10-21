using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using ColossalFramework.UI; //we need this to work with settings ui for tool tips.
using ColossalFramework;
using UnityEngine;  //we need this for using coroutines related to settings ui tool tips.

// Make sure if your VS project you add at least ICities.dll, Assembly-CSharp.dll, and UnityEngine.dll and ColossalManaged.dll
// as refferences. this project keeps copies of those dlls in the RefferenceFiles subfolder of the project.
// they are distributed with this sample however to avoid any copyright issues, you can find copies in your Cities_Data\Managed folder.
//

//Your namespace ie 
namespace SomeMod
{
    //Name your class whatever you want, good idea to keep it to your mod's name or Mod
    // The class will inherit (take on the base properties of and implment..) the IUserMod interface found in ICities. 
    public class SomeModName : IUserMod
    {
        internal const string MY_MODS_NAME = "My ModName";  //the name of your mod, make it constant in case you use it more then once.
        internal const string MY_MODS_DESC = "This is my cool mode it does cool stuff";  //keep it short.
        internal const string MY_MODS_LOG_PREFIX = "MyModName";  //This is used by the logging routine incase you want something different then MY_MODS_NAME
        internal static bool isEnabled = false;  //var we use to track if your mod is enabled. 
        internal static bool isInited = false;   //var we use to track if you've done any needed 'startup' stuff you need to do.
        internal static Configuration config;  //hold our configuration data object. 
        internal const string MOD_CONFIGPATH = "MyModName_Config.xml"; 
 
        // These next in theory could not be used and instead you could always pull from our optional config file
        // however I like to use them before even loading the config file so basically store them twice and just sync config
        // values to these, it allows for more flexibility during development.
        internal static bool DEBUG_LOG_ON = false;  //holds if user has enabled Debug logging.
        internal static byte DEBUG_LOG_LEVEL = 0;  // hold if user has selected a particular bug log detail level.


        /// <summary>
        /// Implements the REQUIRED Name property - The game will request this of your mod.
        /// </summary>
        public string Name
        {
            //implement the get'er of the property, no need for a set'er
            get
            {
                return MY_MODS_NAME;
            }
 
        } 

        /// <summary>
        /// Implements the REQUIRED Description property - The game will request this of your mod.
        /// </summary>
        public string Description
        {
            get
            {
                return MY_MODS_DESC;
            }
        }

        /// <summary>
        /// Optional - do only if needed.
        /// Your public contructor, you don't need this. 
        /// It fires off as your mod is loaded by CSL into ram no matter if your mod is enabled or not.
        /// But if you really do need to do something upon loading do it here, do not assume however the game is fully loaded.
        /// keep it to safe things like maybe loading your own configuration files or data in a safe try\catch protected way.
        /// 
        /// </summary>
        public void SomeMod()
        {
            Logger.dbgLog(SomeModName.MY_MODS_NAME + " has been loaded.");

        }

        /// <summary>
        /// Optional
        /// This will fire when your mod is Enabled upon the game starting if your mod is already marked enabled by the user.
        /// or it will fire later when the user 'enables' it.
        /// </summary>
        public void OnEnabled()
        {
            isEnabled = true;
            Logger.dbgLog(SomeModName.MY_MODS_NAME + " has been enabled.");
            Do_Init();
        }


        /// <summary>
        /// Optional
        /// This will fire when either the game shuts down and your dll is unloaded.
        /// or upon your mod being disabled and your mod dll unloaded.
        /// 
        /// </summary>
        public void OnDisabled()
        {

            isEnabled = false;
        }


        /// <summary>
        /// This our private function to do anything you need to 'initialize' your mod here.
        /// Keep in mind the way we have the code in this example it will fire off every time a user
        /// enables your mod, or initially if game loads and this is already enabled.
        /// </summary>
        private void Do_Init()
        {
            //do anything you need to do to 
            Helper.ReloadConfigValues(false, false);

        }


        /// <summary>
        /// The game will detect if you have this public call back, if it does it will create the "options" button
        /// for your mod, and upon selection will bring up and preset the options screen for the user as you define
        /// in this function. The cities skylines wiki provides a basic over view of what your options are here
        /// below is just a sample.
        /// </summary>
        /// <param name="helper"></param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                // These first three lines are not required they are part of us using a trick to futher access
                // the settings screen object and insert mouse-over tool-tips. C\O does not provide an easy way to
                // to add those at the moment so we have to do it manually. 
                UIHelper hp = (UIHelper)helper;
                UIScrollablePanel panel = (UIScrollablePanel)hp.self;  //.self returns the root panel's object we want to track
                // we set an event callback to our SettingEventVisibilityChanged function
                // this way every time the panel goes visiable we can add our tooltips.
                panel.eventVisibilityChanged += SettingsEventVisibilityChanged;

                //Ok back to the normal OnSettingsUI stuff
                UIHelperBase group = helper.AddGroup("Some Mod's Title"); //Title of your settings options panel, keep it short.
                group.AddCheckbox("Auto Show On Map Load", config.UseAlternateKeyBinding, OnUseAlternateKeyBinding); //<-- last part is function you want called when clicked\unclicked.
                group.AddCheckbox("Use Alternate Keybinding", config.UseAlternateKeyBinding, OnUseAlternateKeyBinding); //<-- last part is function you want called when clicked\unclicked.
                group.AddCheckbox("Enable Verbose Logging", DEBUG_LOG_ON, AutoShowChecked); //<-- last part is function you want called when clicked\unclicked. 

                // please note the nexts lines in this example are NOT needed. It's provide as just an example,
                // as the above lines link to function that already automatically and immediately SAVE the config data.
                // I provide this here just for example if you didn't want to do what I actually do.
                group.AddSpace(16);  //add some space between this and the next line.
                group.AddButton("Save", SaveConfigFile);
            }
            catch (Exception ex)
            {
                Logger.dbgLog("Error in settings panel.", ex, true);
            }
        }


        /// <summary>
        /// This is our function that gets call by C\O when visibity gets changed on the settings panel screen.
        /// </summary>
        /// <param name="component">a refferene to the UIComponent object involved</param>
        /// <param name="value">true\false for the isVisiable varible of the object; visable==true</param>
        private void SettingsEventVisibilityChanged(UIComponent component, bool value)
        {
            if (value)
            {
                //unsubsribe from the trigger, we'll re-subscribe again in the next OnSettingsUI call.
                component.eventVisibilityChanged -= SettingsEventVisibilityChanged; 

                //Ok now we're getting a little advanced for someone new but don't be scared.
                //Unity has these things call Coroutines, they all you to go fire off a function
                //that will go do stuff at either certain times, or do a portion of stuff per game-frame
                //allowing that function to continue where it last left off during the next game-fame.
                // don't worry we're not actually doing anything like that here. We just want to go
                // start this other function.
                component.parent.StartCoroutine(Do_ToolTips(component));
            }
        }


        /// <summary>
        /// This is our function that will get called when someone checks our "Enable Verbose Logging" checkbox
        /// </summary>
        /// <param name="bValue">This is the value C\O feeds us, true or false</param>
        private void LoggingChecked(bool bValue)
        {
            DEBUG_LOG_ON = bValue; //update our global var
            config.DebugLogging = bValue; //update our config object
            Configuration.Serialize(MOD_CONFIGPATH, config); //write the data. 
        }


        /// <summary>
        /// This is our function that will get called when someone checks our "Use Alternate Keybinding" checkbox
        /// </summary>
        /// <param name="bValue">This is the value C\O feeds us, true or false</param>
        private void OnUseAlternateKeyBinding(bool bValue)
        {
            config.UseAlternateKeyBinding = bValue; //update our config settings object
            Configuration.Serialize(MOD_CONFIGPATH, config); //save our config settings to disk after update.
        }

        
        /// <summary>
        /// This is our function that will get called when someone checks our "Enable Verbose Logging" checkbox
        /// </summary>
        /// <param name="bValue">This is the value C\O feeds us, true or false</param>
        private void AutoShowChecked(bool bValue)
        {
            config.AutoShowOnMapLoad = bValue; //update our config object
            Configuration.Serialize(MOD_CONFIGPATH, config); //write the data. 
        }

        
        /// <summary>
        /// This is our function that will get called when someone presses our "Save" button
        /// Note again:  This is just an redudant example and isn't really needed in this example mod.
        /// </summary>
        private void SaveConfigFile()
        {
            Configuration.Serialize(MOD_CONFIGPATH,config);
        }


         /// <summary>
         /// Sets up tool tips. Would have been much easier if they would have let us specify the name of the 'components'.
         /// or if C\O just let use set the tooltips directly during creation, but they don't so we work around it.
         /// </summary>
        /// <param name="component">The UIComponent that we'll operate on</param>
         /// <returns>nothing really it's an enumerator</returns>
        private System.Collections.IEnumerator Do_ToolTips(UIComponent component)
        {
            // So remember above when we talked about coroutines, well this one.
            // We're not doing anything fancy like trying to return to ourselves on every game frame.
            // instead we have a very simple one that just wait's 1/2 a second, does some work and then dies
            // and does not stick around.

            yield return new WaitForSeconds(0.500f);  //Go wait for 1/2 a second and do other things you need to do.
            try //ok 1/2 a second has passed and it's come back to us, let's do some stuff.
            {
                //create an array of UICheckBox's call cb and fill it with all the checkboxes inside the 'component' we were fed in. 
                UICheckBox[] cb = component.GetComponentsInChildren<UICheckBox>(true); 

                // let's make sure our array of checkboxes actually got filled with some data.
                if (cb != null && cb.Length > 0)
                {
                    // Awesome it has some checkboxes, now for every single one let's go compare the text
                    // it it matches certain text, then lets set the correct tool tip text on that one.
                    // The reason why we have to loop though all of them is because Colosal doesn't
                    // set the object's .name property to what we actually called it and there is no other way
                    // that I know of to pick out the right one directly, so we have to loop though them one at a time.
                    // annoying but not a big deal, I mean how many are you actually creating anyway right?
                    for (int i = 0; i < (cb.Length); i++)
                    {
                        // I use a switch here as a better example for when you might have 1/2 a dozen or more
                        // technically since I only have three here a couple of if(){} statments would have worked just the same.
                        switch (cb[i].text)
                        {
                            case "Enable Verbose Logging":
                                cb[i].tooltip = "Enables detailed logging for debugging purposes\n See config file for even more options, unless there are problems you probably don't want to enable this.";
                                break;
                            case "Use Alternate Keybinding":
                                cb[i].tooltip = "Enable the use of an alternative key to trigger the gui\n You can set this in your config file\n The default alternative is LeftControl + (LeftAlt + V)<-same time, if not changed.";
                                break;
                            case "Auto Show On Map Load":
                                cb[i].tooltip = "Sets if the info panel will show automatically when your maps load.";
                                break;
                            default:
                                //Well if it's not what we're looking for do anything
                                break;
                        }
                    }
                }

                List<UIButton> bb = new List<UIButton>();   //create a new list of UIButtons.
                component.GetComponentsInChildren<UIButton>(true, bb);  //Get get all child buttons and stick them in our list
                //do we at least have 1 child, if we do it must be out button cause we only created one!
                if ( bb.Count > 0)
                { 
                    //Good we have 1, now since this is a UIButton object it has a tooltip setting, let's set it.
                    bb[0].tooltip = "Clicking on this will save your settings to your config file."; 
                }

            }
            catch(Exception ex)
            {
                Logger.dbgLog("Some error happened.", ex, true);
            }
            yield break; //break out of our coroutine, permenantly.
        }

    }
}
