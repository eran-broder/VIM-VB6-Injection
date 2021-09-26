#pragma warning(disable:4996)

#include <filesystem>
#include <iostream>
#include <string>
#include <thread>
#include <Windows.h>
#include "dynamicClrHosting.h"
#include "Utils.h" //TODO: bad include. should be in a shared project


typedef int (WINAPI* doWork_ptr)(HWND x);

struct UserData
{
	char path_to_clr[256];
	char assembly_name[256];
	char class_name[256];
	char method_name[256];	
	byte more[2056];//TODO: this is only used to avoid stack slicing. think it over!	
};


//TODO: disgusting code. so many globals. give it some love
void LoadAndInvokeClr(UserData userData);// (DummyStruct userData);
BOOL setup_thread(std::string);
void CreateConsole();
ManagedClassProxy g_cls;
std::unique_ptr<ClrWrapper> g_clr; //TODO: use unique pointer
doWork_ptr GetMethod(LPCSTR methodName);
doWork_ptr invoke_pending_message;

extern "C" __declspec(dllexport)  LONG VimStart(UserData userData)
{
	CreateConsole();	
	std::thread t{ LoadAndInvokeClr, userData };
	t.detach(); 
	std::cout << "after start thread" << std::endl;
	return 0;
}

//TODO: decide who cleans up the data. me? the caller?
extern "C" __declspec(dllexport)  LONG VimStart2(UserData * data)
{
	return VimStart(*data);
}




extern "C" __declspec(dllexport) LONG InvokePendingAction(LONG arg)
{
	std::cout << "cpp was asked to invoke pending message :  " << arg << std::endl;
	//auto cls = g_clr->GetClass("ManagedLibraryForInjection, Version=1.0.0.0","ManagedLibraryForInjection.Program");
	//const auto method_raw_pointer = cls.GetMethod("InvokePendingMessage");
	//const auto method_pointer = static_cast<doWork_ptr>(method_raw_pointer);	
	std::cout << "cpp calling c#:  " << arg << std::endl;
	return invoke_pending_message(reinterpret_cast<HWND>(arg));
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

//TODO: rethink the signature
extern "C" __declspec(dllexport) LRESULT GetMsgProc(
	int    code,
	WPARAM wParam,
	LPARAM lParam)
{
	PMSG asMsg = reinterpret_cast<PMSG>(lParam);
	if(asMsg->message == 1031)
	{
		auto msgId = asMsg->wParam;
		std::cout << "Invoking pending message number[" << msgId << "]" << std::endl;
		InvokePendingAction(msgId);
	}
	
	return CallNextHookEx(nullptr, code, wParam, lParam);
}


std::string g_path; //TODO: abolish this crap

//TODO: how do we handle shit that happens here?
void LoadAndInvokeClr(UserData data)
{
	#define FAIL() return;
	g_clr.reset(LoadClr(data.path_to_clr));
	FAIL_IF_NULL_MSG(g_clr, "error loading clr")	
	std::cout << "clr loaded" << std::endl;

	g_cls = g_clr->GetClass(data.assembly_name, data.class_name);

	invoke_pending_message = GetMethod("InvokePendingMessage");
	
	const auto managed_delegate = GetMethod(data.method_name);

	managed_delegate(reinterpret_cast<HWND>(&data.more));
	//managed_delegate(data.window_to_be_hooked);
}


void CreateConsole()
{
	if (!AllocConsole()) {
		// TODO: Add some error handling here.
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
