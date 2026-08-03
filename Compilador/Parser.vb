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

                Dim sentencia = ParsearStatement()

                If sentencia IsNot Nothing Then
                    programa.Statements.Add(sentencia)
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
                ElseIf TokenActual().Valor = "if" Then
                    Return ParsearIf()
                End If
            Case Else
                Lanzar("Statement not recognized: " & TokenActual().Valor)
        End Select
        Return Nothing
    End Function

    Private Function ParsearIf() As IfStatement
        ' Consumimos la palabra 'if' (ya sabemos que estamos en ella, pero avanzamos)
        Avanzar() ' Consume "if"

        ' Esperamos '('
        Consumir(TipoToken.PARENTESIS_IZQUIERDO, "Expected '(' after if")

        ' Parseamos la condicion (puede incluir && y ||)
        Dim condition = ParsearOr()

        ' Esperamos ')'
        Consumir(TipoToken.PARENTESIS_DERECHO, "Expected ')' after condition")

        ' Parseamos el bloque del then
        Dim thenBody = ParsearBloque()

        ' Verificamos si hay 'else'
        Dim elseBody As List(Of Statement) = Nothing
        If TokenActual().Tipo = TipoToken.PALABRA_RESERVADA AndAlso TokenActual().Valor = "else" Then
            Avanzar() ' Consume "else"
            elseBody = ParsearBloque()
        End If

        Return New IfStatement(condition, thenBody, elseBody)
    End Function

    Private Function ParsearBloque() As List(Of Statement)
        ' Esperamos '{'
        Consumir(TipoToken.LLAVE_IZQUIERDA, "Expected '{'")

        Dim statements As New List(Of Statement)

        ' Mientras no lleguemos a '}'
        While TokenActual().Tipo <> TipoToken.LLAVE_DERECHA AndAlso TokenActual().Tipo <> TipoToken.FIN_ARCHIVO
            ' Saltamos puntos y coma sueltos (por si hay l�neas vac�as)
            If TokenActual().Tipo = TipoToken.FIN_LINEA Then
                Avanzar()
                Continue While
            End If

            Dim stmt = ParsearStatement()
            If stmt IsNot Nothing Then
                statements.Add(stmt)
            End If
        End While

        ' Esperamos '}'
        Consumir(TipoToken.LLAVE_DERECHA, "Expected '}'")

        Return statements
    End Function



    Private Function ParsearDeclaracion() As Declaration

        Dim tipoDato = TokenActual().Valor
        Avanzar()

        If TokenActual().Tipo <> TipoToken.IDENTIFICADOR Then
            Lanzar("Se esperaba un identificador despu�s del tipo de dato.")
        End If

        Dim nombre = TokenActual().Valor
        Avanzar()

        Dim valor As Expression = Nothing

        If TokenActual().Tipo = TipoToken.ASIGNACION Then
            Avanzar()
            valor = ParsearExpresion()
        End If

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la declaraci�n.")

        Return New Declaration(tipoDato, nombre, valor)

    End Function

    Private Function ParsearAsignacionOExpresion() As Statement

        Dim nombre = TokenActual().Valor
        Avanzar()

        If TokenActual().Tipo = TipoToken.ASIGNACION Then

            Avanzar()

            Dim valor = ParsearExpresion()

            Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la asignaci�n.")

            Return New Assignment(nombre, valor)

        Else

            Lanzar("Se esperaba '=' despu�s del identificador.")

        End If

        Return Nothing

    End Function

    Private Function ParsearImprimir() As PrintStatement

        Consumir(TipoToken.PALABRA_RESERVADA, "Se esperaba la palabra reservada 'print'.")

        Dim expresion = ParsearExpresion()

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la instrucci�n print.")

        Return New PrintStatement(expresion)

    End Function

    Private Function ParsearExpresion() As Expression
        Return ParsearOr()
    End Function

    Private Function ParsearComparacion() As Expression
        Dim izq = ParsearSuma()
        ' Solo se permite UNA comparacion encadenada.
        ' Expresiones como "a < b < c" daran error de sintaxis.
        If TokenActual().Tipo = TipoToken.OP_RELACIONAL Then
            Dim op = TokenActual().Valor
            Avanzar()
            Dim der = ParsearSuma()
            izq = New ComparisonExpression(izq, op, der)
        End If
        Return izq
    End Function

    Private Function ParsearNot() As Expression
        If TokenActual().Tipo = TipoToken.OP_LOGICO AndAlso TokenActual().Valor = "!" Then
            Avanzar()
            Dim operando = ParsearNot()
            Return New LogicalExpression("!", Nothing, operando)
        End If
        Return ParsearComparacion()
    End Function

    Private Function ParsearAnd() As Expression
        Dim opAnd As String = ChrW(38) & ChrW(38)
        Dim izq = ParsearNot()
        While TokenActual().Tipo = TipoToken.OP_LOGICO
            If TokenActual().Valor <> opAnd Then Exit While
            Avanzar()
            Dim der = ParsearNot()
            izq = New LogicalExpression(opAnd, izq, der)
        End While
        Return izq
    End Function

    Private Function ParsearOr() As Expression
        Dim opOr As String = ChrW(124) & ChrW(124)
        Dim izq = ParsearAnd()
        While TokenActual().Tipo = TipoToken.OP_LOGICO
            If TokenActual().Valor <> opOr Then Exit While
            Avanzar()
            Dim der = ParsearAnd()
            izq = New LogicalExpression(opOr, izq, der)
        End While
        Return izq
    End Function

    Private Function ParsearSuma() As Expression

        Dim izquierda = ParsearMultiplicacion()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
          (TokenActual().Valor = "+" OrElse TokenActual().Valor = "-")

            Dim operador = TokenActual().Valor

            Avanzar()

            Dim derecha = ParsearMultiplicacion()

            izquierda = New BinaryOp(izquierda, operador, derecha)

        End While

        Return izquierda

    End Function

    Private Function ParsearMultiplicacion() As Expression

        Dim izquierda = ParsearFactor()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
          (TokenActual().Valor = "*" OrElse TokenActual().Valor = "/")

            Dim operador = TokenActual().Valor

            Avanzar()

            Dim derecha = ParsearFactor()

            izquierda = New BinaryOp(izquierda, operador, derecha)

        End While

        Return izquierda

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

            Dim expresion = ParsearExpresion()

            Consumir(TipoToken.PARENTESIS_DERECHO, "Se esperaba ')'.")

            Return expresion

        End If

        Lanzar("Se esperaba una expresi�n.")

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

        Throw New Exception(
            "Error sint�ctico: " &
            mensaje &
            " en la posici�n " &
            TokenActual().Posicion
        )

    End Sub

End Class