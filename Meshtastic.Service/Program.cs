using Meshtastic.Persistance;
using Meshtastic.Service.Hubs;
using Meshtastic.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MeshtasticDbContext>(a => a.UseSqlite("Filename=Meshtastic.db"));
builder.Services.AddSignalR();
builder.Services.AddHostedService<DeviceConnectionService>();
builder.Services.AddSwaggerDocument();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<BrokerHub>("/broker");
app.UseOpenApi();
app.UseSwaggerUi3();

UpdateDatabase(app);

app.Run();

static void UpdateDatabase(IApplicationBuilder app)
{
    using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
        .CreateScope();

    using var context = serviceScope.ServiceProvider.GetService<MeshtasticDbContext>();

    context!.Database.Migrate();
}