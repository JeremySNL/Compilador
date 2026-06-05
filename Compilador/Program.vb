Imports System

Module Program

    Sub Main()

        While True
            Console.WriteLine("Ingrese una instrucción:")

            Dim codigo As String = Console.ReadLine()

            Dim lexer As New AnalizadorLexico()

            Dim tokens = lexer.Analizar(codigo)

            Console.WriteLine()

            Console.WriteLine("TOKENS ENCONTRADOS:")

            For Each token In tokens
                Console.WriteLine(token.ToString())
            Next

            Console.ReadLine()

            Dim parser As New AnalizadorSintactico()

            Try

                parser.Analizar(tokens)

                Console.WriteLine()
                Console.WriteLine("Análisis sintáctico correcto.")

            Catch ex As Exception

                Console.WriteLine()
                Console.WriteLine(ex.Message)

            End Try

            Dim semantico As New AnalizadorSemantico()

            Try
                semantico.Analizar(tokens)
                Console.WriteLine("Análisis semántico correcto.")
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Console.WriteLine()

        End While

    End Sub

End Module