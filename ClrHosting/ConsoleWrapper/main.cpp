#include <iostream>
#include <filesystem>
#include "dynamicClrHosting.h"

constexpr auto kPathToClr = R"(..\ManagedLibraryForInjection\bin\Release\net5.0\publish\coreclr.dll)";


auto main() -> int
{
	std::cout << "Hello world!" << std::endl;
	const auto a1 = std::filesystem::absolute(kPathToClr);
	const auto a2 = a1.string();
	const auto absolute_path = a2.c_str();
	LoadClr(absolute_path);
}
