#include "pch.h"
#include <windows.h> // WinApi header 

BOOL setup_thread();
DWORD WINAPI ListenerThread(LPVOID lpParam);

struct VimMessage {
	int MessageId;
	char MessageType[255];
	char Arg1[255];
	char Arg2[255];
	char Arg3[255];
	char Arg4[255];
	char Arg5[255];
};

extern "C" __declspec(dllexport) int StartListenerThread() {
	setup_thread();
	return 333;
}

extern "C" __declspec(dllexport) int getValue(VimMessage* result) {
	strcpy_s(result->MessageType, "InvokeOnForm");
	strcpy_s(result->Arg1, "[HandleOfForm]");
	strcpy_s(result->Arg2, "[NameOfMethod]");
	strcpy_s(result->Arg3, "[Arg1]");
	strcpy_s(result->Arg4, "[Arg2]");
	strcpy_s(result->Arg5, "[Arg3]");
	return 0;
}

BOOL setup_thread() {
	DWORD threadID = 0;

	auto g_hThread = CreateThread(nullptr, 0, ListenerThread, nullptr, 0, &threadID);
	if (g_hThread == nullptr) {
		//TODO: Log it
		return FALSE;
	}
	return TRUE;
}

DWORD WINAPI ListenerThread(LPVOID lpParam) {
	while (true)
	{
		Beep(800, 100);
		Sleep(2000);
	}
}