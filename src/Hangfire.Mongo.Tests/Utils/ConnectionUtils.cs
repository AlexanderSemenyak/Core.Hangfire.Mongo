﻿using System;
using System.Runtime.InteropServices;
using Hangfire.Mongo.Database;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Mongo2Go;
using MongoDB.Driver;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Hangfire.Mongo.Tests.Utils.ConnectionUtils", "Hangfire.Mongo.Tests")]

namespace Hangfire.Mongo.Tests.Utils
{
#pragma warning disable 1591
    public class ConnectionUtils : XunitTestFramework
    {
        private static Mongo2Go.MongoDbRunner _runner;
        private const string DefaultDatabaseName = @"Hangfire-Mongo-Tests";

        public static string GetDatabaseName()
        {
            var framework = "Net46";
            if (RuntimeInformation.FrameworkDescription.Contains(".NET Core"))
            {
                framework = "NetCore";
            }
            else if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
            {
                framework = "Mono";
            }
            return DefaultDatabaseName + "-" + framework;
        }


        public static MongoStorage CreateStorage(string databaseName = null)
        {
            var storageOptions = new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new DropMongoMigrationStrategy(),
                    BackupStrategy = new NoneMongoBackupStrategy()
                }
            };
            return CreateStorage(storageOptions, databaseName);
        }

        
        public static MongoStorage CreateStorage(MongoStorageOptions storageOptions, string databaseName=null)
        {
            if (_runner == null)
            {
                _runner = MongoDbRunner.Start(singleNodeReplSet: true);
            }
            var mongoClientSettings = MongoClientSettings.FromConnectionString(_runner.ConnectionString);
            return new MongoStorage(mongoClientSettings, databaseName ?? GetDatabaseName(), storageOptions);
        }

        public static HangfireDbContext CreateDbContext(string dbName = null)
        {
            if (_runner == null)
            {
                _runner = MongoDbRunner.Start(singleNodeReplSet: true);
            }
            return new HangfireDbContext(_runner.ConnectionString, dbName ?? GetDatabaseName());
        }

        public static void DropDatabase()
        {
            if (_runner == null)
            {
                return;
            }

            var client = new MongoClient(_runner.ConnectionString);
            client.DropDatabase(GetDatabaseName());
        }
        

        public ConnectionUtils(IMessageSink messageSink) : base(messageSink)
        {
            _runner = MongoDbRunner.Start(singleNodeReplSet: true);
            DisposalTracker.Add(_runner);
        }
    }
#pragma warning restore 1591
}