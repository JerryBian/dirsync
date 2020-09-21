# build docker image

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [String]
    $Version
)

New-Item ../__docker -ItemType Directory -ErrorAction SilentlyContinue
dotnet publish ../src/DirSync.csproj -p:PublishTrimmed=true -p:PublishSingleFile=true -r linux-musl-x64 -c Release -o ../__docker --self-contained
Copy-Item -Path "../Dockerfile" -Destination "../__docker"
Copy-Item -Path "../script/docker-run.sh" -Destination "../__docker"
cd ../__docker
docker build --tag cnbian/dirsync:$Version .
cd ../script