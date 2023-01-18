namespace MagicAgent.AppSettings;

internal sealed class NetworkSettings
{
  public int ListenPort { get; set; } = 9;
  public string ListenAddress { get; set; } = "0.0.0.0";

  public int BroadcastPort { get; set; } = 9;
  public string BroadcastAddress { get; set; } = "255.255.255.255";
}