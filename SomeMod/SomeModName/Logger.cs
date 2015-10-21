using System;
using System.Text;
using System.IO;
using ColossalFramework.IO; //need for dataloction's
using UnityEngine; //we need this for the debug.log calls.

namespace SomeMod
{
    static class Logger
    {
        //we create a new stringbuilder and init it to 1024 bytes, typically our message will not be longer then that.
        //If more spaced is need it will auto grow, always x2 current size per growth. So if you know you need to log
        // a huge amount like say 64k or larger, fire up a different stringbuilder with the larger size for that.
        // no need for the generic logger to be much bigger then say 4k - that' big enough for a full stack trace.
        private static StringBuilder logSB = new StringBuilder(1024); 
        
        
        /// <summary>
        /// Our LogWrapper...used to log things so that they have our prefix prepended and logged either to custom file or not.
        /// </summary>
        /// <param name="sText">Text to log</param>
        /// <param name="ex">An Exception - if not null it's basic data will be printed.</param>
        /// <param name="bDumpStack">If an Exception was passed do you want the full stack trace?</param>
        /// <param name="bNoIncMethod">If for some reason you don't want the method name prefaced with the log line.</param>
        public static void dbgLog(string sText, Exception ex = null, bool bDumpStack = false, bool bNoIncMethod = false)
        {
            try
            {
                logSB.Length = 0; //clear the existing log data.
                //now go get our prefix if needed and add it to the stringbuilder.
                string sPrefix = string.Concat("[", SomeModName.MY_MODS_LOG_PREFIX);
                if (bNoIncMethod) { string.Concat(sPrefix, "] "); }
                else
                {
                    // Here we step back a 1 frame in the stack(current frame would be logger), and add that method name
                    // to our prefix so you know wtf method triggered your error. ie "[ModPrefixname:SomeModClassName.TheMethodThatCausedError]:"
                    // Saves you from having to add that to your debug messages manually and is very handy i find.
                    System.Diagnostics.StackFrame oStack = new System.Diagnostics.StackFrame(1); //pop back one frame, ie our caller.
                    sPrefix = string.Concat(sPrefix, ":", oStack.GetMethod().DeclaringType.Name, ".", oStack.GetMethod().Name, "] ");
                }
                logSB.Append(string.Concat(sPrefix, sText));

                //Were we sent and exception object? If so let's log it's top level error message.
                if (ex != null)
                {
                    logSB.Append(string.Concat("\r\nException: ", ex.Message.ToString()));
                }
                //Were we asked to log the stacktrace with that exception?
                if (bDumpStack & ex !=null)
                {
                    logSB.Append(string.Concat("\r\nStackTrace: ", ex.ToString()));  //ex.tostring will return more data then ex.stracktrace.tostring
                }
                //If we have configuration data does it tell use to use a custom log file?
                //if it does let's go use the specified full path to the custom log or use the default file name in the root of CSL folder.
                if (SomeModName.config != null && SomeModName.config.UseCustomLogFile == true)
                {
                    string strPath = System.IO.Directory.Exists(Path.GetDirectoryName(SomeModName.config.CustomLogFilePath)) ? SomeModName.config.CustomLogFilePath.ToString() : Path.Combine(DataLocation.executableDirectory.ToString(), SomeModName.config.CustomLogFilePath);
                    using (StreamWriter streamWriter = new StreamWriter(strPath, true))
                    {
                        streamWriter.WriteLine(logSB.ToString());
                    }
                }
                else
                {
                    Debug.Log(logSB.ToString());
                }
            }
            catch (Exception Exp)
            {
                // Well shit! even our logger errored, lets try to log in CSL log about it.
                Debug.Log(string.Concat(SomeMod.SomeModName.MY_MODS_LOG_PREFIX + " Error in log attempt!  ", Exp.Message.ToString()));
            }
            logSB.Length = 0;
            if (logSB.Capacity > 16384) { logSB.Capacity = 2048;} //shrink outselves if we've grown way to large.
        }



        /// <summary>
        /// Our LogWrapper...used to log larger amounts of text to custom file or not.
        /// * This one different then dbglog in that we create the StringBuilder for each call.
        /// * It's meant to be used so we don't auto-grow the regular static logSB object that stay around in memory 
        /// * so we don't use unneccessary amounts. Yes it's sort of anal since I already have a size check above.
        /// </summary>
        /// <param name="sText">Text to log</param>
        /// <param name="ex">An Exception - if not null it's basic data will be printed.</param>
        /// <param name="bDumpStack">If an Exception was passed do you want the full stack trace?</param>
        /// <param name="bNoIncMethod">If for some reason you don't want the method name prefaced with the log line.</param>
        public static void LargeLog(string sText, int iApproxSize = 4096)
        {
            try
            {
                StringBuilder lgSB = new StringBuilder(iApproxSize);
                //now go get our prefix if needed and add it to the stringbuilder.
                string sPrefix = string.Concat("[", SomeModName.MY_MODS_LOG_PREFIX);
                System.Diagnostics.StackFrame oStack = new System.Diagnostics.StackFrame(1); //pop back one frame, ie our caller.
                sPrefix = string.Concat(sPrefix, ":", oStack.GetMethod().DeclaringType.Name, ".", oStack.GetMethod().Name, "] ");

                lgSB.Append(string.Concat(sPrefix, sText));

                //If we have configuration data does it tell use to use a custom log file?
                //if it does let's go use the specified full path to the custom log or use the default file name in the root of CSL folder.
                if (SomeModName.config != null && SomeModName.config.UseCustomLogFile == true)
                {
                    string strPath = System.IO.Directory.Exists(Path.GetDirectoryName(SomeModName.config.CustomLogFilePath)) ? SomeModName.config.CustomLogFilePath.ToString() : Path.Combine(DataLocation.executableDirectory.ToString(), SomeModName.config.CustomLogFilePath);
                    using (StreamWriter streamWriter = new StreamWriter(strPath, true))
                    {
                        streamWriter.WriteLine(lgSB.ToString());
                    }
                }
                else
                {
                    Debug.Log(lgSB.ToString());
                }
            }
            catch (Exception Exp)
            {
                // Well shit! even our logger errored, lets try to log in CSL log about it.
                Debug.Log(string.Concat(SomeMod.SomeModName.MY_MODS_LOG_PREFIX + " Error in log attempt!  ", Exp.Message.ToString()));
            }
        }

    }
}
