Public Class ResultadoExpresion

    Public Property Tipo As String
    Public Property Valor As Object

    Public Sub New(tipo As String, valor As Object)
        Me.Tipo = tipo
        Me.Valor = valor
    End Sub

End Class