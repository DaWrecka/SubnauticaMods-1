:: usage: switch_branches.bat <SN|BZ> <stable|exp>
@echo off

if "%1" == "" goto :eof
if "%2" == "" goto :eof

set game=%1
set branch=%2

if not %branch% == exp if not %branch% == stable goto :eof

if %game% == SN goto :SN
if %game% == BZ goto :BZ
goto :eof

:SN :: Subnautica, stable branch on Epic, experimental on Steam

set games_folder="c:\games"
set sn_folder="subnautica"
set sn_stable_folder=".subnautica.stable"
set sn_exp_folder="d:\games\steamapps\common\subnautica\"

if %branch% == exp goto :SN_TO_EXP
if %branch% == stable goto :SN_TO_STABLE
goto :eof

:SN_TO_EXP
if exist %games_folder%\%sn_stable_folder% goto :eof

ren %games_folder%\%sn_folder% %sn_stable_folder%
attrib +H %games_folder%\%sn_stable_folder%

mklink /j %games_folder%\%sn_folder% %sn_exp_folder% 1>nul
goto :exit

:SN_TO_STABLE
if not exist %games_folder%\%sn_stable_folder% goto :eof

rmdir %games_folder%\%sn_folder%
attrib -H %games_folder%\%sn_stable_folder%
ren %games_folder%\%sn_stable_folder% %sn_folder%
goto :exit

:BZ :: BelowZero, both branches on Steam

set steam_folder="c:\games\{steam}\steamapps\common"
set branches_folder="d:\games"
set game_folder="SubnauticaZero"
set game_manifest="appmanifest_848450.acf"

taskkill /t /f /im steam.exe 1>nul 2>nul

rmdir %steam_folder%\%game_folder%
mklink /j %steam_folder%\%game_folder% %branches_folder%\%game_folder%.%branch% 1>nul

del %steam_folder%\..\%game_manifest%
mklink /h %steam_folder%\..\%game_manifest% %steam_folder%\..\%game_manifest%.%branch% 1>nul

:exit
echo:
echo %game% branch switched to "%branch%"