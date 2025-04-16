using Meshtastic.Virtual.Service;
using Meshtastic.Virtual.Service.Persistance;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();
builder.Services.AddSingleton<IFilePersistance, FilePersistance>();
builder.Services.AddSingleton<IVirtualStore, VirtualStore>();
builder.Services.AddHostedService<VirtualWorker>();

var host = builder.Build();
host.Run();
