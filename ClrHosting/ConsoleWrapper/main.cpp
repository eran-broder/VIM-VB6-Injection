#include <iostream>
#include <filesystem>
#include <string>

#include "dynamicClrHosting.h"
#include "ClrWrapper.h"

constexpr auto kPathToClr = R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\bin\Release\net5.0\publish\coreclr.dll)";

DWORD WINAPI ListenerThread(LPVOID lpParam);
HANDLE setup_thread(LPCSTR arg);


typedef int (WINAPI* doWork_ptr)(HWND x);
auto main() -> int
{	
	const auto a1 = std::filesystem::absolute(kPathToClr);
	const auto a2 = a1.string();
	const auto absolute_path = a2.c_str();
	std::cout << "Loading from : " << absolute_path << std::endl;
	
	//LoadClr(absolute_path);
	auto threadHandle = setup_thread(kPathToClr);
	WaitForSingleObject(threadHandle, INFINITE);

}

HANDLE setup_thread(LPCSTR arg) {
    DWORD threadID = 0;
    const auto g_hThread = CreateThread(nullptr, 0, ListenerThread, (LPVOID)arg, 0, &threadID);
    if (g_hThread == nullptr) {
        //TODO: Log it
        return 0;
    }
	return g_hThread;
}

DWORD WINAPI ListenerThread(LPVOID lpParam)
{
	auto path = (LPCSTR)lpParam;
	auto clr = LoadClr(path);
	auto runner = clr->GetClass("ManagedAssemblyRunner, Version=1.0.0.0", "ManagedAssemblyRunner.Runner");
	auto method = static_cast<doWork_ptr>(runner.GetMethod("DoWork"));
	method(nullptr);
	if(!clr->Shutdown()){
		std::cout << "error shutting down" << std::endl;
	}
	else{
		std::cout << "after shutdown" << std::endl;
	}
	std::cout << "enter to free library" << std::endl;
	std::string str;
	std::getline(std::cin, str);
	auto h = LoadLibraryA("ManagedAssemblyRunner");
	FreeLibrary(h);

	std::cout << "enter to reload library" << std::endl;	
	std::getline(std::cin, str);
	
	runner = clr->GetClass("ManagedAssemblyRunner, Version=1.0.0.0", "ManagedAssemblyRunner.Runner");
	method = static_cast<doWork_ptr>(runner.GetMethod("DoWork"));
	method(nullptr);
	return 0;
}