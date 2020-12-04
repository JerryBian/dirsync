A tiny cross platform utility tool for sync files between two directories.

### Features

- [x] Supports `Windows`, `macOS`, `Linux`
- [x] Docker image ready
- [x] Supports email notification after process finished

### Installation

Make sure .NET Core 3.1 runtime is installed. Open command line tool:

```
dotnet tool install --global DirSync
```

After the installation, check current version.

```
dirsync --version
```

If you saw the version is printed, your installation is completed.

For docker usage, please refer to [Docker section](#docker).

### Usage

```
 -c, --cleanup                Cleanup target directory files which are not in source directory. Default: False.

  -f, --force                  Force to overwrite files if already exists in target directory. Default: False.

  -s, --strict                 Do binary check of target files if already exists. Default: False.

  -i, --include                Use glob pattern to include files. Default: inlucde all files.

  -e, --exclude                Use glob pattern to exclude files. Default: no files to exclude. Note: this has higher
                               priority over -i/--include.

  --config                     Json configuration file for specifying multiple source-target mappings.

  -v, --verbose

  --help                       Display this help screen.

  --version                    Display version information.

  Source directory (pos. 1)    The source directory which sync from. Required while config file is not provided.

  Target directory (pos. 2)    The target directory which sync to. Required while config file is not provided.
```

### Samples

Sync from `/tmp/src` directory to `/tmp/target`. If the file which has same file name in target directory, it will be skipped. The required directory structure is also created automatically as needed.

`dirsync /tmp/src /tmp/target`

Sync PNG files only.


`dirsync -i "*.png" /tmp/src /tmp/target`

Sync all files except text files.

`dirsync -e "*.txt" /tmp/src /tmp/target`

Cleanup the target directory after sync finished. This can make sure target directory has exactly same file structures as source.


`dirsync -c /tmp/src /tmp/target`

Override the file if it's already exist in target.

`dirsync -f /tmp/src /tmp/target`

While there is already same file name exists in target directory, it will do binary checking to make sure the file content is same too. This operation might be slow depends on the file size.

`dirsync -s /tmp/src /tmp/target`

Make sure target directory has exactly same files as source.

`dirsync -n 1607093968 /tmp/src /tmp/target`

Only copy files whose modification date is newer than '1607093968(timestamp)' '2020/12/4 22:59:28(datetime)'.

`dirsync -f -c /tmp/src /tmp/target`

Pass in multiple source-target mappings in configuration file. If this configuration file is specified, the source and target position arguments will be ignored.

`dirsync --config /tmp/dirsync.json`

Sample `dirsync.json`:

```json
[
    {
        "src": "/app/source/music",
        "target": "/app/target/backup/music"
    },
    {
        "src": "/app/source/movie",
        "target": "/app/target/backup/movie"
    }
]
```

### Enviroment variables

`SENDGRID_API_KEY`

This is required for sending notifications after the process finished. 

You can apply the key in SendGrid.

`FILE_LOG_DIR`

In order for file logging, you need to provide valid directory path as environment variable. It will be created if this directory doesn't exist yet.

`EMAIL_TO_ADDRESS`

The target email address for notifications.

`EMAIL_TO_NAME`

The target name for notifications.

`CRON_EXP`

This is docker container only. Specify CRON expression to make sync happens periodly.

`EXEC_AT_STARTUP`

This is docker container only. Specify whether the sync happens immediately once container starts.

### Docker

There is additional features for Docker containers: you can specify the CRON expression to make the sync process happens periodly. This is quite useful if you want to backup your files at soem specified time points.

