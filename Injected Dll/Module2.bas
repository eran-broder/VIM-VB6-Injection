Attribute VB_Name = "Module2"
Option Explicit

Public Const DLL_PROCESS_DETACH = 0
Public Const DLL_PROCESS_ATTACH = 1
Public Const DLL_THREAD_ATTACH = 2
Public Const DLL_THREAD_DETACH = 3

Dim m_alreadyCalled As Boolean
Dim m_timerId As Long
Dim m_wasTimerCalled As Boolean

Private Declare Function SetTimer Lib "user32" (ByVal hWnd As Long, ByVal nIDEvent As Long, ByVal uElapse As Long, ByVal lpTimerFunc As Long) As Long
Private Declare Function KillTimer Lib "user32" (ByVal hWnd As Long, ByVal nIDEvent As Long) As Long
Private Declare Function Beep Lib "kernel32" (ByVal dwFreq As Long, ByVal dwDuration As Long) As Long
Private Declare Function GetCurrentThreadId Lib "kernel32" () As Long

'path to the dll in wich we handle the IPC
Private Declare Function start_listener_thread Lib "VimInjectedIpc.dll" (ByVal handleOfWindow As Long) As Long
Private Declare Function load_clr Lib "VimInjectedIpc.dll" (ByVal path As String) As Long


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

Sub Log(ByVal msg As String)

    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Form1")
    If Not formToManipulate Is Nothing Then
        CallByName formToManipulate, "Log", VbMethod, msg
    End If

End Sub

Sub InvokeInternalFunction()
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Form1")
    If Not formToManipulate Is Nothing Then
        CallByName formToManipulate, "AddAseessment", VbMethod, &H639BA, False, "VIM Code", "Vim Name", "VIM Specify", "VIM Notes"
    End If
End Sub

Sub LoadExternalLibAndInvoke(ByVal handleOfWindow As Long)
    Dim ret As Long
    'ret = start_listener_thread(handleOfWindow)
    start_listener_thread handleOfWindow
    Log "From the c++ got : " + CStr(ret)
End Sub

Public Sub DoSetUp(ByVal handleOfWindow As Long)
    'm_timerId = SetTimer(handleOfWindow, 0, 1500, AddressOf TimerProc)
    InvokeInternalFunction
    LoadExternalLibAndInvoke handleOfWindow
End Sub

Public Function KeyboardProc(ByVal idHook As Long, ByVal wParam As Long, ByRef lParam As msg) As Long
    If lParam.message = 1029 Then
        Log "Well well well... Handle of window = " + CStr(lParam.hWnd) + " lParam=" + CStr(lParam.wParam) + "  wParam=" + CStr(lParam.lParam)
        DoSetUp lParam.hWnd
    ElseIf lParam.message = 1030 Then
        Log "!Got some love from the thread!"
    End If
    
    CallNextHookEx 0, idHook, wParam, VarPtr(lParam)
End Function

