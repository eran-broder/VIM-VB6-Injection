VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ControlNamePred"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Implements IControlPred

Private m_ControlName As String

Public Property Get ControlName() As String
    ControlName = m_ControlName
End Property

Public Property Let ControlName(ByVal vNewValue As String)
    m_ControlName = vNewValue
End Property

Public Function IControlPred_Execute(ctrl As control) As Boolean
    IControlPred_Execute = False
    If (ctrl.Name = Me.ControlName) Then
        IControlPred_Execute = True
    End If
End Function
