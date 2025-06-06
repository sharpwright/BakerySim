using Azure.Data.Tables;
using PastryTycoon.Core.Abstractions.Constants;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Host.UseOrleansClient((context, client) =>
{
    // Configure Orleans client to be able to find Orleans clusters.
    client.UseAzureStorageClustering(configureOptions: options =>
    {
        options.TableServiceClient = new TableServiceClient(OrleansConstants.AZURE_STORAGE_CONNECTION_STRING);
    });

    // Configure Cluster Options, needs to match the silo options.
    client.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = OrleansConstants.CLUSTER_ID;
        options.ServiceId = OrleansConstants.SERVICE_ID;  
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.MapControllers();
//app.UseHttpsRedirection();
app.Run();