VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "DummyClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Public Static Function Foo(arg As Long) As Long
    Foo = arg * 2
End Function

Public Sub Goo()
    
End Sub

Public Static Sub Xoo()

End Sub

Public Function Boo() As Long

    Boo = 222

End Function

Public Function Access() As Long
    Dim res As String
    res = ExtractGridInfoStr("Form1")
    MsgBox res
End Function
