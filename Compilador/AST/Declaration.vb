Public Class Declaration
    Inherits Statement

    Public Property TipoDato As String
    Public Property Nombre As String
    Public Property Valor As Expression

    Public Sub New(tipoDato As String, nombre As String, Optional valor As Expression = Nothing)
        Me.TipoDato = tipoDato
        Me.Nombre = nombre
        Me.Valor = valor
    End Sub
End Class
