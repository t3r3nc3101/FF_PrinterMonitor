Imports System.Net.Sockets
Imports System.Text
Imports System.Diagnostics

Imports System.Runtime.InteropServices
Imports System.Windows.Threading

Class MainWindow

    Dim printerIP = "192.168.1.3" ''  is set at launch / when the ip textbox has changed   -- OR BY UI EDITOR IF NEEDED

    Private Sub Grid_Loaded(sender As Object, e As RoutedEventArgs)

        'System.Threading.Thread.Sleep(1500)

        Connect(printerIP, "~M601 S1")
        label.Content = responseData + label.Content

        'Debug.WriteLine()
        'if here
        NewTimerPls()   ' < was on!!! t333

        webView11.Source = New Uri("http://" + printerIP + ":8080/?action=stream")


    End Sub




    '          ////////////////////////////////////////////////////////

    Private dpTimer As DispatcherTimer

    Public Sub NewTimerPls()
        dpTimer = New DispatcherTimer
        TickMe()
        dpTimer.Interval = TimeSpan.FromMilliseconds(10000) '5000
        AddHandler dpTimer.Tick, AddressOf TickMe
        dpTimer.Start()
    End Sub

    Private Sub TickMe()

        If connectedSuccessfully = True Then   ' < CHECK FOR CONNECTION FIRST

            webView11.Source = New Uri("http://" + printerIP + ":8080/?action=stream")
            System.Threading.Thread.Sleep(500)    ' <<<< DELAY NEEDED SO PRINTER DOESNT FREAK OUT
            Dim currentJobPercent = getJobPercent()
            System.Threading.Thread.Sleep(500)    ' <<<< DELAY NEEDED SO PRINTER DOESNT FREAK OUT
            Dim currentNozzleTemp = getTemp()

            jobProgressBar.Value = currentJobPercent
            jobPercentText.Content = currentJobPercent.ToString() + "%"

            nozzleTempText.Content = currentNozzleTemp.ToString()
        End If


    End Sub


    '          //////////////////////////////////////////////////////////







    Public Async Sub asyncRefresh()
        ' loading_LABEL.Visibility = Visibility.Visible
        Dim task As Task = Task.Run(Function() refreshPrinterData())
        Await task
        ' do somthin when done waiting
        'loading_LABEL.Visibility = Visibility.Collapsed

        'asyncRefresh()
        label.Content = responseData + label.Content
        'Debug.WriteLine(responseData)

        asyncRefresh()

    End Sub

    Function refreshPrinterData()
        Connect(printerIP, "~M114")
        'System.Threading.Thread.Sleep(2000)
    End Function

    Dim percentAsInt = 0

    '   Private Sub button_Copy_Click(sender As Object, e As RoutedEventArgs) Handles button_Copy.Click  ''  << HEAD POS
    '       Connect(printerIP, "~M114")
    '   'Debug.WriteLine(responseData)
    '   End Sub



    Function getJobPercent()
        '' GET AND FORMAT PERCENT ''
        Connect(printerIP, "~M27")
        Dim toBeSearched = "byte "
        Dim code = responseData.Substring(responseData.IndexOf(toBeSearched) + toBeSearched.Length)
        Dim extractedPercentString = code.Remove(2, 9)
        Dim cleaned = extractedPercentString.Replace("/", "")
        'Debug.WriteLine(cleaned)
        Dim percentAsInt = Convert.ToInt32(cleaned)

        If cleaned > 0 Then
            statusText_label.Content = "Printing..."    '  << set job status text
        End If

        Return percentAsInt
    End Function
    Function getTemp()
        '' GET AND FORMAT TEMPS & LIMITS ''
        Connect(printerIP, "~M105")

        Dim b4str As String
        Dim b4x
        b4str = responseData.ToString()
        b4x = InStr(b4str, ".")

        Dim b4cleaned = Strings.Right(b4str, b4x - 1)

        Dim str As String
        Dim x
        str = b4cleaned
        x = InStr(str, " ")
        Dim cleaned = Strings.Left(str, x - 1)

        'Debug.WriteLine(str)
        'Dim cleaned = code.Remove(1, code.Length - 1)
        'Dim cleaned = extractedPercentString.Replace("/", "")
        Debug.WriteLine(b4str)
        Dim tempAsInt = Convert.ToInt32(cleaned)
        Return tempAsInt
    End Function




    ' String to store the response ASCII representation.
    Dim responseData As [String] = [String].Empty

    Dim connectedSuccessfully = False

    Sub Connect(server As [String], message As [String])
        ' Create a TcpClient.
        ' Note, for this client to work you need to have a TcpServer 
        ' connected to the same address as specified by the server, port
        ' combination.
        Dim port As Int32 = 8899    ' 13000
        'Debug.WriteLine(server)


        Dim client As TcpClient
        Try
            client = New TcpClient(server, port)   '<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            connectedSuccessfully = True

        Catch
            connectedSuccessfully = False

        Catch e As ArgumentNullException
            Debug.WriteLine("ArgumentNullException: {0}", e)
        Catch e As SocketException
            Debug.WriteLine("SocketException: {0}", e)

        Finally

            If connectedSuccessfully = True Then

                '   NewTimerPls()   ' <<<< no

                ' Translate the passed message into ASCII and store it as a Byte array.
                Dim data As [Byte]() = System.Text.Encoding.ASCII.GetBytes(message)

            ' Get a client stream for reading and writing.
            '  Stream stream = client.GetStream();
            Dim stream As NetworkStream = client.GetStream()

            ' Send the message to the connected TcpServer. 
            stream.Write(data, 0, data.Length)

            'Console.WriteLine("Sent: {0}", message)

            ' Receive the TcpServer.response.
            ' Buffer to store the response bytes.
            data = New [Byte](256) {}

            ' Read the first batch of the TcpServer response bytes.
            Dim bytes As Int32 = stream.Read(data, 0, data.Length)
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes)

            ' Close everything.
            stream.Close()
            client.Close()

        End If
        End Try


    End Sub



    Private Sub textBox1_TextInput(sender As Object, e As TextCompositionEventArgs) Handles textBox1.TextInput
        printerIP = textBox1.Text
    End Sub

    Private Sub textBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles textBox1.TextChanged
        printerIP = textBox1.Text
    End Sub

    Private Sub button_Click(sender As Object, e As RoutedEventArgs) Handles button.Click
        Connect(printerIP, "~M601 S1")
    End Sub



End Class



