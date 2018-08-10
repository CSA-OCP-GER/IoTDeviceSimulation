namespace MSIOT_Hub_sample_device.Constants
{
    public static class DeviceModelConstants
    {
        public const string OBJECT_NAME = "ObjectName";
        public const string OBJECT_TYPE = "ObjectType";
        public const string VERSION = "Version";
        public const string IS_SIMULATED_DEVICE = "IsSimulatedDevice";
        public const string DEVICE_PROPERTIES = "DeviceProperties";
        public const string COMMANDS = "Commands";
        public const string COMMAND_HISTORY = "CommandHistory";
        public const string ID = "id";
        public const string SELF_LINK = "_self";
    }

    public static class DevicePropertiesConstants
    {
        public const string DEVICE_ID = "DeviceID";
        public const string DEVICE_STATE = "DeviceState";
        public const string HUB_ENABLED_STATE = "HubEnabledState";
        public const string MANUFACTURER = "Manufacturer";
        public const string MODEL_NUMBER = "ModelNumber";
        public const string SERIAL_NUMBER = "SerialNumber";
        public const string FIRMWARE_VERSION = "FirmwareVersion";
        public const string AVAILABLE_POWER_SOURCES = "AvailablePowerSources";
        public const string POWER_SOURCE_VOLTAGE = "PowerSourceVoltage";
        public const string BATTERY_LEVEL = "BatteryLevel";
        public const string MEMORY_FREE = "MemoryFree";
        public const string PLATFORM = "Platform";
        public const string PROCESSOR = "Processor";
        public const string INSTALLED_RAM = "InstalledRAM";
        public const string CREATED_TIME = "CreatedTime";
        public const string UPDATED_TIME = "UpdatedTime";
        public const string HOST_NAME = "HostName";
    }

    public static class CommandModelConstants
    {
        public const string NAME = "Name";
        public const string PARAMETERS = "Parameters";
    }

    public static class CommandNames
    {
        public const string PING_COMMAND_NAME = "Ping device";
        public const string SET_GAUGES_TO_50 = "Set gauges to 50%";
        public const string OPEN_VENT_COMMAND = "Open vents";
        public const string CLOSE_VENT_COMMAND = "Close vents";
        public const string TURN_ON_COMMAND_NAME = "Turn on device";
        public const string TURN_OFF_COMMAND_NAME = "Turn off device";
        public const string SET_DEVICE_TEMPERATURE = "Set device temperature";
    }

    public static class ParameterModelConstants
    {
        public const string NAME = "Name";
        public const string TYPE = "Type";
        public const string DOUBLE_TYPE = "double";
    }
}
