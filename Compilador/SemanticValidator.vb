Public Class SemanticValidator
    Private symbolTable As SymbolTable

    Public Sub New()
        symbolTable = New SymbolTable()
    End Sub

    Public Sub Validar(programa As ProgramNode)
        For Each statement In programa.Statements
            ValidarStatement(statement)
        Next
    End Sub

    Private Sub ValidarStatement(stmt As Statement)
        If TypeOf stmt Is Declaration Then
            ValidarDeclaracion(CType(stmt, Declaration))
        ElseIf TypeOf stmt Is Assignment Then
            ValidarAsignacion(CType(stmt, Assignment))
        ElseIf TypeOf stmt Is PrintStatement Then
            ValidarImprimir(CType(stmt, PrintStatement))
        End If
    End Sub

    Private Sub ValidarDeclaracion(decl As Declaration)
        ' Validate that data type is valid
        If decl.TipoDato <> "int" AndAlso decl.TipoDato <> "float" AndAlso decl.TipoDato <> "string" Then
            Throw New Exception("Unknown data type: " & decl.TipoDato)
        End If

        ' Declare variable
        symbolTable.DeclararVariable(decl.Nombre, decl.TipoDato)

        ' If there's an initial value, validate and assign
        If decl.Valor IsNot Nothing Then
            Dim tipoExpr = ObtenerTipoExpresion(decl.Valor)
            ValidarCompatibilidadTipos(decl.TipoDato, tipoExpr, "declaration of " & decl.Nombre)
            symbolTable.AsignarValor(decl.Nombre, Nothing) ' Placeholder
        End If
    End Sub

    Private Sub ValidarAsignacion(asign As Assignment)
        ' Verify that variable exists
        If Not symbolTable.VariableExiste(asign.Nombre) Then
            Throw New Exception("Variable '" & asign.Nombre & "' has not been declared")
        End If

        Dim variable = symbolTable.ObtenerVariable(asign.Nombre)
        Dim tipoExpr = ObtenerTipoExpresion(asign.Valor)

        ValidarCompatibilidadTipos(variable.Tipo, tipoExpr, "assignment to " & asign.Nombre)
        symbolTable.AsignarValor(asign.Nombre, Nothing) ' Placeholder
    End Sub

    Private Sub ValidarImprimir(print As PrintStatement)
        ' Just validate that the expression is valid
        ObtenerTipoExpresion(print.Expresion)
    End Sub

    Private Function ObtenerTipoExpresion(expr As Expression) As String
        If TypeOf expr Is NumericLiteral Then
            Return "int"
        ElseIf TypeOf expr Is StringLiteral Then
            Return "string"
        ElseIf TypeOf expr Is Identifier Then
            Dim ident = CType(expr, Identifier)
            If Not symbolTable.VariableExiste(ident.Nombre) Then
                Throw New Exception("Variable '" & ident.Nombre & "' has not been declared")
            End If
            If Not symbolTable.EstaInicializada(ident.Nombre) Then
                Throw New Exception("Variable '" & ident.Nombre & "' has not been initialized")
            End If
            Return symbolTable.ObtenerVariable(ident.Nombre).Tipo
        ElseIf TypeOf expr Is BinaryOp Then
            Dim binop = CType(expr, BinaryOp)
            Dim tipoIzq = ObtenerTipoExpresion(binop.Izquierda)
            Dim tipoDer = ObtenerTipoExpresion(binop.Derecha)
            Return ResolverTipoBinOp(tipoIzq, tipoDer, binop.Operador)
        End If
        Throw New Exception("Unknown expression type")
    End Function

    Private Function ResolverTipoBinOp(tipoIzq As String, tipoDer As String, op As String) As String
        ' Operator + with string is concatenation
        If op = "+" AndAlso (tipoIzq = "string" OrElse tipoDer = "string") Then
            Return "string"
        End If

        ' Arithmetic operator only with numbers
        If op = "+" OrElse op = "-" OrElse op = "*" OrElse op = "/" Then
            If tipoIzq = "string" OrElse tipoDer = "string" Then
                Throw New Exception("Cannot use operator '" & op & "' with string")
            End If
            If tipoIzq = "float" OrElse tipoDer = "float" Then
                Return "float"
            End If
            Return "int"
        End If

        Throw New Exception("Unsupported operator: " & op)
    End Function

    Private Sub ValidarCompatibilidadTipos(tipoEsperado As String, tipoObtenido As String, contexto As String)
        ' For numbers, allow mix of int/float
        If (tipoEsperado = "int" OrElse tipoEsperado = "float") AndAlso
           (tipoObtenido = "int" OrElse tipoObtenido = "float") Then
            Return
        End If

        If tipoEsperado <> tipoObtenido Then
            Throw New Exception("Type error in " & contexto & ": expected " & tipoEsperado & " but got " & tipoObtenido)
        End If
    End Sub

    Public Function ObtenerSymbolTable() As SymbolTable
        Return symbolTable
    End Function
End Class
