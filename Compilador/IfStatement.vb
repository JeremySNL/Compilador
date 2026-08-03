' Representa una sentencia if-else
Public Class IfStatement
    Inherits Statement

    Public Property Condition As Expression
    Public Property ThenBody As List(Of Statement)   ' Cuerpo del if
    Public Property ElseBody As List(Of Statement)   ' Cuerpo del else (opcional)

    Public Sub New(condition As Expression, thenBody As List(Of Statement),
                   Optional elseBody As List(Of Statement) = Nothing)
        Me.Condition = condition
        Me.ThenBody = thenBody
        Me.ElseBody = elseBody
    End Sub
End Class