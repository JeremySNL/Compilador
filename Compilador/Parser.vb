Public Class Parser
    Private tokens As List(Of Token)
    Private posicion As Integer

    Public Function Parsear(listaTokens As List(Of Token)) As ProgramNode
        tokens = listaTokens
        posicion = 0

        Dim programa As New ProgramNode()

        While TokenActual().Tipo <> TipoToken.FIN_ARCHIVO
            If TokenActual().Tipo = TipoToken.FIN_LINEA Then
                Avanzar()
            Else
                Dim statement = ParsearStatement()
                If statement IsNot Nothing Then
                    programa.Statements.Add(statement)
                End If
            End If
        End While

        Return programa
    End Function

    Private Function ParsearStatement() As Statement
        Select Case TokenActual().Tipo
            Case TipoToken.TIPO_DATO
                Return ParsearDeclaracion()
            Case TipoToken.IDENTIFICADOR
                Return ParsearAsignacionOExpresion()
            Case TipoToken.PALABRA_RESERVADA
                If TokenActual().Valor = "print" Then
                    Return ParsearImprimir()
                End If
            Case Else
                Lanzar("Statement not recognized: " & TokenActual().Valor)
        End Select
        Return Nothing
    End Function

    Private Function ParsearDeclaracion() As Declaration
        Dim tipoDato = TokenActual().Valor
        Avanzar()

        If TokenActual().Tipo <> TipoToken.IDENTIFICADOR Then
            Lanzar("Expected identifier after data type")
        End If

        Dim nombre = TokenActual().Valor
        Avanzar()

        Dim valor As Expression = Nothing
        If TokenActual().Tipo = TipoToken.ASIGNACION Then
            Avanzar()
            valor = ParsearExpresion()
        End If

        Consumir(TipoToken.FIN_LINEA, "Expected ';' at end of declaration")

        Return New Declaration(tipoDato, nombre, valor)
    End Function

    Private Function ParsearAsignacionOExpresion() As Statement
        Dim nombre = TokenActual().Valor
        Avanzar()

        If TokenActual().Tipo = TipoToken.ASIGNACION Then
            Avanzar()
            Dim valor = ParsearExpresion()
            Consumir(TipoToken.FIN_LINEA, "Expected ';' at end of assignment")
            Return New Assignment(nombre, valor)
        Else
            Lanzar("Expected '=' after identifier")
        End If

        Return Nothing
    End Function

    Private Function ParsearImprimir() As PrintStatement
        Consumir(TipoToken.PALABRA_RESERVADA, "Expected 'print'")
        Dim expr = ParsearExpresion()
        Consumir(TipoToken.FIN_LINEA, "Expected ';' at end of print")
        Return New PrintStatement(expr)
    End Function

    Private Function ParsearExpresion() As Expression
        Return ParsearSuma()
    End Function

    Private Function ParsearSuma() As Expression
        Dim izq = ParsearResta()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
              (TokenActual().Valor = "+" OrElse TokenActual().Valor = "-")
            Dim op = TokenActual().Valor
            Avanzar()
            Dim der = ParsearResta()
            izq = New BinaryOp(izq, op, der)
        End While

        Return izq
    End Function

    Private Function ParsearResta() As Expression
        Return ParsearMultiplicacion()
    End Function

    Private Function ParsearMultiplicacion() As Expression
        Dim izq = ParsearDivision()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
              (TokenActual().Valor = "*" OrElse TokenActual().Valor = "/")
            Dim op = TokenActual().Valor
            Avanzar()
            Dim der = ParsearDivision()
            izq = New BinaryOp(izq, op, der)
        End While

        Return izq
    End Function

    Private Function ParsearDivision() As Expression
        Return ParsearFactor()
    End Function

    Private Function ParsearFactor() As Expression
        If TokenActual().Tipo = TipoToken.NUMERO Then
            Dim valor = TokenActual().Valor
            Avanzar()
            If valor.Contains(".") Then
                Return New NumericLiteral(CDbl(valor))
            Else
                Return New NumericLiteral(CDbl(valor))
            End If
        End If

        If TokenActual().Tipo = TipoToken.CADENA Then
            Dim valor = TokenActual().Valor
            Avanzar()
            Return New StringLiteral(valor)
        End If

        If TokenActual().Tipo = TipoToken.IDENTIFICADOR Then
            Dim nombre = TokenActual().Valor
            Avanzar()
            Return New Identifier(nombre)
        End If

        If TokenActual().Tipo = TipoToken.PARENTESIS_IZQUIERDO Then
            Avanzar()
            Dim expr = ParsearExpresion()
            Consumir(TipoToken.PARENTESIS_DERECHO, "Expected ')'")
            Return expr
        End If

        Lanzar("Expected expression")
        Return Nothing
    End Function

    Private Function TokenActual() As Token
        If posicion >= tokens.Count Then
            Return New Token(TipoToken.FIN_ARCHIVO, "EOF", 0)
        End If
        Return tokens(posicion)
    End Function

    Private Sub Avanzar()
        If posicion < tokens.Count - 1 Then
            posicion += 1
        End If
    End Sub

    Private Sub Consumir(tipo As TipoToken, mensaje As String)
        If TokenActual().Tipo <> tipo Then
            Lanzar(mensaje)
        End If
        Avanzar()
    End Sub

    Private Sub Lanzar(mensaje As String)
        Throw New Exception("Parser error: " & mensaje & " at position " & TokenActual().Posicion)
    End Sub
End Class
