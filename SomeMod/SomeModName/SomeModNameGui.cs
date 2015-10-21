using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
//The following line is just a file level global alias trick so I don't have to type SomeModName_VehicleData.GoodsType a bunch of times.
//If you want to remove just replace VD in RefreshVehicleData with 'SomeModName_VehicleData.GoodsType'
using VD = SomeMod.SomeModName_VehicleData.GoodsType; 

namespace SomeMod
{
    public class SomeModNameGUI : UIPanel
    {
        public static readonly string cacheName = "SomeModNameGUI"; //we set this in case we need to find the instance of this guy at some point.
        public static SomeModNameGUI instance;  //incase we or someone else needs wants to interact with us they can 'get' a reference to us.
        private const string DTMilli = "MM/dd/yyyy hh:mm:ss.fff tt"; // remembering this is a pain so I make it a constant.
        private const string sVALUE_PLACEHOLDER = "00000 | 00000 |  00000 | 00000  00000 | 00000  00000 | 00000";
        private const string sVALUE_FSTRING = " {0} | {1}      {2} | {3}     {4} | {5}     {6} | {7}";
        private const string sHEADER_TEXT = "Cargo Type:  #TotalVeh's | #AmtTotal  #Local | #AmtLocal  #Import | #AmtImport  #Export | #AmtExport";
        private const string sTEXT_FSTRING = "{0}   :";
        private const string TAG_VALUE_PREFIX = "SomeModName_Value_"; //We don't make use of this or the next in this example but can come in handy when trying to 'find' specific object of yours by tag name.
        private const string TAG_TEXT_PREFIX = "SomeModName_Text_";
        private static readonly float WIDTH = 720f; //The width of our gui in pixel points
        private static readonly float HEIGHT = 400f; // the hieght of our gui
        private static readonly float HEADER = 40f; //how big our header is
        private static readonly float SPACING = 10f; //default spacing amount
        private static readonly float SPACING22 = 22f; // bigger spacer amount
        private static bool isRefreshing = false;  //Used basically as a safety lock.

        private bool CoDisplayRefreshEnabled = false; //This tell us if certain coroutine is running.

        private Dictionary<string, UILabel> _txtControlContainer = new Dictionary<string, UILabel>(16);
        private Dictionary<string,UILabel> _valuesControlContainer = new Dictionary<string,UILabel>(16);

        //all our UI related objects.
        UIDragHandle m_DragHandler; //object that lets us move the panel around.
        UIButton m_closeButton; //object for our close button
        UILabel m_title;    //object for our title bar
        UIButton m_refresh;  //our manual refresh button
        UILabel m_AutoRefreshChkboxText; //label that get assigned to the AutoRefreshCheckbox.
        UICheckBox m_AutoRefreshCheckbox; //Our AutoRefresh checkbox
        UILabel m_AdditionalText1Text;
        UIButton m_LogdataButton;
        UIButton m_ClearDataButton;

        UILabel m_HeaderDataText;  //our header text data

        UILabel m_GoodsText;
        UILabel m_GoodsValue;
        UILabel m_GrainText;
        UILabel m_GrainValue;
        UILabel m_OilText;
        UILabel m_OilValue;
        UILabel m_FoodText;
        UILabel m_FoodValue;
        UILabel m_TaxiText;
        UILabel m_TaxiValue;

        private ItemClass.Availability CurrentMode;
        private Configuration.KeycodeData AlternateBindingData;
        private bool bUseAlternateKeys = false;
        private SomeModName_VehicleData VData;  //Holds an instance of our VehicleData object.


        ///
        /// <summary>
        /// Our public constructor
        /// We could do some other init stuff here but I prefer to most of it via Start() as
        /// basically we know the panel is created at that point.
        /// </summary>
        public SomeModNameGUI()
        {
            try
            {
                VData = new SomeModName_VehicleData(); //create and assign our VehicleData object.
            }
            catch (Exception ex)
            { Logger.dbgLog("Error during contruction. ", ex, true); }
 
        }

