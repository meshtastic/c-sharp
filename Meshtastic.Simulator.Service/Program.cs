using Meshtastic.Simulator.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<SimulatorWorker>();

var host = builder.Build();
host.Run();
