Imports System.Net.Sockets
Imports System.Threading
Imports System.IO
Imports System.Text

Module servermod
    Dim port As Int32 = 8000
    Dim listener = New TcpListener(port)
    Dim desktop_path = My.Computer.FileSystem.SpecialDirectories.Desktop

    Sub Main()
        Try
            listener.Start()
            Dim listthread As New Thread(New ThreadStart(AddressOf listening))
            listthread.Start()

            Dim bytes(1024) As Byte
            Dim message As String = Nothing

            Console.WriteLine("Server listening on port " + port.ToString())

            While True
                Console.WriteLine("Waiting for a connection...")
                Dim client As TcpClient = listener.AcceptTcpClient
                Console.WriteLine("Processing request...")

                message = Nothing

                Dim i As Int32
                Dim stream As NetworkStream = client.GetStream()

                '' START READING AND EVALUATING
                i = stream.Read(bytes, 0, bytes.Length)

                While (i <> 0)
                    message = Encoding.ASCII.GetString(bytes, 0, i)
                    Dim separator As String = ":" ' used to get filenames and urls
                    Dim content_indicator As String = ">" ' used to indicate the beginning the content to be saved

                    If (message.Contains("open disk tray")) Then
                        Console.WriteLine(Format(Now, "[dd, MM | hh: mmHss]") & " RECEIVED [client]: " & "open disk tray" & vbCrLf)
                        mciSendString("set CDAudio door open", 0, 0, 0)
                        Exit While ' exits the loop and begins to listen for new connections
                    End If

                    If (message.Contains("create folder")) Then
                        If (message.Contains(separator)) Then
                            Dim x As Integer = InStr(message, separator)
                            Dim folder_name As String = message.Substring(x + separator.Length - 1)

                            Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " RECEIVED [client]: " & "create folder" & vbCrLf)
                            My.Computer.FileSystem.CreateDirectory(desktop_path + "\" + folder_name)
                            Exit While
                        End If
                    End If

                    If (message.Contains("create file")) Then
                        Dim content As String = ""
                        Dim file_name As String
                        If (message.Contains(separator)) Then
                            Dim g As Integer = message.IndexOf(separator)
                            If (message.Contains(content_indicator)) Then
                                Dim contentMark = InStr(message, content_indicator)
                                content = message.Substring(contentMark + content_indicator.Length - 1)

                                file_name = message.Substring(g + 1, message.IndexOf(content_indicator, g + 1) - g - 1)
                                Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " RECEIVED [client]: " & "create file" & vbCrLf)

                            Else
                                Dim s As Integer = InStr(message, separator)

                                file_name = message.Substring(s + separator.Length - 1)
                                Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " RECEIVED [client]: " & "create file" & vbCrLf)
                            End If

                            Dim filepath As String = desktop_path + "/" + file_name

                            If Not File.Exists(filepath) Then
                                File.Create(filepath).Dispose()
                            End If

                            My.Computer.FileSystem.WriteAllText(filepath, content, True)
                            Exit While
                        End If
                    End If

                    If (message.Contains("delete file")) Then
                        If (message.Contains(":")) Then

                            Dim x As Integer = InStr(message, separator)
                            Dim tbd_flname As String = message.Substring(x + separator.Length - 1)

                            Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " RECEIVED [client]: " & "delete file" & vbCrLf)
                            Dim FileToDelete = desktop_path + "/" + tbd_flname + ".txt"

                            If File.Exists(FileToDelete) = True Then
                                File.Delete(FileToDelete)
                            End If
                            Exit While
                        End If
                    End If

                    If (message.Contains("open browser")) Then
                        If (message.Contains(separator)) Then
                            Dim x As Integer = InStr(message, separator)
                            Dim url As String = message.Substring(x + separator.Length - 1)

                            Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " RECEIVED [client]: " & "open browser" & vbCrLf)
                            Process.Start("iexplore", url)
                            Exit While
                        End If
                    End If

                    If (message.Contains("reboot")) Then
                        System.Diagnostics.Process.Start("ShutDown", "/r")
                    End If
                    
                    If (message.Contains("shutdown")) Then
                        client.Close()
                        listener.Stop()
                        Environment.Exit(0)
                    End If

                    If Not (message.Contains("create folder") And message.Contains("create file") And message.Contains("delete file") And message.Contains("open browser") And message.Contains("open disk tray") And message.Contains("shutdown")) Then
                        Console.WriteLine(Format(Now, "[dd,MM | hh:mmHss]") & " MESSAGE [client]: " & "INVALID REQUEST" & vbCrLf)
                        Exit While
                    End If

                    Dim arrMsg() As String = Split(message, "|")

                    If (arrMsg(0) = "BRW") Then
                        Process.Start(arrMsg(1))
                    End If

                End While
                ' Shutdown and end connection
                client.Close()
            End While

        Catch e As SocketException
            Console.WriteLine("SocketException: {0}", e)
        Finally
            listener.Stop()
        End Try
    End Sub

    Private Sub listening()
        listener.Start()
    End Sub

    Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpszCommand As String, ByVal lpszReturnString As String, ByVal cchReturnLength As Long, ByVal hwndCallback As Long) As Long

End Module
