Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics

Public Module CleaningModule
    Sub XamlClean()
        Dim indent As String = "      "
        Dim j As Integer = 8
        Dim k As Integer = 0
        Dim newHeader As String
        Dim LFChars As Char() = Environment.NewLine.ToCharArray()

        LFChars.SetValue(Nothing, 0)
        If Not DTE.ActiveDocument.Name.EndsWith(".xaml") Then
            MsgBox("The active document should be XAML")
            Return
        End If

        DTE.ActiveDocument.Selection.SelectAll()

        Dim doc As String = DTE.ActiveDocument.Selection.Text
        Dim headerEndIndex As Integer = doc.IndexOf(">")
        Dim headerParts As String() = doc.Substring(0, headerEndIndex).Split(LFChars)
        Dim docBody As String = doc.Substring(headerEndIndex + 1)
        Dim headerPartsSorted(headerParts.Length) As String
        Dim headerPartsCustom(headerParts.Length) As String
        headerPartsSorted(0) = headerParts(0)
        For i = 1 To headerParts.Length - 1
            Dim trimedHeaderPart As String = headerParts(i).Trim()
            Select Case trimedHeaderPart
                Case ""
                Case "xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"""
                    headerPartsSorted(1) = trimedHeaderPart
                Case "xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"""
                    headerPartsSorted(2) = trimedHeaderPart
                Case "xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"""
                    headerPartsSorted(3) = trimedHeaderPart
                Case "xmlns:d=""http://schemas.microsoft.com/expression/blend/2008"""
                    headerPartsSorted(4) = trimedHeaderPart
                Case "mc:Ignorable=""d"""
                    headerPartsSorted(5) = trimedHeaderPart
                    'Case "d:DesignHeight=""300"" d:DesignWidth=""300""", "d:DesignHeight=""500"" d:DesignWidth=""700""", "d:DesignHeight=""300"""
                    '    headerPartsSorted(6) = "d:DesignHeight=""500"" d:DesignWidth=""700"""

                Case Else

                    If trimedHeaderPart.StartsWith("mc:Ignorable=""d""") Then
                        headerPartsSorted(5) = "mc:Ignorable=""d"""
                        trimedHeaderPart = trimedHeaderPart.Substring(headerPartsSorted(5).Length).Trim()
                    End If

                    If trimedHeaderPart.StartsWith("Title") Then
                        headerPartsSorted(7) = trimedHeaderPart
                    ElseIf trimedHeaderPart.StartsWith("d:Design") Then
                        If headerPartsSorted(6) Is Nothing Then headerPartsSorted(6) = "d:DesignHeight=""500"" d:DesignWidth=""700"""
                    Else
                        If trimedHeaderPart.Length > 0 And NSIsReferenced(trimedHeaderPart, docBody) Then
                            headerPartsCustom(k) = trimedHeaderPart
                            k += 1
                        End If
                    End If
            End Select

        Next

        Array.Resize(headerPartsCustom, k)
        Array.Sort(headerPartsCustom)

        If (headerPartsCustom.Length + j) > headerPartsSorted.Length Then
            Array.Resize(headerPartsSorted, headerPartsCustom.Length + j)
        End If

        'Array.Reverse(headerPartsCustom)
        For Each item In headerPartsCustom
            If Not item Is Nothing Then
                headerPartsSorted(j) = item
                j += 1
            End If
        Next


        newHeader = headerPartsSorted(0).Trim() + Environment.NewLine
        For i = 1 To j - 2
            newHeader += indent + headerPartsSorted(i) + Environment.NewLine
        Next
        newHeader += indent + headerPartsSorted(j - 1) + ">" + Environment.NewLine
        Dim docLines As String() = docBody.Split(LFChars, System.StringSplitOptions.RemoveEmptyEntries)

        Dim previousLineIsEmpty As Boolean = False
        For i = 0 To docLines.Length - 1
            Dim line As String = docLines(i).Trim()
            If line.Length > 0 Then
                previousLineIsEmpty = False
                newHeader += docLines(i).Trim() + Environment.NewLine
            Else
                If previousLineIsEmpty = False Then
                    newHeader += Environment.NewLine
                End If
                previousLineIsEmpty = True
            End If
        Next

        DTE.ActiveDocument.Selection.SelectAll()
        Dim objSel As TextSelection = DTE.ActiveDocument.Selection
        objSel.Insert(newHeader, _
         vsInsertFlags.vsInsertFlagsContainNewText)
        objSel.Untabify()
        DTE.ExecuteCommand("Edit.FormatDocument")


        DTE.ActiveDocument.Selection.SelectAll()
        doc = DTE.ActiveDocument.Selection.Text
        Dim unindentedParts As String() = doc.Split(LFChars)

        For i = 1 To unindentedParts.Length - 2
            If unindentedParts(i).Length > 1 And Not unindentedParts(i).StartsWith(" ") Then
                DTE.ActiveDocument.Selection.GotoLine(i + 1)
                DTE.ActiveDocument.Selection.DeleteLeft()
                DTE.ActiveDocument.Selection.NewLine()
            End If
        Next

        DTE.ExecuteCommand("View.ViewCode")
        DTE.ExecuteCommand("Edit.RemoveAndSort")
        DTE.ExecuteCommand("Edit.FormatDocument")
        'DTE.ExecuteCommand("View.ViewDesigner")
    End Sub
    Private Function NSIsReferenced(ByVal nsLine As String, ByVal docBody As String) As Boolean
        Dim nsAlias As String
        Dim ns As String = "xmlns:"
        If nsLine.StartsWith(ns) Then
            nsAlias = nsLine.Substring(ns.Length, nsLine.IndexOf("=") - ns.Length)
            Return docBody.Contains("<" + nsAlias + ":") Or _
                   docBody.Contains("""" + nsAlias + ":") Or _
                   docBody.Contains("/" + nsAlias + ":") Or _
                   docBody.Contains(" " + nsAlias + ":")
        Else
            Return True
        End If
    End Function
End Module
