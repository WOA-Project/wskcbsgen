using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;
using Microsoft.Composition.Packaging.Interfaces;

namespace WSKCBSGen.MMO
{
    class Program
    {
        static readonly string ProjectRoot = @"C:\Users\gus33\Documents\GitHub\WSKCBSGen\Project";

        static readonly string PhoneName              = "Talkman";
        static readonly string BuildVersion           = "10.0.20279.1002";
        static readonly string WSKBuildVersion        = "10.0.22000.0";
        static readonly string WSKLocation            = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\Documents\GitHub\WSKCBSGen\WSK";

        static readonly string OEMOutputDir           = $@"{ProjectRoot}\MMO\{PhoneName}\ARM64\fre";
        static readonly string binDirectory           = $@"{ProjectRoot}\BuildCabs.{PhoneName}\bin";
        static readonly string nonBinDirectory        = $@"{ProjectRoot}\BuildCabs.{PhoneName}\nonbin";

        static readonly string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
        static readonly string OEMCBSPublicKey2       = "628844477771337a";

        static void Main(string[] _)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", $@"{Environment.GetEnvironmentVariable("PATH")}{WSKLocation}\bin\{WSKBuildVersion}\x64\;");
            Environment.CurrentDirectory = $@"{WSKLocation}\tools\bin\i386";

            CbsPackageInfo[] binPkgs    = BuildBinaryPackages($"{PhoneName}", CpuArch.ARM64);
            CbsPackageInfo[] nonBinPkgs = BuildNonBinaryPackages($"{PhoneName}", CpuArch.ARM64);

            string Output             = $@"{ProjectRoot}\MMO\{PhoneName}\ARM64\fre";
            string DeviceFM           = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}DeviceFM.xml";
            string OEMDevicePlatform  = $@"{ProjectRoot}\BuildCabs.{PhoneName}\OEMDevicePlatform.xml";
            string DeviceLayout       = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayout.xml";
            string DeviceLayoutLegacy = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayoutNonPool.xml";

            BuildComponents(PhoneName, BuildVersion, Output, DeviceFM, OEMDevicePlatform, DeviceLayout, DeviceLayoutLegacy, CpuArch.ARM64, "ModernPC", binPkgs.Union(nonBinPkgs));
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

            string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-DevicePlatform-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            string DeviceLayoutCabFileName = $"Microsoft-OneCore-{DeviceName}-SpaceDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
            string LegacyDeviceLayoutCabFileName = $"Microsoft-OneCore-{DeviceName}-LegacyDeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

            {
                CbsPackage DevicePlatformCbsCabinet = new()
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

                DevicePlatformCbsCabinet.AddFile(FileType.Regular,
                    OEMDevicePlatformPath,
                    @"\Windows\ImageUpdate\OEMDevicePlatform.xml",
                    DevicePlatformCbsCabinet.PackageName);

                DevicePlatformCbsCabinet.Validate();
                DevicePlatformCbsCabinet.SaveCab(Path.Combine(OutputPath, DevicePlatformCabFileName));
            }

            {
                string DevicePlatformFIPCabFileName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_TALKMAN.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
                //string DevicePlatformFIPCabFileName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                CbsPackage DevicePlatformFIPCbsCabinet = new()
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

                List<IPackageInfo> lst =
                [
                    new CbsPackageInfo(Path.Combine(OutputPath, DevicePlatformCabFileName))
                ];

                DevicePlatformFIPCbsCabinet.SetCBSFeatureInfo(FeatureManifestId,
                                             $"OEMDEVICEPLATFORM_TALKMAN",
                                             //$"OEMDEVICEPLATFORM_{DeviceName.ToUpper()}",
                                             "OEM",
                                             lst);

                DevicePlatformFIPCbsCabinet.Validate();

                DevicePlatformFIPCbsCabinet.SaveCab(Path.Combine(OutputPath, DevicePlatformFIPCabFileName));
            }

