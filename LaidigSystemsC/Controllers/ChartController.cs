using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaidigSystemsC.Models;
using System.Web.Helpers;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;


using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LaidigSystemsC.Controllers
{

    [SessionExpire]
    public class ChartController : Controller
    {

        List<SelectListItem> JobNo = new List<SelectListItem>();
        List<SelectListItem> SelectMachine = new List<SelectListItem>();
        List<SelectListItem> MachineSN = new List<SelectListItem>();
        List<SelectListItem> Diameter = new List<SelectListItem>();
        List<SelectListItem> Material = new List<SelectListItem>();
        List<SelectListItem> TrendData = new List<SelectListItem>();
        List<SelectListItem> SysColumns = new List<SelectListItem>();
        List<SelectListItem> SysColumnsx = new List<SelectListItem>();
        List<SelectListItem> eventslog = new List<SelectListItem>();

        // GET: Chart
        public ActionResult Index()
        {



            DataTable data = new DataTable();

            List<string> dropDownList = new List<string>();

            ViewBag.var1 = GetOptionsEnjineeringjobno();
            //ViewBag.var2 = GetMachineSerialNo();
            //ViewBag.var3 = GetMachineModel();

            //ViewBag.var4 = GetDiameter();
            //ViewBag.var5 = GetMaterial();
            ViewBag.var6 = GetSysColumns();
            ViewBag.var7 = GetSysColumnsx();
            ViewBag.var8 = GetOptionsEventsDropdown();






            return View("Index", dropDownList);
        }
        public SelectList GetSysColumns()
        {
            string finalDescription = "";
            string finalCode = "";
            List<string> finalcode = new List<string>();
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            string Sql = "SELECT [Code],[Description] FROM [LaidigDb].[dbo].[LookupMeasureCodes] order by Description";
            // string Sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'keymachineparameters'";

            using (var cmd = new SqlCommand(Sql, con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                //con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            List<string> columnDescription = new List<string>();
            List<string> columnvalues = new List<string>();

            columnDescription = dt.AsEnumerable().Select(r => r.Field<string>("Description")).ToList();
            columnvalues = dt.AsEnumerable().Select(r => r.Field<string>("Code")).ToList();

            for (var i = 0; i < columnDescription.Count; i++)
            {


                finalDescription = (columnDescription[i]).ToString();
                finalCode = (columnvalues[i]).ToString();

                SysColumns.Add(new SelectListItem { Text = finalDescription.ToString(), Value = finalCode.ToString() });
            }

            return new SelectList(SysColumns, "Value", "Text", "id");

        }
        public SelectList GetSysColumnsx()
        {

            string finalDescription = "";
            string finalCode = "";
            List<string> finalcode = new List<string>();
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            // string Sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'keymachineparameters'";
            string Sql = "SELECT [Code],[Description] FROM [LaidigDb].[dbo].[LookupMeasureCodes] order by Description";
            using (var cmd = new SqlCommand(Sql, con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                //con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            List<string> columnDescription = new List<string>();
            List<string> columnvalues = new List<string>();

            columnDescription = dt.AsEnumerable().Select(r => r.Field<string>("Description")).ToList();
            columnvalues = dt.AsEnumerable().Select(r => r.Field<string>("Code")).ToList();

            for (var i = 0; i < columnDescription.Count; i++)
            {


                finalDescription = (columnDescription[i]).ToString();
                finalCode = (columnvalues[i]).ToString();

                SysColumnsx.Add(new SelectListItem { Text = finalDescription.ToString(), Value = finalCode.ToString() });
            }

            return new SelectList(SysColumnsx, "Value", "Text", "id");

        }
        public ActionResult LoadJobsNo()
        {

            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT DISTINCT  [2]  FROM [dbo].[machineidfile]", conn);
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {

                    JobNo.Add(new SelectListItem { Text = "J" + myReader["2"].ToString(), Value = "J" + myReader["2"].ToString() });
                }

                conn.Close();
                //return the list objects

            }
            ViewData["JobNo"] = JobNo;
            return View();
        }
        public ActionResult MultipleYAxis()
        {
            return View();
        }

        public string DataTableToJSONWithStringBuilder()
        {
            DataTable table = GetDataTABLE("machine");
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("[{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {

                            JSONString.Append("\"" + "v" + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + "v" + "\":" + "\"" + table.Rows[i][j].ToString() + "\"}");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}]");
                    }
                    else
                    {
                        JSONString.Append("}],");
                    }
                }


            }
            return JSONString.ToString();
        }
        public static DataTable GetDataTABLE(string machine)
        {

            DataTable table = new DataTable();
            try
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                string machinecode = machine;
                string sb = "";
                sb = sb = "select top 100 convert(varchar(32),logdatetime, 101) as logdatetime,CAST([m12904] AS INT) AS [m12904],CAST([m12901] AS INT) AS [m12901],CAST([m12908] AS INT) AS [m12908] from [LaidigDb].[dbo].[keymachineparameters] where [m12901] is not null";
                using (var cmd = new SqlCommand(sb, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(table);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            catch (Exception e)
            {
                string msg = e.ToString();
            }



            return table;
        }
        public JsonResult GetData(string machine, string model1)
        {

            int count = 0;
            int count1 = 0;
            string newString;

            DataTable dt = GetDataTABLE(machine);
            StringBuilder sb = new StringBuilder();

            List<string> col1 = dt.AsEnumerable().Select(x => x[0].ToString()).ToList();
            for (int i = 0; i < col1.Count; i++)
            {
                string s = col1[i];
                newString = s.Substring(s.IndexOf(' ') + 1);

                col1[i] = newString;
            }
            count1 = count;

            List<string> col2 = dt.AsEnumerable().Select(x => x[1].ToString()).ToList();

            ViewBag.list = col1;

            return Json(col1, JsonRequestBehavior.AllowGet);

        }
        public static List<string> GetMachineSerialNoDropDownList()
        {

            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            DataTable dt = new DataTable();
            string sb = "";
            sb = "select TimeDate,T11002,G11001 from DataLogMonitouches";
            using (var cmd = new SqlCommand(sb, con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            List<string> machinSn = new List<string>();
            machinSn = dt.AsEnumerable().Select(r => r.Field<string>("T11002")).ToList();

            return machinSn;
        }
        public ActionResult GetEngineerinJobNumber()
        {
            try
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                DataTable dt = new DataTable();
                string sb = "";
                sb = " SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'machineidfile' AND TABLE_SCHEMA='dbo' ";
                using (var cmd = new SqlCommand(sb, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
                List<string> JobDetails = new List<string>();
                JobDetails = dt.AsEnumerable().Select(r => r.Field<string>("COLUMN_NAME")).ToList();
                StringBuilder sb1 = new StringBuilder();
                sb1.Append("J");
                string s = "";
                sb1.Append(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private SelectList GetOptionsEnjineeringjobno()
        {
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            try
            {
                JobNo.Add(new SelectListItem { Text = "None", Value = "0" });
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT DISTINCT  [2]  FROM [dbo].[machineidfile] ", conn);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {

                        JobNo.Add(new SelectListItem { Text = "J" + myReader["2"].ToString(), Value = "J" + myReader["2"].ToString() });
                    }

                    conn.Close();
                    //return the list objects

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new SelectList(JobNo, "Value", "Text", "id");
        }

        private SelectList GetOptionsEventsDropdown()
        {
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT [Code],[Description]  FROM [dbo].[LookupMeasureCodesEventlogs]", conn);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {

                        eventslog.Add(new SelectListItem { Text = myReader["Description"].ToString(), Value = myReader["Code"].ToString() });
                    }

                    conn.Close();
                    //return the list objects

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new SelectList(eventslog, "Value", "Text", "id");
        }

        private SelectList GetMachineModel()
        {
            try
            {
                string final = "";
                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();

                machineidfile = GetMachineIdfileForSelectedMachine();

                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();



                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();


                //machinecode6 = snPar6.ToString();

                for (var i = 0; i < snPar5.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = (machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);
                    SelectMachine.Add(new SelectListItem { Text = final.ToString(), Value = final.ToString() });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }




            return new SelectList(SelectMachine, "Value", "Text", "id");

        }

        public JsonResult MachineModelDropDownJason(string job)
        {
            string modelNoString = "";
            try
            {
                List<string> setMachineModel = new List<string>();
                if (!string.IsNullOrEmpty(job))
                {
                    string jobParameter = job.Substring(1, 5);
                    setMachineModel = GetModelDataTableBasedOnJobSelection(jobParameter);
                }

                string final = "";

                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();

                machineidfile = GetMachineIdfileForSelectedMachine();

                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();



                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();


                //machinecode6 = snPar6.ToString();

                for (var i = 0; i < snPar5.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = (machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);
                    modelNoString = string.Join(",", finalcode.ToArray());
                    //SelectMachine.Add(new SelectListItem { Text = final.ToString(), Value = final.ToString() });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }





            return Json(modelNoString, JsonRequestBehavior.AllowGet);

        }

        public List<string> GetModelDataTableBasedOnJobSelection(string job)
        {
            string jobNofromDropdown = job.Replace("multiselect-all", "");
            string[] jobNofrmDrp = jobNofromDropdown.Split(',');
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            DataTable dt = new DataTable();
            string sb = "";
            sb = "select distinct [5] from [dbo].[machineidfile] where [2] = " + job;
            List<string> modelNoForDropDown = new List<string>();
            try
            {
                using (var cmd = new SqlCommand(sb, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");

                    modelNoForDropDown = dt.AsEnumerable().Select(r => r.Field<string>("5")).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }




            return modelNoForDropDown;
        }
        private SelectList GetMachineSerialNo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string final = "";
                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();
                //parameter4Dt = GetParameter4SelectedMachine();
                //parameter6Dt = GetParameter6SelectedMachine();
                machineidfile = GetMachineIdfile();
                List<string> snPar3 = new List<string>();
                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();
                snPar3 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter3")).ToList();
                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();
                for (var i = 0; i < snPar3.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = (snPar3[i] + machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);
                    MachineSN.Add(new SelectListItem { Text = "SN" + final.ToString(), Value = "SN" + final.ToString() });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new SelectList(MachineSN, "Value", "Text", "id");

        }
        private SelectList GetMachineSerialNoNew(string SN, string Model1)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string final = "";
                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();
                //parameter4Dt = GetParameter4SelectedMachine();
                //parameter6Dt = GetParameter6SelectedMachine();
                machineidfile = GetMachineIdfile();
                List<string> snPar3 = new List<string>();
                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();
                snPar3 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter3")).ToList();
                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();
                for (var i = 0; i < snPar3.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = (snPar3[i] + machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);
                    MachineSN.Add(new SelectListItem { Text = "SN" + final.ToString(), Value = "SN" + final.ToString() });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new SelectList(MachineSN, "Value", "Text", "id");

        }

        private SelectList GetMaterial()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string final = "";
                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();
                //parameter4Dt = GetParameter4SelectedMachine();
                //parameter6Dt = GetParameter6SelectedMachine();
                machineidfile = GetMachineIdfile();
                List<string> snPar13 = new List<string>();


                snPar13 = machineidfile.AsEnumerable().Select(r => r.Field<string>("13")).ToList();

                for (var i = 0; i < snPar13.Count; i++)
                {


                    final = (snPar13[i]).ToString();
                    finalcode.Add(final);
                    Material.Add(new SelectListItem { Text = final.ToString(), Value = final.ToString() });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return new SelectList(Material, "Value", "Text", "id");


        }

        private SelectList GetDiameter()
        {
            try
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT DISTINCT RIGHT('000'+ISNULL([12],''),3) as [12]  FROM [dbo].[machineidfile]", conn);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {

                        Diameter.Add(new SelectListItem { Text = myReader["12"].ToString(), Value = myReader["12"].ToString() });
                    }

                    conn.Close();
                    //return the list objects

                }
            }
            catch (Exception e)
            {

            }

            return new SelectList(Diameter, "Value", "Text", "id");
        }

        public DataTable GetMachineIdfile()
        {
            DataTable dt = new DataTable();
            try
            {

                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                using (var cmd = new SqlCommand("select distinct [MachineId], [9], RIGHT('000'+ISNULL([3],''),3)as parameter3 ,[4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6,RIGHT('0000'+ISNULL([13],''),4) as [13] from [machineidfile]", con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return dt;
        }
        public DataTable GetMachineIdfileForSelectedMachine()
        {
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            using (var cmd = new SqlCommand("select distinct [uniqueid], [9], [4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6 from [machineidfile]", con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return dt;
        }

        public DataTable GetKMPTable()
        {
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            using (var cmd = new SqlCommand("select distinct * from  [dbo].[keymachineparameters]", con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return dt;
        }
        public DataTable GetKMPTableDateTime()
        {
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            using (var cmd = new SqlCommand(" select Convert(nvarchar(50),[localdate])+Convert(nvarchar(50),[localtime]) as DateTime from [dbo].[keymachineparameters]", con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return dt;
        }

        public string GetparameterFour(string parameter4)
        {
            string parameterFour = "";
            switch (parameter4)
            {

                case "1":
                    parameterFour = "PL";
                    break;
                case "2":
                    parameterFour = "CS";
                    break;
                case "3":
                    parameterFour = "RS";
                    break;
                case "4":
                    parameterFour = "FS";
                    break;
                default:
                    parameterFour = null;
                    break;
            }
            return parameterFour;
        }

        public string GetparameterSix(string parameter6)
        {
            string parameterFour = "";
            switch (parameter6)
            {

                case "1":
                    parameterFour = "H";
                    break;
                case "2":
                    parameterFour = "HD";
                    break;
                default:
                    parameterFour = null;
                    break;
            }
            return parameterFour;

        }

        public DataTable GetSerialNumberQueryBasedOnModel(string machine, string job, string mod, string startDate, string enDate, string columny, string axisX, string diameter, string material)
        {
            string JobNo = job.Replace("multiselect-all", "");
            string QueryJobNo = JobNo.Substring(1, JobNo.Length - 1);
            string ModNo = job.Replace("multiselect-all", "");
            string MachineSN = machine.Replace("multiselect-all", "");
            string[] ModNoArr = ModNo.Split(',');
            string[] JobNoArr = JobNo.Split(',');
            string[] serialNo = MachineSN.Split(',');
            List<string> para3 = new List<string>();
            List<string> para4 = new List<string>();
            List<string> para5 = new List<string>();
            List<string> para6 = new List<string>();
            foreach (string s in serialNo)
            {
                int length = s.Length;
                if (!string.IsNullOrEmpty(s))
                {
                    if (length == 13)
                    {
                        if (s[5] >= 'a' && s[5] <= 'z' || s[5] >= 'A' && s[5] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara4 = s.Substring(5, 2);
                            string snPara5 = s.Substring(7, 4);
                            para3.Add(snPara3);
                            para4.Add(snPara4);
                            para5.Add(snPara5);
                        }
                        if (s[11] >= 'a' && s[11] <= 'z' || s[11] >= 'A' && s[11] <= 'Z')
                        {
                            string snPara6 = s.Substring(11, 2);

                            para6.Add(snPara6);
                        }
                    }
                    else if (length == 11)
                    {
                        if (s[5] >= 'a' && s[5] <= 'z' || s[5] >= 'A' && s[5] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara4 = s.Substring(5, 2);
                            string snPara5 = s.Substring(7, 4);
                            para3.Add(snPara3);
                            para4.Add(snPara4);
                            para5.Add(snPara5);
                        }
                        else if (s[9] >= 'a' && s[9] <= 'z' || s[9] >= 'A' && s[9] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara5 = s.Substring(5, 4);
                            string snPara6 = s.Substring(9, 2);
                            para3.Add(snPara3);
                            para5.Add(snPara5);
                            para6.Add(snPara6);

                        }
                    }
                    else
                    {
                        string snPara3 = s.Substring(2, 3);
                        string snPara5 = s.Substring(5, 4);
                        para3.Add(snPara3);
                        para5.Add(snPara5);
                    }

                }

            }

            List<string> distinctPara3 = para3.Distinct().ToList();
            List<string> distinctPara4 = para4.Distinct().ToList();
            List<string> distinctPara5 = para5.Distinct().ToList();
            List<string> distinctPara6 = para6.Distinct().ToList();

            string parameter3 = string.Join(",", distinctPara3.ToArray());
            string parameter4 = string.Join(",", distinctPara4.ToArray());
            string parameter5 = string.Join(",", distinctPara5.ToArray());
            string parameter6 = string.Join(",", distinctPara6.ToArray());

            StringBuilder sb = new StringBuilder();
            sb.Append("select distinct [MachineId], [9], RIGHT('000'+ISNULL([3],''),3)as parameter3 , RIGHT('00000'+ISNULL([2],''),5)as parameter2  ,[4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6,RIGHT('00'+ISNULL([12],''),3) as parameter12,RIGHT('0000'+ISNULL([13],''),4) as parameter13 from [machineidfile] where [5] in (");
            sb.Append(parameter5.TrimStart('0'));
            sb.Append(") AND [12] in (" + diameter + ")  AND [13] in (" + material + ");");

            string query = sb.ToString();
            DataTable dt = new DataTable();
            dt = GetDataTABLEForGoogleChart(query);
            List<int> Uniqides = new List<int>();

            Uniqides = dt.AsEnumerable().Select(r => r.Field<int>("MachineId")).ToList();
            string Uniquide1 = string.Join(",", Uniqides.ToArray());
            DataTable data = new DataTable();
            string eventscol = null;
            data = GetKMPTableDataForGoogleChart(Uniquide1, startDate, enDate, columny, axisX, eventscol);



            return data;
        }

        public DataTable GetSerialNumberQueryBasedOnModelEvents(string machine, string job, string mod, string startDate, string enDate, string columny, string axisX, string eventscol, string diameter, string material)
        {
            string JobNo = job.Replace("multiselect-all", "");
            string QueryJobNo = JobNo.Substring(1, JobNo.Length - 1);
            string ModNo = job.Replace("multiselect-all", "");
            string MachineSN = machine.Replace("multiselect-all", "");
            string[] ModNoArr = ModNo.Split(',');
            string[] JobNoArr = JobNo.Split(',');
            string[] serialNo = MachineSN.Split(',');
            List<string> para3 = new List<string>();
            List<string> para4 = new List<string>();
            List<string> para5 = new List<string>();
            List<string> para6 = new List<string>();
            foreach (string s in serialNo)
            {
                int length = s.Length;
                if (!string.IsNullOrEmpty(s))
                {
                    if (length == 13)
                    {
                        if (s[5] >= 'a' && s[5] <= 'z' || s[5] >= 'A' && s[5] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara4 = s.Substring(5, 2);
                            string snPara5 = s.Substring(7, 4);
                            para3.Add(snPara3);
                            para4.Add(snPara4);
                            para5.Add(snPara5);
                        }
                        if (s[11] >= 'a' && s[11] <= 'z' || s[11] >= 'A' && s[11] <= 'Z')
                        {
                            string snPara6 = s.Substring(11, 2);

                            para6.Add(snPara6);
                        }
                    }
                    else if (length == 11)
                    {
                        if (s[5] >= 'a' && s[5] <= 'z' || s[5] >= 'A' && s[5] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara4 = s.Substring(5, 2);
                            string snPara5 = s.Substring(7, 4);
                            para3.Add(snPara3);
                            para4.Add(snPara4);
                            para5.Add(snPara5);
                        }
                        else if (s[9] >= 'a' && s[9] <= 'z' || s[9] >= 'A' && s[9] <= 'Z')
                        {
                            string snPara3 = s.Substring(2, 3);
                            string snPara5 = s.Substring(5, 4);
                            string snPara6 = s.Substring(9, 2);
                            para3.Add(snPara3);
                            para5.Add(snPara5);
                            para6.Add(snPara6);

                        }
                    }
                    else
                    {
                        string snPara3 = s.Substring(2, 3);
                        string snPara5 = s.Substring(5, 4);
                        para3.Add(snPara3);
                        para5.Add(snPara5);
                    }

                }

            }

            List<string> distinctPara3 = para3.Distinct().ToList();
            List<string> distinctPara4 = para4.Distinct().ToList();
            List<string> distinctPara5 = para5.Distinct().ToList();
            List<string> distinctPara6 = para6.Distinct().ToList();

            string parameter3 = string.Join(",", distinctPara3.ToArray());
            string parameter4 = string.Join(",", distinctPara4.ToArray());
            string parameter5 = string.Join(",", distinctPara5.ToArray());
            string parameter6 = string.Join(",", distinctPara6.ToArray());

            StringBuilder sb = new StringBuilder();
            sb.Append("select distinct [MachineId], [9], RIGHT('000'+ISNULL([3],''),3)as parameter3 , RIGHT('00000'+ISNULL([2],''),5)as parameter2  ,[4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6,RIGHT('00'+ISNULL([12],''),3) as parameter12,RIGHT('0000'+ISNULL([13],''),4) as parameter13 from [machineidfile] where [5] in (");
            sb.Append(parameter5.TrimStart('0'));
            sb.Append(") AND [12] in (" + diameter + ")  AND [13] in (" + material + ");");
            //sb.Append("select distinct [MachineId], [9], RIGHT('000'+ISNULL([3],''),3)as parameter3 , RIGHT('00000'+ISNULL([2],''),5)as parameter2  ,[4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6,RIGHT('0000'+ISNULL([13],''),4) as [13] from [machineidfile] where [5] in (");
            //sb.Append(parameter5);
            //sb.Append(")");

            string query = sb.ToString();
            DataTable dt = new DataTable();
            dt = GetDataTABLEForGoogleChart(query);
            List<int> Uniqides = new List<int>();

            Uniqides = dt.AsEnumerable().Select(r => r.Field<int>("MachineId")).ToList();
            string Uniquide1 = string.Join(",", Uniqides.ToArray());
            DataTable data = new DataTable();
            data = GetKMPTableDataForGoogleChart(Uniquide1, startDate, enDate, columny, axisX, eventscol);



            return data;
        }
        public string DataTableToJSONWithJavaScriptSerializer(string machine, string job, string mod, string startDate, string endDate, string syscolumn, string axisX, string diameter, string material)
        {


            DataTable table = GetSerialNumberQueryBasedOnModel(machine, job, mod, startDate, endDate, syscolumn, axisX, diameter, material);
            DateTime datetime = new DateTime();
            string columanname = table.Columns[0].ColumnName.ToString();
            string firstColumn = "logdatetime";

            string datetime1;

            StringBuilder JsonString = new StringBuilder();

            if (table.Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JsonString.Append("[");

                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j == 0)
                        {


                            if (string.Compare(columanname, firstColumn) == 0)
                            {

                                string finalconverted = GetDateConverted(table.Rows[i][j].ToString());

                                JsonString.Append("new Date(" + finalconverted + ")" + ",");

                            }
                            else
                            {
                                JsonString.Append(table.Rows[i][j] + ",");
                            }


                        }

                        else if (j < table.Columns.Count - 1 && j != 0)
                        {

                            JsonString.Append(table.Rows[i][j] + ",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JsonString.Append(table.Rows[i][j]);
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JsonString.Append("]");
                    }
                    else
                    {
                        JsonString.Append("],");
                    }
                }
                JsonString.Append("]");

                return JsonString.ToString();
            }
            else
            {
                return null;
            }

        }


        public string DataTableToJSONWithJavaScriptSerializerEvents(string machine, string job, string mod, string startDate, string endDate, string syscolumn, string axisX, string eventscol, string diameter, string material)
        {



            DataTable table = GetSerialNumberQueryBasedOnModelEvents(machine, job, mod, startDate, endDate, syscolumn, axisX, eventscol, diameter, material);
            DateTime datetime = new DateTime();
            string columanname = table.Columns[0].ColumnName.ToString();
            string firstColumn = "logdatetime";

            string datetime1;

            StringBuilder JsonString = new StringBuilder();

            if (table.Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JsonString.Append("[");

                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j == 0)
                        {


                            if (string.Compare(columanname, firstColumn) == 0)
                            {

                                string finalconverted = GetDateConverted(table.Rows[i][j].ToString());

                                JsonString.Append("new Date(" + finalconverted + ")" + ",");

                            }
                            else
                            {
                                JsonString.Append(table.Rows[i][j] + ",");
                            }


                        }

                        else if (j < table.Columns.Count - 1 && j != 0)
                        {

                            JsonString.Append(table.Rows[i][j] + ",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JsonString.Append(table.Rows[i][j]);
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JsonString.Append("]");
                    }
                    else
                    {
                        JsonString.Append("],");
                    }
                }
                JsonString.Append("]");

                return JsonString.ToString();
            }
            else
            {
                return null;
            }

        }

        public string DataTableToJsonEvents(string startdate, string enddate, string eventscol, string syscolumnx)
        {
            string uniquid = null;

            DataTable table = GetEvents(uniquid, startdate, enddate, eventscol, syscolumnx);
            //GetSerialNumberQueryBasedOnModel(eventslog, startdate, enddate, eventscol, syscolumnx);
            DateTime datetime = new DateTime();
            string columanname = table.Columns[0].ColumnName.ToString();
            string firstColumn = "logdatetime";

            string datetime1;

            StringBuilder JsonString = new StringBuilder();

            if (table.Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JsonString.Append("[");

                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j == 0)
                        {


                            if (string.Compare(columanname, firstColumn) == 0)
                            {

                                string finalconverted = GetDateConverted(table.Rows[i][j].ToString());

                                JsonString.Append("new Date(" + finalconverted + ")" + ",");

                            }
                            else
                            {
                                JsonString.Append(table.Rows[i][j] + ",");
                            }


                        }

                        else if (j < table.Columns.Count - 1 && j != 0)
                        {

                            JsonString.Append(table.Rows[i][j] + ",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JsonString.Append(table.Rows[i][j]);
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JsonString.Append("]");
                    }
                    else
                    {
                        JsonString.Append("],");
                    }
                }
                JsonString.Append("]");

                return JsonString.ToString();
            }
            else
            {
                return null;
            }

        }


        public DataTable GetEvents(string uniquid, string startdate, string enddate, string eventscol, string syscolumnx)
        {
            string sbnew = GetConvertedQueryEvent(uniquid, startdate, enddate, eventscol, syscolumnx, eventscol);
            // " select [logdatetime],[eventalarm] FROM [dbo].[MasterUploadedData] where  eventalarm is not null and  [logdatetime] between '2017-01-08 22:11:35.000' and '2017-01-08 22:14:39.000' ";
            //GetConvertedQuery(startdate, enddate, eventscol, syscolumnx);


            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(sbnew))
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                //sbnew.Append("select " + axisX + "," + columny + " from  [dbo].[keymachineparameters]  where CONVERT(VARCHAR(20),logdatetime,101) BETWEEN CONVERT(VARCHAR(20),'" + start + "',101) AND CONVERT(VARCHAR(20),'" + end + "',101) and  [uniqueid] in ('" + uniquid + "')");
                // string wherequery = sbnew.ToString();
                using (var cmd = new SqlCommand(sbnew, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }


            return dt;
        }
        public string GetDateConverted(string date)
        {

            string datetime1 = date;
            string[] datetime2 = datetime1.Split('-');
            string[] yyyytime = datetime2[2].Split(' ');
            string yyyy = datetime2[0].ToString();
            string dd = yyyytime[0].ToString();
            string mm = datetime2[1].ToString();
            string[] timarray = yyyytime[1].Split(':');
            string hh = timarray[0];
            string min = timarray[1];
            if (timarray.Length > 2)
            {
                string ss = timarray[2].ToString();
                string sss = ss.Substring(0, 2);

                string dateconverted = yyyy + "/" + mm + "/" + dd + ", " + hh + ":" + min + ":" + sss;

                string finalconverted = ((dateconverted).Replace("/", ", ")).Replace(":", ", ");
                return finalconverted;
            }

            string dateconverted1 = yyyy + "/" + mm + "/" + dd + ", " + hh + ":" + min + ":";

            string finalconverted1 = ((dateconverted1).Replace("/", ", ")).Replace(":", ", ");
            return finalconverted1;
        }


        public DataTable GetStartDateEndDate(string startdate, string enddate)
        {
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            using (var cmd = new SqlCommand(" select Convert(nvarchar(50),[localdate])+Convert(nvarchar(50),[localtime]) as DateTime from [dbo].[keymachineparameters]", con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return dt;
        }

        public static DataTable GetDataTABLEForGoogleChart(string query)
        {
            DataTable table = new DataTable();

            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            string sb = query;

            using (var cmd = new SqlCommand(sb, con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(table);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return table;
        }

        public DataTable GetKMPTableDataForGoogleChart(string uniquid, string startDate, string enDate, string columny, string axisX, string eventscol)
        {
            string sbnew = null;
            if (string.IsNullOrEmpty(eventscol))
            {
                sbnew = GetConvertedQuery(uniquid, startDate, enDate, columny, axisX);

            }
            else
            {
                sbnew = GetConvertedQueryEvent(uniquid, startDate, enDate, columny, axisX, eventscol);
            }


            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(uniquid))
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                //sbnew.Append("select " + axisX + "," + columny + " from  [dbo].[keymachineparameters]  where CONVERT(VARCHAR(20),logdatetime,101) BETWEEN CONVERT(VARCHAR(20),'" + start + "',101) AND CONVERT(VARCHAR(20),'" + end + "',101) and  [uniqueid] in ('" + uniquid + "')");
                // string wherequery = sbnew.ToString();
                using (var cmd = new SqlCommand(sbnew, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }

            DataTable dtnew = new DataTable();
            if (!string.IsNullOrWhiteSpace(eventscol))
            {
                string column = axisX + ",eventlogmessage," + columny;
                String[] selectedColumns = column.Split(',');


                dtnew = new DataView(dt).ToTable(false, selectedColumns);
                return dtnew;
                //return dt;
            }
            else
            {
                string column = axisX + "," + columny;
                String[] selectedColumns = column.Split(',');


                dtnew = new DataView(dt).ToTable(false, selectedColumns);
                return dtnew;
            }


        }

        public string GetConvertedQueryold(string uniquid, string startDate, string enDate, string columny, string axisX)
        {
            DateTime dtstart = new DateTime();
            DateTime dtend = new DateTime();
            StringBuilder sbnew = new StringBuilder();
            StringBuilder sbuniqid = new StringBuilder();
            //dtstart = Convert.ToDateTime(startDate);
            dtstart = DateTime.Parse(startDate);
            //dtend = Convert.ToDateTime(enDate);
            dtend = DateTime.Parse(enDate);
            string start = dtstart.ToString("yyyy-MM-dd  HH:mm:ss");

            string end = dtend.ToString("yyyy-MM-dd  HH:mm:ss");
            var seconds = System.Math.Abs((dtstart - dtend).TotalSeconds);
            long ss = Convert.ToInt64(seconds);

            int countSpacesstart = startDate.Count(Char.IsWhiteSpace);
            int countSpacesend = enDate.Count(Char.IsWhiteSpace);
            int count1 = columny.Count(x => x == ',');
            string axisXOrderbyname = "";
            StringBuilder yaxisvalue = new StringBuilder();
            StringBuilder yaxisNotNull = new StringBuilder();
            string[] yaxis = new string[count1];
            string axisXvalues = "";
            yaxis = columny.Split(',');

            if (ss > 21600)
            {
                //for (int i = 0; i < count1; i++)
                //{
                //    yaxisNotNull.Append(yaxis[i] + " IS NOT NULL AND ");
                //}
                // string yaxisvalueNotNull = yaxisNotNull.ToString();
                for (int i = 0; i <= count1; i++)
                {
                    yaxisvalue.Append("MAX(CAST([" + yaxis[i] + "] AS decimal(18, 2))) AS [" + yaxis[i] + "],");
                }
                string columnYValue = yaxisvalue.ToString();
                string columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = "CAST(" + axisX + " AS INT) AS " + axisX + " ";
                    axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);
                }

                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }

                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                        //sbnew.Append("select min(" + axisXvalues + ") as logdatetime, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime,DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime)  order by logdatetime asc ");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "') and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
            }
            else
            {
                for (int i = 0; i < count1; i++)
                {
                    yaxisNotNull.Append(yaxis[i] + " IS NOT NULL AND ");
                }
                string yaxisvalueNotNull = yaxisNotNull.ToString();
                for (int i = 0; i <= count1; i++)
                {
                    yaxisvalue.Append("CAST([" + yaxis[i] + "] AS decimal(18, 2)) AS [" + yaxis[i] + "],");
                }
                string columnYValue = yaxisvalue.ToString();
                string columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = "CAST(" + axisX + " AS INT) AS " + axisX + " ";
                    axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);
                }

                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }

                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisXvalues + " asc");
                        //sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisXvalues + " asc");
                        //sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisXvalues + " asc");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                        //sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "') and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
            }
            return sbnew.ToString();
        }


        public string GetConvertedQuery(string uniquid, string startDate, string enDate, string columny, string axisX)
        {
            string columnYValue = null;
            string columnYvalues = null;
            DateTime dtstart = new DateTime();
            DateTime dtend = new DateTime();
            StringBuilder sbnew = new StringBuilder();
            StringBuilder sbuniqid = new StringBuilder();
            //dtstart = Convert.ToDateTime(startDate);
            //dtend = Convert.ToDateTime(enDate);
            dtstart = Convert.ToDateTime(startDate, System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            //dtend = Convert.ToDateTime(enDate);
            dtend = Convert.ToDateTime(enDate, System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            string start = dtstart.ToString("yyyy-MM-dd  HH:mm:ss");

            string end = dtend.ToString("yyyy-MM-dd  HH:mm:ss");
            var seconds = System.Math.Abs((dtstart - dtend).TotalSeconds);
            long ss = Convert.ToInt64(seconds);

            int countSpacesstart = startDate.Count(Char.IsWhiteSpace);
            int countSpacesend = enDate.Count(Char.IsWhiteSpace);
            int count1 = columny.Count(x => x == ',');
            string axisXOrderbyname = "";
            StringBuilder yaxisvalue = new StringBuilder();
            StringBuilder yaxisNotNull = new StringBuilder();
            string[] yaxis = new string[count1];
            string axisXvalues = "";
            yaxis = columny.Split(',');

            if (ss > 21600)
            {



                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    for (int i = 0; i <= count1; i++)
                    {
                        yaxisvalue.Append("MAX(CAST([" + yaxis[i] + "] AS decimal(18, 2))) AS [" + yaxis[i] + "],");
                    }
                    columnYValue = yaxisvalue.ToString();
                    columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = "CAST(" + axisX + " AS INT) AS " + axisX + " ";
                    axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);

                    for (int i = 0; i <= count1; i++)
                    {
                        yaxisvalue.Append("CAST([" + yaxis[i] + "] AS decimal(18, 2)) AS [" + yaxis[i] + "],");
                    }
                    columnYValue = yaxisvalue.ToString();
                    columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                }

                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }

                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                        //sbnew.Append("select min(" + axisXvalues + ") as logdatetime, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisX + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime,DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime)  order by logdatetime asc ");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by " + axisX + " asc");
                    }

                }
            }
            else
            {

                for (int i = 0; i < count1; i++)
                {
                    yaxisNotNull.Append(yaxis[i] + " IS NOT NULL AND ");
                }
                string yaxisvalueNotNull = yaxisNotNull.ToString();

                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    for (int i = 0; i <= count1; i++)
                    {
                        yaxisvalue.Append("CAST([" + yaxis[i] + "] AS decimal(18, 2)) AS [" + yaxis[i] + "],");
                    }
                    columnYValue = yaxisvalue.ToString();
                    columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = "CAST(" + axisX + " AS decimal(18, 2)) AS " + axisX + " ";
                    axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);

                    for (int i = 0; i <= count1; i++)
                    {
                        yaxisvalue.Append("CAST([" + yaxis[i] + "] AS decimal(18, 2)) AS [" + yaxis[i] + "],");
                    }
                    columnYValue = yaxisvalue.ToString();
                    columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                }

                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }

                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisXvalues + " asc");
                        //sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  order by " + axisXvalues + " asc");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") AND " + axisX + " is not null  order by " + axisX + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  AND " + axisX + " is not null   order by " + axisX + " asc");
                    }

                }
            }
            return sbnew.ToString();
        }

        public string GetConvertedQueryEvent(string uniquid, string startDate, string enDate, string columny, string axisX, string eventscol)
        {
            DateTime dtstart = new DateTime();
            DateTime dtend = new DateTime();
            StringBuilder sbnew = new StringBuilder();
            StringBuilder sbuniqid = new StringBuilder();
            //dtstart = Convert.ToDateTime(startDate);
            //dtend = Convert.ToDateTime(enDate);
            dtstart = Convert.ToDateTime(startDate, System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            dtend = Convert.ToDateTime(enDate, System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            string start = dtstart.ToString("yyyy-MM-dd  HH:mm:ss");

            string end = dtend.ToString("yyyy-MM-dd  HH:mm:ss");

            var seconds = System.Math.Abs((dtstart - dtend).TotalSeconds);
            long ss = Convert.ToInt64(seconds);


            int countSpacesstart = startDate.Count(Char.IsWhiteSpace);
            int countSpacesend = enDate.Count(Char.IsWhiteSpace);
            int count1 = columny.Count(x => x == ',');
            string axisXOrderbyname = "";
            StringBuilder yaxisvalue = new StringBuilder();
            StringBuilder yaxisNotNull = new StringBuilder();
            string[] yaxis = new string[count1];
            string axisXvalues = "";
            yaxis = columny.Split(',');

            if (ss > 21600)
            {
                //for (int i = 0; i < count1; i++)
                //{
                //    yaxisNotNull.Append(yaxis[i] + " IS NOT NULL AND ");
                //}
                //string yaxisvalueNotNull = yaxisNotNull.ToString();

                for (int i = 0; i <= count1; i++)
                {
                    yaxisvalue.Append("MAX(CAST([" + yaxis[i] + "] AS decimal(18, 2))) AS [" + yaxis[i] + "],");
                }
                string columnYValue = yaxisvalue.ToString();
                string columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);

                //for (int i = 0; i <= count1; i++)
                //{
                //    yaxisvalue.Append(yaxis[i] + ",");
                //}
                //string eventtext = yaxisvalue.ToString().Substring(0, yaxisvalue.ToString().Length - 1);
                //string columnYValue = yaxisvalue.ToString();
                //string columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    axisXOrderbyname = axisXvalues;
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = axisX + " ";
                    axisXOrderbyname = axisXvalues;
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);
                }

                // string uniquid = null;
                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }

                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime, count(eventlogmessage) eventlogmessage, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                        //sbnew.Append("select min(FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss')) as logdatetime, count(eventlogmessage) eventlogmessage, DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ")  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime) order by logdatetime asc ");
                        //sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND  [MachineId] in (" + finaluniqid + ")     order by " + axisXvalues + " asc");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select min(CONVERT(VARCHAR(30), " + axisXvalues + ", 121)) as logdatetime ,count(eventlogmessage) eventlogmessage,DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime)  order by logdatetime asc ");
                        //sbnew.Append("select min(" + axisXvalues + ")  as logdatetime,count(eventlogmessage) eventlogmessage,DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime)  order by logdatetime asc ");
                        //sbnew.Append("select min(FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss'))  as logdatetime,count(eventlogmessage) eventlogmessage,DATEPART(YEAR,logdatetime) AS year , DATEPART(MONTH,logdatetime)  AS MONTH,DATEPART(DAY,logdatetime)  AS day,DATEPART(HOUR,logdatetime)  AS hours," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  GROUP BY  DATEPART(YEAR,logdatetime),DATEPART(MONTH,logdatetime) ,DATEPART(DAY,logdatetime) ,DATEPART(HOUR,logdatetime)  order by logdatetime asc ");
                        // sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + ", " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "') and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
            }
            else
            {

                for (int i = 0; i < count1; i++)
                {
                    yaxisNotNull.Append(yaxis[i] + " IS NOT NULL AND ");
                }
                string yaxisvalueNotNull = yaxisNotNull.ToString();
                for (int i = 0; i <= count1; i++)
                {
                    yaxisvalue.Append(yaxis[i] + ",");
                }
                string eventtext = yaxisvalue.ToString().Substring(0, yaxisvalue.ToString().Length - 1);
                string columnYValue = yaxisvalue.ToString();
                string columnYvalues = columnYValue.Substring(0, columnYValue.Length - 1);
                if (axisX.Equals("logdatetime"))
                {
                    axisXvalues = "logdatetime";
                    axisXOrderbyname = axisXvalues;
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ")+2);
                }
                else
                {
                    axisXvalues = axisX + " ";
                    axisXOrderbyname = axisXvalues;
                    // axisXOrderbyname = axisXvalues.Substring(axisXvalues.IndexOf("AS ") + 11);
                }

                // string uniquid = null;
                int count = uniquid.Count(f => f == ',');
                string[] uniq = new string[count];

                string uniqides;
                string finaluniqid;

                if (uniquid.Contains(','))
                {

                    for (int i = 0; i < count; i++)
                    {
                        uniq = uniquid.Split(',');
                    }

                }
                foreach (string s in uniq)
                {
                    sbuniqid.Append("'" + s + "'");
                    sbuniqid.Append(",");
                }
                if (uniq.Length > 0)
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND  [MachineId] in (" + finaluniqid + ")     order by " + axisXvalues + " asc");
                        //sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND  [MachineId] in (" + finaluniqid + ")     order by " + axisXvalues + " asc");
                    }
                    else
                    {
                        uniqides = sbuniqid.ToString();
                        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
                else
                {
                    if (axisX.Equals("logdatetime"))
                    {
                        sbnew.Append("select CONVERT(VARCHAR(30), " + axisXvalues + ", 121) as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                        //sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                    }
                    else
                    {

                        sbnew.Append("select " + axisXvalues + ", " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "') and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                    }

                }
                //if (uniq.Length > 0)
                //{
                //    if (axisX.Equals("logdatetime"))
                //    {
                //        uniqides = sbuniqid.ToString();
                //        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                //        sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND  [MachineId] in (" + finaluniqid + ")     order by " + axisXvalues + " asc");
                //    }
                //    else
                //    {
                //        uniqides = sbuniqid.ToString();
                //        finaluniqid = uniqides.Substring(0, uniqides.Length - 1);
                //        sbnew.Append("select " + axisXvalues + "," + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND   [MachineId] in (" + finaluniqid + ") and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                //    }

                //}
                //else
                //{
                //    if (axisX.Equals("logdatetime"))
                //    {
                //        sbnew.Append("select FORMAT(" + axisXvalues + ",'MM/dd/yyyy HH:mm:ss') as logdatetime, eventlogmessage, " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "')  order by logdatetime asc");
                //    }
                //    else
                //    {

                //        sbnew.Append("select " + axisXvalues + ", " + columnYvalues + " from  [dbo].[MasterUploadedData]  where logdatetime BETWEEN '" + start + "' AND '" + end + "' AND    [MachineId] in ('" + uniquid + "') and " + axisXOrderbyname + " is not null order by " + axisXOrderbyname + " asc");
                //    }

                //}
            }



            return sbnew.ToString();
        }

        public DataTable GetKMPTableDataForGoogleChartdaterange(string startdate, string enddate, string columnname, string sysColumnX)
        {
            string colAxisY = columnname.ToLower();
            int count = colAxisY.Count(x => x == ',');
            DateTime dtstart = new DateTime();
            DateTime dtend = new DateTime();
            dtstart = Convert.ToDateTime(startdate);
            dtend = Convert.ToDateTime(enddate);
            string start = dtstart.ToString("MM/dd/yyyy  hh:mm:ss");
            string end = dtend.ToString("MM/dd/yyyy  hh:mm:ss");
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT KM." + sysColumnX + ",");
            sb.Append(" KM.");
            sb.Append(colAxisY);
            sb.Append(" FROM [dbo].[keymachineparameters] as KM where ");
            sb.Append("CONVERT(VARCHAR(20),KM.logdatetime,101) BETWEEN ");
            sb.Append("CONVERT(VARCHAR(20),'");
            sb.Append(start);
            sb.Append("',101)");

            sb.Append(" AND CONVERT(VARCHAR(20),'");
            sb.Append(end);
            sb.Append("',101)");



            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(startdate))
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                string wherequery = sb.ToString();
                // string wherequery = "select [uniqueid],[logdatetime],[m12904],[m12901],[m12908] from  [dbo].[keymachineparameters] where [uniqueid] in (" + uniquid + ")";
                using (var cmd = new SqlCommand(wherequery, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            else
            {
                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                string wherequery = "select distinct * from  [dbo].[keymachineparameters]";
                using (var cmd = new SqlCommand(wherequery, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }



            return dt;
        }

        public JsonResult FillMachine(string ddlJobNo)
        {
            string modelNoString = "";
            try
            {
                string jobParameter = string.Empty;
                List<string> setMachineModel = new List<string>();
                if (!string.IsNullOrEmpty(ddlJobNo))
                {
                    jobParameter = ddlJobNo.Substring(1);
                    setMachineModel = GetModelDataTableBasedOnJobSelection(jobParameter);
                }

                string final = "";

                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();

                machineidfile = GetMachineIdfileForSelectedJob(jobParameter);

                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();



                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();


                //machinecode6 = snPar6.ToString();

                for (var i = 0; i < snPar5.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = (machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);
                    modelNoString = string.Join(",", finalcode.ToArray());
                    //SelectMachine.Add(new SelectListItem { Text = final.ToString(), Value = final.ToString() });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }





            return Json(modelNoString, JsonRequestBehavior.AllowGet);

        }
        public DataTable GetMachineIdfileForSelectedJob(string job)
        {
            DataTable dt = new DataTable();
            string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            using (var cmd = new SqlCommand("select distinct RIGHT('000'+ISNULL([5],''),4)as parameter5, [9], [4] as parameter4 ,[6] as parameter6 from [machineidfile] where [2]=" + job, con))
            {
                Console.WriteLine("command created successfuly");

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                con.Open();
                Console.WriteLine("connection opened successfuly");
                adapt.Fill(dt);
                con.Close();
                Console.WriteLine("connection closed successfuly");
            }
            return dt;
        }

        public ActionResult getMachineSerialNo(string machineSerialNo, string jobno)
        {
            string mSerialNo = "";
            string serialNo = machineSerialNo;
            string Stack = serialNo;
            string machineSerialNo1 = null;
            int modelNoIsAlpha = Regex.Matches(Stack, @"[a-zA-Z]").Count;
            if (modelNoIsAlpha > 0)
            {
                bool isLetter = !String.IsNullOrEmpty(Stack) && Char.IsLetter(Stack[0]);
                if(isLetter)
                {
                    machineSerialNo1 = Stack.Substring(2, Stack.Length - 2);
                }
                else
                {
                    machineSerialNo1 = Stack.Substring(0, Stack.Length - 2);
                }
               

            }
            else
            {
                machineSerialNo1 = Stack;
            }
            //string[] digits = Regex.Split(Stack, @"\D+");
            //if(Stack.Contains)
            //string machineSerialNo1 = digits[0].ToString();



            try
            {
                StringBuilder sb = new StringBuilder();

                string final = "";
                List<string> finalcode = new List<string>();
                List<string> combine = new List<string>();

                DataTable dt = new DataTable();
                DataTable parameter4Dt = new DataTable();
                DataTable parameter6Dt = new DataTable();
                DataTable machineidfile = new DataTable();
                //parameter4Dt = GetParameter4SelectedMachine();
                //parameter6Dt = GetParameter6SelectedMachine();
                machineidfile = GetMachineId(machineSerialNo1, jobno);
                List<string> snPar3 = new List<string>();
                List<string> snPar4 = new List<string>();
                List<string> snPar5 = new List<string>();
                List<string> snPar6 = new List<string>();
                List<string> machineidcode4 = new List<string>();
                List<string> machineidcode6 = new List<string>();
                snPar3 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter3")).ToList();
                snPar4 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter4")).ToList();
                snPar5 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter5")).ToList();
                snPar6 = machineidfile.AsEnumerable().Select(r => r.Field<string>("parameter6")).ToList();
                for (var i = 0; i < snPar3.Count; i++)
                {
                    if (snPar4[i] == "0")
                    {
                        machineidcode4.Add(GetparameterFour("0"));

                    }
                    else if (snPar4[i] == "1")
                    {
                        machineidcode4.Add(GetparameterFour("1"));

                    }
                    else if (snPar4[i] == "2")
                    {
                        machineidcode4.Add(GetparameterFour("2"));

                    }
                    else if (snPar4[i] == "3")
                    {
                        machineidcode4.Add(GetparameterFour("3"));

                    }
                    else
                    {

                        machineidcode4.Add(GetparameterSix("4"));
                    }

                    if (snPar6[i] == "0")
                    {

                        machineidcode6.Add(GetparameterSix("0"));
                    }
                    else if (snPar6[i] == "1")
                    {

                        machineidcode6.Add(GetparameterSix("1"));
                    }
                    else
                    {

                        machineidcode6.Add(GetparameterSix("2"));
                    }

                    final = "SN" + (snPar3[i] + machineidcode4[i] + snPar5[i] + machineidcode6[i]).ToString();
                    finalcode.Add(final);

                    mSerialNo = string.Join(",", finalcode.ToArray());

                }
                var distinctNames = finalcode.Distinct();
                mSerialNo = string.Join(",", distinctNames.ToArray());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //return new SelectList(MachineSN, "Value", "Text", "id");
            return Json(mSerialNo, JsonRequestBehavior.AllowGet);

        }

        public DataTable GetMachineId(string serialNo, string jobno)
        {
            DataTable dt = new DataTable();
            try
            {

                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                string sql = "select distinct [MachineId], [9], RIGHT('000'+ISNULL([3],''),3)as parameter3 ,[4] as parameter4 ,RIGHT('000'+ISNULL([5],''),4)as parameter5,[6] as parameter6,RIGHT('0000'+ISNULL([13],''),4) as [13] from [machineidfile] where [5]=" + serialNo + " AND [2]= " + jobno.TrimStart('J') + ";";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return dt;
        }
        public ActionResult FillDiameter(string modelno, string srNo, string machinejobno)
        {
            string modelSn = null;
            int length = srNo.Count(x => x == ',');
            string[] SNstring = new string[length + 1];
            string[] SN = new string[length + 1];
            if (srNo.Contains(','))
            {
                SN = srNo.Split(',');
                // int length = srNo.Count(x => x == ',');

                for (int i = 0; i <= length; i++)
                {
                    SNstring[i] = "'" + (SN[i].Substring(2, 3)).TrimStart('0') + "'";
                }
                modelSn = string.Join(",", SNstring);
            }
            else
            {
                modelSn = srNo.Substring(2, 3).TrimStart('0');
            }

            string mSerialNo = "";
            string serialNo = modelno;
            string Stack = serialNo;
            // string[] digits = Regex.Split(Stack, @"\D+");
            //string machineSerialNo1 = digits[0].ToString();
            string machineSerialNo1 = null;
            //bool result = Stack.Any(x => !char.IsLetter(x));
            //if (result)
            int modelNoIsAlpha = Regex.Matches(Stack, @"[a-zA-Z]").Count;
            if (modelNoIsAlpha > 0)
            {
                bool isLetter = !String.IsNullOrEmpty(Stack) && Char.IsLetter(Stack[0]);
                if (isLetter)
                {
                    machineSerialNo1 = Stack.Substring(2, Stack.Length - 2);
                }
                else
                {
                    machineSerialNo1 = Stack.Substring(0, Stack.Length - 2);
                }

                //machineSerialNo1 = Stack;
               

            }
            else
            {
                machineSerialNo1 = Stack;
                // machineSerialNo1 = Stack.Substring(2, Stack.Length - 2);

            }
            DataTable dt = new DataTable();
            try
            {

                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);
                con.Open();
                string sql = "SELECT DISTINCT RIGHT('000'+ISNULL([12],''),3)  FROM [dbo].[machineidfile] where [5]= " + machineSerialNo1.TrimStart('0') + " AND  [3] in (" + modelSn + ") AND [2] = "+ machinejobno.TrimStart('J') + ";";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);


                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string json = JsonConvert.SerializeObject(dt);

            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public ActionResult FillMaterial(string modelno, string srNo, string machinejobno)
        {
            string modelSn = null;
            int length = srNo.Count(x => x == ',');
            string[] SNstring = new string[length + 1];
            string[] SN = new string[length + 1];
            if (srNo.Contains(','))
            {
                SN = srNo.Split(',');
                // int length = srNo.Count(x => x == ',');

                for (int i = 0; i <= length; i++)
                {
                    SNstring[i] = "'" + (SN[i].Substring(2, 3)).TrimStart('0') + "'";
                }
                modelSn = string.Join(",", SNstring);
            }
            else
            {
                modelSn = srNo.Substring(2, 3).TrimStart('0');
            }

            string serialNo = modelno;
            string Stack = serialNo;
            string machineSerialNo1 = null;
            //bool result = Stack.Any(x => !char.IsLetter(x));
            //if (result)
            int modelNoIsAlpha = Regex.Matches(Stack, @"[a-zA-Z]").Count;
            if (modelNoIsAlpha > 0)
            {
                //machineSerialNo1 = Stack;
                bool isLetter = !String.IsNullOrEmpty(Stack) && Char.IsLetter(Stack[0]);
                if (isLetter)
                {
                    machineSerialNo1 = Stack.Substring(2, Stack.Length - 2);
                }
                else
                {
                    machineSerialNo1 = Stack.Substring(0, Stack.Length - 2);
                }
              

            }
            else
            {
                // machineSerialNo1 = Stack.Substring(2, Stack.Length - 2);
                machineSerialNo1 = Stack;
            }
            //string[] digits = Regex.Split(Stack, @"\D+");
            //string machineSerialNo1 = digits[0].ToString();
            DataTable dt = new DataTable();
            try
            {

                string conStr = ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
                SqlConnection con = new SqlConnection(conStr);

                string sql = "select distinct RIGHT('0000'+ISNULL([13],''),4) as [13] from [machineidfile] where [5]=" + machineSerialNo1.TrimStart('0') + " AND  [3] in (" + modelSn + ") AND [2] = "+ machinejobno.TrimStart('J') +";";

                using (var cmd = new SqlCommand(sql, con))
                {
                    Console.WriteLine("command created successfuly");

                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                    con.Open();
                    Console.WriteLine("connection opened successfuly");
                    adapt.Fill(dt);
                    con.Close();
                    Console.WriteLine("connection closed successfuly");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            string json = JsonConvert.SerializeObject(dt);

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public string GetAvailableDateForJob(string jobNo, string modelNo, string serialNo,string diameter,string material)
        {
            string availableStartEndDate;
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString()))
            {
                sqlConn.Open();
                using (SqlCommand sqlCmd = new SqlCommand("sp_AvailableDatesForJobs", sqlConn))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@jobNo", SqlDbType.VarChar).Value = jobNo;
                    sqlCmd.Parameters.Add("@modelNo", SqlDbType.VarChar).Value = modelNo;
                    sqlCmd.Parameters.Add("@serialNo", SqlDbType.VarChar).Value = serialNo;
                    sqlCmd.Parameters.Add("@diameter", SqlDbType.VarChar).Value = diameter;
                    sqlCmd.Parameters.Add("@material", SqlDbType.VarChar).Value = material;

                   // SqlParameter result1 = sqlCmd.Parameters.Add("@availableDates", SqlDbType.VarChar);
                    //result1.Direction = ParameterDirection.ReturnValue;
                    sqlCmd.Parameters.Add("@availableDates", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                    
                    sqlCmd.ExecuteNonQuery();
                    availableStartEndDate = sqlCmd.Parameters["@availableDates"].Value.ToString();
                    sqlConn.Close();
                    
                }
            }


            //using (SqlConnection con = new SqlConnection(dc.Con))
            //{
            //    using (SqlCommand cmd = new SqlCommand("sp_Add_contact", con))
            //    {
            //        cmd.CommandType = CommandType.StoredProcedure;

            //        cmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = txtFirstName.Text;
            //        cmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = txtLastName.Text;

            //        con.Open();
            //        cmd.ExecuteNonQuery();
            //    }
            //}
            return availableStartEndDate;
        }
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["LaidigDbConStr"].ConnectionString;
        }
    }
}