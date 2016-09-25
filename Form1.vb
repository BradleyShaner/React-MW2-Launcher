Imports System.IO

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'If CheckBox1.Checked Then Directory.Delete("players", True)

        'If Len(TextBox1.Text) <= 3 Then ShowMsg("Please enter a longer Player Name!") : Exit Sub

        If CheckBox1.Checked Then
            Try
                Kill("players/*.stat")
                Kill("players/*.iw4")
            Catch ex As Exception

            End Try

        End If

        SetStatus("Attempting to locate IW4..")
        SetButtons(False)

        If Len(TextBox1.Text) <= 3 Then
            SendCmd("unlockstats")
        Else
            SendCmd("name " & TextBox1.Text & ";" & "unlockstats")
        End If

    End Sub

    Public Sub SendCmd(ByVal command As String)
        If ConnectionManager.CheckProcess = True Then
            ConnectionManager.Handle(command, True)
        Else
            ConnectionManager.Handle(command, False)
        End If
    End Sub

    Public Sub ShowMsg(Msg As String)
        SetStatus(Msg)
        MessageBox.Show(Msg)
        SetButtons(True)
    End Sub

    Public Sub SetButtons(State As Boolean)
        TextBox1.Enabled = State
        CheckBox1.Enabled = State
        Button1.Enabled = State
    End Sub

    Public Sub SetStatus(Status As String, Optional AddToLbl As Boolean = False)
        If AddToLbl Then
            lblStatus.Text += Status
        Else
            lblStatus.Text = Status
        End If
    End Sub

    Private Sub lblStatus_Click(sender As Object, e As EventArgs) Handles lblStatus.Click

    End Sub

    Private Sub TextBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles TextBox1.MouseMove
        ToolTip1.Show("This box controls your in-game name. 
If left empty, no name will be sent to the client.
If the client remembers your name, you can safely leave this box blank.", TextBox1)
    End Sub
End Class