// File: Program.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Marketplace.Services.ImageStorage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
