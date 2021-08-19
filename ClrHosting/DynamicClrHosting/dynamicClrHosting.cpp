#include <Windows.h>
#include "dynamicClrHosting.h"
#include "ClrWrapper.h"

//TODO: why does this module even exist???


auto LoadClr(LPCSTR path_of_coreclr) -> ClrWrapper*
{
	auto opt_clr = InitClr(path_of_coreclr);
	return opt_clr;    
}
