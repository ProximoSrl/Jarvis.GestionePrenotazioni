@ECHO OFF
if '%VisualStudioversion%' == '' goto usevscmd
msbuild
pause
goto exit


:usevscmd
echo 'Please run this script from Developer Command Prompt for VS 2015/7'


:exit