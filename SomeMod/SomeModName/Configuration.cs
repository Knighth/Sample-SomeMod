using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;  //we need this for keycode object

namespace SomeMod
{
    public class Configuration
    {
        //All public vars will be saved as XML entries 
        //All default assignment here will be inserted into a new configuration object when one is created.

        // store your debuglogging setting
        public bool DebugLogging = false;

        // store your debuglogging level setting;
        // personal preffence: treat 0 as off 1 as same as DebugLogging = true 
        // Where 0\1 means you're logging some basic information, basic startup data perhaps.
        // 2 = Detailed loggin used to for person to troubleshoot, or info they might need to send you the developer.
        // 3+ development\extreme logging only meant for you during development to check someting.
        public byte DebugLoggingLevel = 0;

        public bool AutoShowOnMapLoad = true;  //stores our value of if to show the gui upon map load or not.
        public bool UseCustomLogFile = false;  //our setting to the custom log file name or not.
        public string CustomLogFilePath = "SomeModName_Log.txt";  //name of our default customlog filename
        public bool UseAlternateKeyBinding = false;  //our keybinding setting.
        public string AlternateKeyBindingCode = "LeftControl,LeftAlt,V"; //out keybinding default alternative.
        public float AutoRefreshSeconds = 3.0f;  //how fast our gui will refresh it's data.
        public float GuiOpacity = 1.0f; //how opaqe our gui panel will be.
        public bool GuiUseAutoUpdate = true; //sore the gui screens autoupdate selection state.
        public Configuration() { }

        //this struture holds keycodedata 
        public struct KeycodeData
        {
            public byte NumOfCodes;
            public KeyCode kCode1;
            public KeyCode kCode2;
            public KeyCode kCode3;
        }

        /// <summary>
        /// Returns a KeycodeData structure object containing information about any keybinding codes in the config file.
        /// 
        /// </summary>
        /// <param name="sTheText"></param>
        /// <returns>Returns KeycodeData object where NumOfCodes contains how many are in use, 0 if there were errors</returns>
        public static KeycodeData getAlternateKeyBindings(String sTheText)
        { 
            KeycodeData kcData = new KeycodeData();
            kcData.NumOfCodes = 0;
            kcData.kCode1 = KeyCode.None;
            kcData.kCode2 = KeyCode.None;
            kcData.kCode3 = KeyCode.None;
            try
            {
                string[] sArray = sTheText.Split(',');
                byte ilen = (byte)sArray.Length;
                // if not at least two, just return an object with 0 marked, basically error condition.
                if (ilen <= 1)
                { return kcData; }

                // if 2 string then let's try and convert them to proper Unity KeyCodes. 
                if (ilen == 2)
                {
                    kcData.kCode1 = (KeyCode)Enum.Parse(typeof(KeyCode), sArray[0].ToString());
                    kcData.kCode2 = (KeyCode)Enum.Parse(typeof(KeyCode), sArray[1].ToString());
                    kcData.NumOfCodes = 2;
                }
                else
                {
                    // if 3 strings then let's try and convert first three to proper Unity KeyCodes. 
                    kcData.kCode1 = (KeyCode)Enum.Parse(typeof(KeyCode), sArray[0].ToString());
                    kcData.kCode2 = (KeyCode)Enum.Parse(typeof(KeyCode), sArray[1].ToString());
                    kcData.kCode3 = (KeyCode)Enum.Parse(typeof(KeyCode), sArray[2].ToString());
                    kcData.NumOfCodes = 3;
                }
                if (SomeModName.DEBUG_LOG_ON)
                { Logger.dbgLog("Alternate Keys bound: " + kcData.NumOfCodes.ToString()); }

            }
            catch(Exception ex)
            {
                Logger.dbgLog(ex.Message.ToString(), ex, true);
            }

            // We check if our kCodes1 or 2 are "None", if they are then there were problems and lets just use
            // our default values and return a well formed object.
            if ((kcData.kCode1 == KeyCode.None) || kcData.kCode2 == KeyCode.None)
            {
                kcData.kCode1 = KeyCode.LeftControl; kcData.kCode2 = KeyCode.LeftAlt; kcData.kCode3 = KeyCode.L;
                kcData.NumOfCodes = 3;
                Logger.dbgLog("Alternate Keys enabled but used incorrectly, using default alternate.");
            }

            return kcData;

        }


        //Save the given instance of your config data to disk as XML
        public static void Serialize(string filename, Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (var writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, config);
                }
            }
            catch (System.IO.IOException ex1)
            {
                Logger.dbgLog("Filesystem or IO Error: \r\n", ex1, true);
            }
            catch (Exception ex1)
            {
                Logger.dbgLog(ex1.Message.ToString() + "\r\n", ex1, true);
            }
        }

        //Load your config data to disk as XML into an instance of this object and return it.
        public static Configuration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (Configuration)serializer.Deserialize(reader);
                    ValidateConfig(ref config);
                    return config;
                }
            }
            
            catch(System.IO.FileNotFoundException ex4)
            {
                Logger.dbgLog("config file not found. This is expected if no config file. \r\n", ex4, false);
            }

            catch (System.IO.IOException ex1)
            {
                Logger.dbgLog("Filesystem or IO Error: \r\n", ex1, true);
            }
            catch (Exception ex1)
            {
                Logger.dbgLog(ex1.Message.ToString() + "\r\n", ex1, true);
            }

            return null;
        }

        /// <summary>
        /// Constrain certain values read in from the config file that will either cause issue or just make no sense. 
        /// </summary>
        /// <param name="tmpConfig"> An instance of an initialized Configuration object (*byref*)</param>

        public static void ValidateConfig(ref Configuration tmpConfig)
        {
            //put some stuff here to validate and change actual values of the config object 
            //if you think they are out of bounds, like if you have a display 
            //refresh rate and you want to set limits around it.
            if (tmpConfig.GuiOpacity > 1.0f | tmpConfig.GuiOpacity < 0.10f) tmpConfig.GuiOpacity = 1.0f;
            if (tmpConfig.AutoRefreshSeconds > 60.0f | tmpConfig.AutoRefreshSeconds < 1.0f) tmpConfig.AutoRefreshSeconds = 3.0f;
        }
    }
}
