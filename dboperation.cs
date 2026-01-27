namespace MonthReadingRecalculation
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Windows.Forms;

    /// <summary>
    /// Summary description for dboperation
    /// </summary>
    public class dboperation
    {
        public SqlCommand objcmd;
        private SqlConnection connection;
        private SqlDataAdapter objda;
        private DataTable datatable;
        public static string Connstr = "";
        public static bool OfflineMode = false;

        public dboperation()
        {
            try
            {
                //// Create conenction string from local database
                //StringBuilder Con = new StringBuilder("Password=P@$$w0rd");
                //Con.Append(";Persist Security Info=True;User ID=sa");
                //Con.Append(";Initial Catalog=BanhaOnlineLive");
                //Con.Append(";Data Source=.\\MZSQLSERVER");
                //Connstr = Con.ToString();
                objcmd = new SqlCommand();
                connection = new SqlConnection(Connstr.Trim() + ";");
                objcmd.Connection = connection;
                objcmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "dboperation", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        /// <summary>
        /// Set Connection to database dynamic
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        public dboperation(string connectionString)
        {
            try
            {
                objcmd = new SqlCommand();
                connection = new SqlConnection(connectionString);
                objcmd.Connection = connection;
                objcmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "dboperation", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public SqlConnection GetConnection()
        {
            try
            {
                return connection;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "GetConnection", Utility.getDateTime().ToShortTimeString(), ex);
                return new SqlConnection();
            }
        }

        public bool OpenConnection()
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                else if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "OpenConnection", Utility.getDateTime().ToShortTimeString(), ex);
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "CloseConnection", Utility.getDateTime().ToShortTimeString(), ex);
                return false;
            }
        }

        public bool ExecuteSqlScripts(System.Collections.ArrayList arrListQueries)
        {
            SqlTransaction myTrans = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Start the transaction here
                myTrans = connection.BeginTransaction();

                // Assign the connection object to command
                // Also assign our transaction object to the command
                objcmd.Connection = connection;
                objcmd.Transaction = myTrans;
                objcmd.CommandType = CommandType.Text;

                for (int i = 0; i < arrListQueries.Count; i++)
                {
                    objcmd.CommandText = arrListQueries[i].ToString();
                    objcmd.ExecuteNonQuery();
                }

                myTrans.Commit();
                return true;
            }
            catch (Exception ex)
            {
                myTrans.Dispose();
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool ExecuteTransaction(System.Collections.ArrayList arrListQueries)
        {
            SqlTransaction myTrans = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Start the transaction here
                myTrans = connection.BeginTransaction();

                // Assign the connection object to command
                // Also assign our transaction object to the command
                objcmd.Connection = connection;
                objcmd.Transaction = myTrans;
                objcmd.CommandType = CommandType.Text;

                for (int i = 0; i < arrListQueries.Count; i++)
                {
                    objcmd.CommandText = arrListQueries[i].ToString();
                    objcmd.ExecuteNonQuery();
                }

                myTrans.Commit();
                return true;
            }
            catch
            {
                myTrans.Rollback();
                myTrans.Dispose();
                // Utility.MakeExceptionLog("DbOperation", "ExecuteTransaction", Utility.getDateTime().ToShortTimeString(), ex);
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public object GetSingleValue()
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                return (object)objcmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "GetSingleValue", Utility.getDateTime().ToShortTimeString(), ex);
                return null;
            }
        }

        public object GetSingleValue(string strQuery)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                objcmd.Parameters.Clear();
                objcmd.CommandType = CommandType.Text;
                objcmd.CommandText = strQuery;
                return (object)objcmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "GetSingleValue", Utility.getDateTime().ToShortTimeString(), ex);
                return null;
            }
        }

        public int ExecuteNonQuery(string strQuery)
        {
            try
            {
                if (strQuery.Trim() != "")
                {
                    objcmd.CommandType = CommandType.Text;
                    objcmd.CommandText = strQuery;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    return objcmd.ExecuteNonQuery();
                }
                else
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    int ret = objcmd.ExecuteNonQuery();
                    return ret;
                }
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "ExecuteNonQuery", Utility.getDateTime().ToShortTimeString(), ex);
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        public DataTable SelectData(string strQuery)
        {
            datatable = new DataTable();

            try
            {
                if (strQuery.Trim() != "")
                {
                    objda = new SqlDataAdapter();
                    objcmd.Parameters.Clear();
                    objcmd.CommandType = CommandType.Text;
                    objcmd.CommandText = strQuery;
                    objda.SelectCommand = objcmd;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    objda.Fill(datatable);
                    return datatable;
                }
                else
                {
                    objda = new SqlDataAdapter();
                    objda.SelectCommand = objcmd;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    objda.Fill(datatable);
                    return datatable;
                }
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "SelectData", Utility.getDateTime().ToShortTimeString(), ex);
                return datatable;
            }
            finally
            {
                connection.Close();
            }
        }

        public DataSet SelectDataSet(string strQuery)
        {
            DataSet datatable = new DataSet();

            try
            {
                if (strQuery.Trim() != "")
                {
                    objda = new SqlDataAdapter();
                    objcmd.CommandType = CommandType.Text;
                    objcmd.CommandText = strQuery;
                    objda.SelectCommand = objcmd;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    objda.Fill(datatable);
                    return datatable;
                }
                else
                {
                    objda = new SqlDataAdapter();
                    objda.SelectCommand = objcmd;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    objda.Fill(datatable);
                    return datatable;
                }
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "SelectDataSet", Utility.getDateTime().ToShortTimeString(), ex);
                return datatable;
            }
            finally
            {
                connection.Close();
            }
        }

        public void FillCombo(string Query, string DisplayMember, string ValueMember, ComboBox ReQuiredComboBox)
        {
            try
            {
                DataTable dtSource = SelectData(Query);
                ReQuiredComboBox.DataSource = dtSource;
                ReQuiredComboBox.DisplayMember = DisplayMember;
                ReQuiredComboBox.ValueMember = ValueMember;
                ReQuiredComboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCombo", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillCombo(string Query, string DisplayMember, string ValueMember, ComboBox ReQuiredComboBox, bool EmptyItem)
        {
            try
            {
                DataTable dtSource = SelectData(Query);
                ReQuiredComboBox.DisplayMember = DisplayMember;
                ReQuiredComboBox.ValueMember = ValueMember;

                if (dtSource.Rows.Count > 0 && EmptyItem)
                {
                    DataRow dr = dtSource.NewRow();
                    dr[ValueMember] = "0";
                    dr[DisplayMember] = " ";
                    dtSource.Rows.InsertAt(dr, 0);
                }

                ReQuiredComboBox.DataSource = dtSource;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCombo", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillCombo(DataTable dtSource, string DisplayMember, string ValueMember, ComboBox ReQuiredComboBox)
        {
            try
            {
                ReQuiredComboBox.DisplayMember = DisplayMember;
                ReQuiredComboBox.ValueMember = ValueMember;

                if (dtSource.Rows.Count > 0)
                {
                    ReQuiredComboBox.DataSource = dtSource;
                }
            }
            catch
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCombo", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillListBox(string Query, string DisplayVal, ListBox ReQuiredListBox)
        {
            try
            {
                ReQuiredListBox.Items.Clear();
                DataTable dtSource = SelectData(Query);

                foreach (DataRow dr in dtSource.Rows)
                {
                    ReQuiredListBox.Items.Add(dr[DisplayVal].ToString());
                }

                ReQuiredListBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillListBox", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillCheckListBox(string Query, CheckedListBox ReQuiredCheckListBox)
        {
            try
            {
                ReQuiredCheckListBox.Items.Clear();
                DataTable dtSource = SelectData(Query);

                foreach (DataRow dr in dtSource.Rows)
                {
                    ReQuiredCheckListBox.Items.Add(dr[0].ToString());
                }

                ReQuiredCheckListBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCheckListBox", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillCheckListBox(string Query, string ColumName, CheckedListBox ReQuiredCheckListBox)
        {
            try
            {
                ReQuiredCheckListBox.Items.Clear();
                DataTable dtSource = SelectData(Query);

                foreach (DataRow dr in dtSource.Rows)
                {
                    ReQuiredCheckListBox.Items.Add(dr[ColumName].ToString());
                }

                ReQuiredCheckListBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCheckListBox", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public void FillCheckListBox(string Query, string DisplayMember, string ValueMember, CheckedListBox ReQuiredCheckListBox)
        {
            try
            {
                ReQuiredCheckListBox.Items.Clear();
                DataTable dtSource = SelectData(Query);
                ReQuiredCheckListBox.DataSource = dtSource;
                ReQuiredCheckListBox.DisplayMember = DisplayMember;
                ReQuiredCheckListBox.ValueMember = ValueMember;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "FillCheckListBox", Utility.getDateTime().ToShortTimeString(), ex);
            }
        }

        public int ReturnInt(string query)
        {
            try
            {
                objcmd.CommandType = CommandType.Text;
                objcmd.CommandText = query;
                return Convert.ToInt32(GetSingleValue()?.ToString());
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "ReturnInt", Utility.getDateTime().ToShortTimeString(), ex);
                return 0;
            }
        }

        /// <summary>
        /// Return string Query 
        /// </summary>
        /// <param name="query"> Sql Query </param>
        /// <returns> string </returns>
        public string ReturnStr(string query)
        {
            try
            {
                objcmd.CommandType = CommandType.Text;
                objcmd.CommandText = query;
                object obj = GetSingleValue();

                if (obj != null)
                {
                    return obj.ToString();
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "ReturnStr", Utility.getDateTime().ToShortTimeString(), ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Check the string int 
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true if string is int else false </returns>
        public bool IsNumeric(string str)
        {
            try
            {
                Int64.Parse(str);
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "IsNumeric", Utility.getDateTime().ToShortTimeString(), ex);
                return false;
            }

            return true;
        }

        public string SelectName(string TableName, string FieldName, string FieldID, int IDValue)
        {
            try
            {
                objcmd.CommandType = CommandType.Text;
                objcmd.CommandText = "SELECT " + FieldName + " FROM " + TableName + " WHERE " + FieldID + " = " + IDValue.ToString();

                if (GetSingleValue() != null)
                {
                    return GetSingleValue().ToString();
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "SelectName", Utility.getDateTime().ToShortTimeString(), ex);
                return string.Empty;
            }
        }

        public string SelectName(string TableName, string FieldName, string FieldID, string IDValue)
        {
            try
            {
                objcmd.CommandType = CommandType.Text;
                objcmd.CommandText = "SELECT " + FieldName + " FROM " + TableName + " WHERE " + FieldID + " = '" + IDValue.ToString() + "'";

                if (GetSingleValue() != null)
                {
                    return GetSingleValue().ToString();
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "SelectName", Utility.getDateTime().ToShortTimeString(), ex);
                return string.Empty;
            }
        }

        public int ExecuteBulk(string tblName, DataTable data, DataRowState state)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tblName;
                    bulkCopy.WriteToServer(data, state);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                // Utility.MakeExceptionLog("DbOperation", "ExecuteBulk", Utility.getDateTime().ToShortTimeString(), ex);
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
