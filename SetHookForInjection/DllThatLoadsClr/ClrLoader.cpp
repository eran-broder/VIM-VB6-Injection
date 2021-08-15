#include "pch.h"
#include <stdlib.h>
#include <string.h>
#include <string>
#include <combaseapi.h>
// https://github.com/dotnet/coreclr/blob/master/src/coreclr/hosts/inc/coreclrhost.h
#include "coreclrhost.h"
#include "framework.h"

#define MANAGED_ASSEMBLY "ManagedLibrary.dll"
#define WINDOWS TRUE

#define DBG(msg) MessageBoxA(nullptr, msg, "VIM", MB_OK)

// Define OS-specific items like the CoreCLR library's name and path elements
#include <Windows.h>
#define FS_SEPARATOR "\\"
#define PATH_DELIMITER ";"
#define CORECLR_FILE_NAME "coreclr.dll"

#define MANAGED_CODE_PATH "C:\\Users\\broder\\Documents\\GitHub\\VIM-VB6-Injection\\SetHookForInjection\\InjectedAppIpc\\bin\\Release\\net5.0\\publish\\InjectedAppIpc.exe"
//#define MANAGED_CODE_PATH "C:\\vim\\samples-main\\core\\hosting\\HostWithCoreClrHost\\bin\\windows\\SampleHost.exe"

// Function pointer types for the managed call and callback
typedef int (*report_callback_ptr)(int progress);
typedef char* (*doWork_ptr)(const char* jobName, int iterations, int dataSize, double* data, report_callback_ptr callbackFunction);
typedef int (*doWork2_ptr)(int windowHandle);
//typedef int (*doWork2_ptr)(const char* windowHandle);
//typedef int (*doWork2_ptr)();

void BuildTpaList(const char* directory, const char* extension, std::string& tpaList);
int ReportProgressCallback(int progress);
BOOL setup_thread(HWND hookedWindowHandle);
DWORD WINAPI ListenerThread(LPVOID lpParam);
extern "C" __declspec(dllexport) BOOL load_clr(char* pathOfExe);

extern "C" __declspec(dllexport) BOOL stop_listener_thread() {    
    return TRUE;
}

HWND _hookedHandle;
extern "C" __declspec(dllexport) BOOL start_listener_thread(HWND hookedWindowHandle) {
    //setup_thread(hookedWindowHandle);
    DBG("GOGO GO GO ");
    load_clr((char*)"bla");
    _hookedHandle = hookedWindowHandle;
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
    load_clr((char*)"bla");
    return 0;
}

