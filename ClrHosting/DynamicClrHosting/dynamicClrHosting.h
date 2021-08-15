#pragma once
#include <Windows.h>

extern "C" __declspec(dllexport) auto LoadClr(LPCSTR path_of_coreclr) -> BOOL;

