// HookedLibrary.cpp : Defines the exported functions for the DLL.
//

#include "pch.h"
#include "framework.h"
#include "HookedLibrary.h"
#include <stdio.h>
#include <windows.h>



// This is an example of an exported variable
HOOKEDLIBRARY_API int nHookedLibrary=0;

// This is an example of an exported function.
HOOKEDLIBRARY_API int fnHookedLibrary(void)
{
    return 0;
}

// This is the constructor of a class that has been exported.
CHookedLibrary::CHookedLibrary()
{
    return;
}

static bool RunFlag = false;

typedef int (CALLBACK* VBSIG)(int code);

extern "C" __declspec(dllexport) int broder(int code, WPARAM wParam, LPARAM lParam) {
    if (!RunFlag) {
        FILE* file;
        fopen_s(&file, "C:\\t\\function.txt", "w");
        fprintf(file, "Function keyboard_hook called.\n");


        auto dll = LoadLibrary(L"C:\\vim\\vblab\\vbm2dll\\Called.dll");
        fprintf(file, "dll : [%d]", dll);
        if (dll == 0) {
            fprintf(file, "error : [%d]", GetLastError());
        }
        auto addr = (VBSIG)GetProcAddress(dll, "Broder");
        fprintf(file, "addr : [%ul]", addr);

        fprintf(file, "thread : [%ul]", GetCurrentThread());

        auto msgPointer = (PMSG)lParam;
        fprintf(file, "hwnd : [%ul]", msgPointer->hwnd);
        auto result = addr((int)(msgPointer->hwnd));
        fprintf(file, "result: [%d]", result);
        fclose(file);        

        
        RunFlag = true;
    }

    return(CallNextHookEx(NULL, code, wParam, lParam));
}