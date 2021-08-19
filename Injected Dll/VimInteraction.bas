Attribute VB_Name = "VimInteraction"
Option Explicit

Public Declare Function VimStart Lib "VimInProcessOrchestrator.dll" (ByVal windowHandle As Long) As Long
Public Declare Sub VimLog Lib "VimInProcessOrchestrator.dll" (ByVal message As String)
Public Declare Function VimInvokeAgain Lib "VimInProcessOrchestrator.dll" (ByVal value As Long) As Long
