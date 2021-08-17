#include <iostream>
#include <filesystem>
#include "dynamicClrHosting.h"

constexpr auto kPathToClr = R"(..\ManagedLibraryForInjection\bin\Release\net5.0\publish\coreclr.dll)";

DWORD WINAPI ListenerThread(LPVOID lpParam);
BOOL setup_thread(LPCSTR arg);

auto main() -> int
{	
	const auto a1 = std::filesystem::absolute(kPathToClr);
	const auto a2 = a1.string();
	const auto absolute_path = a2.c_str();
	std::cout << "Loading from : " << absolute_path << std::endl;
	//LoadClr(absolute_path);
	setup_thread(absolute_path);
	while(TRUE)
	{
		Sleep(1000);
		std::cout << ".";
	}
}


BOOL setup_thread(LPCSTR arg) {
    DWORD threadID = 0;
    const auto g_hThread = CreateThread(nullptr, 0, ListenerThread, (LPVOID)arg, 0, &threadID);
    if (g_hThread == nullptr) {
        //TODO: Log it
        return FALSE;
    }
    return TRUE;
}

DWORD WINAPI ListenerThread(LPVOID lpParam)
{
	auto path = (LPCSTR)lpParam;
	LoadClr(path);
	return 0;
}