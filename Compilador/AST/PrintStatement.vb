Public Class PrintStatement
    Inherits Statement

    Public Property Expresion As Expression

    Public Sub New(expresion As Expression)
        Me.Expresion = expresion
    End Sub
End Class
