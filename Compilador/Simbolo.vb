Public Class Simbolo

    Public Property Nombre As String
    Public Property Tipo As String
    Public Property Valor As Object
    Public Property Inicializada As Boolean

    Public Sub New(nombre As String, tipo As String)
        Me.Nombre = nombre
        Me.Tipo = tipo
        Me.Valor = Nothing
        Me.Inicializada = False
    End Sub

    Public Overrides Function ToString() As String
        Return Nombre & " | Tipo: " & Tipo & " | Valor: " & If(Valor Is Nothing, "NULL", Valor.ToString()) & " | Inicializada: " & Inicializada
    End Function

End Class