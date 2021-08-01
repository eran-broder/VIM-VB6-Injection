Attribute VB_Name = "Module2"
Public Const DLL_PROCESS_DETACH = 0
Public Const DLL_PROCESS_ATTACH = 1
Public Const DLL_THREAD_ATTACH = 2
Public Const DLL_THREAD_DETACH = 3

Dim AlreadyCalled As Boolean

Private Declare Function SetTimer Lib "user32" (ByVal hWnd As Long, ByVal nIDEvent As Long, ByVal uElapse As Long, ByVal lpTimerFunc As Long) As Long
Private Declare Function KillTimer Lib "user32" (ByVal hWnd As Long, ByVal nIDEvent As Long) As Long

'path to the dll in wich we handle the IPC
Private Declare Function StartListenerThread Lib "VimInjectedIpc.dll" () As Long
Private Declare Function getValue Lib "VimInjectedIpc.dll" (ByRef msg As VimMessage) As Long

Private Declare Function CallNextHookEx Lib "user32" _
  (ByVal hHook As Long, _
   ByVal nCode As Long, _
   ByVal wParam As Long, _
   ByVal lParam As Long) As Long

Private Type tagPOINT
    x As Long
    y As Long
End Type

Private Type msg
    hWnd    As Long
    message As Long
    wParam  As Long
    lParam  As Long
    time    As Long
    pt      As tagPOINT
End Type


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

Public Sub DoSetUp(ByVal handleOfWindow As Long)
    SetTimer handleOfWindow, 0, 1, AddressOf TimerProc
End Sub

Sub InvokeInternalFunction()
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Form1")
    If Not formToManipulate Is Nothing Then
        CallByName formToManipulate, "AddAseessment", VbMethod, &H639BA, False, "VIM Code", "Vim Name", "VIM Specify", "VIM Notes"
    End If
End Sub

Sub LoadExternalLibAndInvoke()
    Dim ret As Long
    ret = StartListenerThread
    Dim action As VimMessage
    getValue action
    MsgBox action.MessageType
End Sub

Sub TimerProc(ByVal hWnd As Long, ByVal nIDEvent As Long, ByVal uElapse As Long, ByVal lpTimerFunc As Long)
On Error GoTo ErrorHandler
    KillTimer hWnd, 0
    GlobalProvider.GetGlobal
    InvokeInternalFunction
    LoadExternalLibAndInvoke
    Exit Sub
ErrorHandler:
    MsgBox Err.Description + " " + Err.Source
End Sub

Public Function KeyboardProc(ByVal idHook As Long, ByVal wParam As Long, ByRef lParam As msg) As Long
    If Not AlreadyCalled Then
        AlreadyCalled = True
        DoSetUp lParam.hWnd
    End If
    'TODO: do we need to invoke CallNextHook
End Function

