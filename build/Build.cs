using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    [Parameter( "Configuration to build - Default is 'Debug' (local) or 'Release' (server)" )] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    // [GitRepository] readonly GitRepository GitRepository;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
                         .Before( Restore )
                         .Executes( () =>
                         {
                             SourceDirectory.GlobDirectories( "**/bin", "**/obj" )
                                            .ForEach( DeleteDirectory );
                             TestsDirectory.GlobDirectories( "**/bin", "**/obj" )
                                           .ForEach( DeleteDirectory );
                             EnsureCleanDirectory( OutputDirectory );
                         } );

    Target Restore => _ => _
        .Executes( () =>
        {
            DotNetRestore( s => s
                               .SetProjectFile( Solution ) );
        } );

    Target Compile => _ => _
                           .DependsOn( Restore )
                           .Executes( () =>
                           {
                               DotNetBuild( s => s
                                                 .SetProjectFile( Solution )
                                                 .SetConfiguration( Configuration )
                                                 .EnableNoRestore() );
                           } );

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static Int32 Main() => Execute<Build>( x => x.Compile );
}