using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;

namespace SomeMod
{
    public class SomeModName_VehicleData
    {
        //I do this for simpliciy, you could use a simple enum but then you always have to explicitly convert to int. Annoying
        //I avoid it with useding static class with just constants.
        internal static class GoodsType
        {
            public const int Goods = 0;
            public const int Grain = 1;
            public const int Oil = 2;
            public const int Food = 3;
            public const int Taxi = 4;

            public const int CountDataTotal = 0;
            public const int CargoDataTotal = 1;
            public const int CountDataLocal = 2;
            public const int CargoDataLocal = 3;
            public const int CountDataImports = 4;
            public const int CargoDataImports = 5;
            public const int CountDataExports = 6;
            public const int CargoDataExports = 7;

        }
        private VehicleManager vMgr = Singleton<VehicleManager>.instance; //Get a reference to Vehicle Manager.
        internal const byte NUM_GOODS_ENTRIES = 5;
        internal const byte NUM_GOODS_DATAVALUES = 8;
        public int[,] m_InfoArray = new int[NUM_GOODS_ENTRIES,NUM_GOODS_DATAVALUES]; //store all our information. 5rows\8columns
        public object[][] m_strFormatArray = new object[NUM_GOODS_ENTRIES][];  //store the text verson of our data. We store it this was for use by string.format();
        // Now you could go making a boat load of public properties to extract each piece of info
        // here, someone would likely tell you that would be right way to do it...but seems a total waste to me
        // just make it a public array and be done with it.

        
        public SomeModName_VehicleData()
        {
            try
            {
                //go create the sub arrays that will hold our string data.
                for (byte i = 0; i < m_strFormatArray.Length; i++)
                {
                    m_strFormatArray[i] = new object[NUM_GOODS_DATAVALUES];
                }
            }
            catch (Exception ex)
            { Logger.dbgLog("Error during contruction. ", ex, true); }

        }


        /// <summary>
        /// Wrapper function that calls into the game and fills\refills our object with all it's data.
        /// </summary>
        public void RefreshData()
        {
            ResetAllData();
            FetchTransferTypeData();
            FillsFormatArray();
        }

        /// <summary>
        /// Clears all currently stored vehicle cargo and count data.
        /// </summary>
        public void ResetAllData(bool bFlushText = false)
        {
            try
            {
                for (int i = 0; i < m_InfoArray.GetLength(0); i++)
                {
                    for (int j = 0; j < m_InfoArray.GetLength(1); j++)
                    {
                        m_InfoArray[i, j] = 0;
                    }
                }

                if (!bFlushText) { return; } //no need to continue
                for (int i = 0; i < m_InfoArray.GetLength(0); i++)
                {
                    for (int j = 0; j < m_InfoArray.GetLength(1); j++)
                    {
                        m_strFormatArray[i][j] = "0";
                    }
                }
            }
            catch (Exception ex)
            { Logger.dbgLog("", ex, true); }

        }



