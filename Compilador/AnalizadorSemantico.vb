Public Class AnalizadorSemantico

    Private tokens As List(Of Token)
    Private posicionActual As Integer
    Private tablaSimbolos As New Dictionary(Of String, Simbolo)
    Public Sub Analizar(listaTokens As List(Of Token))

        tokens = listaTokens
        posicionActual = 0

        While TokenActual().Tipo <> TipoToken.FIN_ARCHIVO

            If TokenActual().Tipo = TipoToken.FIN_LINEA Then
                Avanzar()

            ElseIf TokenActual().Tipo = TipoToken.TIPO_DATO Then
                AnalizarDeclaracion()

            ElseIf TokenActual().Tipo = TipoToken.IDENTIFICADOR Then
                AnalizarAsignacion()

            ElseIf TokenActual().Tipo = TipoToken.PALABRA_RESERVADA AndAlso TokenActual().Valor = "imprimir" Then
                AnalizarImprimir()

            Else
                LanzarError("Instrucción no reconocida.")
            End If

        End While

    End Sub

    Private Sub AnalizarDeclaracion()

        Dim tipoDato As String = TokenActual().Valor
        Avanzar()

        Dim nombreVariable As String = TokenActual().Valor

        If tablaSimbolos.ContainsKey(nombreVariable) Then
            LanzarError("La variable '" & nombreVariable & "' ya fue declarada.")
        End If

        Dim nuevoSimbolo As New Simbolo(nombreVariable, tipoDato)

        tablaSimbolos.Add(nombreVariable, nuevoSimbolo)

        Avanzar()

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la declaración.")

    End Sub

    Private Sub AnalizarAsignacion()

        Dim nombreVariable As String = TokenActual().Valor

        If Not tablaSimbolos.ContainsKey(nombreVariable) Then
            LanzarError("La variable '" & nombreVariable & "' no ha sido declarada.")
        End If

        Dim simboloVariable As Simbolo = tablaSimbolos(nombreVariable)

        Avanzar()

        Consumir(TipoToken.ASIGNACION, "Se esperaba '='.")

        Dim resultadoExpresion As ResultadoExpresion = AnalizarExpresion()

        If simboloVariable.Tipo <> resultadoExpresion.Tipo Then
            LanzarError("No se puede asignar una expresión de tipo '" &
                    resultadoExpresion.Tipo & "' a la variable '" &
                    nombreVariable & "' de tipo '" &
                    simboloVariable.Tipo & "'.")
        End If

        simboloVariable.Valor = resultadoExpresion.Valor
        simboloVariable.Inicializada = True

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la asignación.")

    End Sub

    Private Sub AnalizarImprimir()

        Avanzar()

        Dim resultado As ResultadoExpresion = AnalizarExpresion()

        Console.WriteLine("Salida imprimir: " & resultado.Valor)

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de imprimir.")

    End Sub

    Private Function AnalizarExpresion() As ResultadoExpresion

        Dim resultado As ResultadoExpresion = AnalizarTermino()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
          (TokenActual().Valor = "+" OrElse TokenActual().Valor = "-")

            Dim operador As String = TokenActual().Valor
            Avanzar()

            Dim derecha As ResultadoExpresion = AnalizarTermino()

            resultado = AplicarOperacion(resultado, derecha, operador)

        End While

        Return resultado

    End Function

    Private Function AnalizarTermino() As ResultadoExpresion

        Dim resultado As ResultadoExpresion = AnalizarFactor()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO AndAlso
          (TokenActual().Valor = "*" OrElse TokenActual().Valor = "/")

            Dim operador As String = TokenActual().Valor
            Avanzar()

            Dim derecha As ResultadoExpresion = AnalizarFactor()

            resultado = AplicarOperacion(resultado, derecha, operador)

        End While

        Return resultado

    End Function

    Private Function AnalizarFactor() As ResultadoExpresion

        If TokenActual().Tipo = TipoToken.NUMERO Then

            Dim valor As String = TokenActual().Valor
            Avanzar()

            If valor.Contains(".") Then
                Return New ResultadoExpresion("real", Convert.ToDouble(valor))
            Else
                Return New ResultadoExpresion("entero", Convert.ToInt32(valor))
            End If

        ElseIf TokenActual().Tipo = TipoToken.CADENA Then

            Dim valorCadena As String = TokenActual().Valor
            Avanzar()

            Return New ResultadoExpresion("texto", valorCadena)

        ElseIf TokenActual().Tipo = TipoToken.IDENTIFICADOR Then

            Dim nombreVariable As String = TokenActual().Valor

            If Not tablaSimbolos.ContainsKey(nombreVariable) Then
                LanzarError("La variable '" & nombreVariable & "' no ha sido declarada.")
            End If

            Dim simbolo As Simbolo = tablaSimbolos(nombreVariable)

            If Not simbolo.Inicializada Then
                LanzarError("La variable '" & nombreVariable & "' fue declarada, pero no tiene valor asignado.")
            End If

            Avanzar()

            Return New ResultadoExpresion(simbolo.Tipo, simbolo.Valor)

        ElseIf TokenActual().Tipo = TipoToken.PARENTESIS_IZQUIERDO Then

            Avanzar()

            Dim resultadoInterno As ResultadoExpresion = AnalizarExpresion()

            Consumir(TipoToken.PARENTESIS_DERECHO, "Se esperaba ')'.")

            Return resultadoInterno

        Else

            LanzarError("Se esperaba un número, cadena, identificador o expresión entre paréntesis.")

        End If

        Return New ResultadoExpresion("desconocido", Nothing)

    End Function

    Private Function AplicarOperacion(izquierda As ResultadoExpresion,
                                  derecha As ResultadoExpresion,
                                  operador As String) As ResultadoExpresion

        If izquierda.Tipo = "texto" OrElse derecha.Tipo = "texto" Then

            If operador <> "+" Then
                LanzarError("No se puede usar el operador '" & operador & "' con texto.")
            End If

            Return New ResultadoExpresion("texto", izquierda.Valor.ToString() & derecha.Valor.ToString())

        End If

        If izquierda.Tipo = "real" OrElse derecha.Tipo = "real" Then

            Dim izq As Double = Convert.ToDouble(izquierda.Valor)
            Dim der As Double = Convert.ToDouble(derecha.Valor)

            Return New ResultadoExpresion("real", OperarNumeros(izq, der, operador))

        Else

            Dim izq As Integer = Convert.ToInt32(izquierda.Valor)
            Dim der As Integer = Convert.ToInt32(derecha.Valor)

            Return New ResultadoExpresion("entero", CInt(OperarNumeros(izq, der, operador)))

        End If

    End Function

    Private Function OperarNumeros(izquierda As Double, derecha As Double, operador As String) As Double

        Select Case operador
            Case "+"
                Return izquierda + derecha
            Case "-"
                Return izquierda - derecha
            Case "*"
                Return izquierda * derecha
            Case "/"
                If derecha = 0 Then
                    LanzarError("No se puede dividir entre cero.")
                End If

                Return izquierda / derecha
            Case Else
                LanzarError("Operador no reconocido: " & operador)
        End Select

        Return 0

    End Function

    Private Function TokenActual() As Token
        Return tokens(posicionActual)
    End Function

    Private Sub Avanzar()
        If posicionActual < tokens.Count - 1 Then
            posicionActual += 1
        End If
    End Sub

    Private Sub Consumir(tipoEsperado As TipoToken, mensajeError As String)

        If TokenActual().Tipo = tipoEsperado Then
            Avanzar()
        Else
            LanzarError(mensajeError)
        End If

    End Sub

    Private Sub LanzarError(mensaje As String)

        Throw New Exception(
            "Error semántico en posición " &
            TokenActual().Posicion &
            ": " &
            mensaje &
            " Token encontrado: '" &
            TokenActual().Valor &
            "'"
        )

    End Sub

End Class