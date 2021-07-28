Attribute VB_Name = "MainModule"
Option Explicit

Public Sub Main()

Dim blnCallLinker As Boolean
Dim intPos As Integer
Dim intPos2 As Integer
Dim strCmd As String
Dim strDLLfilepath As String
Dim strDEFfilepath As String
Dim oFS As New Scripting.FileSystemObject
Dim oFS2 As New Scripting.FileSystemObject
Dim ts As TextStream

strCmd = Command

Set ts = oFS.CreateTextFile(App.Path + "\proxy_linker_log.txt")

ts.WriteLine "Execution of proxy linker begun at " + CStr(Date) + " " + CStr(Time())
ts.WriteBlankLines 1
ts.WriteLine "Command line arguments to LINK call:"
ts.WriteBlankLines 1
ts.WriteLine strCmd
ts.WriteBlankLines 1

blnCallLinker = False

intPos = InStr(1, UCase(strCmd), "/DLL")
If intPos = 0 Then
    ts.WriteLine "Not creating a DLL.  No change to command line arguments."
    ts.WriteBlankLines 1
    blnCallLinker = True
Else
    intPos = InStr(1, UCase(strCmd), "/OUT")
    If intPos = 0 Then
        ts.WriteLine "No /OUT argument found."
        ts.WriteBlankLines 1
    Else
        intPos2 = InStr(intPos + 6, strCmd, Chr(34))
        If intPos2 = 0 Then
            ts.WriteLine "Cannot get DLL filepath."
            ts.WriteBlankLines 1
        Else
            strDLLfilepath = Mid(strCmd, intPos + 6, intPos2 - intPos - 6)
            ts.WriteLine "DLL filepath is " + strDLLfilepath
            strDEFfilepath = Left(strDLLfilepath, Len(strDLLfilepath) - 3) + "def"
            ts.WriteLine "DEF filepath is " + strDEFfilepath
            ts.WriteBlankLines 1

            'Check that DEF file exists.
            'Set oFS2 = CreateObject("Scripting.FileSystemObject")
            If Not oFS2.FileExists(strDEFfilepath) Then
                ts.WriteLine "DEF file " + strDEFfilepath + " not found."
                ts.WriteBlankLines 1
            Else
                'Add module definition before /DLL switch
                intPos = InStr(1, strCmd, "/DLL")
                strCmd = Left(strCmd, intPos - 1) _
                    + " /DEF:" + Chr(34) + strDEFfilepath + Chr(34) + " " _
                    + Mid(strCmd, intPos) + " /ENTRY:DllMain"
                ts.WriteLine "Command line arguments after modification:"
                ts.WriteBlankLines 1
                ts.WriteLine strCmd
                ts.WriteBlankLines 1
                blnCallLinker = True
            End If
        End If
    End If
End If

If blnCallLinker Then
    Shell "LINK-ORIG.EXE " + strCmd
    If Err.Number = 0 Then
        ts.WriteLine "Call to original linker was successful."
    Else
        ts.WriteLine "Call to original linker produced an error."
        Err.Clear
    End If
    ts.WriteBlankLines 1
End If

ts.WriteLine "Recommended: restore the original linker as LINK.EXE."
ts.Close
End Sub

