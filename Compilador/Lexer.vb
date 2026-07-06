Imports System.Text

Public Class Lexer

    Public Function Analizar(codigo As String) As List(Of Token)

        Dim tokens As New List(Of Token)
        Dim i As Integer = 0

        While i < codigo.Length

            Dim caracter As Char = codigo(i)

            If Char.IsWhiteSpace(caracter) Then
                i += 1

            ElseIf Char.IsLetter(caracter) Or caracter = "_"c Then

                Dim inicio As Integer = i
                Dim palabra As New StringBuilder()

                While i < codigo.Length AndAlso
                    (Char.IsLetterOrDigit(codigo(i)) Or codigo(i) = "_"c)

                    palabra.Append(codigo(i))
                    i += 1

                End While

                Dim texto As String = palabra.ToString().ToLower()

                If texto = "int" Or texto = "float" Or texto = "string" Then
                    tokens.Add(New Token(TipoToken.TIPO_DATO, texto, inicio))

                ElseIf texto = "print" Then
                    tokens.Add(New Token(TipoToken.PALABRA_RESERVADA, texto, inicio))

                Else
                    tokens.Add(New Token(TipoToken.IDENTIFICADOR, texto, inicio))
                End If

            ElseIf Char.IsDigit(caracter) Then

                Dim inicio As Integer = i
                Dim numero As New StringBuilder()
                Dim tienePunto As Boolean = False

                While i < codigo.Length AndAlso
                    (Char.IsDigit(codigo(i)) Or codigo(i) = "."c)

                    If codigo(i) = "."c Then
                        If tienePunto Then
                            tokens.Add(New Token(TipoToken.ERROR_404, numero.ToString() & ".", inicio))
                            i += 1
                            Exit While
                        End If

                        tienePunto = True
                    End If

                    numero.Append(codigo(i))
                    i += 1

                End While

                tokens.Add(New Token(TipoToken.NUMERO, numero.ToString(), inicio))

            ElseIf caracter = """"c Then

                Dim inicio As Integer = i
                i += 1

                Dim cadena As New StringBuilder()

                While i < codigo.Length AndAlso codigo(i) <> """"c
                    cadena.Append(codigo(i))
                    i += 1
                End While

                If i < codigo.Length AndAlso codigo(i) = """"c Then
                    i += 1
                    tokens.Add(New Token(TipoToken.CADENA, cadena.ToString(), inicio))
                Else
                    tokens.Add(New Token(TipoToken.ERROR_404, "Cadena sin cerrar", inicio))
                End If

            ElseIf caracter = "="c Then

                tokens.Add(New Token(TipoToken.ASIGNACION, "=", i))
                i += 1

            ElseIf caracter = "+"c Or caracter = "-"c Or caracter = "*"c Or caracter = "/"c Then

                tokens.Add(New Token(TipoToken.OPERADOR_ARITMETICO, caracter.ToString(), i))
                i += 1

            ElseIf caracter = "("c Then

                tokens.Add(New Token(TipoToken.PARENTESIS_IZQUIERDO, caracter.ToString(), i))
                i += 1

            ElseIf caracter = ")"c Then

                tokens.Add(New Token(TipoToken.PARENTESIS_DERECHO, caracter.ToString(), i))
                i += 1

            ElseIf caracter = ";"c Then

                tokens.Add(New Token(TipoToken.FIN_LINEA, caracter.ToString(), i))
                i += 1

            Else

                tokens.Add(New Token(TipoToken.ERROR_404, caracter.ToString(), i))
                i += 1

            End If

        End While

        tokens.Add(New Token(TipoToken.FIN_ARCHIVO, "EOF", codigo.Length))

        Return tokens

    End Function

End Class