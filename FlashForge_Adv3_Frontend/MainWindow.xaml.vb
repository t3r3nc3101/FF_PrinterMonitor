Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Threading
Imports System.Diagnostics

Class MainWindow
    Private printerIP As String = "192.168.1.32"
    Private responseData As String = String.Empty
    Private connectedSuccessfully As Boolean = False
    Private dpTimer As DispatcherTimer

    Private Sub Grid_Loaded(sender As Object, e As RoutedEventArgs)
        NewTimerPls()
    End Sub

    Private Sub NewTimerPls()
        dpTimer = New DispatcherTimer
        dpTimer.Interval = TimeSpan.FromMilliseconds(10000)
        AddHandler dpTimer.Tick, AddressOf TickMe
        dpTimer.Start()
    End Sub

    Private Sub TickMe()
        If connectedSuccessfully Then
            System.Threading.Thread.Sleep(500)    ' <<<< DELAY NEEDED SO PRINTER DOESNT FREAK OUT
            RefreshPrinterData()
            System.Threading.Thread.Sleep(500)    ' <<<< DELAY NEEDED SO PRINTER DOESNT FREAK OUT
            DisplayCameraFeed()
            System.Threading.Thread.Sleep(500)    ' <<<< DELAY NEEDED SO PRINTER DOESNT FREAK OUT
        End If
    End Sub

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

    Private Sub DisplayCameraFeed()
        Dim cameraFeedUrl = $"http://{printerIP}:8080/?action=stream"
        webView11.Source = New Uri(cameraFeedUrl)
    End Sub

    Private Sub RefreshPrinterData()
        responseData = String.Empty

        Connect(printerIP, "~M105")
        Dim tempInfo = ExtractTempInfo(responseData)
        Dim nozzleValues = tempInfo("nozzle")
        Dim bedValues = tempInfo("bed")

        Dim nozzleTemp = nozzleValues.Item1
        Dim nozzleMaxTemp = nozzleValues.Item2
        Dim bedTemp = bedValues.Item1
        Dim bedMaxTemp = bedValues.Item2

        nozzleTempText.Content = $"{nozzleTemp}"
        nozzleTempMAXText.Content = $"/{nozzleMaxTemp}"

        bedTempText.Content = $"{bedTemp}"
        bedTempMAXText.Content = $"/{bedMaxTemp}"

        Dim currentJobPercent = getJobPercent()

        jobProgressBar.Value = currentJobPercent
        jobPercentText.Content = currentJobPercent.ToString() + "%"


        Debug.WriteLine($"Nozzle Temp: {nozzleTemp} / {nozzleMaxTemp}")
        Debug.WriteLine($"Bed Temp: {bedTemp} / {bedMaxTemp}")

        lastUpdatedLabel.Content = "Last Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    End Sub

    Private Function ExtractTempInfo(data As String) As Dictionary(Of String, Tuple(Of Integer, Integer))
        Dim tempInfo As New Dictionary(Of String, Tuple(Of Integer, Integer))()

        Dim match = Regex.Match(data, "T0:(\d+)/(\d+) B:(\d+)/(\d+)")
        If match.Success Then
            tempInfo("nozzle") = New Tuple(Of Integer, Integer)(Integer.Parse(match.Groups(1).Value), Integer.Parse(match.Groups(2).Value))
            tempInfo("bed") = New Tuple(Of Integer, Integer)(Integer.Parse(match.Groups(3).Value), Integer.Parse(match.Groups(4).Value))
        End If

        Return tempInfo
    End Function

    Private Sub Connect(server As String, message As String)
        Dim port As Integer = 8899
        Dim client As TcpClient

        Try
            client = New TcpClient(server, port)
            connectedSuccessfully = True
            connectionStatusLabel.Content = "CONNECTED"
        Catch
            connectedSuccessfully = False
            connectionStatusLabel.Content = "DISCONNECTED"
        End Try

        If connectedSuccessfully Then
            Dim data As Byte() = Encoding.ASCII.GetBytes(message)
            Dim stream As NetworkStream = client.GetStream()

            stream.Write(data, 0, data.Length)

            Dim responseDataBytes As Byte() = New Byte(256) {}
            Dim bytesRead As Integer = stream.Read(responseDataBytes, 0, responseDataBytes.Length)
            responseData = Encoding.ASCII.GetString(responseDataBytes, 0, bytesRead)

            stream.Close()
            client.Close()
        End If
    End Sub

    Private Sub textBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles textBox1.TextChanged
        printerIP = textBox1.Text
    End Sub

    Private Sub button_Click(sender As Object, e As RoutedEventArgs) Handles button.Click
        Connect(printerIP, "~M601 S1")
        RefreshPrinterData()
    End Sub




    Public Sub New()
        InitializeComponent()
        AddHandler Me.Loaded, AddressOf MainWindow_Loaded
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        NewTimerPls()
        Connect(printerIP, "~M601 S1") ' Call the Connect function
        RefreshPrinterData()           ' Call the RefreshPrinterData function
    End Sub




End Class
