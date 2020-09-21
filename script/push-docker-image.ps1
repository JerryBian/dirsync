# pack docker images

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [String]
    $Version
)

$DockerUserName=[System.Environment]::GetEnvironmentVariable('DockerUserName')
$DockerPassword=[System.Environment]::GetEnvironmentVariable('DockerPassword')
$DockerPassword | docker login -u $DockerUserName --password-stdin

dotnet publish ../src/DirSync.csproj -p:PublishTrimmed=true -p:PublishSingleFile=true -r linux-musl-x64 -c Release -o ../__docker --self-contained
Copy-Item -Path "../Dockerfile" -Destination "../__docker"
cd ../__docker
docker build --tag cnbian/dirsync:$Version --tag cnbian/dirsync:latest .
docker push cnbian/dirsync:$Version
docker push cnbian/dirsync:latest