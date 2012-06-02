'Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient

Public Class TeamsClass

    Private m_teamNumber As Integer
    Private m_shortName As String
    Private m_longName As String
    Private m_manager As String
    Private m_league As String

    Public Sub New(ByVal teamNumber As Integer)
        Dim sql As New System.Text.StringBuilder(96)
        sql.Append("SELECT teams.shortname, teams.longname, teams.manager, teams.league" & Environment.NewLine)
        sql.Append("FROM teams" & Environment.NewLine)
        sql.Append("WHERE teams.number=" & teamNumber.ToString)

        m_teamNumber = teamNumber

        'Using conn As New MySqlConnection(RosterDB.g_connectionString)
        Using conn As New SqlConnection(RosterDB.g_connectionString)
            conn.Open()

            'Using command As New MySqlCommand(sql.ToString, conn)
            '    Dim reader As MySqlDataReader = command.ExecuteReader(CommandBehavior.SingleRow)
            Using command As New SqlCommand(sql.ToString, conn)
                Dim reader As SqlDataReader = command.ExecuteReader(CommandBehavior.SingleRow)

                reader.Read()

                m_shortName = reader.GetString(0)
                m_longName = reader.GetString(1)
                m_manager = reader.GetString(2)
                m_league = reader.GetString(3)

                reader = Nothing
            End Using
        End Using
    End Sub

    Public ReadOnly Property TeamNumber() As Integer
        Get
            Return m_teamNumber
        End Get
    End Property

    Public ReadOnly Property TeamShortName() As String
        Get
            Return m_shortName
        End Get
    End Property

    Public ReadOnly Property TeamLongName() As String
        Get
            Return m_longName
        End Get
    End Property

    Public ReadOnly Property TeamManager() As String
        Get
            Return m_manager
        End Get
    End Property

    Public ReadOnly Property TeamLeague() As String
        Get
            Return m_league
        End Get
    End Property
End Class
