
docker rm -f dirsync
docker run -d --name dirsync --env CRON_EXP="* * * * *" --env EXEC_AT_STARTUP="0" --mount type=bind,source="C:\dirsynclog",target=/app/log --mount type=bind,source="C:\10",target=/app/source --mount type=bind,source="C:\20",target=/app/target cnbian/dirsync:0.1.0 /app/source /app/target