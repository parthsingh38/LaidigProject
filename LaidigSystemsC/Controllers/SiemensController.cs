using LaidigSystemsC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using PagedList;
using System.Text;

namespace LaidigSystemsC.Controllers
{

    [SessionExpire]
    public class SiemensController : Controller
    {
        OurDbContext db = new OurDbContext();
        List<string> path2 = new List<string>();
        List<string> paths = new List<string>();
        // GET: Siemens
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateUser()
        {
            return View();
        }
        public ActionResult Delete()
        {
            using (OurDbContext db = new OurDbContext())
            {
                //return View(db.useraccounts.ToList());
                ViewBag.userdetails = db.useraccounts.ToList();
            }
            return View();
        }
        [HttpPost]
        public ActionResult View(int userid)
        {
            using (OurDbContext db = new OurDbContext())
            {
                //return View(db.useraccounts.ToList());
                return PartialView("View", db.useraccounts.Find(userid));
            }
            
        }

        [HttpPost]
        public ActionResult update(UserAccount account)
        {
            UserAccount user = new UserAccount();
            using (var db = new OurDbContext())
            {
                user = db.useraccounts.Where(s => s.UserId == account.UserId).FirstOrDefault<UserAccount>();
            }
            if (user != null)
            {
                user.FirstName = account.FirstName;
                user.LastName = account.LastName;
                user.Email = account.Email;
                user.UserName = account.UserName;
                user.password = account.password;
                user.ConfirmPassword = account.password;
                user.userTypes = account.userTypes;
                user.userStatus = account.userStatus;

            }
            using (var db = new OurDbContext())
            {
                //3. Mark entity as modified
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                //4. call SaveChanges
                db.SaveChanges();
                string message = "User records updated for " + user.FirstName + " " + user.LastName + "user Type : " + user.userTypes;
                Mails mail = new Mails();
                mail.SendActivationLinkUsingEmailUserUpdate(Session["UserName"].ToString(), message);
                ViewBag.message = "Successfully Updated";
            }
            return RedirectToAction("Delete", "Siemens");


        }

        [HttpPost]
        public ActionResult CreateUser(UserAccount account)
        {
            if (ModelState.IsValid)
            {
                OurDbContext db = new OurDbContext();
                db.useraccounts.Add(account);
                db.SaveChanges();
                ModelState.Clear();
                ViewBag.Message = account.FirstName + " " + account.LastName + " Successfully Registered !";
                string message = "User : " + account.FirstName + " " + account.LastName + "  Successfully Registered !";
                Mails mail = new Mails();
                // string ActivationUrl = Url.Action("ChangePassword", "Account", new { email = propLogin.Email }, "http");
                mail.SendActivationLinkUsingEmail(account, message, Session["UserName"].ToString());
                return RedirectToAction("Delete", "Siemens");
            }
            else
            {
                return View();
            }



           
        }
        public ActionResult Deleteuser(UserAccount account)
        {
            UserAccount user = new UserAccount();
            using (var db = new OurDbContext())
            {
                user = db.useraccounts.Where(s => s.UserId == account.UserId).FirstOrDefault<UserAccount>();
            }
          
            using (var db = new OurDbContext())
            {
                //3. Mark entity as modified
                db.Entry(user).State = System.Data.Entity.EntityState.Deleted;

                //4. call SaveChanges
                db.SaveChanges();
                ViewBag.Message = "user " + user.FirstName + " Removed from Database successfully!!!";
                string message = "user " + user.FirstName + " " + user.LastName + "has been Removed from Database";
                Mails mail = new Mails();
                mail.SendActivationLinkUsingEmailUserDelete(Session["UserName"].ToString(), message);
            }


            return RedirectToAction("Index", "Siemens");


        
        }
        [HttpPost]
        public ActionResult PostMethod()
        {
            System.Threading.Thread.Sleep(5000);

            return Json("Message from Post action method.");
        }

