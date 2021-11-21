﻿using DefectMessageNamespace;
using Grapevine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetRestExampleGravepine
{
    [RestResource]
    class NetRestExample
    {
        //[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        //static extern bool FreeConsole();

        private static List<Defect> defects = new List<Defect>();

        static void Main(string[] args)
        {
            PrepareData();
            Action<IServiceCollection> configServices = (services) =>
            {
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
            };

            Action<IRestServer> configServer = (server) =>
            {
                server.Prefixes.Add("http://localhost:5000/");
            };

            using (var restServer = new RestServerBuilder(new ServiceCollection(), configServices, configServer).Build())
            { 
                restServer.Start();
                Console.WriteLine("Press enter to stop the server");
                Console.ReadLine();
            }
        }

        private static void PrepareData()
        {
            for (var i = 0; i < 150; i++)
            {
                var defectCoordinates = new DefectCoordinates();
                if (i > 1)
                {
                    defectCoordinates.X = i + 10;
                    defectCoordinates.Y = i + 20;
                    defectCoordinates.Z = i + 30;
                    defectCoordinates.Xr = i + 110;
                    defectCoordinates.Yr = i + 120;
                    defectCoordinates.Zr = i + 130;
                }
                else
                {
                    if (i == 0)
                    {
                        defectCoordinates.X = 1490.0;
                        defectCoordinates.Y = 1018.0;
                        defectCoordinates.Z = -147.0;
                        defectCoordinates.Xr = 180;
                        defectCoordinates.Yr = 0;
                        defectCoordinates.Zr = 0;
                    }
                    else
                    {
                        defectCoordinates.X = 1490.0;
                        defectCoordinates.Y = 1018.0;
                        defectCoordinates.Z = 0;
                        defectCoordinates.Xr = 180;
                        defectCoordinates.Yr = 0;
                        defectCoordinates.Zr = 0;
                    }
                }

                var defect = new Defect();
                defect.DefectId = i + 1;
                defect.DefectCoordinates = defectCoordinates;
                //defect.ImageBase64 = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes("Custom image" + i));
                if (i < 4)
                {
                    defect.Descriptions.Add("Описание дефекта 1");
                }
                else if (i % 2 == 0 || i % 3 == 0)
                {
                    defect.Descriptions.Add("Описание дефекта 2");
                }
                else
                {
                    defect.Descriptions.Add("Описание дефекта 3");
                }

                defects.Add(defect);
            }
        }

        [RestRoute("Get", "/defects")]
        public async Task Test(IHttpContext context)
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");

            var start = 0;
            var end = 1024;
            var model = "";
            if (context.Request.QueryString["start"] != null)
            {
                start = int.Parse(context.Request.QueryString["start"]);
            }
            if (context.Request.QueryString["end"] != null)
            {
                end = int.Parse(context.Request.QueryString["end"]);
            }
            if (context.Request.QueryString["model"] != null)
            {
                model = context.Request.QueryString["model"];
            }

            var defectMessage = new DefectMessage
            {
                ModelId = model,
                TotalDefects = defects.Count
            };
            for (var i = start; i < end && i < defects.Count; i++)
            {
                defectMessage.Defects.Add(defects[i]);
            }

            string jsonString = JsonSerializer.Serialize(defectMessage);
            await context.Response.SendResponseAsync(jsonString);
        }
    }
}