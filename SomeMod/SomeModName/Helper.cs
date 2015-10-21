//using ICities;
//using ColossalFramework.IO;
//using ColossalFramework.Plugins;
//using ColossalFramework.Packaging;
//using ColossalFramework.Steamworks;
//using ColossalFramework;
//using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SomeMod
{
    public class Helper
    {

        /// <summary>
        /// Called to either initially load, or force a reload our config file var; called by mod initialization and again at mapload. 
        /// </summary>
        /// <param name="bForceReread">Set to true to flush the old object and create a new one.</param>
        /// <param name="bNoReloadVars">Set this to true to NOT reload the values from the new read of config file to our class level counterpart vars</param>
        public static void ReloadConfigValues(bool bForceReread, bool bNoReloadVars)
        {
            try
            {

                if (bForceReread)
                {
                    SomeModName.config = null;
                    if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL >= 1) { Logger.dbgLog("Config wipe requested."); }
                }
                SomeModName.config = Configuration.Deserialize(SomeModName.MOD_CONFIGPATH);
                if (SomeModName.config == null)
                {
                    SomeModName.config = new Configuration();
                    //reset of setting should pull defaults
                    Logger.dbgLog("Existing config was null. Created new one.");
                    Configuration.Serialize(SomeModName.MOD_CONFIGPATH, SomeModName.config); //let's write it.
                }
                if (SomeModName.config != null && bNoReloadVars == false) //set\refresh our vars by default.
                {
                    SomeModName.DEBUG_LOG_ON = SomeModName.config.DebugLogging;
                    SomeModName.DEBUG_LOG_LEVEL = SomeModName.config.DebugLoggingLevel;
                    if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL >= 2) { Logger.dbgLog("Vars refreshed"); }
                }
                if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL >= 2) { Logger.dbgLog(string.Format("Reloaded Config data ({0}:{1} :{2})", bForceReread.ToString(), bNoReloadVars.ToString() )); }
            }
            catch (Exception ex)
            { Logger.dbgLog("Exception while loading config values.", ex, true); }

        }


    }

}
