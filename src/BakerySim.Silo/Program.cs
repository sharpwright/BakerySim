﻿using Azure.Data.Tables;
using Azure.Storage.Queues;
using BakerySim.Common.Orleans;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;


await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        
    })
    .UseOrleans(static siloBuilder =>
    {
        // CONFIGURE CLUSTERING: use Azure Storage for clustering.
        siloBuilder.UseAzureStorageClustering(options =>
        {
            options.TableServiceClient = new TableServiceClient(OrleansConstants.AZURE_STORAGE_CONNECTION_STRING);
        });

        // CONFIGURE CLUSTER
        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = OrleansConstants.CLUSTER_ID;
            options.ServiceId = OrleansConstants.SERVICE_ID;
        });

        // CONFIGURE GRAIN STORAGE: add grain state persistence using Azure Table Storage.
        // UNCOMMENT: when you want a regular Grain to use Table Storage for state persistence.
        // siloBuilder.AddAzureTableGrainStorage(OrleansConstants.AZURE_TABLE_GRAIN_STORAGE, options =>
        // {
        //     options.TableServiceClient = new TableServiceClient(OrleansConstants.STORAGE_CONNECTION_STRING);
        //     options.UseStringFormat = false;
        // });

        // CONFIGURE STREAMING API: add streaming using Azure Queue Storage.
        siloBuilder.AddAzureQueueStreams(OrleansConstants.AZURE_QUEUE_STREAM_PROVIDER, optionsBuilder =>
        {
            optionsBuilder.Configure(options =>
            {
                options.QueueServiceClient = new QueueServiceClient(OrleansConstants.AZURE_STORAGE_CONNECTION_STRING);
            });
        });

        // CONFIGURE STREAMING API: add PubSubStore using Azure Table Storage.
        // Must be named "PubSubStore" so that Orleans can find and use it by convention or configuration.
        siloBuilder.AddAzureTableGrainStorage(OrleansConstants.STREAM_PUBSUB_STORE, options =>
        {
            options.TableServiceClient = new TableServiceClient(OrleansConstants.AZURE_STORAGE_CONNECTION_STRING);
            options.TableName = OrleansConstants.STREAM_PUBSUB_STORE;
            options.UseStringFormat = false;
        });

        // CONFIGURE EVENT SOURCING: add consistency provider to store all events in a log.
        siloBuilder.AddLogStorageBasedLogConsistencyProvider("EventLog");

        // CONFIGURE EVENT SOURCING: add GameEvent log storage using Azure Table Storage. 
        siloBuilder.AddAzureTableGrainStorage("GameEventLog", options =>
        {            
            options.TableServiceClient = new TableServiceClient(OrleansConstants.AZURE_STORAGE_CONNECTION_STRING);
            options.TableName = "GameEventLog";
            options.UseStringFormat = true;
        });

        // CONFIGURE LOGGING
        siloBuilder.ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });          

        // Configure how often Grains need to be cleaned up from the cluster.
        // siloBuilder.Configure<GrainCollectionOptions>(options =>
        // {
        //     options.CollectionQuantum = TimeSpan.FromSeconds(30);
        //     options.CollectionAge = TimeSpan.FromMinutes(5); 
        // });
    })
    .RunConsoleAsync();