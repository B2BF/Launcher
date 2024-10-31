using B2BF.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "B2BF Background Service";
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
