using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace MonthReadingRecalculation
{
    public partial class FrmRecalculate : Form
    {
        public string connectionString = "";
        private BackgroundWorker worker;
        delegate void SetTextCallback(string text, bool append = false);

        public FrmRecalculate()
        {
            InitializeComponent();
        }

        #region Recalculate month readings

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

        private void RecalculateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                RecalcProgressBar.Value = 0;
                RecalcProgressBar.Step = 1;
                RecalcProgressBar.Maximum = 0;
                RecalculateResultTxt.Text = "";
                RecalcProgressLbl.Text = "";

                ConnectDB();
                RecalculateMonthReading(RecalcQueryTxt.Text, IncudeEstidamaCkBx.Checked);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Recalculate water meter month readings based on table query  using threads
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void RecalculateMonthReading(string MonthReadingQuery, bool IncludeEstidama = false)
        {
            DataTable monthReadingList = ExecuteSelectQuery(MonthReadingQuery);

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {
                RecalcProgressLbl.Text = monthReadingList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                RecalcProgressBar.Value = 0;
                RecalcProgressBar.Step = 1;
                RecalcProgressBar.Maximum = monthReadingList.Rows.Count;
                RecalcProgressLbl.Text = string.Format("{0} records Completed from {1}", RecalcProgressBar.Value, RecalcProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            // Log data before 
                            //SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                            //SetRecalculateResultTxt("Start recalculate ID:" + monthReadingList.Rows[t]["ID"].ToString() +
                            //                        "- MeterID:" + monthReadingList.Rows[t]["MeterID"].ToString() +
                            //                        "- ActivityID:" + monthReadingList.Rows[t]["ActivityID"].ToString() +
                            //                        "- Year:" + monthReadingList.Rows[t]["Year"].ToString() +
                            //                        "- Month:" + monthReadingList.Rows[t]["Month"].ToString() +
                            //                        "- Read:" + monthReadingList.Rows[t]["Read"].ToString() +
                            //                        "- PhaseNo:" + monthReadingList.Rows[t]["PhaseNo"].ToString() +
                            //                        "- GuCode:" + monthReadingList.Rows[t]["GuCode"].ToString() +
                            //                        "- ConsumptionMoney:" + monthReadingList.Rows[t]["ConsumptionMoney"].ToString() +
                            //                        "- CBMPrice:" + monthReadingList.Rows[t]["CBMPrice"].ToString() +
                            //                        "- Healthy:" + monthReadingList.Rows[t]["Healthy"].ToString() +
                            //                        "- ServiceBox:" + monthReadingList.Rows[t]["ServiceBox"].ToString() +
                            //                        "- FixFee:" + monthReadingList.Rows[t]["FixFee"].ToString() +
                            //                        "- MeterFixFee:" + monthReadingList.Rows[t]["MeterFixFee"].ToString() + System.Environment.NewLine
                            //    , true);

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
                    RecalcProgressBar.PerformStep();
                    RecalcProgressLbl.Text = string.Format("{0} records Completed from {1}", RecalcProgressBar.Value, RecalcProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                RecalcProgressBar.Value = 0;
                RecalcProgressBar.Step = 1;
                RecalcProgressBar.Maximum = 0;
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
                var monthDate = new DateTime(Year, Month, 1);
                //var meterDate = GetMeterChangesByDate(MeterID, monthDate);

                // Get activity tariff
                var meterTarrifa = GetTariff(ActivityID, monthDate.AddMonths(1).AddDays(-1));

                // Get reading stair details
                var priceResult = calcTariffStairsDetails(TotalReading, meterTarrifa, meterUnits, sewage);

                // Get activity estidama
                decimal FixFee = GetMeterEstidamaByActivityID(MeterID, ActivityID, meterUnits, Convert.ToDateTime(meterTarrifa.Rows[0]["StartDate"].ToString()), TotalReading); // monthDate.AddMonths(1).AddDays(-1)

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
                    //SetRecalculateResultTxt(
                    //                    "End recalculate   ID:" + MonthReadingID +
                    //                    "- MeterID:" + MeterID +
                    //                    "- ActivityID:" + ActivityID +
                    //                    "- Year:" + Year +
                    //                    "- Month:" + Month +
                    //                    "- Read:" + TotalReading +
                    //                    "- PhaseNo:" + sewage +
                    //                    "- GuCode:" + meterUnits +
                    //                    "- ConsumptionMoney:" + TotalPrice.ToString("F3") +
                    //                    "- CBMPrice:" + WaterPrice.ToString("F3") +
                    //                    "- Healthy:" + SewagePrice.ToString("F3") +
                    //                    "- ServiceBox:" + ServiceBoxWithTax.ToString("F3") +
                    //                    "- FixFee:" + FixFee +
                    //                    "- MeterFixFee:" + MeterFixFee +
                    //                    System.Environment.NewLine, true);
                    //SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);

                }
                else
                {
                    SetRecalculateResultTxt("fail to update ID:" + MonthReadingID + System.Environment.NewLine, true);
                    SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                }
            }
            catch
            {
                SetRecalculateResultTxt("fail to update ID:" + MonthReadingID + System.Environment.NewLine, true);
                SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
            }

            return true;
        }

        /// <summary>
        /// Get last tarrif or last tarrif before specific date
        /// </summary>
        /// <param name="ActivityID">Activity identifier</param>
        /// <param name="tarrifDate">Specific tarrif date</param>
        /// <returns>Tarriff details</returns>
        public DataTable GetTariff(string ActivityID, DateTime? tarrifDate = null)
        {
            try
            {
                string sql = " SELECT ActivityID , StairID , StairTo , StairValue , Activities.alarmamt, Activities.dreditamt ,Activities.Name, InitialFees  ,SwgPercent,SwgPrice,PerMeterFees, (select top 1 tax from Settings) as tax , " +
                               " CustomersServiceFees,IsCumulative,IsNoOfUnitsIncludedInCalc  , convert( datetime ,  StartDate , 103 ) as StartDate , StepSwgPrice,IsStepSwgPrice, " +
                               " MinimumFee, MaximumFee, Activities.CurrencyRatio  ,  StairID AS [From],StairTo AS [To], StairValue AS Value,MonthFeesOptionId,MonthStepFees, " +
                               " Activities.Stair , Healthy , PerMeterFees as ServiceBox  , isnull( exceptionvalue , 0 ) as exceptionvalue , ClosedMeterMonthFees ,IsMonthStepFeesCumulative,TariffDetails.id as tariffId " +
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get stairs details
        /// </summary>
        /// <param name="stairs"> stairs details</param>
        /// <returns>Row of stair details</returns>
        private DataTable PrepareStairsDetails(decimal[,] stairs)
        {
            try
            {
                decimal ServiceBox = 0;
                DataTable data = new DataTable();
                DataRow row = data.NewRow();

                for (int i = 1; i < 7; i++)
                {
                    data.Columns.Add("QuantityStair" + i);
                    data.Columns.Add("WaterPrice" + i);
                    data.Columns.Add("Heleathy" + i);
                    data.Columns.Add("Price" + i);
                }

                data.Columns.Add("WService");

                for (int j = 0; j < stairs.GetLength(0); j++)
                {
                    ServiceBox += stairs[j, 2];
                }

                row["WService"] = ServiceBox;

                for (int i = 0; i < 6; i++)
                {
                    row["QuantityStair" + (i + 1)] = stairs[i, 0];
                    row["WaterPrice" + (i + 1)] = stairs[i, 1];
                    row["Heleathy" + (i + 1)] = stairs[i, 5];
                    row["Price" + (i + 1)] = stairs[i, 6];
                }

                data.Rows.Add(row);
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Return Consumption details for specific tarrif
        /// </summary>
        /// <param name="specificDate">Specific date</param>
        /// <param name="ActivityID">Activity</param>
        /// <param name="unitNo">Unit number</param>
        /// <param name="sewage">Sewage</param>
        /// <param name="reading">Quantity reading</param>
        /// <param name="meterid">Meter identifier</param>
        /// <returns></returns>
        public ConsumptionModel GetSpecificDateConsumption(DateTime specificDate, string ActivityID, int unitNo, int sewage, decimal reading, string meterid, DataTable tariff = null)
        {
            try
            {
                var result = new ConsumptionModel();

                // Get activity tariff and stairs (Calculate based on system meter data)
                result.Tarrifa = tariff ?? GetTariff(ActivityID, specificDate);
                result.PriceResult = calcTariffStairsDetails(reading, result.Tarrifa, unitNo, sewage);
                result.Stairs = PrepareStairsDetails(result.PriceResult);

                // Get activity estidama
                result.Fixfee = GetMeterEstidamaByActivityID(meterid, ActivityID, unitNo, Convert.ToDateTime(result.Tarrifa.Rows[0]["StartDate"]), reading);

                result.tarrifId = int.Parse(result.Tarrifa.Rows[0]["tariffId"]?.ToString());
                result.tarrifStartDate = Convert.ToDateTime(result.Tarrifa.Rows[0]["StartDate"]?.ToString());

                for (int i = 0; i < result.PriceResult.GetLength(0); i++)
                {
                    result.WaterPrice += result.PriceResult[i, 1];
                    result.ServiceBoxWithTax += result.PriceResult[i, 2];
                    result.SewagePrice += result.PriceResult[i, 5];
                    result.TotalPrice = result.PriceResult[i, 6] != 0 ? result.PriceResult[i, 6] : result.TotalPrice;
                }

                return result;
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
        public decimal GetMeterEstidamaByActivityID(string meterID, string activityID, int UnitNo, DateTime tarriffDate, decimal quantity)
        {
            try
            {
                var query = "select [dbo].[GetActivityFixedFeeWithConsumption] ('" + meterID + "','" + activityID + "','" + UnitNo + "','" + tarriffDate.ToString("yyyy-MM-dd") + "'," + quantity + ")"; // GetMeterFixedFee
                return Convert.ToDecimal(new dboperation(connectionString).ReturnStr(query));
            }
            catch
            {
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
            bool IncludeUnitNo, IsCumulative, IsStepSwgPrice = false;
            decimal estidamaPerStair, from, to, Price, Tax, ServiceBox, ServiceBoxWithTax, SewagePrice, SewagePercentage, totalSewage, StepSwgPrice, waterPrice, totalPrice = 0;
            decimal allQuantity = Quantity;
            decimal[,] stair = new decimal[dataTariff.Rows.Count, 8];

            for (int i = 0; i < dataTariff.Rows.Count; i++)
            {
                // 1- Prepare water price per units
                IncludeUnitNo = Convert.ToBoolean(dataTariff.Rows[i]["IsNoOfUnitsIncludedInCalc"].ToString());

                from = Convert.ToDecimal(dataTariff.Rows[i]["from"].ToString());
                to = Convert.ToDecimal(dataTariff.Rows[i]["to"].ToString());
                estidamaPerStair = Convert.ToDecimal(dataTariff.Rows[i]["MonthStepFees"].ToString());

                if (IncludeUnitNo)
                {
                    from = from * unitno;
                    to = to * unitno;
                }

                Price = Convert.ToDecimal(dataTariff.Rows[i]["value"]?.ToString());

                // 2- Prepare service box
                Tax = decimal.Parse(dataTariff.Rows[i]["tax"]?.ToString()) / 100;
                ServiceBox = decimal.Parse(dataTariff.Rows[i]["ServiceBox"]?.ToString()) + decimal.Parse(dataTariff.Rows[i]["CustomersServiceFees"].ToString());
                ServiceBoxWithTax = ServiceBox * (1 + Tax);

                // 3- Prepare sewage price
                if (sewage == 1)
                {
                    SewagePrice = Convert.ToDecimal(dataTariff.Rows[i]["SwgPrice"]?.ToString());
                    SewagePercentage = Convert.ToDecimal(dataTariff.Rows[i]["SwgPercent"]?.ToString());
                    IsStepSwgPrice = Convert.ToBoolean(dataTariff.Rows[i]["IsStepSwgPrice"]?.ToString());
                    StepSwgPrice = Convert.ToDecimal(dataTariff.Rows[i]["StepSwgPrice"]?.ToString());
                    totalSewage = (IsStepSwgPrice ? StepSwgPrice : (SewagePrice == 0 ? Price : SewagePrice)) * SewagePercentage / 100;
                }
                else
                {
                    totalSewage = 0;
                }

                // 4- Prepare stair price
                waterPrice = Price + ServiceBoxWithTax + totalSewage;

                // 5- Prepare cumulative
                IsCumulative = Convert.ToBoolean(dataTariff.Rows[i]["IsCumulative"]?.ToString());

                // restart tarriff details
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
                        stair[j, 7] = 0; // Estidama استدامة الشريحة
                    }
                }

                // Calculate stairs main prices
                if (Quantity > to - from && i != dataTariff.Rows.Count - 1)
                {
                    totalPrice += (to - from) * waterPrice;
                    Quantity = Quantity - (to - from);
                    stair[i, 0] = to - from;
                    stair[i, 1] = (to - from) * Price;
                    stair[i, 2] = (to - from) * ServiceBoxWithTax;
                    stair[i, 3] = stair[i, 2] / (1 + Tax);
                    stair[i, 4] = stair[i, 3] * Tax;
                    stair[i, 5] = (to - from) * totalSewage;
                    stair[i, 6] = totalPrice;
                    stair[i, 7] = estidamaPerStair;
                }
                else
                {
                    totalPrice += Quantity * waterPrice;
                    stair[i, 0] = Quantity;
                    stair[i, 1] = Quantity * Price;
                    stair[i, 2] = Quantity * ServiceBoxWithTax;
                    stair[i, 3] = stair[i, 2] / (1 + Tax);
                    stair[i, 4] = stair[i, 3] * Tax;
                    stair[i, 5] = Quantity * totalSewage;
                    stair[i, 6] = totalPrice;
                    stair[i, 7] = estidamaPerStair;
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

        #region Update month readings data

        private void SetUpdateDataResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.UpdateDataResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetUpdateDataResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.UpdateDataResultTxt.AppendText(text);
                }
                else
                {
                    this.UpdateDataResultTxt.Text = text;
                }
            }
        }

        private void UpdateMRDataBtn_Click(object sender, EventArgs e)
        {
            UpdateProgressBar.Value = 0;
            UpdateProgressBar.Step = 1;
            UpdateProgressBar.Maximum = 0;
            UpdateDataResultTxt.Text = "";
            UpdateProgressLbl.Text = "";

            ConnectDB();
            UpdateMonthReading(UpdateQueryTxt.Text);
        }

        /// <summary>
        /// Update water meter month readings based on table query
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void UpdateMonthReading(string MonthReadingQuery)
        {
            DataTable monthReadingList = ExecuteSelectQuery(MonthReadingQuery);

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {
                UpdateProgressLbl.Text = monthReadingList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                UpdateProgressBar.Value = 0;
                UpdateProgressBar.Step = 1;
                UpdateProgressBar.Maximum = monthReadingList.Rows.Count;
                UpdateProgressLbl.Text = string.Format("{0} records Completed from {1}", UpdateProgressBar.Value, UpdateProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            // Log data before 
                            //SetUpdateDataResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                            //SetUpdateDataResultTxt("Start recalculate ID:" + monthReadingList.Rows[t]["ID"].ToString() +
                            //                    "- MeterID:" + monthReadingList.Rows[t]["MeterID"].ToString() +
                            //                    "- ActivityID:" + monthReadingList.Rows[t]["ActivityID"].ToString() +
                            //                    "- Year:" + monthReadingList.Rows[t]["Year"].ToString() +
                            //                    "- Month:" + monthReadingList.Rows[t]["Month"].ToString() +
                            //                    "- PhaseNo:" + monthReadingList.Rows[t]["PhaseNo"].ToString() +
                            //                    "- GuCode:" + monthReadingList.Rows[t]["GuCode"].ToString() + System.Environment.NewLine
                            //    , true);

                            if (i < monthReadingList.Rows.Count)
                            {
                                try
                                {
                                    UpdateWaterMonthReadings(int.Parse(monthReadingList.Rows[i]["ID"].ToString()));
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                });

                // Handle progress change
                worker.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    UpdateProgressBar.PerformStep();
                    UpdateProgressLbl.Text = string.Format("{0} records Completed from {1}", UpdateProgressBar.Value, UpdateProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                UpdateProgressBar.Value = 0;
                UpdateProgressBar.Step = 1;
                UpdateProgressBar.Maximum = 0;
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
        public bool UpdateWaterMonthReadings(int MonthReadingID)
        {
            try
            {
                // Update month reading
                var result = UpdateDbMonthReadingDetails(MonthReadingID);

                //// Log data after
                //SetUpdateDataResultTxt("End recalculate   ID:" + MonthReadingID + "- Result:" + result + System.Environment.NewLine, true);
                //SetUpdateDataResultTxt("--------------------------------------------" + System.Environment.NewLine, true);

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
        /// <param name="ID">Month reading identifier</param>
        /// <returns>Bool indicator saved or not</returns>
        public bool UpdateDbMonthReadingDetails(int ID)
        {
            try
            {
                dboperation db = new dboperation(connectionString);
                db.objcmd.Parameters.Clear();
                db.objcmd.CommandType = CommandType.StoredProcedure;
                db.objcmd.CommandText = "UpdateMonthReadingData";
                db.objcmd.Parameters.AddWithValue("@MonthReadingId", ID);
                SqlParameter sqlResult = new SqlParameter("@ReturnVal", SqlDbType.Bit, 1);
                sqlResult.Direction = ParameterDirection.Output;
                db.objcmd.Parameters.Add(sqlResult);
                db.ExecuteNonQuery("");

                if (sqlResult.Value != null && (bool)sqlResult.Value == true) // Add month reading
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Cancel charges

        private void SetCancelResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.CancelChargesResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetCancelResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.CancelChargesResultTxt.AppendText(text);
                }
                else
                {
                    this.CancelChargesResultTxt.Text = text;
                }
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            CancelProgressBar.Value = 0;
            CancelProgressBar.Step = 1;
            CancelProgressBar.Maximum = 0;
            CancelChargesResultTxt.Text = "";
            CancelProgressLbl.Text = "";

            ConnectDB();
            CancelCharges(CancelChargesQueryTxt.Text);
        }

        /// <summary>
        /// Cancel charges based on table query
        /// </summary>
        /// <param name="ChargesQuery">Charges query</param>
        public void CancelCharges(string ChargesQuery)
        {
            DataTable chargesList = ExecuteSelectQuery(ChargesQuery);

            if (chargesList != null && chargesList.Rows.Count > 0)
            {
                CancelProgressLbl.Text = chargesList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                CancelProgressBar.Value = 0;
                CancelProgressBar.Step = 1;
                CancelProgressBar.Maximum = chargesList.Rows.Count;
                CancelProgressLbl.Text = string.Format("{0} records Completed from {1}", CancelProgressBar.Value, CancelProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {

                    for (int t = 0; t < chargesList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            // Log data before 
                            SetCancelResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                            SetCancelResultTxt("Start cancel Charge serial number:" + chargesList.Rows[t]["SerialNo"].ToString() + System.Environment.NewLine, true);

                            if (i < chargesList.Rows.Count)
                            {
                                try
                                {
                                    // Log data after
                                    if (CancelCharge(chargesList.Rows[i]["SerialNo"].ToString()))
                                    {
                                        SetCancelResultTxt("End: Success to cancel Charge serial number:" + chargesList.Rows[i]["SerialNo"].ToString() + System.Environment.NewLine);
                                        SetCancelResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                                    }
                                    else
                                    {
                                        SetCancelResultTxt("End: Failed to cancel Charge serial number:" + chargesList.Rows[i]["SerialNo"].ToString() + System.Environment.NewLine, true);
                                        SetCancelResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                });

                // Handle progress change
                worker.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    CancelProgressBar.PerformStep();
                    CancelProgressLbl.Text = string.Format("{0} records Completed  from {1}", CancelProgressBar.Value, CancelProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                CancelProgressBar.Value = 0;
                CancelProgressBar.Step = 1;
                CancelProgressBar.Maximum = 0;
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

        #region Shared

        public void ConnectDB()
        {
            StringBuilder Con = new StringBuilder("Password=" + textBox4.Text);
            Con.Append(";Persist Security Info=True;User ID=" + textBox3.Text);
            Con.Append(";Initial Catalog=" + textBox2.Text);
            Con.Append(";MultipleActiveResultSets=True;Max Pool Size=30000;Data Source=" + textBox1.Text + ";");
            this.connectionString = Con.ToString();
        }

        /// <summary>
        /// Execute select query
        /// </summary>
        /// <param name="SelectQuery">Sql query</param>
        /// <returns>Query result</returns>
        public DataTable ExecuteSelectQuery(string SelectQuery)
        {
            try
            {
                return new dboperation(connectionString).SelectData(SelectQuery);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Update Read mass card readings

        private void SetReviewCardResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.UpdateDataResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetReviewCardResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.ReviewCardResultTxt.AppendText(text);
                }
                else
                {
                    this.ReviewCardResultTxt.Text = text;
                }
            }
        }

        private void ReviewCardDataBtn_Click(object sender, EventArgs e)
        {
            ReviewCardProgressBar.Value = 0;
            ReviewCardProgressBar.Step = 1;
            ReviewCardProgressBar.Maximum = 0;
            ReviewCardResultTxt.Text = "";
            ReviewCardProgressLbl.Text = "";

            ConnectDB();
            ReviewCardMassCardReading(ReviewCardQueryTxt.Text);
        }

        /// <summary>
        /// Update mass card readings based on table query
        /// </summary>
        /// <param name="MonthReadingQuery">Mass card reading query</param>
        public void ReviewCardMassCardReading(string ReviewCardQuery)
        {
            DataTable cardReadingList = ExecuteSelectQuery(ReviewCardQuery);

            if (cardReadingList != null && cardReadingList.Rows.Count > 0)
            {
                ReviewCardProgressLbl.Text = cardReadingList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                UpdateProgressBar.Value = 0;
                UpdateProgressBar.Step = 1;
                ReviewCardProgressBar.Maximum = cardReadingList.Rows.Count;
                ReviewCardProgressLbl.Text = string.Format("{0} records Completed from {1}", ReviewCardProgressBar.Value, ReviewCardProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < cardReadingList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            // Log data before 
                            //SetReviewCardResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
                            //SetReviewCardResultTxt("Start recalculate ID:" + cardReadingList.Rows[t]["ID"].ToString() +
                            //                    "- MeterID:" + cardReadingList.Rows[t]["MeterID"].ToString() +
                            //                    "- ActivityID:" + cardReadingList.Rows[t]["ActivityID"].ToString() +
                            //                    "- Year:" + cardReadingList.Rows[t]["Year"].ToString() +
                            //                    "- Month:" + cardReadingList.Rows[t]["Month"].ToString() +
                            //                    "- PhaseNo:" + cardReadingList.Rows[t]["PhaseNo"].ToString() +
                            //                    "- GuCode:" + cardReadingList.Rows[t]["GuCode"].ToString() + System.Environment.NewLine
                            //    , true);

                            if (i < cardReadingList.Rows.Count)
                            {
                                try
                                {
                                    UpdateReviewCardReadings(int.Parse(cardReadingList.Rows[i]["ID"].ToString()));
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                });

                // Handle progress change
                worker.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    ReviewCardProgressBar.PerformStep();
                    ReviewCardProgressLbl.Text = string.Format("{0} records Completed from {1}", ReviewCardProgressBar.Value, ReviewCardProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                ReviewCardProgressBar.Value = 0;
                ReviewCardProgressBar.Step = 1;
                ReviewCardProgressBar.Maximum = 0;
            }
        }

        /// <summary>
        /// Update mass card readings
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
        public bool UpdateReviewCardReadings(int CardReadingID)
        {
            try
            {
                // Update read mass retrieval card reading
                var result = UpdateReviewCardDetails(CardReadingID);

                //// Log data after
                //SetReviewCardResultTxt("End recalculate   ID:" + CardReadingID + "- Result:" + result + System.Environment.NewLine, true);
                //SetReviewCardResultTxt("--------------------------------------------" + System.Environment.NewLine, true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update mass card readings details
        /// </summary>
        /// <param name="ID">Mass reading identifier</param>
        /// <returns>Bool indicator saved or not</returns>
        public bool UpdateReviewCardDetails(int ID)
        {
            try
            {
                dboperation db = new dboperation(connectionString);
                db.objcmd.Parameters.Clear();
                db.objcmd.CommandType = CommandType.StoredProcedure;
                db.objcmd.CommandText = "UpdateMassReadingData";
                db.objcmd.Parameters.AddWithValue("@MassCardReadingId", ID);
                SqlParameter sqlResult = new SqlParameter("@ReturnVal", SqlDbType.Bit, 1);
                sqlResult.Direction = ParameterDirection.Output;
                db.objcmd.Parameters.Add(sqlResult);
                db.ExecuteNonQuery("");

                if (sqlResult.Value != null && (bool)sqlResult.Value == true) // Add mass card reading
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Update tarriff difference

        private void SetTarrifDifferenceResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TarrifDifferenceResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetTarrifDifferenceResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.TarrifDifferenceResultTxt.AppendText(text);
                }
                else
                {
                    this.TarrifDifferenceResultTxt.Text = text;
                }
            }
        }

        private void TarrifDifferenceBtn_Click(object sender, EventArgs e)
        {
            TarrifDifferenceProgressBar.Value = 0;
            TarrifDifferenceProgressBar.Step = 1;
            TarrifDifferenceProgressBar.Maximum = 0;
            TarrifDifferenceResultTxt.Text = "";
            TarrifDifferenceProgressLbl.Text = "";

            ConnectDB();
            UpdateandRecalcTarrifDifference(TarrifDifferenceQueryTxt.Text);
        }

        /// <summary>
        /// Update mass card readings based on table query
        /// </summary>
        /// <param name="MonthReadingQuery">Mass card reading query</param>
        public void UpdateandRecalcTarrifDifference(string TarrifDiffQuery)
        {
            DataTable monthReadingsList = ExecuteSelectQuery(TarrifDiffQuery);

            if (monthReadingsList != null && monthReadingsList.Rows.Count > 0)
            {
                TarrifDifferenceProgressLbl.Text = monthReadingsList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                TarrifDifferenceProgressBar.Value = 0;
                TarrifDifferenceProgressBar.Step = 1;
                TarrifDifferenceProgressBar.Maximum = monthReadingsList.Rows.Count;
                TarrifDifferenceProgressLbl.Text = string.Format("{0} records Completed from {1}", TarrifDifferenceProgressBar.Value, TarrifDifferenceProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingsList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            if (i < monthReadingsList.Rows.Count)
                            {
                                try
                                {
                                    CalculateTariffDifference(int.Parse(monthReadingsList.Rows[i]["ID"].ToString()),
                                                              monthReadingsList.Rows[i]["ActivityID"].ToString(),
                                                              monthReadingsList.Rows[i]["SerialNu"].ToString(),
                                                              monthReadingsList.Rows[i]["MeterId"].ToString(),
                                                              int.Parse(monthReadingsList.Rows[i]["month"].ToString()),
                                                              int.Parse(monthReadingsList.Rows[i]["year"].ToString()),
                                                              int.Parse(monthReadingsList.Rows[i]["GuCode"].ToString() == "" ? "1" : monthReadingsList.Rows[i]["GuCode"].ToString()),
                                                              int.Parse(monthReadingsList.Rows[i]["PhaseNo"].ToString() == "" ? "1" : monthReadingsList.Rows[i]["PhaseNo"].ToString()),
                                                              Convert.ToDecimal(monthReadingsList.Rows[i]["TotalConsumption"].ToString()),
                                                              Convert.ToDecimal(monthReadingsList.Rows[i]["ConsumptionMoney"].ToString()),
                                                              double.Parse(monthReadingsList.Rows[i]["CBMPrice"].ToString()),
                                                              double.Parse(monthReadingsList.Rows[i]["Healthy"].ToString()),
                                                              double.Parse(monthReadingsList.Rows[i]["ServiceBox"].ToString()),
                                                              System.DateTime.Parse(monthReadingsList.Rows[i]["TariffStartDate"].ToString()));
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
                    TarrifDifferenceProgressBar.PerformStep();
                    TarrifDifferenceProgressLbl.Text = string.Format("{0} records Completed from {1}", TarrifDifferenceProgressBar.Value, TarrifDifferenceProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                TarrifDifferenceProgressBar.Value = 0;
                TarrifDifferenceProgressBar.Step = 1;
                TarrifDifferenceProgressBar.Maximum = 0;
            }
        }

        /// <summary>
        /// Update mass card readings
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
        public bool CalculateTariffDifference(int MonthReadingID, string activityId, string serialNu, string meterId, int month, int year, int unitsNo, int sewage, decimal totalConsumption, decimal oldConsumptionMoney, double oldCBMPrice, double oldHealthy, double oldServiceBox, DateTime oldActiveDate)
        {
            SqlTransaction transaction = null;
            dboperation db = new dboperation(connectionString);

            try
            {
                // Get adjustment Type
                var adjustmentType = int.Parse(db.ReturnStr("select id from AdjustmentTypes where Code = '6'"));
                var setingDT = db.SelectData("select top 1 MaxInstalmentsAmount,DefaultInstalmentsNumber from settings");
                var MaxInstalmentsAmount = Convert.ToDecimal(string.IsNullOrEmpty(setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString()) ? "0" : setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString());
                var DefaultInstalmentsNumber = int.Parse(string.IsNullOrEmpty(setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString()) ? "1" : setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString());
                var newActiveDate = new System.DateTime(2022, 3, 1);

                // Get new water price
                var newTariff = GetTariff(activityId, newActiveDate);
                var newConsumptionModel = GetSpecificDateConsumption(newActiveDate, activityId, unitsNo, sewage, totalConsumption, meterId, newTariff);
                newActiveDate = newConsumptionModel.tarrifStartDate;
                var newTotalPrice = newConsumptionModel.TotalPrice + newConsumptionModel.Fixfee;
                var consumptionPriceDifference = newTotalPrice - oldConsumptionMoney;

                // Start transaction
                if (db.objcmd.Connection.State != ConnectionState.Open)
                {
                    db.objcmd.Connection.Open();
                }

                transaction = db.objcmd.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                db.objcmd.Transaction = transaction;

                // Save Water Tariff Price Differences
                string query = " insert into TariffPriceDifferences ( MeterId , ActivityId,OldTariffDate,NewTariffDate,MonthReadingsId, Year, Month ,ChargeSerialNu,MeterTotalConsumption, MeterConsumptionPrice , OldWaterPrice , OldHealthyPrice,OldServiceBoxPrice ,"
                             + " NewWaterPrice , NewHealthyPrice,NewServiceBoxPrice,NewConsumptionPrice,ConsumptionPriceDifference,isAdjustmentAdded,CreatedDate,CreatedBy )"
                             + " values('" + meterId + "','" + activityId + "','" + oldActiveDate.ToString("yyyy-MM-dd") + "','" + newActiveDate.ToString("yyyy-MM-dd") + "'," + MonthReadingID + "," + year + "," + month + ",'" + serialNu + "',"
                             + totalConsumption + "," + oldConsumptionMoney + "," + oldCBMPrice + "," + oldHealthy + "," + oldServiceBox + "," + newConsumptionModel.WaterPrice + "," + newConsumptionModel.SewagePrice + "," + newConsumptionModel.ServiceBoxWithTax + ","
                             + newTotalPrice + "," + consumptionPriceDifference + ",0,getDate(),(select top 1 userid from users))";
                db.objcmd.CommandText = query;
                var ret = db.objcmd.ExecuteNonQuery();

                if (ret > 0)
                {
                    // update month reading
                    string updateMonthReadingQuery = "update MonthReadings set CBMPrice = " + newConsumptionModel.WaterPrice + " , Healthy = " + newConsumptionModel.SewagePrice + " ,"
                                                            + " ServiceBox = " + newConsumptionModel.ServiceBoxWithTax + " ,"
                                                            + " FixFee = " + newConsumptionModel.Fixfee + " ,"
                                                            + " ConsumptionMoney = " + newTotalPrice + " ,"
                                                            + " tarriffAdjustment = " + consumptionPriceDifference + ","
                                                            + " TariffStartDate = '" + newActiveDate.ToString("yyyy-MM-dd") + "'"
                                                            + " where id = " + MonthReadingID;

                    db.objcmd.CommandText = updateMonthReadingQuery;
                    var res = db.objcmd.ExecuteNonQuery();

                    if (res > 0)
                    {
                        // Add Water Tariff Price Differences as adjustment if positive value 
                        query = " insert into Adjustments (Code,MeterID,Type,Reason,CurrentDate,TotalValue,MonthsCount,MonthlyRate,Remminder,PaidMonths,PercentValue,IsDeleted,ActiveAd,description ,UserID ,AccountNo,inputdate , DueDate) "
                            + " select (IDENT_CURRENT('Adjustments') + ROW_NUMBER() OVER (ORDER BY MeterId)) ,MeterId, " + adjustmentType + " , (select top 1 id from AdjustmentReasons),getDate(),sum(ConsumptionPriceDifference),CASE WHEN "
                            + " sum(ConsumptionPriceDifference) > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN " + DefaultInstalmentsNumber + " ELSE 1 END,"
                            + " CASE WHEN sum(ConsumptionPriceDifference) > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN (sum(ConsumptionPriceDifference) / " + DefaultInstalmentsNumber + " )   ELSE sum(ConsumptionPriceDifference) "
                            + " END,sum(ConsumptionPriceDifference),0,100,0,1,(select concat(sum(ConsumptionPriceDifference) , '')) + (select '  فرق التعريفة اثر رجعي ') ,"
                            + " (select top 1 userid from users),(SELECT top 1 AccountNo FROM METERS where meterid = TariffPriceDifferences.MeterId),getDate(),getDate() "
                            + " from TariffPriceDifferences "
                            + " where isAdjustmentAdded = 0 and ConsumptionPriceDifference > 0.1 "
                            + " group by MeterId ";

                        db.objcmd.CommandText = query;
                        db.objcmd.ExecuteNonQuery();

                        query = "update TariffPriceDifferences set isAdjustmentAdded = 1 where isAdjustmentAdded = 0";
                        db.objcmd.CommandText = query;
                        db.objcmd.ExecuteNonQuery();

                        //// insert Reading Stairs
                        //query = "delete from ReadingStairs where ReadingID = " + MonthReadingID;
                        //db.objcmd.CommandText = query;
                        //db.objcmd.ExecuteNonQuery();

                        //query = " Insert into ReadingStairs(ReadingID, Price1, Price2, Price3, Price4, Price5, Price6, "
                        //      + " WaterPrice1, WaterPrice2, WaterPrice3, WaterPrice4, WaterPrice5, WaterPrice6, QuantityStair1, QuantityStair2, "
                        //      + " QuantityStair3, QuantityStair4, QuantityStair5, QuantityStair6, Heleathy1, Heleathy2, Heleathy3, Heleathy4, Heleathy5, Heleathy6, WService)"
                        //      + " Values ( " + MonthReadingID + "," + newConsumptionModel.Stairs.Rows[0]["Price1"] + "," + newConsumptionModel.Stairs.Rows[0]["Price2"] + "," + newConsumptionModel.Stairs.Rows[0]["Price3"] + "," + newConsumptionModel.Stairs.Rows[0]["Price4"] + "," + newConsumptionModel.Stairs.Rows[0]["Price5"] + "," + newConsumptionModel.Stairs.Rows[0]["Price6"] + ","
                        //      + newConsumptionModel.Stairs.Rows[0]["WaterPrice1"] + "," + newConsumptionModel.Stairs.Rows[0]["WaterPrice2"] + "," + newConsumptionModel.Stairs.Rows[0]["WaterPrice3"] + "," + newConsumptionModel.Stairs.Rows[0]["WaterPrice4"] + "," + newConsumptionModel.Stairs.Rows[0]["WaterPrice5"] + "," + newConsumptionModel.Stairs.Rows[0]["WaterPrice6"] + "," + newConsumptionModel.Stairs.Rows[0]["QuantityStair1"] + "," + newConsumptionModel.Stairs.Rows[0]["QuantityStair2"] + ", "
                        //      + newConsumptionModel.Stairs.Rows[0]["QuantityStair3"] + "," + newConsumptionModel.Stairs.Rows[0]["QuantityStair4"] + "," + newConsumptionModel.Stairs.Rows[0]["QuantityStair5"] + "," + newConsumptionModel.Stairs.Rows[0]["QuantityStair6"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy1"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy2"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy3"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy4"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy5"] + "," + newConsumptionModel.Stairs.Rows[0]["Heleathy6"] + "," + newConsumptionModel.Stairs.Rows[0]["WService"] + ")";
                        //db.objcmd.CommandText = query;
                        //db.objcmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
        }


        #endregion

        #region Fix sewage for tarrif difference

        private void SetFixSewageResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.FixSewageResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetFixSewageResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.FixSewageResultTxt.AppendText(text);
                }
                else
                {
                    this.FixSewageResultTxt.Text = text;
                }
            }
        }

        private void FixSewageBtn_Click(object sender, EventArgs e)
        {
            FixSewageProgressBar.Value = 0;
            FixSewageProgressBar.Step = 1;
            FixSewageProgressBar.Maximum = 0;
            FixSewageResultTxt.Text = "";
            FixSewageProgressLbl.Text = "";

            ConnectDB();
            UpdateAndFixSewage(FixSewageQueryTxt.Text);
        }

        /// <summary>
        /// Update mass card readings based on table query
        /// </summary>
        /// <param name="MonthReadingQuery">Mass card reading query</param>
        public void UpdateAndFixSewage(string fixSewageQuery)
        {
            DataTable monthReadingsList = ExecuteSelectQuery(fixSewageQuery);

            if (monthReadingsList != null && monthReadingsList.Rows.Count > 0)
            {
                FixSewageProgressLbl.Text = monthReadingsList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                FixSewageProgressBar.Value = 0;
                FixSewageProgressBar.Step = 1;
                FixSewageProgressBar.Maximum = monthReadingsList.Rows.Count;
                FixSewageProgressLbl.Text = string.Format("{0} records Completed from {1}", FixSewageProgressBar.Value, FixSewageProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingsList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);

                            if (i < monthReadingsList.Rows.Count)
                            {
                                try
                                {
                                    RecalcSewageforTariffDifference(int.Parse(monthReadingsList.Rows[i]["ID"].ToString()),
                                                                    monthReadingsList.Rows[i]["ActivityID"].ToString(),
                                                                    monthReadingsList.Rows[i]["MeterId"].ToString(),
                                                                    int.Parse(monthReadingsList.Rows[i]["GuCode"].ToString() == "" ? "1" : monthReadingsList.Rows[i]["GuCode"].ToString()),
                                                                    int.Parse(monthReadingsList.Rows[i]["PhaseNo"].ToString() == "" ? "1" : monthReadingsList.Rows[i]["PhaseNo"].ToString()),
                                                                    Convert.ToDecimal(monthReadingsList.Rows[i]["TotalConsumption"].ToString()),
                                                                    Convert.ToDecimal(monthReadingsList.Rows[i]["OldConsumption"].ToString()),
                                                                    Convert.ToDecimal(monthReadingsList.Rows[i]["tarriffAdjustment"].ToString()),
                                                                    double.Parse(monthReadingsList.Rows[i]["Healthy"].ToString()));
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
                    FixSewageProgressBar.PerformStep();
                    FixSewageProgressLbl.Text = string.Format("{0} records Completed from {1}", FixSewageProgressBar.Value, FixSewageProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                FixSewageProgressBar.Value = 0;
                FixSewageProgressBar.Step = 1;
                FixSewageProgressBar.Maximum = 0;
            }
        }

        /// <summary>
        /// Update and fix wrong sewage
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
        public bool RecalcSewageforTariffDifference(int MonthReadingID, string activityId, string meterId, int unitsNo, int sewage, decimal Reading, decimal OldConsumption, decimal oldTarrifAdjustment, double oldHealthy)
        {
            SqlTransaction transaction = null;
            dboperation db = new dboperation(connectionString);
            int adjustmentID = 0;
            decimal adjustmentValue = 0;
            int adjustmentPaid = 0;

            try
            {
                // Get adjustment Type
                var freeAdjustment = int.Parse(db.ReturnStr("select id from AdjustmentTypes where Code = '12'"));
                var adjustmentType = int.Parse(db.ReturnStr("select id from AdjustmentTypes where Code = '6'"));
                var setingDT = db.SelectData("select top 1 MaxInstalmentsAmount,DefaultInstalmentsNumber from settings");
                var MaxInstalmentsAmount = Convert.ToDecimal(string.IsNullOrEmpty(setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString()) ? "0" : setingDT.Rows[0]["MaxInstalmentsAmount"]?.ToString());
                var DefaultInstalmentsNumber = int.Parse(string.IsNullOrEmpty(setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString()) ? "1" : setingDT.Rows[0]["DefaultInstalmentsNumber"]?.ToString());
                var newActiveDate = new System.DateTime(2022, 3, 1);

                // Get new water price
                var newTariff = GetTariff(activityId, newActiveDate);
                var newConsumptionModel = GetSpecificDateConsumption(newActiveDate, activityId, unitsNo, sewage, Reading, meterId, newTariff);

                var newTotalPrice = newConsumptionModel.TotalPrice + newConsumptionModel.Fixfee;
                var consumptionPriceDifference = newTotalPrice - OldConsumption;

                if (oldTarrifAdjustment > 0)
                {
                    var adjustment = db.SelectData("select top 1 ID,PaidMonths,TotalValue from Adjustments where type = " + adjustmentType + " and meterid = '" + meterId + "'");
                    adjustmentID = int.Parse(adjustment.Rows[0]["ID"].ToString());
                    adjustmentPaid = int.Parse(adjustment.Rows[0]["PaidMonths"].ToString());
                    adjustmentValue = Convert.ToDecimal(adjustment.Rows[0]["TotalValue"].ToString());
                }

                // Start transaction
                if (db.objcmd.Connection.State != ConnectionState.Open)
                {
                    db.objcmd.Connection.Open();
                }

                transaction = db.objcmd.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                db.objcmd.Transaction = transaction;

                // update month reading
                string query = "update MonthReadings set Healthy = " + newConsumptionModel.SewagePrice + " ,"
                                                        + " ConsumptionMoney = " + newTotalPrice + " ,"
                                                        + " tarriffAdjustment = " + consumptionPriceDifference
                                                        + " where id = " + MonthReadingID;

                db.objcmd.CommandText = query;
                var res = db.objcmd.ExecuteNonQuery();

                // update tarrif difference
                query = "update TariffPriceDifferences set NewHealthyPrice = " + newConsumptionModel.SewagePrice + " ,"
                                                        + " NewConsumptionPrice = " + newTotalPrice + " ,"
                                                        + " ConsumptionPriceDifference = " + consumptionPriceDifference
                                                        + " where MonthReadingsId = " + MonthReadingID;

                db.objcmd.CommandText = query;
                res = db.objcmd.ExecuteNonQuery();

                // Update adjustments
                if (oldTarrifAdjustment > 0)
                {
                    decimal AdjustmentDiff = oldTarrifAdjustment - consumptionPriceDifference;

                    if (adjustmentPaid > 0)
                    {
                        // Insert new one with diff
                        if (AdjustmentDiff < 0)
                        {
                            AdjustmentDiff = AdjustmentDiff * -1;

                            // for HCWW
                            if (AdjustmentDiff > (decimal)0.1)
                            {
                                query = " insert into Adjustments (Code,MeterID,Type,Reason,CurrentDate,TotalValue,MonthsCount,MonthlyRate,Remminder,PaidMonths,PercentValue,IsDeleted,ActiveAd,description ,UserID ,AccountNo,inputdate , DueDate) "
                                    + " values( (IDENT_CURRENT('Adjustments') + 1 ) ,'"+meterId+"', " + adjustmentType + " , (select top 1 id from AdjustmentReasons),getDate(),"+AdjustmentDiff+" ,CASE WHEN "
                                    + AdjustmentDiff+ " > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN " + DefaultInstalmentsNumber + " ELSE 1 END,"
                                    + " CASE WHEN "+AdjustmentDiff+" > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN ("+AdjustmentDiff+" / " + DefaultInstalmentsNumber + " )   ELSE  " + AdjustmentDiff
                                    + " END,"+AdjustmentDiff+",0,100,0,1,(select concat("+AdjustmentDiff+" , '')) + (select '  فرق التعريفة اثر رجعي ') ,"
                                    + " (select top 1 userid from users),(SELECT top 1 AccountNo FROM METERS where meterid = '"+meterId+"'),getDate(),getDate())";
                            }
                        }
                        else if (AdjustmentDiff > (decimal)0.1)
                        {
                            // for customer
                            query = " insert into Adjustments (Code,MeterID,Type,Reason,CurrentDate,TotalValue,MonthsCount,MonthlyRate,Remminder,PaidMonths,PercentValue,IsDeleted,ActiveAd,description ,UserID ,AccountNo,inputdate , DueDate) "
                                   + " values( (IDENT_CURRENT('Adjustments') + 1) ,'"+meterId+"', " + freeAdjustment + " , (select top 1 id from AdjustmentReasons),getDate(),"+AdjustmentDiff+" ,CASE WHEN "
                                   + AdjustmentDiff+ " > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN " + DefaultInstalmentsNumber + " ELSE 1 END,"
                                   + " CASE WHEN "+AdjustmentDiff+" > " + MaxInstalmentsAmount + " and " + MaxInstalmentsAmount + " != 0  THEN ("+AdjustmentDiff+" / " + DefaultInstalmentsNumber + " )   ELSE  " + AdjustmentDiff
                                   + " END,"+AdjustmentDiff+",0,100,0,1,(select concat("+AdjustmentDiff+" , '')) + (select '  فرق التعريفة اثر رجعي ') ,"
                                   + " (select top 1 userid from users),(SELECT top 1 AccountNo FROM METERS where meterid = '"+meterId+"'),getDate(),getDate())";
                        }
                    }
                    else
                    {
                        // Update old one
                        query = " update Adjustments set TotalValue = " + consumptionPriceDifference + ", Remminder="+ consumptionPriceDifference +" , MonthlyRate=" +consumptionPriceDifference +" where id = " + adjustmentID;
                    }

                    db.objcmd.CommandText = query;
                    db.objcmd.ExecuteNonQuery();
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
        }


        #endregion

        #region Recalc month reading with new quantity

        private void SetRecalcQuantityResultTxt(string text, bool append = false)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.RecalcQuantityResultTxt.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetRecalcQuantityResultTxt);
                this.Invoke(d, new object[] { text, append });
            }
            else
            {
                if (append)
                {
                    this.RecalcQuantityResultTxt.AppendText(text);
                }
                else
                {
                    this.RecalcQuantityResultTxt.Text = text;
                }
            }
        }

        private void RecalcQuantityBtn_Click(object sender, EventArgs e)
        {
            try
            {
                RecalcQuantityProgressBar.Value = 0;
                RecalcQuantityProgressBar.Step = 1;
                RecalcQuantityProgressBar.Maximum = 0;
                RecalcQuantityResultTxt.Text = "";
                RecalcQuantityProgressLbl.Text = "";

                ConnectDB();
                RecalcMonthReadingQuantity(RecalcQuantityQueryTxt.Text);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Recalc water meter month readings based on new quantity
        /// </summary>
        /// <param name="MonthReadingQuery">Month reading query</param>
        public void RecalcMonthReadingQuantity(string MonthReadingQuery)
        {
            DataTable monthReadingList = ExecuteSelectQuery(MonthReadingQuery);

            if (monthReadingList != null && monthReadingList.Rows.Count > 0)
            {
                RecalcQuantityProgressLbl.Text = monthReadingList.Rows.Count.ToString();
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                RecalcQuantityProgressBar.Value = 0;
                RecalcQuantityProgressBar.Step = 1;
                RecalcQuantityProgressBar.Maximum = monthReadingList.Rows.Count;
                RecalcQuantityProgressLbl.Text = string.Format("{0} records Completed from {1}", RecalcQuantityProgressBar.Value, RecalcQuantityProgressBar.Maximum);

                worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                {
                    for (int t = 0; t < monthReadingList.Rows.Count; t++)
                    {
                        var i = t;

                        try
                        {
                            worker.ReportProgress(t);
                            
                            if (i < monthReadingList.Rows.Count)
                            {
                                try
                                {
                                    RecalcWaterMonthReadingsWithNewQuantity(
                                          int.Parse(monthReadingList.Rows[i]["ID"].ToString()),
                                          monthReadingList.Rows[i]["MeterID"].ToString(),
                                          monthReadingList.Rows[i]["ActivityID"].ToString(),
                                          int.Parse(monthReadingList.Rows[i]["PhaseNo"].ToString()),
                                          int.Parse(monthReadingList.Rows[i]["GuCode"].ToString()),
                                          decimal.Parse(monthReadingList.Rows[i]["Read"].ToString()),
                                          decimal.Parse(monthReadingList.Rows[i]["OldConsumption"].ToString()),
                                          decimal.Parse(monthReadingList.Rows[i]["ConsumptionMoney"].ToString()),
                                          int.Parse(monthReadingList.Rows[i]["Year"].ToString()),
                                          int.Parse(monthReadingList.Rows[i]["Month"].ToString()),
                                          Convert.ToDecimal(monthReadingList.Rows[i]["consumptionAdjustment"]),
                                          Convert.ToDecimal(monthReadingList.Rows[i]["tarriffAdjustment"]),
                                          Convert.ToDecimal(monthReadingList.Rows[i]["meterInstallment"])
                                          );
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
                    RecalcQuantityProgressBar.PerformStep();
                    RecalcQuantityProgressLbl.Text = string.Format("{0} records Completed from {1}", RecalcQuantityProgressBar.Value, RecalcQuantityProgressBar.Maximum);
                });

                // Handle complete
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
            else
            {
                RecalcQuantityProgressBar.Value = 0;
                RecalcQuantityProgressBar.Step = 1;
                RecalcQuantityProgressBar.Maximum = 0;
            }
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
        public bool RecalcWaterMonthReadingsWithNewQuantity(int MonthReadingId, string MeterID, string ActivityID, int sewage, int meterUnits, decimal OldConsumption,decimal NewConsumption,decimal UsedMonthly, int Year, int Month, decimal oldConsumptionAdjustment,decimal oldTarriffAdjustment,decimal meterInstallment)
        {
            try
            {
                decimal consumptionAdjustment = 0;
                decimal tarriffAdjustment = 0;
                var monthDate = new System.DateTime(Year, Month, 1);
                ConsumptionModel lastChargeConsumptionModel = null;

                // Get meter model type
                int meterType = GetMeterTypeByMeterId(MeterID);

                // Get expected tarrif calculation object
                var expectedConsumptionModel = GetSpecificDateConsumption(monthDate.AddMonths(1).AddDays(-1), ActivityID, meterUnits, sewage, NewConsumption, MeterID);

                // Set old meters used month
                if (meterType == 10 || meterType == 11)
                {
                    UsedMonthly = expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee;
                }

                // Get last success charge details
                DataTable lastChargeDT = GetMeterLastSuccessCharge(MeterID, monthDate.AddMonths(1));

                // Check tarrif changes
                if (lastChargeDT.Rows.Count > 0 && Convert.ToDateTime(lastChargeDT.Rows[0]["TariffStartDate"].ToString()) != expectedConsumptionModel.tarrifStartDate
                    && System.DateTime.Parse(lastChargeDT.Rows[0]["serverDate"].ToString()) <= monthDate.AddMonths(1))
                {
                    // Get last charge tarrif calculation object
                    lastChargeConsumptionModel = GetSpecificDateConsumption(System.DateTime.Parse(lastChargeDT.Rows[0]["TariffStartDate"].ToString()), ActivityID, meterUnits, sewage, NewConsumption, MeterID);

                    // Get different tarrif recalc adjustment
                    if (lastChargeConsumptionModel != null)
                    {
                        tarriffAdjustment = (expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee + meterInstallment) - (lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee);

                        // Set old meters used month
                        if (meterType == 10 || meterType == 11)
                        {
                            UsedMonthly = lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee;
                        }
                    }
                }

                // Get different money consumption adjustment (New meters only)
                if (meterType != 10 && meterType != 11)
                {
                    if (lastChargeConsumptionModel != null)
                    {
                        consumptionAdjustment = (lastChargeConsumptionModel.TotalPrice + lastChargeConsumptionModel.Fixfee + meterInstallment) - UsedMonthly;
                    }
                    else
                    {
                        consumptionAdjustment = (expectedConsumptionModel.TotalPrice + expectedConsumptionModel.Fixfee + meterInstallment) - UsedMonthly;
                    }
                }

                // Prepare new adjustments difference
                var consumptionAdjustmentDiff = consumptionAdjustment - oldConsumptionAdjustment;
                var tarriffAdjustmentDiff = tarriffAdjustment - oldTarriffAdjustment;

                // Update MonthReadings record
                var updateMonthReadingsResult = UpdateMonthReadingData(MonthReadingId, UsedMonthly, consumptionAdjustment, tarriffAdjustment);
                
                if (updateMonthReadingsResult > 0)
                {
                    // Add MonthReadingConsumptionDiffs record
                    var addMonthReadingConsumptionDiffsResult = UpdateMonthReadingConsumptionDiff(MonthReadingId, MeterID, Year, Month, OldConsumption, NewConsumption, oldTarriffAdjustment, tarriffAdjustment, oldConsumptionAdjustment, consumptionAdjustment, consumptionAdjustmentDiff, tarriffAdjustmentDiff);

                    if (addMonthReadingConsumptionDiffsResult > 0)
                    {
                        // Add adjustment difference
                        if (tarriffAdjustmentDiff > (decimal)0.1)
                        {
                            var AddAdjustmentdt = PrepareAddAdjustment(14);
                            decimal MaxInstalmentsAmount = decimal.Parse(AddAdjustmentdt.Rows[0]["MaxInstalmentsAmount"].ToString());
                            int DefaultInstalmentsNumber = int.Parse(AddAdjustmentdt.Rows[0]["DefaultInstalmentsNumber"].ToString());

                            if (AddAdjustmentdt.Rows.Count > 0)
                            {
                                var monthCount = 1;
                                var monthrate = tarriffAdjustmentDiff;

                                if (tarriffAdjustmentDiff > MaxInstalmentsAmount && MaxInstalmentsAmount != 0)
                                {
                                    monthCount = DefaultInstalmentsNumber;
                                    monthrate = tarriffAdjustmentDiff / DefaultInstalmentsNumber;
                                }

                                AddAdjustment(MeterID, tarriffAdjustmentDiff.ToString(), monthCount.ToString(), monthrate.ToString(), AddAdjustmentdt.Rows[0]["AdjustmentType"].ToString(), getDateTime(), AddAdjustmentdt.Rows[0]["adjustmentCode"].ToString(), "100", AddAdjustmentdt.Rows[0]["DefaultAdjustmentReason"].ToString(), 1, MonthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
                            }
                        }
                        else if (tarriffAdjustmentDiff < -(decimal)0.1)
                        {
                            var AddAdjustmentdt = PrepareAddAdjustment(17);

                            if (AddAdjustmentdt.Rows.Count > 0)
                            {
                                var monthCount = 1;
                                var monthrate = -tarriffAdjustmentDiff;

                                AddAdjustment(MeterID, (-tarriffAdjustmentDiff).ToString(), monthCount.ToString(), monthrate.ToString(), AddAdjustmentdt.Rows[0]["AdjustmentType"].ToString(), getDateTime(), AddAdjustmentdt.Rows[0]["adjustmentCode"].ToString(), "100", AddAdjustmentdt.Rows[0]["DefaultAdjustmentReason"].ToString(), 1, MonthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
                            }
                        }

                        // Add adjustment: on difference used money (New meters only)
                        if (consumptionAdjustmentDiff > (decimal)0.1)
                        {
                            var AddAdjustmentdt = PrepareAddAdjustment(10);
                            decimal MaxInstalmentsAmount = decimal.Parse(AddAdjustmentdt.Rows[0]["MaxInstalmentsAmount"].ToString());
                            int DefaultInstalmentsNumber = int.Parse(AddAdjustmentdt.Rows[0]["DefaultInstalmentsNumber"].ToString());

                            if (AddAdjustmentdt.Rows.Count > 0)
                            {
                                var monthCount = 1;
                                var monthrate = consumptionAdjustmentDiff;

                                if (consumptionAdjustmentDiff > MaxInstalmentsAmount && MaxInstalmentsAmount != 0)
                                {
                                    monthCount = DefaultInstalmentsNumber;
                                    monthrate = consumptionAdjustmentDiff / DefaultInstalmentsNumber;
                                }

                                AddAdjustment(MeterID, consumptionAdjustmentDiff.ToString(), monthCount.ToString(), monthrate.ToString(), AddAdjustmentdt.Rows[0]["AdjustmentType"].ToString(), getDateTime(), AddAdjustmentdt.Rows[0]["adjustmentCode"].ToString(), "100", AddAdjustmentdt.Rows[0]["DefaultAdjustmentReason"].ToString(), 1, MonthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
                            }
                        }
                        else if (consumptionAdjustmentDiff < -(decimal)0.1)
                        {

                            var AddAdjustmentdt = PrepareAddAdjustment(16);

                            if (AddAdjustmentdt.Rows.Count > 0)
                            {
                                var monthCount = 1;
                                var monthrate = -consumptionAdjustmentDiff;

                                AddAdjustment(MeterID, (-consumptionAdjustmentDiff).ToString(), monthCount.ToString(), monthrate.ToString(), AddAdjustmentdt.Rows[0]["AdjustmentType"].ToString(), getDateTime(), AddAdjustmentdt.Rows[0]["adjustmentCode"].ToString(), "100", AddAdjustmentdt.Rows[0]["DefaultAdjustmentReason"].ToString(), 1, MonthReadingId, " عن شهر " + monthDate.ToString("MM-yyyy"));
                            }
                        }
                    }
                }


            }
            catch
            {
                SetRecalculateResultTxt("fail to update ID:" + MonthReadingId + System.Environment.NewLine, true);
                SetRecalculateResultTxt("--------------------------------------------" + System.Environment.NewLine, true);
            }

            return true;
        }

        public DataTable GetMeterLastSuccessCharge(string meterId, DateTime specificDate)
        {
            return new dboperation(connectionString).SelectData("select top 1 id ,SerialNo, TotalValue , ChargeValue , ChargeNo,Type,ActivityID,Curdate , PaymentType,serverDate ,TariffStartDate,UnitNo,PhaseNo " +
                                " from charges where meterid = '" + meterId + "' and (makecard = 1 or MakeCard IS NULL) " +
                                 " and  convert(datetime , serverDate, 103 ) < convert(datetime , '" + specificDate.ToString("dd/MM/yyyy") + "' , 103 ) order by Curdate desc ");
        }

        public int UpdateMonthReadingData(int MonthReadingId, decimal UsedMonthly, decimal consumptionAdjustment,decimal tarriffAdjustment)
        {
            try
            {
                return new dboperation(connectionString).ExecuteNonQuery($@"update MonthReadings with (ROWLOCK)  set [Read] = OldConsumption ,TotalConsumption = OldConsumption , ConsumptionMoney = '{UsedMonthly}' ,consumptionAdjustment = {consumptionAdjustment} ,tarriffAdjustment = {tarriffAdjustment} where ID = {MonthReadingId}");
            }
            catch
            {
                return 0;
            }          
        }

        public int UpdateMonthReadingConsumptionDiff(int MonthReadingId, string MeterID, int Year, int Month,decimal OldConsumption,decimal NewConsumption,decimal oldTarriffAdjustment,decimal tarriffAdjustment,decimal oldConsumptionAdjustment,decimal consumptionAdjustment,decimal consumptionAdjustmentDiff,decimal tarriffAdjustmentDiff)
        {
            try
            {
                return new dboperation(connectionString).ExecuteNonQuery($@"insert into MonthReadingConsumptionDiffs(MonthReadingId ,MeterId ,[Year] ,[Month] ,OldConsumption ,NewConsumption ,OldTarriffAdjustment ,NewTarriffAdjustment ,OldConsumptionAdjustment  ,NewConsumptionAdjustment,ConsumptionAdjustmentDiff,TarriffAdjustmentDiff  )
                                              values('{MonthReadingId}','{MeterID}','{Year}','{Month}','{OldConsumption}','{NewConsumption}','{oldTarriffAdjustment}','{tarriffAdjustment}','{oldConsumptionAdjustment}','{consumptionAdjustment}','{consumptionAdjustmentDiff}','{tarriffAdjustmentDiff}')");
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get meter type
        /// </summary>
        /// <param name="meterID">Meter identifier</param>
        /// <returns>Meter fixed fee values</returns>
        public int GetMeterTypeByMeterId(string meterID)
        {
            try
            {
                var query = "SELECT [MeterTypes].[MeterModelVersionID] FROM [dbo].[Meters] INNER JOIN [dbo].[MeterTypes] ON [Meters].[MeterType] = [MeterTypes].[ID] WHERE ([Meters].[MeterID] = '" + meterID + "')";
                return new dboperation(connectionString).ReturnInt(query);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// get last charge date in system 
        /// </summary>
        /// <returns> date time </returns>
        public DateTime getDateTime()
        {
            try
            {
                //var db = new dboperation();
                //var dtAllTime = db.SelectData("select top 1 getdate() as NowDate");
                return DateTime.Now;
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }

        /// <summary>
        /// Prepare add adjustment
        /// </summary>
        /// <param name="adjustmentCode">Adjustment code</param>
        /// <returns>Main data need for add adjustment</returns>
        public DataTable PrepareAddAdjustment(int adjustmentCode)
        {
            return new dboperation(connectionString).SelectData(" select isnull(max(id) + 1 , 1 ) as adjustmentCode ," +
                                 " (select top 1 id from AdjustmentReasons) as DefaultAdjustmentReason ,"+
                                 " (select id from AdjustmentTypes where Code = '" + adjustmentCode + "') as AdjustmentType ,"+
                                 " (select top 1 MaxInstalmentsAmount from settings) as MaxInstalmentsAmount ," +
                                 " (select top 1 DefaultInstalmentsNumber from settings) as DefaultInstalmentsNumber" +
                                 " from Adjustments ");
        }

        public bool AddAdjustment(string MeterID, string AdjustmentValue, string MonthsCount, string MonthlyRate, string AdjustmentType, DateTime CurrentDate, string Code, string Percent, string Reason, int IsActive, int? monthReadingId = null, string description = "")
        {
            bool Saved = false;

            try
            {
                if (decimal.Parse(AdjustmentValue) > 0)
                {
                    try
                    {
                        dboperation db = new dboperation(connectionString);
                        db.objcmd.Parameters.Clear();
                        db.objcmd.CommandType = CommandType.StoredProcedure;
                        db.objcmd.CommandText = "addAdjustment";
                        db.objcmd.Parameters.AddWithValue("@MeterID", MeterID);
                        db.objcmd.Parameters.AddWithValue("@AdjustmenValue", AdjustmentValue);
                        db.objcmd.Parameters.AddWithValue("@MonthsCount", MonthsCount);
                        db.objcmd.Parameters.AddWithValue("@MonthlyRate", MonthlyRate);
                        db.objcmd.Parameters.AddWithValue("@Type", AdjustmentType);
                        db.objcmd.Parameters.AddWithValue("@CurrentDate", getDateFormated(CurrentDate));
                        db.objcmd.Parameters.AddWithValue("@Code", Code);
                        db.objcmd.Parameters.AddWithValue("@Percent", Percent);
                        db.objcmd.Parameters.AddWithValue("@Reason", Reason);
                        db.objcmd.Parameters.AddWithValue("@AdjustmentDescription", description);
                        db.objcmd.Parameters.AddWithValue("@UserID", "");
                        db.objcmd.Parameters.AddWithValue("@monthReadingId", monthReadingId);
                        db.objcmd.Parameters.AddWithValue("@IsActive", IsActive);
                        SqlParameter s = new SqlParameter("@Description", SqlDbType.Text);
                        s.Value = "1";
                        db.objcmd.Parameters.Add(s);
                        db.objcmd.Parameters.AddWithValue("@TransactionID", 1); // add
                        db.objcmd.Parameters.AddWithValue("@ComputerName", Environment.MachineName);

                        if (db.ExecuteNonQuery("") > 0)
                            return true;
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }

                return Saved;
            }
            catch
            {
                return false;
            }
        }

        public string getDateFormated(System.DateTime date)
        {
            try
            {
                return string.Format("{0:D2}/{1:D2}/{2:D4}", date.Month, date.Day, date.Year);
            }
            catch (Exception ex)
            {
                //MakeExceptionLog("Utility", "getDate", ex);
                return "";
            }
        }


        #endregion
    }
}
