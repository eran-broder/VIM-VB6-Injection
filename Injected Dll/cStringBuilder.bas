Attribute VB_Name = "cStringBuilder"
Option Explicit

' ======================================================================================
' Name:     vbAccelerator cStringBuilder
' Author:   Steve McMahon (steve@vbaccelerator.com)
' Date:     1 January 2002
'
' Copyright ? 2002 Steve McMahon for vbAccelerator
' --------------------------------------------------------------------------------------
' Visit vbAccelerator - advanced free source code for VB programmers
' http://vbaccelerator.com
' --------------------------------------------------------------------------------------
'
' VB can be slow to append strings together because of the continual
' reallocation of string size.  This class pre-allocates a string in
' blocks and hence removes the performance restriction.
'
' Quicker insert and remove is also possible since string space does
' not have to be reallocated.
'
' Example:
' Adding "http://vbaccelerator.com/" 10,000 times to a string:
' Standard VB:   34s
' This Class:    0.35s
'
' ======================================================================================

Private Declare Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" _
      (pDst As Any, pSrc As Any, ByVal ByteLen As Long)
      

Public Type cStringBuilderRecord
    m_sString As String
    m_iChunkSize As Long
    m_iPos As Long
    m_iLen As Long
End Type

Public Function Length(instance As cStringBuilderRecord) As Long
   Length = instance.m_iPos \ 2
End Function

Public Function Capacity(instance As cStringBuilderRecord) As Long
   Capacity = instance.m_iLen \ 2
End Function

Public Function ChunkSize(instance As cStringBuilderRecord) As Long
   ' Return the unicode character chunk size:
   ChunkSize = instance.m_iChunkSize \ 2
End Function

Public Sub SetChunkSize(instance As cStringBuilderRecord, ByVal iChunkSize As Long)
   ' Set the chunksize.  We multiply by 2 because internally
   ' we are considering bytes:
   instance.m_iChunkSize = iChunkSize * 2
End Sub

Public Function toString(instance As cStringBuilderRecord) As String
   ' The internal string:
   If instance.m_iPos > 0 Then
      toString = Left$(instance.m_sString, instance.m_iPos \ 2)
   End If
End Function

Public Sub SetTheString(instance As cStringBuilderRecord, ByRef sThis As String)
   Dim lLen As Long
   
   ' Setting the string:
   lLen = LenB(sThis)
   If lLen = 0 Then
      'Clear
      instance.m_sString = ""
      instance.m_iPos = 0
      instance.m_iLen = 0
   Else
      If instance.m_iLen < lLen Then
         ' Need to expand string to accommodate:
         Do
            instance.m_sString = instance.m_sString & Space$(instance.m_iChunkSize \ 2)
            instance.m_iLen = instance.m_iLen + instance.m_iChunkSize
         Loop While instance.m_iLen < lLen
      End If
      CopyMemory ByVal StrPtr(instance.m_sString), ByVal StrPtr(sThis), lLen
      instance.m_iPos = lLen
   End If
   
End Sub

Public Sub Clear(instance As cStringBuilderRecord)
   instance.m_sString = ""
   instance.m_iPos = 0
   instance.m_iLen = 0
End Sub

Public Sub AppendNL(instance As cStringBuilderRecord, ByRef sThis As String)
   Append instance, sThis
   Append instance, vbCrLf
End Sub

Public Sub Append(instance As cStringBuilderRecord, ByRef sThis As String)
   Dim lLen As Long
   Dim lLenPlusPos As Long
 
   ' Append an item to the string:
   lLen = LenB(sThis)
   lLenPlusPos = lLen + instance.m_iPos
   If lLenPlusPos > instance.m_iLen Then
      Dim lTemp As Long
      
      lTemp = instance.m_iLen
      Do While lTemp < lLenPlusPos
         lTemp = lTemp + instance.m_iChunkSize
      Loop
      
      instance.m_sString = instance.m_sString & Space$((lTemp - instance.m_iLen) \ 2)
      instance.m_iLen = lTemp
   End If
   
   CopyMemory ByVal UnsignedAdd(StrPtr(instance.m_sString), instance.m_iPos), ByVal StrPtr(sThis), lLen
   instance.m_iPos = instance.m_iPos + lLen
End Sub

Public Sub AppendByVal(instance As cStringBuilderRecord, ByVal sThis As String)
   Append instance, sThis
