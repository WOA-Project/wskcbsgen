using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;
using Microsoft.Composition.Packaging.Interfaces;

namespace WSKCBSGen.NMO
{
    class Program
    {
        static readonly string ProjectRoot = @"C:\Users\gus33\Documents\GitHub\WSKCBSGen\Project";

        static readonly string PhoneName              = "Martini";
        static readonly string BuildVersion           = "10.0.17686.1003";
        static readonly string WSKBuildVersion        = "10.0.22000.0";
        static readonly string WSKLocation            = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\Documents\GitHub\WSKCBSGen\WSK";

        static readonly string OEMOutputDir           = $@"{ProjectRoot}\NMO\{PhoneName}\ARM\fre";

        static readonly string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
        static readonly string OEMCBSPublicKey2       = "628844477771337a";

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", $@"{Environment.GetEnvironmentVariable("PATH")}{WSKLocation}\bin\{WSKBuildVersion}\x64\;");
            Environment.CurrentDirectory = $@"{WSKLocation}\tools\bin\i386";

            string DeviceFM = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}DeviceFM.xml";
            string RetailDeviceFM = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}RetailDeviceFM.xml";
            string NonTestDeviceFM = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}NonTestDeviceFM.xml";
            string OSProductDeviceFM = $@"{ProjectRoot}\BuildCabs.{PhoneName}\Andromeda{PhoneName}FM.xml";

            string OEMDevicePlatform = $@"{ProjectRoot}\BuildCabs.{PhoneName}\OEMDevicePlatform.xml";
            string DeviceLayout = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayout.xml";

            BuildComponents(PhoneName, BuildVersion, OEMOutputDir, DeviceFM, RetailDeviceFM, NonTestDeviceFM, OSProductDeviceFM, OEMDevicePlatform, DeviceLayout, CpuArch.ARM, "AndromedaOS");
        }

        public static void BuildComponents(
            string DeviceName,
            string BuildVersion,
            string OutputPath,
            string DeviceFMPath,
            string RetailDeviceFMPath,
            string NonTestDeviceFMPath,
            string OSProductDeviceFMPath,
            string OEMDevicePlatformPath,
            string DeviceLayoutPath,
            CpuArch OSArchitecture,
            string OSProductName)
        {
            string FeatureManifestId = DeviceName.ToUpper()[..4];

            CbsPackage DeviceLayoutCbsCabinet = new()
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

                Component = $"OneCore.{DeviceName}.DeviceLayout",
                PackageName = $"Microsoft-OneCore-{DeviceName}-DeviceLayout-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            DeviceLayoutCbsCabinet.AddFile(FileType.Regular, DeviceLayoutPath, "$(runtime.bootdrive)\\Windows\\ImageUpdate\\DeviceLayout.xml", $"Microsoft-OneCore-{DeviceName}-DeviceLayout-Package");

            DeviceLayoutCbsCabinet.Validate();
            DeviceLayoutCbsCabinet.SaveCab(@$"{OutputPath}\{DeviceLayoutCbsCabinet.PackageName}.cab");



            CbsPackage DevicePlatformCbsCabinet = new()
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

                Component = $"OneCore.{DeviceName}.OemDevicePlatform",
                PackageName = $"Microsoft-OneCore-{DeviceName}-OemDevicePlatform-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            DevicePlatformCbsCabinet.AddFile(FileType.Regular, OEMDevicePlatformPath, @"$(runtime.bootdrive)\Windows\ImageUpdate\OEMDevicePlatform.xml", $"Microsoft-OneCore-{DeviceName}-OemDevicePlatform-Package");

            DevicePlatformCbsCabinet.Validate();
            DevicePlatformCbsCabinet.SaveCab(@$"{OutputPath}\{DevicePlatformCbsCabinet.PackageName}.cab");



            CbsPackage DeviceLayoutFIPCbsCabinet = new()
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

                Component = $"{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SELFHOST.{FeatureManifestId}FM",
                PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}_SELFHOST.{FeatureManifestId}FM.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            List<IPackageInfo> lst =
            [
                new CbsPackageInfo(@$"{OutputPath}\{DeviceLayoutCbsCabinet.PackageName}.cab"),
            ];

            DeviceLayoutFIPCbsCabinet.SetCBSFeatureInfo($"{FeatureManifestId}FM", $"DEVICELAYOUT_{DeviceName.ToUpper()}_SELFHOST", "Microsoft", lst);

            DeviceLayoutFIPCbsCabinet.Validate();

            DeviceLayoutFIPCbsCabinet.SaveCab(@$"{OutputPath}\{DeviceLayoutFIPCbsCabinet.PackageName}.cab");



            CbsPackage DevicePlatformFIPCbsCabinet = new()
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

                Component = $"{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}FM",
                PackageName = $"Microsoft.{OSProductName}.OEMDEVICEPLATFORM_{DeviceName.ToUpper()}.{FeatureManifestId}FM.FIP",
                SubComponent = "FIP",
                PublicKey = OEMCBSPublicKey2
            };

            List<IPackageInfo> lst2 =
            [
                new CbsPackageInfo(@$"{OutputPath}\{DevicePlatformCbsCabinet.PackageName}.cab"),
            ];

            DevicePlatformFIPCbsCabinet.SetCBSFeatureInfo($"{FeatureManifestId}FM", $"OEMDEVICEPLATFORM_{DeviceName.ToUpper()}", "Microsoft", lst2);

            DevicePlatformFIPCbsCabinet.Validate();

            DevicePlatformFIPCbsCabinet.SaveCab(@$"{OutputPath}\{DevicePlatformFIPCbsCabinet.PackageName}.cab");



            CbsPackage ImageCustomizationCbsCabinet = new()
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

                Component = $"Andromeda.ImageCustomization.{DeviceName}",
                PackageName = $"Microsoft-Andromeda-ImageCustomization-{DeviceName}-Package",
                SubComponent = "",
                PublicKey = MicrosoftCBSPublicKey1
            };

            //TODO: add reg stuff in

            ImageCustomizationCbsCabinet.Validate();
            ImageCustomizationCbsCabinet.SaveCab(@$"{OutputPath}\{ImageCustomizationCbsCabinet.PackageName}.cab");



            CbsPackage ProductDeviceFMCbsCabinet = new()
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

                Component = $"{OSProductName}.Andromeda{DeviceName}FM",
                PackageName = $"Microsoft.{OSProductName}.Andromeda{DeviceName}FM",
                SubComponent = "",
                PublicKey = OEMCBSPublicKey2
            };

            ProductDeviceFMCbsCabinet.AddFile(FileType.Regular, OSProductDeviceFMPath, @$"$(runtime.systemroot)\ImageUpdate\FeatureManifest\Microsoft\Andromeda{DeviceName}FM.xml", "");

            List<IPackageInfo> lst3 =
            [
                new CbsPackageInfo(@$"{OutputPath}\{ImageCustomizationCbsCabinet.PackageName}.cab"),
            ];

            ProductDeviceFMCbsCabinet.SetCBSFeatureInfo($"AOS{FeatureManifestId}FM", "BASE", "Microsoft", lst3);

            ProductDeviceFMCbsCabinet.Validate();

            ProductDeviceFMCbsCabinet.SaveCab(@$"{OutputPath}\{ProductDeviceFMCbsCabinet.PackageName}.cab");




            CbsPackage DeviceFMCbsCabinet = new()
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
                PublicKey = OEMCBSPublicKey2
            };

            DeviceFMCbsCabinet.AddFile(FileType.Regular, DeviceFMPath, @$"$(runtime.systemroot)\ImageUpdate\FeatureManifest\Microsoft\{DeviceName}DeviceFM.xml", "");

            List<IPackageInfo> lst4 = [];

            //TODO: add driver stuff

            //lst4.Add(new CbsPackageInfo(@$"{OutputPath}\{cab3.PackageName}.cab"));
            DeviceFMCbsCabinet.SetCBSFeatureInfo($"{FeatureManifestId}FM", "BASE", "Microsoft", lst4);

            DeviceFMCbsCabinet.Validate();
            DeviceFMCbsCabinet.SaveCab(@$"{OutputPath}\{DeviceFMCbsCabinet.PackageName}.cab");




            CbsPackage NonTestDeviceFMCbsCabinet = new()
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

                Component = $"{OSProductName}.{DeviceName}NonTestDeviceFM",
                PackageName = $"Microsoft.{OSProductName}.{DeviceName}NonTestDeviceFM",
                SubComponent = "",
                PublicKey = OEMCBSPublicKey2
            };

            NonTestDeviceFMCbsCabinet.AddFile(FileType.Regular, NonTestDeviceFMPath, @$"$(runtime.systemroot)\ImageUpdate\FeatureManifest\Microsoft\{DeviceName}NonTestDeviceFM.xml", "");

            List<IPackageInfo> lst5 = [];
            NonTestDeviceFMCbsCabinet.SetCBSFeatureInfo($"{FeatureManifestId}NTFM", "BASE", "Microsoft", lst5);

            NonTestDeviceFMCbsCabinet.Validate();
            NonTestDeviceFMCbsCabinet.SaveCab(@$"{OutputPath}\{NonTestDeviceFMCbsCabinet.PackageName}.cab");



            CbsPackage RetailDeviceCbsCabinet = new()
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

                Component = $"{OSProductName}.{DeviceName}RetailDeviceFM",
                PackageName = $"Microsoft.{OSProductName}.{DeviceName}RetailDeviceFM",
                SubComponent = "",
                PublicKey = OEMCBSPublicKey2
            };

            RetailDeviceCbsCabinet.AddFile(FileType.Regular, RetailDeviceFMPath, @$"$(runtime.systemroot)\ImageUpdate\FeatureManifest\Microsoft\{DeviceName}RetailDeviceFM.xml", "");

            List<IPackageInfo> lst6 =
            [
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-CoreSysCRT120-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-Andromeda-OneScreen-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Qualcomm-DebuggerTransport-Network-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-RIL-TestInfra-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-RIL-TestInfra-Package_Lang_en-US.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-RIL-TestInfra-Package_Lang_qps-ploc.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-Onecore-Test-TNE-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-Onecore-Test-TNE-Package_Lang_en-US.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-Onecore-Test-TNE-Package_Lang_qps-ploc.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-FakeRcs-Package.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-FakeRcs-Package_Lang_en-US.cab"),
                new CbsPackageInfo(@$"{WSKLocation}\MSPackages\Retail\{OSArchitecture.ToString().ToUpper()}\fre\Microsoft-OneCore-Cellcore-FakeRcs-Package_Lang_qps-ploc.cab"),
            ];
            RetailDeviceCbsCabinet.SetCBSFeatureInfo($"{FeatureManifestId}RETFM", "BASE", "Microsoft", lst6);

            RetailDeviceCbsCabinet.Validate();
            RetailDeviceCbsCabinet.SaveCab(@$"{OutputPath}\{RetailDeviceCbsCabinet.PackageName}.cab");
        }
    }
}
