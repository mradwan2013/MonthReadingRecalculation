using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.IO.Ports;
using System.Data;
using System.Drawing;
using System.Xml;
using System.Drawing.Printing;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartWaterMeter;

namespace MonthReadingRecalculation
{
    enum ConfigurationType : int
    {
        Optical = 1,
        Card = 2
    }

    static class Utility
    {
        // Maximum meter identifier length
        const int MaxMeterID = int.MaxValue;
        const int base10 = 10;
        static char[] cHexa = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };
        static int[] iHexaNumeric = new int[] { 10, 11, 12, 13, 14, 15 };
        static int[] iHexaIndices = new int[] { 0, 1, 2, 3, 4, 5 };
        const int asciiDiff = 48;
        //static CustomerCardRepository _customerCardRepo;
        //static SellingCardRepository _sellingCardRepo;
        static VersionCountry version_Country = VersionCountry.WaterRFID;

        // Last login datetime
        public static System.DateTime cashedServerDateTime = System.DateTime.MinValue;
        public static string cashedServerDateTimeFormated = "01/01/1900";

        /// <summary>
        /// Get meter activity,unitNo,sewage in specific date
        /// </summary>
        /// <param name="meterId">Meter identifier</param>
        /// <param name="activity">Activity</param>
        /// <param name="PhaseNo">Sewage status</param>
        /// <param name="GUcode">Unit no</param>
        /// <param name="specificDate">Specific date</param>
        public static void GetMeterCatagoryData(string meterId, ref string activity, ref int PhaseNo, ref int GUcode, System.DateTime specificDate)
        {
            try
            {
                dboperation db = new dboperation();
                db.objcmd.Parameters.Clear();
                db.objcmd.CommandType = CommandType.StoredProcedure;
                db.objcmd.CommandText = "GetMeterRecalculationMainData";
                db.objcmd.Parameters.AddWithValue("@MeterId", meterId);                         // Meter identifier
                db.objcmd.Parameters.AddWithValue("@SpecificDate", specificDate);               // Specific date
                DataTable dt = db.SelectData("");

                if (dt != null && dt.Rows.Count > 0)
                {
                    activity = dt.Rows[0]["ActivityId"].ToString();
                    GUcode = int.Parse(dt.Rows[0]["GuCode"].ToString());
                    PhaseNo = int.Parse(dt.Rows[0]["PhaseNo"].ToString());
                }
            }
            catch (Exception ex)
            {
               // MakeExceptionLog("Utility", "GetMeterCatagoryData", ex);
            }
        }

