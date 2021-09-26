#include "ClrWrapper.h"
#include <iostream>
#include "utils.h"
#include <numeric>
#include <vector>
#define EXTRACT_CORECLR_PROC(name, handle) reinterpret_cast<name ## _ptr>(GetProcAddress(handle, #name))
#define FAIL() return std::nullopt;
#define FS_SEPARATOR "\\"
#define PATH_DELIMITER ";"


std::string BuildTpaList(const std::string & directory, const char* extension);
std::string BuildAppPaths(LPCSTR* app_directories, int app_directories_count);

ClrWrapper* InitClr(LPCSTR path_of_coreclr, LPCSTR* appDirectories, int appDirectoriesCount)
{	
	#define FAIL() return nullptr;
	
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
    //const auto tpa_list = BuildTpaList(base_path, ".dll");
    //const char* propertyKeys[] = { "TRUSTED_PLATFORM_ASSEMBLIES" };
    //const char* propertyValues[] = { tpa_list.c_str() };

    const char* property_keys[] = { "APP_PATHS"};
    const auto separated_path = BuildAppPaths(appDirectories, appDirectoriesCount);
    const char* property_values[] = { separated_path.c_str() };

    void* hostHandle;
    unsigned int domainId;

    // This function both starts the .NET Core runtime and creates
    // the default (and only) AppDomain
    int hr = initializeCoreClr(
        base_path.c_str(),        // App base path
        "SampleHost",       // AppDomain friendly name
        sizeof(property_keys) / sizeof(char*),   // Property count
        property_keys,       // Property names
        property_values,     // Property values
        &hostHandle,        // Host handle
        &domainId);         // AppDomain ID


    FAIL_IF(hr < 0, "coreclr_initialize failed");


    CoreClrHandles handles{hostHandle, domainId, initializeCoreClr, createManagedDelegate, shutdownCoreClr};
	
    handles.create_delegate = createManagedDelegate;
    handles.shutdown = shutdownCoreClr;
    handles.domain_id = domainId;
    handles.host_handle = hostHandle;
    return new ClrWrapper(handles);
}

ClrWrapper::ClrWrapper(CoreClrHandles handles): handles_(handles) {	
}

ClrWrapper::~ClrWrapper()
{
    std::cout << "XXXXXXXXXXXX Destroying clr wrapper XXXXXXXXXXX" << std::endl;
}

//TODO: who frees it? only creation appears here. leakage?
void** ClrWrapper::CreateDelegate(LPCSTR assemblyName, LPCSTR className, LPCSTR methodName) const
{
	#define FAIL() return nullptr;	//TODO: is this the right way to go?
	
    void** managedDelegate;
    
    // The assembly name passed in the third parameter is a managed assembly name
    // as described at https://docs.microsoft.com/dotnet/framework/app-domains/assembly-names
    auto hr = handles_.create_delegate(
        handles_.host_handle,
        handles_.domain_id,
        assemblyName,
        className,
        methodName,
        (void**)&managedDelegate);
    // </Snippet5>

    FAIL_IF(hr < 0, "failed to create delegate");

    return managedDelegate;
}

//TODO: no error handling! should there be any?
ManagedClassProxy ClrWrapper::GetClass(LPCSTR assemblyName, LPCSTR className) const
{
    return ManagedClassProxy(this->handles_, assemblyName, className);
}

bool  ClrWrapper::Shutdown() const
{
	const auto shutdownResult = this->handles_.shutdown(this->handles_.host_handle, this->handles_.domain_id);
    return S_OK == shutdownResult;
}

ManagedClassProxy::ManagedClassProxy(CoreClrHandles handles, LPCSTR assemblyName, LPCSTR className):
handles_(handles),
assembly_name(assemblyName),
class_name(className)
{
}

void* ManagedClassProxy::GetMethod(LPCSTR methodName) const
{
    std::cout << "GetMethod called for:" << methodName << std::endl;
    std::cout << "invoking form pointer:" << handles_.create_delegate << std::endl;
    
    void* delegatePointer;
    auto hr = handles_.create_delegate(
        handles_.host_handle,
        handles_.domain_id,
        this->assembly_name,
        this->class_name,
        methodName,
        &delegatePointer);
    FAIL_IF(hr < 0, "failed to create delegate");
    return delegatePointer;
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

std::string BuildAppPaths(LPCSTR* app_directories, const int app_directories_count)
{
    std::vector<std::string> directoriesAsVector(app_directories_count);
    //TODO: no for. use a proper initializer!
    for (auto i = 0; i < app_directories_count; i++) { directoriesAsVector.emplace_back(app_directories[i]); }

    std::string withSeparator = std::accumulate(std::begin(directoriesAsVector), std::end(directoriesAsVector), std::string(),
        [](std::string& ss, std::string& s)
        {
            return ss.empty() ? s : ss + PATH_DELIMITER + s;
        });

    return withSeparator;
}
