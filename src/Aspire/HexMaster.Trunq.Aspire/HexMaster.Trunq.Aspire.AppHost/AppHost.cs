using Google.Protobuf.WellKnownTypes;

var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage - uses Azurite emulator for local development
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(em =>
    {
        em.WithLifetime(ContainerLifetime.Persistent);
    }); // Explicitly run as emulator for local development

var tables = storage.AddTables("tables");

var username = builder.AddParameter("username");
var password = builder.AddParameter("password", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8080, username, password)
    .WithDataVolume()
    .WithRealmImport("./Realms");

// Configure the API with Azure Table Storage reference
builder.AddProject<Projects.HexMaster_Trunq_Api>("hexmaster-trunq-api")
    .WaitFor(storage)
    .WithReference(tables);

var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "../../../../Frontend");
if (Directory.Exists(frontEndSourceFolder))
{
    var frontend = builder.AddNpmApp("Frontend", frontEndSourceFolder)
            .WithHttpEndpoint(isProxied: false, port: 4200)
            .WithHttpHealthCheck();
}

builder.Build().Run();
