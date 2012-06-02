Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Xml
Imports FileHelpers
Imports HtmlAgilityPack
'Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Imports <xmlns="urn:www.rakonza.com-scoringbaseball:roster">

Imports Microsoft.VisualBasic.FileIO


Module MainModule

    Private ReadOnly _csvPath As String = Path.Combine(My.Application.Info.DirectoryPath, "CSV")
    Private ReadOnly _krsPath As String = Path.Combine(My.Application.Info.DirectoryPath, "KRS")
    Private ReadOnly _temPath As String = Path.Combine(My.Application.Info.DirectoryPath, "TEM")
    Private ReadOnly _pdbPath As String = Path.Combine(My.Application.Info.DirectoryPath, "PDB")
    Private ReadOnly _retroPath As String = Path.Combine(My.Application.Info.DirectoryPath, "Retrosheet")

    Private Const _csvConverter As String = "C:\Program Files\Baseball ScoreBook with PDA Companion\bbsbcsv2pdbtem.exe"

    Private Const _csvUploadPath As String = "ftp://ftp.freckie.com/wwwroot/mlb/csv"
    Private Const _krsUploadPath As String = "ftp://ftp.freckie.com/wwwroot/mlb/xml"
    Private Const _temUploadPath As String = "ftp://ftp.freckie.com/wwwroot/mlb/tem"
    Private Const _pdbUploadPath As String = "ftp://ftp.freckie.com/wwwroot/mlb/pdb"
    Private Const _retroUploadPath As String = "ftp://ftp.freckie.com/wwwroot/mlb/Retrosheet"

    'Private _conn As MySqlConnection
    'Private _command As MySqlCommand
    'Private _reader As MySqlDataReader
    Private _reader As SqlDataReader

    Private _maxTeams As Integer

    'Private _allTeams As Boolean = False
    'Private _generateCSV As Boolean = False
    'Private _generateXML As Boolean = False
    'Private _uploadFiles As Boolean = False
    'Private _noConsole As Boolean = False
    'Private _generateRetro As Boolean = False

    Sub Main(ByVal cmdArgs As String())
        Dim args As New List(Of String)(cmdArgs)
        If args.Count = 0 Then
            MsgBox("Must supply parameters")
            Exit Sub
        End If

        'GetArgs(args)
        Dim options As MLBOptions = MLBOptions.CreateInstance()
        options.ParseCommandline(args)

        _maxTeams = RosterDB.GetMaxTeams()

        CheckOutputFolders()

        Dim sr As StreamWriter = Nothing
        If options.NoConsole Then
            sr = New StreamWriter(Path.Combine(My.Application.Info.DirectoryPath, "MLBImport.log"))
            Console.SetOut(sr)
        End If

        If options.AllTeams Then
            GetAllTeams()
        End If

        If options.GenerateXML Then
            GenerateXML()
        End If

        If options.GenerateCSV Then
            GenerateCSV()
            'GenerateTEM()
            'GeneratePDB()
        End If

        If options.GenerateRetro Then
            GenerateRetro()
        End If

        If options.UploadFiles Then
            UploadFiles()
        End If

        'System.Windows.Forms.MessageBox.Show("Done.", "Finished", Windows.Forms.MessageBoxButtons.OK, Windows.Forms.MessageBoxIcon.Information)
        Console.WriteLine("{0}Done.", Environment.NewLine)

        If Not options.NoConsole Then
            Console.ReadLine()
        Else
            sr.Flush()
            sr.Close()
            sr = Nothing
        End If

    End Sub

    Private Sub CheckOutputFolders()
        If Not Directory.Exists(_csvPath) Then
            Directory.CreateDirectory(_csvPath)
        End If

        If Not Directory.Exists(_krsPath) Then
            Directory.CreateDirectory(_krsPath)
        End If

        If Not Directory.Exists(_temPath) Then
            Directory.CreateDirectory(_temPath)
        End If

        If Not Directory.Exists(_pdbPath) Then
            Directory.CreateDirectory(_pdbPath)
        End If

        If Not Directory.Exists(_retroPath) Then
            Directory.CreateDirectory(_retroPath)
        End If
    End Sub

    'Private Sub GetArgs(ByVal args As List(Of String))
    '    If args.Count = 0 OrElse args.Contains("all") OrElse args.Contains("ALL") Then
    '        _allTeams = True
    '    End If

    '    If args.Contains("noconsole") OrElse args.Contains("NOCONSOLE") Then
    '        _noConsole = True
    '    End If

    '    If args.Contains("csv") OrElse args.Contains("CSV") Then
    '        _generateCSV = True
    '    End If

    '    If args.Contains("xml") OrElse args.Contains("XML") Then
    '        _generateXML = True
    '    End If

    '    If args.Contains("ftp") OrElse args.Contains("FTP") Then
    '        _uploadFiles = True
    '    End If

    '    If args.Contains("retro") OrElse args.Contains("RETRO") Then
    '        _generateRetro = True
    '    End If
    'End Sub

    Private Sub GetAllTeams()
        Dim myRosterDB As New RosterDB

        Console.WriteLine("{0}:  Total number of teams = {1}", DateTime.Now, _maxTeams)

        myRosterDB.DeleteTeamPlayers()
        'myRosterDB.ResetAutoIncrement()

        For i As Integer = 1 To _maxTeams
            Console.WriteLine("{0}:  Processing team {1}...", DateTime.Now, i)
            myRosterDB.GetTeam(i)
        Next


        myRosterDB.CloseConnection()
        Console.WriteLine("{0}:  Done updating database.", DateTime.Now)
        Console.WriteLine()
        'Console.ReadLine()
        myRosterDB = Nothing
    End Sub

    Private Sub GenerateCSVOld()
        Dim arr As New List(Of Roster)
        Dim ros As Roster = Nothing
        Dim mySQL As New StringBuilder()

        mySQL.Append("SELECT players.Name, players.Number, " & Environment.NewLine)
        mySQL.Append("players.Position, players.BatsThrows, players.Status, players.Height, players.Weight, players.Born, ")
        mySQL.Append("players.Birthplace" & Environment.NewLine)
        mySQL.Append("FROM players" & Environment.NewLine)
        mySQL.Append("WHERE players.TeamID=@TeamNumber" & Environment.NewLine)

        Try
            'Using conn As New MySqlConnection(RosterDB.g_connectionString)
            Using conn As New SqlConnection(RosterDB.g_connectionString)
                conn.Open()

                'Using command As New MySqlCommand(mySQL.ToString, conn)
                Using command As New SqlCommand(mySQL.ToString, conn)
                    'Dim reader As MySqlDataReader
                    Dim reader As SqlDataReader
                    'command.Parameters.Add("?TeamNumber", MySqlDbType.Int32)
                    command.Parameters.Add("@TeamNumber", SqlDbType.Int)

                    For i As Integer = 1 To _maxTeams
                        'Dim myTeam As New TeamsClass(i)
                        Dim myTeam As New TeamsClass(RosterDB.GetTeamNumber(i))

                        arr.Add(New Roster(myTeam.TeamLongName))
                        arr.Add(New Roster("Manager", myTeam.TeamManager))

                        'command.Parameters.Item(0).Value = i
                        command.Parameters.Item(0).Value = RosterDB.GetTeamNumber(i)

                        reader = command.ExecuteReader()

                        Console.WriteLine("{0}:  Writing csv for {1}...", DateTime.Now, myTeam.TeamLongName)

                        While reader.Read
                            ros = New Roster
                            With ros
                                .Name = reader.GetString(0)
                                .PlayerNumber = reader.GetInt32(1).ToString
                                .Position = reader.GetString(2)
                                .SetBatsThrows(reader.GetString(3))
                                .Status = reader.GetString(4)
                                .Height = reader.GetString(5)
                                .Weight = reader.GetInt32(6).ToString
                                .DateOfBirth = reader.GetDateTime(7).ToString
                                .BirthPlace = reader.GetString(8)
                                .TeamID = i
                            End With

                            If ros.Status = "Active" Then
                                arr.Add(ros)
                            End If
                        End While

                        Dim engine As New FileHelperEngine(Of Roster)()
                        engine.WriteFile(Path.Combine(_csvPath, myTeam.TeamShortName & ".csv"), arr.ToArray)

                        myTeam = Nothing
                        reader.Close()
                        arr.Clear()
                    Next

                    arr = Nothing
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Console.WriteLine("{0}:  Done generating csv.", DateTime.Now)
        Console.WriteLine()
    End Sub

    Private Sub GenerateCSV()
        Dim db As New BaseballDataContext

        Dim allPlayers = From player In db.players _
                         Where player.Status = "Active" _
                         Join team In db.teams On player.TeamID Equals team.number _
                         Order By player.Number _
                         Select New With {player.Name, _
                                           player.Number, _
                                           player.Status, _
                                           player.Height, _
                                           player.Weight, _
                                           player.Born, _
                                           player.Birthplace, _
                                           player.Position, _
                                           player.BatsThrows, _
                                           .teamNumber = team.number, _
                                           team.shortname, _
                                           team.longname}


        For i As Integer = 1 To _maxTeams
            Dim ros As Roster
            Dim rosterList As New List(Of Roster)
            Dim currentTeam As Integer = i
            Dim thisTeam = allPlayers.Where(Function(t) t.teamNumber = currentTeam)
            Dim teamNameShort As String = thisTeam.First.shortname
            Dim teamNameLong As String = thisTeam.First.longname

            Console.WriteLine("{0}:  Writing csv for {1}...", DateTime.Now, teamNameLong)

            For Each p In thisTeam
                ros = New Roster

                With ros
                    .Name = p.Name
                    .PlayerNumber = p.Number.ToString
                    .Position = p.Position
                    .Bats = p.BatsThrows.Substring(0, 1)
                    .Throws = p.BatsThrows.Substring(2, 1)
                    .Status = p.Status
                    .Height = p.Height
                    .Weight = p.Weight.ToString
                    .DateOfBirth = p.Born.ToString("MM/dd/yyyy")
                    .BirthPlace = p.Birthplace
                    .TeamID = currentTeam
                End With

                rosterList.Add(ros)
            Next

            Dim engine As New FileHelperEngine(Of Roster)()
            engine.WriteFile(Path.Combine(_csvPath, teamNameShort & ".csv"), rosterList)


            rosterList.Clear()

        Next
    End Sub

    'Private Sub GenerateXMLOld()
    '    Dim mySQL As String = String.Empty
    '    Dim outputFile As String = String.Empty

    '    Dim myXMLSettings As New XmlWriterSettings

    '    With myXMLSettings
    '        .Indent = True
    '        .Encoding = Encoding.GetEncoding("us-ascii")
    '    End With



    '    mySQL = "SELECT Name, Number, Position, BatsThrows FROM players WHERE TeamID=@TeamNumber AND Status='Active'"

    '    Dim bats As String = String.Empty
    '    Dim throws As String = String.Empty

    '    Using conn As New SqlConnection(RosterDB.g_connectionString)
    '        conn.Open()

    '        Using command As New SqlCommand(mySQL, conn)
    '            Dim reader As SqlDataReader
    '            command.Parameters.Add("@TeamNumber", SqlDbType.Int)


    '            For i As Integer = 1 To _maxTeams
    '                Dim myTeam As New TeamsClass(i)

    '                outputFile = Path.Combine(_krsPath, myTeam.TeamShortName & ".KRS")

    '                'command.Parameters.Item(0).Value = RosterDB.GetTeamNumber(i)
    '                command.Parameters.Item(0).Value = i

    '                reader = command.ExecuteReader

    '                Console.WriteLine("{0}:  Writing xml for {1}...", DateTime.Now, myTeam.TeamLongName)

    '                Using myXMLWriter As XmlWriter = XmlWriter.Create(outputFile, myXMLSettings)
    '                    myXMLWriter.WriteStartDocument()
    '                    myXMLWriter.WriteStartElement("Roster")
    '                    myXMLWriter.WriteAttributeString("xmlns", "x", Nothing, "urn:www.rakonza.com-scoringbaseball:roster")
    '                    myXMLWriter.WriteAttributeString("MinorVersion", "4")

    '                    myXMLWriter.WriteStartElement("Team")
    '                    myXMLWriter.WriteAttributeString("League", myTeam.TeamLeague)
    '                    myXMLWriter.WriteAttributeString("Name", myTeam.TeamLongName)
    '                    myXMLWriter.WriteAttributeString("ShortName", myTeam.TeamShortName)

    '                    While reader.Read
    '                        'bats = reader.GetString("BatsThrows").Substring(0, 1).ToUpper
    '                        'throws = reader.GetString("BatsThrows").Substring(2, 1).ToUpper
    '                        ' TODO: Make enums out of column names
    '                        bats = reader.GetString(3).Substring(0, 1).ToUpper
    '                        throws = reader.GetString(3).Substring(2, 1).ToUpper

    '                        myXMLWriter.WriteStartElement("Player")
    '                        myXMLWriter.WriteAttributeString("Name", reader.GetString(0))
    '                        myXMLWriter.WriteAttributeString("Number", reader.GetInt32(1).ToString)
    '                        myXMLWriter.WriteAttributeString("Bats", bats)
    '                        myXMLWriter.WriteAttributeString("Throws", throws)
    '                        ' TODO: Make enums out of column names
    '                        'myXMLWriter.WriteAttributeString("DefPos", _
    '                        '                                RosterDB.TranslatePosition(reader.GetString("Position")).ToString)
    '                        myXMLWriter.WriteAttributeString("DefPos", _
    '                                                        RosterDB.TranslatePosition(reader.GetString(2)).ToString)
    '                        myXMLWriter.WriteEndElement()  'Player
    '                    End While


    '                    myXMLWriter.WriteEndElement()  'Team

    '                    myXMLWriter.WriteEndElement()  'Roster

    '                    myXMLWriter.WriteEndDocument()

    '                    myXMLWriter.Flush()
    '                    myXMLWriter.Close()

    '                End Using

    '                FixXML(outputFile)

    '                reader.Close()
    '            Next
    '        End Using
    '    End Using

    '    Console.WriteLine("{0}:  Done generating krs (xml).", DateTime.Now)
    '    Console.WriteLine()
    'End Sub

    Private Sub GenerateXML()
        Dim bb As New BaseballDataContext

        Dim allTeams = From team In bb.teams
        Dim allPlayers = From player In bb.players _
                         Where player.Status = "Active"

        For i As Integer = 1 To _maxTeams
            Dim teamNumber As Integer = i
            Dim thisTeam = allTeams.Where(Function(t) t.number = teamNumber)
            Dim teamNameShort = thisTeam.First.shortname
            Dim teamNameLong = thisTeam.First.longname

            Console.WriteLine("Processing {0}...", teamNameLong)

            Dim outxml = <?xml version="1.0" encoding="us-ascii"?>
                         <Roster xmlns="urn:www.rakonza.com-scoringbaseball:roster" MinorVersion="4">
                             <Team League=<%= thisTeam.First.league %> Name=<%= teamNameLong %> ShortName=<%= teamNameShort %>>
                                 <%= From p In allPlayers _
                                     Where p.TeamID = teamNumber _
                                     Order By p.Number _
                                     Select _
                                     <Player Name=<%= p.Name %> Number=<%= p.Number %> Bats=<%= p.BatsThrows.Substring(0, 1) %> Throws=<%= p.BatsThrows.Substring(2, 1) %> DefPos=<%= TranslatePosition(p.Position) %>></Player> %>
                             </Team>
                         </Roster>

            outxml.Save(Path.Combine(_krsPath, teamNameShort & ".KRS"))
        Next

        Console.WriteLine()
        Console.WriteLine("Done.")
    End Sub

    'Private Sub FixXML(ByVal fileName As String)
    '    Dim readerFile As String = _
    '                My.Computer.FileSystem.ReadAllText(fileName)

    '    readerFile = readerFile.Replace("xmlns:x", "xmlns")

    '    My.Computer.FileSystem.WriteAllText(fileName, readerFile, False)
    'End Sub


    Private Sub UploadFiles()
        Dim myFileTransfer As New FileTransfer
        myFileTransfer.UsePassive = True
        myFileTransfer.UseBinary = False

        For Each file As String In Directory.GetFiles(_csvPath, "*.csv", IO.SearchOption.TopDirectoryOnly)
            myFileTransfer.Upload(file, _
                Path.Combine(_csvUploadPath, Path.GetFileName(file)))
        Next

        For Each file As String In Directory.GetFiles(_krsPath, "*.krs", IO.SearchOption.TopDirectoryOnly)
            myFileTransfer.Upload(file, _
                Path.Combine(_krsUploadPath, Path.GetFileName(file)))
        Next

        myFileTransfer.UseBinary = True
        For Each file As String In Directory.GetFiles(_temPath, "*.tem", IO.SearchOption.TopDirectoryOnly)
            myFileTransfer.Upload(file, _
                Path.Combine(_temUploadPath, Path.GetFileName(file)))
        Next

        For Each file As String In Directory.GetFiles(_pdbPath, "*.pdb", IO.SearchOption.TopDirectoryOnly)
            myFileTransfer.Upload(file, _
                Path.Combine(_pdbUploadPath, Path.GetFileName(file)))
        Next

        Console.WriteLine("{0}:  File uploads complete.", DateTime.Now)
        Console.WriteLine()

        myFileTransfer = Nothing
    End Sub

    Private Sub GenerateTEM()
        Dim temFile As String = String.Empty
        Dim myProcess As New Process

        myProcess.StartInfo.CreateNoWindow = True
        myProcess.StartInfo.FileName = _csvConverter

        For Each csvFile As String In Directory.GetFiles(_csvPath, "*.csv", IO.SearchOption.TopDirectoryOnly)
            temFile = Path.ChangeExtension(Path.GetFileName(csvFile), ".tem")
            temFile = Path.Combine(_temPath, temFile)

            myProcess.StartInfo.Arguments = String.Format("""{0}"" ""{1}""", csvFile, temFile)

            Try
                myProcess.Start()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Next

        Console.WriteLine("{0}:  Done generating tem.", DateTime.Now)
        Console.WriteLine()

        myProcess = Nothing
    End Sub

    Private Sub GeneratePDB()
        Dim pdbFile As String = String.Empty
        Dim myProcess As New Process

        myProcess.StartInfo.CreateNoWindow = True
        myProcess.StartInfo.FileName = _csvConverter

        For Each csvFile As String In Directory.GetFiles(_csvPath, "*.csv", IO.SearchOption.TopDirectoryOnly)
            pdbFile = Path.ChangeExtension(Path.GetFileName(csvFile), ".pdb")
            pdbFile = Path.Combine(_pdbPath, pdbFile)

            myProcess.StartInfo.Arguments = String.Format("""{0}"" ""{1}""", csvFile, pdbFile)

            Try
                myProcess.Start()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Next

        myProcess = Nothing

    End Sub

    Private Sub GenerateRetroOld()
        Dim arr As New List(Of Retrosheet)
        Dim retro As Retrosheet = Nothing
        Dim mySQL As New StringBuilder()

        mySQL.Append("SELECT players.Name, players.Position, players.BatsThrows, teams.shortname " & Environment.NewLine)
        mySQL.Append("FROM players INNER JOIN teams ON players.TeamID = teams.number " & Environment.NewLine)
        mySQL.Append("WHERE players.TeamID=@TeamNumber AND " & Environment.NewLine)
        mySQL.Append("      players.status='Active' " & Environment.NewLine)
        mySQL.Append("ORDER BY players.Name")

        Try
            'Using conn As New MySqlConnection(RosterDB.g_connectionString)
            Using conn As New SqlConnection(RosterDB.g_connectionString)
                conn.Open()

                'Using command As New MySqlCommand(mySQL.ToString, conn)
                '    Dim reader As MySqlDataReader
                Using command As New SqlCommand(mySQL.ToString, conn)
                    Dim reader As SqlDataReader
                    'command.Parameters.Add("?TeamNumber", MySqlDbType.Int32)
                    command.Parameters.Add("@TeamNumber", SqlDbType.Int)

                    For i As Integer = 1 To _maxTeams
                        'Dim myTeam As New TeamsClass(i)

                        'command.Parameters.Item(0).Value = i

                        'reader = command.ExecuteReader()
                        'Dim myTeam As New TeamsClass(RosterDB.GetTeamNumber(i))
                        Dim myTeam As New TeamsClass(i)

                        'command.Parameters.Item(0).Value = RosterDB.GetTeamNumber(i)
                        command.Parameters.Item(0).Value = i

                        reader = command.ExecuteReader()

                        Console.WriteLine("{0}:  Writing retrosheet for {1}...", DateTime.Now, myTeam.TeamLongName)

                        While reader.Read
                            retro = New Retrosheet
                            With retro
                                'Dim fullName As String = reader.GetString(0)
                                'Console.WriteLine("Processing {0}...", reader.GetString(0))
                                .SplitName(reader.GetString(0))
                                .Position = reader.GetString(1)
                                .GetBatsThrows(reader.GetString(2))
                                .Team = reader.GetString(3)
                            End With

                            arr.Add(retro)
                        End While

                        Dim engine As New FileHelperEngine(Of Retrosheet)()
                        engine.WriteFile(Path.Combine(_retroPath, myTeam.TeamShortName & ".ros"), arr.ToArray)

                        myTeam = Nothing
                        reader.Close()
                        arr.Clear()

                    Next

                    arr = Nothing
                End Using
            End Using
        Catch ex As Exception
            MsgBox("Exception: " & ex.Message)
        End Try
    End Sub

    Private Sub GenerateRetro()
        Dim db As New BaseballDataContext

        Dim allPlayers = From player In db.players _
                         Where player.Status = "Active" _
                         Join team In db.teams On player.TeamID Equals team.number _
                         Order By player.Name _
                         Select New With {player.Name, _
                                          player.Position, _
                                          player.BatsThrows, _
                                          team.number, _
                                          team.shortname, _
                                          team.longname}


        For i As Integer = 1 To _maxTeams
            'Dim myTeam As New TeamsClass(i)
            Dim retro As Retrosheet
            Dim retroList As New List(Of Retrosheet)
            Dim currentTeam As Integer = i
            Dim thisTeam = allPlayers.Where(Function(t) t.number = currentTeam)
            Dim teamNameShort As String = thisTeam.First.shortname
            Dim teamNameLong As String = thisTeam.First.longname

            Console.WriteLine("{0}:  Writing retrosheet for {1}...", DateTime.Now, teamNameLong)

            For Each p In thisTeam
                retro = New Retrosheet

                With retro
                    .SplitName(p.Name)
                    .Position = p.Position
                    .GetBatsThrows(p.BatsThrows)
                    .Team = p.shortname
                End With

                retroList.Add(retro)
            Next

            Dim engine As New FileHelperEngine(Of Retrosheet)()
            engine.WriteFile(Path.Combine(_retroPath, teamNameShort & ".ros"), retroList)


            retroList.Clear()

        Next
    End Sub

    Private Function TranslatePosition(ByVal position As String) As Integer

        Select Case position.ToUpper
            Case "P"
                Return 1
            Case "C"
                Return 2
            Case "1B"
                Return 3
            Case "2B"
                Return 4
            Case "3B"
                Return 5
            Case "SS"
                Return 6
            Case "LF"
                Return 7
            Case "CF"
                Return 8
            Case "RF"
                Return 9
            Case Else
                Return 0
        End Select
    End Function
End Module
