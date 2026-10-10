#!/bin/bash

/mnt/public/other/arrpatch/ArrPatchListInterval -a -n

if [ $? -eq 0 ]; then
    rc-service sonarr restart
else
    exit 1
fi