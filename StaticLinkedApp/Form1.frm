VERSION 5.00
Object = "{D76D7128-4A96-11D3-BD95-D296DC2DD072}#1.0#0"; "Vsflex7.ocx"
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   6975
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   10245
   LinkTopic       =   "Form1"
   ScaleHeight     =   6975
   ScaleWidth      =   10245
   StartUpPosition =   3  'Windows Default
   Begin VSFlex7Ctl.VSFlexGrid fg 
      Height          =   2295
      Left            =   2760
      TabIndex        =   4
      Top             =   3600
      Width           =   6135
      _cx             =   10821
      _cy             =   4048
      _ConvInfo       =   -1
      Appearance      =   1
      BorderStyle     =   1
      Enabled         =   -1  'True
      BeginProperty Font {0BE35203-8F91-11CE-9DE3-00AA004BB851} 
         Name            =   "MS Sans Serif"
         Size            =   8.25
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      MousePointer    =   0
      BackColor       =   -2147483643
      ForeColor       =   -2147483640
      BackColorFixed  =   -2147483633
      ForeColorFixed  =   -2147483630
      BackColorSel    =   -2147483635
      ForeColorSel    =   -2147483634
      BackColorBkg    =   -2147483636
      BackColorAlternate=   -2147483643
      GridColor       =   -2147483633
      GridColorFixed  =   -2147483632
      TreeColor       =   -2147483632
      FloodColor      =   192
      SheetBorder     =   -2147483642
      FocusRect       =   1
      HighLight       =   1
      AllowSelection  =   -1  'True
      AllowBigSelection=   -1  'True
      AllowUserResizing=   0
      SelectionMode   =   0
      GridLines       =   1
      GridLinesFixed  =   2
      GridLineWidth   =   1
      Rows            =   50
      Cols            =   10
      FixedRows       =   1
      FixedCols       =   1
      RowHeightMin    =   0
      RowHeightMax    =   0
      ColWidthMin     =   0
      ColWidthMax     =   0
      ExtendLastCol   =   0   'False
      FormatString    =   ""
      ScrollTrack     =   0   'False
      ScrollBars      =   3
      ScrollTips      =   0   'False
      MergeCells      =   0
      MergeCompare    =   0
      AutoResize      =   -1  'True
      AutoSizeMode    =   0
      AutoSearch      =   0
      AutoSearchDelay =   2
      MultiTotals     =   -1  'True
      SubtotalPosition=   1
      OutlineBar      =   0
      OutlineCol      =   0
      Ellipsis        =   0
      ExplorerBar     =   0
      PicturesOver    =   0   'False
      FillStyle       =   0
      RightToLeft     =   0   'False
      PictureType     =   0
      TabBehavior     =   0
      OwnerDraw       =   0
      Editable        =   0
      ShowComboButton =   -1  'True
      WordWrap        =   0   'False
      TextStyle       =   0
      TextStyleFixed  =   0
      OleDragMode     =   0
      OleDropMode     =   0
      DataMode        =   0
      VirtualData     =   -1  'True
      DataMember      =   ""
      ComboSearch     =   3
      AutoSizeMouse   =   -1  'True
      FrozenRows      =   0
      FrozenCols      =   0
      AllowUserFreezing=   0
      BackColorFrozen =   0
      ForeColorFrozen =   0
      WallPaperAlignment=   9
   End
   Begin VB.PictureBox Picture1 
      Height          =   2175
      Left            =   7920
      ScaleHeight     =   2115
      ScaleWidth      =   1995
      TabIndex        =   3
      Top             =   360
      Width           =   2055
   End
   Begin VB.TextBox Text1 
      Height          =   495
      Left            =   960
      TabIndex        =   2
      Text            =   "Text1"
      Top             =   1320
      Width           =   1815
   End
   Begin VB.Frame Frame1 
      Caption         =   "Frame1"
      Height          =   2295
      Left            =   3240
      TabIndex        =   0
      Top             =   480
      Width           =   3255
      Begin VB.CommandButton Command1 
         Caption         =   "Command1"
         Height          =   975
         Left            =   1080
         TabIndex        =   1
         Top             =   720
         Width           =   1215
      End
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit


Private Sub EnumControls(ctl)
    MsgBox ctl.Name
    
    Dim c As Control
    If HasControls(ctl) Then
        For Each c In ctl.Controls
            EnumControls (c)
        Next
    End If
    

End Sub

Private Function HasControls(obj) As Boolean
On Error GoTo Handler
    Dim x
    x = obj.Controls
    HasControls = True
    Exit Function
Handler:
    Exit Function
End Function

Private Sub EnumForm(ByVal frm As Form)
    MsgBox frm.Name
    
    Dim c As Control
    For Each c In frm.Controls
        EnumControls c
    Next
    

End Sub

Private Sub Command1_Click()
    'EnumForm Me
    Dim x As Called.DummyClass
    Set x = New Called.DummyClass
    'Project1.DummyClass.Access
    x.SetGlobal [Global]
    Dim json As String
    
    json = x.Access
    MsgBox json
End Sub

Private Sub SubWizard1_GotFocus()

End Sub

Private Sub PopulateGrid()
    Dim i As Long
    Dim max As Double
    ' initialize array with random data
    Dim count(1, 7) As Single
    For i = 0 To 7
        count(0, i) = Rnd * 100
        count(1, i) = Rnd * 100
    Next
    ' initialize control
    fg.Cols = 3
    fg.Rows = 9
    fg.FloodColor = RGB(100, 255, 100)
    fg.ColAlignment(0) = flexAlignCenterCenter
    fg.ColAlignment(1) = flexAlignRightCenter
    fg.ColAlignment(2) = flexAlignLeftCenter
    fg.Cell(flexcpText, 0, 0) = "Age Range"
    fg.Cell(flexcpText, 0, 1) = "Females"
    fg.Cell(flexcpText, 0, 2) = "Males"
    fg.ColFormat(-1) = "#.##"
    ' make data bold
    fg.Cell(flexcpFontBold, 1, 1, fg.Rows - 1, fg.Cols - 1) = True
    ' place text in cells, keep track of maximum
    For i = 0 To 7
        fg.Cell(flexcpText, i + 1, 0) = 10 * i & " - " & (10 * (i + 1) - 1)
        fg.Cell(flexcpText, i + 1, 1) = count(0, i)
        fg.Cell(flexcpText, i + 1, 2) = count(1, i)
        If count(0, i) > max Then max = count(0, i)
        If count(1, i) > max Then max = count(1, i)
    Next
    ' set each cell's flood percentage,
    ' using max to scale from 0 to -100 for column 1
    ' and from 0 to 100 for column 2:
    For i = 0 To 7
        fg.Cell(flexcpFloodPercent, i + 1, 1) = 100 * count(0, i) / max
        fg.Cell(flexcpFloodPercent, i + 1, 2) = 100 * count(1, i) / max
    Next
End Sub

Private Sub Form_Load()
 PopulateGrid
End Sub
