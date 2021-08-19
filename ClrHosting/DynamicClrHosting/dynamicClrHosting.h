#pragma once
#include <optional>
#include <Windows.h>
#include "ClrWrapper.h"

extern "C" __declspec(dllexport) auto LoadClr(LPCSTR path_of_coreclr) -> ClrWrapper*;


