Public Class Token

    Public Property Tipo As TipoToken
    Public Property Valor As String
    Public Property Posicion As Integer

    Public Sub New(tipo As TipoToken, valor As String, posicion As Integer)
        Me.Tipo = tipo
        Me.Valor = valor
        Me.Posicion = posicion
    End Sub

    Public Overrides Function ToString() As String
        Return Tipo.ToString() & " -> " & Valor & " | Posición: " & Posicion
    End Function

End Class