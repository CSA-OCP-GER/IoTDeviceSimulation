using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Client;
using MSIOT_Hub_sample_device.Constants;
using MSIOT_Hub_sample_device.Helpers;
using Newtonsoft.Json;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace MSIOT_Hub_sample_device
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private DeviceClient _sendDeviceClient;
    private DeviceClient _receiveDeviceClient;
    private CancellationTokenSource _sendCancelationTokenSource;
    private CancellationTokenSource _receiveCancelationTokenSource;
    private string _deviceId;
    private string _devicePrimaryKey;
    private string _deviceHostName;
    private bool _deviceEnabled = false;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void ConnectDevice(object sender, RoutedEventArgs e)
    {
      // get device properties from the user
      _deviceId = System.Configuration.ConfigurationManager.AppSettings["deviceId"];
      _devicePrimaryKey = System.Configuration.ConfigurationManager.AppSettings["devicePrimaryKey"]; ;
      _deviceHostName = System.Configuration.ConfigurationManager.AppSettings["deviceHostName"]; ;

      try
      {
        var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(_deviceId, _devicePrimaryKey);

        var deviceConnectionString = Microsoft.Azure.Devices.Client.IotHubConnectionStringBuilder.Create(_deviceHostName, authMethod).ToString();

        _sendDeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);
        _receiveDeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        // we are using 2 device clients due to each device client being used in a different task. if we merge both 
        // the send and receive loops into the same task we can use one device client.
        // https://msdn.microsoft.com/en-us/library/microsoft.azure.devices.client.deviceclient.createfromconnectionstring.aspx#M:Microsoft.Azure.Devices.Client.DeviceClient.CreateFromConnectionString%28System.String%29

        await SendStartupEventMessage(_deviceId, _sendDeviceClient);

        DeviceIdLabel.Content = _deviceId;
        MessageBox.Show("Device connected successfully");
      }
      catch (Exception exception)
      {
        // tell the user what they have done wrong
        MessageBox.Show(exception.Message);
        return;
      }

      // run the receive loop in a non blocking thread
#pragma warning disable 4014
      ReceiveCommandsAsync();
