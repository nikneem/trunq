using Google.Protobuf.WellKnownTypes;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

var username = builder.AddParameter("username");
var password = builder.AddParameter("password", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8080, username, password)
    .WithDataVolume()
    .WithRealmImport("./Realms");


builder.AddProject<Projects.HexMaster_Trunq_Api>("hexmaster-trunq-api");

var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "../../../../Frontend");
if (Directory.Exists(frontEndSourceFolder))
{
    var frontend = builder.AddNpmApp("Frontend", frontEndSourceFolder)
            .WithHttpEndpoint(isProxied: false, port: 4200)
            .WithHttpHealthCheck();
}



builder.Build().Run();
