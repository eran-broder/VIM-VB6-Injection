Attribute VB_Name = "FormFinder"
Option Explicit

Function FindFormByCaption(ByVal caption As String) As form
    'TODO: arrrrrr. would not use globals but no args to constructor!
    Dim frmCurr As VB.form
    For Each frmCurr In GlobalProvider.GetGlobal().Forms
        If frmCurr.caption = caption Then
            Set FindFormByCaption = frmCurr
            Exit Function
        End If
    Next frmCurr
End Function

