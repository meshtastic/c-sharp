using Meshtastic.Simulator.Service;
using Meshtastic.Simulator.Service.Persistance;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();
builder.Services.AddSingleton<IFilePersistance, FilePersistance>();
builder.Services.AddSingleton<ISimulatorStore, SimulatorStore>();
builder.Services.AddHostedService<SimulatorWorker>();

var host = builder.Build();
host.Run();
