Public Class AnalizadorSintactico

    Private tokens As List(Of Token)
    Private posicionActual As Integer

    Public Sub Analizar(listaTokens As List(Of Token))

        tokens = listaTokens
        posicionActual = 0

        While TokenActual().Tipo <> TipoToken.FIN_ARCHIVO

            If TokenActual().Tipo = TipoToken.FIN_LINEA Then
                Avanzar()
            Else
                AnalizarInstruccion()
            End If

        End While

    End Sub

    Private Sub AnalizarInstruccion()

        If TokenActual().Tipo = TipoToken.TIPO_DATO Then

            AnalizarDeclaracion()

        ElseIf TokenActual().Tipo = TipoToken.IDENTIFICADOR Then

            AnalizarAsignacion()

        ElseIf TokenActual().Tipo = TipoToken.PALABRA_RESERVADA AndAlso TokenActual().Valor = "imprimir" Then

            AnalizarImprimir()

        Else

            LanzarError("Se esperaba una declaración, asignación o instrucción imprimir.")

        End If

    End Sub

    Private Sub AnalizarDeclaracion()

        Consumir(TipoToken.TIPO_DATO, "Se esperaba un tipo de dato.")

        Consumir(TipoToken.IDENTIFICADOR, "Se esperaba un identificador después del tipo de dato.")

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la declaración.")

    End Sub

    Private Sub AnalizarAsignacion()

        Consumir(TipoToken.IDENTIFICADOR, "Se esperaba un identificador.")

        Consumir(TipoToken.ASIGNACION, "Se esperaba '=' después del identificador.")

        AnalizarExpresion()

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la asignación.")

    End Sub

    Private Sub AnalizarImprimir()

        Consumir(TipoToken.PALABRA_RESERVADA, "Se esperaba la palabra reservada 'imprimir'.")

        AnalizarExpresion()

        Consumir(TipoToken.FIN_LINEA, "Se esperaba ';' al final de la instrucción imprimir.")

    End Sub

    Private Sub AnalizarExpresion()

        AnalizarTermino()

        While TokenActual().Tipo = TipoToken.OPERADOR_ARITMETICO
            Avanzar()
            AnalizarTermino()
        End While

    End Sub

    Private Sub AnalizarTermino()

        If TokenActual().Tipo = TipoToken.IDENTIFICADOR OrElse
           TokenActual().Tipo = TipoToken.NUMERO OrElse
           TokenActual().Tipo = TipoToken.CADENA Then

            Avanzar()

        ElseIf TokenActual().Tipo = TipoToken.PARENTESIS_IZQUIERDO Then

            Avanzar()
            AnalizarExpresion()
            Consumir(TipoToken.PARENTESIS_DERECHO, "Se esperaba ')'.")

        Else

            LanzarError("Se esperaba un identificador, número, cadena o expresión entre paréntesis.")

        End If

    End Sub

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
            "Error sintáctico en posición " &
            TokenActual().Posicion &
            ": " &
            mensaje &
            " Token encontrado: '" &
            TokenActual().Valor &
            "'"
        )

    End Sub

End Class