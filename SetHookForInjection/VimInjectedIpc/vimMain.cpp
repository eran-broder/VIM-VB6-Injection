#include "pch.h"
#include <Windows.h> // WinApi header 
#include <chrono>
#include <thread>

using namespace std::chrono_literals;

BOOL setup_thread(HWND hookedWindowHandle);
DWORD WINAPI ListenerThread(LPVOID lpParam);

BOOL g_should_keep_running = TRUE;

//TODO: can I return a callback function?
extern "C" __declspec(dllexport) BOOL start_listener_thread(HWND hookedWindowHandle) {
	return setup_thread(hookedWindowHandle);	
}

//TODO: this should block.
//TODO: the start should return some cancellation token. 
extern "C" __declspec(dllexport) BOOL stop_listener_thread() {
	g_should_keep_running = FALSE;
	return TRUE;
}

BOOL setup_thread(HWND hookedWindowHandle) {
	DWORD threadID = 0;
	const auto g_hThread = CreateThread(nullptr, 0, ListenerThread, hookedWindowHandle, 0, &threadID);
	if (g_hThread == nullptr) {
		//TODO: Log it
		return FALSE;
	}
	return TRUE;
}

DWORD WINAPI ListenerThread(LPVOID lpParam) {
	const auto hooked_window_handle = static_cast<HWND>(lpParam);
	
	while (g_should_keep_running)
	{		
		Beep(800, 100);
		std::this_thread::sleep_for(2000ms);
		PostMessage(hooked_window_handle, 1030, 555, 666);
	}
	return 0;
}