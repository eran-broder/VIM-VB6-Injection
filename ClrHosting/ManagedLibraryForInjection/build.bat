SET OUTDIR=%~dp0bin\Release\net5.0\publish

REM mkdir %OUTDIR%

REM Build managed component
echo %OUTDIR%
dotnet publish --self-contained -r win10-x86 ManagedLibraryForInjection.csproj -o %OUTDIR%