Attribute VB_Name = "MessageHandler"
Public Type VimMessage
    MessageId As Long
    MessageType As String * 255
    Arg1 As String * 255
    Arg2 As String * 255
    Arg3 As String * 255
    Arg4 As String * 255
    Arg5 As String * 255
End Type



