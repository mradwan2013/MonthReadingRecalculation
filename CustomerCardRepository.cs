namespace MonthReadingRecalculation
{
    using System;
    using System.Data;

    /// <summary>
    /// customer card repository
    /// </summary>
    public class CustomerCardRepository
    {
        dboperation db = new dboperation();

        /// <summary>
        /// Get catagories
        /// </summary>
        /// <returns>catagories</returns>
        public DataTable GetCatagories(bool isMultiCategories)
        {
            return db.SelectData("SELECT DISTINCT ID,Name FROM Activities with(nolock) WHERE IsDeleted = 0 " + (isMultiCategories ? "" : "and Id != '255'"));
        }

        /// <summary>
        /// Get catagory activities
        /// </summary>
        /// <param name="catagoryId">Catagory identifier</param>
        /// <returns>Catagory activities</returns>
        public DataTable GetCatagoryActivities(string catagoryId)
        {
            return db.SelectData("SELECT DISTINCT ID,Name FROM departments with(nolock) WHERE ActivityID = '" + catagoryId + "' and IsDeleted = 0");
        }

        /// <summary>
        /// Get district concentrators
        /// </summary>
        /// <param name="districtId">District identifier</param>
        /// <returns>District concentrators</returns>
        public DataTable GetDistrictConcentrators(string districtId)
        {
            var query = " Select CN.ID,CN.[Name] from Concentrators CN with(nolock) " +
                        " inner join Transformers TR with(nolock) on CN.TransformerID = TR.ID " +
                        " inner join district DS with(nolock) on TR.DistrictID = DS.ID " +
                        " Where DS.ID = '" + districtId + "' and DS.IsDeleted = 0 ";

            return db.SelectData(query);
        }

        /// <summary>
        /// Get meter units activites summary
        /// </summary>
        /// <param name="meterId"> Meter identifier</param>
        /// <returns>Units activites</returns>
        public DataTable GetMeterUnitsActivitesSummary(string meterId)
        {
            //var s = " Select [CatagoryId], sum(CAST([UnitNo] AS INT)) as [UnitNo] from[dbo].[MeterUnitsActivites] with(nolock)" +
            //        " Where [MeterId] = '" + meterId + "' group by [CatagoryId], Id order by Id desc";

            var s = "Select  ActivityID as CatagoryId ,GuCode as UnitNo from meters with(nolock) where meterID = '" + meterId + "'";
            return db.SelectData(s);
        }

        /// <summary>
        /// Get meter first activity
        /// </summary>
        /// <param name="meterId"> Meter identifier</param>
        /// <returns>Activity</returns>
        public string GetMeterFirstActivity(string meterId)
        {
            // var query = "(Select top 1 [CatagoryId] from[dbo].[MeterUnitsActivites] with(nolock) Where [MeterId] = '" + meterId + "')";
            var query = "(Select ActivityID as CatagoryId from Meters with(nolock) Where MeterId = '" + meterId + "')";
            return db.ReturnStr(query);
        }

        /// <summary>
        /// Get meter model version
        /// </summary>
        /// <param name="meterId"> Meter identifier</param>
        /// <returns>MeterModelVersionID</returns>
        public int GetMeterModelVersion(string meterId)
        {
            int MeterModelVersionID = 0;
            db.objcmd.CommandText = "GetMeterData";
            db.objcmd.CommandType = CommandType.StoredProcedure;
            db.objcmd.Parameters.AddWithValue("@MeterID", meterId);
            DataTable dt = db.SelectData("");

            foreach (DataRow dr in dt.Rows)
            {
                MeterModelVersionID = Convert.ToInt16(dr["MeterModelVersionID"].ToString());
            }

            return MeterModelVersionID;
        }

        /// <summary>
        /// Get meter units activites
        /// </summary>
        /// <param name="meterId"> Meter identifier</param>
        /// <returns>Units activites</returns>
        public DataTable GetMeterUnitsActivites(string meterId)
        {
            //var query = " Select A.Name Catagory, D.Name Activity, MUA.UnitNo,MUA.CatagoryId,MUA.ActivityId from[dbo].[MeterUnitsActivites]" +
            //        " MUA with(nolock) inner join[dbo].[Activities] A with(nolock) on MUA.CatagoryId = A.ID inner join[dbo].[departments] D with(nolock) on MUA.ActivityId = D.ID " +
            //        " Where[MeterId] = '" + meterId + "'";

            var query = "Select A.Name Catagory, D.Name Activity, mt.GuCode,mt.ActivityID as CatagoryId,mt.DepartmentID  as ActivityId from meters mt with(nolock) inner join [dbo].[Activities] A with(nolock) on mt.ActivityID = A.ID inner join [dbo].[departments] D with(nolock) on mt.DepartmentID = D.ID where meterId = '" + meterId + "'";
            return db.SelectData(query);
        }
    }
}