//int main(int argc, char* argv[])
extern "C" __declspec(dllexport) BOOL load_clr(char* pathOfExe) 
{        
    // Get the current executable's directory
    // This sample assumes that both CoreCLR and the
    // managed assembly to be loaded are next to this host
    // so we need to get the current path in order to locate those.
    //char runtimePath[MAX_PATH];
    //GetFullPathNameA(pathOfExe, MAX_PATH, runtimePath, NULL);
	
    char runtimePath[MAX_PATH] = MANAGED_CODE_PATH;

    //char runtimePath[MAX_PATH] = "C:\\vim\\samples-main\\core\\hosting\\HostWithCoreClrHost\\bin\\windows\\ManagedLibrary.exe";    
    //char runtimePath[MAX_PATH] = "C:\\Users\\broder\\source\\repos\\InjectedAppIpc\\InjectedAppIpc\\bin\\Release\\net5.0\\publish\\InjectedAppIpc.exe";	
    //char runtimePath[MAX_PATH] = "C:\\Users\\broder\\Documents\\GitHub\\VIM-VB6-Injection\\Playground App\\Caller.exe";
    char* last_slash = strrchr(runtimePath, FS_SEPARATOR[0]);
    if (last_slash != NULL)
        *last_slash = 0;

    // Construct the CoreCLR path
    // For this sample, we know CoreCLR's path. For other hosts,
    // it may be necessary to probe for coreclr.dll/libcoreclr.so
    std::string coreClrPath(runtimePath);
    coreClrPath.append(FS_SEPARATOR);
    coreClrPath.append(CORECLR_FILE_NAME);

    // Construct the managed library path
    std::string managedLibraryPath(runtimePath);
    managedLibraryPath.append(FS_SEPARATOR);
    managedLibraryPath.append(MANAGED_ASSEMBLY);

    //
    // STEP 1: Load CoreCLR (coreclr.dll/libcoreclr.so)
    //
    // <Snippet1>    
    HMODULE coreClr = LoadLibraryExA(coreClrPath.c_str(), NULL, 0);
    if (coreClr == NULL)
    {
        DBG("Failed to load CoreCLR ");        
        printf("ERROR: Failed to load CoreCLR from %s\n", coreClrPath.c_str());
        return -1;
    }
    else
    {
        printf("Loaded CoreCLR from %s\n", coreClrPath.c_str());
    }


    coreclr_initialize_ptr initializeCoreClr = (coreclr_initialize_ptr)GetProcAddress(coreClr, "coreclr_initialize");
    coreclr_create_delegate_ptr createManagedDelegate = (coreclr_create_delegate_ptr)GetProcAddress(coreClr, "coreclr_create_delegate");
    coreclr_shutdown_ptr shutdownCoreClr = (coreclr_shutdown_ptr)GetProcAddress(coreClr, "coreclr_shutdown");

    if (initializeCoreClr == NULL)
    {
        MessageBoxA(nullptr, "coreclr_initialize not found ", "bla", MB_OK);
        printf("coreclr_initialize not found");
        return -1;
    }
    if (createManagedDelegate == NULL)
    {
        MessageBoxA(nullptr, "coreclr_create_delegate not found", "bla", MB_OK);
        printf("coreclr_create_delegate not found");
        return -1;
    }

    if (shutdownCoreClr == NULL)
    {
        MessageBoxA(nullptr, "coreclr_shutdown not found", "bla", MB_OK);
        printf("coreclr_shutdown not found");
        return -1;
    }

    //
    // STEP 3: Construct properties used when starting the runtime
    //

    // Construct the trusted platform assemblies (TPA) list
    // This is the list of assemblies that .NET Core can load as
    // trusted system assemblies.
    // For this host (as with most), assemblies next to CoreCLR will
    // be included in the TPA list
    std::string tpaList;
    BuildTpaList(runtimePath, ".dll", tpaList);

    // <Snippet3>
    // Define CoreCLR properties
    // Other properties related to assembly loading are common here,
    // but for this simple sample, TRUSTED_PLATFORM_ASSEMBLIES is all
    // that is needed. Check hosting documentation for other common properties.
    const char* propertyKeys[] = {
        "TRUSTED_PLATFORM_ASSEMBLIES"      // Trusted assemblies
    };

    const char* propertyValues[] = {
        tpaList.c_str()
    };
    // </Snippet3>

    //
    // STEP 4: Start the CoreCLR runtime
    //

    // <Snippet4>
    void* hostHandle;
    unsigned int domainId;

    // This function both starts the .NET Core runtime and creates
    // the default (and only) AppDomain
    int hr = initializeCoreClr(
        runtimePath,        // App base path
        "SampleHost",       // AppDomain friendly name
        sizeof(propertyKeys) / sizeof(char*),   // Property count
        propertyKeys,       // Property names
        propertyValues,     // Property values
        &hostHandle,        // Host handle
        &domainId);         // AppDomain ID
// </Snippet4>

    if (hr >= 0)
    {
        printf("CoreCLR started\n");
    }
    else
    {
        printf("coreclr_initialize failed - status: 0x%08x\n", hr);
        MessageBoxA(nullptr, "coreclr_initialize failed", "bla", MB_OK);
        return -1;
    }


	//---------------------------------------------------------------

	
    doWork2_ptr managedDelegate2;

    // The assembly name passed in the third parameter is a managed assembly name
    // as described at https://docs.microsoft.com/dotnet/framework/app-domains/assembly-names
    hr = createManagedDelegate(
        hostHandle,
        domainId,
        "ManagedLibrary, Version=1.0.0.0",
        "ManagedLibrary.ManagedWorker",
        "DoWork2",
        (void**)&managedDelegate2);
    // </Snippet5>

    if (hr >= 0)
    {
        printf("Managed delegate created\n");
    }
    else
    {
        DBG("createManagedDelegate failed");
        return -1;
    }

    // Create sample data for the double[] argument of the managed method to be called

    // Invoke the managed delegate and write the returned string to the console
    MessageBoxA(nullptr, "about to call", "hello!", MB_OK);
    int x = managedDelegate2(111);
    std::string ret2 = "Managed code returned: " + std::to_string(x);
    MessageBoxA(nullptr, ret2.c_str(), "hello!", MB_OK);         

    // Strings returned to native code must be freed by the native code
    //CoTaskMemFree(ret);
    
    hr = shutdownCoreClr(hostHandle, domainId);   
    if (hr >= 0)
    {
        printf("CoreCLR successfully shutdown\n");
    }
    else
    {
        MessageBoxA(nullptr, "coreclr_shutdown failed", "bla", MB_OK);
    }

    return 0;
}

// Win32 directory search for .dll files
// <Snippet7>
void BuildTpaList(const char* directory, const char* extension, std::string& tpaList)
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

    WIN32_FIND_DATAA findData;
    HANDLE fileHandle = FindFirstFileA(searchPath.c_str(), &findData);

    if (fileHandle != INVALID_HANDLE_VALUE)
    {
        do
        {
            // Append the assembly to the list
            tpaList.append(directory);
            tpaList.append(FS_SEPARATOR);
            tpaList.append(findData.cFileName);
            tpaList.append(PATH_DELIMITER);

            // Note that the CLR does not guarantee which assembly will be loaded if an assembly
            // is in the TPA list multiple times (perhaps from different paths or perhaps with different NI/NI.dll
            // extensions. Therefore, a real host should probably add items to the list in priority order and only
            // add a file if it's not already present on the list.
            //
            // For this simple sample, though, and because we're only loading TPA assemblies from a single path,
            // and have no native images, we can ignore that complication.
        } while (FindNextFileA(fileHandle, &findData));
        FindClose(fileHandle);
    }
}

// Callback function passed to managed code to facilitate calling back into native code with status
int ReportProgressCallback(int progress)
{
    // Just print the progress parameter to the console and return -progress
    printf("Received status from managed code: %d\n", progress);
    return -progress;
}
