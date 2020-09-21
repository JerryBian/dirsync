# pack global tool

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [String]
    $Version
)

$OutputPath="../__nupkg/$Version"
dotnet pack ../src/DirSync.csproj -c Release --include-source --include-symbols --output $OutputPath -p:PackageVersion=$Version

$PkgPath="$OutputPath/dirsync.$Version.nupkg"
if (!(Test-Path $PkgPath)){
    Write-Error "No package found under $PkgPath"
}
else{
    $ApiKey=[System.Environment]::GetEnvironmentVariable('NugetApiKey')
    if(!$ApiKey){
        Write-Error "Invalid ApiKey. Please check environment variable NugetApiKey."
    }
    else{
        dotnet nuget push $PkgPath -k $ApiKey -s https://api.nuget.org/v3/index.json
    }
}