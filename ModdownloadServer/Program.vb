Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Text

Module Program

    Sub Main()
        Console.WriteLine($"=======================================================")
        Console.WriteLine($"Mod Server made by zv800 https://github.com/zv8001")
        Console.WriteLine($"=======================================================")

        Dim port As Integer = 2930
        Dim serverUrl As String = $"http://localhost:{port}/"

        Dim listener As New HttpListener()
        listener.Prefixes.Add(serverUrl)

        Try
            listener.Start()



            Console.WriteLine($"Server listening on port {port}...")

            Dim processTask As Task = ProcessRequestsAsync(listener)
            processTask.Wait()
        Catch ex As Exception
            Console.WriteLine($"CRASH: {ex.Message}")
        Finally
            ' Wait for user input before exiting
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey()
            listener.Stop()
        End Try
    End Sub

    Private Async Function ProcessRequestsAsync(listener As HttpListener) As Task
        While True
            Dim contextTask As Task(Of HttpListenerContext) = listener.GetContextAsync()
            Dim context As HttpListenerContext = Await contextTask
            Await ProcessRequestAsync(context)
        End While
    End Function


    Private clientRequestTimes As New Dictionary(Of String, DateTime)

    Private Async Function ProcessRequestAsync(context As HttpListenerContext) As Task


        Dim request As HttpListenerRequest = context.Request
        Dim response As HttpListenerResponse = context.Response


        Dim clientIP As String = request.RemoteEndPoint.ToString()
        Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        Console.WriteLine($"Client IP: {clientIP}, Timestamp: {timestamp}")


        If clientRequestTimes.ContainsKey(clientIP) Then
            Dim lastRequestTime As DateTime = clientRequestTimes(clientIP)
            Dim timeDifference As TimeSpan = DateTime.Now - lastRequestTime


            If timeDifference.TotalSeconds < 10 Then
                Console.WriteLine($"FAILURE: 429 client has sent too many requests.")
                response.StatusCode = 429

                Dim responseString As String = "HTTP 429 too many requests; please try again later."
                Dim buffer As Byte() = Encoding.UTF8.GetBytes(responseString)
                response.ContentLength64 = buffer.Length
                Await response.OutputStream.WriteAsync(buffer, 0, buffer.Length)
                response.Close()
                Return

            Else


            End If
        End If


        clientRequestTimes(clientIP) = DateTime.Now

        If request.Url.AbsolutePath = "/" Then
            Try
                Console.WriteLine($"Creating zip file...")
                Dim files As String() = Directory.GetFiles(Directory.GetCurrentDirectory())
                Dim ModsName = GetRandom(1000, 1000000) & ".zip"
                Dim startPath As String = "mods"
                Dim zipPath As String = ModsName
                Dim extractPath As String = "c:\example\extract"
                ZipFile.CreateFromDirectory(startPath, zipPath)
                Console.WriteLine($"zip file created...")

                Console.WriteLine($"sending request for download..")
                Dim fileName As String = Path.GetFileName(zipPath)
                Dim fileBytes As Byte() = File.ReadAllBytes(zipPath)

                response.ContentType = "application/octet-stream"
                response.ContentLength64 = fileBytes.Length
                response.AddHeader("Content-Disposition", $"attachment; filename=""{fileName}""")
                Await response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length)

                response.OutputStream.Close()
                Console.WriteLine($"All files downloaded.")
                File.Delete(ModsName)
            Catch ex As Exception
                Console.WriteLine($"ERR: {ex.Message}")
            End Try
        End If


    End Function


    Public Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer
        Dim Generator As System.Random = New System.Random()
        Return Generator.Next(Min, Max)
    End Function
End Module
