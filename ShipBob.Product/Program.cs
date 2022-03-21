using System.Text.Json.Serialization;
using ES.EventStoreDb.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShipBob.Product.ConfigSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

JsonConvert.DefaultSettings = () =>
{
    var settings = new JsonSerializerSettings();
    settings.Converters.Add(new StringEnumConverter());
    return settings;
};

builder.Services.AddControllers()
    .AddNewtonsoftJson(opts => opts
        .SerializerSettings.Converters.Add(new StringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

var mongoDbOptions = new MongoDbOptions();
builder.Configuration.GetSection(MongoDbOptions.MongoDbSection).Bind(mongoDbOptions);
var settings = MongoClientSettings.FromConnectionString(mongoDbOptions.ConnectionString);
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
var mongoDbClient = new MongoClient(settings);
builder.Services.AddSingleton(mongoDbClient);

builder.Services.StartUp(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();