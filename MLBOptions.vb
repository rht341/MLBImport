Imports System.Collections.Generic

Public Class MLBOptions
    Private Shared obj As Object = New Object

    Private Shared _mlbOptions As MLBOptions

    Private _allTeams As Boolean
    Private _generateCSV As Boolean
    Private _generateXML As Boolean
    Private _uploadFiles As Boolean
    Private _noConsole As Boolean
    Private _generateRetro As Boolean

    Private Sub New()
        _allTeams = False
        _generateCSV = False
        _generateXML = False
        _uploadFiles = False
        _noConsole = False
        _generateRetro = False
    End Sub

    Public Shared Function CreateInstance() As MLBOptions
        If _mlbOptions Is Nothing Then
            SyncLock (obj)
                _mlbOptions = New MLBOptions
            End SyncLock
        End If

        Return _mlbOptions
    End Function

    Public ReadOnly Property AllTeams() As Boolean
        Get
            Return _allTeams
        End Get
    End Property

    Public ReadOnly Property GenerateCSV() As Boolean
        Get
            Return _generateCSV
        End Get
    End Property

    Public ReadOnly Property GenerateXML() As Boolean
        Get
            Return _generateXML
        End Get
    End Property

    Public ReadOnly Property GenerateRetro() As Boolean
        Get
            Return _generateRetro
        End Get
    End Property

    Public ReadOnly Property UploadFiles() As Boolean
        Get
            Return _uploadFiles
        End Get
    End Property

    Public ReadOnly Property NoConsole() As Boolean
        Get
            Return _noConsole
        End Get
    End Property

    Public Sub ParseCommandline(ByVal args As List(Of String))
        If args.Count = 0 OrElse args.Contains("all") OrElse args.Contains("ALL") Then
            _allTeams = True
        End If

        If args.Contains("noconsole") OrElse args.Contains("NOCONSOLE") Then
            _noConsole = True
        End If

        If args.Contains("csv") OrElse args.Contains("CSV") Then
            _generateCSV = True
        End If

        If args.Contains("xml") OrElse args.Contains("XML") Then
            _generateXML = True
        End If

        If args.Contains("ftp") OrElse args.Contains("FTP") Then
            _uploadFiles = True
        End If

        If args.Contains("retro") OrElse args.Contains("RETRO") Then
            _generateRetro = True
        End If
    End Sub
End Class
