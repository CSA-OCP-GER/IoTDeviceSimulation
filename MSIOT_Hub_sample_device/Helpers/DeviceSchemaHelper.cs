using System;
using System.Collections.Generic;
using MSIOT_Hub_sample_device.Constants;
using Newtonsoft.Json.Linq;

namespace MSIOT_Hub_sample_device.Helpers
{
    /// <summary>
    /// Helper class to encapsulate interactions with the device schema.
    /// 
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net where necessary to populate the objects 
    /// behind the schema.
    /// </summary>
    public static class DeviceSchemaHelper
    {
        /// <summary>
        /// Build a valid device representation in the dynamic format used throughout the app.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static dynamic BuildDeviceStructure(string deviceId)
        {
            
            JObject device = new JObject();
            
            // must be DeviceInfo or will not work, checked for in event webjob
            device.Add(DeviceModelConstants.OBJECT_TYPE, "DeviceInfo");

            // must be 1.0 or will not work, checked for in event webjob
            device.Add(DeviceModelConstants.VERSION, "1.0");
            device.Add(DeviceModelConstants.IS_SIMULATED_DEVICE, "false");
            
            // deviceProps is just a JObject
            JObject deviceProps = new JObject();

            //the device properties are read out of the device properties object
            deviceProps.Add(DevicePropertiesConstants.DEVICE_ID, deviceId);
            deviceProps.Add(DevicePropertiesConstants.HUB_ENABLED_STATE, true);
            deviceProps.Add(DevicePropertiesConstants.CREATED_TIME, DateTime.UtcNow);
            deviceProps.Add(DevicePropertiesConstants.DEVICE_STATE, "normal");
            deviceProps.Add(DevicePropertiesConstants.UPDATED_TIME, DateTime.UtcNow);
            deviceProps.Add("Device color", "Bright red");

            //add the device properties and the empty command history
            device.Add(DeviceModelConstants.DEVICE_PROPERTIES, deviceProps);
            device.Add(DeviceModelConstants.COMMAND_HISTORY, new JArray());
            
            // create on and off commands commands
            var switchOnCommand = new JObject { { CommandModelConstants.NAME, CommandNames.TURN_ON_COMMAND_NAME } };
            var switchOffCommand = new JObject { { CommandModelConstants.NAME, CommandNames.TURN_OFF_COMMAND_NAME } };

            var commands = new JArray();
            commands.Add(switchOnCommand);
            commands.Add(switchOffCommand);

            device.Add(DeviceModelConstants.COMMANDS, commands);
            return device;
        }
    }
}
