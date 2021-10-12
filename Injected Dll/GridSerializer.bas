Attribute VB_Name = "GridSerializer"
Option Explicit

Public Function Serialize(grid As VSFlexGrid) As Dictionary
    Dim dict As Dictionary
    Set dict = New Dictionary
    
    Dim row As Integer
    Dim col As Integer
    For row = 0 To grid.rows - 1
        For col = 0 To grid.Cols - 1
            Dim value As String
            value = grid.TextMatrix(row, col)
            Dim key As String
            key = CStr(row) + "," + CStr(col)
            dict.Add key, value
        Next col
    Next row
    
    Set Serialize = dict
    
End Function

