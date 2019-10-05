@echo off
if not X%JENKINS_URL%X == XX goto autobuild
wmic process where (name="node.exe") get commandline | findstr /i /c:"webpack" | findstr /i /c:"admin_watch"> nul
if %ERRORLEVEL% == 0 goto running
runjob cmd /c node_modules\.bin\webpack --mode development --progress --watch --env.name=admin_watch
goto eof
:running
echo Watcher process already running
goto eof
:autobuild
echo This is auto build, forcing rebuild and not entering watch
echo CDN path https://d1qwl4ymp6qhug.cloudfront.net/Admin-%CONF%/-/
node_modules\.bin\webpack --mode none --env.conf=release --env.cdn=https://d1qwl4ymp6qhug.cloudfront.net/Admin-%CONF%/-/
echo Webpack - done
:eof