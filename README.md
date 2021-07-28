# VIM-VB6-Injection
A working example of how to instrument a VB6 application

There are several parts for this solution

**SetHookForInjection**
This is an orchestrator for the "show". It starts the targeted application, and hooks it.

**Playground App**
The simplest VB6 app, with one public function to invoke

**Linker Proxy**
A tool for allowing the creation of VB6 dlls that export functions.
You can read about it in more details here:
https://www.hermetic.ch/vbm2dll.htm
