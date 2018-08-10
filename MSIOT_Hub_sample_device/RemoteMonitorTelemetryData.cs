using System.CodeDom;

namespace MSIOT_Hub_sample_device
{
  public class RemoteMonitorTelemetryData
  {
    public string deviceid { get; set; }
    public double temperature { get; set; }
    public double humidity { get; set; }
    public double oilpressure { get; set; }
    public double vibration { get; set; }
    public string timestamp { get; set; }
  }
}