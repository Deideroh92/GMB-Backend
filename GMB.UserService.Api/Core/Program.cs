﻿using GMB.Place.Api.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GMB.Place.Api.Core
{
    /// <summary>
    /// Main class.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Set up the configuration.
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Place.Agent")
            .AddJsonFile("GMB.Place.Api.settings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        /// <summary>
        /// Create web host.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder
        CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseConfiguration(Configuration).ConfigureLogging(log => log.AddSerilog(Log.Logger)).UseStartup<Startup>().UseKestrel());

        /// <summary>
        /// Main method of the program.
        /// </summary>
        /// <param name="args"></param>
        public static int
        Main(string[] args)
        {
            // Create the Serilog logger, and configure the sinks
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();


            // Wrap creating and running the host in a try-catch block
            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            } catch (Exception e)
            {
                Log.Fatal(e, $"Host terminated unexpectedly. Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return -1;
            } finally
            {
                Log.CloseAndFlush();
            }
        }
    };
}