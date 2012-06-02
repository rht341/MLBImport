Imports System.Net
Imports System.IO

Public Class FileTransfer
    Private _usePassive As Boolean
    Private _useBinary As Boolean


    Public Sub New()
        _usePassive = False
        _useBinary = False
    End Sub

    Public Property UsePassive() As Boolean
        Get
            Return _usePassive
        End Get
        Set(ByVal value As Boolean)
            _usePassive = value
        End Set
    End Property

    Public Property UseBinary() As Boolean
        Get
            Return _useBinary
        End Get
        Set(ByVal value As Boolean)
            _useBinary = False
        End Set
    End Property

    Public Sub Upload(ByVal fileName As String, ByVal uploadUrl As String)
        Dim requestStream As Stream = Nothing
        Dim fileStream As FileStream = Nothing
        Dim uploadResponse As FtpWebResponse = Nothing
        Try
            Dim uploadRequest As FtpWebRequest = DirectCast(WebRequest.Create(uploadUrl), FtpWebRequest)
            uploadRequest.Method = WebRequestMethods.Ftp.UploadFile
            uploadRequest.UsePassive = _usePassive
            uploadRequest.UseBinary = _useBinary
            uploadRequest.Credentials = New NetworkCredential("freckie.com", "jumping")

            ' UploadFile is not supported through an Http proxy
            ' so we disable the proxy for this request.
            uploadRequest.Proxy = Nothing

            requestStream = uploadRequest.GetRequestStream()
            fileStream = File.Open(fileName, FileMode.Open)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer
            While True
                bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                If bytesRead = 0 Then
                    Exit While
                End If
                requestStream.Write(buffer, 0, bytesRead)
            End While

            ' The request stream must be closed before getting the response.
            requestStream.Close()

            uploadResponse = DirectCast(uploadRequest.GetResponse(), FtpWebResponse)
            Console.WriteLine("{0}:  Uploading file {1}.", DateTime.Now, Path.GetFileName(fileName))
        Catch ex As UriFormatException
            Console.WriteLine(ex.Message)
        Catch ex As IOException
            Console.WriteLine(ex.Message)
        Catch ex As WebException
            Console.WriteLine(ex.Message)
        Finally
            If uploadResponse IsNot Nothing Then
                uploadResponse.Close()
            End If
            If fileStream IsNot Nothing Then
                fileStream.Close()
            End If
            If requestStream IsNot Nothing Then
                requestStream.Close()
            End If
        End Try

    End Sub

End Class
