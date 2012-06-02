Option Infer On
Imports System.Text
Imports System.Text.RegularExpressions
Imports HtmlAgilityPack
'Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.Linq

Public Class RosterDB
    Private Enum PlayerState
        None
        Number
        Name
        Position
        BatsThrows
        'GamesStarted
        Status
        Height
        Weight
        Born
        Birthplace
    End Enum

    'Private Const m_rosterURL As String = _
    '"http://bbdirect.stats.com/mlb/teamreports.asp?tm=#TM#&report=roster"

    'Private Const m_rosterURL As String = _
    '"http://jsonline.stats.com/mlb/teamreports.asp?tm=#TM#&report=roster"

    Private Const m_rosterURL As String = _
    "http://hosted.stats.com/mlb/teamreports.asp?tm=#TM#&report=roster"

    'Public Const g_connectionString As String = _
    '    "server=localhost; user id=rich;password=jumping; database=baseball; pooling=true"

    Public Const g_connectionString As String = _
        "Data Source=.\SQLEXPRESS;Initial Catalog=baseball;Integrated Security=True"

    Private m_sql As StringBuilder
    'Private m_conn As MySqlConnection
    Private m_conn As SqlConnection
    'Private m_insertCommand As MySqlCommand
    Private m_insertCommand As SqlCommand
    Private m_playerState As PlayerState

    Public Sub New()
        m_sql = New StringBuilder

        With m_sql
            .Append("INSERT INTO players ")
            '.Append("(Number,Name,Position,BatsThrows,GamesStarted,Status,Height,Weight,Born,Birthplace,TeamID,LastUpdate) ")
            .Append("(Number,Name,Position,BatsThrows,Status,Height,Weight,Born,Birthplace,TeamID,LastUpdate) ")
            .Append("VALUES ")
            .Append("(@Number,@Name,@Position,@BatsThrows,@Status,@Height,@Weight,@Born,@Birthplace,@TeamID,@LastUpdate)")
        End With

        Try
            'm_conn = New MySqlConnection(g_connectionString)
            'm_insertCommand = New MySqlCommand(m_sql.ToString)
            m_conn = New SqlConnection(g_connectionString)
            m_insertCommand = New SqlCommand(m_sql.ToString)

            With m_insertCommand.Parameters
                '.Add("?Number", MySqlDbType.Int32)
                '.Add("?Name", MySqlDbType.VarChar)
                '.Add("?Position", MySqlDbType.VarChar)
                '.Add("?BatsThrows", MySqlDbType.VarChar)
                ''.Add("?GamesStarted", MySqlDbType.Int32)
                '.Add("?Status", MySqlDbType.VarChar)
                '.Add("?Height", MySqlDbType.VarChar)
                '.Add("?Weight", MySqlDbType.Int32)
                '.Add("?Born", MySqlDbType.Date)
                '.Add("?Birthplace", MySqlDbType.VarChar)
                '.Add("?TeamID", MySqlDbType.Int32)
                '.Add("?LastUpdate", MySqlDbType.Datetime)
                .Add("@Number", SqlDbType.Int)
                .Add("@Name", SqlDbType.VarChar)
                .Add("@Position", SqlDbType.VarChar)
                .Add("@BatsThrows", SqlDbType.VarChar)
                '.Add("@GamesStarted", sqldbtype.Int32)
                .Add("@Status", SqlDbType.VarChar)
                .Add("@Height", SqlDbType.VarChar)
                .Add("@Weight", SqlDbType.Int)
                .Add("@Born", SqlDbType.DateTime)
                .Add("@Birthplace", SqlDbType.VarChar)
                .Add("@TeamID", SqlDbType.Int)
                .Add("@LastUpdate", SqlDbType.DateTime)
            End With

            m_conn.Open()
            m_insertCommand.Connection = m_conn
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Shared Function GetMaxTeams() As Integer
        Dim maxValue As Integer

        Try
            'Using conn As New MySqlConnection(g_connectionString)
            Using conn As New SqlConnection(g_connectionString)
                'Using command As New MySqlCommand("SELECT MAX(number) FROM teams", conn)
                Using command As New SqlCommand("SELECT MAX(number) FROM teams", conn)
                    conn.Open()
                    maxValue = CInt(command.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Return maxValue
    End Function


    Public Sub DeleteTeamPlayers()
        'Using conn As New MySqlConnection(g_connectionString)
        Using conn As New SqlConnection(g_connectionString)
            conn.Open()

            'Using command As New MySqlCommand("DELETE from players", conn)
            'Using command As New SqlCommand("DELETE from players", conn)
            '    Dim rowsAffected As Integer = command.ExecuteNonQuery()
            'End Using
            Using command As New SqlCommand("TRUNCATE TABLE players", conn)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub DeleteAllPlayers(ByVal teamNumber As Integer)
        'Using conn As New MySqlConnection(g_connectionString)
        Using conn As New SqlConnection(g_connectionString)
            conn.Open()

            'Using command As New MySqlCommand("DELETE from players WHERE TeamID=" & teamNumber.ToString, conn)
            '    Dim rowsAffected As Integer = command.ExecuteNonQuery()
            'End Using

            Using command As New SqlCommand("DELETE from players WHERE TeamID=" & teamNumber.ToString, conn)
                Dim rowsAffected As Integer = command.ExecuteNonQuery()
            End Using

        End Using
    End Sub

    Public Sub ResetAutoIncrement()
        'Using conn As New MySqlConnection(g_connectionString)
        '    conn.Open()

        '    Using command As New MySqlCommand("ALTER TABLE `baseball`.`players` AUTO_INCREMENT = 1", conn)
        '        command.ExecuteNonQuery()
        '    End Using
        'End Using
        Using conn As New SqlConnection(g_connectionString)
            conn.Open()


            Using command As New SqlCommand("TRUNCATE TABLE players", conn)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub GetTeam(ByVal teamID As Integer)
        Dim teamString As String = String.Empty
        Dim teamRosterURL As String = String.Empty
        Dim fieldName As String = String.Empty

        Dim web As HtmlWeb
        Dim doc As HtmlDocument
        Dim cols As HtmlNodeCollection

        Dim recordComplete As Boolean = False

        Dim birthDate As Date
        Dim returnValue As Boolean = False

        'Dim myTransaction As MySqlTransaction
        Dim myTransaction As SqlTransaction

        teamString = teamID.ToString("00")
        'teamString = GetTeamNumber(teamID).ToString("00")
        teamRosterURL = m_rosterURL.Replace("#TM#", teamString)

        m_playerState = PlayerState.Number

        With m_insertCommand.Parameters
            '.Item("@TeamID").Value = GetTeamNumber(teamID)
            .Item("@TeamID").Value = teamID
            .Item("@LastUpdate").Value = DateTime.Now
        End With

        Try
            web = New HtmlWeb
            doc = web.Load(teamRosterURL)
            cols = doc.DocumentNode.SelectNodes("//td[@class='shsNamD']")

            If cols IsNot Nothing Then
                'myTransaction = m_conn.BeginTransaction()
                'm_insertCommand.Transaction = myTransaction

                For Each col As HtmlNode In cols
                    Dim innerText As String = col.InnerText.Trim.Replace("&nbsp;", String.Empty)

                    'If innerText.Length > 0 AndAlso _
                    '     Not Regex.IsMatch(innerText, _
                    '    "^(No\.|Name|Pos|B/T|GS|Status|Ht|Wt|Born|Birthplace)") Then

                    If Not Regex.IsMatch(innerText, _
                        "^(No\.|Name|Pos|B/T|GS|Status|Ht|Wt|Born|Birthplace)") Then

                        Select Case m_playerState
                            Case PlayerState.Number
                                fieldName = "@Number"
                                m_playerState = PlayerState.Name

                            Case PlayerState.Name
                                fieldName = "@Name"
                                m_playerState = PlayerState.Position

                            Case PlayerState.Position
                                fieldName = "@Position"
                                m_playerState = PlayerState.BatsThrows

                            Case PlayerState.BatsThrows
                                fieldName = "@BatsThrows"
                                'm_playerState = PlayerState.GamesStarted
                                m_playerState = PlayerState.Status

                                ' rht 2008/5/31 Games started field no longer in source
                                'Case PlayerState.GamesStarted
                                '    fieldName = "@GamesStarted"
                                '    m_playerState = PlayerState.Status

                            Case PlayerState.Status
                                fieldName = "@Status"
                                m_playerState = PlayerState.Height

                            Case PlayerState.Height
                                fieldName = "@Height"
                                m_playerState = PlayerState.Weight

                            Case PlayerState.Weight
                                fieldName = "@Weight"
                                m_playerState = PlayerState.Born

                            Case PlayerState.Born
                                fieldName = "@Born"
                                m_playerState = PlayerState.Birthplace

                            Case PlayerState.Birthplace
                                fieldName = "@Birthplace"
                                m_playerState = PlayerState.Number
                                recordComplete = True

                        End Select

                        If fieldName = "@Number" OrElse _
                           fieldName = "@Weight" Then

                            If IsNumeric(innerText) Then
                                m_insertCommand.Parameters.Item(fieldName).Value = CInt(innerText)
                            Else
                                m_insertCommand.Parameters.Item(fieldName).Value = 0
                            End If

                        ElseIf fieldName = "@Born" Then
                            returnValue = Date.TryParse(innerText, birthDate)
                            'm_insertCommand.Parameters.Item(fieldName).Value = CDate(innerText)
                            If returnValue Then _
                                m_insertCommand.Parameters.Item(fieldName).Value = birthDate
                        Else
                            m_insertCommand.Parameters.Item(fieldName).Value = innerText
                        End If

                            If recordComplete Then

                                'For i As Integer = 0 To m_insertCommand.Parameters.Count - 1
                                '    Console.WriteLine("{0} : {1}", _
                                '        m_insertCommand.Parameters.Item(i).ParameterName, _
                                '        m_insertCommand.Parameters.Item(i).Value.ToString)
                                'Next

                                myTransaction = m_conn.BeginTransaction()
                                m_insertCommand.Transaction = myTransaction
                                InsertRow(myTransaction)

                                recordComplete = False
                            End If
                        End If
                Next
            End If


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub InsertRow(ByVal myTransaction As SqlTransaction)

        Try
            Dim rows As Integer = m_insertCommand.ExecuteNonQuery()

            myTransaction.Commit()
        Catch ex As Exception
            Console.WriteLine("An exception: " & ex.Message & "has occured in InsertRow.")
            Try
                myTransaction.Rollback()

                'Catch mySqlEx As MySqlException
            Catch mySqlEx As SqlException
                If Not myTransaction.Connection Is Nothing Then
                    Console.WriteLine("An exception of type " + ex.GetType().ToString() + _
                                      " was encountered while attempting to roll back the transaction.")
                End If

                MsgBox("Exception: " & ex.Message)
            End Try


        End Try
    End Sub

    Public Sub CloseConnection()
        If Not m_insertCommand Is Nothing Then
            m_insertCommand = Nothing
        End If

        If Not m_conn Is Nothing Then
            m_conn.Close()
            m_conn = Nothing
        End If
    End Sub

    Public Shared Function TranslatePosition(ByVal position As String) As Integer

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

    Public Shared Function GetTeamNumber(ByVal teamID As Integer) As Integer
        Dim bb As New BaseballDataContext

        Dim query = From t In bb.teams _
                    Where t.id = teamID _
                    Select teamNumber = t.number

        'Dim query = From t In bb.teams

        'query = query.Where(Function(t) t.id = teamID)

        Return CType(query.First, Integer)

        'Dim q = From team In bb.teams


    End Function
End Class
