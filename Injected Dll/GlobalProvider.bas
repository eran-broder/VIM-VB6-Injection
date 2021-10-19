Attribute VB_Name = "GlobalProvider"
Public Globalz As VB.Global

Private Declare Function GetModuleHandle Lib "kernel32" Alias "GetModuleHandleA" (ByVal lpModuleName As String) As Long
Private Declare Sub GetMem4 Lib "msvbvm60.dll" (ByVal lAddress As Long, var As Long)
Private Declare Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" (lpDest As Any, lpSource As Any, ByVal cBytes&)

Private Type extEntry
    flag As Long
    offset As Long
End Type

Private Type extEntryTarget
    clsidIIDStructOffset As Long
    lpValue As Long
End Type

Public Sub SetGlobal(arg As IUnknown)
    Set Globalz = arg
End Sub

Public Function GetGlobal() As VB.Global
    If Globalz Is Nothing Then
        
        Set Globalz = InternalGet()
    End If
    Set GetGlobal = Globalz
End Function

Function InternalGet() As VB.Global
    Dim vbHeader As Long, lpProjectData As Long, projectDataPointerValue As Long
    Dim lpExternalTable As Long, externalTablePointerValue As Long
    Dim externalCount As Long, lpExternalCount As Long
    Dim i As Integer, e As extEntry, myCaption As String
    Dim globalPtr As Long, lpGlobalPtr As Long, globalClone As VB.Global
      
    vbHeader = GetVBHeader()
    
    lpProjectData = vbHeader + &H30
    GetMem4 ByVal lpProjectData, projectDataPointerValue

    lpExternalCount = projectDataPointerValue + &H238
    
    GetMem4 ByVal lpExternalCount, externalCount
    
    lpExternalTable = projectDataPointerValue + &H234
    
    GetMem4 ByVal lpExternalTable, externalTablePointerValue
    
    For i = 0 To externalCount
        CopyMemory ByVal VarPtr(e), ByVal externalTablePointerValue, 8
        
        If e.flag = 6 Then 'Tag of global object
            GetMem4 ByVal e.offset + 4, lpGlobalPtr
            GetMem4 ByVal lpGlobalPtr, globalPtr
            Set InternalGet = GlobalFromPointer(ByVal globalPtr)
        End If
        
        externalTablePointerValue = externalTablePointerValue + 8
    Next
End Function


' // Get VBHeader structure
Private Function GetVBHeader() As Long
    Dim ptr     As Long
    Dim hModule As Long
    Dim hModuleFromNull As Long
            
    hModule = GetModuleHandle(vbNullString)
        
    ' Get e_lfanew
    GetMem4 ByVal hModule + &H3C, ptr
    ' Get AddressOfEntryPoint
    GetMem4 ByVal ptr + &H28 + hModule, ptr
    ' Get VBHeader
    GetMem4 ByVal ptr + hModule + 1, GetVBHeader
    
End Function

Private Function GlobalFromPointer(ByVal ptr&) As VB.Global
  ' this function returns a reference to our form class
  
  ' dimension an object variable that we can copy the
  ' passed pointer into
  Dim globalClone As VB.Global
    
  ' use the CopyMemory API function to copy the
  ' long pointer into the object variable.
  CopyMemory globalClone, ptr, 4&
  Set GlobalFromPointer = globalClone
  CopyMemory ptr&, 0&, 4&
  
End Function
