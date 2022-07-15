using System;
using System.IO;

namespace ArtifactBuilder.Artifacts
{

    public class ZipArchive : Artifact
    {
        private const string FrameworkSubDirectoryName = "netframework";
        private const string CoreSubDirectoryName = "netcore";

        public ZipArchive(string platform, string configuration) : base(nameof(ZipArchive))
        {
            Platform = platform;
            Configuration = configuration;
            StagingDirectory = $@"{RepoRootDirectory}\build\_staging\{Name}-{Platform}";
            OutputDirectory = $@"{RepoRootDirectory}\build\BuildArtifacts\{Name}-{Platform}";
        }

        public string Configuration { get; }
        public string Platform { get; }

        protected override void InternalBuild()
        {
            var frameworkAgentComponents = AgentComponents.GetAgentComponents(AgentType.Framework, Configuration, Platform, RepoRootDirectory, HomeRootDirectory);
            frameworkAgentComponents.ValidateComponents();
            frameworkAgentComponents.CopyComponents(StagingDirectory, FrameworkSubDirectoryName);

            var coreAgentComponents = AgentComponents.GetAgentComponents(AgentType.Core, Configuration, Platform, RepoRootDirectory, HomeRootDirectory);
            coreAgentComponents.ValidateComponents();
            coreAgentComponents.CopyComponents(StagingDirectory, CoreSubDirectoryName);

            var agentInfo = new AgentInfo
            {
                InstallType = $"ZipWin{Platform}"
            };

            agentInfo.WriteToDisk(Path.Combine(StagingDirectory, FrameworkSubDirectoryName));
            agentInfo.WriteToDisk(Path.Combine(StagingDirectory, CoreSubDirectoryName));

            var zipFilePath = $@"{OutputDirectory}\NewRelicDotNetAgent_{frameworkAgentComponents.SemanticVersion}_{Platform}.zip";
            Directory.CreateDirectory(OutputDirectory);
            System.IO.Compression.ZipFile.CreateFromDirectory(StagingDirectory, zipFilePath);
            File.WriteAllText($@"{OutputDirectory}\checksum.sha256", FileHelpers.GetSha256Checksum(zipFilePath));

            Console.WriteLine($"Successfully created artifact for {nameof(ZipArchive)}.");
        }
    }
}
