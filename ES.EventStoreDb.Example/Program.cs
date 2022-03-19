using ES.EventStoreDb.Example;
using ES.EventStoreDb.Example.ConfigSettings;
using ES.EventStoreDb.Example.Sql;
using ES.EventStoreDb.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// var sqlOptions = new SqlOptions();
// builder.Configuration.GetSection(SqlOptions.SqlSection).Bind(sqlOptions);
// builder.Services.AddDbContext<SqlDbContext>(options =>
// {
//     options.UseSqlServer(sqlOptions.ConnectionString);
// });

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