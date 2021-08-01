VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ControlFilter"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit


Dim m_result() As control
Dim m_pred As IControlPred
Dim m_isEmpty As Boolean

Public Function GetAllControlsThatMatch(frm As Form, filter As IControlPred) As control()
    Set m_pred = filter
    m_isEmpty = True
    EnumControlsOfForm frm
        
    'TODO: clone it? destroy the state? possible leakage
    GetAllControlsThatMatch = m_result
End Function


Private Sub AddToResult(ByVal ctrl As control)
    If m_isEmpty Then
        ReDim m_result(0)
        m_isEmpty = False
    Else
        ReDim Preserve m_result(UBound(m_result) + 1)
    End If
    
    Set m_result(UBound(m_result)) = ctrl
End Sub

Private Sub EnumControlsOfForm(ByVal aform As Form)
    Dim c As control
    If CheckIfHasChildren(aform) Then
        For Each c In aform.Controls
            EnumControlsOfControl c
        Next
    End If
End Sub

Private Sub EnumControlsOfControl(ByVal ctl As control)
    If (m_pred.Execute(ctl)) Then
        AddToResult ctl
    End If
    
    Dim c As control
    If CheckIfHasChildren(ctl) Then
        For Each c In ctl.Controls
            EnumControlsOfControl (c)
        Next
    End If
End Sub

Private Function CheckIfHasChildren(ByVal control As Variant) As Boolean
On Error GoTo Handler
    If (control.Controls.Count > 0) Then
        CheckIfHasChildren = True
        Exit Function
    End If
Handler:
    CheckIfHasChildren = False
End Function
