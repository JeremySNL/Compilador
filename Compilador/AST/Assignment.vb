Public Class Assignment
    Inherits Statement

    Public Property Nombre As String
    Public Property Valor As Expression

    Public Sub New(nombre As String, valor As Expression)
        Me.Nombre = nombre
        Me.Valor = valor
    End Sub
End Class
