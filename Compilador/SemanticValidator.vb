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

        ' Verifica que el tipo de dato sea válido
        If decl.TipoDato <> "int" AndAlso decl.TipoDato <> "float" AndAlso decl.TipoDato <> "string" Then
            Throw New Exception("Tipo de dato desconocido: " & decl.TipoDato)
        End If

        ' Declara la variable en la tabla de símbolos
        symbolTable.DeclararVariable(decl.Nombre, decl.TipoDato)

        ' Si existe un valor inicial, valida su tipo y realiza la asignación
        If decl.Valor IsNot Nothing Then
            Dim tipoExpr = ObtenerTipoExpresion(decl.Valor)
            ValidarCompatibilidadTipos(decl.TipoDato, tipoExpr, "declaración de " & decl.Nombre)
            symbolTable.AsignarValor(decl.Nombre, Nothing) ' Temporal (Placeholder)
        End If

    End Sub

    Private Sub ValidarAsignacion(asign As Assignment)

        ' Verifica que la variable exista
        If Not symbolTable.VariableExiste(asign.Nombre) Then
            Throw New Exception("La variable '" & asign.Nombre & "' no ha sido declarada")
        End If

        Dim variable = symbolTable.ObtenerVariable(asign.Nombre)
        Dim tipoExpr = ObtenerTipoExpresion(asign.Valor)

        ValidarCompatibilidadTipos(variable.Tipo, tipoExpr, "asignación a " & asign.Nombre)

        symbolTable.AsignarValor(asign.Nombre, Nothing) ' Temporal (Placeholder)

    End Sub

    Private Sub ValidarImprimir(print As PrintStatement)

        ' Solo verifica que la expresión sea válida
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
                Throw New Exception("La variable '" & ident.Nombre & "' no ha sido declarada")
            End If

            If Not symbolTable.EstaInicializada(ident.Nombre) Then
                Throw New Exception("La variable '" & ident.Nombre & "' no ha sido inicializada")
            End If

            Return symbolTable.ObtenerVariable(ident.Nombre).Tipo

        ElseIf TypeOf expr Is BinaryOp Then

            Dim binop = CType(expr, BinaryOp)

            Dim tipoIzq = ObtenerTipoExpresion(binop.Izquierda)
            Dim tipoDer = ObtenerTipoExpresion(binop.Derecha)

            Return ResolverTipoBinOp(tipoIzq, tipoDer, binop.Operador)

        End If

        Throw New Exception("Tipo de expresión desconocido")

    End Function

    Private Function ResolverTipoBinOp(tipoIzq As String, tipoDer As String, op As String) As String

        ' El operador + con cadenas realiza concatenación
        If op = "+" AndAlso (tipoIzq = "string" OrElse tipoDer = "string") Then
            Return "string"
        End If

        ' Los operadores aritméticos solo pueden utilizarse con números
        If op = "+" OrElse op = "-" OrElse op = "*" OrElse op = "/" Then

            If tipoIzq = "string" OrElse tipoDer = "string" Then
                Throw New Exception("No se puede utilizar el operador '" & op & "' con cadenas")
            End If

            If tipoIzq = "float" OrElse tipoDer = "float" Then
                Return "float"
            End If

            Return "int"

        End If

        Throw New Exception("Operador no soportado: " & op)

    End Function

    Private Sub ValidarCompatibilidadTipos(tipoEsperado As String, tipoObtenido As String, contexto As String)

        ' Entre tipos numéricos se permite combinar int y float
        If (tipoEsperado = "int" OrElse tipoEsperado = "float") AndAlso
           (tipoObtenido = "int" OrElse tipoObtenido = "float") Then
            Return
        End If

        If tipoEsperado <> tipoObtenido Then
            Throw New Exception("Error de tipos en " & contexto &
                                ": se esperaba " & tipoEsperado &
                                " pero se obtuvo " & tipoObtenido)
        End If

    End Sub

    Public Function ObtenerSymbolTable() As SymbolTable
        Return symbolTable
    End Function

End Class