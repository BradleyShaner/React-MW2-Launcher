Imports Microsoft.Win32
Imports System
Imports System.Diagnostics
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.IO.File

Public Class ConnectionManager
    ' Methods
    Private Shared Function GetEditText(ByVal hwnd As IntPtr) As String
        Dim capacity As Integer = ConnectionManager.SendMessage(hwnd, 14, 0, DirectCast(Nothing, StringBuilder))
        Dim lParam As New StringBuilder(capacity)
        ConnectionManager.SendMessage(hwnd, 13, CInt((capacity + 1)), lParam)
        Return lParam.ToString
    End Function

    Private Shared Function GetLogContent() As String
        Return ConnectionManager.GetEditText(ConnectionManager._hwndLog)
    End Function

    Public Shared Function CheckProcess() As Boolean
        If (Process.GetProcessesByName("iw4m").Length > 0) Then
            Return True
        ElseIf (Process.GetProcessesByName("iw4m.exe").Length > 0) Then
            Return True
        End If

        Return False
    End Function

    Public Shared Function StartMW2(Optional ByVal strFile As String = "iw4m.exe") As Boolean
        If CheckProcess() = False Then
            'Console.WriteLine("Dir:" & Environment.CurrentDirectory())
            If Exists(Environment.CurrentDirectory() & "/" & strFile) Then
                Try
                    Dim nProcess As New Process
                    With nProcess
                        .StartInfo.UseShellExecute = False
                        .StartInfo.FileName = strFile
                        .Start()
                    End With
                Catch e As Exception
                    Form1.ShowMsg(e.Message)
                    Return False
                End Try
                Return True
            End If
            'If Shell("iw4m.exe", AppWinStyle.NormalFocus, False) > 0 Then StartMW2 = True
        Else
            'Console.WriteLine("iw4m.exe does not exist.")
            Return True
        End If

        Return False
    End Function

    Public Shared Sub Handle(ByVal args As String, ByVal isInitial As Boolean)
        If StartMW2() = False Then
            Form1.ShowMsg("IW4 was not found or could not be started.")
        Else
            ConnectionManager.PerformConnect(args, isInitial)
        End If
    End Sub

    Private Shared Function OpenIW4Process() As IntPtr
        Dim procHwnd As Integer

        Form1.SetStatus("Waiting for IW4's process..")
        For i = 1 To 120
            procHwnd = FindWindow("IW4 WinConsole", vbNullString)
            If (procHwnd <> IntPtr.Zero) Then
                Return procHwnd
            End If
            Thread.Sleep(100)
            Application.DoEvents()
            If IsEven(i) Then Form1.SetStatus(".", True)
        Next
    End Function

    Private Shared Sub PerformConnect(ByVal command As String, ByVal isInitial As Boolean)
        Dim ptr As IntPtr = ConnectionManager.OpenIW4Process

        If (ptr <> IntPtr.Zero) Then
            Form1.SetStatus("Waiting until IW4 has loaded..")
            ConnectionManager._process = ptr

            If ConnectionManager.WaitForConsole() = False Then Form1.ShowMsg("Timed out, please try again.")

            If Not isInitial Then
                'If ConnectionManager.WaitForLog("Successfully read stats data from IWNet") = False Then
                If ConnectionManager.WaitForLog("Calling Party_StopParty() for partyId") = False Then
                    Form1.ShowMsg("Unable to determine if console is ready.")
                End If
            End If

            If ConnectionManager.GetLogContent.Contains("EXPRESION DEBUG 'Stats:Secondary offhand none locked") Then
                For Each p As Process In System.Diagnostics.Process.GetProcessesByName("iw4m")
                    Try
                        p.Kill()
                        p.WaitForExit(10000)
                    Catch ex As Exception

                    End Try
                Next
                Form1.ShowMsg("The game appears to have lost your loadouts and stats." & vbNewLine &
                "Please Launch the game again and we'll try to correct the issue.")
                Form1.CheckBox1.Checked = True
                Exit Sub
            End If

            ConnectionManager.SendToInput(command)
        Else
            Form1.ShowMsg("Error locating IW4's process!")
        End If
    End Sub

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, <Out()> ByVal lpBuffer As Byte(), ByVal dwSize As Integer, <Out()> ByRef lpNumberOfBytesRead As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function FindWindowExA(ByVal hWnd1 As Integer, ByVal hWnd2 As Integer, ByVal lpsz1 As String, ByVal lpsz2 As String) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindow(ByVal hWnd As IntPtr, ByVal uCmd As Integer) As IntPtr
    End Function

    '<DllImport("user32.dll")>
    'Private Shared Function SendMessageA(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Int32, ByVal lParam As String) As Int32
    'End Function

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInt32, ByVal wParam As Integer, ByVal lParam As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInt32, ByVal wParam As Integer, ByVal lParam As StringBuilder) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInt32, ByVal wParam As IntPtr, <MarshalAs(UnmanagedType.LPStr)> ByVal lParam As String) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInt32, ByVal wParam As IntPtr, ByVal lParam As StringBuilder) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal Msg As UInt32, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    '<DllImport("user32.dll")> _
    'Private Shared Function SendMessageW(ByVal hWnd As IntPtr, ByVal Msg As UInt32, ByVal wParam As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> ByVal lParam As String) As IntPtr
    'End Function

    Private Shared Sub SendToInput(ByVal [text] As String)
        ConnectionManager.SendMessage(ConnectionManager._hwndInput, 12, IntPtr.Zero, [text])
        ConnectionManager.SendMessage(ConnectionManager._hwndInput, &H102, 13, 0)

        Form1.SetButtons(True)
        Form1.SetStatus("Success!")
        If Form1.CheckBox2.Checked Then End

    End Sub

    Private Shared Function WaitForConsole() As Boolean
        Form1.SetStatus("Waiting for console")
        For i = 1 To 60

            ConnectionManager._hwndInput = FindWindowExA(_process, 0, "edit", vbNullString)
            ConnectionManager._hwndLog = GetWindow(ConnectionManager._hwndInput, 2)

            If ((ConnectionManager._hwndInput <> IntPtr.Zero) AndAlso (ConnectionManager._hwndLog <> IntPtr.Zero)) Then
                Return True
            End If
            Thread.Sleep(500)
            Application.DoEvents()
            If IsEven(i) Then Form1.SetStatus(".", True)
        Next
        Form1.ShowMsg("Timed out, please try again")
        Return False
    End Function

    Private Shared Function WaitForLog(ByVal contains As String) As Boolean
        Form1.SetStatus("Waiting for log")
        For i = 1 To 60
            If IsEven(i) Then Form1.SetStatus(".", True)
            If ConnectionManager.GetLogContent.Contains(contains) Then
                Return True
            End If
            Thread.Sleep(500)
            Application.DoEvents()
        Next
        Form1.ShowMsg("Timed out, please try again")
        Return False
    End Function

    Public Shared Function IsEven(ByVal Number As Long) As Boolean
        IsEven = (Number Mod 2 = 0)
    End Function

    ' Fields
    Private Shared _hwndInput As IntPtr
    Private Shared _hwndLog As IntPtr
    Private Shared _process As IntPtr
    Public Const WM_CHAR As UInt32 = &H102
    Public Const WM_GETTEXT As UInt32 = 13
    Public Const WM_GETTEXTLENGTH As UInt32 = 14
    Public Const WM_SETTEXT As UInt32 = 12
End Class
