#pragma once

//TODO: overload?
#define DBG(msg) std::cout << (msg) << std::endl
#define DBG_(msg) std::cout << (msg)
//#define FAIL() return FALSE;
#define FAIL_MSG(msg) DBG(msg);FAIL();
#define FAIL_IF_NULL(arg) if(nullptr == (arg)){DBG_((#arg));DBG(" is null");FAIL()}
#define FAIL_IF_NULL_MSG(arg, msg) if(nullptr == (arg)){FAIL_MSG(msg)}
#define FAIL_IF(expr, msg) if((expr)){FAIL_MSG(msg)}

//TODO: buggy
inline std::string GetFileDirectory(std::string path)
{	
	return path.substr(0, path.find_last_of(R"(\)"));
}