End Sub

Public Sub Insert(instance As cStringBuilderRecord, ByVal iIndex As Long, ByRef sThis As String)
   Dim lLen As Long
   Dim lPos As Long
   Dim lSize As Long
   
   ' is iIndex within bounds?
   If (iIndex * 2 > instance.m_iPos) Then
      Err.Raise 9
   Else
   
      lLen = LenB(sThis)
      If (instance.m_iPos + lLen) > instance.m_iLen Then
         instance.m_sString = instance.m_sString & Space$(instance.m_iChunkSize \ 2)
         instance.m_iLen = instance.m_iLen + instance.m_iChunkSize
      End If
      
      ' Move existing characters from current position
      lPos = UnsignedAdd(StrPtr(instance.m_sString), iIndex * 2)
      lSize = instance.m_iPos - iIndex * 2
      
      ' moving from iIndex to iIndex + lLen
      CopyMemory ByVal UnsignedAdd(lPos, lLen), ByVal lPos, lSize
      
      ' Insert new characters:
      CopyMemory ByVal lPos, ByVal StrPtr(sThis), lLen
      
      instance.m_iPos = instance.m_iPos + lLen
   End If
End Sub

Public Sub InsertByVal(instance As cStringBuilderRecord, ByVal iIndex As Long, ByVal sThis As String)
   Insert instance, iIndex, sThis
End Sub

Public Sub Remove(instance As cStringBuilderRecord, ByVal iIndex As Long, ByVal lLen As Long)
   Dim lSrc As Long
   Dim lDst As Long
   Dim lSize As Long

   ' is iIndex within bounds?
   If (iIndex * 2 > instance.m_iPos) Then
      Err.Raise 9
   Else
      ' is there sufficient length?
      If ((iIndex + lLen) * 2 > instance.m_iPos) Then
         Err.Raise 9
      Else
         ' Need to copy characters from iIndex*2 to m_iPos back by lLen chars:
         lSrc = UnsignedAdd(StrPtr(instance.m_sString), (iIndex + lLen) * 2)
         lDst = UnsignedAdd(StrPtr(instance.m_sString), iIndex * 2)
         lSize = (instance.m_iPos - (iIndex + lLen) * 2)
         CopyMemory ByVal lDst, ByVal lSrc, lSize
         instance.m_iPos = instance.m_iPos - lLen * 2
      End If
   End If
End Sub

Public Function Find(instance As cStringBuilderRecord, ByVal sToFind As String, _
   Optional ByVal lStartIndex As Long = 1, _
   Optional ByVal compare As VbCompareMethod = vbTextCompare _
   ) As Long
   
   Dim lInstr As Long
   If (lStartIndex > 0) Then
      lInstr = InStr(lStartIndex, instance.m_sString, sToFind, compare)
   Else
      lInstr = InStr(instance.m_sString, sToFind, compare)
   End If
   If (lInstr < instance.m_iPos \ 2) Then
      Find = lInstr
   End If
End Function

Public Sub HeapMinimize(instance As cStringBuilderRecord)
   Dim iLen As Long
   
   ' Reduce the string size so only the minimal chunks
   ' are allocated:
   If (instance.m_iLen - instance.m_iPos) > instance.m_iChunkSize Then
      iLen = instance.m_iLen
      Do While (iLen - instance.m_iPos) > instance.m_iChunkSize
         iLen = iLen - instance.m_iChunkSize
      Loop
      instance.m_sString = Left$(instance.m_sString, iLen \ 2)
      instance.m_iLen = iLen
   End If
   
End Sub
Private Function UnsignedAdd(Start As Long, Incr As Long) As Long
' This function is useful when doing pointer arithmetic,
' but note it only works for positive values of Incr

   If Start And &H80000000 Then 'Start < 0
      UnsignedAdd = Start + Incr
   ElseIf (Start Or &H80000000) < -Incr Then
      UnsignedAdd = Start + Incr
   Else
      UnsignedAdd = (Start + &H80000000) + (Incr + &H80000000)
   End If
   
End Function
Public Function CreateStringBuilder() As cStringBuilderRecord
   ' The default allocation: 8192 characters.
   Dim result As cStringBuilderRecord
   result.m_iChunkSize = 16384
   CreateStringBuilder = result
End Function




