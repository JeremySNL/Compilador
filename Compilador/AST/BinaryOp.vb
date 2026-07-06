Public Class BinaryOp
    Inherits Expression

    Public Property Izquierda As Expression
    Public Property Operador As String
    Public Property Derecha As Expression

    Public Sub New(izquierda As Expression, operador As String, derecha As Expression)
        Me.Izquierda = izquierda
        Me.Operador = operador
        Me.Derecha = derecha
    End Sub
End Class