            {
                CbsPackage SpaceDeviceLayoutCbsCabinet = new()
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

                SpaceDeviceLayoutCbsCabinet.AddFile(FileType.Regular,
                    DeviceLayoutPath,
                    @"\Windows\ImageUpdate\DeviceLayout.xml",
                    SpaceDeviceLayoutCbsCabinet.PackageName);

                SpaceDeviceLayoutCbsCabinet.Validate();

                SpaceDeviceLayoutCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutCabFileName));
            }

            {
                string DeviceLayoutFIPCabFileName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                CbsPackage SpaceDeviceLayoutFIPCbsCabinet = new()
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

                List<IPackageInfo> lst =
                [
                    new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))
                ];

                SpaceDeviceLayoutFIPCbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{DeviceName.ToUpper()}_SPACES", "OEM", lst);

                SpaceDeviceLayoutFIPCbsCabinet.Validate();

                SpaceDeviceLayoutFIPCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutFIPCabFileName));
            }

            {
                CbsPackage LegacyDeviceLayoutCbsCabinet = new()
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

                LegacyDeviceLayoutCbsCabinet.AddFile(FileType.Regular,
                    DeviceLayoutLegacyPath,
                    @"\Windows\ImageUpdate\DeviceLayout.xml",
                    LegacyDeviceLayoutCbsCabinet.PackageName);

                LegacyDeviceLayoutCbsCabinet.Validate();

                LegacyDeviceLayoutCbsCabinet.SaveCab(Path.Combine(OutputPath, LegacyDeviceLayoutCabFileName));
            }

            {
                string LegacyDeviceLayoutFIPCabFileName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                CbsPackage LegacyDeviceLayoutFIPCbsCabinet = new()
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

                List<IPackageInfo> lst =
                [
                    new CbsPackageInfo(Path.Combine(OutputPath, LegacyDeviceLayoutCabFileName))
                ];

                LegacyDeviceLayoutFIPCbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{DeviceName.ToUpper()}_LEGACY", "OEM", lst);

                LegacyDeviceLayoutFIPCbsCabinet.Validate();
                LegacyDeviceLayoutFIPCbsCabinet.SaveCab(Path.Combine(OutputPath, LegacyDeviceLayoutFIPCabFileName));
            }

            {
                string DeviceFMCabFileName = $"Microsoft.{OSProductName}.{DeviceName}DeviceFM~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                CbsPackage DeviceFMCbsCabinet = new()
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

                DeviceFMCbsCabinet.AddFile(FileType.Regular,
                    DeviceFMPath,
                    $@"\Windows\ImageUpdate\FeatureManifest\OEM\{DeviceName}DeviceFM.xml", "");

                DeviceFMCbsCabinet.SetCBSFeatureInfo(FeatureManifestId, "BASE", "OEM", SupplementalPackages.Union([new CbsPackageInfo($@"{ProjectRoot}\Retail\ARM64\fre\Microsoft-Composable-ModernUX-SystemSupportedOrientations-All-Package~31bf3856ad364e35~ARM64~~.cab")]).ToList());
                DeviceFMCbsCabinet.Validate();
                DeviceFMCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceFMCabFileName));
            }
        }

        public static CbsPackageInfo[] BuildNonBinaryPackages(string DeviceName, CpuArch OSArchitecture)
        {
            List<CbsPackageInfo> packages = [];

            foreach (string partitionNonBinaryFilePath in Directory.EnumerateDirectories(nonBinDirectory))
            {
                string partitionName = partitionNonBinaryFilePath.Split('\\').Last();

                CbsPackage cbsCabinet = new()
                {
                    BuildType = BuildType.Release,
                    BinaryPartition = false,
                    Owner = "Microsoft",
                    Partition = partitionName,
                    OwnerType = PhoneOwnerType.OEM,
                    PhoneReleaseType = PhoneReleaseType.Production,
                    ReleaseType = "Feature Pack",
                    HostArch = OSArchitecture,
                    Version = new Version(BuildVersion),

                    Component = $"OneCore.{DeviceName}.{partitionName}",
                    PackageName = $"Microsoft-OneCore-{DeviceName}-{partitionName}-Package",
                    SubComponent = "",
                    PublicKey = MicrosoftCBSPublicKey1
                };

                foreach (string file in Directory.EnumerateFiles(partitionNonBinaryFilePath, "*.*", SearchOption.AllDirectories))
                {
                    Console.WriteLine(file);
                    cbsCabinet.AddFile(FileType.Regular, file, file.Replace(partitionNonBinaryFilePath, ""), cbsCabinet.PackageName);
                }

                cbsCabinet.Validate();

                string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-{partitionName}-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";
                cbsCabinet.SaveCab(Path.Combine(OEMOutputDir, DevicePlatformCabFileName));
                packages.Add(new CbsPackageInfo(Path.Combine(OEMOutputDir, DevicePlatformCabFileName)));
            }

            return [.. packages];
        }

        public static CbsPackageInfo[] BuildBinaryPackages(string DeviceName, CpuArch OSArchitecture)
        {
            List<CbsPackageInfo> packages = [];

            foreach (string partitionBinaryFilePath in Directory.EnumerateFiles(binDirectory, "*.bin"))
            {
                string partitionName = Path.GetFileNameWithoutExtension(partitionBinaryFilePath);

                CbsPackage cbsCabinet = new()
                {
                    BuildType = BuildType.Release,
                    BinaryPartition = true,
                    Owner = "Microsoft",
                    Partition = partitionName,
                    OwnerType = PhoneOwnerType.OEM,
                    PhoneReleaseType = PhoneReleaseType.Production,
                    ReleaseType = "Feature Pack",
                    HostArch = OSArchitecture,
                    Version = new Version(BuildVersion),

                    Component = $"OneCore.{DeviceName}.{partitionName}",
                    PackageName = $"Microsoft-OneCore-{DeviceName}-{partitionName}-Package",
                    SubComponent = "",
                    PublicKey = MicrosoftCBSPublicKey1
                };

                cbsCabinet.AddFile(FileType.BinaryPartition, partitionBinaryFilePath, $"\\{partitionName}.bin", "");

                cbsCabinet.Validate();

                string DevicePlatformCabFileName = $"Microsoft-OneCore-{DeviceName}-{partitionName}-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                cbsCabinet.SaveCab(Path.Combine(OEMOutputDir, DevicePlatformCabFileName));
                packages.Add(new CbsPackageInfo(Path.Combine(OEMOutputDir, DevicePlatformCabFileName)));
            }

            return [.. packages];
        }
    }
}
