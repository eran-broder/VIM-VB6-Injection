Attribute VB_Name = "MainModule"
Option Explicit


Private Declare Function SysAllocString Lib "OleAut32.dll" (ByRef pOlechar As Any) As Long
Private Declare Function VimLog Lib "VimInProcessOrchestrator" (ByVal message As String) As Long
Private Declare Function VimInvokePendingAction Lib "VimInProcessOrchestrator" (ByVal MessageId As Long) As Long



Public Const DLL_PROCESS_DETACH = 0
Public Const DLL_PROCESS_ATTACH = 1
Public Const DLL_THREAD_ATTACH = 2
Public Const DLL_THREAD_DETACH = 3

Public Function DllMain(ByVal hInst As Long, ByVal fdwReason As Long, _
    ByVal lpvReserved As Long) As Boolean
   Select Case fdwReason
      Case DLL_PROCESS_DETACH
      Case DLL_PROCESS_ATTACH
         DllMain = True
      Case DLL_THREAD_ATTACH
      Case DLL_THREAD_DETACH
   End Select
End Function

Sub Log(ByVal msg As String)

    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Form1")
    If Not formToManipulate Is Nothing Then
        'CallByName formToManipulate, "Log", VbMethod, msg
    End If

    cout msg
    
End Sub

Function ExtractGridInfoStr(ByVal arg As String) As String
    Log "About to find"
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption(arg)
    If Not formToManipulate Is Nothing Then
        Dim grid As VSFlexGrid
        Set grid = FindControlByType(formToManipulate, "VSFlexGrid")
        Dim gridData As Dictionary
        Set gridData = GridSerializer.Serialize(grid)
        MsgBox CStr(gridData.Count)
        Dim jsonResult As String
        MsgBox "Preprepre"
                        
        jsonResult = JSON.toString(gridData)
        MsgBox "5"
        ExtractGridInfoStr = jsonResult
    Else
        Log "Cannot find the damm form!"
    End If
End Function

Function ExtractGridInfo(ByVal arg As String) As String
    'MsgBox "Called with : [" + arg + "]"
    Log "Called with " + arg
    Exit Function
    Log "About to find"
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption(arg)
    Log "After find"
    If Not formToManipulate Is Nothing Then
        Dim grid As VSFlexGrid
        Set grid = FindControlByType(formToManipulate, "VSFlexGrid")
        Dim cellValue As String
        cellValue = grid.TextMatrix(1, 2)
        Log "value of cell is : [" + cellValue + "]"
        ExtractGridInfo = cellValue
    Else
        Log "Cannot find the damm form!"
    End If
End Function


Sub TheRealShitAddAssessment()
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Referral (Outgoing)")
    If Not formToManipulate Is Nothing Then
        CallByName formToManipulate, "AddAseessment", VbMethod, &H639BA, False, "VIM Code", "Vim Name", "VIM Specify", "VIM Notes"
    Else
        Log "Cannot find the damm form!"
    End If
End Sub

Private Sub cout(ByVal msg As String)
    VimLog "[VB6] : " + msg
End Sub

