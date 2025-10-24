using MagicAgent.AppSettings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace MagicAgent;

internal class Worker : BackgroundService
{
  private readonly ILogger<Worker> logger;
  private readonly NetworkSettings settings;

  public Worker(ILogger<Worker> logger, IOptions<NetworkSettings> settings)
  {
    this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    this.settings = settings.Value;
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0011:Add braces", Justification = "<Pending>")]
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // Create listener.
        var listenerEndpoint = new IPEndPoint(IPAddress.Parse(settings.ListenAddress), settings.ListenPort);
        using var udpListener = new UdpClient(listenerEndpoint);

        // Create sender.
        var senderEndpoint = new IPEndPoint(IPAddress.Parse(settings.BroadcastAddress), settings.BroadcastPort);
        using var udpSender = new UdpClient() { EnableBroadcast = true };
        udpSender.Connect(senderEndpoint);

        // Log..
        logger.LogInformation("{time}: Listening at \"{listener}\".",
          DateTime.Now.ToString("u"), listenerEndpoint.ToString());

        // The loop.
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            // Listen for UDP packets.
            UdpReceiveResult received = await udpListener.ReceiveAsync(stoppingToken);

            // Get received buffer.
            byte[] bytesReceived = received.Buffer;

            // Test if is a magic packet.
            if (!IsMagicPacket(bytesReceived))
              continue;

            // Skip only if this is the exact datagram we ourselves just broadcast (same source IP+port).
            if (received.RemoteEndPoint.Equals(udpSender.Client.LocalEndPoint))
              continue;

            // Get MAC address from buffer.
            string mac = BitConverter.ToString(bytesReceived, 6, 6);

            // Log..
            logger.LogInformation("{time}: Magic packet for MAC address \"{mac}\" received from \"{remote}\" at \"{local}\".",
              DateTime.Now.ToString("u"), mac, received.RemoteEndPoint.ToString(), udpListener.Client.LocalEndPoint?.ToString());

            // Forward the packet.
            await udpSender.SendAsync(bytesReceived, bytesReceived.Length);

            // Log..
            logger.LogInformation("{time}: Magic packet for MAC address \"{mac}\" forwarded to \"{remote}\" from \"{local}\".",
              DateTime.Now.ToString("u"), mac, senderEndpoint.ToString(), udpSender.Client.LocalEndPoint?.ToString());
          }
          catch (OperationCanceledException)
          {
            throw;
          }
          catch (Exception Ex)
          {
            logger.LogError(Ex, "{time}: Network exception.", DateTime.Now.ToString("u"));
          }

          // Log..
          logger.LogInformation("{time}: Listening at \"{listener}\".",
            DateTime.Now.ToString("u"), listenerEndpoint.ToString());
        }
      }
      catch (OperationCanceledException)
      {
        logger.LogError("{time}: Worker shutting down ..", DateTime.Now.ToString("u"));
        break;
      }
      catch (Exception Ex)
      {
        logger.LogError(Ex, "{time}: Worker exception. Restarting ..", DateTime.Now.ToString("u"));
        continue;
      }
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0011:Add braces", Justification = "<Pending>")]
  private static bool IsMagicPacket(byte[] bytesReceived)
  {
    // Total size of a magic packet must be at least 102 bytes.
    if (bytesReceived.Length < 102)
      return false;

    // The first 6 bytes in the payload should be all 0xFF (or 255) ..
    for (int i = 0; i < 5; i++)
      if (bytesReceived[i] != 0xff)
        return false;

    // .. folowed by 16 repetitions of the target NIC's 6-byte MAC address.
    for (int i = 2; i < 16; i++)
      for (int j = 0; j < 5; j++)
        if (bytesReceived[6 + j] != bytesReceived[(6 * i) + j])
          return false;

    return true;
  }
}