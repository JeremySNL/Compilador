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

    Private Sub EjecutarStatement(stmt As Statement)
        If TypeOf stmt Is Declaration Then
            EjecutarDeclaracion(CType(stmt, Declaration))
        ElseIf TypeOf stmt Is Assignment Then
            EjecutarAsignacion(CType(stmt, Assignment))
        ElseIf TypeOf stmt Is PrintStatement Then
            EjecutarImprimir(CType(stmt, PrintStatement))
        End If
    End Sub

    Private Sub EjecutarDeclaracion(decl As Declaration)
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
        End If
        Throw New Exception("Expresión desconocida")
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
