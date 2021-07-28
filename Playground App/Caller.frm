VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   4380
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   4560
   LinkTopic       =   "Form1"
   ScaleHeight     =   4380
   ScaleWidth      =   4560
   StartUpPosition =   3  'Windows Default
   Begin VB.TextBox TextBoxOutput 
      Height          =   855
      Left            =   240
      TabIndex        =   7
      Text            =   "Text1"
      Top             =   2520
      Width           =   4095
   End
   Begin VB.TextBox txbHandle 
      Height          =   495
      Left            =   2160
      TabIndex        =   6
      Top             =   1560
      Width           =   2175
   End
   Begin VB.TextBox txbThread 
      Height          =   495
      Left            =   2160
      TabIndex        =   4
      Top             =   960
      Width           =   2175
   End
   Begin VB.CommandButton cmdCall 
      Caption         =   "Call Function"
      Height          =   495
      Left            =   720
      TabIndex        =   2
      Top             =   3600
      Width           =   2895
   End
   Begin VB.TextBox txbProcessId 
      Height          =   495
      Left            =   2160
      TabIndex        =   0
      Text            =   "Text1"
      Top             =   360
      Width           =   2175
   End
   Begin VB.Label Label1 
      Caption         =   "Handle"
      Height          =   375
      Left            =   0
      TabIndex        =   5
      Top             =   1560
      Width           =   1455
   End
   Begin VB.Label lblValueReturned 
      Caption         =   "Thread ID"
      Height          =   375
      Left            =   120
      TabIndex        =   3
      Top             =   840
      Width           =   1455
   End
   Begin VB.Label lblProcessId 
      Caption         =   "Process ID"
      Height          =   375
      Left            =   120
      TabIndex        =   1
      Top             =   360
      Width           =   1455
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Declare Function GetCurrentProcessId Lib "kernel32" () As Long
Private Declare Function GetCurrentThreadId Lib "kernel32" () As Long



Private Sub Form_Load()
    Me.Caption = "Form1"
    txbProcessId = GetCurrentProcessId()
    txbThread = GetCurrentThreadId()
    txbHandle = Me.hWnd
    cmdCall.Caption = Forms.Count
End Sub


Public Sub AddAseessment(nId, bPrimary, strCode, strName, strSpecify, strNotes) '14F11A0
    TextBoxOutput = "Assesment added : " + CStr(nId) + " - " + CStr(strCode)
End Sub

Private Sub cmdCall_Click()
'txbValueReturned = FunctionCalled(txbValuePassed)
End Sub
