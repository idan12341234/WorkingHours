using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WorkingHours
{
    public static class AppLogic
    {
        public static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["WorkingHours"].ConnectionString; } }
        public static string UserIdForTest { get { return "1234"; } }

        public static void InsertDataForTest()
        {
            DateTime date = new DateTime(2015, 1, 1);
            Random r = new Random();
            while (date <= DateTime.Today)
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = @"insert into workingDayHours(userId, year, month, dayInMonth, checkInDate, checkOutDate)
                                        values(@userId, @year, @month, @dayInMonth, @checkInDate, @checkOutDate)";

                    DateTime dateCheckIn = new DateTime(date.Year, date.Month, date.Day);
                    DateTime dateCheckOut = new DateTime(date.Year, date.Month, date.Day);

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@userId", System.Data.SqlDbType.VarChar).Value = UserIdForTest;
                    cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = date.Year;
                    cmd.Parameters.Add("@month", System.Data.SqlDbType.Int).Value = date.Month;
                    cmd.Parameters.Add("@dayInMonth", System.Data.SqlDbType.Int).Value = date.Day;

                    if (date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday)
                    {
                        if (r.Next(0, 11) != 10) //if equal to 10 consider that its a day that worker didnt come to work
                        {
                            if (r.Next(0, 100) > 80) //probabilty that worker didnt come in time
                            {
                                int minutes = r.Next(0, 120);
                                dateCheckIn = dateCheckIn.AddHours(9).AddMinutes(minutes);

                                int hours = r.Next(7, 11);
                                dateCheckOut = dateCheckOut.AddHours(9).AddHours(hours);

                                cmd.Parameters.Add("@checkInDate", System.Data.SqlDbType.DateTime).Value = dateCheckIn;
                                cmd.Parameters.Add("@checkOutDate", System.Data.SqlDbType.DateTime).Value = dateCheckOut;
                            }
                            else
                            {
                                cmd.Parameters.Add("@checkInDate", System.Data.SqlDbType.DateTime).Value = dateCheckIn.AddHours(9);
                                cmd.Parameters.Add("@checkOutDate", System.Data.SqlDbType.DateTime).Value = dateCheckOut.AddHours(18);
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add("@checkInDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                            cmd.Parameters.Add("@checkOutDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add("@checkInDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                        cmd.Parameters.Add("@checkOutDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                date = date.AddDays(1);
            }
        }
    }
}