        /// <summary>
        /// Get name of activity
        /// </summary>
        /// <param name="id">Code of activity</param>
        /// <returns>Name of activity</returns>
        public static string GetActivityNameByID(string Id)
        {
            try
            {
                dboperation db = new dboperation();
                return db.ReturnStr("select Name from Activities with(nolock) where ID = '" + Id + "'");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Update meter state
        /// </summary>
        /// <param name="aMeterState"></param>
        /// <param name="bValveStatus"></param>
        /// <param name="CloseValveReason"></param>
        /// <param name="meterstatebin"></param>
        /// <param name="BatteryStatus"></param>
        /// <param name="resopn"></param>
        /// <param name="ValveStatus"></param>
        /// <param name="batresopn"></param>
        public static void Meterstate(int aMeterState, bool isOldMetersAndV5V8, ref bool bValveStatus, ref int CloseValveReason,
            ref char[] meterstatebin, ref int BatteryStatus, ref string resopn, ref string ValveStatus, ref string batresopn)
        {
            ValveStatus = "";
            bValveStatus = false;
            CloseValveReason = 0;
            string Ans = Convert.ToString(aMeterState, 2);

            if (version_Country == VersionCountry.WaterRFIDEGYPTTENDER)
            {
                #region Explanation_Meterstate for EL05 杜工@20170504.txt
                /*
             "Meterstate"   is  "word format" , and also can be treat as 2 bytes:  MeterState0|MeterState1
        we can explain the details based bit.(A total of 16 bits,but only 9 bits are significant )
                     XYZF8421       XYZF8421
                     DEFGHIJK       DEFGHIJK
             * 
        #define   meterstate0_State_Remain              (0x01&MeterState0)    =0   meter's RemainCredit >0                                                                                          =1  inverse
        #define   meterstate0_State_Overdraft           (0x02&MeterState0)    =1   meter's OverdraftCredit >0                                                                                       =0  inverse
        #define   meterstate0_StateBatPowerLow          (0x04&MeterState0)    =1   Batterry's  power is low                                                                                         =0  Batterry power is operable 
        #define   meterstate0_StateOverdraftThreshold   (0x10&MeterState0)    =1   meter's OverdraftCredit>=OverdraftThreshold(parameter can be set to meter by card)                               =0  inverse
        #define   meterstate0_StateRemainWaring         (0x20&MeterState0)    =1   meter's RemainCredit<=RemainWaring(parameter can be set to meter by card)                                        =0  inverse 
        #define   meterstate0_StateBatPowerDown         (0x40&MeterState0)    =1   Batterry's  power is run out can't be used any more                                                              =0  can be used  
        #define   meterstate0_StateDisable              (0x80&MeterState0)    =1   meter is in "disable state"(by set card), and valve will be closed until meter's state change to "enable state"  =0  enable state  
        #define   meterstate1_State_CoverErr            (0x08&MeterState1)    =1  meter's cover had be opened  =0 meter's CoverState is  right
        #define   meterstate1_State_Valve               (0x10&MeterState1)    =0  meter's valve is closed  =1  meter's valve is opened  

             *       1 0  0 1     0001    0   0      0   0  0100
             *       0 0  1  0    0011    1   0      0   1  0000
             *      80 40 20 10   8421    80  40     20  10 8421
             *      144
             *      0 0 0 0 0000  1001 0000
             *      
             */
                #endregion


                resopn = "";
                batresopn = "حالة البطارية : جيدة ";
                BatteryStatus = 0;

                if (isOldMetersAndV5V8)
                {
                    Ans = Ans.PadLeft(16, '0');
                    meterstatebin = Ans.ToCharArray();

                    if (meterstatebin[0] == '1')
                    {
                        resopn = " لتعطيل العداد بواسطة فنى ";
                        CloseValveReason = 1;
                    }

                    if (meterstatebin[1] == '1')
                    {
                        batresopn = batresopn = " حالة البطارية : لا يمكن استخدامها";
                        if (BatteryStatus == 0) BatteryStatus = 2;
                    }

                    if (meterstatebin[2] == '1') resopn = "الرصيد المتبقى اقل من حد الفصل";

                    if (meterstatebin[3] == '1') resopn = "استهلاك السماحية اكبر من حد السماحية ";

                    if (meterstatebin[5] == '1' && meterstatebin[1] == '0')
                    {
                        batresopn = " حالة البطارية : ضعيفة من فضلك قم بتغيرها فى أسرع وقت";
                        if (BatteryStatus == 0) BatteryStatus = 1;
                    }

                    if (meterstatebin[5] == '1' && meterstatebin[1] == '1')
                    {
                        batresopn = " حالة البطارية : ضعيفة لا يمكن استخدامها";
                        if (BatteryStatus == 2) BatteryStatus = 3;
                    }

                    if (meterstatebin[6] == '1') resopn = "meter's OverdraftCredit >0 ";

                    if (meterstatebin[7] == '1' || meterstatebin[2] == '1')
                    {
                        resopn = " بسبب صفر الرصيد ";
                        CloseValveReason = 2;
                    }

                    if (meterstatebin[12] == '1')
                    {
                        resopn = " لفتح الغطاء ";
                        bValveStatus = true;
                        CloseValveReason = 3;
                    }

                    if (meterstatebin[11] == '0')
                    {
                        bValveStatus = true;

                        //if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                            ValveStatus = " حالة المحبس : مغلق  " + " ---- " + resopn;
                        //else
                        //    ValveStatus = "Valve status : Closed ";
                    }
                    else
                    {
                        bValveStatus = false;

                       // if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                            ValveStatus = "حالة المحبس : مفتوح ";
                        //else
                        //    ValveStatus = "Valve status : Open ";
                    }
                }
                else
                {
                    meterstatebin = Convert.ToString(aMeterState, 2).PadLeft(8, '0').ToCharArray();

                    if (meterstatebin[0] == '0')
                    {
                       // if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                            batresopn = " حالة البطارية : ضعيفة من فضلك قم بتغيرها فى أسرع وقت";
                        //else
                        //    batresopn = " Battery Low";
                    }


                    if (meterstatebin[1] == '0')
                    {
                        bValveStatus = true;

                       // if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                            ValveStatus = " حالة المحبس : مغلق  " + " ---- " + resopn;
                        //else
                        //    ValveStatus = "Valve status : Closed ";
                    }
                    else
                    {
                        bValveStatus = false;

                       // if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                            ValveStatus = "حالة المحبس : مفتوح ";
                        //else
                        //    ValveStatus = "Valve status : Open ";
                    }
                }
            }
            else if (version_Country == VersionCountry.WaterRFID)
            {
                /*
                 The integer returned and translated into octet binary system explained from the last number to the front 
                    Bit0: state of value
                    Bit1: state of lower power
                    Bit2: state of seriously lower power
                    Bit3: state of opening time is enabled
                    Bit4: state of overdraft
                    Bit5: state of alarm credit
                    Bit6: state of balance lower than 0.1 CBM
                    Bit7: state of normal prepaid meter

                 *  Bit0: 0- valve open;1- valve close
                    Bit1:0- normal; 1- lower power (power is available for one month)
                    Bit2: 0- normal; 1- seriously lower power(valve close, waiting for change battery) 
                    *****Bit3: 0- disabled state; 1- enabled state 
                    Bit4: 0- normal; 1- overdraft state
                    Bit5: 0- normal; 1- in alarm credit 
                    Bit6: 0- normal; 1- in this state
                    Bit7: 0- normal; 1-forbidden state, valve close

                 * //// 0   0  0 0    0   0  0 1
                 * 
                 */
                Ans = Ans.PadLeft(8, '0');
                meterstatebin = Ans.ToCharArray();
                resopn = "";
                batresopn = "حالة البطارية : جيدة ";
                batresopn = "Battery status : Good ";
                BatteryStatus = 0;

                if (meterstatebin[3] == '1')
                {
                    resopn = " لتعطيل العداد بواسطة فنى ";
                    resopn = "disabled state";
                    CloseValveReason = 1;
                }

                if (meterstatebin[1] == '1')
                {
                    //"Batterry's  power is run out can't be used any more ";
                    batresopn = batresopn = " حالة البطارية : لا يمكن استخدامها";
                    batresopn = "Battery status : lower power (power is available for one month)";
                    if (BatteryStatus == 0) BatteryStatus = 2;
                }

                if (meterstatebin[2] == '1')
                {
                    //"Batterry's  power is low";
                    batresopn = " حالة البطارية : ضعيفة من فضلك قم بتغيرها فى أسرع وقت";
                    batresopn = " seriously lower power(valve close, waiting for change battery) ";
                    if (BatteryStatus == 0) BatteryStatus = 1;
                }

                if (meterstatebin[5] == '1')
                {
                    resopn = "الرصيد المتبقى اقل من حد الفصل";
                    resopn = "in alarm credit";
                }

                if (meterstatebin[4] == '1')
                {
                    resopn = "استهلاك السماحية اكبر من حد السماحية ";
                    resopn = "overdraft state";
                }

                if (meterstatebin[0] == '1')
                {
                    bValveStatus = true;

                   // if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                        ValveStatus = " حالة المحبس : مغلق  " + " ---- " + resopn;
                    //else
                    //    ValveStatus = "Valve status : Closed ";
                }
                else
                {
                    bValveStatus = false;

                  //  if (Multilingual.BaseForm.ApplicationLanguage.Contains("Arabic"))
                        ValveStatus = "حالة المحبس : مفتوح ";
                    //else
                    //    ValveStatus = "Valve status : Open ";
                }
            }
        }

        ///// <summary>
        ///// Add month reading
        ///// </summary>
        ///// <param name="rc">Card</param>
        ///// <param name="isAddCurrentMonth">Flag for (Add or skip) current month</param>
        ///// <param name="MeterVersionType">Meter version</param>
        ///// <returns>Add result</returns>
        //public static bool addMonthReadingtoDB(ReadCard rc, bool isAddCurrentMonth = false, int MeterVersionType = 0)
        //{
        //    try
        //    {
        //        dboperation DB1 = new dboperation();
        //        SellingCardRepository _sellingCardRepo = new SellingCardRepository();
        //        int rmnth = 0;
        //        int ryr = 0;

        //        // Get meter identifer
        //        string meterid = GetMeterID2(rc.aConsumerID.ToString(), MeterVersionType.ToString());

        //        if (frmMain.Version_Country == VersionCountry.WaterRFIDEGYPTTENDER)
        //        {
        //            int phaseno = 1;
        //            int Gucode = 1;
        //            int aConsumerType = 0;
        //            string meterActivty = string.Empty;
        //            System.DateTime min = new System.DateTime(2017, 1, 1);
        //            rc.CardTime = rc.aSysTime;

        //            if (rc.CardTime == null || rc.CardTime < min)
        //                return false;

        //            rmnth = rc.CardTime.Month;
        //            ryr = rc.CardTime.Year;

        //            // Get meter working date
        //            dboperation db = new dboperation();
        //            string sql = "select WorkingDate,ActivityID,PhaseNo,GuCode , (SELECT top 1 serverDate from charges with(nolock) where type = 0 and meterid = '" + meterid + "' and (MakeCard = 1 or MakeCard IS NULL) order by serverDate desc) as ReplaceMeterDate from meters with(nolock) where meterid like '" + meterid + "'";
        //            var meterDT = db.SelectData(sql);
        //            var WorkingDate = System.DateTime.Parse(meterDT.Rows[0]["WorkingDate"].ToString());
        //            var ReplacementDate = string.IsNullOrEmpty(meterDT.Rows[0]["ReplaceMeterDate"]?.ToString()) ? WorkingDate : System.DateTime.Parse(meterDT.Rows[0]["ReplaceMeterDate"]?.ToString());

        //            var installationdate = ReplacementDate > WorkingDate ? ReplacementDate : WorkingDate;

        //            // Get activity code from card
        //            if (MeterVersionType == (int)MeterTypeEnum.Water_EGRFID || MeterVersionType == (int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch)
        //            {
        //                aConsumerType = EGYGetActivityCodeinMeter(rc.aConsumerType, ref phaseno, ref Gucode, meterid);
        //                meterActivty = meterDT.Rows[0][1].ToString();
        //                rc.deduction = 0;
        //            }
        //            else
        //            {
        //                aConsumerType = rc.aConsumerType;
        //                meterActivty = aConsumerType.ToString();
        //                phaseno = rc.meterPhaseNo;
        //                Gucode = rc.meterUnitsNo;
        //            }

        //            // Add db number prefix
        //            if (aConsumerType.ToString() != "1")
        //                meterActivty = frmMain.DBNumber + "-" + aConsumerType.ToString();

        //            // Check activity exists                   
        //            if (!_sellingCardRepo.ValidateActivityExists(meterActivty))
        //            {
        //                // Get activity code from meter
        //                meterActivty = meterDT.Rows[0][1].ToString();
        //            }

        //            // Get month count difference
        //            var lastReadingCount = GetLastMonthReadingMonthDiff(meterid);

        //            if (lastReadingCount > 12)
        //                lastReadingCount = 12;

        //            try
        //            {
        //                double deductionValue = 0;
        //                int month = 0;
        //                string Monthrate = string.Empty;
        //                decimal tarrifAdjustmentValue = 0;

        //                if (MeterVersionType != (int)MeterTypeEnum.Water_EGRFID && MeterVersionType != (int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch)
        //                {
        //                    #region Add month reading for new meters

        //                    for (int i = 0; i < lastReadingCount; i++)
        //                    {
        //                        var lastTransDate = rc.LastTransDate != null ? rc.LastTransDate.Value : System.DateTime.Now;

        //                        ryr = lastTransDate.Year;

        //                        if (lastTransDate.Month - (i + 1) <= 0)
        //                        {
        //                            ryr = lastTransDate.Year - 1;
        //                        }

        //                        month = lastTransDate.AddMonths(-1 * (i + 1)).Month;

        //                        if (month == 0 || month > 12)
        //                        {
        //                            month = 12;
        //                        }

        //                        Monthrate = _sellingCardRepo.GetMeterMonthDeduction(meterid, month, ryr);
        //                        deductionValue = string.IsNullOrEmpty(Monthrate) ? deductionValue : double.Parse(Monthrate);

        //                        if (rc.MonthQuantity[i] >= 0)
        //                        {
        //                            if (rc.MonthQuantity[i] == 0)
        //                            {
        //                                // Skip months before meter installment
        //                                int maxMonthDay = System.DateTime.DaysInMonth(ryr, month);
        //                                maxMonthDay = maxMonthDay < installationdate.Day ? maxMonthDay : installationdate.Day;

        //                                if (installationdate.Date > new System.DateTime(ryr, month, maxMonthDay).Date)
        //                                {
        //                                    continue;
        //                                }
        //                            }

        //                            tarrifAdjustmentValue = 0;

        //                            // Insert month reading for new meters 
        //                            SaveWaterMonthReadings(meterid, ryr, month, (decimal)(rc.MonthQuantity[i]), (decimal)(rc.UsedMonthly[i]), (decimal)(rc.aFixFee / 100), meterActivty, phaseno, Gucode, ref tarrifAdjustmentValue, deductionValue, rc.deduction, MeterVersionType, rc.aConsumerType);
        //                        }
        //                    }

        //                    if (isAddCurrentMonth)
        //                    {
        //                        // Add Current Month in case damaged meter retrieve data
        //                        Monthrate = _sellingCardRepo.GetMeterMonthDeduction(meterid, rmnth, ryr);
        //                        deductionValue = string.IsNullOrEmpty(Monthrate) ? deductionValue : double.Parse(Monthrate);
        //                        SaveWaterMonthReadings(meterid, ryr, rmnth, (decimal)(rc.QuantityTotal), (decimal)(rc.aConsumedCredit), (decimal)(rc.aFixFee / 100), meterActivty, phaseno, Gucode, ref tarrifAdjustmentValue, deductionValue, rc.deduction, MeterVersionType, rc.aConsumerType);
        //                    }

        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region Add month reading for V5,V8 meters

        //                    for (int i = 0; i < 12; i++)
        //                    {
        //                        rmnth = i;

        //                        if (i < rc.CardTime.Month)
        //                        {
        //                            ryr = rc.CardTime.Year;
        //                        }
        //                        else
        //                        {
        //                            ryr = rc.CardTime.Year - 1;
        //                        }

        //                        if (i == 0)
        //                        {
        //                            rmnth = 12;
        //                            ryr = rc.CardTime.Year - 1;
        //                        }

        //                        if (rc.MonthQuantity[i] >= 0)
        //                        {
        //                            if (rc.MonthQuantity[i] == 0)
        //                            {
        //                                int maxMonthDay = System.DateTime.DaysInMonth(ryr, rmnth);
        //                                maxMonthDay = maxMonthDay < installationdate.Day ? maxMonthDay : installationdate.Day;

        //                                if (installationdate.Date > new System.DateTime(ryr, rmnth, maxMonthDay).Date)
        //                                {
        //                                    continue;
        //                                }
        //                            }

        //                            Monthrate = _sellingCardRepo.GetMeterMonthDeduction(meterid, rmnth, ryr);
        //                            deductionValue = string.IsNullOrEmpty(Monthrate) ? deductionValue : double.Parse(Monthrate);
        //                            tarrifAdjustmentValue = 0;

        //                            // Insert month reading for old meters [V5,V8]
        //                            SaveWaterMonthReadings(meterid, ryr, rmnth, (decimal)(rc.MonthQuantity[i]), 0, (decimal)(rc.aFixFee), meterActivty, phaseno, Gucode, ref tarrifAdjustmentValue, deductionValue, 0, MeterVersionType, rc.aConsumerType);
        //                        }
        //                    }

        //                    # region Add Bulk adjustment 

        //                    if (lastReadingCount > 0)
        //                    {
        //                        // Recalc here then add adjustment then update last water meter reading
        //                        var lastRecalcDate = GetMeterLastRecalc(meterid);

        //                        if (lastRecalcDate != null && lastRecalcDate.Rows.Count > 0)
        //                        {
        //                            // 1- Determine sum of monthreadings consumptions + old adjustments
        //                            decimal SystemOldConsumption = 0;

        //                            if (!string.IsNullOrEmpty(lastRecalcDate.Rows[0]["SumOfConsumption"].ToString()))
        //                            {
        //                                SystemOldConsumption = decimal.Parse(lastRecalcDate.Rows[0]["SumOfConsumption"].ToString());
        //                            }

        //                            // 2- Determine initial charge
        //                            decimal InitialCharge = 0;

        //                            if (!string.IsNullOrEmpty(lastRecalcDate.Rows[0]["InitialCharge"].ToString()))
        //                            {
        //                                InitialCharge = decimal.Parse(lastRecalcDate.Rows[0]["InitialCharge"].ToString());
        //                            }

        //                            // 3- Determine card current consumption
        //                            var CardConsumption = (decimal)rc.aConsumedCredit - InitialCharge;

        //                            if (SystemOldConsumption > 0)
        //                            {
        //                                // Get last success charge tarrifa details
        //                                SellingCardRepository _sellingCardRepository = new SellingCardRepository();
        //                                DataTable lastChargeDT = _sellingCardRepository.GetMeterLastSuccessCharge(meterid);
        //                                System.DateTime lastChargeTarrifDate = rc.CardTime;

        //                                if (lastChargeDT.Rows.Count > 0)
        //                                {
        //                                    lastChargeTarrifDate = Convert.ToDateTime(lastChargeDT.Rows[0]["TariffStartDate"].ToString());
        //                                    phaseno = int.Parse(lastChargeDT.Rows[0]["PhaseNo"].ToString());
        //                                    meterActivty = lastChargeDT.Rows[0]["ActivityID"].ToString();
        //                                    Gucode = int.Parse(lastChargeDT.Rows[0]["UnitNo"].ToString());
        //                                }

        //                                // 4- Determine current month consumption in money
        //                                var CurrentMonthSystemConsumption = CalcMeterWaterConsumptionForSpecificRegion(meterid, (decimal)rc.ThisMonthQuantity, meterActivty, Gucode, phaseno, lastChargeTarrifDate, false);

        //                                var calcDiff = SystemOldConsumption - (CardConsumption - CurrentMonthSystemConsumption);

        //                                if (calcDiff > (decimal)0.1)
        //                                {
        //                                    var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //                                    var MaxInstalmentsAmount = frmMain.MaxInstalmentsAmount;
        //                                    var DefaultInstalmentsNumber = frmMain.DefaultInstalmentsNumber;
        //                                    var monthCount = 1;
        //                                    var monthrate = calcDiff;

        //                                    if (calcDiff > MaxInstalmentsAmount && MaxInstalmentsAmount != 0)
        //                                    {
        //                                        monthCount = DefaultInstalmentsNumber;
        //                                        monthrate = calcDiff / DefaultInstalmentsNumber;
        //                                    }

        //                                    var DBobj = new mgrAdjustments();
        //                                    DBobj.AddAdjustment(meterid, calcDiff.ToString(), monthCount.ToString(), monthrate.ToString(), frmMain.OldConsumptionRecalculationCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 0, null, " حتى يوم  " + rc.CardTime.Day + "  شهر  " + rc.CardTime.Month + " لسنة " + rc.CardTime.Year);
        //                                }
        //                            }
        //                        }

        //                        // Update last water meter reading to br recalc true
        //                        UpdateLastWaterMeterReading(meterid);
        //                    }

        //                    #endregion

        //                    #endregion
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MakeExceptionLog("Utility", "Add month reading calculate months", ex);
        //            }
        //        }
        //        else
        //        {
        //            decimal FixFee = GetMeterEstidama(meterid);

        //            if (rc.Smart)
        //            {
        //                if (!(rc.readyear > 0 && rc.readmnth > 0 && rc.readday > 0))
        //                    return false;
        //                rmnth = rc.readmnth;
        //                ryr = rc.readyear + 2000;
        //            }
        //            else
        //            {
        //                System.DateTime min = new System.DateTime(2013, 1, 1);
        //                rc.CardTime = System.DateTime.Now;
        //                if (rc.CardTime == null || rc.CardTime < min)
        //                    return false;
        //                rmnth = rc.CardTime.Month;
        //                ryr = rc.CardTime.Year;
        //            }

        //            for (int i = 0; i < 12; i++)
        //            {
        //                if (rmnth > 1)
        //                    rmnth = rmnth - 1;
        //                else
        //                {
        //                    ryr = ryr - 1;
        //                    rmnth = 12;
        //                }

        //                if (rc.Smart)
        //                {
        //                    if (rc.monthcon[i] > 0)
        //                    {
        //                        // Save month reading for old meters [smart meter]
        //                        if (!SaveDbMonthReadings(meterid, ryr, rmnth, (decimal)(rc.monthcon[i]), (decimal)(rc.UsedMonthly[i]), FixFee, (decimal)(rc.aFixFee), 0, 0, "", (decimal)(rc.UsedMonthly[i]), 0, 0, 0, 0, 0, new DataTable(), 0, 0, new System.DateTime(ryr, rmnth, 1), rc.aConsumerType))
        //                        {
        //                            return false;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (rc.MonthQuantity[i] > 0)
        //                    {
        //                        // Save month reading for old meters [not smart meter]
        //                        if (!SaveDbMonthReadings(meterid, ryr, rmnth, (decimal)(rc.MonthQuantity[i]), (decimal)(rc.UsedMonthly[i]), FixFee, (decimal)(rc.aFixFee), 0, 0, "", (decimal)(rc.UsedMonthly[i]), 0, 0, 0, 0, 0, new DataTable(), 0, 0, new System.DateTime(ryr, rmnth, 1), rc.aConsumerType))
        //                        {
        //                            return false;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "addMonthReadingtoDB", ex);
        //    }

        //    return true;
        //}





















        //#region Methods

        //public static void WaterMeterreading(ReadCard rc)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string meter = GetMeterNumberWithMeterType(rc.aConsumerID.ToString(), rc.meterType.ToString());
        //        string StrMeterId = GetMeterID2(meter);
        //        int Transactionid = 0;
        //        char[] meterstatebin = null;
        //        int BatteryStatus = 0;
        //        string resopn = "";
        //        string batresopn = "حالة البطارية : جيدة ";
        //        string ValveStatus = "";
        //        bool bValveStatus = false;
        //        int CloseValveReason = 0;

        //        Meterstate(rc.aMeterState, true, ref bValveStatus, ref CloseValveReason, ref meterstatebin, ref BatteryStatus, ref resopn, ref ValveStatus, ref batresopn);
        //        int aSysTimeInt = rc.aSysTime.Year * 10000;
        //        aSysTimeInt += rc.aSysTime.Month * 100;
        //        aSysTimeInt += rc.aSysTime.Day;
        //        System.DateTime LastOpenDate = System.DateTime.Parse(rc.LastOpenDate.ToString());
        //        string sLastOpenDate = "";

        //        try
        //        {
        //            if (LastOpenDate == new System.DateTime())
        //            {
        //                sLastOpenDate = null;
        //            }
        //            else
        //                sLastOpenDate = LastOpenDate.ToString();
        //        }
        //        catch
        //        {
        //            sLastOpenDate = null;
        //        }

        //        rc.customIDs = GetCustomerID(StrMeterId);
        //        string sqlret = "select top 1  convert( nvarchar , aQuantityTotal ) as Prev,   GuCode , Sewage  from WaterMetersReadings with(nolock) where MeterID = '" + StrMeterId +
        //                        "' and  convert(datetime ,  aSysTime , 103)   <  convert(datetime , '" + rc.aSysTime.ToString() + "'  , 103)  order by convert(datetime ,  aSysTime , 103)  desc  ";

        //        double summonth = 0;
        //        int prevGuCode = 0;
        //        int prevSewage = 0;
        //        double prevcons = 0;
        //        DataTable Prevdt = db.SelectData(sqlret);

        //        // Get last reading before current reading
        //        if (Prevdt.Rows.Count > 0)
        //        {
        //            sqlret = Prevdt.Rows[0]["Prev"].ToString();
        //            prevGuCode = int.Parse(Prevdt.Rows[0]["GuCode"].ToString());
        //            prevSewage = int.Parse(Prevdt.Rows[0]["Sewage"].ToString());
        //            prevcons = double.Parse(sqlret == "" ? "0" : sqlret);

        //            if (prevcons > rc.QuantityTotal)
        //                prevcons = 0;

        //            sqlret = "select convert(nvarchar ,  sum( aQuantityTotal - PrevQuantityTotal)  )  from WaterMetersReadings with(nolock) where MeterID = '" +
        //                        StrMeterId + "' and  convert(datetime ,  aSysTime , 103)   <  convert(datetime , '" + rc.aSysTime.ToString() +
        //                      "'  , 103) and year (convert(datetime ,  aSysTime , 103) ) *100 + MONTH( convert(datetime ,  aSysTime , 103) ) = " +
        //                      " year (convert(datetime ,  '" + rc.aSysTime.ToString() + "' , 103)  ) *100 + MONTH( convert(datetime ,  '" + rc.aSysTime.ToString() + "' , 103)  ) ";

        //            sqlret = db.ReturnStr(sqlret).ToString();
        //            summonth = double.Parse(sqlret == "" ? "0" : sqlret);
        //        }

        //        DataTable dtTariff = new DataTable();
        //        DataTable meterdata = db.SelectData("select activityid , GuCode , PhaseNo  from meters with(nolock) where meterid ='" + StrMeterId + "'");
        //        string ActivityID = meterdata.Rows[0]["activityid"].ToString();
        //        int GuCode = int.Parse(meterdata.Rows[0]["GuCode"].ToString());
        //        int PhaseNo = int.Parse(meterdata.Rows[0]["PhaseNo"].ToString());

        //        dtTariff = GetTariff(ActivityID);
        //        decimal[] price = getWaterDetail((decimal)(rc.QuantityTotal - prevcons), GuCode, PhaseNo, dtTariff);
        //        decimal EstimatedValue = calcWaterMoneyIncludeUnits(decimal.Parse(rc.aUsedMonthly.ToString()), dtTariff, GuCode);
        //        decimal aoverdraftCredit = decimal.Parse(rc.aOverdraftCredit.ToString());
        //        decimal PositiveCredit = EstimatedValue - aoverdraftCredit;
        //        decimal Positiveconsumption = calcWaterReadingQuantityIncludeUnits(PositiveCredit, dtTariff, GuCode, PhaseNo);
        //        decimal friendlyCBM = decimal.Parse(rc.aUsedMonthly.ToString()) - Positiveconsumption;
        //        decimal[] friendlyprice = getWaterDetailByCreditandConsumption(friendlyCBM, aoverdraftCredit, ActivityID);
        //        rc.aSysTime.AddSeconds(-rc.aSysTime.Second);

        //        SellingCardRepository _sellingCardRepository = new SellingCardRepository();
        //        var result = InsertWaterReading(rc.aConsumerType.ToString(), (rc.aRemainCredit + rc.aConsumedCredit).ToString(), Transactionid.ToString() + '-' + rc.index.ToString(), "MassRetrivalCard", StrMeterId, rc.customIDs, rc.aMeterState.ToString(),
        //            rc.aOverdraftCredit.ToString(),
        //            rc.aQuantityTotalNeg.ToString(), rc.aSysTime.AddSeconds(-rc.aSysTime.Second).ToString(),
        //             BatteryStatus.ToString(), GuCode.ToString(), rc.aBuyTimesMeter.ToString(), rc.ThisMonthQuantity.ToString(),
        //             rc.aConsumedCredit.ToString(), rc.QuantityTotal.ToString(), rc.aUsedMonthly.ToString(), rc.aValveErrorTimes.ToString(),
        //             rc.aOpenBatteryTimes.ToString(), rc.aOpenCoverTimes.ToString(), rc.aSysTime.ToString(), aSysTimeInt.ToString(), sLastOpenDate,
        //             ActivityID, PhaseNo.ToString(), CloseValveReason.ToString(), prevcons.ToString(), price[0].ToString(), price[1].ToString(), price[2].ToString(),
        //             friendlyCBM.ToString(), friendlyprice[0].ToString(), friendlyprice[1].ToString(), friendlyprice[2].ToString(),
        //             bValveStatus, rc.aMegnaticTimes.ToString(), rc.magnaticDate.ToString(), rc.LastBatteryDate.ToString(),
        //             rc.ValveErrorDate != null ? rc.ValveErrorDate.Value.ToString() : string.Empty,
        //             rc.ClosedValveDate != null ? rc.ClosedValveDate.Value.ToString() : string.Empty,
        //             rc.OpenValveDate != null ? rc.OpenValveDate.Value.ToString() : string.Empty,
        //             rc.aRemainCredit, rc.AreaNo, rc.MonthQuantity, rc.UsedMonthly);

        //        if (!result)
        //        {
        //            throw new Exception("frmSellingCard:WaterMeterReading : failed to add water meter reading");
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        ///// <summary>
        ///// Operate on month reading and insert reading stairs with values 
        ///// </summary>
        ///// <returns>Insert reading stairs</returns>
        //public static bool CalculateReadingStairs()
        //{
        //    try
        //    {
        //        decimal[,] stairTemp = new decimal[5, 7];
        //        dboperation db = new dboperation();
        //        int GuCode, Sewage;
        //        string ActivityID = "";
        //        decimal ServiceBox = 0;
        //        string sql = " select top 5000 MonthReadings.ID ,serialnu, CONVERT(datetime, '01/' +CONVERT(nvarchar(2) ,[MONTH] )+'/' +  CONVERT(nvarchar(4) , [YEAR] ) , 103 ) as ReadDate,MeterID, ActivityID , [Read] as  Quantity " +
        //                     " from dbo.MonthReadings with(nolock) where (ReadingStairs is null or ReadingStairs = 0 ) and [Read] > 0 ";
        //        var dtMonthReadings = db.SelectData(sql);

        //        for (int i = 0; i < dtMonthReadings.Rows.Count; i++)
        //        {
        //            stairTemp = new decimal[5, 7];
        //            ActivityID = "";
        //            ServiceBox = 0;
        //            GuCode = 0;
        //            Sewage = 0;

        //            string serialNu = dtMonthReadings.Rows[i]["SerialNu"].ToString();
        //            string StrMeterId = dtMonthReadings.Rows[i]["MeterId"].ToString();
        //            int ReadingID = int.Parse(dtMonthReadings.Rows[i]["ID"].ToString());
        //            decimal QuantityTotal = decimal.Parse(dtMonthReadings.Rows[i]["Quantity"].ToString());
        //            var aSysTime = System.DateTime.Parse(dtMonthReadings.Rows[i]["ReadDate"].ToString());

        //            // Get meter data from water reading
        //            sql = " select GuCode , Sewage , ActivityID from dbo.WaterMetersReadings with(nolock) where serialNu = '" + serialNu + "'";
        //            var meterData = db.SelectData(sql);

        //            if (meterData.Rows.Count <= 0)
        //            {
        //                // Get meter data from meter
        //                sql = " select GuCode , PhaseNo as Sewage , ActivityID from meters with(nolock) where MeterID = '" + StrMeterId + "'";
        //                meterData = db.SelectData(sql);
        //            }

        //            if (meterData.Rows.Count > 0)
        //            {
        //                GuCode = int.Parse(meterData.Rows[0]["GuCode"].ToString() == "" ? "1" : meterData.Rows[0]["GuCode"].ToString());
        //                Sewage = int.Parse(meterData.Rows[0]["Sewage"].ToString() == "" ? "1" : meterData.Rows[0]["Sewage"].ToString());
        //                ActivityID = meterData.Rows[0]["ActivityID"].ToString();
        //            }
        //            else
        //            {
        //                continue;
        //            }

        //            // Get tariff date
        //            sql = " SELECT MAX(CONVERT(datetime, startdate, 103)) FROM tariffdetails with(nolock) " +
        //                  " where CONVERT(datetime, startdate, 103) <=  CONVERT(datetime,'" + aSysTime.ToShortDateString().ToString() +
        //                  "' , 103 ) and ActivityID = '" + ActivityID + "'";
        //            var activestart = new System.DateTime(2017, 12, 12);

        //            try
        //            {
        //                activestart = System.DateTime.Parse(db.ReturnStr(sql).ToString());
        //            }
        //            catch
        //            {
        //                activestart = new System.DateTime(2017, 12, 12);
        //            }

        //            // Get tariff
        //            DataTable dtTariff = GetTariff(ActivityID, activestart);

        //            // Get stair details
        //            var stair = calcTariffStairsDetails(QuantityTotal, dtTariff, GuCode, Sewage);

        //            for (int j = 0; j < stair.GetLength(0); j++)
        //            {
        //                ServiceBox += stair[j, 2];
        //            }

        //            // Fill stair temp
        //            for (int ii = 0; ii < stair.GetLength(0); ii++)
        //            {
        //                for (int jj = 0; jj < stair.GetLength(1); jj++)
        //                {
        //                    stairTemp[ii, jj] = stair[ii, jj];
        //                }
        //            }

        //            // Insert reading stair
        //            string SQL = "insert into ReadingStairs (ReadingID ,QuantityStair1,QuantityStair2,QuantityStair3,QuantityStair4,QuantityStair5" +//QuantityStair6" +
        //                         " , Price1, Price2, Price3, Price4, Price5, WService " +// Price6 , WService " +
        //                         " , WaterPrice1, WaterPrice2, WaterPrice3, WaterPrice4, WaterPrice5," +// WaterPrice6, " +
        //                         " Heleathy1, Heleathy2, Heleathy3, Heleathy4, Heleathy5) " +//, Heleathy6) " +
        //                         "values (" + ReadingID + "," + stairTemp[0, 0] + "," + stairTemp[1, 0] + "," + stairTemp[2, 0] + "," + stairTemp[3, 0] + "," + stairTemp[4, 0] + "," + //stair[5, 0] + "," +
        //                         "" + stairTemp[0, 6] + "," + stairTemp[1, 6] + "," + stairTemp[2, 6] + "," + stairTemp[3, 6] + "," + stairTemp[4, 6] + "," /*+ stair[5, 6] + ","*/ + ServiceBox + "," +
        //                         "" + stairTemp[0, 1] + "," + stairTemp[1, 1] + "," + stairTemp[2, 1] + "," + stairTemp[3, 1] + "," + stairTemp[4, 1] + "," /*+ stair[5, 1] + ","*/ +
        //                         "" + stairTemp[0, 5] + "," + stairTemp[1, 5] + "," + stairTemp[2, 5] + "," + stairTemp[3, 5] + "," + stairTemp[4, 5] + /*"," + stair[5, 5] +*/ ")";
        //            var result = db.ExecuteNonQuery(SQL);

        //            if (result >= 0)
        //            {
        //                // Update month reading
        //                string sqlQry = "update MonthReadings with(Rowlock) set ReadingStairs = 1 where ID = " + ReadingID + "";
        //                db.ExecuteNonQuery(sqlQry);
        //            }
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Operate on month reading and calculate missed prices
        ///// </summary>
        ///// <param name="starting">Start date</param>
        ///// <param name="ending">End date</param>
        ///// <returns>Status</returns>
        //public static bool CalculateMonthReadingMissData(System.DateTime starting, System.DateTime ending)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string sql = "select ActivityID , MeterID , [MONTH] as man , [YEAR] as yr ,[Read] as ree,  CONVERT(datetime, '01/' +CONVERT(nvarchar(2) ,[MONTH] )+'/' +  CONVERT(nvarchar(4)  , [YEAR] ) , 103 )  as ActualDate, SerialNu , *   from MonthReadings with(nolock)" +
        //                          " where( CBMPrice = 0 or CBMPrice is null) and  " +
        //                          " CONVERT(datetime ,CurrentDate , 103 )  >=  CONVERT(datetime , '" + starting.ToShortDateString() + "' , 103 ) " +
        //                          " and CONVERT(datetime ,CurrentDate , 103 )  <=  CONVERT(datetime , '" + ending.ToShortDateString() + "' , 103 ) " +
        //                          " order by customerid ";
        //        int GuCode, Sewage;
        //        DataTable dtWaterMetersReadings = db.SelectData(sql);
        //        decimal WaterPrice;
        //        decimal ServiceBoxWithTax;
        //        decimal SewagePrice;

        //        for (int i = 0; i < dtWaterMetersReadings.Rows.Count; i++)
        //        {
        //            WaterPrice = 0;
        //            ServiceBoxWithTax = 0;
        //            SewagePrice = 0;
        //            string serialNu = dtWaterMetersReadings.Rows[i]["SerialNu"].ToString();
        //            string StrMeterId = dtWaterMetersReadings.Rows[i]["MeterId"].ToString();
        //            string ActivityID = "";
        //            decimal QuantityTotal = decimal.Parse(dtWaterMetersReadings.Rows[i]["ree"].ToString());
        //            System.DateTime aSysTime = System.DateTime.Parse(dtWaterMetersReadings.Rows[i]["ActualDate"].ToString());
        //            int id = int.Parse(dtWaterMetersReadings.Rows[i]["id"].ToString());
        //            int intmonth = int.Parse(dtWaterMetersReadings.Rows[i]["man"].ToString());
        //            int intyear = int.Parse(dtWaterMetersReadings.Rows[i]["yr"].ToString());

        //            // Get meter data from water reading
        //            sql = " select GuCode , Sewage , ActivityID from dbo.WaterMetersReadings with(nolock) where serialNu = '" + serialNu + "'";
        //            var meterData = db.SelectData(sql);

        //            if (meterData.Rows.Count <= 0)
        //            {
        //                // Get meter data from meter
        //                sql = " select GuCode , PhaseNo as Sewage , ActivityID from meters with(nolock) where MeterID = '" + StrMeterId + "'";
        //                meterData = db.SelectData(sql);
        //            }

        //            if (meterData.Rows.Count > 0)
        //            {
        //                GuCode = int.Parse(meterData.Rows[0]["GuCode"].ToString() == "" ? "1" : meterData.Rows[0]["GuCode"].ToString());
        //                Sewage = int.Parse(meterData.Rows[0]["Sewage"].ToString() == "" ? "1" : meterData.Rows[0]["Sewage"].ToString());
        //                ActivityID = meterData.Rows[0]["ActivityID"].ToString();
        //            }
        //            else
        //            {
        //                continue;
        //            }

        //            // Get tariff date
        //            sql = " SELECT MAX(CONVERT(datetime, startdate, 103)) FROM tariffdetails with(nolock) " +
        //                  " where CONVERT(datetime, startdate, 103) <=  CONVERT(datetime,'" + aSysTime.ToShortDateString().ToString() +
        //                  "' , 103 ) and ActivityID = '" + ActivityID + "'";
        //            var activestart = new System.DateTime(2017, 12, 12);

        //            try
        //            {
        //                activestart = System.DateTime.Parse(db.ReturnStr(sql).ToString());
        //            }
        //            catch
        //            {
        //                activestart = new System.DateTime(2017, 12, 12);
        //            }

        //            // Get tariff
        //            DataTable dtTariff = GetTariff(ActivityID, activestart);

        //            // Get stair details
        //            var stairDetails = calcTariffStairsDetails(QuantityTotal, dtTariff, GuCode, Sewage);

        //            for (int j = 0; j < stairDetails.GetLength(0); i++)
        //            {
        //                WaterPrice += stairDetails[j, 1];
        //                ServiceBoxWithTax += stairDetails[j, 2];
        //                SewagePrice += stairDetails[j, 5];
        //            }

        //            string SqlQry = " update MonthReadings with(Rowlock) set CBMPrice = " + WaterPrice.ToString() +
        //                            " , Healthy = " + SewagePrice.ToString() + " , ServiceBox = " + ServiceBoxWithTax.ToString() +
        //                            " , ActivityID = '" + ActivityID + "' " +
        //                            " where meterid = '" + StrMeterId.ToString() + "' and  [MONTH] = " + intmonth.ToString() +
        //                            " and [YEAR] = " + intyear.ToString();
        //            db.ExecuteNonQuery(SqlQry);
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "CalculateMonthReadingMissData", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Calculate tariff difference
        ///// </summary>
        ///// <param name="activityId">Activity Id</param>
        ///// <param name="oldActiveDate">Old Tariff Active Date</param>
        ///// <param name="newActiveDate">New Tariff Active Date</param>
        ///// <returns>isSaved</returns>
        //public static bool CalculateTariffDifference(string activityId, System.DateTime newActiveDate)
        //{
        //    SqlTransaction transaction = null;
        //    dboperation db = new dboperation();

        //    try
        //    {
        //        // Get month readings of new tarrif
        //        string query = "select m.id,m.activityId , m.MeterID ,m.SerialNu , m.[MONTH] , m.[YEAR] ,m.[Read],m.ConsumptionMoney,m.TotalConsumption,m.CBMPrice,m.Healthy,m.ServiceBox,m.GuCode,m.PhaseNo as 'Sewage',  CONVERT(datetime, '01/' +CONVERT(nvarchar(2) ,[MONTH] )+'/' +  CONVERT(nvarchar(4)  , [YEAR] ) , 103 )  as ActualDate , TariffStartDate " +
        //                       " from MonthReadings m with(nolock) " +
        //                       " left outer join WaterMetersReadings w with(nolock) on m.SerialNu = w.SerialNu " +
        //                       " where m.activityId = '" + activityId + "' " +
        //                       " and CONVERT(datetime ,CONVERT(datetime, '01/' +CONVERT(nvarchar(2) ,[MONTH] )+'/' +  CONVERT(nvarchar(4)  , [YEAR] ) , 103 )  , 103 )  >=  CONVERT(datetime , '" + newActiveDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "' , 103 ) " +
        //                       " and CONVERT(datetime ,CONVERT(datetime, '01/' +CONVERT(nvarchar(2) ,[MONTH] )+'/' +  CONVERT(nvarchar(4)  , [YEAR] ) , 103 )  , 103 )  <=  CONVERT(datetime , getdate() , 103 ) ";
        //        DataTable dtMetersReadings = db.SelectData(query);

        //        // Get adjustment Type
        //        var adjustmentType = db.ReturnInt("SELECT Top 1 Id FROM AdjustmentTypes with(nolock) where Code = '" + ((int)AdjustmentTypeEnum.TarrifChangeDifference).ToString() + "'");
        //        var setingDT = db.SelectData("select top 1 MaxInstalmentsAmount,DefaultInstalmentsNumber from settings with(nolock)");
        //        var MaxInstalmentsAmount = Convert.ToDecimal(string.IsNullOrEmpty(setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString()) ? "0" : setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString());
        //        var DefaultInstalmentsNumber = int.Parse(string.IsNullOrEmpty(setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString()) ? "1" : setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString());

        //        // Start transaction
        //        if (db.objcmd.Connection.State != ConnectionState.Open)
        //        {
        //            db.objcmd.Connection.Open();
        //        }

        //        transaction = db.objcmd.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        //        db.objcmd.Transaction = transaction;

        //        for (int i = 0; i < dtMetersReadings.Rows.Count; i++)
        //        {
        //            try
        //            {
        //                int metersReadingId = int.Parse(dtMetersReadings.Rows[i]["id"].ToString());
        //                string serialNu = dtMetersReadings.Rows[i]["SerialNu"].ToString();
        //                var meterId = dtMetersReadings.Rows[i]["MeterId"].ToString();
        //                int month = int.Parse(dtMetersReadings.Rows[i]["month"].ToString());
        //                int year = int.Parse(dtMetersReadings.Rows[i]["year"].ToString());
        //                int unitsNo = int.Parse(dtMetersReadings.Rows[i]["GuCode"].ToString() == "" ? "1" : dtMetersReadings.Rows[i]["GuCode"].ToString());
        //                int sewage = int.Parse(dtMetersReadings.Rows[i]["Sewage"].ToString() == "" ? "1" : dtMetersReadings.Rows[i]["Sewage"].ToString());
        //                decimal totalConsumption = Convert.ToDecimal(dtMetersReadings.Rows[i]["TotalConsumption"].ToString());
        //                decimal oldConsumptionMoney = Convert.ToDecimal(dtMetersReadings.Rows[i]["ConsumptionMoney"].ToString());
        //                double oldCBMPrice = double.Parse(dtMetersReadings.Rows[i]["CBMPrice"].ToString());
        //                double oldHealthy = double.Parse(dtMetersReadings.Rows[i]["Healthy"].ToString());
        //                double oldServiceBox = double.Parse(dtMetersReadings.Rows[i]["ServiceBox"].ToString());
        //                var readingDate = System.DateTime.Parse(dtMetersReadings.Rows[i]["ActualDate"].ToString());
        //                var oldActiveDate = System.DateTime.Parse(dtMetersReadings.Rows[i]["TariffStartDate"].ToString());

        //                // Get new water price
        //                var newTariff = GetTariff(activityId, newActiveDate);
        //                var newConsumptionModel = GetSpecificDateConsumption(newActiveDate, activityId, unitsNo, sewage, totalConsumption, meterId, newTariff);
        //                var newTotalPrice = newConsumptionModel.TotalPrice + newConsumptionModel.Fixfee;
        //                var consumptionPriceDifference = newTotalPrice - oldConsumptionMoney;

        //                // Save Water Tariff Price Differences
        //                query = $@" insert into TariffPriceDifferences ( MeterId , ActivityId,OldTariffDate,NewTariffDate,MonthReadingsId, Year, Month ,ChargeSerialNu,MeterTotalConsumption, MeterConsumptionPrice , OldWaterPrice , OldHealthyPrice,OldServiceBoxPrice , NewWaterPrice , NewHealthyPrice,NewServiceBoxPrice,NewConsumptionPrice,ConsumptionPriceDifference,isAdjustmentAdded,CreatedDate,CreatedBy )
        //                       values('{meterId}','{activityId}','{oldActiveDate.ToString("yyyy-MM-dd")}','{newActiveDate.ToString("yyyy-MM-dd")}',{metersReadingId},{year},{month},'{serialNu}',{totalConsumption},{oldConsumptionMoney},{oldCBMPrice},{oldHealthy},{oldServiceBox},{newConsumptionModel.WaterPrice},{newConsumptionModel.SewagePrice},{newConsumptionModel.ServiceBoxWithTax},{newTotalPrice},{consumptionPriceDifference},0,getDate(),'{frmMain.UserID}')";
        //                db.objcmd.CommandText = query;
        //                var ret = db.objcmd.ExecuteNonQuery();

        //                if (ret > 0)
        //                {
        //                    // update month reading
        //                    string updateMonthReadingQuery = $@"update MonthReadings set CBMPrice = {newConsumptionModel.WaterPrice} , Healthy = {newConsumptionModel.SewagePrice} ,
        //                                                    ServiceBox = {newConsumptionModel.ServiceBoxWithTax} ,FixFee = {newConsumptionModel.Fixfee} , ConsumptionMoney = {newTotalPrice} ,TariffStartDate = '{newActiveDate.ToString("yyyy-MM-dd")}', tarriffAdjustment = {consumptionPriceDifference}
        //                                                    where id = {metersReadingId}";

        //                    db.objcmd.CommandText = updateMonthReadingQuery;
        //                    db.objcmd.ExecuteNonQuery();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MakeExceptionLog("Utility", "CalculateTariffDifference", ex);
        //            }

        //        }

        //        System.DateTime dateTime = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.AddMonths(1).Month, 1);

        //        // Add Water Tariff Price Differences as adjustment if positive value 
        //        query = $@" insert into Adjustments (Code,MeterID,Type,Reason,CurrentDate,TotalValue,MonthsCount,MonthlyRate,Remminder,PaidMonths,PercentValue,IsDeleted,ActiveAd,description ,UserID ,AccountNo,inputdate , DueDate)
        //                    select (IDENT_CURRENT('Adjustments') + ROW_NUMBER() OVER (ORDER BY MeterId)) ,MeterId, {adjustmentType} , (select top 1 id from AdjustmentReasons with(nolock)),getDate(),sum(ConsumptionPriceDifference),CASE WHEN sum(ConsumptionPriceDifference) > {MaxInstalmentsAmount}  and {MaxInstalmentsAmount} != 0  THEN {DefaultInstalmentsNumber} ELSE 1 END,CASE WHEN sum(ConsumptionPriceDifference) > {MaxInstalmentsAmount}  and {MaxInstalmentsAmount} != 0  THEN (sum(ConsumptionPriceDifference) /{DefaultInstalmentsNumber})   ELSE sum(ConsumptionPriceDifference)  END,sum(ConsumptionPriceDifference),0,100,0,1,(select concat(sum(ConsumptionPriceDifference) , '')) + (select '  فرق التعريفة اثر رجعي ') ,'{frmMain.UserID}',(SELECT top 1 AccountNo FROM METERS with(nolock) where meterid = TariffPriceDifferences.MeterId),getDate(),getDate()
        //                    from TariffPriceDifferences with(nolock)
        //                    where isAdjustmentAdded = 0 and ConsumptionPriceDifference > 0.1
        //                    group by MeterId";

        //        db.objcmd.CommandText = query;
        //        db.objcmd.ExecuteNonQuery();

        //        query = $@"update TariffPriceDifferences with(Rowlock) set isAdjustmentAdded = 1 where isAdjustmentAdded = 0";
        //        db.objcmd.CommandText = query;
        //        db.objcmd.ExecuteNonQuery();
        //        transaction.Commit();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        MakeExceptionLog("Utility", "CalculateTariffDifference", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Fill friendly time and holidays
        ///// </summary>
        ///// <param name="rc">Read card object</param>
        ///// <returns>Status</returns>
        //public static bool fillVacationsDB(ref ReadCard rc)
        //{
        //    try
        //    {
        //        rc.aHolidayStartMonth = frmMain.HolidayStartMonth;
        //        rc.aHolidayStartDay = frmMain.HolidayStartMonth;
        //        rc.aHolidayEndDay = frmMain.HolidayStartMonth;
        //        rc.aWorkTimeStartHour = frmMain.WorkTimeStartHour;
        //        rc.aWorkTimeStartMin = frmMain.WorkTimeStartMin;
        //        rc.aWorkTimeEndHour = frmMain.WorkTimeEndHour;
        //        rc.aWorkTimeEndMin = frmMain.WorkTimeEndMin;
        //        rc.aBitWeekEndSat = frmMain.BitWeekEndSat;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "fillVacationsDB", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Calculate final stair price
        ///// </summary>
        ///// <param name="StairValue">stair price</param>
        ///// <param name="sewage">sewage percentage</param>
        ///// <param name="sewagePrice">sewage price</param>
        ///// <param name="ServiceBox">service box price</param>
        ///// <param name="PhaseNo">have sewage service</param>
        ///// <param name="CustomersServiceFees">Customers Service Fees</param>
        ///// <returns>final stair price</returns>
        //public static double GetStairValueWithTax(double StairValue, double sewage, double sewagePrice, double ServiceBox, int PhaseNo, double CustomersServiceFees, bool IsStepSwgPrice, double StepSwgPrice)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        double result = StairValue;

        //        if (PhaseNo == 1)
        //        {
        //            // With sewage
        //            var sewageValue = (IsStepSwgPrice ? StepSwgPrice : (sewagePrice == 0 ? StairValue : sewagePrice)) * sewage / 100;
        //            result = StairValue + sewageValue;
        //        }

        //        // Get Tax
        //        var Tax = db.ReturnStr("Select top 1 Tax from Settings with(nolock)");
        //        var TaxValue = 1 + (string.IsNullOrEmpty(Tax) ? 0 : Convert.ToDouble(Tax) / 100);

        //        // Add per meter fee
        //        var perMeterFee = (ServiceBox + CustomersServiceFees) * TaxValue;
        //        result += perMeterFee;

        //        return Math.Ceiling(result * 10000) / 10000;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("StairValuewithTax", ex);
        //    }
        //}

        ///// <summary>
        ///// Get meter activity
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <returns>Meter activity</returns>
        //public static string GetMeterActivity(string meterID)
        //{
        //    string activity = string.Empty;

        //    try
        //    {
        //        _customerCardRepo = new CustomerCardRepository();

        //        // Get meter activities
        //        var meterActivities = _customerCardRepo.GetMeterUnitsActivitesSummary(meterID);

        //        //if (meterActivities != null && meterActivities.Rows.Count > 1)
        //        //{
        //        //    // Support multi units activities
        //        //    activity = "255";
        //        //}
        //        //else
        //        //{
        //        activity = meterActivities.Rows[0]["CatagoryId"].ToString();
        //        // }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterActivity", ex);
        //    }

        //    return activity;
        //}

        ///// <summary>
        ///// Get activity by meter units
        ///// </summary>
        ///// <param name="meterUnits">meter units</param>
        ///// <returns>Meter activity</returns>
        //public static string GetActivityIdByUnits(DataTable meterUnits)
        //{
        //    string activity = string.Empty;

        //    try
        //    {
        //        if (meterUnits != null && meterUnits.Rows.Count > 0)
        //        {
        //            var groups = meterUnits.AsEnumerable().GroupBy(row => row.Field<string>("CatagoryID"));

        //            if (groups != null && groups.Count() > 1)
        //            {
        //                // Support multi units activities
        //                activity = "255";
        //            }
        //            else
        //            {
        //                activity = meterUnits.Rows[0]["CatagoryId"].ToString();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterActivity", ex);
        //    }

        //    return activity;
        //}

        ///// <summary>
        ///// Get meter customer identifier
        ///// </summary>
        ///// <param name="meterid">meter identifier</param>
        ///// <returns>customer identifier</returns>
        //public static string GetCustomerID(string meterid)
        //{
        //    dboperation db = new dboperation();
        //    string Query = "select CustomerId from meters with(nolock) where MeterID =  '" + meterid + "'";
        //    string CustomerID = db.ReturnStr(Query);
        //    return CustomerID;
        //}

        ///// <summary>
        ///// Print meter card
        ///// </summary>
        ///// <param name="MeterID">Megter identifier</param>
        //public static void PrintMeterCard(string MeterID)
        //{
        //    try
        //    {
        //        string sql = " select customers.id , customers.Name , displaymeterid ,  AccountNo , (select top 1 Name from Cities with(nolock)) as cityname from meters with(nolock) " +
        //            " join Customers with(nolock) on Customers.id = meters.customerid where meterid = '" + MeterID + "'";
        //        dboperation db = new dboperation();
        //        DataTable dt = db.SelectData(sql);

        //        if (dt.Rows.Count > 0)
        //        {
        //            CrystalDecisions.CrystalReports.Engine.ReportClass rptCard;
        //            rptCard = new rptprintCard();
        //            rptCard.SetDataSource(dt);
        //            rptCard.SetParameterValue("Customer", dt.Rows[0]["Name"].ToString());
        //            rptCard.SetParameterValue("Account", dt.Rows[0]["id"].ToString());
        //            rptCard.SetParameterValue("GCode", dt.Rows[0]["cityname"].ToString());
        //            rptCard.SetParameterValue("MeterNo", dt.Rows[0]["displaymeterid"].ToString());
        //            rptCard.SetParameterValue("MeterID", dt.Rows[0]["AccountNo"].ToString());

        //            string cardPrinterName = "";

        //            frmSelectCardPrinter s = new frmSelectCardPrinter();
        //            s.ShowDialog();

        //            if (frmSelectCardPrinter.CardPrinterName != "")
        //                cardPrinterName = frmSelectCardPrinter.CardPrinterName;

        //            PrinterSettings ps = new PrinterSettings();
        //            PageSettings pgs = new PageSettings(ps);
        //            ps.PrinterName = cardPrinterName;
        //            CrystalDecisions.Shared.PrintLayoutSettings pls = new CrystalDecisions.Shared.PrintLayoutSettings();
        //            pls.FitHorizontalPages = true;
        //            rptCard.PrintOptions.PrinterName = cardPrinterName;
        //            Multilingual.ReportsBaseForm frm = new Multilingual.ReportsBaseForm();

        //            frm.crystalReportViewerAllReports.ReportSource = rptCard;
        //            pls.Centered = true;
        //            pls.Scaling = CrystalDecisions.Shared.PrintLayoutSettings.PrintScaling.Scale;
        //            rptCard.PrintToPrinter(ps, pgs, false, pls);
        //            Multilingual.Messages.Show("462");
        //        }
        //    }
        //    catch
        //    {
        //        Multilingual.Messages.Show("461");
        //    }
        //}

        ///// <summary>
        ///// Get user allowed days range 
        ///// </summary>
        ///// <param name="userType"></param>
        ///// <returns></returns>
        //public static int GetAllowedDaysRangeForSearchForUserType(string userType)
        //{
        //    try
        //    {
        //        dboperation dbOperation = new dboperation();
        //        return dbOperation.ReturnInt("SELECT AllowedDaysRangeForSearch FROM UserTypes WHERE ID ='" + userType + "'");
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetAllowedDaysRangeForSearchForUserType", ex);

        //        return 0;

        //    }
        //}
        //public static void ShowReplaceCardBill(string RecieptNo, bool isCopy = false)
        //{
        //    try
        //    {
        //        int index = 0;
        //        dboperation DBobj = new dboperation();
        //        DBobj.objcmd.Parameters.Clear();
        //        DBobj.objcmd.CommandType = CommandType.StoredProcedure;
        //        DBobj.objcmd.CommandText = "rptRecipts";
        //        DBobj.objcmd.Parameters.AddWithValue("@DateFrom", cashedServerDateTime.ToShortDateString());
        //        DBobj.objcmd.Parameters.AddWithValue("@DateTo", cashedServerDateTime.ToShortDateString());
        //        DBobj.objcmd.Parameters.AddWithValue("@WhereClause", " And serialno= '" + RecieptNo + "' ");
        //        DataTable tbl = DBobj.SelectData("");

        //        string txtCompany = "   ", txtregion = "    ", txtDistrict = "   ", txtDepartment = "   ";

        //        if (File.Exists(Application.StartupPath + "\\..\\RReport.tembo"))
        //        {
        //            StreamReader rd = new StreamReader(Application.StartupPath + "\\..\\RReport.tembo");
        //            string FileData = rd.ReadToEnd();
        //            rd.Close();
        //            string[] ReportArray = FileData.Split(new string[] { "---" }, StringSplitOptions.None);
        //            txtCompany = "     " + ReportArray[0];
        //            txtregion = "     " + ReportArray[1];
        //            txtDistrict = "     " + ReportArray[2];
        //            txtDepartment = "     " + ReportArray[3];
        //        }

        //        SellingCardRepository _sellingCardRepository = new SellingCardRepository();
        //        DataTable reportHeader = _sellingCardRepository.GetSetting(txtCompany, txtregion, txtDistrict, txtDepartment);
        //        CrystalDecisions.CrystalReports.Engine.ReportClass rpt;
        //        rpt = new rptShowBillReplaceCardAR();

        //        rpt.Subreports[0].SetDataSource(reportHeader);
        //        rpt.SetDataSource(tbl);

        //        rpt.SetParameterValue("Load Profile", "Receipts Report");
        //        rpt.SetParameterValue("ChargeID", tbl.Rows[index]["Bill ID"].ToString());
        //        rpt.SetParameterValue("MeterID", tbl.Rows[index]["Meter ID"].ToString());
        //        var VendingStation = tbl.Rows[0]["Vending Station"].ToString();
        //        rpt.SetParameterValue("VendingStation", VendingStation);
        //        rpt.SetParameterValue("Address", tbl.Rows[index]["Address"].ToString());

        //        string copy = isCopy ? "صورة طبق الاصل" : "";
        //        if (frmMain.Version_Country == VersionCountry.WaterRFIDEGYPTTENDER)
        //        {
        //            if (tbl.Rows[0]["MakeCard"].ToString() == "5")
        //                copy = "لاغى";
        //        }

        //        rpt.SetParameterValue("copy", copy);


        //        rpt.SetParameterValue("Value", Convert.ToDecimal(tbl.Rows[index]["Total Value"]));
        //        rpt.SetParameterValue("LastChargeValue", Convert.ToDecimal(tbl.Rows[index]["ChargeValue"]));

        //        rpt.SetParameterValue("Customer", tbl.Rows[index]["Customer"].ToString());
        //        rpt.SetParameterValue("paymenttype", tbl.Rows[0]["Payment Type"].ToString());
        //        rpt.SetParameterValue("Date", tbl.Rows[index]["Date"].ToString());
        //        rpt.SetParameterValue("Type", tbl.Rows[0]["Charge Type"].ToString());
        //        rpt.SetParameterValue("Operator", tbl.Rows[index]["Operator"].ToString());
        //        rpt.SetParameterValue("SoftwareVersion", frmMain.strVersion);

        //        try
        //        {
        //            rpt.SetParameterValue("MeterID", tbl.Rows[0]["Meter ID"].ToString());
        //        }
        //        catch (Exception ex)
        //        {
        //            MakeExceptionLog("Utility", "ShowReplaceCardBill", ex);
        //        }

        //        rpt.SetParameterValue("District", tbl.Rows[0]["District"].ToString());
        //        rpt.SetParameterValue("Activity", tbl.Rows[0]["Activity"].ToString());
        //        rpt.SetParameterValue("Account", tbl.Rows[0]["Account No"].ToString());
        //        Multilingual.ReportsBaseForm frm = new Multilingual.ReportsBaseForm();
        //        frm.crystalReportViewerAllReports.ReportSource = rpt;
        //        frm.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("ShowReplaceCardBill", "ShowReplaceCardBill", ex);
        //    }
        //}

        ///// <summary>
        ///// Get meter main parameters
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <returns>Meter object</returns>
        //public static waterParameter GetMeterParameters(string MeterID)
        //{
        //    dboperation db = new dboperation();
        //    SmartWaterMeter.waterParameter wp = new SmartWaterMeter.waterParameter();
        //    string Query = "select  Activities.dreditamt , Activities.alarmamt , Activities.MaximumFee " +
        //                   ",  ActivityID , Meters.MeterType , MeterTypes.MeterModelVersionID  " +
        //                   " from Activities with(nolock) inner join Meters with(nolock) on Meters.ActivityID = Activities.ID " +
        //                   "inner join MeterTypes with(nolock) on meters.MeterType = MeterTypes.ID where Meters.MeterID = '" + MeterID + "'";
        //    DataTable dt = db.SelectData(Query);

        //    if (dt.Rows.Count > 0)
        //    {
        //        wp.alarmamt = decimal.Parse(dt.Rows[0]["alarmamt"].ToString() == "" ? "0" : dt.Rows[0]["alarmamt"].ToString());
        //        wp.dreditamt = decimal.Parse(dt.Rows[0]["dreditamt"].ToString() == "" ? "0" : dt.Rows[0]["dreditamt"].ToString());
        //        wp.maxcharge = decimal.Parse(dt.Rows[0]["MaximumFee"].ToString() == "" ? "0" : dt.Rows[0]["MaximumFee"].ToString());
        //        wp.ActivityID = dt.Rows[0]["ActivityID"].ToString();
        //        wp.MeterType = dt.Rows[0]["MeterType"].ToString();
        //        wp.MeterModelVersionID = int.Parse(dt.Rows[0]["MeterModelVersionID"].ToString());
        //    }

        //    return wp;
        //}

        ///// <summary>
        ///// Get water settings
        ///// </summary>
        ///// <returns>Settings</returns>
        //public static watersys Watersys()
        //{
        //    SmartWaterMeter.watersys ws = new SmartWaterMeter.watersys();

        //    try
        //    {
        //        dboperation db = new dboperation();
        //        SmartWaterMeter.waterParameter wp = new SmartWaterMeter.waterParameter();
        //        string Query = " select top 1 syscode ,syskey  , Salemode from Settings with(nolock)";
        //        DataTable dt = db.SelectData(Query);

        //        if (dt.Rows.Count > 0)
        //        {
        //            SmartWaterMeter.watersys.salemode = (short)(dt.Rows[0]["Salemode"].ToString() == "Units" ? 1 : 0);
        //            SmartWaterMeter.watersys.syscode = short.Parse(dt.Rows[0]["syscode"].ToString() == "" ? "0" : dt.Rows[0]["syscode"].ToString());
        //            SmartWaterMeter.watersys.syskey = short.Parse(dt.Rows[0]["syskey"].ToString() == "" ? "0" : dt.Rows[0]["syskey"].ToString());
        //        }

        //        try
        //        {
        //            CommSettings cms = GetCOMMSettings((int)ConfigurationType.Optical);

        //            if (cms != null)
        //                SmartWaterMeter.watersys.port = short.Parse(cms.mcomport.ToString() == "" ? "0" : cms.mcomport.ToString().Substring(3, 1));
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    catch
        //    {
        //    }

        //    return ws;
        //}

        ///// <summary>
        ///// Set activity code
        ///// </summary>
        ///// <param name="activity">activity code</param>
        ///// <param name="GUcode">Unit number</param>
        ///// <param name="PhaseNo">Sewage</param>
        ///// <returns>activity code</returns>
        //public static int EGYSetActivityCodeinMeter(int activity, int GUcode, int PhaseNo)
        //{
        //    // PhaseNo = 1 mean there is Sewage , 0 no sewage
        //    int ActivityCodeinMeter;

        //    if (GUcode >= 10)
        //        GUcode = 0;

        //    if (activity == 10)
        //        activity = 0;

        //    if (activity < 10)
        //    {
        //        ActivityCodeinMeter = PhaseNo * 100 + activity * 10 + GUcode;
        //    }
        //    else
        //    {
        //        ActivityCodeinMeter = 200 + activity + (PhaseNo == 0 ? 1 : 0);
        //    }

        //    if (ActivityCodeinMeter > 255)
        //        ActivityCodeinMeter = 255;

        //    return ActivityCodeinMeter;
        //}

        ///// <summary>
        ///// Get activity code
        ///// </summary>
        ///// <param name="activity">activity code</param>
        ///// <param name="PhaseNo">Sewage</param>
        ///// <param name="GUcode">Unit number</param>
        ///// <returns>activity code</returns>
        //public static byte EGYGetActivityCodeinMeter(int activity, ref int PhaseNo, ref int GUcode, string meterId)
        //{
        //    dboperation db = new dboperation();
        //    char[] chactivity = activity.ToString().PadLeft(3, '0').ToCharArray();
        //    int meterSewage;

        //    if (activity > 200)
        //    {
        //        GUcode = 1;
        //        activity = activity - 200;

        //        if (activity % 2 > 0)
        //        {
        //            activity = activity - 1;
        //            PhaseNo = 0;
        //        }
        //        else
        //        {
        //            PhaseNo = 1;
        //        }

        //        // Check meter change requests by date (any changes in: activity, department , gucode, phase no)
        //        var monthDate = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
        //        var meterDate = GetMeterChangesByDate(meterId, monthDate);

        //        // get new activity instead of meter current activity
        //        if (meterDate != null && meterDate.Rows.Count > 0)
        //        {
        //            activity = int.Parse(meterDate.Rows[0]["ActivityId"].ToString().Split('-')[1]);
        //            GUcode = int.Parse(meterDate.Rows[0]["GuCode"].ToString());
        //            PhaseNo = int.Parse(meterDate.Rows[0]["PhaseNo"].ToString());
        //        }

        //        var dt = db.SelectData("select MeterID, PhaseNo ,GuCode , ActivityID ,DepartmentID from Meters with(nolock) where MeterID = '" + meterId + "'");

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            meterSewage = int.Parse(dt.Rows[0]["PhaseNo"].ToString() == "" ? "1" : dt.Rows[0]["PhaseNo"].ToString());
        //            //meterGucode = int.Parse(dt.Rows[0]["GuCode"].ToString() == "" ? "1" : dt.Rows[0]["GuCode"].ToString());
        //            //meterActivity = int.Parse(dt.Rows[0]["ActivityID"].ToString().Split('-')[1]);

        //            if (PhaseNo != meterSewage)
        //            {
        //                if (PhaseNo == 0)
        //                {
        //                    activity = activity + 1;
        //                    PhaseNo = 1;
        //                }
        //                else
        //                {
        //                    PhaseNo = 0;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        PhaseNo = int.Parse(chactivity[0].ToString());
        //        activity = int.Parse(chactivity[1].ToString());
        //        GUcode = int.Parse(chactivity[2].ToString());
        //    }

        //    byte ActivityCodeinMeter = byte.Parse(activity.ToString());
        //    return ActivityCodeinMeter;
        //}



        //#endregion

        //#region calculate tarrif methods

        ///// <summary>
        ///// Calculate tariff stairs details
        ///// </summary>
        ///// <param name="Quantity">Quantity</param>
        ///// <param name="dataTariff">Tarriff</param>
        ///// <param name="unitno">Unit number</param>
        ///// <param name="sewage">sewage</param>
        ///// <returns>Stairs details</returns>
        //public static decimal[,] calcTariffStairsDetails(decimal Quantity, DataTable dataTariff, int unitno, int sewage)
        //{
        //    bool IncludeUnitNo, IsCumulative, IsStepSwgPrice = false;
        //    decimal estidamaPerStair, from, to, Price, Tax, ServiceBox, ServiceBoxWithTax, SewagePrice, SewagePercentage, totalSewage, StepSwgPrice, waterPrice, totalPrice = 0;
        //    decimal allQuantity = Quantity;
        //    decimal[,] stair = new decimal[dataTariff.Rows.Count, 8];

        //    for (int i = 0; i < dataTariff.Rows.Count; i++)
        //    {
        //        // 1- Prepare water price per units
        //        IncludeUnitNo = Convert.ToBoolean(dataTariff.Rows[i]["IsNoOfUnitsIncludedInCalc"].ToString());

        //        from = Convert.ToDecimal(dataTariff.Rows[i]["from"].ToString());
        //        to = Convert.ToDecimal(dataTariff.Rows[i]["to"].ToString());
        //        estidamaPerStair = Convert.ToDecimal(dataTariff.Rows[i]["MonthStepFees"].ToString());

        //        if (IncludeUnitNo)
        //        {
        //            from = from * unitno;
        //            to = to * unitno;
        //        }

        //        Price = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString());

        //        // 2- Prepare service box
        //        Tax = decimal.Parse(dataTariff.Rows[i]["tax"].ToString()) / 100;
        //        ServiceBox = decimal.Parse(dataTariff.Rows[i]["ServiceBox"].ToString()) + decimal.Parse(dataTariff.Rows[i]["CustomersServiceFees"].ToString());
        //        ServiceBoxWithTax = ServiceBox * (1 + Tax);

        //        // 3- Prepare sewage price
        //        if (sewage == 1)
        //        {
        //            SewagePrice = Convert.ToDecimal(dataTariff.Rows[i]["SwgPrice"].ToString());
        //            SewagePercentage = Convert.ToDecimal(dataTariff.Rows[i]["SwgPercent"].ToString());
        //            IsStepSwgPrice = Convert.ToBoolean(dataTariff.Rows[i]["IsStepSwgPrice"].ToString());
        //            StepSwgPrice = Convert.ToDecimal(dataTariff.Rows[i]["StepSwgPrice"].ToString());
        //            totalSewage = (IsStepSwgPrice ? StepSwgPrice : (SewagePrice == 0 ? Price : SewagePrice)) * SewagePercentage / 100;
        //        }
        //        else
        //        {
        //            totalSewage = 0;
        //        }

        //        // 4- Prepare stair price
        //        waterPrice = Price + ServiceBoxWithTax + totalSewage;

        //        // 5- Prepare cumulative
        //        IsCumulative = Convert.ToBoolean(dataTariff.Rows[i]["IsCumulative"].ToString());

        //        // restart tarriff details
        //        if (from == 0 && !IsCumulative)
        //        {
        //            from = 0;
        //            totalPrice = 0;
        //            Quantity = allQuantity;

        //            for (int j = 0; j < i; j++)
        //            {
        //                stair[j, 0] = 0; // Quantity الكمية
        //                stair[j, 1] = 0; // Total water price إجمالى ثمن المياه
        //                stair[j, 2] = 0; // Total service box with tax إجمالى ثمن الخدمات شامل الضريبة
        //                stair[j, 3] = 0; // Total service box إجمالى ثمن الخدمات 
        //                stair[j, 4] = 0; // Total tax إجمالى الضريبة
        //                stair[j, 5] = 0; // Total sewage إجمالى ثمن الصرف
        //                stair[j, 6] = 0; // Total price إجمالى الثمن الكلى
        //                stair[j, 7] = 0; // Estidama استدامة الشريحة
        //            }
        //        }

        //        // Calculate stairs main prices
        //        if (Quantity > to - from && i != dataTariff.Rows.Count - 1)
        //        {
        //            totalPrice += (to - from) * waterPrice;
        //            Quantity = Quantity - (to - from);
        //            stair[i, 0] = to - from;
        //            stair[i, 1] = (to - from) * Price;
        //            stair[i, 2] = (to - from) * ServiceBoxWithTax;
        //            stair[i, 3] = stair[i, 2] / (1 + Tax);
        //            stair[i, 4] = stair[i, 3] * Tax;
        //            stair[i, 5] = (to - from) * totalSewage;
        //            stair[i, 6] = totalPrice;
        //            stair[i, 7] = estidamaPerStair;
        //        }
        //        else
        //        {
        //            totalPrice += Quantity * waterPrice;
        //            stair[i, 0] = Quantity;
        //            stair[i, 1] = Quantity * Price;
        //            stair[i, 2] = Quantity * ServiceBoxWithTax;
        //            stair[i, 3] = stair[i, 2] / (1 + Tax);
        //            stair[i, 4] = stair[i, 3] * Tax;
        //            stair[i, 5] = Quantity * totalSewage;
        //            stair[i, 6] = totalPrice;
        //            stair[i, 7] = estidamaPerStair;
        //            break;
        //        }
        //    }

        //    return stair;
        //}

        ///// <summary>
        ///// Calculate tariff stairs details for specific date without estidama
        ///// </summary>
        ///// <param name="meterid">Meter identifier</param>
        ///// <param name="reading">Quantity</param>
        ///// <param name="activityId">Activity identifier</param>
        ///// <param name="meterUnits">Unit number</param>
        ///// <param name="sewage">Sewage</param>
        ///// <param name="specificDate">Specific date</param>
        ///// <param name="checkMeterData">Flag for enable check meter data in specific date</param>
        ///// <returns>Consumption money without estidama</returns>
        //public static decimal CalcMeterWaterConsumptionForSpecificRegion(string meterid, decimal reading, string activityId, int meterUnits, int sewage, System.DateTime specificDate, bool checkMeterData)
        //{
        //    decimal TotalPrice = 0;
        //    decimal ServiceBoxWithTax = 0;
        //    decimal WaterPrice = 0;
        //    decimal SewagePrice = 0;

        //    try
        //    {
        //        // Get meter data in specific date
        //        if (checkMeterData)
        //        {
        //            GetMeterCatagoryData(meterid, ref activityId, ref sewage, ref meterUnits, specificDate);
        //        }

        //        // Get activity tariff and stairs (Calculate based on system meter data)
        //        var tarrifa = GetTariff(activityId, specificDate.AddMonths(1).AddDays(-1));
        //        var priceResult = calcTariffStairsDetails(reading, tarrifa, meterUnits, sewage);

        //        // Get activity estidama
        //        // decimal FixFee = GetMeterEstidamaByActivityID(meterid, activityId, meterUnits, Convert.ToDateTime(tarrifa.Rows[0]["StartDate"].ToString()), reading);

        //        for (int i = 0; i < priceResult.GetLength(0); i++)
        //        {
        //            WaterPrice += priceResult[i, 1];
        //            ServiceBoxWithTax += priceResult[i, 2];
        //            SewagePrice += priceResult[i, 5];
        //            TotalPrice = priceResult[i, 6] != 0 ? priceResult[i, 6] : TotalPrice;
        //        }

        //        // Include estidama if on stairs except first stair one
        //        //if (int.Parse(tarrifa.Rows[0]["MonthFeesOptionId"].ToString()) == (int)MonthFeesOptionsEnum.StepFees || int.Parse(tarrifa.Rows[0]["MonthFeesOptionId"].ToString()) == (int)MonthFeesOptionsEnum.StepFeesPerUnit)
        //        //{
        //        //    TotalPrice = TotalPrice + FixFee - priceResult[0, 7];
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "CalcMeterWaterConsumptionForSpecificRegion", ex);
        //    }

        //    return TotalPrice;
        //}

        ///// <summary>
        ///// Return Consumption details for specific tarrif
        ///// </summary>
        ///// <param name="specificDate">Specific date</param>
        ///// <param name="ActivityID">Activity</param>
        ///// <param name="unitNo">Unit number</param>
        ///// <param name="sewage">Sewage</param>
        ///// <param name="reading">Quantity reading</param>
        ///// <param name="meterid">Meter identifier</param>
        ///// <returns></returns>
        //public static ConsumptionModel GetSpecificDateConsumption(System.DateTime specificDate, string ActivityID, int unitNo, int sewage, decimal reading, string meterid, DataTable tariff = null)
        //{
        //    try
        //    {
        //        var result = new ConsumptionModel();

        //        // Get activity tariff and stairs (Calculate based on system meter data)
        //        result.Tarrifa = (tariff ?? GetTariff(ActivityID, specificDate));
        //        result.PriceResult = calcTariffStairsDetails(reading, result.Tarrifa, unitNo, sewage);
        //        result.Stairs = PrepareStairsDetails(result.PriceResult);

        //        // Get activity estidama
        //        result.Fixfee = GetMeterEstidamaByActivityID(meterid, ActivityID, unitNo, Convert.ToDateTime(result.Tarrifa.Rows[0]["StartDate"]), reading);

        //        result.tarrifId = int.Parse(result.Tarrifa.Rows[0]["tariffId"].ToString());
        //        result.tarrifStartDate = Convert.ToDateTime(result.Tarrifa.Rows[0]["StartDate"].ToString());

        //        for (int i = 0; i < result.PriceResult.GetLength(0); i++)
        //        {
        //            result.WaterPrice += result.PriceResult[i, 1];
        //            result.ServiceBoxWithTax += result.PriceResult[i, 2];
        //            result.SewagePrice += result.PriceResult[i, 5];
        //            result.TotalPrice = result.PriceResult[i, 6] != 0 ? result.PriceResult[i, 6] : result.TotalPrice;
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetSpecificDateConsumption", ex);
        //        return null;
        //    }
        //}

        //// Get money
        ///// <summary>
        ///// Calculate water consumption money from quantity include unit number
        ///// </summary>
        ///// <param name="Quantity">Quantity</param>
        ///// <param name="dataTariff">Tarriff</param>
        ///// <param name="unitno">UnitNo</param>
        ///// <returns>water consumption money</returns>
        //public static decimal calcWaterMoneyIncludeUnits(decimal Quantity, DataTable dataTariff, int unitno)
        //{
        //    decimal from = 0;
        //    decimal to = 0;
        //    decimal money = 0;
        //    decimal orgunit = Quantity;
        //    decimal price = 0;
        //    char cumulativeMode = '0';

        //    if (unitno < 1)
        //    {
        //        unitno = 1;
        //    }

        //    for (int i = 0; i < dataTariff.Rows.Count; i++)
        //    {
        //        from = Convert.ToDecimal(dataTariff.Rows[i]["from"].ToString()) * unitno;
        //        to = Convert.ToDecimal(dataTariff.Rows[i]["to"].ToString()) * unitno;
        //        price = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString());

        //        if (from == 0)
        //        {
        //            cumulativeMode = '1';
        //        }
        //        else
        //        {
        //            cumulativeMode = '0';
        //        }

        //        if (cumulativeMode == '1')
        //        {
        //            from = 0;
        //            money = 0;
        //            Quantity = orgunit;
        //        }

        //        if (Quantity > to - from)
        //        {
        //            money += (to - from) * price;
        //            Quantity = Quantity - (to - from);
        //        }
        //        else
        //        {
        //            money += Quantity * price;
        //            break;
        //        }
        //    }

        //    return Math.Round(money, 4);
        //}

        //// Get units
        //public static decimal calcWaterTatiffEGcalc(decimal netval, DataTable dataTariff, int unitno, int sewage)
        //{
        //    decimal fromval = 0, toval = 0;
        //    decimal cmval = 0;
        //    decimal prev = 0;
        //    decimal orgnetval = netval;
        //    decimal unit = 0;
        //    char crmode = '0';
        //    decimal hf = 0, ServiceBox = 0;
        //    decimal SewageDiff = 0;

        //    if (sewage == 1)
        //    {
        //        hf = 0;
        //        SewageDiff = 0;
        //    }
        //    else
        //    {
        //        hf = decimal.Parse(dataTariff.Rows[0]["tax"].ToString());
        //    }

        //    ServiceBox = decimal.Parse(dataTariff.Rows[0]["ServiceBox"].ToString()) + decimal.Parse(dataTariff.Rows[0]["CustomersServiceFees"].ToString());
        //    decimal val = 0;

        //    for (int i = 0; i < dataTariff.Rows.Count; i++)
        //    {
        //        fromval = Convert.ToDecimal(dataTariff.Rows[i]["from"].ToString()) * Convert.ToDecimal(unitno.ToString());
        //        toval = Convert.ToDecimal(dataTariff.Rows[i]["to"].ToString()) * Convert.ToDecimal(unitno.ToString());
        //        val = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString()); // stair price

        //        if (fromval == 0)
        //            crmode = '1';
        //        else
        //            crmode = '0';

        //        SewageDiff = (val - ServiceBox) * (hf / (1 + hf));  // sewage price
        //        cmval = val - SewageDiff;  // water price

        //        if (crmode == '1')
        //        {
        //            fromval = 0;
        //            unit = 0;
        //            netval = orgnetval;
        //        }

        //        if ((toval - fromval) * cmval > netval)
        //        {
        //            if (i > 0)
        //            {
        //                prev = Convert.ToDecimal(dataTariff.Rows[i - 1]["to"].ToString()) * Convert.ToDecimal(dataTariff.Rows[i - 1]["value"].ToString());

        //                if (netval > prev && crmode == '1')
        //                {
        //                    unit += Convert.ToDecimal(dataTariff.Rows[i - 1]["to"].ToString());
        //                    netval = netval - prev;

        //                    if (orgnetval > toval * cmval)
        //                        continue;
        //                    else
        //                    {
        //                        unit = orgnetval / cmval;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    unit += netval / cmval;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                unit += netval / cmval;
        //            }

        //            netval = 0;
        //        }
        //        else
        //        {
        //            unit += (toval - fromval);
        //            netval = netval - (cmval * (toval - fromval));
        //        }
        //    }

        //    return Math.Round(unit, 3);
        //}

        //// Get units
        ///// <summary>
        ///// Calculate water reading quantity from money include unit number and phase status
        ///// </summary>
        ///// <param name="netval">Money</param>
        ///// <param name="dataTariff">Tariff</param>
        ///// <param name="unitno">unit number</param>
        ///// <param name="PhaseNo">Phase status</param>
        ///// <returns></returns>
        //public static decimal calcWaterReadingQuantityIncludeUnits(decimal netval, DataTable dataTariff, int unitno, int PhaseNo)
        //{
        //    int from = 0;
        //    int to = 0;
        //    decimal price = 0;
        //    decimal SewageDiff = 0;
        //    decimal tax = 0;
        //    decimal ServiceBox = 0;
        //    decimal prev = 0;
        //    decimal orgnetval = netval;
        //    decimal unit = 0;
        //    char crmode = '0';

        //    if (dataTariff.Rows.Count == 0)
        //        return 0;

        //    if (PhaseNo == 1)
        //    {
        //        tax = 0;
        //        SewageDiff = 0;
        //    }
        //    else
        //    {
        //        tax = decimal.Parse(dataTariff.Rows[0]["tax"]?.ToString());
        //    }

        //    ServiceBox = decimal.Parse(dataTariff.Rows[0]["ServiceBox"].ToString()) + decimal.Parse(dataTariff.Rows[0]["CustomersServiceFees"].ToString());
        //    decimal val = 0;

        //    for (int i = 0; i < dataTariff.Rows.Count; i++)
        //    {
        //        from = Convert.ToInt16(dataTariff.Rows[i]["from"].ToString()) * unitno;
        //        to = Convert.ToInt32(dataTariff.Rows[i]["to"].ToString()) * unitno;
        //        val = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString());

        //        // Calculate sewage value
        //        SewageDiff = (val - ServiceBox) * (tax / (1 + tax));

        //        price = val - SewageDiff;

        //        if (from == 0)
        //        {
        //            crmode = '1';
        //        }
        //        else
        //        {
        //            crmode = '0';
        //        }

        //        if (crmode == '1')
        //        {
        //            from = 0;
        //            unit = 0;
        //            netval = orgnetval;
        //        }

        //        if ((to - from) * price > netval)
        //        {
        //            if (i > 0)
        //            {
        //                prev = Convert.ToDecimal(dataTariff.Rows[i - 1]["to"].ToString()) * Convert.ToDecimal(dataTariff.Rows[i - 1]["value"].ToString());

        //                if (netval > prev && crmode == '1')
        //                {
        //                    unit += Convert.ToDecimal(dataTariff.Rows[i - 1]["to"].ToString());
        //                    netval = netval - prev;

        //                    if (orgnetval > to * price)
        //                    {
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        unit = orgnetval / price;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    unit += netval / price;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                unit += netval / price;
        //            }

        //            netval = 0;
        //        }
        //        else
        //        {
        //            unit += to - from;
        //            netval = netval - (price * (to - from));
        //        }
        //    }

        //    return Math.Round(unit, 3);
        //}

        //// Get units
        ///// <summary>
        ///// Calculate water reading quantity from money
        ///// </summary>
        ///// <param name="netval">Money</param>
        ///// <param name="dataTariff">Tariff</param>
        ///// <returns></returns>
        //public static decimal calcWaterReadingQuantity(decimal money, DataTable dataTariff)
        //{
        //    int from = 0;
        //    int to = 0;
        //    decimal cmtr = 0;
        //    decimal price = 0;

        //    foreach (DataRow dr in dataTariff.Rows)
        //    {
        //        from = Convert.ToInt16(dr["from"].ToString());
        //        to = Convert.ToInt32(dr["to"].ToString());
        //        price = Convert.ToDecimal(dr["value"].ToString());

        //        if (money >= (price * (to - from)))
        //        {
        //            cmtr += to - from;
        //            money = money - (price * (to - from));
        //        }
        //        else
        //        {
        //            cmtr += (money / price);
        //            money = 0;
        //        }
        //    }

        //    return Math.Round(cmtr, 2);
        //}

        //// Get money [3]
        //public static decimal[] getWaterDetail(decimal consumption, int UnitNo, int Healthy, DataTable dtTariff)
        //{
        //    decimal[] price = new decimal[3];
        //    double tax = 0, ServiceBox = 0;

        //    if (dtTariff.Rows.Count > 0)
        //    {
        //        tax = double.Parse(dtTariff.Rows[0]["tax"].ToString()) / 100;
        //        ServiceBox = double.Parse(dtTariff.Rows[0]["ServiceBox"].ToString()) + double.Parse(dtTariff.Rows[0]["CustomersServiceFees"].ToString());
        //    }

        //    price[0] = calcWaterMoneyIncludeUnits(consumption, dtTariff, UnitNo);
        //    price[2] = consumption * (decimal)ServiceBox;
        //    price[0] -= price[2];
        //    price[0] = price[0] / (decimal)(1 + tax);

        //    if (Healthy == 0)
        //    {
        //        tax = 0;
        //    }

        //    price[1] = price[0] * (decimal)tax;
        //    return price;
        //}

        //// Get money [3]
        //public static decimal[] getWaterDetailByCreditandConsumption(decimal consumption, decimal credit, string ActivityID)
        //{
        //    dboperation db = new dboperation();
        //    decimal[] price = new decimal[3];
        //    int Healthy = 0;
        //    DataTable dtTariff = new DataTable();
        //    dtTariff = GetTariff(ActivityID);
        //    double hf = 0;
        //    double ServiceBox = 0;

        //    if (dtTariff.Rows.Count > 0)
        //    {
        //        Healthy = int.Parse(dtTariff.Rows[0]["Healthy"].ToString());
        //        hf = double.Parse(dtTariff.Rows[0]["tax"].ToString());
        //        ServiceBox = double.Parse(dtTariff.Rows[0]["ServiceBox"].ToString()) + double.Parse(dtTariff.Rows[0]["CustomersServiceFees"].ToString());

        //        if (Healthy == 0)
        //        {
        //            hf = 0;
        //        }
        //    }

        //    price[0] = credit;
        //    price[2] = consumption * (decimal)ServiceBox;
        //    price[0] -= price[2];
        //    price[0] = price[0] / (decimal)(1 + hf);
        //    price[1] = price[0] * (decimal)hf;
        //    return price;
        //}

        //// Get money [3]
        //public static decimal[] getWaterDetailcalc(decimal consumption, int UnitNo, int Healthy, DataTable dtTariff)
        //{
        //    dboperation db = new dboperation();
        //    decimal[] price = new decimal[3];
        //    double hf = 0, ServiceBox = 0;

        //    if (dtTariff.Rows.Count > 0)
        //    {
        //        hf = double.Parse(dtTariff.Rows[0]["tax"].ToString());
        //        ServiceBox = double.Parse(dtTariff.Rows[0]["ServiceBox"].ToString()) + double.Parse(dtTariff.Rows[0]["CustomersServiceFees"].ToString());
        //    }

        //    price[0] = calcWaterMoneyIncludeUnits(consumption, dtTariff, UnitNo);
        //    price[2] = consumption * (decimal)ServiceBox;
        //    price[0] -= price[2];
        //    price[0] = price[0] / (decimal)(1 + hf);

        //    if (Healthy == 0)
        //    {
        //        hf = 0;
        //    }

        //    price[1] = price[0] * (decimal)hf;
        //    return price;
        //}

        //// Not used
        //public static decimal calcWaterEGYTConsumptionFee(decimal Quantity, DataTable dataTariff, int unitno)
        //{
        //    try
        //    {
        //        decimal from = 0, to = 0;
        //        decimal cmtr = 0, orgunit = Quantity;
        //        decimal price = 0;
        //        decimal friendly = 0;
        //        char[] mode = ("000110110").ToString().ToCharArray();
        //        char crmode = mode[0];

        //        for (int i = 0; i < dataTariff.Rows.Count; i++)
        //        {
        //            crmode = mode[i];
        //            from = decimal.Parse(dataTariff.Rows[i]["from"].ToString()) * unitno;
        //            to = decimal.Parse(dataTariff.Rows[i]["to"].ToString()) * unitno;
        //            price = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString());
        //            friendly = Convert.ToDecimal(dataTariff.Rows[i]["friendly"].ToString());

        //            if (from == 0)
        //            {
        //                from = 0;
        //                cmtr = 0;
        //                Quantity = orgunit;
        //            }

        //            if (Quantity > to - from)
        //            {
        //                cmtr += ((to - from) > friendly ? friendly : (to - from)) * price;
        //                Quantity = Quantity - (to - from);
        //            }
        //            else
        //            {
        //                cmtr += (Quantity > friendly ? friendly : Quantity) * price;
        //                break;
        //            }
        //        }

        //        return Math.Round(cmtr, 4);
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //// Get money 
        //// Old meter RFID
        ///// <summary>
        ///// Calculate water consumption money from water units (RFID meter)
        ///// </summary>
        ///// <param name="units">Water units</param>
        ///// <param name="dataTariff">Tariff</param>
        ///// <param name="MeterVersionType">Meter type</param>
        ///// <returns>Money amount</returns>
        //public static decimal calcWaterMoneyfromWaterUnits(decimal units, DataTable dataTariff, int MeterVersionType = 0)
        //{
        //    int from = 0, to = 0;
        //    decimal money = 0;
        //    decimal price = 0;

        //    if (frmMain.Version_Country == VersionCountry.WaterRFIDEGYPTTENDER)
        //    {
        //        foreach (DataRow dr in dataTariff.Rows)
        //        {
        //            from = Convert.ToInt16(dr["from"].ToString());
        //            to = Convert.ToInt32(dr["to"].ToString());
        //            price = Convert.ToDecimal(dr["value"].ToString());

        //            if (units <= to)
        //            {
        //                money = units * price;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (DataRow dr in dataTariff.Rows)
        //        {
        //            from = Convert.ToInt16(dr["from"].ToString());
        //            to = Convert.ToInt32(dr["to"].ToString());
        //            price = Convert.ToDecimal(dr["value"].ToString());

        //            if (units >= to)
        //            {
        //                money += (to - from) * price;
        //            }
        //            else
        //            {
        //                money += (units - from) * price;
        //                break;
        //            }
        //        }
        //    }

        //    return Math.Round(money, 2);
        //}

        //#endregion

        //#region Add month reading

        //public static ReadCard PrepareRC(CardID cardID, MeterID meterId, ClientID clientId, byte meterAction,
        //    PriceSched priceSched, Deductions deductions, OffTimes offTimes, ThisCreditInfo creditInfo,
        //   CreditBalance creditBalance, ReadingsQty readingsQty, MState mState, double maxOverdraftCredit, int meterType)
        //{
        //    ReadCard rc = new ReadCard();
        //    rc.meterType = meterType;
        //    var meterActions = Convert.ToString(meterAction, 2).PadLeft(8, '0');
        //    rc.aBitConsumerType = meterActions[0] == '1';
        //    rc.aBitHolidayEn = meterActions[2] == '1';
        //    rc.aBitStepEn = meterActions[1] == '1';
        //    rc.aBitRemainWaringEn = meterActions[2] == '1';
        //    rc.aBitOverdraftThreshold = creditBalance.overDraftCredit > 0;
        //    rc.aConsumerType = clientId.clientCat; // account code from activities
        //    rc.aCardSN = cardID.CardNo;
        //    rc.AreaNo = cardID.CompID;

        //    if (offTimes.HolidayMonth != null)
        //    {
        //        var Days = Convert.ToString(offTimes.WrkDays, 2).PadLeft(8, '0');
        //        rc.aBitWeekEndFri = Days[6] != '1';
        //        rc.aBitWeekEndSat = Days[0] != '1';
        //    }

        //    rc.aBuyTimes = (byte)(creditInfo.chargeNo > byte.MaxValue ? byte.MaxValue : creditInfo.chargeNo);
        //    rc.aBuyTimesMeter = rc.aBuyTimes;
        //    rc.metercount = rc.aBuyTimes;
        //    rc.cardcount = rc.aBuyTimes;
        //    rc.cumlativeCharge = creditBalance.cumlativeCharge;
        //    rc.mode = cardID.mode;
        //    rc.aWorkTimeStartHour = offTimes.WStartHr;
        //    rc.aWorkTimeStartMin = 0;
        //    rc.aWorkTimeEndHour = offTimes.WEndHr;
        //    rc.aWorkTimeEndMin = 0;
        //    rc.aChargeMode = meterId.chargeMode;
        //    rc.aConsumerID = meterId.meterID;

        //    if (creditBalance.usedMonthly != null)
        //    {
        //        rc.aOverdraftThreshold = creditBalance.overDraftCredit;
        //        rc.aCreditDate = TemplatesProvider.Models.DateTime.GetSystemDate(creditBalance.appDate);
        //        rc.aMainCredit = creditBalance.cumlativeCharge - creditBalance.consumedCredit;
        //        rc.UsedMonthly = creditBalance.usedMonthly;
        //    }

        //    rc.aRemainWaring = creditInfo.cutOffWarnLmt;
        //    rc.LastTransDate = TemplatesProvider.Models.DateTime.GetSystemDate(cardID.LastTransDate);
        //    rc.deductionCount = deductions.month;

        //    // Set deduction
        //    if (deductions.monthFees > 0)
        //    {
        //        rc.deduction = deductions.monthFees;
        //    }

        //    if (mState.meterState != null)
        //    {
        //        var batteryMstate = mState.meterState.Length > 0 ? mState.meterState[2] : new MalFun();

        //        if (batteryMstate.month != 0)
        //        {
        //            rc.aOpenBatteryTimes = batteryMstate.malFunCount;
        //            rc.LastBatteryDate = new System.DateTime(batteryMstate.month <= System.DateTime.Now.Month ? System.DateTime.Now.Year : System.DateTime.Now.Year - 1,
        //                batteryMstate.month, batteryMstate.day);
        //        }


        //        var openCoverMstate = mState.meterState.Length > 0 ? mState.meterState[0] : new MalFun();

        //        if (openCoverMstate.month != 0)
        //        {
        //            rc.aOpenCoverTimes = openCoverMstate.malFunCount;
        //            rc.LastOpenDate = new System.DateTime(openCoverMstate.month <= System.DateTime.Now.Month ? System.DateTime.Now.Year : System.DateTime.Now.Year - 1,
        //              openCoverMstate.month, openCoverMstate.day);
        //        }

        //        var magnaticMstate = mState.meterState.Length > 0 ? mState.meterState[1] : new MalFun();

        //        if (magnaticMstate.month != 0)
        //        {
        //            rc.aMegnaticTimes = magnaticMstate.malFunCount;
        //            rc.magnaticDate = new System.DateTime(magnaticMstate.month <= System.DateTime.Now.Month ? System.DateTime.Now.Year : System.DateTime.Now.Year - 1,
        //              magnaticMstate.month, magnaticMstate.day);
        //        }

        //        if (mState.MeterStateDates.Length > 0)
        //        {
        //            rc.ValveErrorDate = TemplatesProvider.Models.DateTime.GetSystemDate(mState.MeterStateDates[0]);

        //            System.DateTime defaultDate = new System.DateTime(2000, 1, 1);

        //            if (rc.ValveErrorDate == null || rc.ValveErrorDate == defaultDate)
        //            {
        //                rc.ValveErrorDate = TemplatesProvider.Models.DateTime.GetSystemDate(mState.MeterStateDates[1]);
        //            }

        //            // Closed valve date (by disable meter card)
        //            rc.ClosedValveDate = TemplatesProvider.Models.DateTime.GetSystemDate(mState.MeterStateDates[2]);

        //            // Open valve date (by enable meter card)
        //            rc.OpenValveDate = TemplatesProvider.Models.DateTime.GetSystemDate(mState.MeterStateDates[3]);
        //        }

        //        string meterErrors = Convert.ToString(mState.MeterErrors, 2).PadLeft(8, '0');

        //        rc.aValveErrorTimes = meterErrors[2] == '1' || meterErrors[3] == '1' ? (byte)1 : (byte)0;

        //    }

        //    if (priceSched.monthFees != null)
        //    {
        //        rc.aFixFee = Convert.ToByte((MonthFeesOptionsEnum.FixedFeesPerUnit).GetDescription()) == priceSched.monthFeesOptions ?
        //            priceSched.monthFees[1] * priceSched.noOfUnitsIncludedInCalc : priceSched.monthFees[0];

        //        rc.aStartDate = TemplatesProvider.Models.DateTime.GetSystemDate(priceSched.appDate);

        //        for (int i = 0; i < rc.Price.Length; i++)
        //        {
        //            rc.Price[i] = priceSched.aPrice[i];
        //        }

        //        for (int i = 0; i < rc.PriceLevel.Length; i++)
        //        {
        //            rc.PriceLevel[i] = priceSched.aStepMax[i];
        //        }
        //    }

        //    if (offTimes.HolidayMonth != null)
        //    {
        //        for (int i = 0; i < 25; i++)
        //        {
        //            rc.aHolidayStartMonth[i] = offTimes.HolidayMonth[i];
        //            rc.aHolidayStartDay[i] = offTimes.HolidayDay[i];
        //        }
        //    }

        //    rc.aRemainCredit = creditInfo.chargeNo == 0 ? creditBalance.cumlativeCharge - creditBalance.consumedCredit : creditBalance.remainCredit;
        //    rc.aOverdraftCredit = creditInfo.chargeNo == 0 ? 0 : creditBalance.overDraftCredit;
        //    rc.aConsumedCredit = creditInfo.chargeNo == 0 ? 0 : creditBalance.consumedCredit;
        //    rc.aSysTime = TemplatesProvider.Models.DateTime.GetSystemDate(cardID.LastTransDate);
        //    rc.QuantityTotal = readingsQty.reading;
        //    rc.aQuantityTotal = readingsQty.reading;
        //    rc.aQuantityTotalNeg = readingsQty.quantityTotalNeg;

        //    if (readingsQty.monthConusmption != null)
        //    {
        //        rc.MonthQuantity = readingsQty.monthConusmption;
        //    }

        //    rc.ret = 0;
        //    rc.meterPhaseNo = clientId.sewdgeSrv;
        //    rc.meterUnitsNo = clientId.noOfUnits;
        //    rc.clientId = clientId.clientID;
        //    rc.aMeterState = mState.MeterErrors;
        //    rc.CardID = cardID;
        //    rc.ClientID = clientId;
        //    rc.priceSched = priceSched;
        //    rc.ThisCreditInfo = creditInfo;
        //    rc.CreditBalance = creditBalance;
        //    rc.ThisMonthMoney = creditBalance.consumedCredit;
        //    rc.ThisMonthQuantity = SmartWaterMeter.Cards.CardOperation.GetReadingQuantityFromCardReading(rc);
        //    rc.aUsedMonthly = rc.ThisMonthQuantity;
        //    return rc;
        //}



        ///// <summary>
        ///// Get last meter month reading
        ///// </summary>
        ///// <param name="meterID">Meter id</param>
        ///// <returns></returns>
        //public static DataTable GetLastMeterMonthReading(string meterID)
        //{
        //    try
        //    {
        //        dboperation dboperation = new dboperation();
        //        string lastMeterMonthReading = $"SELECT TOP 1 [Read], ConsumptionMoney FROM MonthReadings WHERE MeterID = '{meterID}' ORDER BY CurrentDate DESC";
        //        return dboperation.SelectData(lastMeterMonthReading);
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetLastMeterMonthReading", ex);
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Prepare water meter month readings
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <param name="Year">Reading year</param>
        ///// <param name="Month">Reading month from meter</param>
        ///// <param name="TotalReading">Total consumption Reading from meter</param>
        ///// <param name="UsedMonthly">Used Consuption Money Monthly</param>
        ///// <param name="meterFixFee">Fixed Fee from meter</param>
        ///// <param name="ActivityID">ActivityID</param>
        ///// <param name="sewage">sewage</param> 
        ///// <param name="meterUnits">meterUnits</param> 
        ///// <param name="Installment">Installment</param> 
        ///// <param name="meterType">Meter type vendor</param> 
        ///// <param name="consumerType">Consumer type</param>
        ///// <returns>Add result</returns>
        //private static bool SaveWaterMonthReadings(string MeterID, int Year, int Month, decimal TotalReading, decimal UsedMonthly, decimal meterFixFee, string meterActivityID, int sewage, int meterUnits, ref decimal tarriffAdjustment, /*bool isAddMonthReading,*/ double Installment = 0, double meterInstallment = 0, int meterType = 0, int consumerType = 0)
        //{
        //    SellingCardRepository _sellingCardRepository = new SellingCardRepository();
        //    dboperation DB1 = new dboperation();

        //    ConsumptionModel lastChargeConsumptionModel = null;
        //    var ActivityID = meterActivityID;
        //    _customerCardRepo = new CustomerCardRepository();
        //    decimal consumptionAdjustment = 0;

        //    // Check meter change requests by date (any changes in: activity, department , gucode, phase no)
        //    var monthDate = new System.DateTime(Year, Month, 1);
        //    GetMeterCatagoryData(MeterID, ref ActivityID, ref sewage, ref meterUnits, monthDate);

        //    // Get expected system tarrif
        //    var expectedConsumptionModel = GetSpecificDateConsumption(monthDate.AddMonths(1).AddDays(-1), ActivityID, meterUnits, sewage, TotalReading, MeterID);

        //    // Set old meters used month
        //    if (meterType == (int)MeterTypeEnum.Water_EGRFID || meterType == (int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch)
        //    {
        //        // Ignore meter data and take current tarrif consumption
        //        UsedMonthly = expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee;
        //    }

        //    // Get last success charge tarrifa details
        //    DataTable lastChargeDT = _sellingCardRepository.GetMeterLastSuccessCharge(MeterID, monthDate.AddMonths(1));

        //    // Check meter tarrif change
        //    if (lastChargeDT.Rows.Count > 0 && Convert.ToDateTime(lastChargeDT.Rows[0]["TariffStartDate"].ToString()) != expectedConsumptionModel.tarrifStartDate
        //        && System.DateTime.Parse(lastChargeDT.Rows[0]["serverDate"].ToString()) <= monthDate.AddMonths(1))
        //    {
        //        lastChargeConsumptionModel = GetSpecificDateConsumption(System.DateTime.Parse(lastChargeDT.Rows[0]["TariffStartDate"].ToString()), ActivityID, meterUnits, sewage, TotalReading, MeterID);

        //        // Get different tarrif recalc adjustment
        //        if (lastChargeConsumptionModel != null)
        //        {
        //            tarriffAdjustment = (expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee + (decimal)meterInstallment) - (lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee);

        //            // Set old meters used month
        //            if (meterType == (int)MeterTypeEnum.Water_EGRFID || meterType == (int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch)
        //            {
        //                // Ignore meter data and take old tarrif consumption
        //                UsedMonthly = lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee;
        //            }
        //        }
        //    }

        //    // Add adjustment: on difference used money (New meters only)
        //    if (meterType != (int)MeterTypeEnum.Water_EGRFID && meterType != (int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch)
        //    {
        //        if (lastChargeConsumptionModel != null)
        //        {
        //            consumptionAdjustment = (lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee + (decimal)meterInstallment) - UsedMonthly;
        //        }
        //        else
        //        {
        //            consumptionAdjustment = (expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee + (decimal)meterInstallment) - UsedMonthly;
        //        }
        //    }

        //    // Add month reading
        //    var result = SaveDbMonthReadings(MeterID, Year, Month, TotalReading, UsedMonthly, expectedConsumptionModel.Fixfee, meterFixFee, sewage, meterUnits, ActivityID, expectedConsumptionModel.TotalPrice, expectedConsumptionModel.ServiceBoxWithTax, expectedConsumptionModel.WaterPrice, expectedConsumptionModel.SewagePrice, (decimal)Installment, (decimal)meterInstallment, expectedConsumptionModel.Stairs, tarriffAdjustment, consumptionAdjustment, expectedConsumptionModel.tarrifStartDate, consumerType);

        //    if (result)
        //    {
        //        var monthReadingId = DB1.ReturnInt($"select Id from MonthReadings with(nolock) where MeterId = '{MeterID}' and [Year] = '{Year}' and [Month] = '{Month}'");
        //        // Add adjustment: closed meter estidama (have no water consumption)
        //        if (TotalReading == 0)
        //        {
        //            var tax = decimal.Parse(expectedConsumptionModel.Tarrifa.Rows[0]["tax"].ToString()) / 100;
        //            var ClosedMeterMonthFees = Convert.ToDecimal(expectedConsumptionModel.Tarrifa.Rows[0]["ClosedMeterMonthFees"].ToString()) * (1 + tax);

        //            if (Convert.ToInt32(expectedConsumptionModel.Tarrifa.Rows[0]["MonthFeesOptionId"].ToString()) == (int)SmartWaterMeter.MonthFeesOptionsEnum.FixedFeesPerUnit || Convert.ToInt32(expectedConsumptionModel.Tarrifa.Rows[0]["MonthFeesOptionId"].ToString()) == (int)SmartWaterMeter.MonthFeesOptionsEnum.StepFeesPerUnit)
        //                ClosedMeterMonthFees *= meterUnits;

        //            if (ClosedMeterMonthFees != 0 && ClosedMeterMonthFees > expectedConsumptionModel.Fixfee)
        //            {
        //                var estidamaDiff = ClosedMeterMonthFees - expectedConsumptionModel.Fixfee;
        //                var DBobj = new mgrAdjustments();
        //                var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //                DBobj.AddAdjustment(MeterID, estidamaDiff.ToString(), "1", estidamaDiff.ToString(), frmMain.ClosedMeterEstidamaCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 1, monthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
        //            }
        //        }

        //        // Add adjustment: different tarrif recalc adjustment
        //        if (tarriffAdjustment > (decimal)0.1)
        //        {
        //            var MaxInstalmentsAmount = frmMain.MaxInstalmentsAmount;
        //            var DefaultInstalmentsNumber = frmMain.DefaultInstalmentsNumber;
        //            var monthCount = 1;
        //            var monthrate = tarriffAdjustment;

        //            if (tarriffAdjustment > MaxInstalmentsAmount && MaxInstalmentsAmount != 0)
        //            {
        //                monthCount = DefaultInstalmentsNumber;
        //                monthrate = tarriffAdjustment / DefaultInstalmentsNumber;
        //            }

        //            var DBobj = new mgrAdjustments();
        //            var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //            DBobj.AddAdjustment(MeterID, tarriffAdjustment.ToString(), monthCount.ToString(), monthrate.ToString(), frmMain.TarrifDifferenceRecaculationCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 1, monthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
        //        }
        //        else if (tarriffAdjustment < -(decimal)0.1)
        //        {
        //            var monthCount = 1;
        //            var monthrate = -tarriffAdjustment;

        //            var DBobj = new mgrAdjustments();
        //            var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //            DBobj.AddAdjustment(MeterID, (-tarriffAdjustment).ToString(), monthCount.ToString(), monthrate.ToString(), frmMain.TarrifDifferenceRecaculationForCustomerCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 1, monthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
        //        }

        //        // Add adjustment: on difference used money (New meters only)
        //        if (consumptionAdjustment > (decimal)0.1)
        //        {
        //            var MaxInstalmentsAmount = frmMain.MaxInstalmentsAmount;
        //            var DefaultInstalmentsNumber = frmMain.DefaultInstalmentsNumber;
        //            var monthCount = 1;
        //            var monthrate = consumptionAdjustment;

        //            if (consumptionAdjustment > MaxInstalmentsAmount && MaxInstalmentsAmount != 0)
        //            {
        //                monthCount = DefaultInstalmentsNumber;
        //                monthrate = consumptionAdjustment / DefaultInstalmentsNumber;
        //            }

        //            var DBobj = new mgrAdjustments();
        //            var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //            DBobj.AddAdjustment(MeterID, consumptionAdjustment.ToString(), monthCount.ToString(), monthrate.ToString(), frmMain.OldConsumptionRecalculationCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 1, monthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
        //        }
        //        else if (consumptionAdjustment < -(decimal)0.1)
        //        {
        //            var monthCount = 1;
        //            var monthrate = -consumptionAdjustment;
        //            var DBobj = new mgrAdjustments();
        //            var dt = DB1.SelectData(" select isnull(max(id) + 1, 1) as maxID from Adjustments");
        //            DBobj.AddAdjustment(MeterID, (-consumptionAdjustment).ToString(), monthCount.ToString(), monthrate.ToString(), frmMain.OldConsumptionRecalculationForCustomerCode, cashedServerDateTime, dt.Rows[0][0].ToString(), "100", frmMain.DefaultAdjustmentReason, 1, monthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
        //        }
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Get stairs details
        ///// </summary>
        ///// <param name="stairs"> stairs details</param>
        ///// <returns>Row of stair details</returns>
        //private static DataTable PrepareStairsDetails(decimal[,] stairs)
        //{
        //    try
        //    {
        //        decimal ServiceBox = 0;
        //        DataTable data = new DataTable();
        //        DataRow row = data.NewRow();

        //        for (int i = 1; i < 7; i++)
        //        {
        //            data.Columns.Add("QuantityStair" + i);
        //            data.Columns.Add("WaterPrice" + i);
        //            data.Columns.Add("Heleathy" + i);
        //            data.Columns.Add("Price" + i);
        //        }

        //        data.Columns.Add("WService");

        //        for (int j = 0; j < stairs.GetLength(0); j++)
        //        {
        //            ServiceBox += stairs[j, 2];
        //        }

        //        row["WService"] = ServiceBox;

        //        for (int i = 0; i < 5; i++)
        //        {
        //            row["QuantityStair" + (i + 1)] = stairs[i, 0];
        //            row["WaterPrice" + (i + 1)] = stairs[i, 1];
        //            row["Heleathy" + (i + 1)] = stairs[i, 5];
        //            row["Price" + (i + 1)] = stairs[i, 6];
        //        }

        //        data.Rows.Add(row);
        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "createMeterStairs", ex);
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Get month reading difference
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <returns>Month difference</returns>
        //public static int GetLastMonthReadingMonthDiff(string MeterID)
        //{
        //    dboperation DB1 = new dboperation();
        //    var dt = DB1.SelectData("select Top 1 * from MonthReadings with(nolock) where MeterID = '" + MeterID + "' order by ID desc");