        [HttpPost]
        public ActionResult Index(List<HttpPostedFileBase> fileNames, string rbGrp)
        {
            if (fileNames[0] == null)
            {
                ViewBag.Error = "Please select a directory/CSV file(s) !!!";
                return View();
            }
            else
            {
                bool isMachineIdExists = false;
                foreach (var file in fileNames)
                {
                    string fn = file.FileName.ToLower();
                    if (fn.Contains("smp0001") || fn.Contains("smp01"))
                    {
                        isMachineIdExists = true;
                        break;
                    }

                }

                if (!isMachineIdExists)
                {
                    ViewBag.Error = "Machine Id related file could not found, Please include Machine Id related file in the uploaded directory !!!";
                    return View();
                }


                string UserName = Session["UserName"].ToString();
                string name = rbGrp.ToString();
             
                string uniqueKey = string.Format("{0:dd-MM-yyyyHHmmss}", DateTime.Now);
                if (name == "Delayed")
                {
                    long no = 3221225472;
                    int totalSize = 0;

                    foreach (var csvName in fileNames)
                    {
                        totalSize += csvName.ContentLength;
                    }

                    if (totalSize <= no)
                    {
                        foreach (HttpPostedFileBase fileAB in fileNames)
                        {
                            List<string> currentDateCsvFiles = new List<string>();
                            String FileExtn1 = System.IO.Path.GetExtension(fileAB.FileName).ToLower();
                            string fileNameStratWithCSV = fileAB.FileName.ToLower();
                            if (FileExtn1.Contains(".csv") && fileNameStratWithCSV.Contains("smp"))
                            {
                                //List<FileDetail> fileDetails = new List<FileDetail>();
                                //List<CsvFile> listcsvfiles = new List<CsvFile>();
                                string dirNameToCreated = null;
                                string fileNameToBeCreated = null;
                                if (fileNameStratWithCSV.Contains('/'))
                                {
                                    dirNameToCreated = fileNameStratWithCSV.Substring(0, fileNameStratWithCSV.LastIndexOf('/'));
                                    fileNameToBeCreated = fileNameStratWithCSV.Substring(0, fileNameStratWithCSV.LastIndexOf('/'));

                                }
                                if (fileNameStratWithCSV.Contains("smp00_00") || fileNameStratWithCSV.Contains("smp0000"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_00.csv";

                                }
                                else if (fileNameStratWithCSV.Contains("smp00_01"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_01.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_02"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_02.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_03"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_03.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_04"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_04.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_05"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_05.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_06"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_06.csv";
                                }
                                else if (fileNameStratWithCSV.Contains("smp00_07"))
                                {
                                    fileNameStratWithCSV = fileNameToBeCreated + "/smp09_07.csv";
                                }

                                string root = @"c:/LaidigSystemFiles/Delayed/CsvFile/UserName-" + UserName + "/";
                                var dirName = @"c:/LaidigSystemFiles/Delayed/CsvFile/UserName-" + UserName + "/" + string.Format("{0:dd-MM-yyyy}", DateTime.Now) + "/" + dirNameToCreated;
                                var modifiedpathString1 = "";
                                if (dirName.ToLower().Contains("sample") || dirName.ToLower().Contains("logging"))
                                {
                                    var modifiedName = "";
                                    if (dirName.ToLower().Contains("sample"))
                                    {
                                        modifiedName = dirName.ToLower().Replace("sample", "sample" + uniqueKey);
                                    }
                                    else
                                    {
                                        modifiedName = dirName.ToLower().Replace("logging", "logging" + uniqueKey);
                                    }

                                    string pathString = System.IO.Path.Combine(root);
                                    string pathString1 = System.IO.Path.Combine(modifiedName);
                                    modifiedpathString1 = pathString1;
                                    if (!Directory.Exists(root))
                                    {
                                        System.IO.Directory.CreateDirectory(pathString);
                                    }
                                    if (!Directory.Exists(modifiedpathString1))
                                    {
                                        System.IO.Directory.CreateDirectory(pathString1);
                                    }
                                }
                                else
                                {

                                    string pathString = System.IO.Path.Combine(root);
                                    string pathString1 = System.IO.Path.Combine(dirName + uniqueKey);
                                    modifiedpathString1 = pathString1;
                                    if (!Directory.Exists(root))
                                    {
                                        System.IO.Directory.CreateDirectory(pathString);
                                    }
                                    if (!Directory.Exists(modifiedpathString1))
                                    {
                                        System.IO.Directory.CreateDirectory(pathString1);
                                    }
                                }

                                
                                var fileName = Path.GetFileNameWithoutExtension(fileNameStratWithCSV);
                                var csvFileName = Path.GetFileName(fileNameStratWithCSV);
                                var fileNameWithExt = Path.GetFileNameWithoutExtension(fileNameStratWithCSV) + DateTime.Now.ToString("dd-MM-yyyyhh-mm-ss") + ".csv";

                                var path = Path.Combine(modifiedpathString1, csvFileName);
                                fileAB.SaveAs(path);
                                string Timeschedule = System.IO.File.ReadLines(HttpContext.Server.MapPath("~/Settings.ini")).First();
                                ViewBag.Message = "Successfully upload files on server. Time is Schedule for File records move to database is scheduled on  : " + Timeschedule + " </br>File processing to database will take some time depends on file size. You will get Email Notification after Job Finished!!!";
                               
                                Mails mail1 = new Mails();
                                mail1.SendActivationLinkUsingEmailfileUpload(UserName, ViewBag.Message);
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Upload limit is 2 GB !!!";
                        return View();
                    }
                }
                else
                {
                    List<string> currentDateCsvFiles1 = new List<string>();

                    long value = 26214400;
                    int totalLength = 0;

                    foreach (var csvName in fileNames)
                    {
                        totalLength += csvName.ContentLength;
                    }

                    if (totalLength <= value)
                    {
                        string nonCSVFiles = null;
                        bool csvFileStatus = false;
                        string emptyFiles = null;
                        string sampleFile = null;
                        foreach (HttpPostedFileBase fileAB in fileNames)
                        {
                            String FileExtn2 = System.IO.Path.GetExtension(fileAB.FileName).ToLower();
                            string fileNameStratWith = fileAB.FileName.ToLower();
                            if (FileExtn2.Contains(".csv") && fileNameStratWith.Contains("smp"))
                            {
                                csvFileStatus = true;
                                //List<FileDetail> fileDetails = new List<FileDetail>();
                                //List<CsvFile> listcsvfiles = new List<CsvFile>();
                                string dirNameToCreated = null;
                                string fileNameToBeCreated = null;
                                if (fileNameStratWith.Contains('/'))
                                {
                                    dirNameToCreated = fileNameStratWith.Substring(0, fileNameStratWith.LastIndexOf('/'));
                                    fileNameToBeCreated = fileNameStratWith.Substring(0, fileNameStratWith.LastIndexOf('/'));

                                }
                                if (fileNameStratWith.Contains("smp00_00") || fileNameStratWith.Contains("smp0000"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_00.csv";

                                }
                                else if (fileNameStratWith.Contains("smp00_01"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_01.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_02"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_02.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_03"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_03.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_04"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_04.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_05"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_05.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_06"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_06.csv";
                                }
                                else if (fileNameStratWith.Contains("smp00_07"))
                                {
                                    fileNameStratWith = fileNameToBeCreated + "/smp09_07.csv";
                                }

                                string root = @"c:/LaidigSystemFiles/Instant/CsvFile/UserName-" + UserName + "/";
                                var dirName = @"c:/LaidigSystemFiles/Instant/CsvFile/UserName-" + UserName + "/" + string.Format("{0:dd-MM-yyyy}", DateTime.Now) + "/" + dirNameToCreated;
                                string pathString = System.IO.Path.Combine(root);
                                string pathString1 = System.IO.Path.Combine(dirName);
                                if (!Directory.Exists(root))
                                {
                                    System.IO.Directory.CreateDirectory(pathString);
                                }
                                if (!Directory.Exists(dirName))
                                {
                                    System.IO.Directory.CreateDirectory(pathString1);
                                }
                                var fileName = Path.GetFileNameWithoutExtension(fileNameStratWith);
                                var name1 = Path.GetFileName(fileNameStratWith);
                                var fileNameWithExt = Path.GetFileNameWithoutExtension(fileNameStratWith) + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".csv";
                                var path = Path.Combine(pathString1, name1);
                                var InstantCsvFile = Path.GetFileName(fileNameStratWith);
                                fileAB.SaveAs(path);
                                string MulfileName = fileName + ".csv";

                               
                                // string emptyFileName = dumpCSV(path, name1, uniqueKey);

                                //string success = dumpCSV(path, name1, uniqueKey);
                                //if (!string.IsNullOrEmpty(success) && success.Equals("machineid"))
                                //{
                                //    ViewBag.Error = "Duplicate Date inside machineiffile !!!";
                                //    return View();
                                //}

                                // ViewBag.Message = success == null ? "CSV Uploaded Successfully" : "This file can't be upload, Please check the file Name.!!!" + success;
                                //emptyFiles += emptyFileName != null || emptyFileName != "" ? emptyFiles + ", " + emptyFileName : null;
                                //string success = dumpCSV(path, name1, uniqueKey);

                                //ViewBag.Message = success == null ? "CSV Uploaded Successfully" : "This file can't be upload, Please check the file Name.!!!" + success;
                                //sampleFile = sampleFile + ", " + name1;
                            }
                            else
                            {
                                //nonCSVFiles = nonCSVFiles + ", " + fileNameStratWith;
                                //ViewBag.Error = "Upload only csv files!!!";
                            }
                        }
                        Mails mail = new Mails();
                        mail.SendActivationLinkUsingEmailfileUpload(UserName," File uploaded successfully ");
                        string csvPath = @"c:/LaidigSystemFiles/Instant/CsvFile/UserName-" + UserName + "/" + string.Format("{0:dd-MM-yyyy}", DateTime.Now);
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
                        string success = dumpCSV(currentDateCsvFiles);


                        System.IO.DirectoryInfo di = new DirectoryInfo(@"c:/LaidigSystemFiles/Instant/CsvFile");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                       
                        ViewBag.Message = success == null ? "CSV Uploaded Successfully" : "This file can't be upload, Please check the file Name.!!!" + success;

                        Mails mail1 = new Mails();
                        mail1.SendActivationLinkUsingEmailInstantFileMovedtoDB(UserName, "File moved to database for Instant upload!!!");

                        //    string csvFileSuccessMsg = null;
                        //    if (csvFileStatus)
                        //    {
                        //        //string fns = sampleFile.Substring(1, sampleFile.Length - 1);
                        //        //csvFileSuccessMsg = "All CSV file(s) are uploaded."+ fns;
                        //        csvFileSuccessMsg = "All CSV file(s) are uploaded.";
                        //    }
                        //    if (nonCSVFiles != null)
                        //    {
                        //        string fns = nonCSVFiles.Substring(1, nonCSVFiles.Length - 1);
                        //        csvFileSuccessMsg += "Following file(s) are non CSV or non SMP " + fns;
                        //    }
                        //    if (emptyFiles != null)
                        //    {
                        //        string fns = emptyFiles.Substring(1, emptyFiles.Length - 1);
                        //        csvFileSuccessMsg += "<br />Following file(s) are empty," + fns;
                        //    }

                        //   ViewBag.Message = csvFileSuccessMsg;

                    }
                    else
                    {
                        ViewBag.Error = "Upload limit is 25 MB !!!";
                        return View();
                    }

                }
                return View();
            }
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
        // private static string dumpCSV(string fileName, string CsvFileName, string uniqueKey)
        private static string dumpCSV(List<string> fileName)
        {
            string epmtyFiles = null;
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
                //string epmtyFiles = null;
                if (lines.Count > 0)
                {
                    bool headingCheck = checkIfAllEmptyInARow(1, lines[0]);
                    if (!headingCheck)
                    {
                        if (lines.Count > 1)
                        {
                            bool csvSecondRow = checkIfAllEmptyInARow(1, lines[1]);
                            if (csvSecondRow)
                            {
                                return MulfileName;
                            }
                        }
                    }
                    else
                    {
                        return MulfileName;
                    }
                }
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

                // }

                int lastRowMachineId = 0;
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


                    StringBuilder finalInserSQL = new StringBuilder();

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
                                // int checkLogdatetime = GetValueExists(logdatetime);
                                //if(checkLogdatetime == 0)
                                //{
                                //    finalUpdateSQL.Append(colHeadValue+" = "+lines[i][dataCount].Trim());
                                //}
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
            return epmtyFiles;
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
