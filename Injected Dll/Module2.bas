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

Private Declare Function VimInvokeAgain Lib "VimInProcessOrchestrator" (ByVal messageCode As Long) As Long
Private Declare Function VimLog Lib "VimInProcessOrchestrator" (ByVal message As String) As Long
Private Declare Function VimStart Lib "VimInProcessOrchestrator" (ByVal handle As Long) As Long
Private Declare Function VimInvokePendingAction Lib "VimInProcessOrchestrator" (ByVal MessageId As Long) As Long




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

    cout msg
    
End Sub

Sub InvokeInternalFunction()
    Dim formToManipulate As form
    Set formToManipulate = FormFinder.FindFormByCaption("Form1")
    If Not formToManipulate Is Nothing Then
        CallByName formToManipulate, "AddAseessment", VbMethod, &H639BA, False, "VIM Code", "Vim Name", "VIM Specify", "VIM Notes"
    End If
End Sub

Public Function CallMeFromFar(ByVal arg As Long) As Long
    CallMeFromFar = arg * 2
End Function

Public Function SetReferral() As Long
    Log "Set Referral called"
    SetReferral = 333
End Function

Public Function GetGrid() As Long
    Log "Get Grid called"
    GetGrid = 19
End Function

Private Sub cout(ByVal msg As String)
    VimLog "[VB6] : " + msg
End Sub

Public Function KeyboardProc(ByVal idHook As Long, ByVal wParam As Long, ByRef lParam As msg) As Long
    If lParam.message = 1029 Then
        Log "got ya!"
        Dim ret As Long
        Log "Window handle is : " + CStr(lParam.hWnd)
        ret = VimStart(lParam.hWnd)
    ElseIf lParam.message = 1031 Then
        Log "Got message ~~~~ 1031"
        cout "try and invoke"
        VimInvokePendingAction (lParam.wParam)
    End If
    
    CallNextHookEx 0, idHook, wParam, VarPtr(lParam)
End Function