        private void FillsFormatArray()
        {
            try
            {
                for (int i = 0; i < m_InfoArray.GetLength(0); i++)
                {
                    for (int j = 0; j < m_InfoArray.GetLength(1); j++)
                    {
                        m_strFormatArray[i][j] = m_InfoArray[i, j].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.dbgLog("error converting int arrays to string array", ex, true);
            }
        }


        private void FetchTransferTypeData()
        {
            try
            {
                for (ushort i = 1; i < vMgr.m_vehicles.m_buffer.Length; i++)
                {
                    Vehicle v = vMgr.m_vehicles.m_buffer[i];
                    //first we need to only trigger on the vehicles we're interested in, they must be created and have info objects.
                    if ((v.m_flags & Vehicle.Flags.Created) == Vehicle.Flags.Created && v.Info != null)
                    {
                        //Ok so we're flagged as created
                        if(v.Info.m_vehicleType == VehicleInfo.VehicleType.Bicycle | v.Info.m_vehicleType == VehicleInfo.VehicleType.None)
                        {
                            continue; //skip to next one we don't care about bikes or 'none'.
                        }
                        //Is it doing anything we are interested in at all?
                        if ((v.m_flags & Vehicle.Flags.Importing) == Vehicle.Flags.Importing |
                           (v.m_flags & Vehicle.Flags.Exporting) == Vehicle.Flags.Exporting |
                           (v.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget)
                        {
                            //Ok now we need to check if it has any transfer types we're interested in
                            if (v.m_transferType == (byte)TransferManager.TransferReason.Goods |
                                v.m_transferType == (byte)TransferManager.TransferReason.Grain |
                                v.m_transferType == (byte)TransferManager.TransferReason.Oil |
                                v.m_transferType == (byte)TransferManager.TransferReason.Food |
                                v.m_transferType == (byte)TransferManager.TransferReason.Taxi)
                            {
                                //oh nice it does, ok let's go get some data from this one
                                FetchSpecificData(v);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.dbgLog("", ex, true);
            }
        }

        /// <summary>
        /// This function figures out what function should be run for the particular transfer type.
        /// </summary>
        /// <param name="v"> The Vehicle object you want to examine</param>
        private void FetchSpecificData(Vehicle v)
        {
            try
            {
                // Right now this is a switch() it since atm there are only two an if would be about the same
                // I expect if one choose to expand the sample though you might have more then one special handler.
                switch (v.m_transferType)
                {
                    case (byte)TransferManager.TransferReason.Goods:
                        ExtractGenericData(v, GoodsType.Goods);
                        break;
                    case (byte)TransferManager.TransferReason.Grain:
                        ExtractGenericData(v, GoodsType.Grain );
                        break;
                    case (byte)TransferManager.TransferReason.Oil:
                        ExtractGenericData(v, GoodsType.Oil);
                        break;
                    case (byte)TransferManager.TransferReason.Food:
                        ExtractGenericData(v, GoodsType.Food);
                        break;
                    case (byte)TransferManager.TransferReason.Taxi:
                        ExtractTaxiData(v, GoodsType.Taxi); // <-- note we have special type here; maybe there will be others
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.dbgLog("", ex, true);
            }
        }


        /// <summary>
        /// This function gathers the actual data and populates the array with information.
        /// </summary>
        /// <param name="v">The Vehicle object to count on</param>
        /// <param name="iGoodsType">The array index value of the type of goods info we're extracting (good,grain,oil,etc)</param>
        private void ExtractGenericData(Vehicle v, int iGoodsType)
        {
            try
            {
                m_InfoArray[iGoodsType, GoodsType.CountDataTotal]++; //up the global # veh counter for type.
                m_InfoArray[iGoodsType, GoodsType.CargoDataTotal] += v.m_transferSize; //add to global cargo figure for type.

                //do imports and exports first and exit after...cause they also have "Transfer to target"
                if ((v.m_flags & Vehicle.Flags.Importing) == Vehicle.Flags.Importing)
                {
                    m_InfoArray[iGoodsType, GoodsType.CountDataImports]++;
                    m_InfoArray[iGoodsType, GoodsType.CargoDataImports] += v.m_transferSize;
                    return;
                }
                if ((v.m_flags & Vehicle.Flags.Exporting) == Vehicle.Flags.Exporting)
                {
                    m_InfoArray[iGoodsType, GoodsType.CountDataExports]++;
                    m_InfoArray[iGoodsType, GoodsType.CargoDataExports] += v.m_transferSize;
                    return;
                }
                //So if they were not importing or exporting but they are transfering, assume it's "local" - inside city?
                if ((v.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget)
                {
                    m_InfoArray[iGoodsType, GoodsType.CountDataLocal]++;
                    m_InfoArray[iGoodsType, GoodsType.CargoDataLocal] += v.m_transferSize;
                }
            }
            catch (Exception ex)
            {
                Logger.dbgLog("", ex, true);
            }
        }

        /// <summary>
        /// This function gathers the actual data and populates the array with information for TAXI's.
        /// </summary>
        /// <param name="v">The Vehicle object to count on</param>
        /// <param name="iGoodsType">The array index value of the type of goods info we're extracting (good,grain,oil,etc)</param>
        private void ExtractTaxiData(Vehicle v, int iGoodsType)
        {
            try
            {
                m_InfoArray[iGoodsType, GoodsType.CountDataTotal]++; //up the global # veh counter for type.
                m_InfoArray[iGoodsType, GoodsType.CountDataLocal]++; //first lets up our count cause it exists right?
                //ok lets add our passengers as the local cargo if there were any.

                // please note I'm not sure this isn't really the correct passenger amounts.
                //I think they live in the v.m_citizenUnit objects where you have to get the CitizenUnit for the vehicle
                // and then loop though all 'next_units' in that object.
                // current this is returning a number that I think is really only the running total
                // of the number of trips so far... it's very strange that running total is being stored in this var.
                // the "count" data above however is accurate.
                m_InfoArray[iGoodsType, GoodsType.CargoDataTotal] += v.m_transferSize;
                m_InfoArray[iGoodsType, GoodsType.CargoDataLocal] += v.m_transferSize; 
            }
            catch (Exception ex)
            {
                Logger.dbgLog("", ex, true);
            }
        }

    }
}
