Public Class Variable
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
End Class

Public Class SymbolTable
    Private variables As Dictionary(Of String, Variable)

    Public Sub New()
        variables = New Dictionary(Of String, Variable)
    End Sub

    Public Sub DeclararVariable(nombre As String, tipo As String)
        If variables.ContainsKey(nombre) Then
            Throw New Exception("Variable '" & nombre & "' has already been declared")
        End If
        variables.Add(nombre, New Variable(nombre, tipo))
    End Sub

    Public Function ObtenerVariable(nombre As String) As Variable
        If Not variables.ContainsKey(nombre) Then
            Throw New Exception("Variable '" & nombre & "' has not been declared")
        End If
        Return variables(nombre)
    End Function

    Public Sub AsignarValor(nombre As String, valor As Object)
        Dim variable = ObtenerVariable(nombre)
        variable.Valor = valor
        variable.Inicializada = True
    End Sub

    Public Function VariableExiste(nombre As String) As Boolean
        Return variables.ContainsKey(nombre)
    End Function

    Public Function EstaInicializada(nombre As String) As Boolean
        If Not variables.ContainsKey(nombre) Then
            Return False
        End If
        Return variables(nombre).Inicializada
    End Function
End Class
