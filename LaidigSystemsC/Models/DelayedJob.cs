using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using System.IO;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;

namespace LaidigSystemsC.Models
{
    public class DelayedJob : IJob
    {
        //public object ViewBag { get; private set; }
        List<string> paths = new List<string>();

        //BulkFile bulk = new BulkFile();
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("DelayedJob  started");
            DataTable dt = new DataTable();
        
            string csvPath = @"C:/LaidigSystemFiles/Delayed/CsvFile";
            List<string> csvFiles = DirSearch(csvPath);
            List<string> currentDateCsvFiles = new List<string>();
            foreach (var dateExits in csvFiles)
            {
                string checkDate = string.Format("{0:dd-MM-yyyy}", DateTime.Now).ToString();
                if (dateExits.Contains(checkDate))
                {
                    currentDateCsvFiles.Add(dateExits);
                }
            }
            dumpCSV(currentDateCsvFiles);
         
            System.IO.DirectoryInfo di = new DirectoryInfo(@"C:/LaidigSystemFiles/Delayed/CsvFile");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            System.Threading.Thread.Sleep(10000);
            Console.WriteLine("Job one finished");
            string messageDelayedupload = "Job one finished for Delayed upload : "+ string.Format("{0:dd-MM-yyyy}", DateTime.Now).ToString();
            Mails mail = new Mails();
            mail.SendActivationLinkUsingEmailDelayedFileMoveToDB(messageDelayedupload);
        }


