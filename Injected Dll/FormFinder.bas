Attribute VB_Name = "FormFinder"
Option Explicit

Function FindFormByCaption(ByVal caption As String) As form
    Dim frmCurr As VB.form
        
    For Each frmCurr In GlobalProvider.GetGlobal().Forms
        If frmCurr.caption = caption Then
            Set FindFormByCaption = frmCurr
            Exit Function
        End If
    Next frmCurr
    
End Function

Function FindControlByHandle(form As VB.form, ByVal handle As Long) As Control
    Dim currentControl As Control
    For Each currentControl In form.Controls
        If currentControl.hWnd = handle Then
            Set FindControlByHandle = currentControl
        End If
    Next currentControl

End Function


Function FindControlByType(form As VB.form, controlType As String) As Control
    Dim currentControl As Control
    For Each currentControl In form.Controls
        If TypeName(currentControl) = controlType Then
            Set FindControlByType = currentControl
        End If
    Next currentControl

End Function

