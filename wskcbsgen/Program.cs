using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;
using Microsoft.Composition.Packaging.Interfaces;
using System.IO;

namespace wskcbsgen
{
    class Program
    {
        static void Main(string[] args)
        {
            string Output = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\MMO\";
            string DeviceFM = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\MMO\CitymanDeviceFM.xml";
            string OEMDevicePlatform = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\MMO\OEMDevicePlatform.xml";
            string DeviceLayout = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\MMO\DeviceLayout.xml";

            BuildComponents("Cityman", "Cityman", "Cityman", "10.0.20279.1002", Output, DeviceFM, OEMDevicePlatform, DeviceLayout, CpuArch.ARM64, "ModernPC");
        }

        public static void BuildComponents(
            string DeviceName,
            string PlatformName,
            string LayoutName,
            string BuildVersion, 
            string OutputPath, 
            string DeviceFMPath, 
            string OEMDevicePlatformPath, 
            string DeviceLayoutPath, 
            CpuArch OSArchitecture, 
            string OSProductName)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") +
                $@"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386;C:\Program Files (x86)\Windows Kits\10\bin\{BuildVersion}\x64\;");

            Environment.CurrentDirectory = @"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386";

            string FeatureManifestId = $"{OSProductName.ToUpper()}{string.Join("", DeviceName.ToUpper().Take(3))}DEV";

            string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
            string MicrosoftCBSPublicKey2 = "628844477771337a";

            var cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "mainos",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"OneCore.{PlatformName}.DevicePlatform",
                PackageName = $"Microsoft-OneCore-{PlatformName}-DevicePlatform-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                OEMDevicePlatformPath,
                @"\Windows\ImageUpdate\OEMDevicePlatform.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DevicePlatformCabFileName = $"Microsoft-OneCore-{PlatformName}-DevicePlatform-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            cbsCabinet.SaveCab(Path.Combine(OutputPath, DevicePlatformCabFileName));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"OneCore.{LayoutName}.DeviceLayout",
                PackageName = $"Microsoft-OneCore-{LayoutName}-DeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayoutPath,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DeviceLayoutCabFileName = $"Microsoft-OneCore-{LayoutName}-DeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            cbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutCabFileName));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.DEVICELAYOUT_{LayoutName.ToUpper()}.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{LayoutName.ToUpper()}.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = MicrosoftCBSPublicKey2
            };

            List<IPackageInfo> lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{LayoutName.ToUpper()}", "Microsoft", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.DEVICELAYOUT_{LayoutName.ToUpper()}.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.OEMDEVICEPLATFORM_{PlatformName.ToUpper()}.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{PlatformName.ToUpper()}.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = MicrosoftCBSPublicKey2
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DevicePlatformCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId,
                                         $"OEMDEVICEPLATFORM_{PlatformName.ToUpper()}",
                                         "Microsoft",
                                         lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{PlatformName.ToUpper()}.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.Microsoft,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.{DeviceName}DeviceFM",
                PackageName = $"Microsoft.{OSProductName}.{DeviceName}DeviceFM",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey2
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceFMPath,
                $@"\Windows\ImageUpdate\FeatureManifest\Microsoft\{DeviceName}DeviceFM.xml", "");

            List<IPackageInfo> lst6 = new List<IPackageInfo>();
            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, "BASE", "Microsoft", lst6);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.{DeviceName}DeviceFM~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));
        }
    }
}
