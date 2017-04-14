param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path $PWD $Configuration),
    [switch]$Portable = $false,
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

# -------------------------------------------
#            Compile application 
# -------------------------------------------
if (!$SkipBuild)
{
    if (Test-Path $OutputPath)
    {
        Remove-Item $OutputPath -Recurse -Force
    }

    # Restore NuGet packages
    if (-not (Test-Path "nuget.exe"))
    {
        Invoke-WebRequest -Uri $NugetUrl -OutFile "nuget.exe"
    }

    $nugetProc = Start-Process "nuget.exe" "restore ..\source\LiteDbExplorer.sln" -PassThru -NoNewWindow
    $handle = $nugetProc.Handle
    $nugetProc.WaitForExit()

    $solutionDir = Join-Path $pwd "..\source"
    $msbuildPath = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir`" /p:OutputPath=`"$outputPath`";Configuration=$configuration /property:Platform=x86 /t:Build";
    $compiler = Start-Process $msbuildPath $arguments -PassThru -NoNewWindow
    $handle = $compiler.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
    $compiler.WaitForExit()

    if ($compiler.ExitCode -ne 0)
    {
        $appCompileSuccess = $false
        Write-Host "Build failed." -ForegroundColor "Red"
    }
    else
    {
        $appCompileSuccess = $true
    }
}
else
{
    $appCompileSuccess = $true
}

# -------------------------------------------
#            Build portable package
# -------------------------------------------
if ($Portable -and $appCompileSuccess)
{
    Write-Host "Building portable package..." -ForegroundColor Green

    $packageName = "LiteDBExplorer.zip"

    if (Test-path $packageName)
    {
        Remove-Item $packageName
    }

    Add-Type -assembly "System.IO.Compression.Filesystem" | Out-Null
    [IO.Compression.ZipFile]::CreateFromDirectory($OutputPath, $packageName, "Optimal", $false) 
}

return $appCompileSuccess