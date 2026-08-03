' Representa una operacion logica: &&, || o negacion unaria !
Public Class LogicalExpression
    Inherits Expression

    Public Property Operador As String       ' "&&", "||", "!"
    Public Property Izquierda As Expression  ' Nothing en caso de "!"
    Public Property Derecha As Expression    ' operando unico cuando operador es "!"

    Public Sub New(operador As String, izquierda As Expression, derecha As Expression)
        Me.Operador = operador
        Me.Izquierda = izquierda
        Me.Derecha = derecha
    End Sub
End Class
