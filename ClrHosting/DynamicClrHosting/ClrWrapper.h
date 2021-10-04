#pragma once
#include <optional>
#include <Windows.h> 
#include "Externals/coreclrhost.h"

class ClrWrapper;
class ManagedClassProxy;

__declspec(dllexport) ClrWrapper* InitClr(LPCSTR path_of_coreclr, LPCSTR* appDirectories, int appDirectoriesCount);


struct CoreClrHandles
{
	void* host_handle;
	unsigned int domain_id;
	coreclr_initialize_ptr initialize;
	coreclr_create_delegate_ptr create_delegate;
	coreclr_shutdown_ptr shutdown;
};

class ClrWrapper
{
public:
	friend ClrWrapper* InitClr(LPCSTR path_of_coreclr, LPCSTR* appDirectories, int appDirectoriesCount);
	ClrWrapper(ClrWrapper& _) = delete;
	ClrWrapper(ClrWrapper&& _) = delete;
	
	void** CreateDelegate(LPCSTR assemblyName, LPCSTR className, LPCSTR methodName) const;
	
	ManagedClassProxy GetClass(LPCSTR assemblyName, LPCSTR className) const;
	
	bool Shutdown() const;

private:	
	CoreClrHandles handles_;
	ClrWrapper(CoreClrHandles handles); //TODO: should not be public
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