#include <Windows.h>
#include <iostream>

BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,  // handle to DLL module
    DWORD fdwReason,     // reason for calling function
    LPVOID lpReserved)  // reserved
{
    // Perform actions based on the reason for calling.
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
        // Initialize once for each new process.
        // Return FALSE to fail DLL load.
        break;

    case DLL_THREAD_ATTACH:
        // Do thread-specific initialization.
        break;

    case DLL_THREAD_DETACH:
        // Do thread-specific cleanup.
        break;

    case DLL_PROCESS_DETACH:
        // Perform any necessary cleanup.
        break;
    }
    return TRUE;  // Successful DLL_PROCESS_ATTACH.
}

typedef LONG(WINAPI* soft_wish_ptr)(void);

extern "C" __declspec(dllexport) LRESULT GetMsgProc(
	int    code,
	WPARAM wParam,
	LPARAM lParam)
{
	PMSG asMsg = reinterpret_cast<PMSG>(lParam);
	Beep(1400, 50);
	
	std::cout << "GOT MESSAGE[" << asMsg->message << "]" << std::endl;

	auto hmod = LoadLibraryA("VimInProcessOrchestrator.dll");
    auto proc = GetProcAddress(hmod, "SoftWish");
    soft_wish_ptr ptr = reinterpret_cast<soft_wish_ptr>(proc);
    ptr();
    std::cout << "hmod" << hmod << std::endl;
	
	
	return CallNextHookEx(nullptr, code, wParam, lParam);
}