        //    if (dt.Rows.Count == 0)
        //    {
        //        return DB1.ReturnInt("select datediff(month, WorkingDate, CONVERT(datetime, Getdate(), 103)) from Meters with(nolock) where MeterID = '" + MeterID + "'");
        //    }
        //    else
        //    {
        //        var lastDate = new System.DateTime(int.Parse(dt.Rows[0]["Year"].ToString()), int.Parse(dt.Rows[0]["Month"].ToString()), 1);
        //        var years = System.DateTime.Now.Year - lastDate.Year;
        //        var month = System.DateTime.Now.Month - lastDate.Month;

        //        if (years == 0 && month == 1)
        //            month = 0;

        //        return month + (years * 12);
        //    }
        //}

        ///// <summary>
        ///// Insert new meter month readings to db directly
        ///// </summary>
        ///// <param name="MeterID">MeterID</param>
        ///// <param name="Year">Reading Year</param>
        ///// <param name="Month">Reading Month</param>
        ///// <param name="TotalReading">Total consumption Reading</param>
        ///// <param name="UsedMonthly">Used Consuption Money Monthly</param>
        ///// <param name="FixFee">Fixed Fee</param>
        ///// <param name="sewage">sewage</param> 
        ///// <param name="meterUnits">meterUnits</param> 
        ///// <param name="activityID">activityID</param> 
        ///// <param name="MoneyValue">MoneyValue</param>
        ///// <param name="Installment">Meter installment</param>
        ///// <param name="stairs">Meter stairs</param>
        ///// <param name="consumerType">Consumer type</param>
        ///// <returns>Bool indicator saved or not</returns>
        //private static bool SaveDbMonthReadings(string MeterID, int Year, int Month, decimal TotalReading, decimal UsedMonthly, decimal FixFee, decimal MeterFixFee, int sewage, int meterUnits, string activityID, decimal MoneyValue, decimal ServiceBox, decimal WaterPrice, decimal SewagePrice, decimal Installment, decimal MeterInstallment, DataTable stairs, decimal tarriffAdjustment, decimal consumptionAdjustment, System.DateTime tarrifStartDate, int consumerType = 0)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "WaterMonthReadings";
        //        db.objcmd.Parameters.AddWithValue("@MeterId", MeterID);                              // Meter identifier
        //        db.objcmd.Parameters.AddWithValue("@Month", Month);                                  // Month
        //        db.objcmd.Parameters.AddWithValue("@Year", Year);                                    // Year
        //        db.objcmd.Parameters.AddWithValue("@ActivityID", activityID);                        // Meter activity
        //        db.objcmd.Parameters.AddWithValue("@GuCode", meterUnits);                            // Meter unit no
        //        db.objcmd.Parameters.AddWithValue("@PhaseNo", sewage);                               // Meter sewage
        //        db.objcmd.Parameters.AddWithValue("@TotalConsumption", TotalReading);                // Quantity reading
        //        db.objcmd.Parameters.AddWithValue("@UsedMonthly", UsedMonthly);                      // consumption money from meter 
        //        db.objcmd.Parameters.AddWithValue("@FixFee", FixFee);                                // Estidama from system
        //        db.objcmd.Parameters.AddWithValue("@MeterFixFee", MeterFixFee);                      // Estidama from Meter
        //        db.objcmd.Parameters.AddWithValue("@CBMPrice", WaterPrice);                          // Calculated water price
        //        db.objcmd.Parameters.AddWithValue("@Healthy", SewagePrice);                          // Calculated sewage price
        //        db.objcmd.Parameters.AddWithValue("@ServiceBox", ServiceBox);                        // Calculated service box
        //        db.objcmd.Parameters.AddWithValue("@Installment", Installment);                      // installment from system 
        //        db.objcmd.Parameters.AddWithValue("@MeterInstallment", MeterInstallment);            // installment from meter
        //        db.objcmd.Parameters.AddWithValue("@ConsumptionMoney", MoneyValue);                  // consumption money from system    
        //        db.objcmd.Parameters.AddWithValue("@consumerType", consumerType);                    // consumer type from meter  (activity code)  
        //        db.objcmd.Parameters.AddWithValue("@tarriffAdjustment", tarriffAdjustment);          // tarriff adjustment from meter
        //        db.objcmd.Parameters.AddWithValue("@consumptionAdjustment", consumptionAdjustment);  // consumption adjustment from meter 
        //        db.objcmd.Parameters.AddWithValue("@Stairs", stairs);                                // stairs details  
        //        db.objcmd.Parameters.AddWithValue("@TariffStartDate", tarrifStartDate.ToString("yyyy-MM-dd")); // tarif start date

