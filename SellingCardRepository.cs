namespace MonthReadingRecalculation
{
    using System;
    using System.Data;

    /// <summary>
    /// Selling card repository
    /// </summary>
    public class SellingCardRepository
    {
        dboperation db = new dboperation();

        public string CheckOnlinePayment(string meterId)
        {
            try
            {
                return db.ReturnStr("SELECT top 1 IsOnlinePAymentActivated FROM Meters m with(nolock) " +
                " INNER JOIN Customers c with(nolock) ON m.CustomerId = c.[Id]" +
                " INNER JOIN VendingStations v with(nolock) ON v.Id = c.vendingstationid" +
                " WHERE m.meterId = '" + meterId + "'");
            }
            catch
            {
                return "0";
            }
        }

        public string GetCreditMoney(string VSID)
        {
            return db.ReturnStr("SELECT ISNULL(credit, 0) FROM  ThirdPartyCreditBalance with(nolock) WHERE (stationid = '" + VSID + "')");
        }

        public string GetAreaCompanyCode(string areaNo = "")
        {
            return ((byte)Cards.CardEnums.GetValueFromDescription<CompanyIdEnum>(frmMain.DBNumber) != 0 ? (byte)Cards.CardEnums.GetValueFromDescription<CompanyIdEnum>(frmMain.DBNumber) : byte.MaxValue).ToString();
        }

        public DataTable GetMeterFeeTypeMonthlyPayment(string meterId, string feeTypeID)
        {
            return db.SelectData("SELECT top 1 PaymentDate FROM MonthlyPayments with(nolock) WHERE (MeterID = '" + meterId + "') AND [Type] <>'Adjustment'  AND BillID <>'0' AND TypeID='" + feeTypeID + "'  ORDER BY PaymentDate DESC");
        }

        public bool GetMeterFeeTypeMonthlyPaymentByCode(string meterId, string feeTypeID, string code, string date)
        {
            return db.SelectData("SELECT ID FROM MonthlyPayments with(nolock) WHERE (MeterID = '" + meterId + "') " +
                                "  AND (TypeID = " + feeTypeID + ") AND (PaymentDate = CONVERT(datetime,'" + date + "', 103))" +
                                " AND (Code = '" + code + "')").Rows.Count > 0;
        }

        public string GetFeeMonthlyPayment(string FeeID)
        {
            return db.ReturnStr("SELECT TypeID FROM MonthlyPayments with(nolock) WHERE (ID = " + FeeID + ")");
        }

        public DataTable GetLastUserCharge(string userId)
        {
            return db.SelectData("SELECT Top 1 ValidateKey,TotalValue,MeterID,SerialNo,Curdate,UserID,VendingStation FROM Charges WHERE UserID='" + userId + "' ORDER BY ID DESC");
        }

        public string GetMeterWorkingdate(string meterId)
        {
            return db.ReturnStr("select workingdate from meters with(nolock) where MeterID = '" + meterId + "'");
        }

        public bool CheckIsFeeTypesFirstTimeMeter(string meterId, string feeID)
        {
            return db.SelectData("select MeterID FROM FeeTypesFirstTime with(nolock) WHERE (MeterID = '" + meterId + "') AND (FeeID = " + feeID + ")").Rows.Count > 0;
        }

        public bool CheckIsFeeTypesFirstTimeMeter(string meterId, string feeID, string feeTypeID)
        {
            return db.SelectData("select MeterID FROM FeeTypesFirstTime with(nolock) WHERE (MeterID = '" + meterId + "') AND (FeeID = " + feeID + ")" + " and feetable = '" + feeTypeID + "'").Rows.Count > 0;
        }

        public bool CheckIsFeeDistrictsMeter(string meterId, string feeTypeID)
        {
            return db.SelectData("SELECT DISTINCT Meters.MeterID FROM Meters with(nolock) INNER JOIN Concentrators with(nolock) ON " +
                                " Meters.ConcentratorID = Concentrators.ID INNER JOIN  FeeTypesDistricts with(nolock) INNER JOIN district with(nolock) ON " +
                                " FeeTypesDistricts.DistrictID = district.ID INNER JOIN Transformers with(nolock) ON district.ID = Transformers.DistrictID  " +
                                " ON Concentrators.TransformerID = Transformers.ID WHERE (Meters.MeterID = '" +
                                meterId + "') AND (FeeTypesDistricts.FeeTypeID ='" + feeTypeID + "')").Rows.Count == 0;
        }

        public DataTable GetMeterChargeBySerialNo(string meterId, string replaceMeterPrefix)
        {
            return db.SelectData("SELECT Top 1 ISNULL(ServerDate,GETDATE()) FROM Charges with(nolock) WHERE MeterID='" + meterId + "'  and serialno not like '" + replaceMeterPrefix + "%' order by serverdate desc");
        }

        public DataTable GetMeterChargeByMeterId(string meterId)
        {
            return db.SelectData("SELECT Top 1 ISNULL(ServerDate,GETDATE()) FROM Charges with(nolock) WHERE MeterID='" + meterId + "' and makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  order by serverdate desc");
        }

        public DataTable GetMeterMonthReadingNotUsedForEveryCharge(string meterId)
        {
            return db.SelectData(" SELECT [READ],[Year],[Month] FROM MonthReadings with(nolock) WHERE (UsedForEveryCharge = 0) AND (MeterID = '" + meterId + "' ) ");
        }

        public DataTable GetMeterMonthReadingNotUsedForEveryCharge(string meterId, int month, int year)
        {
            return db.SelectData(" SELECT [READ],[Year],[Month] FROM MonthReadings with(nolock) WHERE (Used = 0) AND (MeterID = '" + meterId + "' and month =" + month + " and year =" + year + " ) ");
        }

        public string GetFeeTypedetails(string feeId, string monthReading)
        {
            return db.ReturnStr("SELECT ISNULL((SELECT Fees AS Fees FROM FeeTypesDetails with(nolock) WHERE (FeeTypeID = " + feeId + ") AND (" + monthReading + " >= Fromkwh) AND (" + monthReading + " <= Tokwh)), 0) AS Reading");
        }

        public DataTable GetMeterFeeTypes(string type, string meterId)
        {
            return db.SelectData("SELECT ID, Name, Code, Feevalue, ValueOptions, Activity, MeterType, UseDistricts FROM FeeTypes with(nolock) " +
               " WHERE (PaymentType = '" + type + "') AND (IsDeleted = 0) AND (FeeFlag <> 1) " +
               " And (Active = 1) AND Activity= (SELECT ActivityID FROM Meters with(nolock) WHERE (MeterID = '" + meterId + "'))");
        }

        public DataTable GetMeterDetails(string period, string meterType, string activity, string meterId)
        {
            return db.SelectData("SELECT (MONTH(GETDATE()) - MONTH(WorkingDate)) % " + period + " AS month, MeterID,AccountNo FROM Meters with(nolock) WHERE (IsDeleted <> 1) " +
                " AND (MeterType in (SELECT ID FROM MeterTypes with(nolock) WHERE (MeterTypeID = '" + meterType + "') ) ) AND ActivityID='" + activity + "' and MeterID='" + meterId + "' and WorkingDate < GETDATE()");
        }

        /// <summary>
        /// Gets adjustment by identifier
        /// </summary>
        /// <param name="adjustmentId">Adjustment identifier</param>
        /// <returns>result</returns>
        public DataTable GetAdjustmentById(string adjustmentId)
        {
            return db.SelectData(" SELECT Remminder ,  totalvalue , monthscount ,MonthlyRate , PaidMonths from adjustments with(nolock) where id = " + adjustmentId);
        }

        public DataTable GetFeeTypeServerDate(string feeTypeId)
        {
            return db.SelectData("select ServerDate from dbo.FeeTypes with(nolock) where id = " + feeTypeId);
        }

        public DataTable GetAdjustmentDueDate(string adjustmentId)
        {
            return db.SelectData("select DueDate from dbo.Adjustments with(nolock) where id = " + adjustmentId);
        }

        public DataTable GetMeterChargeBySerialNo(int makeCardId, string meterId, string replaceMeterPrefix)
        {
            return db.SelectData("SELECT Top 1 ISNULL(ServerDate,GETDATE()) FROM Charges with(nolock) WHERE MeterID='" + meterId +
                        "' and makecard <> '" + makeCardId + "' and serialno not like '" + replaceMeterPrefix + "%' order by serverdate desc");
        }

        public int UpdateChargeMakeCard(string make, string RecieptNo)
        {
            return db.ExecuteNonQuery(" update charges with(Rowlock) set MakeCard= " + make + "   where ID = " + RecieptNo);
        }

        public int UpdateChargeMakeCard(string make, string RecieptNo, int chargeMethod)
        {
            return db.ExecuteNonQuery(" update charges with(Rowlock) set MakeCard= " + make + " , ChargeMethod = '" + chargeMethod + "' where ID = " + RecieptNo);
        }

        public int UpdateVendingStations(bool offlineMode, string vendingStationID)
        {
            string query = "";

            if (offlineMode)
            {
                query = "update VendingStations with(Rowlock) set offlinemode = 0 ,  OfflineTransaction = isnull(OfflineTransaction , 0 ) + 1 where id ='" +
                                  vendingStationID + "'";
            }
            else
            {
                query = "update VendingStations with(Rowlock) set offlinemode = 0 ,  OfflineTransaction = 0 where id ='" +
                                    vendingStationID + "'";
            }

            return db.ExecuteNonQuery(query);
        }

        public int UpdateVendingStationOfflineMode(string vendingStationID)
        {
            return db.ExecuteNonQuery("update VendingStations with(Rowlock) set offlinemode = 1 where id ='" + vendingStationID + "'");
        }

        public string GetCustomerVendingStations(string customerId)
        {
            return db.GetSingleValue("select VendingStationid from Customers with(nolock) where ID = '" + customerId + "'")?.ToString();
        }

        public int GetVendingStationsExceedMaxTransactionsCount(string vendingStationID)
        {
            return db.ReturnInt(" select count(ID) from VendingStations with(nolock) where ( (OfflineTransaction >= (select MaxOfflineTransaction from Settings)) or   offlinemode = 1 ) and id ='" + vendingStationID + "'");
        }

        public int GetVendingStationsOfflineCount(string vendingStationID)
        {
            return db.ReturnInt(" select count(ID) from VendingStations with(nolock) where offlinemode = 1 and id ='" + vendingStationID + "'");
        }

        public DataTable GetMeterMaxChargeDetails(string meterId)
        {
            return db.SelectData("select id ,  isnull( MakeCard , 0 ) as MakeCard , TotalValue , ChargeValue from charges with(nolock) where meterid = '" + meterId + "' and " +
                                   " serverDate = (select max(serverDate) from charges with(nolock) where meterid = '" + meterId + "')");
        }

        /// <summary>
        /// Get meter last success charge
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>meter last success charge</returns>
        public DataTable GetMeterLastSuccessCharge(string meterId)
        {
            var dt = db.SelectData(" select top 1 charges.id,charges.SerialNo, TotalValue , ChargeValue , charges.ChargeNo,Type,charges.ActivityID,charges.Curdate , " +
                                   " PaymentType,charges.serverDate ,charges.TariffStartDate,charges.UnitNo,charges.PhaseNo,getdate() as currentServerDate,ACT.AccountCode as ActivityAccountCode, " +
                                   " charges.DepartmentID,m.DepartmentID as meterDepartmentID " +
                                   " from charges inner join Activities ACT with(nolock) on charges.ActivityID = ACT.ID" +
                                   " inner join meters m with(nolock) on charges.meterid = m.meterid where charges.meterid = '" + meterId + "' and (makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  or MakeCard IS NULL) " +
                                   " and((select count(id) from charges with(nolock) where type = 0 and meterid = '" + meterId + "') = 0 or serverDate >= (select max(serverDate) from charges where type = 0 and meterid = '" + meterId + "') ) order by Curdate desc ");

            if (dt.Rows.Count > 0)
            {
                Utility.cashedServerDateTime = Convert.ToDateTime(dt.Rows[0]["currentServerDate"]);
                Utility.cashedServerDateTimeFormated = string.Format("{0:D2}/{1:D2}/{2:D4}", Utility.cashedServerDateTime.Day, Utility.cashedServerDateTime.Month, Utility.cashedServerDateTime.Year);
            }

            return dt;
        }

        /// <summary>
        /// Get meter last success charge software version
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>meter last success charge software version</returns>
        public DataTable GetMeterLastSuccessChargeVersion(string meterId)
        {
            return db.SelectData(" select top 1 id ,Softwareversion,(select max(serverDate) from charges where type = 0 and meterid = '" + meterId + "') as serverDate from charges "+
                                   " where meterid = '" + meterId + "' and (makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  or MakeCard IS NULL) " +
                                   " and((select count(id) from charges with(nolock) where type = 0 and meterid = '" + meterId + "') = 0 or serverDate >= (select max(serverDate) from charges where type = 0 and meterid = '" + meterId + "') ) order by Curdate desc ");
        }

        /// <summary>
        /// Get meter last success charge after specific date
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>meter last success charge</returns>
        public DataTable GetMeterLastSuccessCharge(string meterId, DateTime specificDate)
        {
            return db.SelectData("select top 1 id ,SerialNo, TotalValue , ChargeValue , ChargeNo,Type,ActivityID,Curdate , PaymentType,serverDate ,TariffStartDate,UnitNo,PhaseNo " +
                                " from charges where meterid = '" + meterId + "' and (makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  or MakeCard IS NULL) " +
                                 " and  convert(datetime , serverDate, 103 ) < convert(datetime , '" + specificDate.ToString() + "' , 103 ) order by Curdate desc ");
        }

        /// <summary>
        /// Get meter first success charge after specific date
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>meter last success charge</returns>
        public DataTable GetMeterFirstSuccessChargeAfterDate(string meterId, DateTime specificDate)
        {
            return db.SelectData("select top 1 id ,SerialNo, TotalValue , ChargeValue , ChargeNo,Type,ActivityID,Curdate , PaymentType,serverDate ,TariffStartDate,UnitNo,PhaseNo " +
                                " from charges with(nolock) where meterid = '" + meterId + "' and (makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  or MakeCard IS NULL) " +
                                 " and  convert(datetime , serverDate, 103 ) >= convert(datetime , '" + specificDate.ToString() + "' , 103 ) order by Curdate ");
        }

        /// <summary>
        /// Get meter card identifier
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>Card identifier</returns>
        public string GetMeterCardId(string meterId)
        {
            return db.ReturnStr("select CardID from meters with(nolock) where MeterID = '" + meterId + "'");
        }

        /// <summary>
        /// Get meter card charge number
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>Card charge number</returns>
        public int GetMeterCardChargeNo(string meterId)
        {
            return db.ReturnInt("select cardchargeno from meters with(nolock) where meterid = '" + meterId + "'");
        }

        /// <summary>
        /// Update meter card charge number
        /// </summary>
        /// <param name="CardChargeNo">charge number</param>
        /// <param name="meterId">meter identifier</param>
        /// <returns>Update result</returns>
        public int UpdateMeterChargeNo(string CardChargeNo, string meterId)
        {
            return db.ExecuteNonQuery(" update Meters with(Rowlock) set CardChargeNo= " + CardChargeNo + " where MeterID='" + meterId + "'");
        }

        /// <summary>
        /// Get meter closed reason
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>closed reason</returns>
        public string GetMeterClosedReason(string meterId)
        {
            return db.ReturnStr("SELECT CloseReasons.Name FROM ClosedCustomers with(nolock) INNER JOIN CloseReasons with(nolock) ON ClosedCustomers.Reason = CloseReasons.ID WHERE (ClosedCustomers.Reason <> N'1'and MeterID = '" + meterId + "')");
        }

        /// <summary>
        /// Get meter customer migration status
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>closed reason</returns>
        public string GetMeterCustomerMigrationStatus(string meterId)
        {
            return db.ReturnStr("Select C.isMigrated from Customers C with(nolock) inner join Meters M with(nolock) on C.ID = M.CustomerId where M.MeterID = '" + meterId + "'");
        }


        /// <summary>
        /// Check if not migrated customer
        /// </summary>
        /// <param name="MeterID">meter identifier</param>
        /// <returns>closed status</returns>
        public bool CheckCustomerIsMigrated(string MeterID)
        {
            try
            {
                string customerStatus = GetMeterCustomerMigrationStatus(MeterID);

                if (customerStatus.ToLower() == "true" || customerStatus == "1")
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }


        /// <summary>
        /// Get meter reading till time
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <param name="time">time</param>
        /// <returns>meter reading till time</returns>
        public DataTable GetMeterReadingTillTime(string meterId, string time)
        {
            return db.SelectData("select  top 1  convert( nvarchar , aQuantityTotal ) as Prev,   GuCode , Sewage  from WaterMetersReadings with(nolock) where MeterID = '" + meterId +
              "' and  year(convert(datetime ,  aSysTime , 103) ) * 10000 + month(convert(datetime ,  aSysTime , 103) )* 100 + day(convert(datetime ,  aSysTime , 103) )   <  " + time +
              " order by convert(datetime ,  aSysTime , 103)  desc  ");
        }

        /// <summary>
        /// Get meter reading quantity till time
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <param name="time">time</param>
        /// <returns>Quantity difference</returns>
        public string GetMeterReadingDifferenceWithTime(string meterId, string time)
        {
            return db.ReturnStr("select convert(nvarchar ,  sum( aQuantityTotal - PrevQuantityTotal)  )  from WaterMetersReadings with(nolock) where MeterID = '" + meterId +
                  "' and  convert(datetime ,  aSysTime , 103)   <  convert(datetime , '" + time +
                  "'  , 103) and year (convert(datetime ,  aSysTime , 103) ) *100 + MONTH( convert(datetime ,  aSysTime , 103) ) = " +
                  " year (convert(datetime ,  '" + time + "' , 103)  ) *100 + MONTH( convert(datetime ,  '" + time + "' , 103)  ) ");
        }


        /// <summary>
        /// Get meter adjustments
        /// </summary>
        /// <param name="meterId">Meter identifier</param>
        /// <returns>Meter adjustments</returns>
        public DataTable GetMeterAdjustments(string meterId)
        {
            return db.SelectData(" SELECT a.ID,a.Code,t.ValueOptions, a.MonthlyRate, t.Name + ' ' + a.Description as Name, t.PaymentType,t.Sign,a.PercentValue,a.Remminder,a.MonthsCount, a.PaidMonths,a.DueDate" +
                                  " FROM Adjustments a INNER JOIN AdjustmentTypes t with(nolock) ON a.Type = t.ID " +
                                  " INNER JOIN Meters m with(nolock) on m.MeterID = a.MeterID" +
                                  " INNER JOIN MeterTypes mt with(nolock) on m.MeterType = mt.ID WHERE ( " +
                                  "  (mt.MeterModelVersionID in (8, 9, 10, 11))" +
                                  "  or (mt.MeterModelVersionID not in (8, 9, 10, 11) and((t.Code is null) or (t.Code is not null AND t.Code <> '" + (int)AdjustmentTypeEnum.MeterPrice + "'))))" +
                                  " AND(a.ActiveAd = 1) AND(a.Isdeleted <> 1) AND (m.MeterID = '" + meterId + "')  AND cast(CONVERT(date, a.DueDate,103) as datetime) <= GETDATE()");
        }

        /// <summary>
        /// Get meter adjustment which is (send to meter only)
        /// </summary>
        /// <param name="MeterId">Meter identifier</param>
        /// <returns>Meter adjustment</returns>
        public DataTable GetSendToMeterAdjustment(string MeterId)
        {
            return db.SelectData(" select top 1 adj.MonthlyRate as DeductionValue , adj.MonthsCount , adj.CurrentDate as CreateDate , DueDate, adj.ID,adj.PaidMonths from [dbo].[Adjustments] adj inner join [dbo].[AdjustmentTypes] adjType with(nolock) on adj.[Type] = adjType.[ID]" +
                                 " inner join [dbo].[AdjustmentCodes] adjCode with(nolock) on adjType.[Code] = adjCode.Code" +
                                 " where adj.ActiveAd = 1 AND adj.Isdeleted <> 1 and adjCode.[SendToMeter] = 1 and adjCode.Code = '" + (int)AdjustmentTypeEnum.MeterPrice + "' and adj.MeterID = '" + MeterId + "'" +
                                 " order by adj.CurrentDate desc");
        }


        /// <summary>
        /// Get closed deduction for meter
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns></returns>
        public DataTable GetClosedDeduction(string meterId)
        {
            return db.SelectData(" select top 1 adj.MonthlyRate as DeductionValue , adj.MonthsCount , adj.CurrentDate as CreateDate , DueDate, adj.ID,adj.PaidMonths from [dbo].[Adjustments] adj inner join [dbo].[AdjustmentTypes] adjType with(nolock) on adj.[Type] = adjType.[ID]" +
                            " inner join [dbo].[AdjustmentCodes] adjCode with(nolock) on adjType.[Code] = adjCode.Code" +
                            " where adj.ActiveAd = 0 AND adj.Isdeleted = 1 and adjCode.[SendToMeter] = 1 and adjCode.Code = '" + (int)AdjustmentTypeEnum.MeterPrice + "' and adj.MeterID = '" + meterId + "'" +
                            " order by adj.CurrentDate desc");
        }

        /// <summary>
        /// Get meter month deduction 
        /// </summary>
        /// <param name="MeterId">Meter identifier</param>
        /// <param name="month">Month of date that meter deduct</param>
        /// <param name="year">Year of date that meter deduct</param>
        /// <returns></returns>
        public string GetMeterMonthDeduction(string MeterId, int month, int year)
        {
            return db.ReturnStr(@"select PaidValue from AdjustmentPayments ap
                                   join Adjustments a on ap.Ajustment = a.ID
                                   where  ap.IsDeductFromMeter = 1
                                  and  MONTH(ap.CurrentDate) = " + month + " AND YEAR(ap.CurrentDate) = " + year + " and a.MeterID = '" + MeterId + "'");
        }

        public DataTable GetMeterFees(string meterId)
        {
            return db.SelectData(" SELECT f.ID, f.Name, f.ValueOptions, f.Code, f.PaymentType, f.Feevalue,f.UseDistricts , f.sequence,f.accumulated , f.feeflag,f.WithoutTax" +
                                 " FROM FeeTypes f INNER JOIN Meters m with(nolock) ON f.Activity = m.ActivityID INNER JOIN Activities A ON f.Activity = A.ID" +
                                 " WHERE (m.MeterID = '" + meterId + " ') AND (f.IsDeleted = 0) And (f.Active = 1) and FeeFlag <> 1 AND f.ServerDate <= GetDate() " +
                                 " order by f.sequence , f.accumulated");
        }

        public DataTable GetMeterFeeTypes(string meterId)
        {
            return db.SelectData(" SELECT f.ID, f.Name, f.ValueOptions, f.Code, f.PaymentType, f.Feevalue,f.UseDistricts , f.sequence,f.accumulated  , f.feeflag" +
                                 " FROM FeeTypes f INNER JOIN Meters m with(nolock) ON f.Activity = m.ActivityID INNER JOIN Activities a on f.Activity = a.ID INNER JOIN MeterTypes with(nolock) ON m.MeterType = MeterTypes.ID and (f.MeterDimension = MeterTypes.MeterType or a.IsResidential = 1) " +
                                 " WHERE (m.MeterID = '" + meterId + " ') AND (f.IsDeleted = 0) And (f.Active = 1) and (FeeFlag <> 1)" +
                                 " order by f.sequence , f.accumulated");
        }

        public int GetMeterReadingTimeLessThanSpecifiedTime(string meterId, string time)
        {
            return db.ReturnInt("select top 1 aSysTimeInt from WaterMetersReadings with(nolock) where MeterID = '" + meterId + "' and aSysTimeInt < " + time + " order by aSysTimeInt desc ");
        }

        public string GetMeterReadingQuantityByTime(string meterId, string time)
        {
            return db.ReturnStr("select aQuantityTotal from WaterMetersReadings with(nolock) where MeterID = '" + meterId + "' and aSysTimeInt = " + time);
        }

        public bool GetMeterDistrictDetails(string meterId, string feeTypeId)
        {
            string DistrictQuery = "SELECT DISTINCT Meters.MeterID, Transformers.Name FROM Meters with(nolock) INNER JOIN Concentrators with(nolock) ON " +
                            " Meters.ConcentratorID = Concentrators.ID INNER JOIN  FeeTypesDistricts with(nolock) INNER JOIN district with(nolock) ON " +
                            " FeeTypesDistricts.DistrictID = district.ID INNER JOIN Transformers with(nolock) ON district.ID = Transformers.DistrictID  " +
                            " ON Concentrators.TransformerID = Transformers.ID WHERE (Meters.MeterID = '" +
                            meterId + "') AND (FeeTypesDistricts.FeeTypeID ='" + feeTypeId + "')";

            return db.SelectData(DistrictQuery).Rows.Count == 0;
        }

        public DataTable GetFeeTypeDetails(string feeTypeId)
        {
            string query = "SELECT Fromkwh as [from] , Tokwh as [to] , Fees  as value , activities.dreditamt as friendly " +
                           " FROM  FeeTypesDetails join dbo.FeeTypes on FeeTypeID = dbo.FeeTypes.id " +
                           " join activities with(nolock) on FeeTypes.Activity = activities.id WHERE  FeeTypeID = " + feeTypeId;
            return db.SelectData(query);
        }

        public int GetVendingStationsOfflineCount(string meterId, string monthdate, string FeeID, string feeTypeID)
        {
            string query = "select COUNT(ChargeID) from dbo.ChargesDetails where convert( nvarchar(50), SerialNu ) in (select SerialNo from Charges where MeterID = '" + meterId + "'  and makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  ) ";
            query = query + "and convert(datetime , FeeDate, 103 ) >= convert(datetime , '" + monthdate + "' , 103 ) ";
            query = query + "and FeeID = " + FeeID + " and feetable = '" + feeTypeID + "'";

            return db.ReturnInt(query);
        }

        public string GetUserNameById(short userId)
        {
            return db.SelectName("Users", "Name", "ID", userId);
        }

        public int GetChargesCount(string meterId, string time, bool egyptTender)
        {
            string query = "select COUNT(ID) from Charges where Curdate >= convert(datetime , '" + time + "', 103 ) and MeterID = '" + meterId + "'"
             + "and ChargeNo > " + (egyptTender ? " 0 " : " 1 ").ToString();
            return db.ReturnInt(query);
        }

        public int UpdateMeterMonthReading(string meterId, int status)
        {
            return db.ExecuteNonQuery("UPDATE MonthReadings with(Rowlock) SET USED = " + status + " WHERE  (MeterID = '" + meterId + "' )");
        }

        /// <summary>
        /// Check customer in black list customers
        /// </summary>
        /// <param name="customerName">Customer name</param>
        /// <returns>result</returns>
        public bool CheckInBlackListCustomers(string customerName)
        {
            return db.SelectData("SELECT ID FROM BlackListCustomers with(nolock) INNER JOIN Customers with(nolock) ON BlackListCustomers.CustomerID = Customers.ID WHERE (Customers.Name = '" + customerName + "')").Rows.Count > 0;
        }

        /// <summary>
        /// Get meter data
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>meter data</returns>
        public DataTable GetMeterData(string meterId)
        {
            db.objcmd.CommandText = "GetMeterData";
            db.objcmd.CommandType = CommandType.StoredProcedure;
            db.objcmd.Parameters.Clear();
            db.objcmd.Parameters.AddWithValue("@MeterID", meterId);
            return db.SelectData("");
        }

        public int GetMeterValueByColName(string colName, string meterId)
        {
            return db.ReturnInt("SELECT " + colName + " FROM  Meters with(nolock) where MeterID='" + meterId + "'");
        }

        public DataTable GetUserData(string userId)
        {
            return db.SelectData("SELECT Name,ID FROM Users with(nolock) WHERE ( id = '" + userId + "')");
        }

        /// <summary>
        /// Updates adjustment
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="adjustmentId">Adjustment identifier</param>
        ///  <param name="currentDate">Date of pay adjustment</param>
        /// <param name="IsDeductFromMeter">Dedicate adustment paid from meter or system</param>
        /// <returns>result</returns>
        public bool UpdateAdjustment(string value, string adjustmentId, DateTime currentDate, bool IsDeductFromMeter = false)
        {
            DB_Adjustments DBobj = new DB_Adjustments();
            return DBobj.AddAdjustmentPayment(adjustmentId, value, "0", "0", string.Empty, currentDate, IsDeductFromMeter);
        }

        /// <summary>
        /// Update adjustment active status
        /// </summary>
        /// <param name="adjustmentId">Adjustment identifier</param>
        /// <param name="activeStatus">Active status</param>
        /// <returns>result</returns>
        public int UpdateAdjustmentActive(string adjustmentId, int activeStatus)
        {
            return db.ExecuteNonQuery(" UPDATE Adjustments with(Rowlock) SET Activead = " + activeStatus + " WHERE (ID = '" + adjustmentId + "')");
        }

        public int UpdateAdjustmentReminder(string adjustmentId, string value, int paidMonths)
        {
            return db.ExecuteNonQuery("UPDATE Adjustments with(Rowlock) SET Remminder = Remminder - " + value + ", PaidMonths = PaidMonths + " + paidMonths + " WHERE (ID = " + adjustmentId + ")");
        }

        public string GetAdjustmentReminder(string adjustmentId)
        {
            return db.ReturnStr("SELECT Remminder FROM adjustments WHERE id =" + adjustmentId);
        }

        public int GetInActiveAdjustments(string adjustmentId)
        {
            return db.ReturnInt("SELECT count(id) FROM adjustments WHERE Activead = 0 and id =" + adjustmentId);
        }

        public DataTable GetChargesBySerialNo(string serialNo)
        {
            return db.SelectData("Select ID FROM charges with(nolock) WHERE serialno ='" + serialNo + "'");
        }

        public string GetVendingStationBySerialNo(string serialNo)
        {
            return db.ReturnStr("SELECT ID FROM VendingStationDetails with(nolock) WHERE (SerialNo = '" + serialNo + "')");
        }

        public DataTable GetVendingStationDetailsBySerialNo(string serialNo)
        {
            string query = "SELECT '' AS ChargeID, '' AS FeeID, '' AS FeeDate, Charges.SerialNo AS Name, Charges.TotalValue AS FeeValue, '' AS Feetable " +
                           " FROM VendingStationDetails with(nolock) INNER JOIN Charges with(nolock) ON VendingStationDetails.SerialNo = Charges.ID " +
                           " WHERE (VendingStationDetails.ID = " + serialNo + ")";
            return db.SelectData(query);
        }

        public DataTable GetChargeDetailsById(string chargeId)
        {
            string query = "Select Distinct * FROM ChargesDetails with(nolock) inner join charges with(nolock) on charges.SerialNo =ChargesDetails.SerialNu " +
                            " WHERE charges.ID ='" + chargeId + "'";
            return db.SelectData(query);
        }

        public DataTable GetMeterChargeAndActivityDetails(string meterId, string chargeId)
        {
            string query = "SELECT district.Name AS District, Activities.Name AS Activity, Charges.AccountNo, Meters.displaymeterid as IDMeter " +
                       " FROM Activities with(nolock) INNER JOIN Charges ON Activities.ID = Charges.ActivityID INNER JOIN " +
                       " Concentrators ON Charges.ConcentratorID = Concentrators.ID with(nolock) INNER JOIN " +
                       " Meters with(nolock) ON Charges.MeterID = Meters.MeterID INNER JOIN district with(nolock) INNER JOIN " +
                       " Transformers with(nolock) ON district.ID = Transformers.DistrictID ON Concentrators.TransformerID = Transformers.ID " +
                       " WHERE Meters.DisplayMeterID= '" + meterId + "' and charges.ID ='" + chargeId + "'";
            return db.SelectData(query);
        }

        public DataTable GetSetting(string company, string region, string district, string department)
        {
            return db.SelectData("Select Top 1 '" + company + "' CompanyTitle,'" + region + "' Region,'" + district + "' District, '" + department + "' Department ,CompanyLogo FROM settings with(nolock)");
        }

        public DataTable GetMeterChargeNotLikeSerialNo(string meterId, string serialNo)
        {
            return db.SelectData("SELECT TOP (2) Charges.ServerDate FROM Charges inner join meters with(nolock) on meters.meterid = Charges.meterid WHERE (Meters.DisplayMeterID= '" + meterId + "')  and serialno not like '" + serialNo + "%' ORDER BY Charges.ServerDate DESC");
        }

        public DataTable GetMeterReading(string meterId)
        {
            return db.SelectData("SELECT TOP (2) TotalConsumtionEnergy, ServerDate FROM MetersReadings inner join meters with(nolock) on meters.meterid = metersreadings.meterid WHERE (Meters.DisplayMeterID= '" + meterId + "') ORDER BY ServerDate DESC");
        }

        public string GetMeterCustomer(string meterId)
        {
            return db.ReturnStr("SELECT DISTINCT CustomerTypes.TypeNo FROM Meters with(nolock) INNER JOIN Customers with(nolock) ON Meters.CustomerId = Customers.ID INNER JOIN CustomerTypes with(nolock) ON Customers.CustomerType = CustomerTypes.ID WHERE (Meters.DisplayMeterID= '" + meterId + "')");
        }

        public string GetSettingCountry()
        {
            return db.ReturnStr("Select CurrenyStr from settings");
        }

        public string GetChargeValueByMeterId(string meterId)
        {
            return db.ReturnInt("select ChargeValue from Charges where MeterID='" + meterId + "' ORDER BY ID DESC ").ToString();
        }

        public string GetMeterConcentratorId(string meterId)
        {
            return db.ReturnStr("select ConcentratorID from Meters with(nolock) where ID='" + meterId + "'");
        }

        /// <summary>
        /// Get meter concentrators
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>meter concentrators</returns>
        public DataTable GetMeterConcentratorDetails(string meterId)
        {
            return db.SelectData("select Name,ID from Concentrators with(nolock) where  ID in (select ConcentratorID from Meters where MeterID='" + meterId + "')");
        }

        /// <summary>
        /// Checks the status of a meter.
        /// </summary>
        /// <param name="meterId">The ID of the meter to check.</param>
        /// <returns>True if the meter with the specified ID exists and has a status of 'On Customer'; otherwise, false.</returns>
        public string CheckMeterStatus(string meterId)
        {
            // Returns true if a meter with the specified ID exists and has a status of 'On Customer'; otherwise, false.
            return db.ReturnStr($"SELECT MeterStatus.ArabicName FROM Meters WITH(NOLOCK) INNER JOIN MeterStatus WITH(NOLOCK) ON Meters.Status =  MeterStatus.Name WHERE MeterID = '{meterId}' AND Status <> 'On Customer'");
        }


        /// <summary>
        /// Get OnCustomer meters
        /// </summary>
        /// <param name="meterId">meter identifier</param>
        /// <returns>OnCustomer meter</returns>
        public DataTable GetOnCustomerMeterDetails(string meterId)
        {
            return db.SelectData("select DisplayMeterID,MeterID from Meters with(nolock) where MeterID ='" + meterId + "'  and Status='On Customer' and isdeleted <> 1");
        }

        public DataTable GetAllBanks()
        {
            return db.SelectData("select ID,Name from banks with(nolock)");
        }

        public DataTable GetBankById(string bankID)
        {
            return db.SelectData($"SELECT ID, Name FROM Banks WITH(NOLOCK) WHERE ID = '{bankID}'");
        }

        public DataTable GetMeterByConcentratorDetails(int operation, int concentratorIndex, string concentratorValue)
        {
            string query = "select DisplayMeterID,MeterID from Meters with(nolock) where ID<>0 and (isdeleted <> 1)";

            switch (operation)
            {
                case 2:
                    query += " and AMR=1 and (ChargeNo>0 or cardChargeNo>0 )";
                    break;
                case 3:
                    query += " and SmartCard=1 and cardChargeNo>0";
                    break;
                default:
                    break;
            }

            if (concentratorIndex != -1)
                query += " And ConcentratorID='" + concentratorValue + "'";

            query += " order by DisplayMeterID";
            return db.SelectData(query);
        }

        public int Rollback()
        {
            return db.ExecuteNonQuery("rollback");
        }

        /// <summary>
        /// In case of pending charge update receipt image column in  
        /// </summary>
        /// <param name="chargeId">charge Id</param>
        /// <param name="meterId">Id of meter</param>
        /// <param name="image">receipt of image</param>
        /// <param name="correctorUserID">User id that correct pending station </param>
        /// <param name="correctorVendingstation">Name of vending station</param>
        /// <returns>Integer number that determine successfully or not</returns>
        public int UpdateChargeReceiptImage(string chargeId, string meterId, string image, string correctorUserID, string correctorVendingstation)
        {
            return db.ExecuteNonQuery("update charges with(Rowlock) set ReceiptImage = '" + @image + "',IsFree =1,CorrectorUserID='" + correctorUserID + "',CorrectorVendingstation='" + correctorVendingstation + "'  where Id = '" + chargeId + "' and MeterID = '" + meterId + "' and type = 1 and makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  ");
        }

        /// <summary>
        /// Get payment type of last charge of meter
        /// </summary>
        /// <param name="meterId">Id of meter</param>
        /// <param name="chargeNo"> Charge no </param>
        /// <param name="image">receipt of image</param>
        /// <returns>Name of payment type</returns>
        public string GetChargePaymentType(string meterId, string chargeNo)
        {
            return db.ReturnStr("select PaymentType from Charges with(nolock) where MeterID = '" + meterId + "' and ChargeNo=" + (int.Parse(chargeNo) - 1).ToString());
        }

        /// <summary>
        /// Get same month paid fees count
        /// </summary>
        /// <param name="meterId">Meter identifier</param>
        /// <param name="monthdate">Month date</param>
        /// <param name="FeeID">Fee identifier</param>
        /// <param name="feeTypeID">Fee type</param>
        /// <returns>Count</returns>
        public int GetSameMonthPaidFeesCount(string meterId, string monthdate, string FeeID, string feeTypeID)
        {
            string query = " SELECT COUNT(ChargeID) from dbo.ChargesDetails with(nolock) where SerialNu in " +
                           " (select SerialNo from Charges with(nolock) inner join Meters with(nolock) on Charges.MeterID = Meters.MeterID and Charges.CustomerID = Meters.CustomerId and Charges.MeterID = '" + meterId + "' and Charges.makecard = "+ ((int)ChargeStatusEnum.Success).ToString() + "  )" +
                           " and convert(datetime , FeeDate, 103 ) >= convert(datetime , '" + monthdate + "' , 103 ) " +
                           " and FeeID = " + FeeID + " and feetable = '" + feeTypeID + "'";
            return db.ReturnInt(query);
        }

        /// <summary>
        /// Validate activity exists in db
        /// </summary>
        /// <param name="Activity">Activity identifier</param>
        /// <returns>Status</returns>
        public bool ValidateActivityExists(string Activity)
        {
            return db.SelectData("select ID FROM Activities with(nolock) WHERE (ID = '" + Activity + "')").Rows.Count > 0;
        }
    }
}