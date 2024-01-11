# ArrPatchListInterval

### Description:  
&nbsp;&nbsp;&nbsp;&nbsp;Modifies Sonarr/Radarr Plex List default time interval.  
  
### Usage:  
&nbsp;&nbsp;&nbsp;&nbsp;ArrPatchListInterval [options] [file]...  
  
### File:  
&nbsp;&nbsp;&nbsp;&nbsp;Multiple file paths can be specified, separated by space.  
&nbsp;&nbsp;&nbsp;&nbsp;If the path contains spaces, wrap in quotes.  
&nbsp;&nbsp;&nbsp;&nbsp;(default: "C:\ProgramData\Radarr\bin\Radarr.Core.dll"  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"C:\ProgramData\Sonarr\bin\Sonarr.Core.dll")
  
### Options:  
&nbsp;&nbsp;&nbsp;&nbsp;-f [hours] Current interval (default: 6).  
&nbsp;&nbsp;&nbsp;&nbsp;-t [hours] New interval     (default: 0.1).  
&nbsp;&nbsp;&nbsp;&nbsp;-n &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Don't create a backup.  
&nbsp;&nbsp;&nbsp;&nbsp;-a &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Auto mode, will not require user input.  
  
### Examples:  
&nbsp;&nbsp;&nbsp;&nbsp;ArrPatchListInterval -n C:\ProgramData\Radarr\bin\Radarr.Core.dll C:\ProgramData\Sonarr\bin\Sonarr.Core.dll  
&nbsp;&nbsp;&nbsp;&nbsp;ArrPatchListInterval -f 6 -t 1.5 "C:\Program Files\Sonarr\bin\Sonarr.Core.dll"  
&nbsp;&nbsp;&nbsp;&nbsp;ArrPatchListInterval -a  