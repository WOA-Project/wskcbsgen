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
        static string BuildVersion = "10.0.20279.1002";
        static string OEMOutputDir = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\MMO\Cityman\ARM64\fre";
        static string binDirectory = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\bin";
        static string nonBinDirectory = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\nonbin";
        static string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
        static string OEMCBSPublicKey2 = "628844477771337a";

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") +
                $@"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386;C:\Program Files (x86)\Windows Kits\10\bin\{BuildVersion}\x64\;");
            Environment.CurrentDirectory = @"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386";

            var binPkgs = BuildBinaryPackages("Cityman", CpuArch.ARM64);
            var nonBinPkgs = BuildNonBinaryPackages("Cityman", CpuArch.ARM64);

            string Output = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\MMO\Cityman\ARM64\fre";
            string DeviceFM = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\CitymanDeviceFM.xml";
            string OEMDevicePlatform = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\OEMDevicePlatform.xml";
            string DeviceLayout = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\DeviceLayout.xml";
            string DeviceLayoutLegacy = @"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\BuildCabs\DeviceLayoutNonPool.xml";

            BuildComponents("Cityman", "10.0.20279.1002", Output, DeviceFM, OEMDevicePlatform, DeviceLayout, DeviceLayoutLegacy, CpuArch.ARM64, "ModernPC", binPkgs.Union(nonBinPkgs));
        }

        public static void BuildOther()
        {
            var cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "mainos",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = CpuArch.AMD64,
                Version = new Version(BuildVersion),

                Component = $"OneCore.StateSeparation.XRO.Bindflt.Config",
                PackageName = $"Microsoft-OneCore-StateSeparation-XRO-Bindflt-Config-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Manifest,
                @"C:\Users\Gus\Documents\amd64_microsoft-onecore-s..-xro-bindflt-config_31bf3856ad364e35_10.0.20279.1002_none_233ecea4978a65b8.manifest",
                "\\windows\\WinSxS\\arm64_microsoft-onecore-s..-xro-bindflt-config_31bf3856ad364e35_10.0.20279.1002_none_233ecea4978a65b8.manifest",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DevicePlatformCabFileName = $"Microsoft-OneCore-StateSeparation-XRO-Bindflt-Config-Package~31bf3856ad364e35~AMD64~~.cab";
            cbsCabinet.SaveCab(Path.Combine(@"C:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad\Microsoft-OneCore-StateSeparation-XRO-Bindflt-Config-Package~31bf3856ad364e35~AMD64~~.cab", DevicePlatformCabFileName));
        }

        public static void BuildComponents(
            string DeviceName,
            string BuildVersion,
            string OutputPath,
            string DeviceFMPath,
            string OEMDevicePlatformPath,
            string DeviceLayoutPath,
            string DeviceLayoutLegacyPath,
            CpuArch OSArchitecture,
            string OSProductName,
            IEnumerable<IPackageInfo> SupplementalPackages)
        {
            string FeatureManifestId = $"{OSProductName.ToUpper()}{string.Join("", DeviceName.ToUpper().Take(3))}DEV";

            ////////////////////////

            var cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "mainos",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"OneCore.{DeviceName}.DevicePlatform",
                PackageName = $"Microsoft-OneCore-{DeviceName}-DevicePlatform-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                OEMDevicePlatformPath,
                @"\Windows\ImageUpdate\OEMDevicePlatform.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-DevicePlatform-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            cbsCabinet.SaveCab(Path.Combine(OutputPath, DevicePlatformCabFileName));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.OEMDEVICEPLATFORM_TALKMAN.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_TALKMAN.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            List<IPackageInfo> lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DevicePlatformCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId,
                                         $"OEMDEVICEPLATFORM_TALKMAN",
                                         "OEM",
                                         lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_TALKMAN.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

            ////////////////////////

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"OneCore.Cityman.SpaceDeviceLayout",
                PackageName = $"Microsoft-OneCore-Cityman-SpaceDeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayoutPath,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DeviceLayoutCabFileName = $"Microsoft-OneCore-Cityman-SpaceDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            cbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutCabFileName));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.DEVICELAYOUT_CITYMAN_SPACES.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_CITYMAN_SPACES.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_CITYMAN_SPACES", "OEM", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.DEVICELAYOUT_CITYMAN_SPACES.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

            /////////////////

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"OneCore.Cityman.LegacyDeviceLayout",
                PackageName = $"Microsoft-OneCore-Cityman-LegacyDeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayoutLegacyPath,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            DeviceLayoutCabFileName = $"Microsoft-OneCore-Cityman-LegacyDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            cbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutCabFileName));

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MAINOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.DEVICELAYOUT_CITYMAN_LEGACY.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_CITYMAN_LEGACY.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_CITYMAN_LEGACY", "OEM", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.DEVICELAYOUT_CITYMAN_LEGACY.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

            /////////////////

            cbsCabinet = new CbsPackage
            {
                BuildType = BuildType.Release,
                BinaryPartition = false,
                Owner = "Microsoft",
                Partition = "MainOS",
                OwnerType = PhoneOwnerType.OEM,
                PhoneReleaseType = PhoneReleaseType.Production,
                ReleaseType = "Feature Pack",
                HostArch = OSArchitecture,
                Version = new Version(BuildVersion),

                Component = $"{OSProductName}.{DeviceName}DeviceFM",
                PackageName = $"Microsoft.{OSProductName}.{DeviceName}DeviceFM",
                SubComponent = "",
                PublicKey = OEMCBSPublicKey2
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceFMPath,
                $@"\Windows\ImageUpdate\FeatureManifest\OEM\{DeviceName}DeviceFM.xml", "");

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, "BASE", "OEM", SupplementalPackages.ToList());
            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.{DeviceName}DeviceFM~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));
        }

        public static CbsPackageInfo[] BuildNonBinaryPackages(string DeviceName, CpuArch OSArchitecture)
        {
            List<CbsPackageInfo> packages = new List<CbsPackageInfo>();

            foreach (var partpath in Directory.EnumerateDirectories(nonBinDirectory))
            {
                string partitionname = partpath.Split('\\').Last();

                var cbsCabinet = new CbsPackage
                {
                    BuildType = BuildType.Release,
                    BinaryPartition = false,
                    Owner = "Microsoft",
                    Partition = partitionname,
                    OwnerType = PhoneOwnerType.OEM,
                    PhoneReleaseType = PhoneReleaseType.Production,
                    ReleaseType = "Feature Pack",
                    HostArch = OSArchitecture,
                    Version = new Version(BuildVersion),

                    Component = $"OneCore.{DeviceName}.{partitionname}",
                    PackageName = $"Microsoft-OneCore-{DeviceName}-{partitionname}-Package",
                    SubComponent = "",
                    PublicKey = MicrosoftCBSPublicKey1
                };

                foreach (var file in Directory.EnumerateFiles(partpath, "*.*", SearchOption.AllDirectories))
                {
                    Console.WriteLine(file);
                    cbsCabinet.AddFile(FileType.Regular, file, file.Replace(partpath, ""), cbsCabinet.PackageName);
                }

                cbsCabinet.Validate();

                string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-{partitionname}-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
                cbsCabinet.SaveCab(Path.Combine(OEMOutputDir, DevicePlatformCabFileName));
                packages.Add(new CbsPackageInfo(Path.Combine(OEMOutputDir, DevicePlatformCabFileName)));
            }

            return packages.ToArray();
        }

        public static CbsPackageInfo[] BuildBinaryPackages(string DeviceName, CpuArch OSArchitecture)
        {
            List<CbsPackageInfo> packages = new List<CbsPackageInfo>();

            foreach (var partpath in Directory.EnumerateFiles(binDirectory, "*.bin"))
            {
                string partitionname = Path.GetFileNameWithoutExtension(partpath);

                var cbsCabinet = new CbsPackage
                {
                    BuildType = BuildType.Release,
                    BinaryPartition = true,
                    Owner = "Microsoft",
                    Partition = partitionname,
                    OwnerType = PhoneOwnerType.OEM,
                    PhoneReleaseType = PhoneReleaseType.Production,
                    ReleaseType = "Feature Pack",
                    HostArch = OSArchitecture,
                    Version = new Version(BuildVersion),

                    Component = $"OneCore.{DeviceName}.{partitionname}",
                    PackageName = $"Microsoft-OneCore-{DeviceName}-{partitionname}-Package",
                    SubComponent = "",
                    PublicKey = MicrosoftCBSPublicKey1
                };

                cbsCabinet.AddFile(FileType.BinaryPartition, partpath, $"\\{partitionname}.bin", "");

                cbsCabinet.Validate();

                string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-{partitionname}-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
                cbsCabinet.SaveCab(Path.Combine(OEMOutputDir, DevicePlatformCabFileName));
                packages.Add(new CbsPackageInfo(Path.Combine(OEMOutputDir, DevicePlatformCabFileName)));
            }

            return packages.ToArray();
        }
    }
}