        private List<string> DirSearch(string sDir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    paths.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearch(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return paths;
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
        }

        private static List<string> getColumnsName(string tableName)
        {
            List<string> listacolumnas = new List<string>();
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
            {
                sqlConn.Open();
                using (SqlCommand sqlCmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + tableName + "')", sqlConn))
                {
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listacolumnas.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return listacolumnas;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }


        private static string dumpCSV(List<string> fileName)
        {
            string status = null;
            string uniqueKey = string.Format("{0:ddMMyyyyhhmmss}", DateTime.Now);
            foreach (string MulfileName in fileName)
            {
                string tableName = getTableName(MulfileName);
                if (tableName == null)
                {
                    return MulfileName;
                }
                List<List<string>> lines = new List<List<string>>();

                List<string> csvValues = new List<string>();
                using (StreamReader csvReader = new StreamReader(MulfileName))
                {
                    string line = "";
                    string lastLine = string.Empty;
                    int count = 0;

                    if (fileName.Contains("smp0001"))
                    {
                        line = csvReader.ReadLine();
                        List<string> columns = new List<string>(line.Split(','));
                        lines.Add(columns);
                        while (!csvReader.EndOfStream)
                        {
                            count++;
                            lastLine = csvReader.ReadLine();
                        }
                        columns = new List<string>(lastLine.Split(','));
                        lines.Add(columns);

                    }
                    else
                    {
                        while ((line = csvReader.ReadLine()) != null)
                        {
                            List<string> columns = new List<string>(line.Split(','));
                            lines.Add(columns);
                        }
                    }

                }
                //Total number of columns to be created.            
                int csvHeadersCount = Convert.ToInt32(lines[0].Count);
                List<string> csvHeaders = lines[0];
                List<int> columnNumbers = new List<int>();
                for (int hCount = 0; hCount < 3; hCount++)
                {
                    if (lines.Count > 0 && lines.Count <= 2)
                    {
                        break;
                    }
                    columnNumbers.Add(Convert.ToInt32(lines[hCount].Count));
                }
                int columnNumbersToBeCreated = 0;
                if (columnNumbers.Count > 0)
                {
                    columnNumbersToBeCreated = columnNumbers.Max();
                }
                //Checking CSV header count and CSV 2-3 data rows.
                if (csvHeadersCount > columnNumbersToBeCreated)
                {
                    columnNumbersToBeCreated = csvHeadersCount;
                }
                else if (csvHeadersCount < columnNumbersToBeCreated)
                {
                    //add null("") to csv headers
                    for (int extraColCount = 0; extraColCount < columnNumbersToBeCreated - csvHeadersCount; extraColCount++)
                    {
                        csvHeaders.Add("");
                    }
                }
                //Checking nulls and replacing NoHeading with nulls in csvHeads List.
                List<string> modifiedCSVHeaders = new List<string>();
                List<string> alterColumn = new List<string>();
                for (int nullHeader = 0; nullHeader < csvHeaders.Count; nullHeader++)
                {
                    try
                    {
                        string columnName = RemoveSpecialCharacters(csvHeaders[nullHeader].ToLower().Trim());
                        if (columnName.Contains("date") || columnName.Contains("time") || (lines[1][nullHeader].Contains('/') && lines[1][nullHeader].Contains(':')))
                        {
                            columnName = "logdatetime";
                        }
                        else if (columnName == null || columnName == "")
                        {
                            columnName = "noheading" + nullHeader;
                        }
                        alterColumn.Add(columnName);
                        int n;
                        bool isNumeric = int.TryParse(columnName, out n);
                        columnName = isNumeric ? "[" + columnName + "]" : columnName;
                        modifiedCSVHeaders.Add(columnName);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                    
                }
                ////----------------------------------------------------------------
                ////Creating dynamic table from modifiedCSVHeaders List
                int lastRowMachineId = 0; ;
                if (tableName == "machineidfile")
                {
                    StringBuilder createTableSQL = new StringBuilder("IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U')) " +
                                                             " BEGIN " +
                                                             " CREATE TABLE " + tableName + " (MachineId int IDENTITY(1,1) PRIMARY KEY, ");
                    StringBuilder insertDataSQL = new StringBuilder("INSERT INTO " + tableName + "( ");

                    for (int colCount = 0; colCount < modifiedCSVHeaders.Count; colCount++)
                    {
                        string columnDataType = "";
                        if (modifiedCSVHeaders[colCount].Trim() == "logdatetime")
                        {
                            columnDataType = "datetime  ";
                        }
                        else
                        {
                            columnDataType = getType();
                        }

                        createTableSQL.Append(modifiedCSVHeaders[colCount].Trim() + " " + columnDataType);
                        insertDataSQL.Append(modifiedCSVHeaders[colCount].Trim());
                        if (colCount < (modifiedCSVHeaders.Count - 1))
                        {
                            createTableSQL.Append(",");
                            insertDataSQL.Append(",");
                        }
                        else
                        {
                            createTableSQL.Append(") End");
                            insertDataSQL.Append(") ");
                        }
                    }
                    ////Table will be created, if the table does not exists.
                    using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                    {
                        sqlConn.Open();
                        using (SqlCommand sqlCmd = new SqlCommand(createTableSQL.ToString(), sqlConn))
                        {
                            sqlCmd.ExecuteNonQuery();
                            sqlConn.Close();
                        }
                    }

                    //Altering the table with additional column if any new columns found
                    List<string> listOfTableColums = getColumnsName(tableName);
                    List<string> differColumns = (List<string>)alterColumn.Except(listOfTableColums).ToList();
                    if (differColumns.Count > 0)
                    {
                        foreach (string differColumn in differColumns)
                        {
                            string colDataType = getType();
                            string columnName = differColumn;
                            int n;
                            bool isNumeric = int.TryParse(columnName, out n);
                            columnName = isNumeric ? "[" + columnName + "]" : columnName;
                            string alterQuery = "ALTER TABLE " + tableName + " ADD " + columnName + " " + colDataType;
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(alterQuery, sqlConn))
                                {
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                    }

                    ////------------------------------------------------------
                    StringBuilder finalInserSQL = new StringBuilder();
                    for (int i = 1; i < lines.Count; i++)
                    {

                        try
                        {


                            StringBuilder cellData = new StringBuilder("values(");
                            string pk = string.Empty;
                            for (int dataCount = 0; dataCount < lines[i].Count; dataCount++)
                            {
                                string colHeadValue = lines[0][dataCount].Trim().ToLower();
                                string colHeaderLastChar = colHeadValue.Length == 7 ? colHeadValue.Substring(colHeadValue.Length - 1) : null;
                                string dataValue = lines[i][dataCount].Trim();
                                int lastChar;
                                bool isNumeric = int.TryParse(colHeaderLastChar, out lastChar);
                                if (isNumeric && lastChar != 0)
                                {
                                    dataValue = getMultiplyValue(lastChar, Convert.ToDecimal(dataValue));
                                }

                                bool isAllEmpty = checkIfAllEmptyInARow(dataCount, lines[i]);
                                if (isAllEmpty)
                                {
                                    break;
                                }
                                cellData.Append("'" + dataValue + "'");
                                if (dataCount < (lines[i].Count - 1))
                                {
                                    cellData.Append(",");
                                }
                                else
                                {
                                    cellData.Append(");");
                                    finalInserSQL.Append(insertDataSQL.ToString());
                                    finalInserSQL.Append(cellData.ToString());
                                    cellData.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                    if (lines.Count > 1 && finalInserSQL.Length > 0)
                    {
                        try
                        {
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(finalInserSQL.ToString(), sqlConn))
                                {
                                    sqlCmd.CommandTimeout = 0;
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                        catch (Exception EX)
                        {
                            return "machineid";
                        }

                    }

                }
                else if (tableName == "eventalarmlog")
                {

                    using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                    {
                        sqlConn.Open();
                        using (SqlCommand sqlCmd = new SqlCommand("select max(MachineId) as id from machineidfile", sqlConn))
                        {
                            lastRowMachineId = (int)sqlCmd.ExecuteScalar();
                            // lastRowMachineId = lastRowMachineId + 1;
                            //sqlCmd.ExecuteNonQuery();
                            sqlConn.Close();
                        }
                    }
                    tableName = "MasterUploadedData";
                    StringBuilder createTableSQL = new StringBuilder("IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U')) " +
                                                             " BEGIN " +
                                                             " CREATE TABLE " + tableName + " (LogId int IDENTITY(1,1) PRIMARY KEY,MachineId int FOREIGN KEY REFERENCES machineidfile(MachineId), ");
                    StringBuilder insertDataSQL = new StringBuilder("INSERT INTO " + tableName + "(MachineId, ");

                    for (int colCount = 0; colCount < modifiedCSVHeaders.Count; colCount++)
                    {
                        string columnDataType = "";
                        if (modifiedCSVHeaders[colCount].Trim() == "logdatetime")
                        {
                            columnDataType = "datetime  ";
                        }
                        else
                        {
                            columnDataType = getType();
                        }

                        createTableSQL.Append(modifiedCSVHeaders[colCount].Trim() + " " + columnDataType);
                        insertDataSQL.Append(modifiedCSVHeaders[colCount].Trim());
                        if (colCount < (modifiedCSVHeaders.Count - 1))
                        {
                            createTableSQL.Append(",");
                            insertDataSQL.Append(",");
                        }
                        else
                        {
                            createTableSQL.Append(") End");
                            insertDataSQL.Append(") ");
                        }
                    }
                    ////Table will be created, if the table does not exists.
                    using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                    {
                        sqlConn.Open();
                        using (SqlCommand sqlCmd = new SqlCommand(createTableSQL.ToString(), sqlConn))
                        {
                            sqlCmd.ExecuteNonQuery();
                            sqlConn.Close();
                        }
                    }

                    //Altering the table with additional column if any new columns found
                    List<string> listOfTableColums = getColumnsName(tableName);
                    List<string> differColumns = (List<string>)alterColumn.Except(listOfTableColums).ToList();
                    if (differColumns.Count > 0)
                    {
                        foreach (string differColumn in differColumns)
                        {
                            string colDataType = getType();
                            string columnName = differColumn;
                            int n;
                            bool isNumeric = int.TryParse(columnName, out n);
                            columnName = isNumeric ? "[" + columnName + "]" : columnName;
                            string alterQuery = "ALTER TABLE " + tableName + " ADD " + columnName + " " + colDataType;
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(alterQuery, sqlConn))
                                {
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                    }

                    ////------------------------------------------------------
                    StringBuilder finalInserSQL = new StringBuilder();
                    for (int i = 1; i < lines.Count; i++)
                    {
                        try
                        {


                            StringBuilder cellData = new StringBuilder("values('" + lastRowMachineId + "',");
                            //StringBuilder cellData = new StringBuilder("values(");
                            string pk = string.Empty;
                            for (int dataCount = 0; dataCount < lines[i].Count; dataCount++)
                            {
                                string colHeadValue = lines[0][dataCount].Trim().ToLower();
                                string colHeaderLastChar = colHeadValue.Length == 7 ? colHeadValue.Substring(colHeadValue.Length - 1) : null;
                                string dataValue = lines[i][dataCount].Trim();
                                int lastChar;
                                bool isNumeric = int.TryParse(colHeaderLastChar, out lastChar);
                                if (isNumeric && lastChar != 0)
                                {
                                    dataValue = getMultiplyValue(lastChar, Convert.ToDecimal(dataValue));
                                }

                                bool isAllEmpty = checkIfAllEmptyInARow(dataCount, lines[i]);
                                if (isAllEmpty)
                                {
                                    break;
                                }
                                if (dataCount != 0)
                                {
                                    cellData.Append("'" + dataValue + "'");
                                }

                                if (dataCount == 0)
                                {
                                    string code = GetEventAlarmNameCode(dataValue);
                                    cellData.Append(code);
                                }

                                if (dataCount < (lines[i].Count - 1))
                                {
                                    cellData.Append(",");
                                }
                                else
                                {
                                    cellData.Append(");");
                                    finalInserSQL.Append(insertDataSQL.ToString());
                                    finalInserSQL.Append(cellData.ToString());
                                    cellData.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                    if (lines.Count > 1 && finalInserSQL.Length > 0)
                    {
                        try
                        {
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(finalInserSQL.ToString(), sqlConn))
                                {
                                    sqlCmd.CommandTimeout = 0;
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                        catch (Exception EX)
                        {
                            return "machineid";
                        }

                    }

                }
                else
                {
                    using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                    {
                        sqlConn.Open();
                        using (SqlCommand sqlCmd = new SqlCommand("select max(MachineId) as id from machineidfile", sqlConn))
                        {
                            lastRowMachineId = (int)sqlCmd.ExecuteScalar();
                            //sqlCmd.ExecuteNonQuery();
                            sqlConn.Close();
                        }
                    }

                    StringBuilder createTableSQL = new StringBuilder("IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U')) " +
                                                                            " BEGIN " +
                                                                            " CREATE TABLE " + tableName + " (LogId int IDENTITY(1,1) PRIMARY KEY,MachineId int FOREIGN KEY REFERENCES machineidfile(MachineId) , ");
                    StringBuilder insertDataSQL = new StringBuilder("INSERT INTO " + tableName + "(MachineId, ");

                    for (int colCount = 0; colCount < modifiedCSVHeaders.Count; colCount++)
                    {
                        string columnDataType = "";
                        if (modifiedCSVHeaders[colCount].Trim() == "logdatetime")
                        {
                            columnDataType = "datetime  ";
                        }
                        else
                        {
                            columnDataType = getType();
                        }

                        createTableSQL.Append(modifiedCSVHeaders[colCount].Trim() + " " + columnDataType);
                        insertDataSQL.Append(modifiedCSVHeaders[colCount].Trim());
                        if (colCount < (modifiedCSVHeaders.Count - 1))
                        {
                            createTableSQL.Append(",");
                            insertDataSQL.Append(",");
                        }
                        else
                        {
                            createTableSQL.Append(") End");
                            insertDataSQL.Append(") ");
                        }
                    }
                    ////Table will be created, if the table does not exists.
                    using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                    {
                        sqlConn.Open();
                        using (SqlCommand sqlCmd = new SqlCommand(createTableSQL.ToString(), sqlConn))
                        {
                            sqlCmd.ExecuteNonQuery();
                            sqlConn.Close();
                        }
                    }

                    //Altering the table with additional column if any new columns found
                    List<string> listOfTableColums = getColumnsName(tableName);
                    List<string> differColumns = (List<string>)alterColumn.Except(listOfTableColums).ToList();
                    if (differColumns.Count > 0)
                    {
                        foreach (string differColumn in differColumns)
                        {
                            string colDataType = getType();
                            string columnName = differColumn;
                            int n;
                            bool isNumeric = int.TryParse(columnName, out n);
                            columnName = isNumeric ? "[" + columnName + "]" : columnName;
                            string alterQuery = "ALTER TABLE " + tableName + " ADD " + columnName + " " + colDataType;
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(alterQuery, sqlConn))
                                {
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                    }



                    ////------------------------------------------------------
                    StringBuilder finalInserSQL = new StringBuilder();
                    // StringBuilder finalUpdateSQL = new StringBuilder();
                    //finalUpdateSQL.Append("UPDATE " + tableName + " SET ");
                    for (int i = 1; i < lines.Count; i++)
                    {
                        try
                        {


                            StringBuilder cellData = new StringBuilder("values('" + lastRowMachineId + "',");
                            // StringBuilder cellData = new StringBuilder("values(");
                            string pk = string.Empty;
                            for (int dataCount = 0; dataCount < lines[i].Count; dataCount++)
                            {
                                string colHeadValue = lines[0][dataCount].Trim().ToLower();
                                string colHeaderLastChar = colHeadValue.Length == 7 ? colHeadValue.Substring(colHeadValue.Length - 1) : null;
                                string dataValue = lines[i][dataCount].Trim();
                                string logdatetime = lines[i][0].Trim();

                                int lastChar;
                                bool isNumeric = int.TryParse(colHeaderLastChar, out lastChar);
                                if (isNumeric && lastChar != 0)
                                {
                                    dataValue = getMultiplyValue(lastChar, Convert.ToDecimal(dataValue));
                                }

                                bool isAllEmpty = checkIfAllEmptyInARow(dataCount, lines[i]);
                                if (isAllEmpty)
                                {
                                    break;
                                }

                                cellData.Append("'" + dataValue + "'");
                                if (dataCount < (lines[i].Count - 1))
                                {
                                    cellData.Append(",");
                                }
                                else
                                {
                                    cellData.Append(");");
                                    finalInserSQL.Append(insertDataSQL.ToString());
                                    finalInserSQL.Append(cellData.ToString());
                                    cellData.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                    if (lines.Count > 1 && finalInserSQL.Length > 0)
                    {
                        try
                        {
                            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
                            {
                                sqlConn.Open();
                                using (SqlCommand sqlCmd = new SqlCommand(finalInserSQL.ToString(), sqlConn))
                                {
                                    sqlCmd.CommandTimeout = 0;
                                    sqlCmd.ExecuteNonQuery();
                                    sqlConn.Close();
                                }
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }

                }

            }
            return status;
        }




        private static string GetEventAlarmNameCode(string description)
        {
            string eventslog = "0";

            switch (description)
            {
                case "Advance Motion Timeout Fault":
                    eventslog = "1";
                    break;
                case "Advance Motion Timeout Warning to Customer":
                    eventslog = "2";
                    break;
                case "Advance Pressure Transducer Signal Loss Fault":
                    eventslog = "3";
                    break;
                case "Advance Warning Timeout to Customer":
                    eventslog = "4";
                    break;
                case "Auto Cycle Running":
                    eventslog = "5";
                    break;
                case "Customer Adjustment Password Screen":
                    eventslog = "6";
                    break;
                case "Customer Adjustment Screen Active":
                    eventslog = "7";
                    break;
                case "Customer Hardwire Speed Signal Loss Fault":
                    eventslog = "8";
                    break;
                case "Customer Password Screen Active":
                    eventslog = "9";
                    break;
                case "Datalog Memory Management Screen Active":
                    eventslog = "10";
                    break;
                case "Final Clean Cycle is Running":
                    eventslog = "11";
                    break;
                case "Final Clean Cycle Started":
                    eventslog = "12";
                    break;
                case "Hydraulic AUX Pump Running":
                    eventslog = "13";
                    break;
                case "Hydraulic Oil High Temp Fault":
                    eventslog = "14";
                    break;
                case "Hydraulic Pump Starter AUX contact Fault":
                    eventslog = "15";
                    break;
                case "Hydraulic Pump Starter AUX Fault":
                    eventslog = "16";
                    break;
                case "Initial Clean Cycle is Running":
                    eventslog = "17";
                    break;
                case "Initial Clean Cycle Started":
                    eventslog = "18";
                    break;
                case "Laidig Service Only Adjustment Screen Active":
                    eventslog = "19";
                    break;
                case "Laidig Service Only Password Screen Active":
                    eventslog = "20";
                    break;
                case "LMI E-Stop has been Activated":
                    eventslog = "21";
                    break;
                case "LMI in Auto Mode":
                    eventslog = "22";
                    break;
                case "LMI in Manual Mode":
                    eventslog = "23";
                    break;
                case "LMI Reset Button Pushed":
                    eventslog = "24";
                    break;
                case "Manual Run Time Limit Exceeded":
                    eventslog = "25";
                    break;
                case "OK to Fill":
                    eventslog = "26";
                    break;

                case "Power Loss When in Auto or Manual Fault":
                    eventslog = "27";
                    break;
                case "Reclaim Plug Pressure Fault":
                    eventslog = "28";
                    break;
                case "Reclaim UNLoad Valve Routine Active":
                    eventslog = "29";
                    break;
                case "Reclaimer Final Cleanout Run Command is Active":
                    eventslog = "30";
                    break;
                case "Reclaimer Initial Cleanout Run Command is Active":
                    eventslog = "31";
                    break;
                case "Remote System Imediate Stop has been Activated":
                    eventslog = "32";
                    break;
                case "Remote System Immediate Stop has been Activated":
                    eventslog = "33";
                    break;
                case "Safety Protocals are Removed":
                    eventslog = "34";
                    break;
                case "Safety Protocals are Restored":
                    eventslog = "35";
                    break;
                case "Service Adjustment Password Screen":
                    eventslog = "36";
                    break;
                case "Service Adjustment Screen Active":
                    eventslog = "37";
                    break;
                case "Silo Filling Input Signal ON":
                    eventslog = "38";
                    break;
                case "System Safety Protocals Removed":
                    eventslog = "39";
                    break;



                default:
                    return eventslog;
            }

            return eventslog;

        }

        public static int GetValueExists(string logdateime)
        {
            int chekExist = 0;
            string query = "select count(logdatetime) as logdatetime from UploadedData where logdatetime = '" + logdateime + "'";
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
            {
                sqlConn.Open();
                using (SqlCommand sqlCmd = new SqlCommand(query, sqlConn))
                {
                    sqlCmd.CommandTimeout = 0;
                    chekExist = (int)sqlCmd.ExecuteScalar();
                    sqlConn.Close();
                }
            }

            return chekExist;

        }
        private static string getMultiplyValue(int colHeaderLastChar, decimal dataValue)
        {
            decimal res = 0;

            switch (colHeaderLastChar)
            {
                case 1:
                    res = dataValue * 10;
                    break;
                case 2:
                    res = dataValue * 100;
                    break;
                case 3:
                    res = dataValue * 1000;
                    break;
                case 4:
                    res = dataValue * 10000;
                    break;
                case 5:
                    res = dataValue * 100000;
                    break;
                case 6:

                    try
                    {
                        res = dataValue / 10;
                    }
                    catch (DivideByZeroException ex)
                    {

                        Console.Write("Cannot divide by zero. Please try again." + ex);
                    }
                    catch (InvalidOperationException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    catch (FormatException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    break;
                case 7:
                    try
                    {
                        res = dataValue / 100;
                    }
                    catch (DivideByZeroException ex)
                    {

                        Console.Write("Cannot divide by zero. Please try again." + ex);
                    }
                    catch (InvalidOperationException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    catch (FormatException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    break;
                case 8:
                    try
                    {
                        res = dataValue / 1000;
                    }

                    catch (DivideByZeroException ex)
                    {

                        Console.Write("Cannot divide by zero. Please try again." + ex);
                    }
                    catch (InvalidOperationException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    catch (FormatException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    break;
                case 9:
                    try
                    {
                        res = dataValue / 10000;
                    }

                    catch (DivideByZeroException ex)
                    {

                        Console.Write("Cannot divide by zero. Please try again." + ex);
                    }
                    catch (InvalidOperationException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    catch (FormatException ex)
                    {

                        Console.Write("Not a valid number. Please try again." + ex);
                    }
                    break;
            }

            return res.ToString();

        }

        private static bool checkIfAllEmptyInARow(int dataCount, List<string> oneRow)
        {
            bool isAllEmpty = false;
            int countEmpty = 0;
            for (int i = 0; i < oneRow.Count; i++)
            {
                if (oneRow[i] == "" || oneRow[i] == null)
                    countEmpty++;
            }

            isAllEmpty = countEmpty == oneRow.Count ? true : false;
            return isAllEmpty;
        }

        private static bool checkInDB(string tableName, string value)
        {
            bool matchFound = false;
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
            {
                sqlConn.Open();
                using (SqlCommand sqlCmd = new SqlCommand("select count(*) from " + tableName + " where logdatetime='" + value + "'", sqlConn))
                {
                    matchFound = Convert.ToBoolean(sqlCmd.ExecuteScalar());
                    sqlConn.Close();
                }
            }

            return matchFound;
        }
        private static string extractColumnName(string columnName)
        {
            if (columnName.Contains(']'))
            {
                string[] tempColName = columnName.Split(']');
                columnName = tempColName[1].Substring(0, tempColName[1].Length - 1);
            }
            else if (columnName.Contains("date") || columnName.Contains("time"))
            {
                columnName = "datetime";
            }
            else if (columnName.Contains('.'))
            {
                columnName = columnName.Replace(".", "");
            }

            return columnName;
        }

        static string getType()
        {
            return "NVARCHAR(100)";
        }

        static string getTableName(string filePath)
        {
            Dictionary<string, string> tableNames = new Dictionary<string, string>()
            {
                {"smp0900", "eventalarmlog" },
               {"smp0001", "machineidfile" },
               {"smp0002", "MasterUploadedData" },
               {"smp0003", "MasterUploadedData" },
               {"smp0004", "MasterUploadedData" },
               {"smp0005", "MasterUploadedData"},
            };
            string[] filePathArr = filePath.Split('\\');
            string[] fileNameWithExtension = filePathArr[filePathArr.Length - 1].Split('.');
            string fileNameWithOutExtension = fileNameWithExtension[0].ToLower();
            fileNameWithOutExtension = fileNameWithOutExtension.Contains(' ') ? fileNameWithOutExtension.Replace(" ", "") : fileNameWithOutExtension;
            if (fileNameWithOutExtension.Contains('_'))
            {
                fileNameWithOutExtension = convertShortToExactKey(fileNameWithOutExtension);
            }
            string tableName = tableNames.ContainsKey(fileNameWithOutExtension) ? tableNames[fileNameWithOutExtension] : null;
            return tableName;
        }

        private static string convertShortToExactKey(string fileNameWithOutExtension)
        {
            string convertedKey = string.Empty;
            string[] fileNameArr = fileNameWithOutExtension.Split('_');
            string firstElement = fileNameArr[0];
            string lastChar = fileNameArr[0].Substring(fileNameArr[0].Length - 1);

            switch (lastChar)
            {
                case "9":
                    convertedKey = "smp0900";
                    break;
                case "1":
                    convertedKey = "smp0001";
                    break;
                case "2":
                    convertedKey = "smp0002";
                    break;
                case "3":
                    convertedKey = "smp0003";
                    break;
                case "4":
                    convertedKey = "smp0004";
                    break;
                case "5":
                    convertedKey = "smp0005";
                    break;
            }
            return convertedKey;
        }
    }
}