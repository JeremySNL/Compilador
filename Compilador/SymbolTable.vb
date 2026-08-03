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
    ' Pila de diccionarios: el último elemento es el ámbito actual
    Private scopes As Stack(Of Dictionary(Of String, Variable))

    Public Sub New()
        scopes = New Stack(Of Dictionary(Of String, Variable))
        PushScope() ' Ámbito global
    End Sub

    ' Crea un nuevo ámbito y lo apila
    Public Sub PushScope()
        scopes.Push(New Dictionary(Of String, Variable))
    End Sub

    ' Elimina el ámbito actual (se pierden las variables locales)
    Public Sub PopScope()
        If scopes.Count > 1 Then
            scopes.Pop()
        End If
    End Sub

    ' Declara variable en el ámbito actual
    Public Sub DeclararVariable(nombre As String, tipo As String)
        If scopes.Peek().ContainsKey(nombre) Then
            Throw New Exception("Variable '" & nombre & "' ya fue declarada en este ámbito.")
        End If
        scopes.Peek().Add(nombre, New Variable(nombre, tipo))
    End Sub

    ' Busca la variable desde el ámbito más interno hacia el global
    Public Function ObtenerVariable(nombre As String) As Variable
        For Each scope In scopes
            If scope.ContainsKey(nombre) Then
                Return scope(nombre)
            End If
        Next
        Throw New Exception("Variable '" & nombre & "' no ha sido declarada.")
    End Function

    ' Asigna valor buscando en todos los ámbitos
    Public Sub AsignarValor(nombre As String, valor As Object)
        For Each scope In scopes
            If scope.ContainsKey(nombre) Then
                Dim var = scope(nombre)
                var.Valor = valor
                var.Inicializada = True
                Return
            End If
        Next
        Throw New Exception("Variable '" & nombre & "' no ha sido declarada.")
    End Sub

    ' Verifica existencia
    Public Function VariableExiste(nombre As String) As Boolean
        For Each scope In scopes
            If scope.ContainsKey(nombre) Then
                Return True
            End If
        Next
        Return False
    End Function

    ' Verifica inicialización
    Public Function EstaInicializada(nombre As String) As Boolean
        For Each scope In scopes
            If scope.ContainsKey(nombre) Then
                Return scope(nombre).Inicializada
            End If
        Next
        Return False
    End Function
End Class
