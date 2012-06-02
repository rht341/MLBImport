Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Imports FileHelpers

<DelimitedRecord(",")> <System.Runtime.InteropServices.ComVisible(False)> _
Public NotInheritable Class Roster

    Private m_number As String
    Private m_name As String
    Private m_position As String
    Private m_battingOrder As String
    Private m_bats As String
    Private m_throws As String

    <FieldIgnored()> _
    Private m_teamID As Integer
    <FieldIgnored()> _
    Private m_status As String
    <FieldIgnored()> _
    Private m_height As String
    <FieldIgnored()> _
    Private m_weight As String
    <FieldIgnored()> _
    Private m_dateOfBirth As String
    <FieldIgnored()> _
    Private m_birthPlace As String


    Public Sub New()
        m_battingOrder = "0"
    End Sub

    Public Sub New(ByVal teamName As String)
        m_number = teamName
    End Sub

    Public Sub New(ByVal coachType As String, ByVal coachName As String)
        m_number = coachType
        m_name = coachName
    End Sub

    Public Property PlayerNumber() As String
        Get
            Return m_number
        End Get
        Set(ByVal value As String)
            m_number = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = value
        End Set
    End Property

    Public Property Position() As String
        Get
            Return m_position
        End Get
        Set(ByVal value As String)
            m_position = value
        End Set
    End Property

    Public Property BattingOrder() As String
        Get
            Return m_battingOrder
        End Get
        Set(ByVal value As String)
            m_battingOrder = value
        End Set
    End Property

    Public Property Bats() As String
        Get
            Return m_bats
        End Get
        Set(ByVal value As String)
            m_bats = value
        End Set
    End Property

    Public Property Throws() As String
        Get
            Return m_throws
        End Get
        Set(ByVal value As String)
            m_throws = value
        End Set
    End Property

    Public Property TeamID() As Integer
        Get
            Return m_teamID
        End Get
        Set(ByVal value As Integer)
            m_teamID = value
        End Set
    End Property

    Public Property Status() As String
        Get
            Return m_status
        End Get
        Set(ByVal value As String)
            m_status = value
        End Set
    End Property

    Public Property Height() As String
        Get
            Return m_height
        End Get
        Set(ByVal value As String)
            m_height = value
        End Set
    End Property

    Public Property Weight() As String
        Get
            Return m_weight
        End Get
        Set(ByVal value As String)
            m_weight = value
        End Set
    End Property

    Public Sub SetBatsThrows(ByVal batsThrows As String)
        Dim regex As Regex = New Regex("(?<Bats>.)/(?<Throws>.)", RegexOptions.IgnoreCase)
        Dim ms As Match = regex.Match(batsThrows)

        m_bats = ms.Groups("Bats").ToString & "H"
        m_throws = ms.Groups("Throws").ToString & "H"
    End Sub

    Public Property DateOfBirth() As String
        Get
            Return m_dateOfBirth
        End Get
        Set(ByVal value As String)
            m_dateOfBirth = value
        End Set
    End Property

    Public Property BirthPlace() As String
        Get
            Return m_birthPlace
        End Get
        Set(ByVal value As String)
            m_birthPlace = value
        End Set
    End Property
End Class