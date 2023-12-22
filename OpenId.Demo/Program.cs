
using Microsoft.EntityFrameworkCore;
using OpenId.Demo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DbContext>(
    options =>
    {
        //in memory db provider for EF Core
        options.UseInMemoryDatabase(nameof(DbContext));
        //add OpenidDict entities
        options.UseOpenIddict();
    }
);
builder.Services.AddHostedService<ClientSeeder>();
builder.Services.AddOpenIddict()
    .AddCore(_ =>
    {
        _.UseEntityFrameworkCore().UseDbContext<DbContext>();
    })
    .AddServer(_ =>
{
    //enable client_credentials grant_tupe support on server level
    _.AllowClientCredentialsFlow();
    //specify token endpoint uri
    _.SetTokenEndpointUris("token");
    //secret registration
    _.AddDevelopmentEncryptionCertificate()
        .AddDevelopmentSigningCertificate();
    _.DisableAccessTokenEncryption();
    //the asp request handlers configuration itself
    _.UseAspNetCore().EnableTokenEndpointPassthrough();
    _.AllowRefreshTokenFlow();
    _.SetIntrospectionEndpointUris("token/introspect");
});
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