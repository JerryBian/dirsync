#!/bin/sh

chmod +x ./dirsync
if [[ ! -z "$CRON_EXP" ]]; then
  mkdir -p /app/log
  (crontab -l ; echo "$CRON_EXP /app/dirsync $@ > /app/log/dirsync-cron.log 2>&1") | crontab -
  /usr/sbin/crond -b -L /app/log/cron.log
fi

if [[ -z "$EXEC_AT_STARTUP" ]] || [[ "$EXEC_AT_STARTUP" == "1" ]]; then
  ./dirsync "$@" > /app/log/dirsync.log 2>&1
fi

while true; do
    sleep 1h
done