using MagicAgent.AppSettings;

namespace MagicAgent;

public class Program
{
  public static void Main(string[] args)
  {
    IHost host = Host.CreateDefaultBuilder(args)
      .ConfigureServices((builder, services) =>
      {
        services.Configure<NetworkSettings>(builder.Configuration.GetSection("NetworkSettings"));

        services.AddHostedService<Worker>();
      })
      .Build();

    host.Run();
  }
}