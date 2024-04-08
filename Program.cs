using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Effective_TestTask
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var parameters = ParseArgs(args);
                FilterLogs(parameters);
                Console.WriteLine("Filtering completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        public static Parameters ParseArgs(string[] args)
        {
            var parameters = new Parameters();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--file-log" && i + 1 < args.Length)
                    parameters.FileLog = args[i + 1];
                else if (args[i] == "--file-output" && i + 1 < args.Length)
                    parameters.FileOutput = args[i + 1];
                else if (args[i] == "--address-start" && i + 1 < args.Length)
                    parameters.AddressStart = args[i + 1];
                else if (args[i] == "--address-mask" && i + 1 < args.Length)
                    parameters.AddressMask = int.Parse(args[i + 1]);
                else if (args[i] == "--time-start" && i + 1 < args.Length)
                    parameters.TimeStart = DateTime.ParseExact(args[i + 1], "dd.MM.yyyy", null);
                else if (args[i] == "--time-end" && i + 1 < args.Length)
                    parameters.TimeEnd = DateTime.ParseExact(args[i + 1], "dd.MM.yyyy", null);
            }
            return parameters;
        }

        public static void FilterLogs(Parameters parameters)
        {
            var ipCount = new Dictionary<string, DateTime>();
            IPAddress addressStart = null, addressEnd = null;

            if (!string.IsNullOrEmpty(parameters.AddressStart))
            {
                addressStart = IPAddress.Parse(parameters.AddressStart);
                if (parameters.AddressMask.HasValue)
                {
                    var addressBytes = addressStart.GetAddressBytes();
                    var subnetMaskBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        subnetMaskBytes[i] = (byte)(parameters.AddressMask >= (i + 1) * 8 ? 255 : 255 << (8 - parameters.AddressMask % 8));
                    }
                    var subnetMask = new IPAddress(subnetMaskBytes);
                    var addressStartBytes = addressStart.GetAddressBytes();
                    var addressEndBytes = addressStartBytes.Zip(subnetMask.GetAddressBytes(), (a, b) => (byte)(a | ~b)).ToArray();
                    addressEnd = new IPAddress(addressEndBytes);
                }
            }

            using (var reader = new StreamReader(parameters.FileLog))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int parts = line.IndexOf(':');
                    string result = parts != -1 ? line.Substring(parts + 1) : line;
                    var ipAddress = line.Substring(0, parts);
                    var logTime = DateTime.ParseExact(result, "yyyy-MM-dd HH:mm:ss", null);

                    if (addressStart != null && !IsInAddressRange(IPAddress.Parse(ipAddress), addressStart, addressEnd))
                        continue;

                    if (logTime >= parameters.TimeStart && logTime <= parameters.TimeEnd)
                    {
                        ipCount[ipAddress] = logTime;
                    }
                }
            }

            using (var writer = new StreamWriter(parameters.FileOutput))
            {
                foreach (var entry in ipCount)
                {
                    writer.WriteLine($"{entry.Key}: {entry.Value}");
                }
            }
        }

        public static bool IsInAddressRange(IPAddress address, IPAddress start, IPAddress end)
        {
            if (address.AddressFamily != start.AddressFamily || (end != null && address.AddressFamily != end.AddressFamily))
               return false;

            var addressBytes = address.GetAddressBytes();
            var startBytes = start.GetAddressBytes();
            if (end == null)
               return startBytes.SequenceEqual(addressBytes);
            var endBytes = end.GetAddressBytes();
            for (int i = 0; i < addressBytes.Length; i++)
            {
               if (addressBytes[i] < startBytes[i] || addressBytes[i] > endBytes[i])
                    return false;
            }
            return true;
        }
    }
}