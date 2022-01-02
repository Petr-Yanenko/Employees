using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Employees.MVCModels;


namespace Employees
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                GlobalDataContext.GetInstance().HandleException(ex);
                throw ex;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class GlobalDataContext
    {
        static private GlobalDataContext _instance = new GlobalDataContext();

        public EmployeesModel Model { get; } = new EmployeesModel();


        static public GlobalDataContext GetInstance()
        {
            return _instance;
        }

        public void HandleError(ErrorCode code)
        {
        }

        public void HandleException(Exception ex)
        {
        }
    }
}