#pragma warning restore 4014
    }

    private async Task SendStartupEventMessage(string deviceId, DeviceClient deviceClient)
    {
      var deviceSchema = DeviceSchemaHelper.BuildDeviceStructure(deviceId);
      var message = JsonConvert.SerializeObject(deviceSchema);
      await SendMessageAsync(message, deviceClient);
    }

    // SendTelemetryEventsAsync starts the sending telemetry loop and sets up cancelation tokens to stop the telemetry
    private async Task SendTelemetryEventsAsync()
    {
      _sendCancelationTokenSource = new CancellationTokenSource();
      await RunTimedLoopAsync(_sendCancelationTokenSource.Token, 5, async () =>
      {
        var monitorData = new RemoteMonitorTelemetryData
        {
          deviceid = _deviceId,
          temperature = TempSlider.Value,
          humidity = HumiditySlider.Value,
          oilpressure = OilPressureSlider.Value,
          vibration = VibrationSlider.Value,
          timestamp = DateTime.UtcNow.ToString()
        };
        var message = JsonConvert.SerializeObject(monitorData);
        await SendMessageAsync(message, _sendDeviceClient);
      });
    }

    // ReceiveCommandsAsync starts the receiving messages loop and sets up cancelation tokens to stop receiving messages
    private async Task ReceiveCommandsAsync()
    {
      if (_receiveDeviceClient == null)
      {
        MessageBox.Show("you must connect a device before receiving messages");
      }

      // create a new CancelationTokenSource to cancel the receiving loop when required
      _receiveCancelationTokenSource = new CancellationTokenSource();

      //check for device client being null
      if (_receiveDeviceClient == null)
      {
        MessageBox.Show("Error: device client is null");
        return;
      }

      // open device client to prevent device client from closing when no messages are received after some time
      await _receiveDeviceClient.OpenAsync();

      // start the receiving loop, check for message, if there is a message then process the message that is received
      await RunTimedLoopAsync(_receiveCancelationTokenSource.Token, 10, async () =>
      {
        var receivedMessage = await _receiveDeviceClient.ReceiveAsync();
        if (receivedMessage?.Properties.Count > 0)
        {
          StreamReader reader = new StreamReader(receivedMessage.BodyStream);
          string receivedText = reader.ReadToEnd();

                // check received message for a command
                ProcessCommand(receivedText);

          AddLineToTextbox(ReceivedEventsTextBox, receivedText);
        }
              // completing the message to cause the command to show as completed in the portal
              await _receiveDeviceClient.CompleteAsync(receivedMessage);
      });
    }

    private void ProcessCommand(string jsonString)
    {
      // deserialize object
      dynamic command = JsonConvert.DeserializeObject(jsonString);

      if (command.Name.Value == CommandNames.TURN_ON_COMMAND_NAME)
      {
        // turn on the device
        StartDevice();
        AddLineToTextbox(ReceivedEventsTextBox, "Received switch on command");
      }
      else if (command.Name.Value == CommandNames.TURN_OFF_COMMAND_NAME)
      {
        // turn off the device
        // show device status as off
        // print command info in commands text box
        StopDevice();
        AddLineToTextbox(ReceivedEventsTextBox, "Received switch off command");
      }
    }

    // method to run a loop without blocking the ui thread.
    private async Task RunTimedLoopAsync(CancellationToken token, int runEverySeconds, Func<Task> runFunc)
    {
      while (true)
      {
        await runFunc();

        //if canceled let tasks die peacfully in their sleep
        await Task.Delay(TimeSpan.FromSeconds(runEverySeconds), token);
      }
    }

    // send a message through the device client through the iot hub
    private async Task SendMessageAsync(string messageJson, DeviceClient deviceClient)
    {
      await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
      {
        var message = new Message(Encoding.UTF8.GetBytes(messageJson));
        await deviceClient.SendEventAsync(message);

        AddLineToTextbox(SentMessagesTextbox, messageJson);
      });
    }

    // add a line to a text box
    private static void AddLineToTextbox(TextBox textbox, string nextLine)
    {
      textbox.Text += $"{nextLine}\n";
    }

    // start telemetry starts sending telemetry events to the iot suite
    private void StartTelemetry()
    {
      if (_sendDeviceClient == null)
      {
        MessageBox.Show("you must connect a device in the settings tab before starting telemetry");
      }
      // stop the telemetry in case it is running already
      StopTelemetry();

      // run the send loop in a non blocking task
#pragma warning disable 4014
      SendTelemetryEventsAsync();
#pragma warning restore 4014
    }

    // stop telemetry cancels the task that is sending telemetry events
    private void StopTelemetry()
    {
      _sendCancelationTokenSource?.Cancel();
    }

    private void OilPressureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      OilPressureLabel.Content = Math.Round(OilPressureSlider.Value);
    }

    private void VibrationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      VibrationLabel.Content = Math.Round(VibrationSlider.Value);
    }

    private void TempSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      TemperatureLabel.Content = Math.Round(TempSlider.Value);
      SetCorrectEngineImageState();
    }

    private void HumiditySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      HumidityValueLabel.Content = Math.Round(HumiditySlider.Value);
      SetCorrectEngineImageState();
    }

    private void SetCorrectEngineImageState()
    {
      // if the device is on then show the moving device gif
      // if the gauges are over 80 then show the shaking device gif
      // if the device is off then show the still device image
      if (!_deviceEnabled)
      {
        ShakeImage.Visibility = Visibility.Hidden;
        StillImage.Visibility = Visibility.Visible;
        MovingImage.Visibility = Visibility.Hidden;
      }
      else if (HumiditySlider.Value >= 80 || TempSlider.Value >= 80)
      {
        ShakeImage.Visibility = Visibility.Visible;
        StillImage.Visibility = Visibility.Hidden;
        MovingImage.Visibility = Visibility.Hidden;
      }
      else
      {
        ShakeImage.Visibility = Visibility.Hidden;
        StillImage.Visibility = Visibility.Hidden;
        MovingImage.Visibility = Visibility.Visible;
      }
    }

    // button click handling starting the device telemetry
    private void StartDeviceButton_Click(object sender, RoutedEventArgs e)
    {
      StartDevice();
    }

    private void StartDevice()
    {
      // turn on the device
      StartTelemetry();
      _deviceEnabled = true;
      // show device status as on
      DeviceStatusLabel.Content = "On";
      ToggleSliderStates(_deviceEnabled);
      SetCorrectEngineImageState();
    }

    // button click handling stopping the device telemetry
    private void StopDeviceButton_Click(object sender, RoutedEventArgs e)
    {
      StopDevice();
    }

    private void StopDevice()
    {
      // turn off the device
      StopTelemetry();
      _deviceEnabled = false;
      // show device status as off
      DeviceStatusLabel.Content = "Off";
      ToggleSliderStates(_deviceEnabled);
      SetCorrectEngineImageState();
    }

    private void ToggleSliderStates(bool enabled)
    {
      HumiditySlider.IsEnabled = enabled;
      TempSlider.IsEnabled = enabled;
      OilPressureSlider.IsEnabled = enabled;
      VibrationSlider.IsEnabled = enabled;
    }
  }
}
