@echo off
rem We do not use setlocal here because we want these variables to be available to callers

set ServiceName=Tailviewer.PluginRepository
set ScriptDir=%~dp0
set ServicePath=%ScriptDir%repository-svc.exe


