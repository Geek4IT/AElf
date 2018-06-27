﻿using System;
using System.IO;
using System.Threading.Tasks;
using AElf.Database;
using AElf.Database.Config;
using AElf.Kernel;
using AElf.Kernel.KernelAccount;
using AElf.Kernel.Managers;
using AElf.Kernel.Modules.AutofacModule;
using AElf.Kernel.Node;
using AElf.Kernel.Services;
using AElf.Runtime.CSharp;
using Autofac;
using Autofac.Core;

namespace AElf.Benchmark
{
    public class Program
    {
        
        public static async Task Main()
        {
            Hash chainId = Hash.Generate();
            var builder = new ContainerBuilder();
            builder.RegisterModule(new MainModule());
            builder.RegisterModule(new MetadataModule());

            var dataConfig = new DatabaseConfig
            {
                Type = DatabaseType.KeyValue,
                Host = "192.168.9.9",
                Port = 6379
            };
            builder.RegisterModule(new WorldStateDictatorModule());
            builder.RegisterModule(new DatabaseModule(dataConfig));
            builder.RegisterModule(new LoggerModule());
            builder.RegisterType<Benchmarks>().WithParameter("chainId", chainId).WithParameter("maxTxNum", 1000);
            #if DEBUG
            var runner = new SmartContractRunner("../AElf.SDK.CSharp/bin/Debug/netstandard2.0/");
            #else
            var runner = new SmartContractRunner("../AElf.SDK.CSharp/bin/Release/netstandard2.0/");
            #endif
            SmartContractRunnerFactory smartContractRunnerFactory = new SmartContractRunnerFactory();
            smartContractRunnerFactory.AddRunner(0, runner);
            smartContractRunnerFactory.AddRunner(1, runner);
            builder.RegisterInstance(smartContractRunnerFactory).As<ISmartContractRunnerFactory>().SingleInstance();

            var container = builder.Build();
            
            if (container == null)
            {
                Console.WriteLine("IoC setup failed");
                return;
            }

            if (!CheckDBConnect(container))
            {
                Console.WriteLine("Database connection failed");
                return;
            }
            
            using(var scope = container.BeginLifetimeScope())
            {
                var benchmarkTps = scope.Resolve<Benchmarks>();

                
                /*
                 var baseline = benchmarkTps.SingleGroupBenchmark(2000, 1).Result;
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }
                var baseline = benchmarkTps.SingleGroupBenchmark(3000, 1).Result;
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }

                for (double i = 0; i < 1; i+= 0.2)
                {
                    var resDict = benchmarkTps.SingleGroupBenchmark(3000, 0).Result;

                    Console.WriteLine("--------------------\n" + "Tx count: " + 3000 + "| Conflict rate: " + i);
                    foreach (var kv in resDict)
                    {
                        Console.WriteLine(kv.Key + ": " + kv.Value);
                    }
                }
                */
                var multiGroupRes = await benchmarkTps.MultipleGroupBenchmark(1000, 1);
                foreach (var kv in multiGroupRes)
                {
                    Console.WriteLine(kv.Key + kv.Value);
                }
            }
        }
        
        private static bool CheckDBConnect(IContainer container)
        {
            var db = container.Resolve<IKeyValueDatabase>();
            return db.IsConnected();
        }
    }
}