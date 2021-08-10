VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   8340
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   5115
   LinkTopic       =   "Form1"
   ScaleHeight     =   8340
   ScaleWidth      =   5115
   StartUpPosition =   3  'Windows Default
   Begin VB.Frame Frame1 
      Caption         =   "Frame1"
      Height          =   2295
      Left            =   120
      TabIndex        =   2
      Top             =   120
      Width           =   4815
      Begin VB.TextBox txbProcessId 
         Height          =   495
         Left            =   2400
         TabIndex        =   5
         Text            =   "Text1"
         Top             =   360
         Width           =   2175
      End
      Begin VB.TextBox txbThread 
         Height          =   495
         Left            =   2400
         TabIndex        =   4
         Top             =   960
         Width           =   2175
      End
      Begin VB.TextBox txbHandle 
         Height          =   495
         Left            =   2400
         TabIndex        =   3
         Top             =   1560
         Width           =   2175
      End
      Begin VB.Label lblProcessId 
         Caption         =   "Process ID"
         Height          =   375
         Left            =   360
         TabIndex        =   8
         Top             =   360
         Width           =   1455
      End
      Begin VB.Label lblValueReturned 
         Caption         =   "Thread ID"
         Height          =   375
         Left            =   360
         TabIndex        =   7
         Top             =   840
         Width           =   1455
      End
      Begin VB.Label Label1 
         Caption         =   "Handle"
         Height          =   375
         Left            =   360
         TabIndex        =   6
         Top             =   1560
         Width           =   1455
      End
   End
   Begin VB.TextBox TextBoxOutput 
      Height          =   5055
      Left            =   240
      MultiLine       =   -1  'True
      TabIndex        =   1
      Text            =   "Caller.frx":0000
      Top             =   2520
      Width           =   4695
   End
   Begin VB.CommandButton cmdCall 
      Caption         =   "Call Function"
      Height          =   495
      Left            =   960
      TabIndex        =   0
      Top             =   7680
      Width           =   2895
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'Option Explicit

Private Declare Function GetCurrentProcessId Lib "kernel32" () As Long
Private Declare Function GetCurrentThreadId Lib "kernel32" () As Long



Private Sub Form_Load()
    Me.Caption = "Form1"
    txbProcessId = GetCurrentProcessId()
    txbThread = GetCurrentThreadId()
    txbHandle = Me.hWnd
    cmdCall.Caption = Forms.Count
    
    'EnumControlsOfForm Me
    
    Dim f As ControlFilter
    Set f = New ControlFilter
    Dim nameFilter As ControlNamePred
    Set nameFilter = New ControlNamePred
    nameFilter.ControlName = "txbProcessId"
    
    results = f.GetAllControlsThatMatch(Me, nameFilter)
    'MsgBox "Got back : " + CStr(UBound(results)) + " elements. first one is : " + results(0).Name
    
End Sub


Public Sub AddAseessment(nId, bPrimary, strCode, strName, strSpecify, strNotes) '14F11A0
    Log "Assesment added : " + CStr(nId) + " - " + CStr(strCode)
    'TextBoxOutput = "Assesment added : " + CStr(nId) + " - " + CStr(strCode)
End Sub


Public Sub Log(ByVal msg As String) '14F11A0
    TextBoxOutput = TextBoxOutput.Text + vbNewLine + msg
End Sub


