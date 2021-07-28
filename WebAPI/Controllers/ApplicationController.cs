using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QC = Microsoft.Data.SqlClient;
using DT = System.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationController : ControllerBase
    {

        private readonly ILogger<ApplicationController> _logger;
        private QC.SqlConnection _sqlConnection;

        public ApplicationController(ILogger<ApplicationController> logger)
        {
            _logger = logger;
            _sqlConnection = getDBConnection();
        }


        [Route("/")]
        public String getWelcomeMessage()
        {
            _logger.LogDebug("ZBXXX : home path!");
            return "Welcome to WebApp 1.0";
        }


        [Route("/weatherdata/date/{eventDate}")]
        public ArrayList getWeatherInfoByDate(int eventDate)
        {
            Console.WriteLine("ZBXXX : fetching weather data by date = " + eventDate);
            return getWeatherDataFromDB(eventDate);
        }

        [Route("/weatherdata/name/{locName}")]
        public ArrayList getWeatherInfoByLoc(String locName)
        {
            Console.WriteLine("ZBXXX : fetching weather data by loc = " + locName);
            return getWeatherDataFromDB(locName);
        }


/*      [HttpPost]*/
        [Route("/weatherdata/insert/{locName}/{eventDate}/{rainfall}")]
        public WeatherData insertWeatherData(String locName, int eventDate, double rainfall)
        {
            try
            {
                Console.WriteLine("ZBXXX : inserting weather data, locName= "
                    + locName + ", eventDate="+ eventDate + ",rainfall=" + rainfall);
                return insertWeatherDataIntoDB(locName, eventDate, rainfall);
            } 
            catch (Exception e) 
            {
                Console.WriteLine(e.StackTrace);
                return new WeatherData();
            }
        }


        [Route("/error")]
        public String getErrorMessage()
        {
            Console.WriteLine("ZBXXX : incorrect path found!");
            return "Resource Not Found!";
        }



        private QC.SqlConnection getDBConnection()
        {
            lock(this)
            {
                Console.WriteLine("ZBXXX : Connecting To Azure SQL Database");
                var connection = new QC.SqlConnection("Server=tcp:dotnetmigration.database.windows.net,1433;" +
                    "Initial Catalog=dotnetmigration;Persist Security Info=False;User ID=zubair;Password=microsoft@123;" +
                    "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10;");

                connection.Open();
                Console.WriteLine("ZBXXX : DB Connection Initialized Successfully!");
                return connection;
            }
        }



        private ArrayList getWeatherDataFromDB(int eventDate)
        {
            ArrayList finalData = new ArrayList();
            using (var command = new QC.SqlCommand())
            {
                command.Connection = _sqlConnection;
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"SELECT * FROM weather WHERE event_date=" + eventDate;

                Console.WriteLine("ZBXXX : Executing By-Date Query...");
                QC.SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    finalData.Add(new WeatherData
                    {
                        LocationName = reader.GetString(0),
                        EventDate = reader.GetInt32(1),
                        Rainfall = reader.GetDouble(2)
                    });
                }

                Console.WriteLine(finalData);
            }

            return finalData;
        }


        private ArrayList getWeatherDataFromDB(String locationName)
        {
            ArrayList finalData = new ArrayList();

            using (var command = new QC.SqlCommand())
            {
                command.Connection = _sqlConnection;
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"SELECT * FROM weather WHERE location_name='" + locationName + "'";

                Console.WriteLine("ZBXXX - Executing ByName Query...");
                QC.SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    finalData.Add(new WeatherData
                    {
                        LocationName = reader.GetString(0),
                        EventDate = reader.GetInt32(1),
                        Rainfall = reader.GetDouble(2)
                    });
                }

                Console.WriteLine(finalData);
            }

            return finalData;
        }



        private WeatherData insertWeatherDataIntoDB(String locationName, int eventDate, double rainfall)
        {
            using (var command = new QC.SqlCommand())
            {
                command.Connection = _sqlConnection;
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"INSERT into weather 
                    values('" + locationName + "', "+eventDate +","+rainfall + ")";

                Console.WriteLine("ZBXXX - Running Insert Query...");
                command.ExecuteScalar();
                Console.WriteLine("ZBXXX - Successfully Inserted Data...");
            }

            return new WeatherData
            {
                LocationName = locationName,
                EventDate = eventDate,
                Rainfall = rainfall
            };
        }

    }
}