        /// <summary>
        /// Function gets called by Unity on every single frame update, at 60fps that's 60 times per second.
        /// be very careful what you do in here, can kill your performance if try to do too much per-frame.
        /// Also 99.5% of the time there is no need to update all data on your gui every frame.
        /// That's why we used a timed coroutine to just update every couple seconds, more then a few times per second
        /// even for constantly changing data in most cases is just a waste.
        ///
        /// We just check for our key binding combo, maybe there is a better way to register this with the game?
        /// </summary>
        public override void Update()
        {
            if (bUseAlternateKeys) //are alternate keys setup and in use?
            {
                if (AlternateBindingData.NumOfCodes == 2)
                {
                    if (Input.GetKey(AlternateBindingData.kCode1) && Input.GetKeyDown(AlternateBindingData.kCode2))
                    { this.ProcessVisibility(); }
                }
                else 
                {
                    if (Input.GetKey(AlternateBindingData.kCode1) && Input.GetKeyDown(AlternateBindingData.kCode2) && Input.GetKey(AlternateBindingData.kCode3))
                    { this.ProcessVisibility(); }
                }
            }
            else //no, good just check for our default. 
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
                {
                    this.ProcessVisibility();

                }
            }
            base.Update();
        }


        /// <summary>
        /// Gets called upon the base UI component's creation. Basically it's the constructor...but not really.
        /// </summary>
        public override void Start()
        {
            base.Start();
            if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL > 0) Logger.dbgLog(string.Concat("Attempting to create our display panel.  ",DateTime.Now.ToString(DTMilli).ToString()));
            this.size = new Vector2(WIDTH, HEIGHT);
            this.backgroundSprite = "MenuPanel";
            this.canFocus = true;
            this.isInteractive = true;
            this.BringToFront();
            this.relativePosition = new Vector3((Loader.parentGuiView.fixedWidth / 2) - 200, (Loader.parentGuiView.fixedHeight / 2) - 350);
            this.opacity = SomeModName.config.GuiOpacity;
            this.cachedName = cacheName;
            SomeModNameGUI.instance = this;
            CurrentMode = Singleton<ToolManager>.instance.m_properties.m_mode;

            //DragHandler
            m_DragHandler = this.AddUIComponent<UIDragHandle>();
            m_DragHandler.target = this;  //set the drag hangler target to this panel. it will do the rest magically.

            //Our Titlebar UILabel
            m_title = this.AddUIComponent<UILabel>();
            m_title.text = "Vehicle Cargo Data"; //spaces on purpose
            m_title.relativePosition = new Vector3(WIDTH / 2 - (m_title.width / 2) - 25f, (HEADER / 2) - (m_title.height / 2));
            m_title.textAlignment = UIHorizontalAlignment.Center;

            //Our Close Button UIButton
            m_closeButton = this.AddUIComponent<UIButton>();
            m_closeButton.normalBgSprite = "buttonclose";
            m_closeButton.hoveredBgSprite = "buttonclosehover";
            m_closeButton.pressedBgSprite = "buttonclosepressed";
            m_closeButton.relativePosition = new Vector3(WIDTH - 35, 5, 10);
            m_closeButton.eventClick += (component, eventParam) =>
            {
                this.Hide();
            };
            //^^above that's just an inline delegate to trigger hidding the panel when someone clicks the ' X ' (close) Button

            //What's our config say about Showing on map load?
            if (!SomeModName.config.AutoShowOnMapLoad)
            {
                this.Hide();
            }