        //        SqlParameter sqlResult = new SqlParameter("@sqlResult", SqlDbType.Int, 1);
        //        sqlResult.Direction = ParameterDirection.Output;
        //        db.objcmd.Parameters.Add(sqlResult);
        //        db.ExecuteNonQuery("");

        //        if (sqlResult?.Value != null && (int)sqlResult?.Value == 1) // Add month reading
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "SaveDbMonthReadings", ex);
        //        return false;
        //    }
        //}

        //public static string CheckNullDateValue(string dateValue)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(dateValue) || dateValue.Contains("0001") || dateValue.Contains("01/01/2000")
        //            || dateValue.Contains("1899") || dateValue.Contains("1900"))
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "CheckNullDate", ex);
        //        return null;
        //    }

        //    return dateValue;
        //}

        //public static bool InsertWaterReading(string consumerType, string cumlativeCharge, string SerialNu, string readingType, string meterId, string customerId, string aMeterState, string aOverdraftCredit, string aQuantityTotalNeg,
        //    string aSysTimeAddedSeconds, string BatteryStatus, string GuCode, string aBuyTimesMeter, string ThisMonthQuantity,
        //    string aConsumedCredit, string QuantityTotal, string aUsedMonthly, string aValveErrorTimes, string aOpenBatteryTimes,
        //    string aOpenCoverTimes, string aSysTime, string aSysTimeInt, string sLastOpenDate, string lastactivity, string PhaseNo,
        //    string CloseValveReason, string prevcons, string price0, string price1, string price2, string friendlyCBM, string friendlyprice0, string friendlyprice1, string friendlyprice2,
        //    bool bValveStatus, string magneticTimes, string magneticDate, string LastOpenbaterryDate, string valveErrorDate, string closedValveDate, string openValveDate, double aRemainCredit, int companyCode, double[] MonthQuantity, double[] priceShedule)
        //{
        //    try
        //    {
        //        SellingCardRepository sellingRepo = new SellingCardRepository();
        //        dboperation db = new dboperation();

        //        if (readingType == "Charge")
        //        {
        //            // Get meter current date
        //            System.DateTime meterCurrentDate = System.DateTime.Parse(aSysTime);

        //            // Get last success charge server version details
        //            var softwareVersionDetails = sellingRepo.GetMeterLastSuccessChargeVersion(meterId);

        //            // Review last 12 month readings
        //            if (softwareVersionDetails.Rows.Count > 0 && string.IsNullOrEmpty(softwareVersionDetails.Rows[0]["Softwareversion"].ToString()))
        //            {
        //                // Target month
        //                var monthDate = new System.DateTime(meterCurrentDate.Year, (meterCurrentDate.Month - 1), 1);

        //                // Last creation or replacement date
        //                var lastCreationDate = System.DateTime.Parse(softwareVersionDetails.Rows[0]["serverDate"].ToString());

        //                // Fix old monthreadings
        //                for (int monthCount = 0; monthCount < 12; monthCount++)
        //                {
        //                    #region fix new meter

        //                    monthDate = monthDate.AddMonths(-monthCount);

        //                    if (monthDate.Year >= lastCreationDate.Year && monthDate.Month >= lastCreationDate.Month)
        //                    {
        //                        var updateMonthlyOldConsumtionQry =
        //                            $@"if exists(select Id from MonthReadings with(nolock) where MeterId = '{meterId}' and [Year] = '{monthDate.Year}' and [Month] = '{monthDate.Month}' and (  [read] <> '{MonthQuantity[monthCount]}' OR UsedMonthly <> '{priceShedule[monthCount]}' ))
        //                                 begin
        //                                     update MonthReadings with (ROWLOCK)  set OldConsumption = {MonthQuantity[monthCount]} where ID = (select top 1 Id from MonthReadings with(nolock) where [Year]={monthDate.Year} and [Month]={monthDate.Month} and MeterID = '{meterId}')";



        //                        if (meterId.Count(c => c.Equals('-')) >= 2)
        //                        {
        //                            updateMonthlyOldConsumtionQry += $" UPDATE MonthReadings WITH (ROWLOCK) SET OldMoney = {priceShedule[monthCount]} WHERE MeterId = '{meterId}' AND [Year] = '{monthDate.Year}' AND [Month] = '{monthDate.Month}' AND UsedMonthly <> '{priceShedule[monthCount]}'";
        //                        }

        //                        updateMonthlyOldConsumtionQry += " End";
        //                        db.ExecuteNonQuery(updateMonthlyOldConsumtionQry);

        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }

        //                    #endregion
        //                }

        //                // Update last success charge server version
        //                var query3 = $@"if exists(select ID from charges with(nolock) where id = {softwareVersionDetails.Rows[0]["id"].ToString()})
        //                            begin
        //                                update charges with (ROWLOCK)  set Softwareversion = '{frmMain.strVersion}' where id = {softwareVersionDetails.Rows[0]["id"].ToString()}
        //                            End";
        //                db.ExecuteNonQuery(query3);
        //            }
        //        }

        //        // Insert water meter reading
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "AddWaterMeterReadings";
        //        db.objcmd.Parameters.AddWithValue("@consumerType", consumerType);
        //        db.objcmd.Parameters.AddWithValue("@cumlativeCharge", cumlativeCharge);
        //        db.objcmd.Parameters.AddWithValue("@SerialNu", SerialNu);
        //        db.objcmd.Parameters.AddWithValue("@readingType", readingType);
        //        db.objcmd.Parameters.AddWithValue("@meterId", meterId);
        //        db.objcmd.Parameters.AddWithValue("@customerId", customerId);
        //        db.objcmd.Parameters.AddWithValue("@aMeterState", aMeterState);
        //        db.objcmd.Parameters.AddWithValue("@aOverdraftCredit", aOverdraftCredit);
        //        db.objcmd.Parameters.AddWithValue("@aQuantityTotalNeg", aQuantityTotalNeg);
        //        db.objcmd.Parameters.AddWithValue("@aSysTimeAddedSeconds", aSysTimeAddedSeconds);
        //        db.objcmd.Parameters.AddWithValue("@BatteryStatus", BatteryStatus);
        //        db.objcmd.Parameters.AddWithValue("@GuCode", GuCode);
        //        db.objcmd.Parameters.AddWithValue("@aBuyTimesMeter", aBuyTimesMeter);
        //        db.objcmd.Parameters.AddWithValue("@ThisMonthQuantity", ThisMonthQuantity);
        //        db.objcmd.Parameters.AddWithValue("@aConsumedCredit", aConsumedCredit);
        //        db.objcmd.Parameters.AddWithValue("@QuantityTotal", QuantityTotal);
        //        db.objcmd.Parameters.AddWithValue("@aUsedMonthly", aUsedMonthly);
        //        db.objcmd.Parameters.AddWithValue("@aValveErrorTimes", aValveErrorTimes);
        //        db.objcmd.Parameters.AddWithValue("@aOpenBatteryTimes", aOpenBatteryTimes);
        //        db.objcmd.Parameters.AddWithValue("@aOpenCoverTimes", aOpenCoverTimes);
        //        db.objcmd.Parameters.AddWithValue("@aSysTime", aSysTime);
        //        db.objcmd.Parameters.AddWithValue("@aSysTimeInt", aSysTimeInt);
        //        db.objcmd.Parameters.AddWithValue("@sLastOpenDate", CheckNullDateValue(sLastOpenDate));
        //        db.objcmd.Parameters.AddWithValue("@lastactivity", lastactivity);
        //        db.objcmd.Parameters.AddWithValue("@PhaseNo", PhaseNo);
        //        db.objcmd.Parameters.AddWithValue("@CloseValveReason", CloseValveReason);
        //        db.objcmd.Parameters.AddWithValue("@prevcons", prevcons);
        //        db.objcmd.Parameters.AddWithValue("@price0", price0);
        //        db.objcmd.Parameters.AddWithValue("@price1", price1);
        //        db.objcmd.Parameters.AddWithValue("@price2", price2);
        //        db.objcmd.Parameters.AddWithValue("@friendlyCBM", friendlyCBM);
        //        db.objcmd.Parameters.AddWithValue("@friendlyprice0", friendlyprice0);
        //        db.objcmd.Parameters.AddWithValue("@friendlyprice1", friendlyprice1);
        //        db.objcmd.Parameters.AddWithValue("@friendlyprice2", friendlyprice2);
        //        db.objcmd.Parameters.AddWithValue("@bValveStatus", bValveStatus ? "1" : "0");
        //        db.objcmd.Parameters.AddWithValue("@magneticTimes", magneticTimes);
        //        db.objcmd.Parameters.AddWithValue("@magneticDate", CheckNullDateValue(magneticDate));
        //        db.objcmd.Parameters.AddWithValue("@LastOpenbaterryDate", CheckNullDateValue(LastOpenbaterryDate));
        //        db.objcmd.Parameters.AddWithValue("@valveErrorDate", CheckNullDateValue(valveErrorDate));
        //        db.objcmd.Parameters.AddWithValue("@closedValveDate", CheckNullDateValue(closedValveDate));
        //        db.objcmd.Parameters.AddWithValue("@openValveDate", CheckNullDateValue(openValveDate));
        //        db.objcmd.Parameters.AddWithValue("@aRemainCredit", aRemainCredit);
        //        db.objcmd.Parameters.AddWithValue("@companyCode", companyCode);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity0", MonthQuantity[0]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity1", MonthQuantity[1]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity2", MonthQuantity[2]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity3", MonthQuantity[3]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity4", MonthQuantity[4]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity5", MonthQuantity[5]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity6", MonthQuantity[6]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity7", MonthQuantity[7]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity8", MonthQuantity[8]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity9", MonthQuantity[9]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity10", MonthQuantity[10]);
        //        db.objcmd.Parameters.AddWithValue("@MonthQuantity11", MonthQuantity[11]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice0", priceShedule[0]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice1", priceShedule[1]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice2", priceShedule[2]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice3", priceShedule[3]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice4", priceShedule[4]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice5", priceShedule[5]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice6", priceShedule[6]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice7", priceShedule[7]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice8", priceShedule[8]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice9", priceShedule[9]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice10", priceShedule[10]);
        //        db.objcmd.Parameters.AddWithValue("@MonthPrice11", priceShedule[11]);
        //        db.objcmd.Parameters.AddWithValue("@UserName", frmMain.UserName);
        //        SqlParameter sqlResult = new SqlParameter("@sqlResult", SqlDbType.Int, 1);
        //        sqlResult.Direction = ParameterDirection.Output;
        //        db.objcmd.Parameters.Add(sqlResult);
        //        db.ExecuteNonQuery("");

        //        if (sqlResult?.Value != null && ((int)sqlResult?.Value == 1 || (int)sqlResult?.Value == 2)) // Add or Update water meter reading
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "InsertWaterReading", ex);
        //        return false;
        //    }
        //}


        ///// <summary>
        ///// Get meter last recalc data
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <returns></returns>
        //private static DataTable GetMeterLastRecalc(string MeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "GetMeterLastRecalcPoint";
        //        db.objcmd.Parameters.AddWithValue("@MeterId", MeterID);                         // Meter identifier
        //        return db.SelectData("");
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterLastRecalc", ex);
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Update last water meter reading recalc
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <returns></returns>
        //private static int UpdateLastWaterMeterReading(string MeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.ExecuteNonQuery(
        //            "UPDATE WaterMetersReadings with(Rowlock) SET RecalcFlag = 1 WHERE MeterID = '" + MeterID + "' and id in " +
        //            "( " +
        //            "select top 1 id from WaterMetersReadings with(nolock) where MeterID = '" + MeterID + "' " +
        //            "order by convert(datetime, aSysTime, 103) desc " +
        //            ") "
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterLastRecalc", ex);
        //        return 0;
        //    }
        //}

        //#endregion

        //#region Estidama operations

        ///// <summary>
        ///// Calculate meter fixed fees (Fixed Estidama)
        ///// </summary>
        ///// <param name="meterID">meter id</param>
        ///// <param name="startDate">Tariff start Date</param>
        ///// <param name="ActivityId">Activity Id</param>
        ///// <param name="unitNo">Unit No</param>
        ///// <returns>Meter fixed fee calculated value</returns>
        //public static double CalculateFixedFees(string meterID, string startDate, string ActivityId, int? unitNo, string meterDim = "")
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        double fixedFees = 0;

        //        if (unitNo == null)
        //        {
        //            unitNo = 1;
        //        }

        //        var query = (!string.IsNullOrEmpty(ActivityId) ? "Select '" + ActivityId + "' as CatagoryId , '" + unitNo + "' as [UnitNo] " : " Select ActivityID as CatagoryId, GuCode as UnitNo from Meters Where MeterId = '" + meterID + "'");

        //        var meterUnitsActivitiesDT = db.SelectData(query);

        //        foreach (DataRow dr in meterUnitsActivitiesDT.Rows)
        //        {
        //            // Get estidama by activity and feeflag and (MeterDimension or IsResidential)
        //            var feesTable = db.SelectData("select top(1) f.Feevalue , f.FixedFeesPerUnit , (select top 1 tax from settings with(nolock)) as Tax , t.MonthFeesOptionId from FeeTypes f with(nolock) inner join Activities a with(nolock) on a.ID = f.Activity inner join TariffDetails t with(nolock) on t.ActivityID = f.Activity and t.StartDate = f.StartDate  where  f.feeFlag = 1 and f.IsDeleted = 0 and f.Activity = '" + dr["CatagoryId"].ToString() + "' and (f.MeterDimension = " +
        //                (string.IsNullOrEmpty(meterID) ? "'" + meterDim + "'" : "(select top 1 mt.Metertype from meters m with(nolock) join MeterTypes mt with(nolock) on m.MeterType = mt.ID where m.MeterID = '" + meterID + "')") +
        //                "  or a.IsResidential = 1 ) and convert( datetime ,  f.StartDate , 103 ) = '" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

        //            if (feesTable.Rows.Count > 0)
        //            {
        //                var tax = Convert.ToDouble(!string.IsNullOrEmpty(feesTable.Rows[0]["Tax"]?.ToString()) ? feesTable.Rows[0]["Tax"]?.ToString() : "0");
        //                tax = 1 + (tax != 0 ? (tax / 100) : tax);

        //                //Fix Fee Option
        //                if (feesTable.Rows[0]["MonthFeesOptionId"]?.ToString() == ((int)SmartWaterMeter.MonthFeesOptionsEnum.FixedFees).ToString())
        //                {
        //                    var Feevalue = Convert.ToDouble(!string.IsNullOrEmpty(feesTable.Rows[0]["Feevalue"]?.ToString()) ? feesTable.Rows[0]["Feevalue"]?.ToString() : "0");
        //                    fixedFees += Feevalue * tax;
        //                }
        //                else if (feesTable.Rows[0]["MonthFeesOptionId"]?.ToString() == ((int)SmartWaterMeter.MonthFeesOptionsEnum.FixedFeesPerUnit).ToString())
        //                {
        //                    var FixedFeesPerUnit = Convert.ToDouble(!string.IsNullOrEmpty(feesTable.Rows[0]["FixedFeesPerUnit"]?.ToString()) ? feesTable.Rows[0]["FixedFeesPerUnit"]?.ToString() : "0");
        //                    fixedFees += FixedFeesPerUnit * Convert.ToInt32(dr["UnitNo"]?.ToString()) * tax;
        //                }
        //            }
        //        }

        //        return Math.Ceiling(fixedFees * 10000) / 10000;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterFixedFee", ex);
        //    }
        //}

        ///// <summary>
        ///// Get meter fixed fees (Fixed Estidama)
        ///// </summary>
        ///// <param name="meterID">meter id</param>
        ///// <param name="startDate">Tariff start Date</param>
        ///// <param name="ActivityId">Activity Id</param>
        ///// <param name="unitNo">Unit No</param>
        ///// <returns>Meter fixed fee values</returns>
        //public static ushort[] GetMeterFixedFees(string meterID, string startDate, string ActivityId, int? unitNo)
        //{
        //    var monthfees = new ushort[2];

        //    try
        //    {
        //        dboperation db = new dboperation();

        //        if (unitNo == null)
        //        {
        //            unitNo = 1;
        //        }

        //        string query = "";

        //        if (!string.IsNullOrEmpty(ActivityId))
        //        {
        //            query = "Select '" + ActivityId + "'as CatagoryId , '" + unitNo + "' as [UnitNo] ";
        //        }
        //        else
        //        {
        //            query = " Select ActivityID as CatagoryId , GuCode as UnitNo from Meters Where MeterId = '" + meterID + "'";
        //        }

        //        var meterUnitsActivitiesDT = db.SelectData(query);

        //        foreach (DataRow dr in meterUnitsActivitiesDT.Rows)
        //        {
        //            var feeTypeDataTable = db.SelectData("select top(1)  f.Feevalue , f.FixedFeesPerUnit  ,(select top 1 tax from settings) as Tax from FeeTypes f inner join Activities a on a.ID = f.Activity  where  f.feeFlag = 1 and f.IsDeleted = 0 and f.Activity = '" + dr["CatagoryId"].ToString() + "' and (f.MeterDimension = (select top 1 mt.Metertype from meters m join MeterTypes mt on m.MeterType = mt.ID where m.MeterID = '" + meterID + "')  or a.IsResidential = 1 ) and convert( datetime ,  f.StartDate , 103 ) = '" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' ");

        //            if (feeTypeDataTable.Rows.Count > 0)
        //            {
        //                var tax = Convert.ToDouble(!string.IsNullOrEmpty(feeTypeDataTable.Rows[0]["Tax"]?.ToString()) ? feeTypeDataTable.Rows[0]["Tax"]?.ToString() : "0");
        //                tax = 1 + (tax != 0 ? (tax / 100) : tax);
        //                monthfees[0] = (ushort)(Convert.ToDouble(feeTypeDataTable.Rows[0]["Feevalue"]?.ToString()) * 100 * tax);
        //                monthfees[1] = (ushort)(Convert.ToDouble(feeTypeDataTable.Rows[0]["FixedFeesPerUnit"]?.ToString()) * 100 * tax);
        //            }
        //        }

        //        return monthfees;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterFixedFee", ex);
        //    }
        //}

        ///// <summary>
        ///// Get meter fixed fees by activity identifier (Fixed Estidama)
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="activityID">Activity ididentifierparam>
        ///// <param name="UnitNo">Unit no</param>
        ///// <returns>Meter fixed fee values</returns>
        //public static decimal GetMeterEstidamaByActivityID(string meterID, string activityID, int UnitNo, System.DateTime tarriffDate, decimal quantity)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        var query = "select [dbo].[GetActivityFixedFeeWithConsumption] ('" + meterID + "','" + activityID + "','" + UnitNo + "','" + tarriffDate.ToString("yyyy-MM-dd") + "'," + quantity + ")"; // GetMeterFixedFee
        //        var fixedFee = Convert.ToDecimal(db.ReturnStr(query));
        //        return fixedFee;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterFixedFees", ex);
        //        return 0;
        //    }
        //}

        ///// <summary>
        ///// Get meter fixed fees (Fixed Estidama)
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <returns>Meter fixed fee values</returns>
        //public static decimal GetMeterEstidama(string meterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        var query = "select [dbo].[GetMeterFixedFee] ('" + meterID + "')";
        //        var fixedFee = Convert.ToDecimal(db.ReturnStr(query));
        //        return fixedFee;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetMeterFixedFees", ex);
        //        return 0;
        //    }
        //}

        ///// <summary>
        ///// Get meter Variable fees (Variable Estidama)
        ///// </summary>
        ///// <param name="meterID">meter id</param>
        ///// <param name="startDate">Tariff start Date</param>
        ///// <param name="ActivityId">Activity Id</param>
        ///// <param name="unitNo">Unit No</param>
        ///// <returns>Meter variable fee values</returns>
        //public static double[] GetMeterVariableFee(string meterID, string meterType, string startDate, string ActivityId, int? unitNo, int stairsCount = 9)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();

        //        if (unitNo == null)
        //        {
        //            unitNo = 1;
        //        }

        //        var variableFee = new double[stairsCount];
        //        var query = !string.IsNullOrEmpty(ActivityId) ? "Select '" + ActivityId + "'as CatagoryId , '" + unitNo + "' as [UnitNo] " : " Select ActivityID as CatagoryId, GuCode as UnitNo from Meters Where MeterId = '" + meterID + "'";
        //        var meterUnitsActivitiesDT = db.SelectData(query);
        //        double CummlativeVariableFees = 0;

        //        foreach (DataRow dr in meterUnitsActivitiesDT.Rows)
        //        {
        //            string Query = "select MonthStepFees,MonthFeesOptionId, (select top 1 tax from settings) as Tax , IsMonthStepFeesCumulative from dbo.TariffDetails  " +
        //                            "INNER JOIN Activities ON Activities.ID = TariffDetails.ActivityID  where  MeterTypeID  = 1 and CAST(StartDate AS DATE ) = '" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' " +
        //                            "and ActivityID =  '" + dr["CatagoryId"].ToString() + "'";
        //            var StepsDT = db.SelectData(Query);

        //            var tax = Convert.ToDouble(!string.IsNullOrEmpty(StepsDT.Rows[0]["Tax"]?.ToString()) ? StepsDT.Rows[0]["Tax"]?.ToString() : "0");
        //            tax = 1 + (tax != 0 ? (tax / 100) : tax);
        //            int i = 0;

        //            foreach (DataRow step in StepsDT.Rows)
        //            {
        //                var activityStepFeeValue = Convert.ToDouble(!string.IsNullOrEmpty(step["MonthStepFees"]?.ToString()) ? step["MonthStepFees"]?.ToString() : "0");

        //                if (StepsDT.Rows[0]["MonthFeesOptionId"]?.ToString() == ((int)SmartWaterMeter.MonthFeesOptionsEnum.StepFees).ToString())
        //                {
        //                    variableFee[i] += activityStepFeeValue * tax;
        //                }
        //                else if (StepsDT.Rows[0]["MonthFeesOptionId"]?.ToString() == ((int)SmartWaterMeter.MonthFeesOptionsEnum.StepFeesPerUnit).ToString())
        //                {
        //                    variableFee[i] += activityStepFeeValue * Convert.ToInt32(dr["UnitNo"]?.ToString()) * tax;
        //                }

        //                bool isMonthStepFeesCumulative = Convert.ToBoolean(StepsDT.Rows[i]["IsMonthStepFeesCumulative"]?.ToString());

        //                if (!isMonthStepFeesCumulative && i > 0 && variableFee[i] > 0)
        //                {
        //                    variableFee[i] = variableFee[i] - CummlativeVariableFees;
        //                    if (variableFee[i] < 0)
        //                        throw new Exception("GetMeterVariableFee : variableFee cant be minus");

        //                }

        //                CummlativeVariableFees += variableFee[i];
        //                i++;
        //            }
        //        }

        //        for (int i = 0; i < variableFee.Length; i++)
        //        {
        //            variableFee[i] = Math.Ceiling(variableFee[i] * 10000) / 10000;
        //        }

        //        return variableFee;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterVariableFee", ex);
        //    }
        //}

        //#endregion

        //#region Get Tariff operations

        ///// <summary>
        ///// Get last tarrif or last tarrif before specific date
        ///// </summary>
        ///// <param name="ActivityID">Activity identifier</param>
        ///// <param name="tarrifDate">Specific tarrif date</param>
        ///// <returns>Tarriff details</returns>
        //public static DataTable GetTariff(string ActivityID, System.DateTime? tarrifDate = null)
        //{
        //    try
        //    {
        //        string sql = " SELECT ActivityID , StairID , StairTo , StairValue , Activities.alarmamt, Activities.dreditamt ,Activities.Name, InitialFees  ,SwgPercent,SwgPrice,PerMeterFees," +
        //            " (select top 1 tax from Settings with(nolock)) as tax , " +
        //            " CustomersServiceFees,IsCumulative,IsNoOfUnitsIncludedInCalc  , CAST(StartDate AS date) as StartDate , StepSwgPrice,IsStepSwgPrice, " +
        //            " MinimumFee, MaximumFee, Activities.CurrencyRatio  ,  StairID AS [From],StairTo AS [To], StairValue AS Value,MonthFeesOptionId,MonthStepFees, " +
        //            " Activities.Stair , Healthy , PerMeterFees as ServiceBox  , ISNULL(exceptionvalue , 0) as exceptionvalue , ClosedMeterMonthFees ,IsMonthStepFeesCumulative,TariffDetails.id as tariffId, ChargeWithDebitChargeNetValue " +
        //            " FROM TariffDetails  with(nolock) INNER JOIN Activities with(nolock) ON Activities.ID = TariffDetails.ActivityID " +
        //            " WHERE ( CAST( TariffDetails.StartDate AS DATE) = (SELECT MAX(CAST(tt.startdate AS date)) " +
        //            " FROM tariffdetails tt with(nolock) WHERE ";

        //        if (tarrifDate == null)
        //        {
        //            // Get current tariff
        //            sql += "GETDATE() >= CAST(tt.startdate AS date) AND ActivityID ='" + ActivityID + "' ";
        //        }
        //        else
        //        {
        //            // Get tariff after specific date
        //            sql += "CAST('" + tarrifDate.Value.ToString("yyyy-MM-dd") + "' AS date) >= CAST(tt.startdate AS date) AND ActivityID ='" + ActivityID + "' ";
        //        }

        //        sql += " )) AND TariffDetails.ActivityID ='" + ActivityID + "'";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetTariff", ex);
        //    }


        //}

        ///// <summary>
        ///// Get activity current tariff date
        ///// </summary>
        ///// <param name="ActivityID">Activity identifier</param>
        ///// <returns>Current tarriff date</returns>
        //public static string GetActivityCurrentTariffDate(string ActivityID)
        //{
        //    try
        //    {
        //        string sql = "select top 1 CONVERT(Date, StartDate ) from TariffDetails with(nolock) where ActivityID = '" + ActivityID + "' and StartDate <=  CONVERT(datetime, FORMAT(GETDATE(), 'yyyy-MM-dd'), 101 )  group by StartDate order by StartDate desc";
        //        return new dboperation().ReturnStr(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetActivityCurrentTariffDate", ex);
        //    }
        //}

        ///// <summary>
        ///// Get activity next tariff date
        ///// </summary>
        ///// <param name="ActivityID">Activity identifier</param>
        ///// <returns>Next tarriff date</returns>
        //public static string GetActivityNextTariffDate(string ActivityID)
        //{
        //    try
        //    {
        //        string sql = "select top 1 CONVERT(Date, StartDate ) from TariffDetails with(nolock) where ActivityID = '" + ActivityID + "' and StartDate >=  CONVERT(datetime, '" + System.DateTime.Now.ToString("yyyy-MM-dd") + "' , 101 )  group by StartDate order by StartDate";
        //        return new dboperation().ReturnStr(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetActivityNextTariffDate", ex);
        //    }
        //}

        ///// <summary>
        ///// Get meter stairs details for [RFID,Smart,V5 and V8] meters
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="haveSewage">Have sewage</param>
        ///// <param name="country">Water meter country</param>
        ///// <param name="meterStairsCount">Meter stairs size</param>
        ///// <param name="meterTotalUnitNo">Meter total unit number</param>
        ///// <param name="chargingFee">Meter charging fees</param>
        ///// <param name="chargingMode">Meter charging mode</param>
        ///// <returns> Meter tariff</returns>
        //public static MeterTariffa GetMeterStairs(string meterID, string haveSewage, VersionCountry country, int meterStairsCount, int meterTotalUnitNo, string chargingFee, bool chargingMode, ClientCardTypeEnum operation, BasicTarrifModel specificTarrifModel)
        //{
        //    MeterTariffa result = null;
        //    SellingCardRepository scRepo = new SellingCardRepository();
        //    var tariffActivity = "";

        //    try
        //    {
        //        if (specificTarrifModel == null)
        //        {
        //            specificTarrifModel = new BasicTarrifModel();
        //        }
        //        else if (operation == ClientCardTypeEnum.ReplaceCard)
        //        {
        //            tariffActivity = specificTarrifModel.activityId;
        //            meterTotalUnitNo = int.Parse(specificTarrifModel.unitNo);
        //            haveSewage = specificTarrifModel.sewage;
        //        }

        //        int rowNum = 0;
        //        string chargeMode = string.Empty;
        //        DataRow firstStair;
        //        int monthFeeOptionId = 0;
        //        DataTable ActivitesTariffa = new DataTable();
        //        _customerCardRepo = new CustomerCardRepository();
        //        List<MeterTariffa> allActivities = new List<MeterTariffa>();
        //        string currentMeterType = "";

        //        // Get meter units and activites
        //        var CurrentMeterData = GetMeterCurrentActivity(meterID);
        //        var totalunitNofromMeter = 0;
        //        currentMeterType = CurrentMeterData.Rows[0]["MeterModelVersionID"].ToString();

        //        if (CurrentMeterData.Rows.Count > 0)
        //        {
        //            // Get meter activites tariffa
        //            foreach (DataRow currentData in CurrentMeterData.Rows)
        //            {
        //                var meterLastCharges = scRepo.GetMeterLastSuccessCharge(meterID);
        //                string LastActivity = "";

        //                if (meterLastCharges.Rows.Count > 0)
        //                {
        //                    LastActivity = meterLastCharges.Rows[0]["ActivityID"].ToString();
        //                }

        //                // Case new one or different activity
        //                if (LastActivity != currentData[0].ToString() && operation != ClientCardTypeEnum.ReplaceCard)
        //                {
        //                    specificTarrifModel.tarrifStartDate = null;
        //                }

        //                if (specificTarrifModel == null && operation == ClientCardTypeEnum.ReplaceCard)
        //                {
        //                    throw new Exception("GetMeterStairs : replace card with empty data");
        //                }
        //                tariffActivity = (operation != ClientCardTypeEnum.ReplaceCard) ? currentData[0].ToString() : specificTarrifModel.activityId;
        //                var dt = GetTariff(tariffActivity, specificTarrifModel.tarrifStartDate);
        //                monthFeeOptionId = int.Parse(dt.Rows[0]["MonthFeesOptionId"].ToString());
        //                DataColumn unitNumberColumn = new DataColumn("UnitNo", typeof(string));
        //                unitNumberColumn.DefaultValue = (operation != ClientCardTypeEnum.ReplaceCard) ? currentData[3].ToString() : specificTarrifModel.unitNo;
        //                totalunitNofromMeter = (operation != ClientCardTypeEnum.ReplaceCard) ? int.Parse(currentData[3].ToString()) : int.Parse(specificTarrifModel.unitNo);
        //                dt.Columns.Add(unitNumberColumn);
        //                ActivitesTariffa.Merge(dt);
        //                break;
        //            }

        //            if (ActivitesTariffa.Rows.Count > 0)
        //            {
        //                // Update avtivities stairs table
        //                DataColumn finalPriceColumn = new DataColumn("FinalPrice", typeof(string));
        //                finalPriceColumn.DefaultValue = "0";
        //                ActivitesTariffa.Columns.Add(finalPriceColumn);
        //                DataColumn levelColumn = new DataColumn("Level", typeof(string));
        //                levelColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(levelColumn);
        //                DataColumn quantityColumn = new DataColumn("Quantity", typeof(string));
        //                quantityColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(quantityColumn);
        //                DataColumn chargeModeColumn = new DataColumn("ChargeMode", typeof(string));
        //                chargeModeColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(chargeModeColumn);
        //                DataColumn powerLimitColumn = new DataColumn("PowerLimit", typeof(string));
        //                powerLimitColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(powerLimitColumn);
        //                DataColumn chargingFeeColumn = new DataColumn("ChargingFee", typeof(string));
        //                chargingFeeColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(chargingFeeColumn);
        //                DataColumn chargeNumColumn = new DataColumn("ChargeNum", typeof(string));
        //                chargeNumColumn.DefaultValue = string.Empty;
        //                ActivitesTariffa.Columns.Add(chargeNumColumn);

        //                // Prepare tarrif data
        //                foreach (var group in ActivitesTariffa.AsEnumerable().GroupBy(row => row.Field<string>("activityId")))
        //                {
        //                    // Init activity paramters 
        //                    rowNum = 0;
        //                    firstStair = group.FirstOrDefault();

        //                    // Set stairs final price,level and quantity
        //                    foreach (DataRow stair in group)
        //                    {
        //                        // Calculate final price for all stairs
        //                        if (country == VersionCountry.WaterRFIDEGYPTTENDER)
        //                        {
        //                            stair["FinalPrice"] = GetStairValueWithTax(Convert.ToDouble(stair["StairValue"].ToString()),
        //                                Convert.ToDouble(stair["SwgPercent"].ToString()), Convert.ToDouble(stair["SwgPrice"].ToString()), Convert.ToDouble(stair["PerMeterFees"].ToString()),
        //                                int.Parse(haveSewage.ToString()), Convert.ToDouble(stair["CustomersServiceFees"].ToString()), Convert.ToBoolean(stair["IsStepSwgPrice"].ToString()), Convert.ToDouble(stair["StepSwgPrice"].ToString()));
        //                        }
        //                        else
        //                        {
        //                            stair["FinalPrice"] = Convert.ToDouble(stair["StairValue"].ToString());
        //                        }

        //                        // Set level and quantity for all stairs (except last one with unlimited [to])
        //                        if (rowNum < group.Count() - 1)
        //                        {
        //                            if (stair["IsNoOfUnitsIncludedInCalc"].ToString() == "1" || stair["IsNoOfUnitsIncludedInCalc"].ToString() == "True")
        //                            {
        //                                stair["Level"] = Convert.ToDouble(stair["Stairto"].ToString()) * meterTotalUnitNo;
        //                                stair["Quantity"] = Convert.ToInt16(stair["Stairto"].ToString()) * meterTotalUnitNo;
        //                            }
        //                            else
        //                            {
        //                                stair["Level"] = Convert.ToDouble(stair["Stairto"].ToString());
        //                                stair["Quantity"] = Convert.ToInt16(stair["Stairto"].ToString());
        //                            }
        //                        }

        //                        rowNum++;
        //                    }

        //                    // Set charge mode
        //                    for (int i = 1; i < meterStairsCount; i++)
        //                    {
        //                        try
        //                        {
        //                            // Set charge mode (Except last stair)
        //                            if (i < meterStairsCount - 1)
        //                            {
        //                                if (group.Count() >= i + 1)
        //                                {
        //                                    if (string.IsNullOrEmpty(group.ElementAt(i)["StairID"]?.ToString()) || Convert.ToDouble(group.ElementAt(i)["StairID"]?.ToString()) == 0)
        //                                    {
        //                                        chargeMode = "1" + chargeMode;
        //                                    }
        //                                    else
        //                                    {
        //                                        chargeMode = "0" + chargeMode;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    chargeMode = "1" + chargeMode;
        //                                }
        //                            }

        //                            if (group.Count() > i)
        //                            {
        //                                // Update final price (Except first stair as same price)
        //                                if (string.IsNullOrEmpty(group.ElementAt(i)["FinalPrice"]?.ToString()) || Convert.ToDouble(group.ElementAt(i)["FinalPrice"]?.ToString()) == 0)
        //                                    group.ElementAt(i)["FinalPrice"] = group.ElementAt(i - 1)["FinalPrice"];

        //                                // Update levels and quantities (Except first stair and last one)
        //                                if (i < meterStairsCount - 1)
        //                                {
        //                                    if (string.IsNullOrEmpty(group.ElementAt(i)["Level"]?.ToString()) || Convert.ToDouble(group.ElementAt(i)["Level"]?.ToString()) == 0)
        //                                        group.ElementAt(i)["Level"] = Convert.ToDouble(group.ElementAt(i - 1)["Level"].ToString()) + (10 * meterTotalUnitNo);

        //                                    if (string.IsNullOrEmpty(group.ElementAt(i)["Quantity"]?.ToString()) || Convert.ToDouble(group.ElementAt(i)["Quantity"]?.ToString()) == 0)
        //                                        group.ElementAt(i)["Quantity"] = Convert.ToDouble(group.ElementAt(i - 1)["Quantity"].ToString()) + meterTotalUnitNo;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {
        //                            throw new InvalidOperationException("Failed to set charge mode");
        //                        }
        //                    }

        //                    if (chargeMode != "")
        //                    {
        //                        // Set power limit,ChargingFee,ChargeNum,CurrencyRatio
        //                        foreach (DataRow stair in group)
        //                        {
        //                            stair["ChargeMode"] = Convert.ToInt32(chargeMode, 2);
        //                            stair["PowerLimit"] = decimal.Parse(string.IsNullOrEmpty(firstStair["alarmamt"].ToString()) ? "0" : firstStair["alarmamt"].ToString());

        //                            if (country == VersionCountry.WaterRFIDEGYPTTENDER)
        //                            {
        //                                if (chargingMode)
        //                                {
        //                                    stair["ChargingFee"] = Convert.ToDouble(chargingFee);
        //                                    stair["ChargeNum"] = Convert.ToDouble(chargingFee) > 0 ? 1 : 0;
        //                                    stair["PowerLimit"] = decimal.Parse(stair["PowerLimit"].ToString()) * decimal.Parse(chargingFee) / 100;
        //                                }
        //                                else
        //                                {
        //                                    if (Convert.ToDouble(firstStair["InitialFees"].ToString()) > 0)
        //                                    {
        //                                        stair["ChargingFee"] = Convert.ToDouble(firstStair["InitialFees"].ToString());
        //                                        stair["ChargeNum"] = 1;
        //                                        stair["PowerLimit"] = decimal.Parse(stair["PowerLimit"].ToString()) * decimal.Parse(stair["ChargingFee"].ToString()) / 100;
        //                                    }
        //                                    else
        //                                    {
        //                                        stair["ChargingFee"] = 0;
        //                                        stair["ChargeNum"] = 0;
        //                                        stair["PowerLimit"] = 0;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (chargingMode)
        //                                {
        //                                    stair["ChargingFee"] = Convert.ToDouble(chargingFee);
        //                                    stair["ChargeNum"] = Convert.ToDouble(chargingFee) > 0 ? 1 : 0;
        //                                    stair["PowerLimit"] = decimal.Parse(stair["PowerLimit"].ToString()) * decimal.Parse(chargingFee) / 100;
        //                                }
        //                                else
        //                                {
        //                                    if (Convert.ToDouble(firstStair["InitialFees"].ToString()) > 0)
        //                                    {
        //                                        if (SmartWaterMeter.watersys.salemode == 0)
        //                                        {
        //                                            stair["ChargingFee"] = Convert.ToDouble(firstStair["InitialFees"].ToString());
        //                                        }
        //                                        else
        //                                        {
        //                                            stair["ChargingFee"] = calcWaterReadingQuantity(Convert.ToDecimal(Convert.ToDouble(firstStair["InitialFees"].ToString())), GetTariff(stair["activityId"].ToString()));
        //                                        }
        //                                        stair["ChargeNum"] = 1;
        //                                    }
        //                                    else
        //                                    {
        //                                        stair["ChargeNum"] = 1;
        //                                        stair["ChargingFee"] = 0;
        //                                    }
        //                                }
        //                            }

        //                            stair["CurrencyRatio"] = decimal.Parse(string.IsNullOrEmpty(firstStair["CurrencyRatio"]?.ToString()) ? "1" : firstStair["CurrencyRatio"].ToString());
        //                        }
        //                    }

        //                    // Set tariff steps
        //                    var tarrifaStep = 0;

        //                    if (group.Key.IndexOf('-') > 0)
        //                        tarrifaStep = Convert.ToInt16(group.Key.Substring(group.Key.IndexOf('-') + 1, group.Key.Length - group.Key.IndexOf('-') - 1));
        //                    else
        //                        tarrifaStep = int.Parse(group.Key);

        //                    var totalUnitNo = totalunitNofromMeter;
        //                    tarrifaStep = EGYSetActivityCodeinMeter(tarrifaStep, totalUnitNo, int.Parse(haveSewage));

        //                    var currentActivity = new MeterTariffa()
        //                    {
        //                        Author = firstStair["ChargeMode"].ToString(),
        //                        PowerLimit = firstStair["PowerLimit"].ToString(),
        //                        ChargingFee = firstStair["ChargingFee"].ToString(),
        //                        ChargeNum = firstStair["ChargeNum"].ToString(),
        //                        CurrencyRatio = firstStair["CurrencyRatio"].ToString() != "" ? decimal.Parse(firstStair["CurrencyRatio"].ToString()) : 1,
        //                        TariffSteps = tarrifaStep.ToString(),
        //                        StartDate = firstStair["StartDate"].ToString(),
        //                        CutOffWarningPercentage = double.Parse(firstStair["alarmamt"].ToString()) / 100,
        //                        OverdraftAmount = double.Parse(firstStair["dreditamt"].ToString())
        //                    };

        //                    currentActivity.Price = new double[meterStairsCount];
        //                    currentActivity.Level = new double[meterStairsCount - 1];
        //                    currentActivity.Quantity = new int[meterStairsCount - 1];
        //                    currentActivity.StairFrom = new ushort[meterStairsCount];
        //                    currentActivity.StairTo = new ushort[meterStairsCount];

        //                    for (int r = 0; r < meterStairsCount; r++)
        //                    {
        //                        if (r < group.Count())
        //                        {
        //                            currentActivity.Price[r] = Convert.ToDouble(group.ElementAt(r)["FinalPrice"].ToString());
        //                            currentActivity.StairFrom[r] = Convert.ToUInt16(group.ElementAt(r)["From"].ToString());
        //                            currentActivity.StairTo[r] = Convert.ToUInt16(group.ElementAt(r)["To"].ToString());

        //                            if (r < group.Count() - 1)
        //                            {
        //                                if (group.ElementAt(r)["IsNoOfUnitsIncludedInCalc"].ToString() == "1" || group.ElementAt(r)["IsNoOfUnitsIncludedInCalc"].ToString() == "True")
        //                                {
        //                                    currentActivity.StairFrom[r] = Convert.ToUInt16(currentActivity.StairFrom[r] * Int16.Parse(group.ElementAt(r)["UnitNo"].ToString()));
        //                                    currentActivity.StairTo[r] = Convert.ToUInt16(currentActivity.StairTo[r] * Int16.Parse(group.ElementAt(r)["UnitNo"].ToString()));
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            currentActivity.Price[r] = currentActivity.Price[r - 1];
        //                            currentActivity.StairFrom[r] = currentActivity.StairFrom[r - 1];
        //                            currentActivity.StairTo[r] = currentActivity.StairTo[r - 1];
        //                        }

        //                        if (r < meterStairsCount - 1 && r < group.Count())
        //                        {
        //                            if (!string.IsNullOrEmpty(group.ElementAt(r)["Level"].ToString()) && group.ElementAt(r)["Level"].ToString() != "0")
        //                            {
        //                                currentActivity.Level[r] = Convert.ToDouble(group.ElementAt(r)["Level"].ToString());
        //                                currentActivity.Quantity[r] = Convert.ToInt16(group.ElementAt(r)["Quantity"].ToString());
        //                            }
        //                            else
        //                            {
        //                                // Not handle levels for one stair activities
        //                                if (r > 0)
        //                                {
        //                                    currentActivity.Level[r] = Convert.ToDouble(group.ElementAt(r - 1)["Level"].ToString() + 10);
        //                                    currentActivity.Quantity[r] = Convert.ToInt16(group.ElementAt(r - 1)["Quantity"].ToString() + 10);
        //                                }
        //                            }
        //                        }
        //                        else if (r < meterStairsCount - 1 && r >= group.Count())
        //                        {
        //                            // Not handle levels for one stair activities
        //                            if (r > 0)
        //                            {
        //                                currentActivity.Level[r] = Convert.ToDouble(currentActivity.Level[r - 1] + 10);
        //                                currentActivity.Quantity[r] = Convert.ToInt16(currentActivity.Level[r - 1] + 10);
        //                            }
        //                        }
        //                    }

        //                    allActivities.Add(currentActivity);
        //                }

        //                // Calculate fees
        //                var fixFee = CalculateFixedFees(meterID, allActivities.FirstOrDefault().StartDate, tariffActivity, meterTotalUnitNo);
        //                var variableFee = GetMeterVariableFee(meterID, currentMeterType, allActivities.FirstOrDefault().StartDate, tariffActivity, meterTotalUnitNo, 9);

        //                result = new MeterTariffa()
        //                {
        //                    Price = new double[meterStairsCount],
        //                    Level = allActivities.FirstOrDefault().Level,
        //                    Quantity = allActivities.FirstOrDefault().Quantity,
        //                    Author = allActivities.FirstOrDefault().Author,
        //                    PowerLimit = allActivities.FirstOrDefault().PowerLimit,
        //                    ChargingFee = allActivities.FirstOrDefault().ChargingFee,
        //                    ChargeNum = allActivities.FirstOrDefault().ChargeNum,
        //                    CurrencyRatio = allActivities.FirstOrDefault().CurrencyRatio,
        //                    TariffSteps = allActivities.Count > 1 ? "255" : allActivities.FirstOrDefault().TariffSteps.ToString(),
        //                    StartDate = allActivities.FirstOrDefault().StartDate,
        //                    FixFee = fixFee,
        //                    VariableFee = variableFee,
        //                    StairFrom = allActivities.FirstOrDefault().StairFrom,
        //                    StairTo = allActivities.FirstOrDefault().StairTo,
        //                    CutOffWarningPercentage = allActivities.FirstOrDefault().CutOffWarningPercentage,
        //                    OverdraftAmount = allActivities.FirstOrDefault().OverdraftAmount,
        //                    MonthFeesOptionId = monthFeeOptionId
        //                };

        //                for (int i = 0; i < result.Price.Length; i++)
        //                {
        //                    result.Price[i] = allActivities.Sum(r => r.Price[i]);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Multilingual.Messages.Show("1055");
        //        MakeExceptionLog("Utility", "GetMeterStairs", ex);
        //        throw new InvalidOperationException("Utility GetMeterStairs : " + ex.ToString());
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Get meter stairs details for new models greater than 10
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="meterStairsCount">Meter stairs size</param>
        ///// <param name="chargingFee">Meter charging fees</param>
        ///// <param name="chargingMode">Meter charging mode</param>
        ///// <param name="readPriceSchedDate">Reading Price schedule</param>
        ///// <param name="CardType">Client Card Type Enum</param>
        ///// <returns> Meter tariffa</returns>
        //public static NewMeterTariff GetNewMeterStairs(string meterID, int meterStairsCount, string chargingFee, bool chargingMode, bool haveSwage, BasicTarrifModel specificTarrifModel, ClientCardTypeEnum cardType)
        //{
        //    NewMeterTariff result = null;
        //    SellingCardRepository scRepo = new SellingCardRepository();

        //    try
        //    {
        //        string chargeMode = string.Empty;
        //        short chargeNum = 0;
        //        string cummulativecharging = string.Empty;
        //        DataRow firstStair;
        //        DataTable ActivitesTariffa = new DataTable();
        //        _customerCardRepo = new CustomerCardRepository();
        //        List<NewMeterTariff> allActivities = new List<NewMeterTariff>();


        //        // Case same activity check current card tariff startdate
        //        if (specificTarrifModel.tarrifStartDate != null && cardType != ClientCardTypeEnum.ReplaceCard && cardType != ClientCardTypeEnum.Create)
        //        {
        //            string current = GetActivityCurrentTariffDate(specificTarrifModel.activityId.ToString());
        //            string newone = GetActivityNextTariffDate(specificTarrifModel.activityId.ToString());

        //            if (string.IsNullOrEmpty(newone))
        //            {
        //                newone = current;
        //            }

        //            if (specificTarrifModel.tarrifStartDate.Value.Date < System.DateTime.Parse(current))
        //            {
        //                // get current
        //                specificTarrifModel.tarrifStartDate = System.DateTime.Parse(current);
        //            }
        //            else if (specificTarrifModel.tarrifStartDate.Value.Date == System.DateTime.Parse(current))
        //            {
        //                // get new one
        //                specificTarrifModel.tarrifStartDate = System.DateTime.Parse(newone);
        //            }
        //            else if (specificTarrifModel.tarrifStartDate.Value.Date > System.DateTime.Parse(current) ||
        //                specificTarrifModel.tarrifStartDate.Value.Date == System.DateTime.Parse(newone))
        //            {
        //                // get new one
        //                specificTarrifModel.tarrifStartDate = System.DateTime.Parse(newone);
        //            }
        //            else
        //            {
        //                // get current
        //                specificTarrifModel.tarrifStartDate = System.DateTime.Parse(current);
        //            }
        //        }

        //        // mt.ActivityID,mt.DepartmentID,mt.PhaseNo,mt.GuCode
        //        var dt = GetTariff(specificTarrifModel.activityId, specificTarrifModel.tarrifStartDate);
        //        DataColumn unitNumberColumn = new DataColumn("UnitNo", typeof(string));
        //        unitNumberColumn.DefaultValue = specificTarrifModel.unitNo.ToString();
        //        dt.Columns.Add(unitNumberColumn);
        //        ActivitesTariffa.Merge(dt);

        //        if (ActivitesTariffa.Rows.Count > 0)
        //        {
        //            // Update avtivities stairs table
        //            DataColumn finalPriceColumn = new DataColumn("FinalPrice", typeof(string))
        //            {
        //                DefaultValue = 0
        //            };
        //            ActivitesTariffa.Columns.Add(finalPriceColumn);

        //            DataColumn levelColumn = new DataColumn("Level", typeof(string))
        //            {
        //                DefaultValue = 0
        //            };
        //            ActivitesTariffa.Columns.Add(levelColumn);

        //            DataColumn FeeColumn = new DataColumn("fee", typeof(string))
        //            {
        //                DefaultValue = 0
        //            };
        //            ActivitesTariffa.Columns.Add(FeeColumn);

        //            foreach (var group in ActivitesTariffa.AsEnumerable().GroupBy(row => row.Field<string>("activityId")))
        //            {
        //                // Init activity paramters 
        //                firstStair = group.FirstOrDefault();

        //                // Set stairs final price,level and quantity
        //                foreach (DataRow stair in group)
        //                {
        //                    stair["FinalPrice"] = stair["StairValue"].ToString();
        //                    stair["Level"] = stair["Stairto"].ToString();
        //                    stair["fee"] = stair["fee"].ToString();
        //                    stair["UnitNo"] = stair["StairValue"].ToString();
        //                }

        //                // Set cummulative charge
        //                for (int i = 1; i < meterStairsCount; i++)
        //                {
        //                    try
        //                    {
        //                        if (group.Count() >= i + 1)
        //                        {
        //                            if (!string.IsNullOrEmpty(group.ElementAt(i)["IsCumulative"].ToString()) && Convert.ToBoolean(group.ElementAt(i)["IsCumulative"].ToString()) == true)
        //                            {
        //                                cummulativecharging += "1";
        //                            }
        //                            else
        //                            {
        //                                cummulativecharging += "0";
        //                            }
        //                        }
        //                        else
        //                        {
        //                            cummulativecharging += "0";
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {
        //                        throw new InvalidOperationException("Failed to set charge mode");
        //                    }
        //                }

        //                // Set power limit,ChargingFee,ChargeNum,CurrencyRatio
        //                foreach (DataRow stair in group)
        //                {
        //                    if (chargingMode)
        //                    {
        //                        chargeNum = (short)(Convert.ToDouble(chargingFee) > 0 ? 1 : 0);
        //                    }
        //                    else
        //                    {
        //                        if (Convert.ToDouble(firstStair["InitialFees"].ToString()) > 0)
        //                        {
        //                            chargeNum = 1;
        //                        }
        //                    }
        //                }

        //                // Set tariff steps
        //                var currentActivity = new NewMeterTariff()
        //                {
        //                    ChargeNum = chargeNum.ToString(),
        //                    StartDate = firstStair["StartDate"].ToString(),
        //                    Pricing = Convert.ToUInt16(cummulativecharging, 2),
        //                    NoOfUnitsIncludedInCalc = Convert.ToBoolean(firstStair["IsNoOfUnitsIncludedInCalc"].ToString()) == true ? (byte)1 : (byte)0,
        //                    MonthFeesOptions = Convert.ToByte(string.IsNullOrEmpty(firstStair["MonthFeesOptionId"].ToString()) ? "0" : ((SmartWaterMeter.MonthFeesOptionsEnum)Convert.ToInt32(firstStair["MonthFeesOptionId"].ToString())).GetDescription()),
        //                    PerMeterFees = Convert.ToUInt16(Math.Ceiling((Convert.ToDouble(firstStair["PerMeterFees"].ToString()) + Convert.ToDouble(firstStair["CustomersServiceFees"].ToString())) * (1 + (Convert.ToDouble(firstStair["tax"].ToString()) / 100)) * 100)),// must be moth fees
        //                    SwgPercent = Convert.ToBoolean(firstStair["IsStepSwgPrice"].ToString()) ? Convert.ToByte(0) : Convert.ToByte(Convert.ToDouble(firstStair["SwgPercent"].ToString())),
        //                    SwgPrice = Convert.ToBoolean(firstStair["IsStepSwgPrice"].ToString()) ? Convert.ToByte(0) : Convert.ToUInt16(Convert.ToDouble(firstStair["SwgPrice"].ToString()) * 100),
        //                };

        //                currentActivity.Price = new ushort[meterStairsCount];
        //                currentActivity.StepMax = new ushort[meterStairsCount];
        //                currentActivity.monthStepFees = new ushort[meterStairsCount];
        //                currentActivity.StairFrom = new ushort[meterStairsCount];
        //                currentActivity.StairTo = new ushort[meterStairsCount];
        //                currentActivity.StairMode = new char[meterStairsCount];
        //                ushort cummlativeMonthStepFees = 0;

        //                for (int r = 0; r < meterStairsCount; r++)
        //                {
        //                    if (r < group.Count())
        //                    {

        //                        currentActivity.Price[r] = Convert.ToUInt16((Convert.ToDouble(group.ElementAt(r)["FinalPrice"].ToString()) + (haveSwage ? (Convert.ToBoolean(firstStair["IsStepSwgPrice"].ToString()) ? (Convert.ToDouble(group.ElementAt(r)["StepSwgPrice"].ToString()) * (Convert.ToDouble(firstStair["SwgPercent"].ToString()) / 100)) : 0) : 0)) * 100);
        //                        currentActivity.StepMax[r] = (ushort)(Convert.ToDouble(group.ElementAt(r)["Level"].ToString()));
        //                        currentActivity.monthStepFees[r] = Convert.ToUInt16(Convert.ToDouble(group.ElementAt(r)["MonthStepFees"].ToString()) * (1 + (Convert.ToDouble(firstStair["tax"].ToString()) / 100)) * 100);

        //                        try
        //                        {
        //                            currentActivity.StairFrom[r] = Convert.ToUInt16(group.ElementAt(r)["StairID"].ToString());
        //                            currentActivity.StairTo[r] = Convert.ToUInt16(group.ElementAt(r)["StairTo"].ToString());

        //                            if (!string.IsNullOrEmpty(group.ElementAt(r)["IsCumulative"].ToString()) && Convert.ToBoolean(group.ElementAt(r)["IsCumulative"].ToString()) == true)
        //                            {
        //                                currentActivity.StairMode[r] = '1';
        //                            }
        //                            else
        //                            {
        //                                currentActivity.StairMode[r] = '0';
        //                            }
        //                        }
        //                        catch
        //                        {

        //                        }
        //                    }
        //                    else
        //                    {
        //                        currentActivity.Price[r] = currentActivity.Price[r - 1];
        //                        currentActivity.StepMax[r] = ushort.MinValue;
        //                        currentActivity.monthStepFees[r] = currentActivity.monthStepFees[r - 1];
        //                        currentActivity.StairMode[r] = '0';
        //                    }
        //                    try
        //                    {
        //                        bool isCumulative = Convert.ToBoolean(group.ElementAt(r)["IsCumulative"].ToString());
        //                        bool isMonthStepFeesCumulative = Convert.ToBoolean(group.ElementAt(r)["IsMonthStepFeesCumulative"].ToString());

        //                        if ((isCumulative && !isMonthStepFeesCumulative) && r > 0 && currentActivity.monthStepFees[r] > 0)
        //                        {
        //                            currentActivity.monthStepFees[r] = (ushort)(currentActivity.monthStepFees[r] - cummlativeMonthStepFees);
        //                        }
        //                        if ((!isCumulative && isMonthStepFeesCumulative) && r > 0 && currentActivity.monthStepFees[r] > 0)
        //                        {
        //                            currentActivity.monthStepFees[r] = (ushort)(currentActivity.monthStepFees[r] + cummlativeMonthStepFees);
        //                        }

        //                        cummlativeMonthStepFees = isCumulative ? (ushort)(cummlativeMonthStepFees + currentActivity.monthStepFees[r]) : currentActivity.monthStepFees[r];
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                    }

        //                    if (currentActivity.monthStepFees[r] < 0)
        //                        throw new Exception("GetNewMeterStairs : monthStepFees cant be minus");
        //                }

        //                allActivities.Add(currentActivity);
        //            }

        //            var fixFee = GetMeterFixedFees(meterID, allActivities.FirstOrDefault().StartDate, specificTarrifModel.activityId, int.Parse(specificTarrifModel.unitNo));

        //            result = new NewMeterTariff()
        //            {
        //                StairFrom = new ushort[meterStairsCount],
        //                StairTo = new ushort[meterStairsCount],
        //                Price = new ushort[meterStairsCount],
        //                StairMode = new char[meterStairsCount],
        //                StepMax = allActivities.FirstOrDefault().StepMax,
        //                monthStepFees = allActivities.FirstOrDefault().monthStepFees,
        //                ChargeNum = allActivities.FirstOrDefault().ChargeNum,
        //                MonthFeesOptions = allActivities.FirstOrDefault().MonthFeesOptions,
        //                NoOfUnitsIncludedInCalc = allActivities.FirstOrDefault().NoOfUnitsIncludedInCalc,
        //                Pricing = allActivities.FirstOrDefault().Pricing,
        //                StartDate = allActivities.FirstOrDefault().StartDate,
        //                MonthFees = fixFee,
        //                PerMeterFees = allActivities.FirstOrDefault().PerMeterFees,
        //                SwgPrice = allActivities.FirstOrDefault().SwgPrice,
        //                SwgPercent = allActivities.FirstOrDefault().SwgPercent
        //            };

        //            for (int i = 0; i < result.Price.Length; i++)
        //            {
        //                result.Price[i] = (ushort)(allActivities.Sum(r => r.Price[i]));

        //                try
        //                {
        //                    result.StairFrom[i] = allActivities.FirstOrDefault().StairFrom[i];
        //                    result.StairTo[i] = allActivities.FirstOrDefault().StairTo[i];
        //                    result.StairMode[i] = allActivities.FirstOrDefault().StairMode[i];
        //                }
        //                catch
        //                {
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "GetNewMeterStairs", ex);
        //        throw new Exception("GetNewMeterStairs" + ex.ToString());
        //    }

        //    return result;
        //}

        //#endregion

        //#region General functions

        ///// <summary>
        /////  Apply meter change requests
        ///// </summary>
        ///// <param name="ApplyDate">Apply date</param>
        //public static void ApplyMeterChangeRequests(System.DateTime ApplyDate)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "ApplyMeterChangeRequests";
        //        db.objcmd.Parameters.AddWithValue("@ApplyDate", ApplyDate);
        //        db.ExecuteNonQuery("");
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "ApplyMeterChangeRequests", ex);
        //    }
        //}

        ///// <summary>
        /////  Get meter change request in specific date
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="SpecificDate">Specific date</param>
        //public static DataTable GetMeterChangesByDate(string meterID, System.DateTime SpecificDate)
        //{
        //    try
        //    {
        //        string sql = " select top 1 ActivityId , DepartmentId , GuCode , PhaseNo from [dbo].[MeterChangeRequest] with(nolock) where [MeterId] = '" + meterID + "' and [IsApplied] = 1 " +
        //                     " and CONVERT(datetime, ApplyDate, 101) <= CONVERT(datetime, '" + SpecificDate.ToString("yyyy-MM-dd") + "' , 101 )" +
        //                     " order by ApplyDate desc";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterChangesBeforeDate", ex);
        //    }
        //}

        ///// <summary>
        /////  Get first meter change request after specific date
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="SpecificDate">Specific date</param>
        //public static DataTable GetMeterChangesAfterDate(string meterID, System.DateTime SpecificDate)
        //{
        //    try
        //    {
        //        string sql = " select top 1 OldActivityId as ActivityId , OldDepartmentId as DepartmentId , OldGuCode as GuCode , OldPhaseNo as PhaseNo from [dbo].[MeterChangeRequest] with(nolock) where [MeterId] = '" + meterID + "' and [IsApplied] = 1 " +
        //                     " and CONVERT(datetime, ApplyDate, 101) > CONVERT(datetime, '" + SpecificDate.ToString("yyyy-MM-dd") + "' , 101 )" +
        //                     " order by ApplyDate";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterChangesBeforeDate", ex);
        //    }
        //}

        ///// <summary>
        /////  Get meter details
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        //public static DataTable GetMeterDetails(string meterID)
        //{
        //    try
        //    {
        //        string sql = " select top 1 ActivityID , GuCode , PhaseNo from [dbo].[Meters] with(nolock) where [MeterId] = '" + meterID + "'";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMeterDetails", ex);
        //    }
        //}

        ///// <summary>
        ///// Get last water meter reading before specific date
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="SpecificDate">Specific date</param>
        //public static DataTable GetLastWaterMeterReadingBeforeDate(string meterID, System.DateTime SpecificDate)
        //{
        //    try
        //    {
        //        string sql = " select top 1 ActivityID , GuCode , Sewage from [dbo].[WaterMetersReadings] with(nolock) where [MeterId] = '" + meterID + "' and CONVERT(datetime, serverDate, 101)  < CONVERT(datetime, '" + SpecificDate.AddMonths(1).ToString("yyyy-MM-dd") + "' , 101 ) order by serverDate desc";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetLastWaterMeterReadingBeforeDate", ex);
        //    }
        //}

        ///// <summary>
        ///// Get first water meter reading after specific date
        ///// </summary>
        ///// <param name="meterID">Meter identifier</param>
        ///// <param name="SpecificDate">Specific date</param>
        //public static DataTable GetFirstWaterMeterReadingAfterDate(string meterID, System.DateTime SpecificDate)
        //{
        //    try
        //    {
        //        string sql = " select top 1 ActivityID , GuCode , Sewage from [dbo].[WaterMetersReadings] with(nolock) where [MeterId] = '" + meterID + "' and CONVERT(datetime, serverDate, 101)  >= CONVERT(datetime, '" + SpecificDate.AddMonths(1).ToString("yyyy-MM-dd") + "' , 101 ) order by serverDate";
        //        return new dboperation().SelectData(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetFirstWaterMeterReadingAfterDate", ex);
        //    }
        //}

        //public static bool IsTodayIsCashierHoliday()
        //{
        //    dboperation db = new dboperation();
        //    string query = "SELECT Id, HolidayDate, Description, UserId,CONVERT(VARCHAR(10), getdate(), 120) FROM CashierHolidays with(nolock) where HolidayDate = CONVERT(VARCHAR(10), getdate(), 120)";
        //    var dt = db.SelectData(query);

        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public static bool IsThereProfiles()
        //{
        //    try
        //    {
        //        XmlDocument xmlLanguageDocument = new XmlDocument();
        //        xmlLanguageDocument.Load("..\\Config.xml");
        //        string strXpathExpression = "//Profiles";
        //        XmlNode ProfileNode = xmlLanguageDocument.SelectSingleNode(strXpathExpression);

        //        if (ProfileNode != null && ProfileNode.ChildNodes.Count > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool IsThereMultipleProfiles()
        //{
        //    try
        //    {
        //        XmlDocument xmlLanguageDocument = new XmlDocument();
        //        xmlLanguageDocument.Load("..\\Config.xml");
        //        string strXpathExpression = "//Profiles";
        //        XmlNode ProfileNode = xmlLanguageDocument.SelectSingleNode(strXpathExpression);

        //        if (ProfileNode != null && ProfileNode.ChildNodes.Count > 1)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool IsActiveProfile(string ProfileName)
        //{
        //    try
        //    {
        //        XmlDocument xmlLanguageDocument = new XmlDocument();
        //        xmlLanguageDocument.Load("..\\Config.xml");
        //        string strXpathExpression = "//Profile[@Name='" + Crypto.GetEncryptedValue(ProfileName, true) + "']";
        //        XmlNode ProfileNode = xmlLanguageDocument.SelectSingleNode(strXpathExpression);

        //        if (Crypto.GetDecryptedValue(ProfileNode.Attributes["Active"].Value, true) == "1")
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static void MakeExceptionLog(string ClassName, string BlockName, Exception ex)
        //{
        //    try
        //    {
        //        string time = System.DateTime.Now.ToShortTimeString();
        //        string strPath = "..\\Log\\" + cashedServerDateTimeFormated.Replace("/", "-") + ".sewedy";

        //        if (!Directory.Exists("..\\Log\\"))
        //        {
        //            Directory.CreateDirectory("..\\Log\\");
        //        }

        //        FileStream fs = null;

        //        if (!File.Exists(strPath))
        //        {
        //            fs = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.Write);
        //        }
        //        else
        //        {
        //            fs = new FileStream(strPath, FileMode.Append, FileAccess.Write, FileShare.Write);
        //        }

        //        StreamWriter file = new StreamWriter(fs);
        //        file.WriteLine("Request Time : " + time);
        //        file.WriteLine("Class name : " + ClassName);
        //        file.WriteLine("Block name : " + BlockName);
        //        file.WriteLine("Exception message : " + ex.Message);
        //        file.WriteLine("Exception stack: " + ex);
        //        file.WriteLine("");
        //        file.WriteLine("=================================================================");
        //        file.WriteLine("");
        //        file.Close();
        //        fs.Close();
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static void MakeErrorLog(string ClassName, string BlockName, string msg)
        //{
        //    try
        //    {
        //        string time = System.DateTime.Now.ToShortTimeString();
        //        string strPath = "..\\Log\\" + cashedServerDateTimeFormated.Replace("/", "-") + ".sewedy";

        //        if (!Directory.Exists("..\\Log\\"))
        //        {
        //            Directory.CreateDirectory("..\\Log\\");
        //        }

        //        FileStream fs = null;

        //        if (!File.Exists(strPath))
        //        {
        //            fs = new FileStream(strPath, FileMode.Append, FileAccess.Write, FileShare.Write);
        //        }
        //        else
        //        {
        //            fs = new FileStream(strPath, FileMode.Append, FileAccess.Write, FileShare.Write);
        //        }

        //        StreamWriter file = new StreamWriter(fs);
        //        file.WriteLine("Request Time : " + time);
        //        file.WriteLine("Class name : " + ClassName);
        //        file.WriteLine("Block name : " + BlockName);
        //        file.WriteLine("Error message : " + msg);
        //        file.WriteLine("");
        //        file.WriteLine("=================================================================");
        //        file.WriteLine("");
        //        file.Close();
        //        fs.Close();
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static Bitmap GetCompanyLogo()
        //{
        //    try
        //    {
        //        DataTable Tbl = new dboperation().SelectData("SELECT CompanyLogo FROM Settings with(nolock)");
        //        byte[] data = (byte[])Tbl.Rows[0]["CompanyLogo"];

        //        if (data.Length > 0)
        //        {
        //            MemoryStream stream = new MemoryStream(data);

        //            // converting image to byte array 
        //            return ((Bitmap)Image.FromStream(stream));
        //        }

        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static bool CheckDataBaseNumber()
        //{
        //    try
        //    {
        //        if (frmMain.DBNumber != new dboperation().GetSingleValue(" SELECT top 1 DataBaseNumber FROM Settings with(nolock)").ToString())
        //        {
        //            Multilingual.Messages.Show(null, " Your Data Base Has Been Defected.\n You Must Call System Administrator.");
        //            return false;
        //        }

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Check in table Charges if the payment receipt is unique in station or not 
        ///// </summary>
        ///// <param name="vendingStationID">vending station identifier</param>
        ///// <param name="paymentReceiptNumber">payment receipt number</param>
        ///// <returns></returns>
        //public static bool ValidatePaymentReceiptNumberExists(string vendingStationID, string paymentReceiptNumber)
        //{
        //    try
        //    {
        //        string sqlQuery = "select top 1 ID from Charges with(nolock) Where VendingStationID = '" + vendingStationID + "' and PaymentNumber = '" + paymentReceiptNumber + "' and PaymentType = 'PaymentWithReceipt'";

        //        if (new dboperation().GetSingleValue(sqlQuery) != null)
        //        {
        //            return false;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "ValidatePaymentReceiptNumberExists", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Check in table AdjustmentPayments if the payment receipt is unique in station or not
        ///// 
        ///// </summary>
        ///// <param name="vendingStationID">vending station identifier</param>
        ///// <param name="paymentReceiptNumber">payment receipt number</param>
        ///// <returns></returns>
        //public static bool ValidatePaymentReceiptNumberExistsInAdjustments(string vendingStationID, string paymentReceiptNumber)
        //{
        //    try
        //    {
        //        string sqlQuery = "select top 1 ID from AdjustmentPayments with(nolock) Where VendingStationID = '" + vendingStationID + "' and PaymentNumber = '" + paymentReceiptNumber + "' and PaymentType = 'PaymentWithReceipt'";

        //        if (new dboperation().GetSingleValue(sqlQuery) != null)
        //        {
        //            return false;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "ValidatePaymentReceiptNumberExistsInAdjustments", ex);
        //        return false;
        //    }
        //}

        //public static string HexToDecimal(int iDec, int numbase)
        //{
        //    try
        //    {
        //        string strBin = "";
        //        int[] result = new int[32];
        //        int MaxBit = 32;

        //        for (; iDec > 0; iDec /= numbase)
        //        {
        //            int rem = iDec % numbase;
        //            result[--MaxBit] = rem;
        //        }

        //        for (int i = 0; i < result.Length; i++)
        //        {
        //            if ((int)result.GetValue(i) >= base10)
        //                strBin += cHexa[(int)result.GetValue(i) % base10];
        //            else
        //                strBin += result.GetValue(i);
        //        }

        //        strBin = strBin.TrimStart(new char[] { '0' });
        //        return strBin;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static byte[] Partition(int x, int size)
        //{
        //    try
        //    {
        //        string HexNum = HexToDecimal(x, 16);
        //        byte[] buffer = new byte[size];
        //        byte[] Returnbuffer = new byte[4];
        //        int buffIndex = 0;

        //        for (int i = HexNum.Length; i > 0; i -= 2)
        //        {
        //            if (i == 1)
        //                Returnbuffer[buffIndex] = Convert.ToByte(HexNum.Substring(0, 1), 16);
        //            else
        //                Returnbuffer[buffIndex] = Convert.ToByte(HexNum.Substring(i - 2, 2), 16);

        //            buffIndex += 1;
        //        }

        //        for (int j = 0; j < size; j++)
        //        {
        //            if (Returnbuffer.Length > j)
        //                buffer[j] = Returnbuffer[j];
        //        }

        //        return buffer;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static byte[] Partition(string s, int size)
        //{
        //    try
        //    {
        //        Int64 unit = Int64.Parse(s);
        //        byte[] buffer = new byte[size];
        //        buffer = BitConverter.GetBytes(unit);
        //        return buffer;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static byte[] GetBytesofMeterID(string MeterID)
        //{
        //    try
        //    {
        //        byte[] BMeterID = new byte[4];
        //        BMeterID = BitConverter.GetBytes(int.Parse(MeterID));
        //        return BMeterID;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static bool SaveCOMMSettings(string comport, int rate, int databits, string parity, string stopbits, string flow, int configurationType, int ReaderType)
        //{
        //    try
        //    {
        //        CommSettings commObj = new CommSettings();
        //        commObj.mReaderType = (ReaderType == 5 ? 0 : ReaderType);

        //        if (ReaderType == 5)
        //        {
        //            commObj.mrfidwmeterport = comport;
        //            commObj.mcomport = GetCOMMSettings(1).mcomport.ToString();
        //        }
        //        else
        //        {
        //            CommSettings cs = GetCOMMSettings(1);
        //            string mrfidwmeterport = "";

        //            if (cs.mrfidwmeterport != null)
        //                mrfidwmeterport = cs.mrfidwmeterport.ToString().Trim();

        //            commObj.mrfidwmeterport = mrfidwmeterport;
        //            commObj.mcomport = comport;
        //        }

        //        commObj.mrate = rate;
        //        commObj.mdatabits = databits;
        //        commObj.mparity = parity;
        //        commObj.mstopbits = stopbits;
        //        commObj.mflow = flow;
        //        IFormatter formatter = new BinaryFormatter();
        //        Stream stream;

        //        if (configurationType == (int)ConfigurationType.Optical)
        //        {
        //            stream = new FileStream(Application.StartupPath + "..\\CMMSetOPT.bin", FileMode.Create, FileAccess.Write, FileShare.None);
        //        }
        //        else
        //        {
        //            stream = new FileStream(Application.StartupPath + "..\\CMMSetGSM.bin", FileMode.Create, FileAccess.Write, FileShare.None);
        //        }

        //        formatter.Serialize(stream, commObj);
        //        stream.Close();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static CommSettings GetCOMMSettings(int configurationType)
        //{
        //    Stream stream = null;

        //    try
        //    {
        //        IFormatter formatter = new BinaryFormatter();

        //        if (configurationType == (int)ConfigurationType.Optical)
        //        {
        //            stream = new FileStream(Application.StartupPath + "\\CMMSetOPT.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
        //        }
        //        else
        //        {
        //            stream = new FileStream(Application.StartupPath + "\\CMMSetGSM.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
        //        }

        //        CommSettings commobj = (CommSettings)formatter.Deserialize(stream);
        //        stream.Close();
        //        return commobj;
        //    }
        //    catch
        //    {
        //        if (stream != null)
        //            stream.Close();

        //        return null;
        //    }
        //}

        //public static bool InitCOMMSettings(object portControl, int ConfigurationType)
        //{
        //    try
        //    {
        //        CommSettings cmmSettings = GetCOMMSettings(ConfigurationType);

        //        if (cmmSettings != null)
        //        {
        //            if (ConfigurationType != 2)
        //            {
        //                if (((SerialPort)portControl).PortName != cmmSettings.mcomport.Trim())
        //                {
        //                    ((SerialPort)portControl).PortName = cmmSettings.mcomport.Trim();
        //                    ((SerialPort)portControl).BaudRate = cmmSettings.mrate;
        //                    ((SerialPort)portControl).DataBits = cmmSettings.mdatabits;
        //                    ((SerialPort)portControl).Parity = Parity.Even;
        //                    ((SerialPort)portControl).StopBits = StopBits.One;
        //                    ((SerialPort)portControl).Handshake = Handshake.None;
        //                    ((SerialPort)portControl).RtsEnable = true;
        //                    ((SerialPort)portControl).ReadTimeout = 5000;
        //                    ((SerialPort)portControl).DtrEnable = true;
        //                    ((SerialPort)portControl).Encoding = System.Text.ASCIIEncoding.ASCII;
        //                }
        //            }

        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool COMSettingsValid(object portControl, int ConfigurationType)
        //{
        //    try
        //    {
        //        if (!InitCOMMSettings(portControl, ConfigurationType))
        //        {
        //            if (Multilingual.Messages.Show(null, "Your COM Settings Corrupted or need to be initialized", "Question", (int)MessageBoxButtons.YesNo, (int)MessageBoxIcon.Question) == DialogResult.Yes)
        //            {
        //                if (new frmComSettings(ConfigurationType).ShowDialog() == DialogResult.OK) // 1 for Optical   or 2 for GSM
        //                {
        //                    InitCOMMSettings(portControl, ConfigurationType);
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //            return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool AddMessageToDataBase(System.DateTime SavingDate, string TeleNo, string Message, string MeterID, string ConcentratorID, string OperationName, int Status, string Result)
        //{
        //    try
        //    {
        //        Result = "In Queue";
        //        dboperation db = new dboperation();

        //        if (ConcentratorID != "")
        //            MeterID = GetMeterID(MeterID, ConcentratorID);

        //        string sqlQuery = "INSERT INTO GSMMessages (Message, ToTelephone,meterID,OperationName,SavingDate, Status,Result,UserID) VALUES ";
        //        sqlQuery += "('" + Message + "','" + TeleNo + "','" + MeterID + "','" + OperationName + "',Convert(DateTime,'" + SavingDate + "',103)," + Status + ",'" + Result + "','" + frmMain.UserID + "')";
        //        sqlQuery += "; select @@identity from GSMMEssages ";
        //        object ob = db.GetSingleValue(sqlQuery);
        //        return Auditing(TeleNo, MeterID, OperationName, Status, Result, sqlQuery, ob, "GSMMessages", SavingDate);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool AddMessageToDataBase(System.DateTime SavingDate, string TeleNo, string Message, string MeterID, string OperationName, int Status)
        //{
        //    try
        //    {
        //        string Result = "In Queue";
        //        dboperation db = new dboperation();
        //        string sqlQuery = "INSERT INTO GSMMessages (Message, ToTelephone,meterID,OperationName,SavingDate, Status,Result,UserID) VALUES ";
        //        sqlQuery += "('" + Message + "','" + TeleNo + "','" + MeterID + "','" + OperationName + "',Convert(DateTime,'" + SavingDate + "',103)," + Status + ",'" + Result + "','" + frmMain.UserID + "')";
        //        sqlQuery += "; select @@identity from GSMMEssages ";
        //        object ob = db.GetSingleValue(sqlQuery);
        //        return Auditing(TeleNo, MeterID, OperationName, Status, Result, sqlQuery, ob, "GSMMessages", SavingDate);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool Auditing(string TeleNo, string MeterID, string OperationName, int Status, string Result, string sqlQuery, object ob, string TableName)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        System.DateTime dtime = cashedServerDateTime;
        //        int TransactionID = 12;

        //        string desc = "New AMR Operation ...  Operation Details :";

        //        // optical
        //        if (Status == 10)
        //        {
        //            TransactionID = 10;
        //            desc = " Method = Optical , Operation Name =" + OperationName + " , Meter ID =" + MeterID + ",Saving Date =" + dtime.ToString("d", DateTimeFormatInfo.InvariantInfo) + " ,Status =" + Status + " ,Result=" + Result + ", Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //                " On Vending Station :" + frmMain.VendingStationName;
        //        }
        //        else
        //            desc = " Method = GSM , Operation Name =" + OperationName + ",Meter ID =" + MeterID + ",To Telephone =" + TeleNo + ",Saving Date =" + dtime.ToString("d", DateTimeFormatInfo.InvariantInfo) + " ,Status =1 " + ", Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //                " On Vending Station :" + frmMain.VendingStationName;

        //        MeterID = GetMeterID2(MeterID);
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "AddAuditingWMeter";
        //        db.objcmd.Parameters.AddWithValue("@id", ((ob == null) ? "0" : ob.ToString()));
        //        db.objcmd.Parameters.AddWithValue("@TableName", TableName);
        //        db.objcmd.Parameters.AddWithValue("@Query", sqlQuery);
        //        db.objcmd.Parameters.AddWithValue("@MeterID", MeterID);

        //        //  Auditing Section
        //        db.objcmd.Parameters.AddWithValue("@Description", Crypto.GetEncryptedValue(desc));
        //        db.objcmd.Parameters.AddWithValue("@TransactionID", TransactionID);
        //        db.objcmd.Parameters.AddWithValue("@UserID", frmMain.UserID);
        //        db.objcmd.Parameters.AddWithValue("@ComputerName", Environment.MachineName);

        //        if (db.ExecuteNonQuery("") > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool Auditing(string TeleNo, string MeterID, string OperationName, int Status, string Result, string sqlQuery, object ob, string TableName, System.DateTime dtimeNow)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        int TransactionID = 12;
        //        string desc = "New AMR Operation ...  Operation Details :";


        //        if (Status == 10)
        //        {
        //            // optical
        //            TransactionID = 10;
        //            desc = " Method = Optical , Operation Name =" + OperationName + " , Meter ID =" + MeterID + ",Saving Date =" + dtimeNow.ToString("d", DateTimeFormatInfo.InvariantInfo) + " ,Status =" + Status + " ,Result=" + Result + ", Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //                " On Vending Station :" + frmMain.VendingStationName;
        //        }
        //        else
        //            desc = " Method = GSM , Operation Name =" + OperationName + ",Meter ID =" + MeterID + ",To Telephone =" + TeleNo + ",Saving Date =" + dtimeNow.ToString("d", DateTimeFormatInfo.InvariantInfo) + " ,Status =1 " + ", Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //                " On Vending Station :" + frmMain.VendingStationName;

        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "AddAuditingWMeter";
        //        db.objcmd.Parameters.AddWithValue("@id", ((ob == null) ? "0" : ob.ToString()));
        //        db.objcmd.Parameters.AddWithValue("@TableName", TableName);
        //        db.objcmd.Parameters.AddWithValue("@Query", sqlQuery);
        //        db.objcmd.Parameters.AddWithValue("@MeterID", MeterID);

        //        //  Auditing Section
        //        db.objcmd.Parameters.AddWithValue("@Description", Crypto.GetEncryptedValue(desc));
        //        db.objcmd.Parameters.AddWithValue("@TransactionID", TransactionID);
        //        db.objcmd.Parameters.AddWithValue("@UserID", frmMain.UserID);
        //        db.objcmd.Parameters.AddWithValue("@ComputerName", Environment.MachineName);

        //        if (db.ExecuteNonQuery("") > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool AuditingPublic(string MeterID, string OperationName, string sqlQuery, object ob, string TableName)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        System.DateTime dtime = cashedServerDateTime;
        //        int TransactionID = 12;

        //        string desc = "";

        //        TransactionID = 10;
        //        desc = "  Operation Name =" + OperationName + " , Meter ID =" + MeterID + ",Saving Date =" + dtime.ToString("d", DateTimeFormatInfo.InvariantInfo) + " , Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //            " On Vending Station :" + frmMain.VendingStationName;

        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "AddAuditingWMeter";
        //        db.objcmd.Parameters.AddWithValue("@id", ((ob == null) ? "0" : ob.ToString()));
        //        db.objcmd.Parameters.AddWithValue("@TableName", TableName);
        //        db.objcmd.Parameters.AddWithValue("@Query", sqlQuery);
        //        db.objcmd.Parameters.AddWithValue("@MeterID", MeterID);

        //        //  Auditing Section
        //        db.objcmd.Parameters.AddWithValue("@Description", Crypto.GetEncryptedValue(desc));
        //        db.objcmd.Parameters.AddWithValue("@TransactionID", TransactionID);
        //        db.objcmd.Parameters.AddWithValue("@UserID", frmMain.UserID);
        //        db.objcmd.Parameters.AddWithValue("@ComputerName", Environment.MachineName);

        //        if (db.ExecuteNonQuery("") > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool AuditingCard(string MeterID, string OperationName, string sqlQuery, object ob, string TableName)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        int TransactionID = 1;
        //        string desc = "New Smart Card Operation ...  Operation Details :";
        //        TransactionID = 10;
        //        desc = " Method = Smart Card , Operation Name =" + OperationName + " , Meter ID =" + MeterID + ",Creation Date =" + cashedServerDateTime.ToString("d", DateTimeFormatInfo.InvariantInfo) + " , Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //            " On Vending Station :" + frmMain.VendingStationName;

        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "AddAuditing";
        //        db.objcmd.Parameters.AddWithValue("@id", ((ob == null) ? "0" : ob.ToString()));
        //        db.objcmd.Parameters.AddWithValue("@TableName", TableName);
        //        db.objcmd.Parameters.AddWithValue("@Query", sqlQuery);

        //        //  Auditing Section
        //        db.objcmd.Parameters.AddWithValue("@Description", Crypto.GetEncryptedValue(desc));
        //        db.objcmd.Parameters.AddWithValue("@TransactionID", TransactionID);
        //        db.objcmd.Parameters.AddWithValue("@UserID", frmMain.UserID);
        //        db.objcmd.Parameters.AddWithValue("@ComputerName", Environment.MachineName);

        //        if (db.ExecuteNonQuery("") > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static bool AuditingData(string MeterID, string OperationName, string Desc)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string desc = Desc;
        //        desc += ", Operation Name =" + OperationName + " , Meter ID =" + MeterID + ",Version : " + frmMain.strVersion + ",Creation Date =" + cashedServerDateTime.ToString("d", DateTimeFormatInfo.InvariantInfo) + " , Added By :" + frmMain.UserName + " , From Computer : " + Environment.MachineName +
        //            " On Vending Station :" + frmMain.VendingStationName;

        //        if (db.ExecuteNonQuery("INSERT INTO Auditing " +
        //              " (TableName, TransactionID, TransactionDate, Description, TransactionTime, Query, RecordID, UserID, ComputerName,meterid) VALUES " +
        //              " ('" + OperationName + "', 1, getdate(), '" + Crypto.GetEncryptedValue(desc) + "', '', '', 0, '" + frmMain.UserID + "', '" + Environment.MachineName + "', '" + MeterID + "')") > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static string GetRegularExpression(string MaskFormat)
        //{
        //    try
        //    {
        //        string strReturnExpression = "";
        //        MaskFormat = MaskFormat.Replace("0", "[0-9]");
        //        strReturnExpression = MaskFormat.Replace("C", "[a-z]");
        //        return strReturnExpression;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static void SearchForDCU(ComboBox Combo1)
        //{
        //    try
        //    {
        //        new frmSearchForDCU().ShowDialog();
        //        if (frmSearchForDCU.ConcentratorId != "")
        //            Combo1.SelectedValue = frmSearchForDCU.ConcentratorId;
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static void OpenSerial(SerialPort serialPort1)
        //{
        //    try
        //    {
        //        if (!serialPort1.IsOpen)
        //            serialPort1.Open();
        //    }
        //    catch
        //    {
        //        serialPort1.Dispose();
        //    }
        //}

        //public static void CloseSerial(SerialPort serialPort1)
        //{
        //    try
        //    {
        //        if (serialPort1.IsOpen)
        //            serialPort1.Close();
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static bool WriteToSerial(byte[] s, SerialPort serialPort1)
        //{
        //    try
        //    {
        //        OpenSerial(serialPort1);

        //        if (serialPort1.IsOpen)
        //        {
        //            serialPort1.ReadExisting();
        //            serialPort1.Write(s, 0, s.Length);
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        CloseSerial(serialPort1);
        //        return false;
        //    }
        //}

        //public static byte[] ReadNext(int frameLength, SerialPort serialPort1)
        //{
        //    try
        //    {
        //        byte[] fframe = new byte[frameLength];
        //        int count = 0;
        //        int index = 0;

        //        while (count < 20)
        //        {
        //            try
        //            {
        //                int bytesRead = serialPort1.Read(fframe, index, frameLength);
        //                if (bytesRead <= frameLength)
        //                {
        //                    frameLength -= bytesRead;
        //                    index += bytesRead;

        //                    if (frameLength <= 0)
        //                        return fframe;
        //                }
        //            }
        //            catch
        //            {
        //                CloseSerial(serialPort1);
        //                return null;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //    }

        //    return new byte[0];
        //}

        ///// <summary>
        ///// Check existing operation
        ///// </summary>
        ///// <param name="MeterID">Meter identifier</param>
        ///// <param name="OperationName">Operation name</param>
        ///// <returns>result</returns>
        //public static bool CheckExistingOperation(string MeterID, string OperationName)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string sql = "Select ID from GSMMessages with(nolock) where meterID='" + MeterID + "'" +
        //                        " and OperationName='" + OperationName + "' and (Status=1 or Status=3)";

        //        if (db.ReturnInt(sql) == 0)
        //        {
        //            return false;
        //        }
        //        else
        //            return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "CheckExistingOperation", ex);
        //        return false;
        //    }
        //}

        //public static string getDate(System.DateTime date)
        //{
        //    try
        //    {
        //        return string.Format("{0:D2}/{1:D2}/{2:D4}", date.Month, date.Day, date.Year);
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        ///// <summary>
        ///// Get server date time
        ///// </summary>
        ///// <returns>Server date time</returns>
        //public static string getServerDateFormatted()
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        DataTable dt = db.SelectData("select getdate() as NowDate");
        //        cashedServerDateTime = Convert.ToDateTime(dt.Rows[0]["NowDate"]);
        //        cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", cashedServerDateTime.Day, cashedServerDateTime.Month, cashedServerDateTime.Year);
        //        return cashedServerDateTimeFormated;
        //    }
        //    catch
        //    {
        //        return "01/01/1900";
        //    }
        //}

        ///// <summary>
        ///// Get server date time
        ///// </summary>
        ///// <returns>Server date time</returns>
        //public static System.DateTime getServerDate()
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        DataTable dt = db.SelectData("select getdate() as NowDate");
        //        System.DateTime dtime;

        //        if (dboperation.OfflineMode)
        //        {
        //            dtime = (Convert.ToDateTime(dt.Rows[0]["NowDate"])).Add(frmMain.OfflineDifferance);
        //        }
        //        else
        //        {
        //            dtime = Convert.ToDateTime(dt.Rows[0]["NowDate"]);
        //        }

        //        cashedServerDateTime = dtime;
        //        cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", dtime.Day, dtime.Month, dtime.Year);
        //        return cashedServerDateTime;
        //    }
        //    catch
        //    {
        //        return System.DateTime.MinValue;
        //    }
        //}



        //public static void RenameButtonOperation(int SelectionOperation, DevComponents.DotNetBar.ButtonX btok)
        //{
        //    try
        //    {
        //        switch (SelectionOperation)
        //        {
        //            case 1://optical
        //                btok.Text = "Send optical";
        //                break;
        //            case 2://AMR
        //                btok.Text = "Send order";
        //                break;
        //            case 3://Smart card
        //                btok.Text = "Make card";
        //                break;
        //            case 4://RFID
        //                btok.Text = "Make card";
        //                break;
        //        }

        //        Multilingual.LanguageManipulations LM = new Multilingual.LanguageManipulations(Multilingual.LanguageManipulations.LanguageName, "");
        //        LM.SettingHeaders(btok);
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static void RenameButton_Operation(int SelectionOperation, DevComponents.DotNetBar.ButtonX btok)
        //{
        //    try
        //    {
        //        switch (SelectionOperation)
        //        {
        //            case 1:
        //                btok.Text = "Read Optical";
        //                break;
        //            case 2:
        //                btok.Text = "Send Order";
        //                break;
        //            case 3:
        //                btok.Text = "Read Card";
        //                break;
        //            case 4:
        //                btok.Text = "Make Card";
        //                break;
        //        }

        //        Multilingual.LanguageManipulations LM = new Multilingual.LanguageManipulations(Multilingual.LanguageManipulations.LanguageName, "");
        //        LM.SettingHeaders(btok);
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static string GetConcentratorMoblieNumber(string ConcentratorID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string SqlQuery = "SELECT MobileNumber FROM Concentrators with(nolock) where ID = '" + ConcentratorID + "'";
        //        string ConcentratorNum = db.ReturnStr(SqlQuery);
        //        return ConcentratorNum;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static string CreateID(string tb)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "createid";
        //        db.objcmd.Parameters.AddWithValue("@TableName", tb);
        //        SqlParameter parameter = db.objcmd.Parameters.Add("@CompositNumber", SqlDbType.VarChar, 50);
        //        parameter.Direction = ParameterDirection.Output;
        //        db.ExecuteNonQuery("");
        //        string outResult = parameter.Value.ToString();
        //        return outResult;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static string GetDBNo(string MeterID)
        //{
        //    try
        //    {
        //        string[] strArr = MeterID.Split(new char[] { '-' });
        //        return strArr[0].Substring(0, 4);
        //    }
        //    catch
        //    {
        //        return frmMain.DBNumber;
        //    }
        //}

        ///// <summary>
        ///// Get id splitting from id  that store witn db number 
        ///// </summary>
        ///// <param name="idWithDBNumber">Id with db number</param>
        ///// <returns>Id without db number</returns>
        //public static string GetIdWithoutDBNumber(string idWithDBNumber)
        //{
        //    try
        //    {
        //        string[] strArr = idWithDBNumber.Split(new char[] { '-' });
        //        return strArr.Length == 2 ? strArr[1] : strArr[0];
        //    }
        //    catch
        //    {
        //        return frmMain.DBNumber;
        //    }
        //}

        ///// <summary>
        ///// Get meter identifier by Display meter identifier
        ///// </summary>
        ///// <param name="DisPlayMeterID">Display meter identifier</param>
        ///// <returns>Meter identifier</returns>
        //public static string GetMeterID(string DisPlayMeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        var dt = db.SelectData("select MeterID, getdate() as currentdate from Meters with(nolock) where DisplayMeterID='" + DisPlayMeterID + "'");
        //        cashedServerDateTime = Convert.ToDateTime(dt.Rows[0]["currentdate"]);
        //        cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", cashedServerDateTime.Day, cashedServerDateTime.Month, cashedServerDateTime.Year);
        //        return dt.Rows[0]["MeterID"].ToString();
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        ///// <summary>
        ///// Get meter identifier by (Display meter , Concentrator) identifiers
        ///// </summary>
        ///// <param name="DisPlayMeterID">Display meter identifier</param>
        ///// <param name="ConcentratorID">Concentrator identifier</param>
        ///// <returns>Meter identifier</returns>
        //public static string GetMeterID(string DisPlayMeterID, string ConcentratorID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        var dt = db.SelectData("select MeterID , getdate() as currentdate from  Meters with(nolock) where DisplayMeterID='" + DisPlayMeterID + "' and ConcentratorID ='" + ConcentratorID + "'");
        //        cashedServerDateTime = Convert.ToDateTime(dt.Rows[0]["currentdate"]);
        //        cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", cashedServerDateTime.Day, cashedServerDateTime.Month, cashedServerDateTime.Year);
        //        return dt.Rows[0]["MeterID"].ToString();
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        ///// <summary>
        ///// Get meter identifier
        ///// </summary>
        ///// <param name="MeterNumber">Meter number</param>
        ///// <param name="meterType">Meter type</param>
        ///// <returns>Meter identifier</returns>
        //public static string GetMeterID2(string MeterNumber, string meterType = "0")
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(MeterNumber))
        //        {
        //            if (!string.IsNullOrEmpty(meterType))
        //            {
        //                MeterNumber = GetMeterNumberWithMeterType(MeterNumber, meterType);
        //            }

        //            dboperation db = new dboperation();
        //            string qry = "select MeterID, getdate() as currentdate from Meters with(nolock) where MeterNumber='" + MeterNumber + "'";
        //            var dt = db.SelectData(qry);
        //            cashedServerDateTime = Convert.ToDateTime(dt.Rows[0]["currentdate"]);
        //            cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", cashedServerDateTime.Day, cashedServerDateTime.Month, cashedServerDateTime.Year);
        //            return dt.Rows[0]["MeterID"].ToString();
        //        }
        //        else
        //            return MeterNumber;
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static string GetMeterNumberWithMeterType(string meterNumber, string meterType)
        //{
        //    try
        //    {
        //        return (Convert.ToInt32(meterType) <= (int)MeterTypeEnum.Water_EGRFID || meterType == ((int)MeterTypeEnum.Water_EGRFID_V8_1_25_Inch).ToString() || meterNumber.Contains("-")) ? meterNumber : (meterType + "-" + meterNumber);
        //    }
        //    catch
        //    {
        //        return meterType;
        //    }
        //}

        //public static string GetMeterNumberWithoutMeterType(string meterNumber)
        //{
        //    try
        //    {
        //        return meterNumber.Split('-')[1];
        //    }
        //    catch
        //    {
        //        return meterNumber;
        //    }
        //}

        //public static string RemoveDbPrefix(string entityNumber)
        //{
        //    try
        //    {
        //        return entityNumber.Split('-')[1];
        //    }
        //    catch
        //    {
        //        return entityNumber;
        //    }
        //}

        //public static string GetUserNumber(string UserID)
        //{
        //    try
        //    {
        //        if (UserID.Split('-').Length > 1)
        //            return UserID.Split('-')[1].ToString();
        //        else
        //            return UserID;
        //    }
        //    catch
        //    {
        //        return "0";
        //    }
        //}

        //public static string GetMeterNumber(string DisplayMeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.ReturnStr("select MeterNumber from  Meters with(nolock) where DisplayMeterID='" + DisplayMeterID + "'");
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static DataTable GetMeterCurrentActivity(string meterId)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.SelectData("select mt.ActivityID,mt.DepartmentID,mt.PhaseNo,mt.GuCode,MeterTypes.MeterModelVersionID from  Meters mt with(nolock) " +
        //            " INNER JOIN MeterTypes with(nolock) ON mt.MeterType = MeterTypes.ID " +
        //            " where MeterID='" + meterId + "'");
        //    }
        //    catch
        //    {
        //        return new DataTable();
        //    }
        //}

        ///// <summary>
        ///// Return Data of meter such  gucode , phaseNo ,Activity name
        ///// </summary>
        ///// <param name="meterNumber">number identifier of meter</param>
        ///// <returns>Data table cotaon meter info</returns>
        //public static DataTable GetMeterCurrentData(string meterNumber)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.SelectData(@"select am.Name as Meteractivity ,m.GuCode as meterGuCode,m.PhaseNo as MeterPhaseNo
        //                                from Meters m  with(nolock) inner
        //                                join Activities am with(nolock) on m.ActivityId = am.ID
        //                                where m.MeterNumber = '" + meterNumber + "'");
        //    }
        //    catch
        //    {
        //        return new DataTable();
        //    }
        //}

        ///// <summary>
        ///// Get name of activity
        ///// </summary>
        ///// <param name="id">Code of activity</param>
        ///// <returns>Name of activity</returns>
        //public static string GetActivityName(string code)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.ReturnStr("select Name  from Activities with(nolock) where AccountCode = '" + code + "'");
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}




        //public static string GetMeterNumber2(string MeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.ReturnStr("select MeterNumber from  Meters with(nolock) where MeterID='" + MeterID + "'");
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static string GetDisplayMeter(string MeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        return db.ReturnStr("select DisplayMeterID from  Meters with(nolock) where MeterID='" + MeterID + "'");
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static string GetMeterType(string MeterID)
        //{
        //    try
        //    {
        //        //dboperation db = new dboperation();
        //        //return db.ReturnStr("SELECT CASE MeterTypes.MeterType WHEN 'Single Phase' THEN '1' ELSE '3' END AS MeterType " +
        //        //" FROM Meters with(nolock) INNER JOIN MeterTypes with(nolock) ON Meters.MeterType = MeterTypes.ID WHERE (Meters.MeterID = '" + MeterID + "')");
        //        return "3";
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        //public static int GetMeterTypeModel(string MeterID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string str = "SELECT MeterTypes.MeterModelVersionID FROM Meters with(nolock) INNER JOIN MeterTypes with(nolock) ON Meters.MeterType = MeterTypes.ID WHERE (Meters.MeterID = '" + MeterID + "')";
        //        int intMeterType = db.ReturnInt(str);
        //        return intMeterType;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //public static DataTable GetMeterInfo(string MeterID, string RecieptNo)
        //{
        //    dboperation db = new dboperation();
        //    string str = "SELECT Meters.CardID ,MeterTypes.MeterModelVersionID,Meters.MeterNumber,PhaseNo , GuCode, ISNULL((Select top 1 ID from ReplacmentCards with(nolock) WHERE RecieptNo = '" + RecieptNo + "'),'0') as RecieptID FROM Meters with(nolock) INNER JOIN MeterTypes with(nolock) ON Meters.MeterType = MeterTypes.ID WHERE (Meters.MeterID = '" + MeterID + "')";
        //    return db.SelectData(str);
        //}

        //public static void ConvertBinary(byte rec, char[] binary)
        //{
        //    try
        //    {
        //        int dec = Convert.ToUInt16(rec);
        //        int i = 0;

        //        do
        //        {
        //            binary[i] = Convert.ToChar(dec % 2);
        //            i++;
        //            dec = dec - (dec % 2);
        //            dec = dec / 2;
        //        } while (dec > 0);

        //        for (int j = i; j < binary.Length; j++)
        //        {
        //            binary[j] = Convert.ToChar(0);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("ConvertBinary", "ConvertBinary", ex);
        //    }
        //}

        //public static byte[] GeneratPasswordForOptical(string StrMeterId)
        //{
        //    try
        //    {
        //        int i = 1, j = 0;
        //        byte[] Password = new byte[9];
        //        Password[0] = Convert.ToByte("33", 16);
        //        byte[] MeterID = new byte[4];
        //        MeterID = BitConverter.GetBytes(int.Parse(StrMeterId));
        //        byte[] key = new byte[4];
        //        key[0] = Convert.ToByte("44", 16);
        //        key[1] = Convert.ToByte("62", 16);
        //        key[2] = Convert.ToByte("E4", 16);
        //        key[3] = Convert.ToByte("BD", 16);

        //        while (i < 9)
        //        {
        //            Password[i] = Convert.ToByte(Convert.ToInt32(MeterID[j]) ^ Convert.ToInt32(key[j]));
        //            i++;
        //            Password[i] = MeterID[j];
        //            i++;
        //            j++;
        //        }

        //        Password[1] += Password[5];
        //        Password[2] += Password[6];
        //        Password[5] += Password[1];
        //        Password[6] += Password[2];
        //        return Password;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static int ConvertBin2Dec(string binar)
        //{
        //    try
        //    {
        //        int length = binar.Length;
        //        int retDec = 0, setval = 0;
        //        int count = 0;
        //        string currdigit = "";

        //        do
        //        {
        //            currdigit = binar.Substring(count, 1);

        //            if (currdigit == "0")
        //            {
        //                setval = 0;
        //            }
        //            else if (currdigit == "1")
        //            {
        //                setval = 1;
        //            }

        //            retDec = retDec + (int)((Math.Pow(2, (length - count - 1))) * setval);
        //            count++;

        //        } while (count < length);

        //        return retDec;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //public static void GetMeterOperations(ComboBox cbo, bool UseSmartCard, bool UseAMR)
        //{
        //    try
        //    {
        //        DataTable tbl = new DataTable();
        //        tbl.Columns.Add("Name");
        //        tbl.Columns.Add("ID");

        //        DataRow r3 = tbl.NewRow();
        //        r3["Name"] = "Smart Card";
        //        r3["ID"] = "3";

        //        DataRow r1 = tbl.NewRow();
        //        r1["Name"] = "Optical";
        //        r1["ID"] = "1";

        //        DataRow r2 = tbl.NewRow();
        //        r2["Name"] = "AMR";
        //        r2["ID"] = "2";

        //        DataRow r4 = tbl.NewRow();
        //        r4["Name"] = "RFID";
        //        r4["ID"] = "4";

        //        DataRow r5 = tbl.NewRow();
        //        r5["Name"] = "Auto";
        //        r5["ID"] = "5";

        //        if (frmLogIn.OperationOption[0] == '1')
        //        {
        //            tbl.Rows.Add(r1);
        //        }

        //        if (frmLogIn.OperationOption[1] == '1')
        //        {
        //            if (UseAMR)
        //                tbl.Rows.Add(r2);
        //        }

        //        if (frmLogIn.OperationOption[2] == '1')
        //        {
        //            if (UseSmartCard)
        //                tbl.Rows.Add(r3);
        //        }

        //        cbo.DataSource = tbl;
        //        cbo.DisplayMember = "Name";
        //        cbo.ValueMember = "ID";

        //        if (frmMain.DefaultMeterOperation != "")
        //            cbo.SelectedValue = frmMain.DefaultMeterOperation;

        //        if (!UseSmartCard)
        //            cbo.SelectedIndex = 0;
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static void GetMeterOperations(ComboBox cbo, bool UseSmartCard, bool UseAMR, bool UseOptical, bool UseRFID)
        //{
        //    try
        //    {
        //        DataTable tbl = new DataTable();
        //        tbl.Columns.Add("Name");
        //        tbl.Columns.Add("ID");

        //        DataRow r3 = tbl.NewRow();
        //        r3["Name"] = "Smart Card";
        //        r3["ID"] = "3";

        //        DataRow r1 = tbl.NewRow();
        //        r1["Name"] = "Optical";
        //        r1["ID"] = "1";

        //        DataRow r2 = tbl.NewRow();
        //        r2["Name"] = "AMR";
        //        r2["ID"] = "2";

        //        DataRow r4 = tbl.NewRow();
        //        r4["Name"] = "RFID";
        //        r4["ID"] = "4";

        //        DataRow r5 = tbl.NewRow();
        //        r5["Name"] = "Auto";
        //        r5["ID"] = "5";

        //        if (frmLogIn.OperationOption[3] == '1')
        //        {
        //            if (UseRFID)
        //                tbl.Rows.Add(r4);
        //        }

        //        if (frmLogIn.OperationOption[0] == '1')
        //        {
        //            if (UseOptical)
        //                tbl.Rows.Add(r1);
        //        }

        //        if (frmLogIn.OperationOption[1] == '1')
        //        {
        //            if (UseAMR)
        //                tbl.Rows.Add(r2);
        //        }

        //        if (frmLogIn.OperationOption[2] == '1')
        //        {
        //            if (UseSmartCard)
        //                tbl.Rows.Add(r3);
        //        }

        //        cbo.DataSource = tbl;
        //        cbo.DisplayMember = "Name";
        //        cbo.ValueMember = "ID";

        //        if (frmMain.DefaultMeterOperation != "")
        //            cbo.SelectedValue = frmMain.DefaultMeterOperation;
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static void SearchForMeters(CheckedListBox chBoxMeters, ComboBox cbBoxConcentrators, string metertypeId = "")
        //{
        //    try
        //    {
        //        frmSearchForMeter s = new frmSearchForMeter(false, true, false, metertypeId);
        //        s.ShowDialog();

        //        if (frmSearchForMeter.DisplayMeterID != "")
        //        {
        //            if (cbBoxConcentrators != null)
        //            {
        //                dboperation db = new dboperation();
        //                string sql = "select ConcentratorID from meters with(nolock) where ID='" + frmSearchForMeter.ID + "'";
        //                cbBoxConcentrators.SelectedValue = db.ReturnStr(sql);
        //            }

        //            chBoxMeters.SelectedItem = frmSearchForMeter.DisplayMeterID;
        //            chBoxMeters.SetItemChecked(chBoxMeters.SelectedIndex, true);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //public static DataTable GetMeterData(string MeterID)
        //{
        //    dboperation db = new dboperation();
        //    db.objcmd.CommandText = "GetMeterData";
        //    db.objcmd.CommandType = CommandType.StoredProcedure;
        //    db.objcmd.Parameters.Clear();
        //    db.objcmd.Parameters.AddWithValue("@MeterID", MeterID);
        //    DataTable dt = db.SelectData("");
        //    return dt;
        //}

        //public static string GenerateAccountNo(string CustomerName)
        //{
        //    try
        //    {
        //        dboperation dbobj = new dboperation();
        //        dbobj.objcmd.CommandType = CommandType.StoredProcedure;
        //        dbobj.objcmd.CommandText = "CreateAccountNumber";
        //        dbobj.objcmd.Parameters.AddWithValue("@CustomerName", CustomerName);
        //        DataTable result = dbobj.SelectData("");
        //        return result.Rows[0][0].ToString();
        //    }
        //    catch
        //    {
        //        return "00000000000";
        //    }
        //}

        //public static object ReadResourceValue(string key)
        //{
        //    object resourceValue = null;

        //    try
        //    {
        //        resourceValue = frmMainRes.ResourceManager.GetObject(key);
        //    }
        //    catch
        //    {
        //        resourceValue = null;
        //    }

        //    return resourceValue;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="dtSource"></param>
        ///// <param name="distinct"></param>
        ///// <param name="ViewedColumnNames"> the column names separated by commas</param>
        ///// <param name="FilterExpression"></param>
        ///// <param name="SortExpression"></param>
        ///// <returns> DataTable</returns>
        //public static DataTable FilterDataTable(DataTable dtSource, bool distinct, string ViewedColumnNames, string FilterExpression, string SortExpression)
        //{
        //    try
        //    {
        //        DataView view = dtSource.DefaultView;
        //        view.RowFilter = FilterExpression;
        //        view.Sort = SortExpression;

        //        if (distinct && ViewedColumnNames.Trim() != "")
        //            dtSource = view.ToTable(true, ViewedColumnNames.Split(','));
        //        else if (!distinct && ViewedColumnNames.Trim() != "")
        //            dtSource = view.ToTable(false, ViewedColumnNames.Split(','));
        //        else if (ViewedColumnNames.Trim() == "")
        //            return null;

        //        return dtSource;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        //public static void FillCheckedListBox(CheckedListBox checkedListBoxMeters, string ConcentratorID)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        string query = "select DisplayMeterID from Meters with(nolock) where ";
        //        string wh = "ID <> 0 ";
        //        wh += " And ConcentratorID='" + ConcentratorID + "'";
        //        wh += " and AMR=1";
        //        query += wh;
        //        db.FillCheckListBox(query, checkedListBoxMeters);
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("FillCheckedListBox", "FillCheckedListBox", ex);
        //    }
        //}

        //public static void GetMeterID(DevComponents.DotNetBar.ButtonX buttonOK, SerialPort serialPort1, ref string OperationType, Timer timer1, string Name)
        //{
        //    try
        //    {
        //        buttonOK.Enabled = false;

        //        if (COMSettingsValid(serialPort1, (int)ConfigurationType.Optical))
        //        {
        //            byte[] MeterIDParam = new byte[1];
        //            MeterIDParam[0] = Convert.ToByte("63", 16);

        //            if (WriteToSerial(MeterIDParam, serialPort1))
        //            {
        //                OperationType = "GetMeterID";
        //                timer1.Enabled = true;
        //            }
        //            else
        //            {
        //                Multilingual.Messages.Show("232");
        //                buttonOK.Enabled = true;
        //            }
        //        }
        //        else
        //        {
        //            Multilingual.Messages.Show("232");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog(Name, "GetMeterID", ex);
        //        Multilingual.Messages.Show("232");
        //    }
        //}

        ///// <summary>
        ///// Get OffTimes
        ///// </summary>
        ///// <returns>OffTimes object</returns>
        //public static OffTimes GetOffTimes()
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();

        //        var offtimes = new OffTimes();

        //        var Query = "SELECT [StartHour],[StartMin],[EndHour],[EndMin],[aChargeMode],[AllowChargeOff],[ExpireDays],[GracePeriod] FROM [Settings] with(nolock)";
        //        var SettingsRow = db.SelectData(Query).Rows[0];

        //        offtimes.CutOffTime = byte.Parse(SettingsRow["StartHour"].ToString());
        //        offtimes.GracePeriod = byte.Parse(SettingsRow["GracePeriod"].ToString());

        //        Query = "select Saturday , Sunday,Monday,Tuesday,Wednesday,Thursday,Friday from OddWeekends with(nolock)";
        //        var WeekendsRow = db.SelectData(Query).Rows[0];

        //        var WrkDays = ((Convert.ToBoolean(WeekendsRow["Saturday"]) ? "1" : "0") + (Convert.ToBoolean(WeekendsRow["Sunday"]) ? "1" : "0")
        //                        + (Convert.ToBoolean(WeekendsRow["Monday"]) ? "1" : "0") + (Convert.ToBoolean(WeekendsRow["Tuesday"]) ? "1" : "0")
        //                        + (Convert.ToBoolean(WeekendsRow["Wednesday"]) ? "1" : "0") + (Convert.ToBoolean(WeekendsRow["Thursday"]) ? "1" : "0")
        //                        + (Convert.ToBoolean(WeekendsRow["Friday"]) ? "1" : "0") + "0");
        //        offtimes.WrkDays = Convert.ToByte(WrkDays, 2);

        //        offtimes.HolidayDay = new byte[25];
        //        offtimes.HolidayMonth = new byte[25];
        //        Query = "select top 25  CONVERT(date ,  HolidayDate  ) as Holiday  from OddHolidays with(nolock) where HolidayDate >= GETDATE()";
        //        var HolidayDT = db.SelectData(Query);
        //        for (int indx = 0; indx < HolidayDT.Rows.Count; indx++)
        //        {
        //            var holidate = System.DateTime.Parse(HolidayDT.Rows[indx]["Holiday"].ToString());

        //            offtimes.HolidayDay[indx] = Convert.ToByte(holidate.Day);
        //            offtimes.HolidayMonth[indx] = Convert.ToByte(holidate.Month);
        //        }

        //        offtimes.WStartHr = byte.Parse(SettingsRow["StartHour"].ToString());
        //        offtimes.WEndHr = byte.Parse(SettingsRow["EndHour"].ToString());

        //        return offtimes;

        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("GetOffTimes", ex);
        //    }
        //}

        ///// <summary>
        ///// Set Client Card for new meters
        ///// </summary>
        ///// <param name="meterId">Client meter Id</param>
        ///// <param name="chargeNetValue">Charge Net Value</param>
        ///// <param name="cardSN">Card serial number</param>
        ///// <param name="chargeNum">Charge Num</param>
        ///// <param name="cardType">card Type</param>
        ///// <param name="rc">Card Reading</param>
        ///// <returns>result status</returns>
        //public static int SetClientCard(string meterId, string chargeNetValue, ref string cardSN, ref string chargeNum, ClientCardTypeEnum cardType, ushort? AreaNo, BasicTarrifModel specificTarrifModel)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        _sellingCardRepo = new SellingCardRepository();

        //        DataTable dt = db.SelectData("select m.MeterID, m.MeterNumber ,m.CustomerId, m.PhaseNo ,m.GuCode , m.ActivityID ,m.DepartmentID as DepartmentId, mt.MeterType , mt.ShapeType , mt.ManufactureYear,mt.MeterModelVersionID, a.dreditamt , a.AccountCode,a.alarmamt , (select top 1 ExpireDays from Settings with(nolock)) as expireDays from Meters as m with(nolock) join Activities as a with(nolock) on m.ActivityID = a.ID  inner join MeterTypes as mt with(nolock) on m.MeterType = mt.ID where m.MeterID = '" + meterId + "'");

        //        var haveSwage = Convert.ToByte(dt.Rows[0]["PhaseNo"].ToString() == "" ? "1" : dt.Rows[0]["PhaseNo"].ToString());
        //        var unitNo = Convert.ToUInt16(dt.Rows[0]["GuCode"].ToString() == "" ? "1" : dt.Rows[0]["GuCode"].ToString());
        //        var activityId = dt.Rows[0]["ActivityID"].ToString();
        //        var categoryNumber = Convert.ToByte(dt.Rows[0]["AccountCode"].ToString());

        //        if (cardType == ClientCardTypeEnum.ReplaceCard)
        //        {
        //            if (specificTarrifModel == null)
        //            {
        //                throw new Exception("SetClientCard : replace card with empty data");
        //            }

        //            haveSwage = Convert.ToByte(specificTarrifModel.sewage == "" ? "1" : specificTarrifModel.sewage);
        //            unitNo = Convert.ToUInt16(specificTarrifModel.unitNo == "" ? "1" : specificTarrifModel.unitNo);
        //            activityId = specificTarrifModel.activityId;
        //            categoryNumber = Convert.ToByte(specificTarrifModel.activityAccountCode);
        //        }
        //        else
        //        {

        //            string tariffStartDate = GetActivityCurrentTariffDate(activityId);
        //            specificTarrifModel = new BasicTarrifModel();
        //            specificTarrifModel.tarrifStartDate = System.DateTime.Parse(tariffStartDate);
        //            specificTarrifModel.departmentId = Convert.ToByte(GetIdWithoutDBNumber(dt.Rows[0]["DepartmentId"].ToString())).ToString();
        //            specificTarrifModel.activityAccountCode = categoryNumber.ToString();
        //            specificTarrifModel.activityId = activityId;
        //            specificTarrifModel.unitNo = unitNo.ToString();
        //            specificTarrifModel.sewage = haveSwage.ToString();
        //        }

        //        var meterTypeId = (MeterTypeEnum)Convert.ToInt32(dt.Rows[0]["MeterModelVersionID"].ToString());
        //        int meterStairsCount = 16;

        //        // Prepare card identifier and init 
        //        var expireDays = Convert.ToInt32(!string.IsNullOrEmpty(dt.Rows[0]["expireDays"].ToString()) ? dt.Rows[0]["expireDays"].ToString() : "0");
        //        var cardID = CardOperations.CreateCardID(CardFunEnum.CustomerCard, CardModeEnum.NewCard, expireDays, Convert.ToByte(meterTypeId).ToString(), (byte?)AreaNo);
        //        cardSN = cardID.CardNo.ToString();

        //        //Prepare MeterId
        //        var meterNumber = Convert.ToUInt32(GetMeterNumberWithoutMeterType(dt.Rows[0]["MeterNumber"].ToString()));
        //        var meterType = dt.Rows[0]["MeterType"].ToString();
        //        var meterDimantion = Convert.ToByte(meterType.Substring(meterType.Length - 2));
        //        var manufactureYear = dt.Rows[0]["ManufactureYear"].ToString();
        //        var meterOrigin = Convert.ToByte(manufactureYear.Substring(manufactureYear.Length - 2));
        //        var shapeType = dt.Rows[0]["ShapeType"].ToString();
        //        var clintCardMeterId = CardOperations.GetMeterID(meterNumber, meterDimantion, meterOrigin, shapeType);

        //        //Prepare ClientId
        //        var clientNumber = Convert.ToUInt32(GetIdWithoutDBNumber(dt.Rows[0]["CustomerId"].ToString()));
        //        var activityNumber = Convert.ToByte(specificTarrifModel.departmentId);//Convert.ToByte(GetIdWithoutDBNumber(dt.Rows[0]["DepartmentId"].ToString()));
        //        var clientId = CardOperations.GetClientID(clientNumber, activityNumber, categoryNumber, unitNo, haveSwage);

        //        //prepare PriceSched
        //        var meterTariff = GetNewMeterStairs(meterId, meterStairsCount, chargeNetValue, false, Convert.ToBoolean(haveSwage), specificTarrifModel, cardType);

        //        if (string.IsNullOrEmpty(chargeNum))
        //            chargeNum = (!string.IsNullOrEmpty(meterTariff.ChargeNum)) ? meterTariff.ChargeNum : "0";

        //        var priceSched = CardOperations.GetClientCardPriceSched(meterTariff);

        //        //prepare Deduction
        //        var deductionDT = _sellingCardRepo.GetSendToMeterAdjustment(meterId);
        //        var deduction = new Deductions();
        //        var isHaveDeductions = deductionDT.Rows.Count > 0;

        //        //Is have open deduction
        //        if (isHaveDeductions)
        //        {
        //            var deductionValue = Convert.ToDouble(Math.Ceiling(Convert.ToDouble(deductionDT.Rows[0]["DeductionValue"].ToString())));
        //            var monthsCount = int.Parse(deductionDT.Rows[0]["MonthsCount"].ToString()) - int.Parse(deductionDT.Rows[0]["PaidMonths"].ToString());
        //            var deductionDate = Convert.ToDateTime(deductionDT.Rows[0]["DueDate"].ToString()).AddMinutes(1);
        //            var maxdeductionDate = deductionDate.AddMonths(monthsCount);
        //            deduction = CardOperations.GetClientCardDeduction(deductionValue, (monthsCount < 0 ? (byte)0 : Convert.ToByte(monthsCount)), deductionDate);
        //        }
        //        else
        //        {
        //            isHaveDeductions = true;

        //            //if current date is December should start in next year
        //            var deductionddate = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
        //            deductionddate.AddMonths(1);
        //            deduction.monthFees = 0;
        //            deduction.month = 0;

        //            //is have deduction closed in current month
        //            var closedAdjustments = _sellingCardRepo.GetClosedDeduction(meterId);

        //            deduction.appDate = TemplatesProvider.Models.DateTime.Parse(deductionddate.ToString());

        //            if (closedAdjustments.Rows.Count > 0)
        //            {
        //                if (CalculateMonthCountDifference(System.DateTime.Parse(closedAdjustments.Rows[0]["DueDate"].ToString()), cashedServerDateTime) >= 1)
        //                {
        //                    deduction.appDate = TemplatesProvider.Models.DateTime.Parse(closedAdjustments.Rows[0]["DueDate"].ToString());
        //                }
        //            }
        //        }

        //        //Prepare OffTimes
        //        var offTimes = GetOffTimes();

        //        //Prepare CreditInfo
        //        var cutOffWarnLmt = Convert.ToDouble(!string.IsNullOrEmpty(dt.Rows[0]["alarmamt"].ToString()) ? dt.Rows[0]["alarmamt"].ToString() : "20");
        //        var creditInfo = CardOperations.GetClientCardCreditInfo(Convert.ToUInt32(chargeNum), Convert.ToDouble(!string.IsNullOrEmpty(chargeNetValue) ? chargeNetValue : "0"), Convert.ToByte(cutOffWarnLmt));

        //        //Prepare overDraftCrefit
        //        double overDraftCrefit = Convert.ToDouble(dt.Rows[0]["dreditamt"].ToString());

        //        //prepare meter Action
        //        var meterAction = CardOperations.GetClientCardMeterAction(isHaveDeductions, true, Convert.ToUInt32(chargeNum), cardType);

        //        // Set client card
        //        var result = CardOperations.SetClientCard(ref cardID, ref clintCardMeterId, ref clientId, ref meterAction, ref priceSched, ref deduction, ref offTimes, ref creditInfo, ref overDraftCrefit, Convert.ToByte(meterTypeId).ToString(), shapeType, cardType);

        //        // Write Vendor Identifier
        //        SmartWaterMeter.SmartWaterMeter.WriteVendorIdentifier(Convert.ToInt16((short)meterTypeId), shapeType, (frmMain.CompanyCode >= byte.MaxValue ? frmMain.AreaNo : frmMain.CompanyCode));

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.MakeExceptionLog("Utility", "SetClientCard", ex);
        //        return (int)ErrorCode.INVALIED_WRITE;
        //    }
        //}

        ///// <summary>
        ///// Check Cachier Balance if can add new chargies or not
        ///// </summary>
        ///// <param name="cachierId">Cachier Id</param>
        ///// <returns>Cachier Balance indicator if can add new chargies or not</returns>
        //public static bool NoCashierSettlements(string cachierId)
        //{
        //    try
        //    {
        //        if (frmMain.EnableCashierBalance == false)
        //            return true;

        //        // Check if today is off in cashier holidays
        //        bool isHolday = IsTodayIsCashierHoliday();

        //        if (!isHolday)
        //        {
        //            // Check if user must pay old amounts
        //            var dt = GetCachierBalance(cachierId);
        //            int DayCount = frmMain.SettlementDayCount == null ? 1 : frmMain.SettlementDayCount.Value;
        //            System.DateTime date = cashedServerDateTime.Date.AddDays(-1 * DayCount);

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (Convert.ToDateTime(row["day"].ToString()) <= date)
        //                    return false;
        //            }

        //            return true;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Check Cachier Balance if can add new chargies or not
        ///// </summary>
        ///// <param name="cachierId">Cachier Id</param>
        ///// <returns>Cachier Balance indicator if can add new chargies or not</returns>
        //public static DataTable GetCachierBalance(string cachierId)
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        db.objcmd.Parameters.Clear();
        //        db.objcmd.CommandType = CommandType.StoredProcedure;
        //        db.objcmd.CommandText = "GetCashierBalance";
        //        db.objcmd.Parameters.AddWithValue("@CashierID", cachierId);
        //        return db.SelectData("");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetCachierBalance", ex);
        //    }
        //}

        ///// <summary>
        ///// Generate Card No
        ///// </summary>
        ///// <returns>Generated Card No</returns>
        //public static uint GenerateCardNo()
        //{
        //    try
        //    {
        //        dboperation db = new dboperation();
        //        var cardNo = db.ReturnInt("exec  GenerateCardNo");
        //        return Convert.ToUInt32(cardNo);
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.MakeExceptionLog("Utility", "GenerateCardNo", ex);
        //        return 0;
        //    }
        //}

        ///// <summary>
        ///// Cancel transaction
        ///// </summary>
        ///// <param name="serialNo">serial No</param>
        ///// <returns>is Canceled or not</returns>
        //public static bool CancelTransaction(string serialNo)
        //{
        //    try
        //    {
        //        int MakeCard = 5;
        //        SqlTransaction tr = null;

        //        // Get cancelled charge
        //        dboperation db = new dboperation();
        //        string sql = "SELECT TOP 1 CH.ID,CH.SerialNo,CH.PaymentNumber, PaymentType, TotalValue, BanksID, ISNULL(CH.ChargeNo , 0 ) AS ChargeNo , CH.ChargeValue , Customers.CategoryID AS 'CustomerCategoryId', (SELECT RFDBNum FROM Settings WITH(NOLOCK)) AS DataBaseNumber , CH.MakeCard ,M.CardChargeNo as lastChargeNo,CH.MeterID FROM charges CH WITH(NOLOCK) INNER JOIN Meters M with(nolock) on CH.MeterID = M.MeterID INNER JOIN Customers ON Customers.ID = M.CustomerID" +
        //            " where (SerialNo is not null and SerialNo != '' AND SerialNo = '" + serialNo + "') order by ServerDate desc ";
        //        DataTable dt = db.SelectData(sql);

        //        sql = " select FeeID,monthno,Feetable,Feevalue from dbo.ChargesDetails with(nolock) where Feetable Like 'Adjustments' and SerialNu like '" + serialNo + "'";
        //        DataTable GridTbl = db.SelectData(sql);

        //        if (dt.Rows.Count > 0)
        //        {
        //            int LastMeterChargeNo = int.Parse(dt.Rows[0]["lastChargeNo"].ToString());
        //            int Id = int.Parse(dt.Rows[0]["ID"].ToString());
        //            int ChargeNo = int.Parse(dt.Rows[0]["ChargeNo"].ToString());
        //            string RecieptNo = dt.Rows[0]["SerialNo"].ToString();
        //            string PaymentNumber = dt.Rows[0]["PaymentNumber"].ToString();
        //            string ChargeValue = dt.Rows[0]["ChargeValue"].ToString();
        //            int DataBaseNumber = int.Parse(dt.Rows[0]["DataBaseNumber"].ToString());
        //            int Maked = int.Parse(dt.Rows[0]["MakeCard"].ToString());
        //            string meterId = dt.Rows[0]["MeterID"].ToString();
        //            string paymentType = dt.Rows[0]["PaymentType"].ToString();
        //            decimal totalValue = decimal.Parse(dt.Rows[0]["TotalValue"].ToString());
        //            string BankID = dt.Rows[0]["BanksID"].ToString();
        //            string customerCategoryId = dt.Rows[0]["CustomerCategoryId"].ToString();

        //            if (LastMeterChargeNo - ChargeNo > 1)
        //            {
        //                // Prevent cancel not last charge
        //                //IncorrectCancellationNotLastCharge;
        //                return false;
        //            }
        //            else if (Maked == 5)
        //            {
        //                // Check charge make card
        //                //TransactionCancelledBefore;
        //                return true;
        //            }
        //            else
        //            {
        //                // Cancel charge
        //                try
        //                {
        //                    // Open sql transaction
        //                    if (db.objcmd.Connection.State != ConnectionState.Open)
        //                        db.objcmd.Connection.Open();

        //                    tr = db.objcmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        //                    db.objcmd.Transaction = tr;

        //                    db.objcmd.CommandText = " update charges with(Rowlock) set MakeCard= " + MakeCard.ToString() + " , ChargeMethod = " + MakeCard.ToString() + " where Id = " + Id;
        //                    db.objcmd.ExecuteScalar();

        //                    db.objcmd.CommandText = " update meters with(Rowlock) set cardchargeno = " + ChargeNo + " where meterid = '" + meterId + "'";
        //                    db.objcmd.ExecuteScalar();

        //                    db.objcmd.CommandText = " update WaterMetersReadings with(Rowlock) set chargeid = null,SerialNu = null where chargeid = '" + Id + "' ";
        //                    db.objcmd.ExecuteScalar();

        //                    // Reverses adjustments and save fees details
        //                    for (int i = 0; i < GridTbl.Rows.Count; i++)
        //                    {
        //                        string FeeID = GridTbl.Rows[i]["FeeID"].ToString();
        //                        string monthno = GridTbl.Rows[i]["monthno"].ToString();

        //                        // Update adjustments
        //                        if (GridTbl.Rows[i]["Feetable"].ToString() == "Adjustments")
        //                        {
        //                            decimal Value = Convert.ToDecimal(GridTbl.Rows[i]["Feevalue"]);

        //                            if (Value < 0)
        //                                Value = Value * -1;

        //                            db.objcmd.CommandText = $@"
        //                                        UPDATE Adjustments WITH (ROWLOCK)
        //                                        SET
        //                                            DueDate = DATEADD(month, -CAST({monthno} AS int), DueDate),
        //                                            Remminder = Remminder + {Value},
        //                                            AdjustmentPaidValue = AdjustmentPaidValue - {Value},
        //                                            PaidMonths = CASE
        //                                                WHEN AdjustmentTypes.ValueOptions NOT IN ('Charge Percent', 'نسبة من الشحن')
        //                                                THEN PaidMonths - {monthno}
        //                                                ELSE PaidMonths
        //                                            END
        //                                        FROM Adjustments
        //                                        INNER JOIN AdjustmentTypes ON Adjustments.Type = AdjustmentTypes.ID
        //                                        WHERE Adjustments.ID = {FeeID}";
        //                            db.objcmd.ExecuteScalar();

        //                            db.objcmd.CommandText = "SELECT count(id) FROM adjustments with(nolock) WHERE Activead = 0 and id =" + FeeID;
        //                            string Remaining = db.objcmd.ExecuteScalar().ToString();

        //                            if (Convert.ToInt16(Remaining) > 0)
        //                            {
        //                                db.objcmd.CommandText = " UPDATE Adjustments with(Rowlock) SET Activead = 1 , IsDeleted = 0  WHERE (ID = " + FeeID + ")";
        //                                db.objcmd.ExecuteScalar();
        //                            }
        //                        }
        //                    }

        //                    // Audit cancellation
        //                    db.objcmd.CommandText = " insert into Auditing (TableName,TransactionID,TransactionDate,TransactionTime,Description,UserID,ComputerName,MeterID)" +
        //                          " values ('Charges',1,convert(nvarchar,getdate(),103),substring(convert(nvarchar,getdate(),100),13,7),'Cancel charge with receipt number (" + RecieptNo + ")','" + frmMain.UserID + "','" + Environment.MachineName + "','" + meterId + "')";
        //                    db.objcmd.ExecuteScalar();

        //                    if (paymentType == PaymentTypeEnum.Ecard.GetDescription())
        //                    {
        //                        if (string.IsNullOrEmpty(customerCategoryId))
        //                            db.objcmd.CommandText = $"DELETE FROM PaymentOrders WITH(ROWLOCK) WHERE PaymentOrderNumber = '{PaymentNumber}' AND BankID = '{BankID}'";
        //                        else
        //                            db.objcmd.CommandText = $"UPDATE PaymentOrders WITH(ROWLOCK) SET Reminder = Reminder + {totalValue}, ChargesCount = ChargesCount - 1 WHERE PaymentOrderNumber = '{PaymentNumber}' AND BankID = '{BankID}' AND OrganisationID = {customerCategoryId}";

        //                        db.objcmd.ExecuteNonQuery();
        //                    }
        //                    else if (paymentType == PaymentTypeEnum.Debit.GetDescription())
        //                    {
        //                        if (!string.IsNullOrWhiteSpace(BankID))
        //                        {
        //                            db.objcmd.CommandText = $"UPDATE Ministries WITH(ROWLOCK)  SET AvailableBalance = AvailableBalance + {totalValue} WHERE Id = {BankID}";
        //                            int result = db.objcmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                    else if (paymentType == PaymentTypeEnum.ChargeWithDebit.GetDescription())
        //                    {
        //                        db.objcmd.CommandText = $"UPDATE Adjustments WITH(ROWLOCK)  SET IsDeleted = 1, ActiveAd = 0 WHERE Id = {PaymentNumber}";
        //                        int result = db.objcmd.ExecuteNonQuery();
        //                    }
        //                    tr.Commit();
        //                    return true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    MakeExceptionLog("Utility", "CancelTransaction", ex);
        //                    tr.Rollback();
        //                    return false;
        //                }
        //                finally
        //                {
        //                    if (db.objcmd.Connection.State == ConnectionState.Open)
        //                        db.objcmd.Connection.Close();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //NotFoundTransaction;
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //IncorrectCancellation;
        //        MakeExceptionLog("Utility", "CancelTransaction", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Calculate dates difference in months
        ///// </summary>
        ///// <param name="date1">First date</param>
        ///// <param name="date2">Secong date</param>
        ///// <returns>Month difference</returns>
        //public static int CalculateMonthCountDifference(System.DateTime date1, System.DateTime date2)
        //{
        //    try
        //    {
        //        return (date1.Year - date2.Year) * 12 + (date1.Month - date2.Month);
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "CalculateMonthCountDofference", ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        /////  Update adjustment in database  that meter deduct it 
        ///// </summary>
        ///// <param name="meterId">Id of meter</param>
        ///// <param name="lastTransactionDate">Last transaction date card</param>
        ///// <param name="deductionCount">Month count in card</param>
        //public static void UpdateAjustmentFromMeter(string meterId, System.DateTime lastTransactionDate, byte deductionCount)
        //{
        //    try
        //    {
        //        if (deductionCount > 0)
        //        {
        //            _sellingCardRepo = new SellingCardRepository();
        //            var adjustemntDT = _sellingCardRepo.GetSendToMeterAdjustment(meterId);

        //            if (adjustemntDT.Rows.Count > 0)
        //            {
        //                var adjustmentDueDate = Convert.ToDateTime(adjustemntDT.Rows[0]["DueDate"].ToString());

        //                if (lastTransactionDate >= adjustmentDueDate)
        //                {
        //                    var monthDif = CalculateMonthCountDifference(lastTransactionDate, adjustmentDueDate);
        //                    monthDif = (monthDif < 0) ? 0 : monthDif + 1;

        //                    if (monthDif > 0)
        //                    {
        //                        var remainingMonths = int.Parse(adjustemntDT.Rows[0]["MonthsCount"].ToString()) - int.Parse(adjustemntDT.Rows[0]["PaidMonths"].ToString());
        //                        monthDif = monthDif > remainingMonths ? remainingMonths : monthDif;

        //                        var monthRate = Convert.ToDecimal(adjustemntDT.Rows[0]["DeductionValue"].ToString());
        //                        var adjustmentId = adjustemntDT.Rows[0]["ID"].ToString();

        //                        for (int i = 0; i < monthDif; i++)
        //                        {
        //                            _sellingCardRepo.UpdateAdjustment(monthRate.ToString(), adjustmentId, adjustmentDueDate.AddMonths(i), true);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MakeExceptionLog("Utility", "UpdateAjustmentFromMeter", ex);
        //    }
        //}

        //#endregion
    }
}