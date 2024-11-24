using FirmwareGen;
using FirmwareGen.GPT;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace DeviceProfile2DeviceLayout
{
    internal class Program
    {
        static void Main(string[] args)
        {
            XmlSerializer deviceProfileSerializer = new(typeof(DeviceProfile));
            using XmlReader deviceProfileReader = XmlReader.Create(args[0]);
            DeviceProfile deviceProfile = (DeviceProfile)deviceProfileSerializer.Deserialize(deviceProfileReader);

            if (deviceProfile.PartitionLayout.Length == 0)
            {
                Console.WriteLine("No partitions found in device profile");
                return;
            }

            if (deviceProfile.PartitionLayout[0].FirstLBA != 6)
            {
                Console.WriteLine("First partition does not start at LBA 6");
                return;
            }

            ulong LastLBA = 5;

            foreach (GPTPartition partition in deviceProfile.PartitionLayout)
            {
                if (partition.FirstLBA != LastLBA + 1)
                {
                    Console.WriteLine($"Partition {partition.Name} does not start at LBA {LastLBA + 1}");
                    return;
                }

                LastLBA = partition.LastLBA;

                string outputSegment = @$"                <Partition>
                    <Name>{partition.Name}</Name>
                    <TotalSectors>{partition.LastLBA - partition.FirstLBA + 1}</TotalSectors>
                    <Type>{{{partition.TypeGUID.ToString().ToUpper()}}}</Type>
                    <RequiredToFlash>true</RequiredToFlash>
                    <Backup>NotRequired</Backup>
                    <ByteAlignment>1</ByteAlignment>
                </Partition>";
                Console.WriteLine(outputSegment);
            }
        }
    }
}
