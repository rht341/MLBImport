Imports System.Data.Linq
Imports System.IO
Imports System.Xml
Imports <xmlns="urn:www.rakonza.com-scoringbaseball:roster">

Module Module1
    Private ReadOnly _krsPath As String = Path.Combine(My.Application.Info.DirectoryPath, "KRS")

    Sub Main()
        Test1()
        'Test2()
    End Sub

    Private Sub Test1()
        Dim bb As New BaseballDataContext

        'Dim t = From team In bb.teams _
        '        Order By team.longname _
        '        Select league = team.league, longName = team.longname, shortName = team.shortname

        'Dim settings As New XmlWriterSettings
        'With settings
        '    .Indent = True
        '    .Encoding = System.Text.Encoding.ASCII
        'End With

        Dim allTeams = From team In bb.teams
        Dim allPlayers = From player In bb.players _
                         Where player.Status = "Active"

        For i As Integer = 1 To 30
            Dim teamNumber As Integer = i
            Dim thisTeam = allTeams.Where(Function(t) t.number = teamNumber)
            Dim teamNameShort = thisTeam.First.shortname

            Console.WriteLine("Processing {0}...", thisTeam.First.longname)

            Dim outxml As XDocument = <?xml version="1.0" encoding="us-ascii"?>
                                      <Roster xmlns="urn:www.rakonza.com-scoringbaseball:roster" MinorVersion="4">
                                          <Team League=<%= thisTeam.First.league %> Name=<%= thisTeam.First.league %> ShortName=<%= teamNameShort %>>
                                              <%= From p In allPlayers _
                                                  Where p.TeamID = teamNumber _
                                                  Select _
                                                  <Player Name=<%= p.Name %> Number=<%= p.Number %> Bats=<%= p.BatsThrows.Substring(0, 1) %> Throws=<%= p.BatsThrows.Substring(2, 1) %> DefPos=<%= TranslatePosition(p.Position) %>></Player> %>
                                          </Team>
                                      </Roster>


            outxml.Save(Path.Combine(_krsPath, teamNameShort & ".KRS"))

            'Using XmlWriter As XmlWriter = XmlWriter.Create(Path.Combine(_krsPath, teamNameShort & ".KRS"), settings)
            '    outxml.WriteTo(XmlWriter)
            'End Using

        Next

        Console.WriteLine()
        Console.WriteLine("Done.")
        Console.ReadLine()
    End Sub

    Private Sub Test2()
        Dim bb As New BaseballDataContext

        Dim xmlPlayers = From player In bb.players

        For i As Integer = 1 To 1
            Dim current = i
            Dim oneTeam = xmlPlayers.Where(Function(t) t.TeamID = current)

            'For Each p In oneTeam
            '    Console.WriteLine("{0} {1} {2}", p.TeamID, p.Number, p.Name)
            'Next

            For Each p In oneTeam
                Dim outXML = <Player Name=<%= p.Name %>></Player>
                Console.WriteLine(outXML.ToString)
            Next

        Next

        Console.ReadLine()
    End Sub

    'Private Function GetTeamNumber(ByVal teamID As Integer) As Integer
    '    Dim bb As New BaseballDataContext

    '    Dim query = From tt In bb.teams _
    '                Where tt.id = teamID _
    '                Select teamNumber = tt.number

    '    'Dim query = From tt In bb.teams

    '    'query = query.Where(Function(t) t.id = teamID)

    '    'Return CType(query.First, Integer)
    '    Return query.First
    '    'Dim q = From team In bb.teams


    'End Function

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
