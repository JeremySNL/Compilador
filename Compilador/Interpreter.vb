Public Class Interpreter
    Private symbolTable As SymbolTable
    Public Property Salida As List(Of String)

    Public Sub New(symbolTable As SymbolTable)
        Me.symbolTable = symbolTable
        Me.Salida = New List(Of String)
    End Sub

    Public Sub Ejecutar(programa As ProgramNode)
        For Each statement In programa.Statements
            EjecutarStatement(statement)
        Next
    End Sub


    Private Sub EjecutarIf(ifStmt As IfStatement)
        Dim valorCond = EvaluarExpresion(ifStmt.Condition)
        Dim condBool As Boolean
        If TypeOf valorCond Is Boolean Then
            condBool = CBool(valorCond)
        Else
            condBool = EsVerdadero(valorCond)
        End If

        If condBool Then
            symbolTable.PushScope()
            For Each stmt In ifStmt.ThenBody
                EjecutarStatement(stmt)
            Next
            symbolTable.PopScope()
        Else
            If ifStmt.ElseBody IsNot Nothing Then
                symbolTable.PushScope()
                For Each stmt In ifStmt.ElseBody
                    EjecutarStatement(stmt)
                Next
                symbolTable.PopScope()
            End If
        End If
    End Sub




    Private Sub EjecutarStatement(stmt As Statement)
        If TypeOf stmt Is Declaration Then
            EjecutarDeclaracion(CType(stmt, Declaration))
        ElseIf TypeOf stmt Is Assignment Then
            EjecutarAsignacion(CType(stmt, Assignment))
        ElseIf TypeOf stmt Is PrintStatement Then
            EjecutarImprimir(CType(stmt, PrintStatement))
        ElseIf TypeOf stmt Is IfStatement Then
            EjecutarIf(CType(stmt, IfStatement))
        End If
    End Sub



    Private Sub EjecutarDeclaracion(decl As Declaration)
        ' Re-declaramos en el ambito actual de ejecucion.
        ' El validador pop sus scopes tras validar, por lo que el interpreter
        ' necesita declarar la variable en su propio ambito.
        If Not symbolTable.VariableExiste(decl.Nombre) Then
            symbolTable.DeclararVariable(decl.Nombre, decl.TipoDato)
        End If
        ' Si hay valor inicial, asignarlo
        If decl.Valor IsNot Nothing Then
            Dim valor = EvaluarExpresion(decl.Valor)
            symbolTable.AsignarValor(decl.Nombre, valor)
        End If
    End Sub

    Private Sub EjecutarAsignacion(asign As Assignment)
        Dim valor = EvaluarExpresion(asign.Valor)
        symbolTable.AsignarValor(asign.Nombre, valor)
    End Sub

    Private Sub EjecutarImprimir(print As PrintStatement)
        Dim valor = EvaluarExpresion(print.Expresion)
        Salida.Add("Salida: " & valor.ToString())
    End Sub

    ' =========================================================
    Private Function EvaluarExpresion(expr As Expression) As Object
        If TypeOf expr Is NumericLiteral Then
            Return CType(expr, NumericLiteral).Valor
        ElseIf TypeOf expr Is StringLiteral Then
            Return CType(expr, StringLiteral).Valor
        ElseIf TypeOf expr Is Identifier Then
            Dim ident = CType(expr, Identifier)
            Dim var = symbolTable.ObtenerVariable(ident.Nombre)
            Return var.Valor
        ElseIf TypeOf expr Is BinaryOp Then
            Dim binop = CType(expr, BinaryOp)
            Dim valIzq = EvaluarExpresion(binop.Izquierda)
            Dim valDer = EvaluarExpresion(binop.Derecha)
            Return AplicarOperador(valIzq, valDer, binop.Operador)
        ElseIf TypeOf expr Is ComparisonExpression Then
            Dim comp = CType(expr, ComparisonExpression)
            Dim valIzq = EvaluarExpresion(comp.Izquierda)
            Dim valDer = EvaluarExpresion(comp.Derecha)

            ' Comparación de strings
            If TypeOf valIzq Is String AndAlso TypeOf valDer Is String Then
                Return CompararStrings(valIzq.ToString(), valDer.ToString(), comp.Operador)
            End If

            ' Comparación numérica (convertimos ambos a Double)
            Dim numIzq = CDbl(valIzq)
            Dim numDer = CDbl(valDer)
            Return CompararNumeros(numIzq, numDer, comp.Operador)
        ElseIf TypeOf expr Is LogicalExpression Then
            Dim log = CType(expr, LogicalExpression)
            ' Negacion unaria
            If log.Operador = "!" Then
                Dim valOp = EvaluarExpresion(log.Derecha)
                Return Not EsVerdadero(valOp)
            End If
            ' Cortocircuito para && y ||
            Dim valIzqLog = EvaluarExpresion(log.Izquierda)
            If log.Operador = "&&" Then
                If Not EsVerdadero(valIzqLog) Then Return False
                Dim valDerLog = EvaluarExpresion(log.Derecha)
                Return EsVerdadero(valDerLog)
            ElseIf log.Operador = "||" Then
                If EsVerdadero(valIzqLog) Then Return True
                Dim valDerLog = EvaluarExpresion(log.Derecha)
                Return EsVerdadero(valDerLog)
            End If
            Throw New Exception("Operador lógico desconocido: " & log.Operador)
        End If
        Throw New Exception("Expresión desconocida")
    End Function

    Private Function CompararStrings(izq As String, der As String, op As String) As Boolean
        Select Case op
            Case "==" : Return izq = der
            Case "!=" : Return izq <> der
            Case "<" : Return izq < der
            Case ">" : Return izq > der
            Case "<=" : Return izq <= der
            Case ">=" : Return izq >= der
            Case Else : Throw New Exception("Operador de comparación desconocido: " & op)
        End Select
    End Function

    Private Function CompararNumeros(izq As Double, der As Double, op As String) As Boolean
        Select Case op
            Case "==" : Return izq = der
            Case "!=" : Return izq <> der
            Case "<" : Return izq < der
            Case ">" : Return izq > der
            Case "<=" : Return izq <= der
            Case ">=" : Return izq >= der
            Case Else : Throw New Exception("Operador de comparación desconocido: " & op)
        End Select
    End Function


    Private Function EsVerdadero(valor As Object) As Boolean
        If valor Is Nothing Then Return False
        If TypeOf valor Is String Then
            Return valor.ToString().Length > 0
        End If
        If TypeOf valor Is Double Then
            Return CDbl(valor) <> 0.0
        End If
        If TypeOf valor Is Integer Then
            Return CInt(valor) <> 0
        End If
        If TypeOf valor Is Boolean Then
            Return CBool(valor)
        End If
        Return True
    End Function


    Private Function AplicarOperador(izq As Object, der As Object, op As String) As Object
        ' Concatenación de strings
        If op = "+" AndAlso (TypeOf izq Is String OrElse TypeOf der Is String) Then
            Return izq.ToString() & der.ToString()
        End If

        ' Operaciones aritméticas
        Dim izqNum = CDbl(izq)
        Dim derNum = CDbl(der)

        Select Case op
            Case "+"
                Return izqNum + derNum
            Case "-"
                Return izqNum - derNum
            Case "*"
                Return izqNum * derNum
            Case "/"
                If derNum = 0 Then
                    Throw New Exception("División entre cero")
                End If
                Return izqNum / derNum
            Case Else
                Throw New Exception("Operador desconocido: " & op)
        End Select
    End Function
End Class
