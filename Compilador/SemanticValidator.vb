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
        ElseIf TypeOf stmt Is IfStatement Then
            ValidarIf(CType(stmt, IfStatement))
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


    Private Function ObtenerTipoExpresion(expr As Expression) As String
        If TypeOf expr Is NumericLiteral Then
            Return "int"  ' Lo tratamos como int, en runtime puede ser float
        ElseIf TypeOf expr Is StringLiteral Then
            Return "string"
        ElseIf TypeOf expr Is Identifier Then
            Dim ident = CType(expr, Identifier)
            If Not symbolTable.VariableExiste(ident.Nombre) Then
                Throw New Exception("Variable '" & ident.Nombre & "' no ha sido declarada.")
            End If
            If Not symbolTable.EstaInicializada(ident.Nombre) Then
                Throw New Exception("Variable '" & ident.Nombre & "' no ha sido inicializada.")
            End If
            Return symbolTable.ObtenerVariable(ident.Nombre).Tipo
        ElseIf TypeOf expr Is BinaryOp Then
            Dim binop = CType(expr, BinaryOp)
            Dim tipoIzq = ObtenerTipoExpresion(binop.Izquierda)
            Dim tipoDer = ObtenerTipoExpresion(binop.Derecha)
            Return ResolverTipoBinOp(tipoIzq, tipoDer, binop.Operador)
        ElseIf TypeOf expr Is ComparisonExpression Then
            Dim comp = CType(expr, ComparisonExpression)
            Dim tipoIzq = ObtenerTipoExpresion(comp.Izquierda)
            Dim tipoDer = ObtenerTipoExpresion(comp.Derecha)
            ' Permitimos comparar números (int/float) entre sí, y strings con strings
            If (tipoIzq = "int" OrElse tipoIzq = "float") AndAlso (tipoDer = "int" OrElse tipoDer = "float") Then
                Return "bool"
            End If
            If tipoIzq = "string" AndAlso tipoDer = "string" Then
                Return "bool"
            End If
            Throw New Exception("No se puede comparar " & tipoIzq & " con " & tipoDer)
        ElseIf TypeOf expr Is LogicalExpression Then
            Dim log = CType(expr, LogicalExpression)
            ' Negacion unaria
            If log.Operador = "!" Then
                Dim tipoOp = ObtenerTipoExpresion(log.Derecha)
                If tipoOp <> "bool" Then
                    Throw New Exception("El operador '!' solo se puede aplicar a expresiones booleanas, se obtuvo " & tipoOp)
                End If
                Return "bool"
            End If
            ' Operadores binarios && y ||
            Dim tipoIzqLog = ObtenerTipoExpresion(log.Izquierda)
            Dim tipoDerLog = ObtenerTipoExpresion(log.Derecha)
            If tipoIzqLog <> "bool" OrElse tipoDerLog <> "bool" Then
                Throw New Exception("Los operadores '" & log.Operador & "' solo se aplican a expresiones booleanas (se obtuvo " & tipoIzqLog & " y " & tipoDerLog & ")")
            End If
            Return "bool"
        End If
        Throw New Exception("Tipo de expresión desconocido")
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


    Private Sub ValidarIf(ifStmt As IfStatement)
        ' Validamos la condición: debe ser de tipo bool
        Dim tipoCond = ObtenerTipoExpresion(ifStmt.Condition)
        If tipoCond <> "bool" Then
            Throw New Exception("La condición del 'if' debe ser booleana, se obtuvo " & tipoCond)
        End If

        ' Entramos a un nuevo ámbito para el cuerpo del if
        symbolTable.PushScope()
        For Each stmt In ifStmt.ThenBody
            ValidarStatement(stmt)
        Next
        symbolTable.PopScope()

        ' Si hay else, otro nuevo ámbito
        If ifStmt.ElseBody IsNot Nothing Then
            symbolTable.PushScope()
            For Each stmt In ifStmt.ElseBody
                ValidarStatement(stmt)
            Next
            symbolTable.PopScope()
        End If
    End Sub

    Public Function ObtenerSymbolTable() As SymbolTable
        Return symbolTable
    End Function

End Class