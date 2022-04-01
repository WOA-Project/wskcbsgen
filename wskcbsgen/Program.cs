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
        static readonly string ProjectRoot            = @"E:\10.0.20279.1002.fe_release_10x.201214-1532_arm64fre_26ce5ebdeaad";

        static readonly string PhoneName              = "Talkman";
        static readonly string BuildVersion           = "10.0.20279.1002";

        static readonly string OEMOutputDir           = $@"{ProjectRoot}\MMO\{PhoneName}\ARM64\fre";
        static readonly string binDirectory           = $@"{ProjectRoot}\BuildCabs.{PhoneName}\bin";
        static readonly string nonBinDirectory        = $@"{ProjectRoot}\BuildCabs.{PhoneName}\nonbin";

        static readonly string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
        static readonly string OEMCBSPublicKey2       = "628844477771337a";

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") +
                $@"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386;E:\Program Files (x86)\Windows Kits\10\bin\{BuildVersion}\x64\;");
            Environment.CurrentDirectory = @"C:\Program Files (x86)\Windows Kits\10\tools\bin\i386";

            var binPkgs    = BuildBinaryPackages($"{PhoneName}", CpuArch.ARM64);
            var nonBinPkgs = BuildNonBinaryPackages($"{PhoneName}", CpuArch.ARM64);

            string Output             = $@"{ProjectRoot}\MMO\{PhoneName}\ARM64\fre";
            string DeviceFM           = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}DeviceFM.xml";
            string OEMDevicePlatform  = $@"{ProjectRoot}\BuildCabs.{PhoneName}\OEMDevicePlatform.xml";
            string DeviceLayout       = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayout.xml";
            string DeviceLayoutLegacy = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayoutNonPool.xml";

            BuildComponents($"{PhoneName}", BuildVersion, Output, DeviceFM, OEMDevicePlatform, DeviceLayout, DeviceLayoutLegacy, CpuArch.ARM64, "ModernPC", binPkgs.Union(nonBinPkgs));

            //BuildComponents("Campus", BuildVersion, @"E:\Users\Gus\Documents\Campus", @"E:\Users\Gus\Documents\Campus\CampusDeviceFM.xml", @"E:\Users\Gus\Documents\Campus\OEMDevicePlatform.xml", @"E:\Users\Gus\Documents\Campus\DeviceLayout.xml", @"E:\Users\Gus\Documents\Campus\DeviceLayout.xml", CpuArch.ARM64, "ModernPC", new List<IPackageInfo>());
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
                @"E:\Users\Gus\Documents\amd64_microsoft-onecore-s..-xro-bindflt-config_31bf3856ad364e35_10.0.20279.1002_none_233ecea4978a65b8.manifest",
                "\\windows\\WinSxS\\arm64_microsoft-onecore-s..-xro-bindflt-config_31bf3856ad364e35_10.0.20279.1002_none_233ecea4978a65b8.manifest",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DevicePlatformCabFileName = $"Microsoft-OneCore-StateSeparation-XRO-Bindflt-Config-Package~31bf3856ad364e35~AMD64~~.cab";
            cbsCabinet.SaveCab(Path.Combine($@"{ProjectRoot}\Microsoft-OneCore-StateSeparation-XRO-Bindflt-Config-Package~31bf3856ad364e35~AMD64~~.cab", DevicePlatformCabFileName));
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
                //Component = $"{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}",
                //PackageName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            List<IPackageInfo> lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DevicePlatformCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId,
                                         $"OEMDEVICEPLATFORM_TALKMAN",
                                         //$"OEMDEVICEPLATFORM_{DeviceName.ToUpper()}",
                                         "OEM",
                                         lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_TALKMAN.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));
            //cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

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

                Component = $"OneCore.{DeviceName}.SpaceDeviceLayout",
                PackageName = $"Microsoft-OneCore-{DeviceName}-SpaceDeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayoutPath,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            string DeviceLayoutCabFileName = $"Microsoft-OneCore-{DeviceName}-SpaceDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
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

                Component = $"{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES", "OEM", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

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

                Component = $"OneCore.{DeviceName}.LegacyDeviceLayout",
                PackageName = $"Microsoft-OneCore-{DeviceName}-LegacyDeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            cbsCabinet.AddFile(FileType.Regular,
                DeviceLayoutLegacyPath,
                @"\Windows\ImageUpdate\DeviceLayout.xml",
                cbsCabinet.PackageName);

            cbsCabinet.Validate();
            DeviceLayoutCabFileName = $"Microsoft-OneCore-{DeviceName}-LegacyDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
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

                Component = $"{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY.{FeatureManifestId}",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY.{FeatureManifestId}.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            lst = new List<IPackageInfo>
            {
                new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
            };

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY", "OEM", lst);

            cbsCabinet.Validate();
            cbsCabinet.SaveCab(Path.Combine(OutputPath, $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab"));

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

            cbsCabinet.SetCBSFeatureInfo(FeatureManifestId, "BASE", "OEM", SupplementalPackages.Union(new IPackageInfo[] { new CbsPackageInfo($@"{ProjectRoot}\Retail\ARM64\fre\Microsoft-Composable-ModernUX-SystemSupportedOrientations-All-Package~31bf3856ad364e35~ARM64~~.cab") }).ToList());
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
