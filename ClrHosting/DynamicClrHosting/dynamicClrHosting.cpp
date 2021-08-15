#include <iostream>
#include <Windows.h>
#include "dynamicClrHosting.h"

#include <string>

#include "utils.h"
#include "Externals/coreclrhost.h"

#define EXTRACT_CORECLR_PROC(name, handle) reinterpret_cast<name ## _ptr>(GetProcAddress(handle, #name))
//TODO: can you access via system .h file?
#define FS_SEPARATOR "\\"
#define PATH_DELIMITER ";"

typedef int (WINAPI *doWorkSlim_ptr)();
typedef int (WINAPI* doWork_ptr)(int x);

std::string BuildTpaList(const std::string& directory, const char* extension);

auto LoadClr(LPCSTR path_of_coreclr) -> BOOL
{
	std::cout << "loading clr from" << path_of_coreclr << std::endl;

	const HMODULE coreclr_module_handle = LoadLibraryExA(path_of_coreclr, nullptr, 0);
	
	FAIL_IF_NULL_MSG(coreclr_module_handle, "failed loading library")	
	DBG("OK! library loaded!");

	const auto initializeCoreClr = EXTRACT_CORECLR_PROC(coreclr_initialize, coreclr_module_handle);
	const auto createManagedDelegate = EXTRACT_CORECLR_PROC(coreclr_create_delegate, coreclr_module_handle);
	const auto shutdownCoreClr = EXTRACT_CORECLR_PROC(coreclr_shutdown, coreclr_module_handle);
		
    FAIL_IF_NULL(initializeCoreClr)
        FAIL_IF_NULL(createManagedDelegate)
        FAIL_IF_NULL(shutdownCoreClr)

	const auto base_path = GetFileDirectory(path_of_coreclr);
	const auto tpa_list = BuildTpaList(base_path, ".dll");
    const char* propertyKeys[] = {"TRUSTED_PLATFORM_ASSEMBLIES"};
    const char* propertyValues[] = {tpa_list.c_str()};

    void* hostHandle;
    unsigned int domainId;

    // This function both starts the .NET Core runtime and creates
    // the default (and only) AppDomain
    int hr = initializeCoreClr(
        base_path.c_str(),        // App base path
        "SampleHost",       // AppDomain friendly name
        sizeof(propertyKeys) / sizeof(char*),   // Property count
        propertyKeys,       // Property names
        propertyValues,     // Property values
        &hostHandle,        // Host handle
        &domainId);         // AppDomain ID
	

    FAIL_IF(hr < 0, "coreclr_initialize failed");


    doWork_ptr managedDelegate;

    // The assembly name passed in the third parameter is a managed assembly name
    // as described at https://docs.microsoft.com/dotnet/framework/app-domains/assembly-names
    hr = createManagedDelegate(
        hostHandle,
        domainId,
        "ManagedLibraryForInjection, Version=1.0.0.0",
        "ManagedLibraryForInjection.Program",
        "DoWork",
        (void**)&managedDelegate);
    // </Snippet5>

    FAIL_IF(hr < 0, "failed to create delegate");

    // Create sample data for the double[] argument of the managed method to be called    
    int x = managedDelegate(444);
    std::string ret2 = "Managed code returned: " + std::to_string(x);
    std::cout << ret2;
	return TRUE;		
}


std::string BuildTpaList(const std::string& directory, const char* extension)
{
    // This will add all files with a .dll extension to the TPA list.
    // This will include unmanaged assemblies (coreclr.dll, for example) that don't
    // belong on the TPA list. In a real host, only managed assemblies that the host
    // expects to load should be included. Having extra unmanaged assemblies doesn't
    // cause anything to fail, though, so this function just enumerates all dll's in
    // order to keep this sample concise.
    std::string searchPath(directory);
    searchPath.append(FS_SEPARATOR);
    searchPath.append("*");
    searchPath.append(extension);

    WIN32_FIND_DATAA find_data;
    const HANDLE file_handle = FindFirstFileA(searchPath.c_str(), &find_data);

    std::string tpa_list;

    if (file_handle != INVALID_HANDLE_VALUE)
    {
	    do
        {
            // Append the assembly to the list
            tpa_list.append(directory);
            tpa_list.append(FS_SEPARATOR);
            tpa_list.append(find_data.cFileName);
            tpa_list.append(PATH_DELIMITER);

            // Note that the CLR does not guarantee which assembly will be loaded if an assembly
            // is in the TPA list multiple times (perhaps from different paths or perhaps with different NI/NI.dll
            // extensions. Therefore, a real host should probably add items to the list in priority order and only
            // add a file if it's not already present on the list.
            //
            // For this simple sample, though, and because we're only loading TPA assemblies from a single path,
            // and have no native images, we can ignore that complication.
        } while (FindNextFileA(file_handle, &find_data));
        FindClose(file_handle);
    }

    return tpa_list;
}