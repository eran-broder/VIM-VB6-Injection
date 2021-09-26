#include <Windows.h>
#include "dynamicClrHosting.h"
#include "ClrWrapper.h"

//TODO: why does this module even exist???
auto LoadClr(LPCSTR path_of_coreclr) -> ClrWrapper*
{
	const char* directories[] = {
		R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\bin\Debug\net5.0)" ,
		R"(C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedAssemblyRunner\bin\Debug\net5.0)" };
	const auto number_of_directories = sizeof(directories) / sizeof(*directories);
	const auto clr = InitClr(path_of_coreclr, directories, number_of_directories);
	return clr; 
}
