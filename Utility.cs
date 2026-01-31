using System;
using System.Data;

namespace MonthReadingRecalculation
{
    static class Utility
    {
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
            catch
            {
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
                    ValveStatus = " حالة المحبس : مغلق  " + " ---- " + resopn;
                }
                else
                {
                    bValveStatus = false;
                    ValveStatus = "حالة المحبس : مفتوح ";
                }
            }
            else
            {
                meterstatebin = Convert.ToString(aMeterState, 2).PadLeft(8, '0').ToCharArray();

                if (meterstatebin[0] == '0')
                {
                    batresopn = " حالة البطارية : ضعيفة من فضلك قم بتغيرها فى أسرع وقت";
                }

                if (meterstatebin[1] == '0')
                {
                    bValveStatus = true;
                    ValveStatus = " حالة المحبس : مغلق  " + " ---- " + resopn;
                }
                else
                {
                    bValveStatus = false;
                    ValveStatus = "حالة المحبس : مفتوح ";
                }
            }
        }
    }
}