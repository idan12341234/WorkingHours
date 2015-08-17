using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WorkingHours
{
    /// <summary>
    /// Summary description for AppHandler
    /// </summary>
    public class AppHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string type = context.Request["type"];

            switch (type)
            {
                case "fillDBWithTestData":
                    fillDBWithTestData(context);
                    break;
                case "getDataForTable":
                    getDataForTable(context);
                    break;
                case "getDateForGraph":
                    break;
                case "monthAndYearHistory":
                    monthAndYearHistory(context);
                    break;
                case "getDataForGraph":
                    getDateForGraph(context);
                    break;
            }
        }

        private void getDateForGraph(HttpContext context)
        {
            string response = ""; 
            int month = Convert.ToInt32(context.Request["month"]);
            int year = Convert.ToInt32(context.Request["year"]);
            ArrayList graphObject = new ArrayList();
           
            using (SqlConnection conn = new SqlConnection(AppLogic.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select dayInMonth, checkInDate, checkOutDate from workingDayHours where userId = @userId and year = @year and month = @month";
                cmd.Parameters.Add("@userId", System.Data.SqlDbType.VarChar).Value = AppLogic.UserIdForTest;
                cmd.Parameters.Add("@month", System.Data.SqlDbType.Int).Value = month;
                cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
                conn.Open();
                SqlDataReader r = cmd.ExecuteReader();
                List<string> listLabels = new List<string>();
                List<decimal> dataValues = new List<decimal>();
                ArrayList dataset = new ArrayList();
                while (r.Read())
                {
                    DateTime d = new DateTime(year, month, Convert.ToInt32(r["dayInMonth"]));
                    listLabels.Add(d.ToString("ddd d/M"));
                    decimal value = 0;
                    if (r["checkInDate"] != DBNull.Value && r["checkOutDate"] != DBNull.Value)
                    {
                        value = Math.Round((decimal)(Convert.ToDateTime(r["checkOutDate"]) - Convert.ToDateTime(r["checkInDate"])).TotalHours, 2, MidpointRounding.AwayFromZero);
                    }

                    dataValues.Add(value);
                }

                dataset.Add(new
                {
                    label = "working hours graph",
                    data = dataValues
                });


                graphObject.Add(new
                {
                    labels = listLabels,
                    datasets = dataset
                });
            }

            response = JsonConvert.SerializeObject(graphObject);
            context.Response.Write(response);
            context.Response.End();

        }

        private void monthAndYearHistory(HttpContext context)
        {
            string response = "";
            ArrayList arr = new ArrayList();
            using (SqlConnection conn = new SqlConnection(AppLogic.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select distinct year, month from workingDayHours where userId = @userId order by year asc, month asc";
                cmd.Parameters.Add("@userId", System.Data.SqlDbType.VarChar).Value = AppLogic.UserIdForTest;
                conn.Open();
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    arr.Add(new { Year = r["year"].ToString(), Month = r["Month"].ToString() });
                }
            }

            response = JsonConvert.SerializeObject(arr);
            context.Response.Write(response);
            context.Response.End();
        }

        private void fillDBWithTestData(HttpContext context)
        {
            AppLogic.InsertDataForTest();
            context.Response.Write("Test data filled successfully");
            context.Response.End();
        }

        //table page
        private void getDataForTable(HttpContext context)
        {
            string response = "";
            ArrayList arr = new ArrayList();
            string year = context.Request["year"];
            string month = context.Request["month"];
            string str = AppLogic.ConnectionString;
            using (SqlConnection conn = new SqlConnection(AppLogic.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select * from workingDayHours where year = @year and month = @month and userId = @userId order by dayInMonth asc";
                cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
                cmd.Parameters.Add("@month", System.Data.SqlDbType.Int).Value = month;
                cmd.Parameters.Add("@userId", System.Data.SqlDbType.VarChar).Value = AppLogic.UserIdForTest;
                conn.Open();
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string dateFormatted = "";
                    string checkinTime = "";
                    string checkoutTime = "";
                    int totalHours = 0;
                    int totalMinutes = 0;
                    string totalTime = "";

                    if (r["checkInDate"] != DBNull.Value)
                    {
                        DateTime date = Convert.ToDateTime(r["checkInDate"]);
                        checkinTime = date.ToString("hh:mm tt");
                        dateFormatted = date.ToString("ddd d/M/yy");
                    }

                    if (r["checkOutDate"] != DBNull.Value)
                    {
                        DateTime date = Convert.ToDateTime(r["checkOutDate"]);
                        checkoutTime = date.ToString("hh:mm tt");

                        totalMinutes = (int)(Convert.ToDateTime(r["checkOutDate"]) - Convert.ToDateTime(r["checkInDate"])).TotalMinutes;
                        totalHours = TimeSpan.FromMinutes(totalMinutes).Hours;
                        totalMinutes = totalMinutes - (TimeSpan.FromHours(totalHours).Minutes * 60);

                        totalTime = string.Format("{0}:{1}", totalHours > 9 ? totalHours.ToString() : "0" + totalHours, totalMinutes > 9 ? totalMinutes.ToString() : "0" + totalMinutes);

                    }

                    int totalWorkTime = 0;
                    if (r["checkInDate"] != DBNull.Value && r["checkOutDate"] != DBNull.Value)
                    {
                        totalWorkTime = (Convert.ToDateTime(r["checkInDate"]) - Convert.ToDateTime(r["checkOutDate"])).Minutes;
                    }

                    arr.Add(new { Date = dateFormatted, Start = checkinTime, CheckoutTime = checkoutTime, Total = totalTime, TotalMinutes = totalWorkTime });
                }

                if (arr.Count > 0)
                {
                    response = JsonConvert.SerializeObject(arr);
                }
            }

            context.Response.Write(response);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}