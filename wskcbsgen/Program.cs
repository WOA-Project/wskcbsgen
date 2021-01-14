using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;
using Microsoft.Composition.Packaging.Interfaces;

namespace wskcbsgen
{
    class Program
    {
        static void Main(string[] args)
        {
            string DeviceName = "Cityman";
            string BuildVersion = "10.0.20279.1002";
            string Output = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\Cityman\";
            string DeviceFM = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\Cityman\CitymanDeviceFM.xml";
            string OEMDevicePlatform = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\Cityman\OEMDevicePlatform.xml";
            string DeviceLayout = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\Cityman\DeviceLayout.xml";
            var architecture = CpuArch.ARM64;
            string ProductName = "ModernPC";

            BuildComponents(DeviceName, BuildVersion, Output, DeviceFM, OEMDevicePlatform, DeviceLayout, architecture, ProductName);
        }

        public static void BuildComponents(string DeviceName, string BuildVersion, string Output, string DeviceFM, string OEMDevicePlatform, string DeviceLayout, CpuArch architecture, string ProductName)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") +
                @"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386;C:\Program Files (x86)\Windows Kits\10\bin\" + BuildVersion + @"\x64\;");

            Environment.CurrentDirectory = @"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386";

            var cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "mainos",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = architecture,
                Version = new Version(BuildVersion),

                Component = "OneCore." + DeviceName + @".DevicePlatform512",
                PackageName = "Microsoft-OneCore-" + DeviceName + @"-DevicePlatform512-Package",
                SubComponent = "",
                PublicKey = "31bf3856ad364e35"
            };

            cbsCabinet.AddFile(FileType.Regular,
                OEMDevicePlatform,
                @"\Windows\ImageUpdate\OEMDevicePlatform.xml",
                "Microsoft-OneCore-" + DeviceName + @"-DevicePlatform512-Package");

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Output + @"Microsoft-OneCore-" + DeviceName + @"-DevicePlatform512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab");

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = architecture,
                Version = new Version(BuildVersion),

                Component = "OneCore." + DeviceName + @"GPTSpaceDeviceLayout512",
                PackageName = "Microsoft-OneCore-" + DeviceName + @"GPTSpaceDeviceLayout512-Package",
                SubComponent = "",
                PublicKey = "31bf3856ad364e35"
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayout,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                "Microsoft-OneCore-" + DeviceName + @"GPTSpaceDeviceLayout512-Package");

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Output + @"Microsoft-OneCore-" + DeviceName + @"GPTSpaceDeviceLayout512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab");

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = architecture,
                Version = new Version(BuildVersion),

                Component = ProductName + ".DEVICELAYOUT_GPT_SPACES_512." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV",
                PackageName = "Microsoft." + ProductName + ".DEVICELAYOUT_GPT_SPACES_512." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV.FIP",
                SubComponent = "FIP",
                PublicKey = "628844477771337a"
            };

            List<IPackageInfo> lst = new List<IPackageInfo>();
            lst.Add(new CbsPackageInfo(Output + @"Microsoft-OneCore-" + DeviceName + @"GPTSpaceDeviceLayout512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab"));
            cbsCabinet.SetCBSFeatureInfo(ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV", "DEVICELAYOUT_GPT_SPACES_512", "Microsoft", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Output + @"Microsoft." + ProductName + ".DEVICELAYOUT_GPT_SPACES_512." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV.FIP~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab");

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = architecture,
                Version = new Version(BuildVersion),

                Component = ProductName + ".OEMDEVICEPLATFORM_" + DeviceName.ToUpper() + @"DEVICE." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV",
                PackageName = "Microsoft." + ProductName + ".OEMDEVICEPLATFORM_" + DeviceName.ToUpper() + @"DEVICE." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV.FIP",
                SubComponent = "FIP",
                PublicKey = "628844477771337a"
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Output + @"Microsoft-OneCore-" + DeviceName + @"-DevicePlatform512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab")
            };
            cbsCabinet.SetCBSFeatureInfo(ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV",
                                         "OEMDEVICEPLATFORM_" + DeviceName.ToUpper() + @"DEVICE",
                                         "Microsoft",
                                         lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Output + @"Microsoft." + ProductName + ".OEMDEVICEPLATFORM_" + DeviceName.ToUpper() + @"DEVICE." + ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV.FIP~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab");

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = architecture,
                Version = new Version(BuildVersion),

                Component = ProductName + "." + DeviceName + @"DeviceFM",
                PackageName = "Microsoft." + ProductName + "." + DeviceName + @"DeviceFM",
                SubComponent = "",
                PublicKey = "628844477771337a"
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceFM,
                @"\Windows\ImageUpdate\FeatureManifest\Microsoft\" + DeviceName + @"DeviceFM.xml", "");

            List<IPackageInfo> lst6 = new List<IPackageInfo>
            {
                //new CbsPackageInfo(Output + @"Microsoft-OneCore-" + DeviceName + @"-DevicePlatform512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab"),
                //new CbsPackageInfo(Output + @"Microsoft-OneCore-" + DeviceName + @"GPTSpaceDeviceLayout512-Package~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab")
            };
            cbsCabinet.SetCBSFeatureInfo(ProductName.ToUpper() + string.Join("", DeviceName.ToUpper().Take(3)) + "DEV", "BASE", "Microsoft", lst6);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Output + @"Microsoft." + ProductName + "." + DeviceName + @"DeviceFM~31bf3856ad364e35~" + architecture.ToString().ToUpper() + @"~~.cab");
        }
    }
}
