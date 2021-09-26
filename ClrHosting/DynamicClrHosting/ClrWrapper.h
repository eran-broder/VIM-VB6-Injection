#pragma once
#include <optional>
#include <Windows.h> //TODO: really needed?
#include "Externals/coreclrhost.h"

//TODO: should we wrap with a namespace?

class ClrWrapper;
class ManagedClassProxy;
__declspec(dllexport) ClrWrapper* InitClr(LPCSTR path_of_coreclr, LPCSTR* appDirectories, int appDirectoriesCount);

struct CoreClrHandles
{
	void* host_handle;
	unsigned int domain_id;
	coreclr_initialize_ptr initialize; //TODO: DRY. use a macro
	coreclr_create_delegate_ptr create_delegate; //TODO: DRY. use a macro
	coreclr_shutdown_ptr shutdown; //TODO: DRY. use a macro
};

class ClrWrapper
{
public:
	explicit ClrWrapper(CoreClrHandles handles); //TODO: should not be public
	~ClrWrapper();

	//TODO: perhaps not use it as a dll and leverage templating?
	void** CreateDelegate(LPCSTR assemblyName, LPCSTR className, LPCSTR methodName) const;

	ManagedClassProxy GetClass(LPCSTR assemblyName, LPCSTR className) const;
	
	bool Shutdown() const;

private:	
	CoreClrHandles handles_;
};


class __declspec(dllexport) ManagedClassProxy
{
public:
	ManagedClassProxy() = default; //TODO: this is bad. why do you even need a global? use pointers instead
	ManagedClassProxy(CoreClrHandles handles, LPCSTR assemblyName, LPCSTR className);
	void* GetMethod(LPCSTR methodName) const;

private:
	CoreClrHandles handles_;
	LPCSTR assembly_name;
	LPCSTR class_name;
};