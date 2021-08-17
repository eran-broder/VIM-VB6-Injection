#pragma warning(disable:4996)

#include <filesystem>
#include <iostream>
#include <Windows.h>
#include "dynamicClrHosting.h"
#include "Utils.h" //TODO: bad include. should be in a shared project

constexpr auto kPathToClr = R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\bin\Release\net5.0\publish\coreclr.dll)";

DWORD WINAPI ListenerThread(LPVOID lpParam);
BOOL setup_thread(std::string);
BOOL AllocateConsole();
std::string GetCoreClrPath();

extern "C" __declspec(dllexport) LONG VimStart()
{	
	AllocateConsole();
	auto path = GetCoreClrPath();	
	setup_thread(path);
	return 0;
}

//TODO: the whole prefix should use a macro.
extern "C" __declspec(dllexport) LONG VimInvokeAgain(LONG arg)
{
	std::cout << "Invoked the function from the message loop" << std::endl;
	return InvokeAgain(arg);
	return 400;
}

extern "C" __declspec(dllexport) void VimLog(const LPCSTR msg)
{
	std::cout << msg << std::endl;
}

extern "C" __declspec(dllexport) void VimLoopCallback()
{
	
}


//TODO: what is this crap? return char*
std::string GetCoreClrPath()
{
	const auto abs = std::filesystem::absolute(kPathToClr);	
	return std::string(abs.string());
}

BOOL setup_thread(std::string path) {
	DWORD threadID = 0;
	const auto path_clone = (char*)malloc(path.length() + 1);
	strcpy_s(path_clone, path.length() + 1, path.c_str());
	const auto g_hThread = CreateThread(nullptr, 0, ListenerThread, path_clone, 0, &threadID);
	if (g_hThread == nullptr) {
		//TODO: Log it
		return FALSE;
	}
	return TRUE;
}

DWORD WINAPI ListenerThread(LPVOID lpParam)
{
	auto path = (LPCSTR)lpParam;
	std::cout << "2.Got arg :" << path << std::endl;
	LoadClr(path);
	free(lpParam);
	return 0;
}

BOOL AllocateConsole()
{
	auto res = AllocConsole();
	FAIL_IF(res == FALSE, "faild allocation console")
	auto handle = freopen("CONOUT$", "w", stdout);
	FAIL_IF_NULL_MSG(handle, "failed redirecting cout")
	return TRUE;
}
