#pragma warning(disable:4996)

#include <filesystem>
#include <iostream>
#include <string>
#include <thread>
#include <Windows.h>
#include "dynamicClrHosting.h"
#include "Utils.h" //TODO: bad include. should be in a shared project


typedef int (WINAPI* doWork_ptr)(HWND x);

struct DummyStruct
{
	HWND window_to_be_hooked;
	char path_to_clr[256];
	char assembly_name[256];
	char class_name[256];
	char method_name[256];
};


//TODO: disgusting code. so many globals. give it some love
void ListenerThreadStd(DummyStruct userData);// (DummyStruct userData);
BOOL setup_thread(std::string);
void CreateConsole();
std::string GetCoreClrPath();
ManagedClassProxy g_cls;
ClrWrapper* g_clr; //TODO: use unique pointer
doWork_ptr GetMethod(LPCSTR methodName);

constexpr auto kPathToClr = R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\bin\Debug\net5.0\coreclr.dll)";

HWND g_handle;

void foo()
{
	
}
extern "C" __declspec(dllexport)  LONG VimStart(DummyStruct userData)
{
	//AllocateConsole();
	CreateConsole();
	g_handle = userData.window_to_be_hooked; //TODO: fuck the global var
	std::thread t{ ListenerThreadStd, userData };
	t.detach(); //really?
	std::cout << "after start thread" << std::endl;
	return 0;
}

extern "C" __declspec(dllexport)  LONG VimStart2(DummyStruct * data)
{
	return VimStart(*data);
}

extern "C" __declspec(dllexport)  int VimTestInject(int* arg)
{
	const int argValue = *arg;
	Beep(1900, 500);
	return argValue;
}

//TOO: Add a macro for "vim" exported functions. DRY
extern "C" __declspec(dllexport) LONG VimInvokePendingAction(LONG arg)
{
	std::cout << "Lets invoke pending message :  " << arg << std::endl;
	const auto managed_delegate = GetMethod("InvokePendingMessage");
	return managed_delegate(reinterpret_cast<HWND>(arg));
}

extern "C" __declspec(dllexport) void VimUnload()
{
	const auto managed_delegate = GetMethod("Shutdown");
	managed_delegate(0);
	std::cout << "Back from managed shutdown. now shutdown clr itself" << std::endl;	
	if (!g_clr->Shutdown())
		std::cout << "ERROR shutting down" << std::endl; //TODO: facilitate logging
	else
		std::cout << "CLR is shut down" << std::endl; //TODO: facilitate logging
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

std::string g_path; //TODO: abolish this crap

//TODO: how do we handle shit that happens here?
void ListenerThreadStd(DummyStruct data)
{
	#define FAIL() return;
	g_clr = LoadClr(data.path_to_clr);
	FAIL_IF_NULL_MSG(g_clr, "error loading clr")
	std::cout << "clr loaded" << std::endl;

	//TODO: this too should be a parameter
	g_cls = g_clr->GetClass(data.assembly_name, data.class_name);

	const auto managed_delegate = GetMethod(data.method_name);
	managed_delegate(g_handle);
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

doWork_ptr GetMethod(LPCSTR methodName)
{
	return static_cast<doWork_ptr>(g_cls.GetMethod(methodName));
}