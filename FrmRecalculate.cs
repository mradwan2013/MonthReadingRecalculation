using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonthReadingRecalculation
{
    public partial class FrmRecalculate : Form
    {
        public string connectionString = "";
        private BackgroundWorker worker;

        public FrmRecalculate()
        {
            InitializeComponent();
        }

        delegate void SetTextCallback(string text, bool append = false);

        private void SetRecalculateResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.RecalculateResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetRecalculateResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.RecalculateResultTxt.AppendText(text);
                }
                else
                {
                    this.RecalculateResultTxt.Text = text;
                }
            }
        }

        #region Recalculate month readings

        private void RecalculateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar1.Value = 0;
                progressBar1.Step = 1;
                progressBar1.Maximum = 0;
                RecalculateResultTxt.Text = "";
                SetRecalculateResultTxt("");
                label9.Text = "";
                StringBuilder Con = new StringBuilder("Password=" + textBox4.Text);
                Con.Append(";Persist Security Info=True;User ID=" + textBox3.Text);
                Con.Append(";Initial Catalog=" + textBox2.Text);
                Con.Append(";Data Source=" + textBox1.Text + ";");
                this.connectionString = Con.ToString();

                RecalculateMonthReadingThreading(richTextBox1.Text, checkBox1.Checked);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Recalculate water meter month readings based on table query  using threads
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void RecalculateMonthReadingThreading(string MonthReadingQuery, bool IncludeEstidama = false)
        {
            DataTable monthReadingList = GetMonthReadingNeedCalculation(MonthReadingQuery);

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {
                label9.Text = monthReadingList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                //Thread _thread = null;
                worker.WorkerReportsProgress = true;
                progressBar1.Value = 0;
                progressBar1.Step = 1;
                progressBar1.Maximum = monthReadingList.Rows.Count;
                label9.Text = string.Format("{0} records Completed", progressBar1.Value);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            // Log data before 
                            SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                            SetRecalculateResultTxt("Start recalculate ID:" + monthReadingList.Rows[t]["ID"].ToString() +
                                                    "- MeterID:" + monthReadingList.Rows[t]["MeterID"].ToString() +
                                                    "- ActivityID:" + monthReadingList.Rows[t]["ActivityID"].ToString() +
                                                    "- Year:" + monthReadingList.Rows[t]["Year"].ToString() +
                                                    "- Month:" + monthReadingList.Rows[t]["Month"].ToString() +
                                                    "- Read:" + monthReadingList.Rows[t]["Read"].ToString() +
                                                    "- PhaseNo:" + monthReadingList.Rows[t]["PhaseNo"].ToString() +
                                                    "- GuCode:" + monthReadingList.Rows[t]["GuCode"].ToString() +
                                                    "- ConsumptionMoney:" + monthReadingList.Rows[t]["ConsumptionMoney"].ToString() +
                                                    "- CBMPrice:" + monthReadingList.Rows[t]["CBMPrice"].ToString() +
                                                    "- Healthy:" + monthReadingList.Rows[t]["Healthy"].ToString() +
                                                    "- ServiceBox:" + monthReadingList.Rows[t]["ServiceBox"].ToString() +
                                                    "- FixFee:" + monthReadingList.Rows[t]["FixFee"].ToString() +
                                                    "- MeterFixFee:" + monthReadingList.Rows[t]["MeterFixFee"].ToString() + System.Environment.NewLine
                                , true);

                            if (i < monthReadingList.Rows.Count)
                            {
                                try
                                {
                                    //_thread = new Thread(() => 
                                    RecalculateWaterMonthReadings(
                                     int.Parse(monthReadingList.Rows[i]["ID"].ToString()),
                                     monthReadingList.Rows[i]["MeterID"].ToString(),
                                     int.Parse(monthReadingList.Rows[i]["Year"].ToString()),
                                     int.Parse(monthReadingList.Rows[i]["Month"].ToString()),
                                     decimal.Parse(monthReadingList.Rows[i]["Read"].ToString()),
                                     monthReadingList.Rows[i]["ActivityID"].ToString(),
                                     int.Parse(monthReadingList.Rows[i]["PhaseNo"].ToString()),
                                     int.Parse(monthReadingList.Rows[i]["GuCode"].ToString()),
                                     decimal.Parse(monthReadingList.Rows[i]["meterFixFee"].ToString()),
                                     IncludeEstidama
                                     );
                                   // );

                                   // _thread.Start();
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                });

                // Handle progress change
                worker.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    progressBar1.PerformStep();
                    label9.Text = string.Format("{0} records Completed", progressBar1.Value);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                progressBar1.Value = 0;
                progressBar1.Step = 1;
                progressBar1.Maximum = 0;
            }
        }

        /// <summary>
        /// Recalculate water meter month readings based on table query  without threads
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void RecalculateMonthReading(string MonthReadingQuery, bool IncludeEstidama = false)
        {
            DataTable monthReadingList = GetMonthReadingNeedCalculation(MonthReadingQuery);

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {
                foreach (DataRow dr in monthReadingList.Rows)
                {
                    // Log data before 
                    RecalculateResultTxt.AppendText("--------------------------------------------" + System.Environment.NewLine);
                    RecalculateResultTxt.AppendText("Start recalculate ID:" + dr["ID"].ToString() +
                                            "- MeterID:" + dr["MeterID"].ToString() +
                                            "- ActivityID:" + dr["ActivityID"].ToString() +
                                            "- Year:" + dr["Year"].ToString() +
                                            "- Month:" + dr["Month"].ToString() +
                                            "- Read:" + dr["Read"].ToString() +
                                            "- PhaseNo:" + dr["PhaseNo"].ToString() +
                                            "- GuCode:" + dr["GuCode"].ToString() +
                                            "- ConsumptionMoney:" + dr["ConsumptionMoney"].ToString() +
                                            "- CBMPrice:" + dr["CBMPrice"].ToString() +
                                            "- Healthy:" + dr["Healthy"].ToString() +
                                            "- ServiceBox:" + dr["ServiceBox"].ToString() +
                                            "- FixFee:" + dr["FixFee"].ToString() +
                                            "- MeterFixFee:" + dr["MeterFixFee"].ToString() + System.Environment.NewLine);

                    RecalculateWaterMonthReadings(
                        int.Parse(dr["ID"].ToString()),
                        dr["MeterID"].ToString(),
                        int.Parse(dr["Year"].ToString()),
                        int.Parse(dr["Month"].ToString()),
                        decimal.Parse(dr["Read"].ToString()),
                        dr["ActivityID"].ToString(),
                        int.Parse(dr["PhaseNo"].ToString()),
                        int.Parse(dr["GuCode"].ToString()),
                        decimal.Parse(dr["meterFixFee"].ToString()),
                        IncludeEstidama);
                }
            }
        }

        /// <summary>
        /// Handle worker complete event 
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Finished!");
        }


        /// <summary>
        /// Recalculate water meter month readings
        /// </summary>
        /// <param name="MeterID">Meter identifier</param>
        /// <param name="Year">Reading year</param>
        /// <param name="Month">Reading month from meter</param>
        /// <param name="TotalReading">Total consumption Reading from meter</param>
        /// <param name="UsedMonthly">Used Consuption Money Monthly</param>
        /// <param name="FixFee">Fixed Fee from meter</param>
        /// <param name="ActivityID">ActivityID</param>
        /// <param name="sewage">sewage</param> 
        /// <param name="meterUnits">meterUnits</param> 
        /// <param name="MeterVersionType">MeterVersionType</param> 
        /// <returns>Add result</returns>
        public bool RecalculateWaterMonthReadings(int MonthReadingID, string MeterID, int Year, int Month, decimal TotalReading, string ActivityID, int sewage, int meterUnits, decimal MeterFixFee, bool IncludeEstidama = false)
        {
            try
            {
                decimal TotalPrice = 0;
                decimal ServiceBoxWithTax = 0;
                decimal WaterPrice = 0;
                decimal SewagePrice = 0;

                // Check meter change requests by date (any changes in: activity, department , gucode, phase no)
                var monthDate = new System.DateTime(Year, Month, 1);
                var meterDate = GetMeterChangesByDate(MeterID, monthDate);

                // get new activity instead of meter current activity
                if (meterDate != null && meterDate.Rows.Count > 0)
                {
                    ActivityID = meterDate.Rows[0]["ActivityId"].ToString();
                    meterUnits = int.Parse(meterDate.Rows[0]["GuCode"].ToString());
                    sewage = int.Parse(meterDate.Rows[0]["PhaseNo"].ToString());
                }

                // Get activity estidama
                decimal FixFee = GetMeterEstidamaByActivityID(MeterID, ActivityID, meterUnits, monthDate.AddMonths(1).AddDays(-1));

                // Get activity tariff
                var meterTarrifa = GetTariff(ActivityID, monthDate.AddMonths(1).AddDays(-1));

                // Get reading stair details
                var priceResult = calcTariffStairsDetails(TotalReading, meterTarrifa, meterUnits, sewage);

                for (int i = 0; i < priceResult.GetLength(0); i++)
                {
                    WaterPrice += priceResult[i, 1];
                    ServiceBoxWithTax += priceResult[i, 2];
                    SewagePrice += priceResult[i, 5];
                    TotalPrice = priceResult[i, 6] != 0 ? priceResult[i, 6] : TotalPrice;
                }

                if (!IncludeEstidama)
                {
                    FixFee = 0;
                    MeterFixFee = 0;
                }

                // Update month reading
                var res = UpdateDbMonthReadings(MonthReadingID, TotalPrice, WaterPrice, SewagePrice, ServiceBoxWithTax, FixFee, MeterFixFee);

                // Log data after
                if (res)
                {
                    SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                    SetRecalculateResultTxt(
                                        "End recalculate   ID:" + MonthReadingID +
                                        "- MeterID:" + MeterID +
                                        "- ActivityID:" + ActivityID +
                                        "- Year:" + Year +
                                        "- Month:" + Month +
                                        "- Read:" + TotalReading +
                                        "- PhaseNo:" + sewage +
                                        "- GuCode:" + meterUnits +
                                        "- ConsumptionMoney:" + TotalPrice.ToString("F3") +
                                        "- CBMPrice:" + WaterPrice.ToString("F3") +
                                        "- Healthy:" + SewagePrice.ToString("F3") +
                                        "- ServiceBox:" + ServiceBoxWithTax.ToString("F3") +
                                        "- FixFee:" + FixFee +
                                        "- MeterFixFee:" + MeterFixFee +
                                        System.Environment.NewLine, true);
                }
                else
                {
                    SetRecalculateResultTxt("fail to update ID:" + MonthReadingID + System.Environment.NewLine, true);
                }
            }
            catch
            {
                SetRecalculateResultTxt("fail to update ID:" + MonthReadingID + System.Environment.NewLine, true);
            }

            return true;
        }

        /// <summary>
        /// Get Month Reading Need Calculation
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        /// <returns>List of month readings need calculation</returns>
        public DataTable GetMonthReadingNeedCalculation(string MonthReadingQuery)
        {
            try
            {
                return new dboperation(connectionString).SelectData(MonthReadingQuery);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  Get meter change request in specific date
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <param name="SpecificDate">Specific date</param>
        public DataTable GetMeterChangesByDate(string meterID, System.DateTime SpecificDate)
        {
            try
            {
                string sql = " select top 1 ActivityId , DepartmentId , GuCode , PhaseNo from [dbo].[MeterChangeRequest] where [MeterId] = '" + meterID + "' and [IsApplied] = 1 " +
                             " and CONVERT(datetime, ApplyDate, 101) <= CONVERT(datetime, '" + SpecificDate.ToString("yyyy-MM-dd") + "' , 101 )" +
                             " order by ApplyDate desc";
                return new dboperation(connectionString).SelectData(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  Get meter details
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        public DataTable GetMeterDetails(string meterID)
        {
            try
            {
                string sql = " select top 1 ActivityID , GuCode , PhaseNo from [dbo].[Meters] where [MeterId] = '" + meterID + "'";
                return new dboperation(connectionString).SelectData(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  Get meter audit details
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <param name="SpecificDate">Specific date</param>
        /// <param name="fieldName">fieldName ex: ActivityID,PhaseNo,UnitNo</param>
        public string GetMeterAuditDetails(string meterID, System.DateTime SpecificDate, string fieldName)
        {
            try
            {
                string sql = " select top 1 fieldNewValue from meteraudit where meetrid = '" + meterID + "'" +
                             " and CONVERT(datetime, changeDate, 101)  < CONVERT(datetime, '" + SpecificDate.ToString("yyyy-MM-dd") + "' , 101 ) and fieldname = '" + fieldName + "' order by changeDate desc";
                return new dboperation(connectionString).ReturnStr(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  Get meter details
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <param name="SpecificDate">Specific date</param>
        public DataTable GetLastWaterMeterReadingDetails(string meterID, System.DateTime SpecificDate)
        {
            try
            {
                string sql = " select top 1 ActivityID , GuCode , Sewage from [dbo].[WaterMetersReadings] where [MeterId] = '" + meterID + "' and CONVERT(datetime, serverDate, 101)  < CONVERT(datetime, '" + SpecificDate.AddMonths(1).ToString("yyyy-MM-dd") + "' , 101 ) order by serverDate desc";
                return new dboperation(connectionString).SelectData(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  Get meter details
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <param name="SpecificDate">Specific date</param>
        public DataTable GetLatestWaterMeterReadingDetails(string meterID, System.DateTime SpecificDate)
        {
            try
            {
                string sql = " select top 1 ActivityID , GuCode , Sewage from [dbo].[WaterMetersReadings] where [MeterId] = '" + meterID + "' and CONVERT(datetime, serverDate, 101)  > CONVERT(datetime, '" + SpecificDate.AddMonths(1).ToString("yyyy-MM-dd") + "' , 101 ) order by serverDate";
                return new dboperation(connectionString).SelectData(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get last tarrif or last tarrif before specific date
        /// </summary>
        /// <param name="ActivityID">Activity identifier</param>
        /// <param name="tarrifDate">Specific tarrif date</param>
        /// <returns>Tarriff details</returns>
        public DataTable GetTariff(string ActivityID, System.DateTime? tarrifDate = null)
        {
            try
            {
                string sql = " SELECT ActivityID , StairID , StairTo , StairValue , Activities.alarmamt, Activities.dreditamt ,Activities.Name, InitialFees  ,SwgPercent,SwgPrice,PerMeterFees, (select top 1 tax from Settings) as tax , " +
                             " CustomersServiceFees,IsCumulative,IsNoOfUnitsIncludedInCalc  , convert( datetime ,  StartDate , 103 ) as StartDate , StepSwgPrice,IsStepSwgPrice, " +
                             " MinimumFee, MaximumFee, Activities.CurrencyRatio  ,  StairID AS [From],StairTo AS [To], StairValue AS Value,MonthFeesOptionId,MonthStepFees, " +
                             " Activities.Stair , Healthy , PerMeterFees as ServiceBox  , isnull( exceptionvalue , 0 ) as exceptionvalue  " +
                             " FROM TariffDetails  inner join Activities on Activities.ID  = TariffDetails.ActivityID   " +
                             " WHERE (TariffDetails.StartDate = (SELECT MAX(CONVERT(datetime, tt.startdate, 101)) " +
                             " FROM tariffdetails tt WHERE ";

                if (tarrifDate == null)
                {
                    // Get current tariff
                    sql += "GetDate() >= CONVERT(datetime, tt.startdate, 101) AND ActivityID ='" + ActivityID + "' ";
                }
                else
                {
                    // Get tariff after specific date
                    sql += "CONVERT(datetime, '" + tarrifDate.Value.ToString("yyyy-MM-dd") + "' , 101 )    >=  CONVERT(datetime, tt.startdate, 103) AND ActivityID ='" + ActivityID + "' ";
                }

                sql += " )) AND TariffDetails.ActivityID ='" + ActivityID + "'";
                return new dboperation(connectionString).SelectData(sql);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get meter fixed fees by activity identifier (Fixed Estidama)
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <param name="activityID">Activity ididentifierparam>
        /// <param name="UnitNo">Unit no</param>
        /// <returns>Meter fixed fee values</returns>
        public decimal GetMeterEstidamaByActivityID(string meterID, string activityID, int UnitNo, System.DateTime tarriffDate)
        {
            try
            {
                var query = "select [dbo].[GetActivityFixedFee] ('" + meterID + "','" + activityID + "','" + UnitNo + "','" + tarriffDate.ToString("yyyy-MM-dd") + "')"; // GetMeterFixedFee
                var fixedFee = Convert.ToDecimal(new dboperation(connectionString).ReturnStr(query));
                return fixedFee;
            }
            catch (Exception ex)
            {
                //MakeExceptionLog("Utility", "GetMeterFixedFees", getDateTime().ToShortTimeString(), ex);
                return 0;
            }
        }

        /// <summary>
        /// Calculate tariff stairs details
        /// </summary>
        /// <param name="Quantity">Quantity</param>
        /// <param name="dataTariff">Tarriff</param>
        /// <param name="unitno">Unit number</param>
        /// <param name="sewage">sewage</param>
        /// <returns>Stairs details</returns>
        public decimal[,] calcTariffStairsDetails(decimal Quantity, DataTable dataTariff, int unitno, int sewage)
        {
            bool IncludeUnitNo = false;
            decimal from = 0;
            decimal to = 0;
            decimal Price = 0;
            decimal Tax = 0;
            decimal ServiceBox = 0;
            decimal ServiceBoxWithTax = 0;
            bool IsCumulative = false;
            decimal SewagePrice = 0;
            decimal SewagePercentage = 0;
            decimal totalSewage = 0;

            decimal waterPrice = 0;
            decimal totalPrice = 0;
            decimal allQuantity = Quantity;
            decimal[,] stair = new decimal[dataTariff.Rows.Count, 7];

            for (int i = 0; i < dataTariff.Rows.Count; i++)
            {
                // 1- Prepare water price per units
                IncludeUnitNo = Convert.ToBoolean(dataTariff.Rows[i]["IsNoOfUnitsIncludedInCalc"].ToString());

                from = Convert.ToDecimal(dataTariff.Rows[i]["from"].ToString());
                to = Convert.ToDecimal(dataTariff.Rows[i]["to"].ToString());

                if (IncludeUnitNo)
                {
                    from = from * unitno;
                    to = to * unitno;
                }

                Price = Convert.ToDecimal(dataTariff.Rows[i]["value"].ToString());

                // 2- Prepare service box
                Tax = decimal.Parse(dataTariff.Rows[i]["tax"].ToString()) / 100;
                ServiceBox = decimal.Parse(dataTariff.Rows[i]["ServiceBox"].ToString()) + decimal.Parse(dataTariff.Rows[i]["CustomersServiceFees"].ToString());
                ServiceBoxWithTax = ServiceBox * (1 + Tax);

                // 3- Prepare sewage price
                if (sewage == 1)
                {
                    SewagePrice = Convert.ToDecimal(dataTariff.Rows[i]["SwgPrice"].ToString());
                    SewagePercentage = Convert.ToDecimal(dataTariff.Rows[i]["SwgPercent"].ToString());
                    totalSewage = SewagePrice > 0 ? (SewagePercentage * SewagePrice / 100) : (SewagePercentage * Price / 100);
                }
                else
                {
                    SewagePrice = 0;
                    SewagePercentage = 0;
                    totalSewage = 0;
                }

                // 4- Prepare stair price
                waterPrice = Price + ServiceBoxWithTax + totalSewage;

                // 5- Prepare cumulative
                IsCumulative = Convert.ToBoolean(dataTariff.Rows[i]["IsCumulative"].ToString());

                if (from == 0 && !IsCumulative)
                {
                    from = 0;
                    totalPrice = 0;
                    Quantity = allQuantity;

                    for (int j = 0; j < i; j++)
                    {
                        stair[j, 0] = 0; // Quantity الكمية
                        stair[j, 1] = 0; // Total water price إجمالى ثمن المياه
                        stair[j, 2] = 0; // Total service box with tax إجمالى ثمن الخدمات شامل الضريبة
                        stair[j, 3] = 0; // Total service box إجمالى ثمن الخدمات 
                        stair[j, 4] = 0; // Total tax إجمالى الضريبة
                        stair[j, 5] = 0; // Total sewage إجمالى ثمن الصرف
                        stair[j, 6] = 0; // Total price إجمالى الثمن الكلى
                    }
                }

                // Calculate stairs main prices
                if (Quantity > to - from)
                {
                    totalPrice += (to - from) * waterPrice;
                    Quantity = Quantity - (to - from);
                    stair[i, 0] = to - from;
                    stair[i, 1] = (to - from) * Price;
                    stair[i, 2] = (to - from) * (decimal)ServiceBoxWithTax;
                    stair[i, 3] = stair[i, 2] / (decimal)(1 + Tax);
                    stair[i, 4] = stair[i, 3] * (decimal)Tax;
                    stair[i, 5] = (to - from) * (decimal)totalSewage;
                    stair[i, 6] = totalPrice;
                }
                else
                {
                    totalPrice += Quantity * waterPrice;
                    stair[i, 0] = Quantity;
                    stair[i, 1] = Quantity * Price;
                    stair[i, 2] = Quantity * (decimal)ServiceBoxWithTax;
                    stair[i, 3] = stair[i, 2] / (decimal)(1 + Tax);
                    stair[i, 4] = stair[i, 3] * (decimal)Tax;
                    stair[i, 5] = Quantity * (decimal)totalSewage;
                    stair[i, 6] = totalPrice;
                    break;
                }
            }

            return stair;
        }

        /// <summary>
        /// Update month readings to db directly
        /// </summary>
        /// <param name="ID">Identifier</param>
        /// <param name="TotalPrice">Total price</param>
        /// <param name="WaterPrice">Water price</param> 
        /// <param name="SewagePrice">Sewage price</param>
        /// <param name="ServiceBox">Service box</param>
        /// <returns>Bool indicator saved or not</returns>
        public bool UpdateDbMonthReadings(int ID, decimal TotalPrice, decimal WaterPrice, decimal SewagePrice, decimal ServiceBox, decimal FixFee, decimal MeterFixFee)
        {
            try
            {
                dboperation db = new dboperation(connectionString);
                db.objcmd.Parameters.Clear();
                db.objcmd.CommandType = CommandType.StoredProcedure;
                db.objcmd.CommandText = "UpdateMonthReadings";
                db.objcmd.Parameters.AddWithValue("@ID", ID);                                   // Identifier
                db.objcmd.Parameters.AddWithValue("@UsedMonthly", (TotalPrice + FixFee));       // consumption money from meter 
                db.objcmd.Parameters.AddWithValue("@CBMPrice", WaterPrice);                     // Calculated water price
                db.objcmd.Parameters.AddWithValue("@Healthy", SewagePrice);                     // Calculated sewage price
                db.objcmd.Parameters.AddWithValue("@ServiceBox", ServiceBox);                   // Calculated service box
                db.objcmd.Parameters.AddWithValue("@FixFee", FixFee);                           // Calculated fix fee
                db.objcmd.Parameters.AddWithValue("@MeterFixFee", MeterFixFee);                 // Meter fix fee
                db.objcmd.Parameters.AddWithValue("@ConsumptionMoney", (TotalPrice + FixFee));  // Calculated money from system    
                SqlParameter sqlResult = new SqlParameter("@sqlResult", SqlDbType.Int, 1);
                sqlResult.Direction = ParameterDirection.Output;
                db.objcmd.Parameters.Add(sqlResult);
                db.ExecuteNonQuery("");

                if (sqlResult.Value != null && (int)sqlResult.Value == 1) // Add month reading
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Cancel charges

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder Con = new StringBuilder("Password=" + textBox4.Text);
            Con.Append(";Persist Security Info=True;User ID=" + textBox3.Text);
            Con.Append(";Initial Catalog=" + textBox2.Text);
            Con.Append(";Data Source=" + textBox1.Text + ";");
            this.connectionString = Con.ToString();

            CancelCharges(richTextBox3.Text);
        }

        /// <summary>
        /// Get Charges need cancellation
        /// </summary>
        /// <param name="ChargesQuery">Charges query</param>
        /// <returns>List of month readings need calculation</returns>
        public DataTable GetChargesNeedCancellation(string ChargesQuery)
        {
            try
            {
                return new dboperation(connectionString).SelectData(ChargesQuery);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Cancel charges based on table query
        /// </summary>
        /// <param name="ChargesQuery">Charges query</param>
        public void CancelCharges(string ChargesQuery)
        {
            DataTable chargesList = GetChargesNeedCancellation(ChargesQuery);

            if (chargesList != null && chargesList.Rows.Count > 0)
            {
                foreach (DataRow dr in chargesList.Rows)
                {
                    // Log data before 
                    richTextBox4.AppendText("--------------------------------------------" + System.Environment.NewLine);
                    richTextBox4.AppendText("Start cancel Charge serial number:" + dr["SerialNo"].ToString() + System.Environment.NewLine);

                    // Log data after
                    if (CancelCharge(dr["SerialNo"].ToString()))
                    {
                        richTextBox4.AppendText("End: Success to cancel Charge serial number:" + dr["SerialNo"].ToString() + System.Environment.NewLine);
                    }
                    else
                    {
                        richTextBox4.AppendText("End: Failed to cancel Charge serial number:" + dr["SerialNo"].ToString() + System.Environment.NewLine);
                    }
                }
            }
        }

        /// <summary>
        /// Cancel charge
        /// </summary>
        /// <param name="ChargeSerialNo">Charge serial number</param>
        /// <returns>Add result</returns>
        public bool CancelCharge(string ChargeSerialNo)
        {
            try
            {
                int MakeCard = 5;
                SqlTransaction tr = null;

                // Get cancelled charge
                dboperation db = new dboperation(connectionString);
                string sql = "select top 1 CH.ID,CH.SerialNo,CH.PaymentNumber, isnull(CH.ChargeNo , 0 ) as ChargeNo , CH.ChargeValue , (select RFDBNum from Settings) as DataBaseNumber , CH.MakeCard ,M.CardChargeNo as lastChargeNo,CH.MeterID from charges CH inner join Meters M on CH.MeterID = M.MeterID " +
                    " where (SerialNo is not null and SerialNo != '' and SerialNo = '" + ChargeSerialNo + "') order by ServerDate desc ";
                DataTable dt = db.SelectData(sql);

                sql = " select * from dbo.ChargesDetails where Feetable Like 'Adjustments' and SerialNu like '" + ChargeSerialNo + "'";
                DataTable GridTbl = db.SelectData(sql);

                if (dt.Rows.Count > 0)
                {
                    int LastMeterChargeNo = int.Parse(dt.Rows[0]["lastChargeNo"].ToString());
                    int Id = int.Parse(dt.Rows[0]["ID"].ToString());
                    int ChargeNo = int.Parse(dt.Rows[0]["ChargeNo"].ToString());
                    string RecieptNo = dt.Rows[0]["SerialNo"].ToString();
                    string PaymentNumber = dt.Rows[0]["PaymentNumber"].ToString();
                    string ChargeValue = dt.Rows[0]["ChargeValue"].ToString();
                    int DataBaseNumber = int.Parse(dt.Rows[0]["DataBaseNumber"].ToString());
                    int Maked = int.Parse(dt.Rows[0]["MakeCard"].ToString());
                    string meterId = dt.Rows[0]["MeterID"].ToString();

                    //if (LastMeterChargeNo - ChargeNo > 1)
                    //{
                    //    // Prevent cancel not last charge
                    //    //IncorrectCancellationNotLastCharge;
                    //    return false;
                    //}
                    if (Maked == 5)
                    {
                        // Check charge make card
                        //TransactionCancelledBefore;
                        return true;
                    }
                    else
                    {
                        // Cancel charge
                        try
                        {
                            // Open sql transaction
                            if (db.objcmd.Connection.State != ConnectionState.Open)
                                db.objcmd.Connection.Open();

                            tr = db.objcmd.Connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                            db.objcmd.Transaction = tr;

                            db.objcmd.CommandText = " update charges set MakeCard= " + MakeCard.ToString() + " , ChargeMethod = " + MakeCard.ToString() + " where Id = " + Id;
                            db.objcmd.ExecuteScalar();

                            db.objcmd.CommandText = " update meters set cardchargeno = " + ChargeNo + " where meterid = '" + meterId + "'";
                            db.objcmd.ExecuteScalar();

                            // Reverses adjustments and save fees details
                            for (int i = 0; i < GridTbl.Rows.Count; i++)
                            {
                                string FeeID = GridTbl.Rows[i]["FeeID"].ToString();
                                string monthno = GridTbl.Rows[i]["monthno"].ToString();

                                // Update adjustments
                                if (GridTbl.Rows[i]["Feetable"].ToString() == "Adjustments")
                                {
                                    decimal Value = Convert.ToDecimal(GridTbl.Rows[i]["Feevalue"]);

                                    if (Value < 0)
                                        Value = Value * -1;

                                    db.objcmd.CommandText = " UPDATE Adjustments SET DueDate = DATEADD(month, -CAST(" + monthno + " AS int), DueDate), Remminder = Remminder + " + Value.ToString() +
                                          ", PaidMonths = PaidMonths - " + monthno + " WHERE (ID = " + FeeID + ")";
                                    db.objcmd.ExecuteScalar();

                                    db.objcmd.CommandText = "SELECT count(*) FROM adjustments WHERE Activead = 0 and id =" + FeeID;
                                    string Remaining = db.objcmd.ExecuteScalar().ToString();

                                    if (Convert.ToInt16(Remaining) > 0)
                                    {
                                        db.objcmd.CommandText = " UPDATE Adjustments SET Activead = 1 WHERE (ID = " + FeeID + ")";
                                        db.objcmd.ExecuteScalar();
                                    }
                                }
                            }

                            // Audit cancellation
                            db.objcmd.CommandText = " insert into Auditing (TableName,TransactionID,TransactionDate,TransactionTime,Description,UserID,ComputerName,MeterID)" +
                                  " values ('Charges',1,convert(nvarchar,getdate(),103),substring(convert(nvarchar,getdate(),100),13,7),'Cancel charge with receipt number (" + RecieptNo + ")','45624-1','Manual','" + meterId + "')";
                            db.objcmd.ExecuteScalar();
                            tr.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            return false;
                        }
                        finally
                        {
                            if (db.objcmd.Connection.State == ConnectionState.Open)
                                db.objcmd.Connection.Close();
                        }
                    }
                }
                else
                {
                    //NotFoundTransaction;
                    return false;
                }
            }
            catch (Exception ex)
            {
                //IncorrectCancellation;
                return false;
            }
        }

        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            label10.Text = "0";
            richTextBox7.Text = "";
            StringBuilder Con = new StringBuilder("Password=" + textBox4.Text);
            Con.Append(";Persist Security Info=True;User ID=" + textBox3.Text);
            Con.Append(";Initial Catalog=" + textBox2.Text);
            Con.Append(";Data Source=" + textBox1.Text + ";");
            this.connectionString = Con.ToString();

            UpdateMonthReading(richTextBox6.Text);
        }

        /// <summary>
        /// Update water meter month readings based on table query
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void UpdateMonthReading(string MonthReadingQuery)
        {
            DataTable monthReadingList = GetMonthReadingNeedCalculation(MonthReadingQuery);
            int counter = 0;

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {

                foreach (DataRow dr in monthReadingList.Rows)
                {
                    counter++;
                    label10.Text = counter + "";

                    // Log data before 
                    richTextBox7.AppendText("--------------------------------------------" + System.Environment.NewLine);
                    richTextBox7.AppendText("Start recalculate ID:" + dr["ID"].ToString() +
                                            "- MeterID:" + dr["MeterID"].ToString() +
                                            "- ActivityID:" + dr["ActivityID"].ToString() +
                                            "- Year:" + dr["Year"].ToString() +
                                            "- Month:" + dr["Month"].ToString() +
                                            "- PhaseNo:" + dr["PhaseNo"].ToString() +
                                            "- GuCode:" + dr["GuCode"].ToString() + System.Environment.NewLine);

                    UpdateWaterMonthReadings(int.Parse(dr["ID"].ToString()),
                        dr["MeterID"].ToString(),
                        int.Parse(dr["Year"].ToString()),
                        int.Parse(dr["Month"].ToString()),
                        dr["ActivityID"].ToString(),
                        int.Parse(dr["PhaseNo"].ToString()),
                        int.Parse(dr["GuCode"].ToString()));
                }
            }
        }

        /// <summary>
        /// Update water meter month readings
        /// </summary>
        /// <param name="MeterID">Meter identifier</param>
        /// <param name="Year">Reading year</param>
        /// <param name="Month">Reading month from meter</param>
        /// <param name="TotalReading">Total consumption Reading from meter</param>
        /// <param name="UsedMonthly">Used Consuption Money Monthly</param>
        /// <param name="FixFee">Fixed Fee from meter</param>
        /// <param name="ActivityID">ActivityID</param>
        /// <param name="sewage">sewage</param> 
        /// <param name="meterUnits">meterUnits</param> 
        /// <param name="MeterVersionType">MeterVersionType</param> 
        /// <returns>Add result</returns>
        public bool UpdateWaterMonthReadings(int MonthReadingID, string MeterID, int Year, int Month, string ActivityID, int sewage, int meterUnits)
        {
            try
            {
                decimal TotalPrice = 0;
                decimal ServiceBoxWithTax = 0;
                decimal WaterPrice = 0;
                decimal SewagePrice = 0;

                // Check meter change requests by date (any changes in: activity, department , gucode, phase no)
                var monthDate = new System.DateTime(Year, Month, 1);
                var meterDate = GetMeterChangesByDate(MeterID, monthDate);

                try
                {
                    // get new activity instead of meter current activity
                    if (meterDate != null && meterDate.Rows.Count > 0)
                    {
                        ActivityID = meterDate.Rows[0]["ActivityId"].ToString();
                        meterUnits = int.Parse(meterDate.Rows[0]["GuCode"].ToString());
                        sewage = int.Parse(meterDate.Rows[0]["PhaseNo"].ToString());
                    }
                    else
                    {
                        // medter audit
                        var wMeterReading = GetLastWaterMeterReadingDetails(MeterID, monthDate);

                        if (wMeterReading != null && wMeterReading.Rows.Count > 0)
                        {
                            ActivityID = wMeterReading.Rows[0]["ActivityId"].ToString();
                            meterUnits = int.Parse(wMeterReading.Rows[0]["GuCode"].ToString());
                            sewage = int.Parse(wMeterReading.Rows[0]["Sewage"].ToString());
                        }
                        else
                        {
                            var wMeterReading_2 = GetLatestWaterMeterReadingDetails(MeterID, monthDate);

                            if (wMeterReading_2 != null && wMeterReading_2.Rows.Count > 0)
                            {
                                ActivityID = wMeterReading_2.Rows[0]["ActivityId"].ToString();
                                meterUnits = int.Parse(wMeterReading_2.Rows[0]["GuCode"].ToString());
                                sewage = int.Parse(wMeterReading_2.Rows[0]["Sewage"].ToString());
                            }
                            else
                            {
                                var mtrDetails = GetMeterDetails(MeterID);

                                if (mtrDetails != null && mtrDetails.Rows.Count > 0)
                                {
                                    ActivityID = mtrDetails.Rows[0]["ActivityId"].ToString();
                                    meterUnits = int.Parse(mtrDetails.Rows[0]["GuCode"].ToString());
                                    sewage = int.Parse(mtrDetails.Rows[0]["PhaseNo"].ToString());
                                }
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    // Log data after
                    richTextBox7.AppendText("Failed MeterID:" + MeterID + System.Environment.NewLine);
                }

                // Log data after
                richTextBox7.AppendText("End recalculate   ID:" + MonthReadingID +
                                        "- MeterID:" + MeterID +
                                        "- ActivityID:" + ActivityID +
                                        "- Year:" + Year +
                                        "- Month:" + Month +
                                        "- PhaseNo:" + sewage +
                                        "- GuCode:" + meterUnits + System.Environment.NewLine);

                // Update month reading
                UpdateDbMonthReadingDetails(MonthReadingID, sewage, meterUnits, ActivityID);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update month readings details
        /// </summary>
        /// <param name="ID">Identifier</param>
        /// <param name="PhaseNo">PhaseNo</param>
        /// <param name="GuCode">GuCode</param> 
        /// <param name="ActivityID">ActivityID</param>
        /// <returns>Bool indicator saved or not</returns>
        public bool UpdateDbMonthReadingDetails(int ID, int PhaseNo, int GuCode, string ActivityID)
        {
            try
            {
                dboperation db = new dboperation(connectionString);
                db.objcmd.Parameters.Clear();
                db.objcmd.CommandType = CommandType.StoredProcedure;
                db.objcmd.CommandText = "UpdateMonthReadingsDetails";
                db.objcmd.Parameters.AddWithValue("@ID", ID);
                db.objcmd.Parameters.AddWithValue("@PhaseNo", PhaseNo);
                db.objcmd.Parameters.AddWithValue("@GuCode", GuCode);
                db.objcmd.Parameters.AddWithValue("@ActivityID", ActivityID);
                SqlParameter sqlResult = new SqlParameter("@sqlResult", SqlDbType.Int, 1);
                sqlResult.Direction = ParameterDirection.Output;
                db.objcmd.Parameters.Add(sqlResult);
                db.ExecuteNonQuery("");

                if (sqlResult.Value != null && (int)sqlResult.Value == 2) // Add month reading
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
