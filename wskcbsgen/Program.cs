using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;

namespace WSKCBSGen
{
    class Program
    {
        static readonly string ProjectRoot            = @"C:\Users\gus33\Documents\GitHub\WSKCBSGen\Project";

        static readonly string PhoneName              = "Andromeda855";
        static readonly string BuildVersion           = "10.0.20279.1002";
        static readonly string WSKBuildVersion        = "10.0.22000.0";
        static readonly string WSKLocation            = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\Documents\GitHub\WSKCBSGen\WSK";

        static readonly string OEMOutputDir           = $@"{ProjectRoot}\Microsoft\{PhoneName}\ARM64\fre";

        static readonly string MicrosoftCBSPublicKey1 = "31bf3856ad364e35";
        static readonly string OEMCBSPublicKey2       = "628844477771337a";

        static void Main(string[] _)
        {
            Environment.SetEnvironmentVariable("SIGN_OEM", "1");
            Environment.SetEnvironmentVariable("PATH", $@"{Environment.GetEnvironmentVariable("PATH")}{WSKLocation}\bin\{WSKBuildVersion}\x64\;");
            Environment.CurrentDirectory = $@"{WSKLocation}\tools\bin\i386";

            string DeviceFM           = $@"{ProjectRoot}\BuildCabs.{PhoneName}\{PhoneName}DeviceFM.xml";
            string DeviceLayout       = $@"{ProjectRoot}\BuildCabs.{PhoneName}\DeviceLayout.xml";

            BuildComponents(PhoneName, BuildVersion, OEMOutputDir, DeviceFM, DeviceLayout, CpuArch.ARM64, "ModernPC");
        }

        public static void BuildComponents(
            string DeviceName,
            string BuildVersion,
            string OutputPath,
            string DeviceFMPath,
            string DeviceLayoutPath,
            CpuArch OSArchitecture,
            string OSProductName)
        {
            string FeatureManifestId = $"{OSProductName.ToUpper()}{string.Join("", DeviceName.ToUpper().Take(3))}DEV";

            string DeviceLayoutCabFileName = 
                $"Microsoft-OneCore-{DeviceName}-DeviceLayout-Package~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

            {
                CbsPackage DeviceLayoutCbsCabinet = new()
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

                    Component = $"OneCore.{DeviceName}.DeviceLayout",
                    PackageName = $"Microsoft-OneCore-{DeviceName}-DeviceLayout-Package",
                    SubComponent = "",
                    PublicKey = MicrosoftCBSPublicKey1
                };

                DeviceLayoutCbsCabinet.AddFile(FileType.Regular,
                    DeviceLayoutPath,
                    @"\Windows\ImageUpdate\DeviceLayout.xml",
                    DeviceLayoutCbsCabinet.PackageName);

                DeviceLayoutCbsCabinet.Validate();

                DeviceLayoutCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutCabFileName));
            }

            {
                string DeviceLayoutFIPCabFileName =
                    $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}.{FeatureManifestId}.FIP~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

                CbsPackage DeviceLayoutFIPCbsCabinet = new()
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

                    Component = $"{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}.{FeatureManifestId}",
                    PackageName = $"Microsoft.{OSProductName}.DEVICELAYOUT_{DeviceName.ToUpper()}.{FeatureManifestId}.FIP",
                    SubComponent = "FIP",
                    PublicKey = OEMCBSPublicKey2
                };

                DeviceLayoutFIPCbsCabinet.SetCBSFeatureInfo(FeatureManifestId, $"DEVICELAYOUT_{DeviceName.ToUpper()}", "OEM", [new CbsPackageInfo(Path.Combine(OutputPath, DeviceLayoutCabFileName))]);

                DeviceLayoutFIPCbsCabinet.Validate();

                DeviceLayoutFIPCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceLayoutFIPCabFileName));
            }

            {
                string DeviceFMCabFileName =
                    $"Microsoft.{OSProductName}.{DeviceName}DeviceFM~{MicrosoftCBSPublicKey1}~{OSArchitecture.ToString().ToUpper()}~~.cab";

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

                DeviceFMCbsCabinet.AddFile(
                    FileType.Regular,
                    DeviceFMPath,
                    $@"\Windows\ImageUpdate\FeatureManifest\OEM\{DeviceName}DeviceFM.xml",
                    ""
                );

                DeviceFMCbsCabinet.SetCBSFeatureInfo(FeatureManifestId, "BASE", "OEM", new List<Microsoft.Composition.Packaging.Interfaces.IPackageInfo>());

                DeviceFMCbsCabinet.Validate();

                DeviceFMCbsCabinet.SaveCab(Path.Combine(OutputPath, DeviceFMCabFileName));
            }
        }
    }
}
