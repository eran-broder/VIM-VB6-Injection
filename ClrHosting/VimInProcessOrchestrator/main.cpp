#pragma warning(disable:4996)

#include <filesystem>
#include <iostream>
#include <string>
#include <Windows.h>
#include "dynamicClrHosting.h"
#include "Utils.h" //TODO: bad include. should be in a shared project


typedef int (WINAPI* doWork_ptr)(HWND x);

DWORD WINAPI ListenerThread(LPVOID lpParam);
BOOL setup_thread(std::string);
BOOL AllocateConsole();
void CreateConsole();
std::string GetCoreClrPath();
ManagedClassProxy g_cls;

constexpr auto kPathToClr = R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\bin\Debug\net5.0\coreclr.dll)";

struct ListenerThreadArgs
{
	LPCSTR clr_path;
	ClrWrapper* clrWrapper;
};

HWND g_handle;
extern "C" __declspec(dllexport)  LONG VimStart(HWND hookedWindowHandle)
{
	//AllocateConsole();
	CreateConsole();
	g_handle = hookedWindowHandle; //TODO: fuck the global var
	auto path = GetCoreClrPath();
	setup_thread(path);
	return 0;
}

//TODO: the whole prefix should use a macro.
extern "C" __declspec(dllexport) LONG VimInvokeAgain(LONG arg)
{
	std::cout << "Invoked the function from the message loop with " << arg << std::endl;
	auto managed_delegate = static_cast<doWork_ptr>(g_cls.GetMethod("InvokeFromMainThread"));
	return managed_delegate(reinterpret_cast<HWND>(arg));
}

extern "C" __declspec(dllexport) LONG VimInvokePendingAction(LONG arg)
{
	std::cout << "Lets invoke pending message :  " << arg << std::endl;
	const auto managed_delegate = static_cast<doWork_ptr>(g_cls.GetMethod("InvokePendingMessage"));
	return managed_delegate(reinterpret_cast<HWND>(arg));
}

extern "C" __declspec(dllexport) void VimLog(const LPCSTR msg)
{
	std::cout << msg << std::endl;
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
std::unique_ptr<ClrWrapper*> g_clr = nullptr;



DWORD WINAPI ListenerThread(LPVOID lpParam)
{
	//auto args = (ListenerThreadArgs*)lpParam;
	auto path = (LPCSTR)lpParam;
	std::cout << "2.Got arg :" << path << std::endl;
	auto clr = LoadClr(path);
	g_cls = clr->GetClass("ManagedLibraryForInjection, Version=1.0.0.0", "ManagedLibraryForInjection.Program");
	const auto managed_delegate = static_cast<doWork_ptr>(g_cls.GetMethod("DoWork"));
	managed_delegate(g_handle);
	free(lpParam);
	return 0;
}

void CreateConsole()
{
	if (!AllocConsole()) {
		// Add some error handling here.
		// You can call GetLastError() to get more info about the error.
		return;
	}

	// std::cout, std::clog, std::cerr, std::cin
	FILE* fDummy;
	freopen_s(&fDummy, "CONOUT$", "w", stdout);
	freopen_s(&fDummy, "CONOUT$", "w", stderr);
	freopen_s(&fDummy, "CONIN$", "r", stdin);
	std::cout.clear();
	std::clog.clear();
	std::cerr.clear();
	std::cin.clear();

	// std::wcout, std::wclog, std::wcerr, std::wcin
	HANDLE hConOut = CreateFile(L"CONOUT$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	HANDLE hConIn = CreateFile(L"CONIN$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	SetStdHandle(STD_OUTPUT_HANDLE, hConOut);
	SetStdHandle(STD_ERROR_HANDLE, hConOut);
	SetStdHandle(STD_INPUT_HANDLE, hConIn);
	std::wcout.clear();
	std::wclog.clear();
	std::wcerr.clear();
	std::wcin.clear();
}