            DoOnStartup(); //let go do some other stuff too during "start"
            if (SomeModName.DEBUG_LOG_ON) Logger.dbgLog(string.Concat("Display panel created. ",DateTime.Now.ToString(DTMilli).ToString()));
        }


        /// <summary>
        /// Our initialize stuff; called after basic panel\form setup.
        /// </summary>
        private void DoOnStartup()
        {
            CreateTextLabels();
            CreateDataLabels();
            PopulateControlContainers();
            VData.RefreshData();    //lets have it go grab some inital data.

            //Are we using Alternate key bindings? If so go get that data and store it and set a local flag.
            if (SomeModName.config.UseAlternateKeyBinding)
            {
                AlternateBindingData = Configuration.getAlternateKeyBindings(SomeModName.config.AlternateKeyBindingCode);
                if (AlternateBindingData.NumOfCodes > 1) { bUseAlternateKeys = true;}
            }

            // is AutoRefresh checked? If it is lets start up the Coroutine that does that for us.
            if (m_AutoRefreshCheckbox.isChecked)
            {
                this.StartCoroutine(RefreshDisplayDataWrapper());
                if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("RefreshDisplayDataWrapper coroutine started."); }
            }
            else
            {
                RefreshDisplayData(); //at least run once even if set to manual.
            }
        }



        #region Create_The_Text_and_Value_Labels
        /// <summary>
        /// Create and setup up default text and stuff for our Text UILabels;
        /// </summary>
        private void CreateTextLabels() 
        {
            m_HeaderDataText = this.AddUIComponent<UILabel>();
            m_HeaderDataText.textScale = 0.800f;
            m_HeaderDataText.text = sHEADER_TEXT ;
            m_HeaderDataText.tooltip = string.Concat("Some tool tip information you want to add here");
            m_HeaderDataText.relativePosition = new Vector3(SPACING, 50f);
            m_HeaderDataText.textColor = new Color32(255, 0, 0, 0); //just an example of how to set a color to red.   
            m_HeaderDataText.autoSize = true;

            m_GoodsText = this.AddUIComponent<UILabel>();
            m_GoodsText.text = String.Format(sTEXT_FSTRING, "Goods");
            m_GoodsText.tooltip = "Goods!";
            m_GoodsText.relativePosition = new Vector3(SPACING, (m_HeaderDataText.relativePosition.y + SPACING22));
            m_GoodsText.autoSize = true;
            m_GoodsText.name = TAG_TEXT_PREFIX + "0";

            m_GrainText = this.AddUIComponent<UILabel>();
            m_GrainText.relativePosition = new Vector3(SPACING, (m_GoodsText.relativePosition.y + SPACING22));
            m_GrainText.text = String.Format(sTEXT_FSTRING, "Grain");
            m_GrainText.tooltip = "Grain";
            m_GrainText.autoSize = true;
            m_GrainText.name = TAG_TEXT_PREFIX + "1";

            m_OilText = this.AddUIComponent<UILabel>();
            m_OilText.relativePosition = new Vector3(SPACING, (m_GrainText.relativePosition.y + SPACING22));
            m_OilText.text = String.Format(sTEXT_FSTRING, "Oil");
            m_OilText.tooltip = "Oil";
            m_OilText.autoSize = true;
            m_OilText.name = TAG_TEXT_PREFIX + "2";


            m_FoodText = this.AddUIComponent<UILabel>();
            m_FoodText.relativePosition = new Vector3(SPACING, (m_OilText.relativePosition.y + SPACING22));
            m_FoodText.text = String.Format(sTEXT_FSTRING, "Food");
            m_FoodText.tooltip = "Foooooood! Yummy!";
            m_FoodText.autoSize = true;
            m_FoodText.name = TAG_TEXT_PREFIX + "3";

            m_TaxiText = this.AddUIComponent<UILabel>();
            m_TaxiText.relativePosition = new Vector3(SPACING, (m_FoodText.relativePosition.y + SPACING22));
            m_TaxiText.text = String.Format(sTEXT_FSTRING, "Taxi");
            m_TaxiText.tooltip = "Taxi's BEEP BEEP!";
            m_TaxiText.autoSize = true;
            m_TaxiText.name = TAG_TEXT_PREFIX + "4";

            //ok here wer setup the check box and we set it's relative to position based off of the last listing lable.
            m_AutoRefreshCheckbox = this.AddUIComponent<UICheckBox>();
            m_AutoRefreshCheckbox.relativePosition = new Vector3((SPACING), (m_TaxiText.relativePosition.y + 30f));
            //next the text label for that checkbox.
            m_AutoRefreshChkboxText = this.AddUIComponent<UILabel>();
            m_AutoRefreshChkboxText.relativePosition = new Vector3(m_AutoRefreshCheckbox.relativePosition.x + m_AutoRefreshCheckbox.width + (SPACING * 3), (m_AutoRefreshCheckbox.relativePosition.y) + 5f);
            m_AutoRefreshChkboxText.tooltip = "Enables these stats to update every few seconds \n Default is 3 seconds.";
            m_AutoRefreshCheckbox.height = 16;
            m_AutoRefreshCheckbox.width = 16;
            m_AutoRefreshCheckbox.label = m_AutoRefreshChkboxText;
            m_AutoRefreshCheckbox.text = string.Concat("Use AutoRefresh  (", SomeModName.config.AutoRefreshSeconds.ToString("f1"), " sec)");

            //Now we add checked and un-checked sprites (graphics) to the checkbox
            UISprite uncheckSprite = m_AutoRefreshCheckbox.AddUIComponent<UISprite>();
            uncheckSprite.height = 20;
            uncheckSprite.width = 20;
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            UISprite checkSprite = m_AutoRefreshCheckbox.AddUIComponent<UISprite>();
            checkSprite.height = 20;
            checkSprite.width = 20;
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            m_AutoRefreshCheckbox.checkedBoxObject = checkSprite;
            m_AutoRefreshCheckbox.isChecked = SomeModName.config.GuiUseAutoUpdate;
            m_AutoRefreshCheckbox.isEnabled = true;
            m_AutoRefreshCheckbox.isVisible = true;
            m_AutoRefreshCheckbox.canFocus = true;
            m_AutoRefreshCheckbox.isInteractive = true;
            //now we add an event handler delegate telling it to call our function when ever this box is checked or unchecked.
            m_AutoRefreshCheckbox.eventCheckChanged += (component, eventParam) => { AutoRefreshCheckbox_OnCheckChanged(component, eventParam); };

            //setup our extra text message label
            m_AdditionalText1Text = this.AddUIComponent<UILabel>();
            m_AdditionalText1Text.relativePosition = new Vector3(m_AutoRefreshCheckbox.relativePosition.x + m_AutoRefreshCheckbox.width + SPACING, (m_AutoRefreshCheckbox.relativePosition.y) + 25f);
            m_AdditionalText1Text.width = 300f;
            m_AdditionalText1Text.height = 50f;
            m_AdditionalText1Text.textScale = 0.875f;
            m_AdditionalText1Text.text = "* Use CTRL + V to show again. \n  More options available in " + SomeModName.MOD_CONFIGPATH ;

            //create our manual refresh button and setup the eventhandler
            m_refresh = this.AddUIComponent<UIButton>();
            m_refresh.size = new Vector2(120, 24);
            m_refresh.text = "Manual Refresh";
            m_refresh.tooltip = "Use to manually refresh the data. \n (use when auto enabled is off)";
            m_refresh.textScale = 0.875f;
            m_refresh.normalBgSprite = "ButtonMenu";
            m_refresh.hoveredBgSprite = "ButtonMenuHovered";
            m_refresh.pressedBgSprite = "ButtonMenuPressed";
            m_refresh.disabledBgSprite = "ButtonMenuDisabled";
            m_refresh.relativePosition = m_AutoRefreshChkboxText.relativePosition + new Vector3((m_AutoRefreshChkboxText.width + SPACING * 2), -5f);
            m_refresh.eventClick += (component, eventParam) =>
            {
                VData.RefreshData();
                RefreshDisplayData();
            };

            //create our log button and setup the eventhandler.
            m_LogdataButton = this.AddUIComponent<UIButton>();
            m_LogdataButton.size = new Vector2(80, 24);
            m_LogdataButton.text = "Log Data";
            m_LogdataButton.tooltip = "Use to Log the current data to log file.";
            m_LogdataButton.textScale = 0.875f;
            m_LogdataButton.normalBgSprite = "ButtonMenu";
            m_LogdataButton.hoveredBgSprite = "ButtonMenuHovered";
            m_LogdataButton.pressedBgSprite = "ButtonMenuPressed";
            m_LogdataButton.disabledBgSprite = "ButtonMenuDisabled";
            m_LogdataButton.relativePosition = m_refresh.relativePosition + new Vector3((m_refresh.width + SPACING * 3), 0f);
            m_LogdataButton.eventClick += (component, eventParam) => { ProcessOnLogButton(); };

            //create our clear button and setup the eventhandler
            m_ClearDataButton = this.AddUIComponent<UIButton>();
            m_ClearDataButton.size = new Vector2(50, 24);
            m_ClearDataButton.text = "Clear";
            m_ClearDataButton.tooltip = "Use to manually clear and reset the above data values.";
            m_ClearDataButton.textScale = 0.875f;
            m_ClearDataButton.normalBgSprite = "ButtonMenu";
            m_ClearDataButton.hoveredBgSprite = "ButtonMenuHovered";
            m_ClearDataButton.pressedBgSprite = "ButtonMenuPressed";
            m_ClearDataButton.disabledBgSprite = "ButtonMenuDisabled";
            m_ClearDataButton.relativePosition = m_LogdataButton.relativePosition + new Vector3((m_LogdataButton.width + SPACING * 3), 0f);
            m_ClearDataButton.eventClick += (component, eventParam) => { 
                VData.ResetAllData(true);
                RefreshDisplayData();
            };
        }


        
        /// <summary>
        /// Creates all our UILabels that store data that changes\gets refreshed.
        /// </summary>
        private void CreateDataLabels()
        {
            m_GoodsValue = this.AddUIComponent<UILabel>();
            m_GoodsValue.text = sVALUE_PLACEHOLDER;
            m_GoodsValue.relativePosition = new Vector3(m_GoodsText.relativePosition.x + m_GoodsText.width + (SPACING * 4), m_GoodsText.relativePosition.y);
            m_GoodsValue.autoSize = true;
            m_GoodsValue.tooltip = "";
            m_GoodsValue.name = TAG_VALUE_PREFIX + "0";

            m_GrainValue = this.AddUIComponent<UILabel>();
            m_GrainValue.relativePosition = new Vector3(m_GoodsValue.relativePosition.x, m_GrainText.relativePosition.y);
            m_GrainValue.autoSize = true;
            m_GrainValue.text = sVALUE_PLACEHOLDER;
            m_GrainValue.name = TAG_VALUE_PREFIX + "1";

            m_OilValue = this.AddUIComponent<UILabel>();
            m_OilValue.relativePosition = new Vector3(m_GrainValue.relativePosition.x, m_OilText.relativePosition.y);
            m_OilValue.autoSize = true;
            m_OilValue.text = sVALUE_PLACEHOLDER;
            m_OilValue.name = TAG_VALUE_PREFIX + "2";

            m_FoodValue = this.AddUIComponent<UILabel>();
            m_FoodValue.relativePosition = new Vector3(m_OilValue.relativePosition.x, m_FoodText.relativePosition.y);
            m_FoodValue.autoSize = true;
            m_FoodValue.text = sVALUE_PLACEHOLDER;
            m_FoodValue.name = TAG_VALUE_PREFIX + "3";

            m_TaxiValue = this.AddUIComponent<UILabel>();
            m_TaxiValue.relativePosition = new Vector3(m_FoodValue.relativePosition.x, m_TaxiText.relativePosition.y);
            m_TaxiValue.autoSize = true;
            m_TaxiValue.text = sVALUE_PLACEHOLDER;
            m_TaxiValue.name = TAG_VALUE_PREFIX + "4";

        }

        #endregion


        /// <summary>
        /// Event handler for clicking on AutoRefreshbutton.
        /// </summary>
        /// <param name="UIComp">The triggering UIComponent</param>
        /// <param name="bValue">The Value True|False (Checked|Unchecked)</param>

        private void AutoRefreshCheckbox_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("AutoRefreshButton was toggled to: " + bValue.ToString()); }
            SomeModName.config.GuiUseAutoUpdate = bValue;  //update our config object.
            SomeMod.Configuration.Serialize(SomeModName.MOD_CONFIGPATH,SomeModName.config); //write change to disk.
            if (bValue == true)
            {
                if (!CoDisplayRefreshEnabled) { this.StartCoroutine(RefreshDisplayDataWrapper());}
                if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("Starting all coroutines that were not already started, bValue=" + 
                    bValue.ToString()); }
            }
            else
            {
                this.StopAllCoroutines();
                ResetAllCoroutineState(false); //cleanup
                if (SomeModName.DEBUG_LOG_ON) { Logger.dbgLog("Stopping all coroutines: " + bValue.ToString()); }
            }
            return;
        }

        /// <summary>
        /// We use to reset state of Coroutines after forced stop.
        /// Kind of pointsless for just having one, but if you had a couple
        /// say another one that changed the color of certain data based on it's value
        /// running every so often this is here as a catch all
        /// </summary>
        /// <param name="bStatus">True|False</param>
        private void ResetAllCoroutineState(bool bStatus)
        {
            CoDisplayRefreshEnabled = bStatus;
            //CoSomeOtherCoroutingEnabled = bStatus;
        }



        /// <summary>
        /// Primary coroutine function to update the fairly static (in seconds) information display.
        /// as there really is no need to update this more then once per second.
        /// </summary>
        private IEnumerator RefreshDisplayDataWrapper() 
        {
            if (CoDisplayRefreshEnabled == true)
            {
                //ensure only 1 active, there is probably no need to do this but I'm paranoid.
                if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL > 0) Logger.dbgLog("Refresh vehicleData* coroutine exited; Only one allowed at a time.");
                yield break;  //break out of the co-routine.
            } 
 
            // here we make sure we only proceed if nobody else is already in the middle of calling RefreshDisplayData
            // that the panel\form is currently visable and then of course only if autorefresh is still checked.
            while (isRefreshing == false && this.isVisible == true && m_AutoRefreshCheckbox.isChecked)
            {
                CoDisplayRefreshEnabled  = true;
                VData.RefreshData();  //refresh the data
                RefreshDisplayData(); //update display data.
                if (SomeModName.DEBUG_LOG_LEVEL > 2)
                { Logger.dbgLog(string.Concat("Refresh triggered at ", DateTime.Now.ToString(DTMilli))); }
                yield return new WaitForSeconds(SomeModName.config.AutoRefreshSeconds);
            }
            CoDisplayRefreshEnabled = false;
            if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL > 0) Logger.dbgLog("Refresh vehicleData coroutine exited due to AutoRefresh disabled, visiblity change, or already refreshing.");
            yield break;
        }


        /// <summary>
        /// Function refreshes the display data. mostly called from coroutine timer.
        /// It basically just loads up the tmp object with fresh strings from VData and does the string replace.
        /// Techincally it would be faster to string.concat(a,b,c,d,e,f,g,etc) but in theory we're only updating
        /// this every few seconds, and if you wanted to localize the data (we don't) string.Format would make things
        /// much easier.
        /// </summary>
        private void RefreshDisplayData()
        {
            // safety lock so we never get more then one of these, in theory could happen if 
            // co-routine fires and manual refresh pressed at same exact same time, in which case why do the work twice?
            isRefreshing = true; 
            try
            {
                //Makesure our object array full of strings is valid.
                if(VData.m_strFormatArray == null)
                {
                    Logger.dbgLog("StringFormatingArray object itself was null!");
                    return;
                }
                // loop though our goods, for each one load up the data and then switch to the current one
                // and update the text.
                for (int i = 0; i < SomeModName_VehicleData.NUM_GOODS_ENTRIES; i++)
                {
                    if(VData.m_strFormatArray[i] == null)
                    {
                        Logger.dbgLog("StringFormatingArray[" + i + "] was null!");
                        continue;
                    }

                    switch (i)
                    {
                        case VD.Goods:
                            m_GoodsValue.text = string.Format(sVALUE_FSTRING, VData.m_strFormatArray[i]);
                            break;
                        case VD.Grain:
                            m_GrainValue.text = string.Format(sVALUE_FSTRING, VData.m_strFormatArray[i]);
                            break;
                        case VD.Oil:
                            m_OilValue.text = string.Format(sVALUE_FSTRING, VData.m_strFormatArray[i]);
                            break;
                        case VD.Food:
                            m_FoodValue.text = string.Format(sVALUE_FSTRING, VData.m_strFormatArray[i]);
                            break;
                        case VD.Taxi:
                            m_TaxiValue.text = string.Format(sVALUE_FSTRING, VData.m_strFormatArray[i]);
                            break;
                        default:
                            break;
                    }
                }

                if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL >= 3) Logger.dbgLog("Refreshing display data completed. " + DateTime.Now.ToString(DTMilli));
            }
            catch (Exception ex)
            {
                isRefreshing = false;
                Logger.dbgLog("Error during RefreshDisplayData. ",ex,true);
            }
            isRefreshing = false;

        }




        /// <summary>
        /// Handle action for Hide\Show events.
        /// </summary>
        private void ProcessVisibility()
        {
            if (!this.isVisible)
            {
                this.Show();
                if (!CoDisplayRefreshEnabled) { this.StartCoroutine(RefreshDisplayDataWrapper()); }
            }
            else
            {
                this.Hide();
                //we don't have to stop the above coroutine, 
                //should do that itself via it's own 'visibility' checks.
            }
        
        }

        /// <summary>
        /// Handles the Log data button press event by building the text of the last stats snapshot.
        /// and writing it to the log file.
        /// </summary>
        private void ProcessOnLogButton()
        {
            try
            {
                // Loop though the text of our text_components grab the text from each then the text from the associated
                // Value component and build a big string with all this data.
                // then go and write that to the log.
                // This an example where having ref's to our components added to a dictionary can help
                // instead of trying to build this list by pulling\calculating raw data yet again (more time consuming)
                // Example of just one of many ways to do this to sort of "print your screen".
                List<UILabel> tmpList = new List<UILabel>();
                if (_txtControlContainer.Count == _valuesControlContainer.Count)  //do we match?
                {
                    UILabel tmpUIL; string[] tmpspliter;
                    System.Text.StringBuilder sb1 = new System.Text.StringBuilder(2048);
                    sb1.AppendLine("\r\n" + m_HeaderDataText.text + "\r\n");
                    foreach (KeyValuePair<string, UILabel> kvp in _txtControlContainer)
                    {
                        tmpspliter = kvp.Key.Split('_');
                        int tmpnum;
                        if (int.TryParse(tmpspliter[2].ToString(), out tmpnum))
                        {
                            if (_valuesControlContainer.TryGetValue((TAG_VALUE_PREFIX + tmpnum.ToString()), out tmpUIL))
                            {
                                sb1.AppendLine(string.Concat(kvp.Value.text, tmpUIL.text));
                            }
                        }
                    }
                    Logger.LargeLog(sb1.ToString(), 8192);
                }
                else
                {
                    Logger.dbgLog("Missmatched control container counts.");
                }

            }
            catch (Exception ex)
            {
                Logger.dbgLog("Error while dumping data to log.", ex, true);
            }

        }



        private void PopulateControlContainers()
        {
            foreach (UILabel ul in this.GetComponentsInChildren<UILabel>(true))
            {
                if (ul.name.Contains(TAG_TEXT_PREFIX))
                {
                    _txtControlContainer.Add(ul.name, ul);
                    continue;
                }
                if (ul.name.Contains(TAG_VALUE_PREFIX))
                {
                    _valuesControlContainer.Add(ul.name, ul);
                }
            }
            if (SomeModName.DEBUG_LOG_ON & SomeModName.DEBUG_LOG_LEVEL >= 2)
            { 
                Logger.dbgLog(String.Concat("Populated UI controls into containers. ", _txtControlContainer.Count.ToString(),"|",
                _valuesControlContainer.Count.ToString())); 
            }
        }

       

    }
}
