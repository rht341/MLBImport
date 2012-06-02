Imports FileHelpers
Imports System.Text.RegularExpressions

<DelimitedRecord(",")> _
Public NotInheritable Class Retrosheet


    Private mRetroID As String

    Public Property RetroID() As String
        Get
            Return mRetroID
        End Get
        Set(ByVal value As String)
            mRetroID = value
        End Set
    End Property


    Private mLastName As String

    Public Property LastName() As String
        Get
            Return mLastName
        End Get
        Set(ByVal value As String)
            mLastName = value
        End Set
    End Property


    Private mFirstName As String

    Public Property FirstName() As String
        Get
            Return mFirstName
        End Get
        Set(ByVal value As String)
            mFirstName = value
        End Set
    End Property


    Private mBats As String

    Public Property Bats() As String
        Get
            Return mBats
        End Get
        Set(ByVal value As String)
            mBats = value
        End Set
    End Property


    Private mThrows As String

    Public Property Throws() As String
        Get
            Return mThrows
        End Get
        Set(ByVal value As String)
            mThrows = value
        End Set
    End Property


    Private mTeam As String

    Public Property Team() As String
        Get
            Return mTeam
        End Get
        Set(ByVal value As String)
            mTeam = value
        End Set
    End Property


    Private mPosition As String

    Public Property Position() As String
        Get
            Return mPosition
        End Get
        Set(ByVal value As String)
            mPosition = value
        End Set
    End Property

    Public Sub SplitName(ByVal fullName As String)
        Dim regex As Regex = New Regex("(?<First>.+)\s(?<Last>.+)", RegexOptions.IgnoreCase)
        Dim ms As Match = regex.Match(fullName)

        mFirstName = ms.Groups("First").ToString
        mLastName = ms.Groups("Last").ToString

        CreateID()
    End Sub

    Public Sub GetBatsThrows(ByVal batsThrows As String)
        Dim batsThrowsA() As String = batsThrows.Split("/"c)

        mBats = batsThrowsA(0)
        mThrows = batsThrowsA(1)
    End Sub

    Private Sub CreateID()
        Dim sb As New System.Text.StringBuilder

        If mLastName.Length >= 4 Then
            sb.Append(mLastName.Substring(0, 4))
        Else
            sb.Append(mLastName & "-")
        End If

        sb.Append(mFirstName.Substring(0, 1))
        sb.Append("001")
        mRetroID = sb.ToString.ToLower
    End Sub
End Class