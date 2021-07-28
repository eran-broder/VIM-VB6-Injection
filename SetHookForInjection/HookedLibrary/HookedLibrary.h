// The following ifdef block is the standard way of creating macros which make exporting
// from a DLL simpler. All files within this DLL are compiled with the HOOKEDLIBRARY_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see
// HOOKEDLIBRARY_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef HOOKEDLIBRARY_EXPORTS
#define HOOKEDLIBRARY_API __declspec(dllexport)
#else
#define HOOKEDLIBRARY_API __declspec(dllimport)
#endif

// This class is exported from the dll
class HOOKEDLIBRARY_API CHookedLibrary {
public:
	CHookedLibrary(void);
	// TODO: add your methods here.
};

extern HOOKEDLIBRARY_API int nHookedLibrary;

HOOKEDLIBRARY_API int fnHookedLibrary(void);
