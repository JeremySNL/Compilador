Public Class Compiler
    Private lexer As AnalizadorLexico
    Public Property Salida As List(Of String)
    Public Property DebugMode As Boolean

    Public Sub New(Optional debugMode As Boolean = True)
        lexer = New AnalizadorLexico()
        Salida = New List(Of String)
        Me.DebugMode = debugMode
    End Sub

    Public Function Compilar(codigo As String) As Boolean
        Salida.Clear()

        Try
            ' Phase 1: Lexer
            If DebugMode Then Salida.Add("=== FASE 1: ANÁLISIS LÉXICO ===")
            Dim tokens = lexer.Analizar(codigo)
            If DebugMode Then
                Salida.Add("Tokens encontrados: " & tokens.Count)
                For Each token In tokens
                    If token.Tipo <> TipoToken.FIN_ARCHIVO Then
                        Salida.Add("  " & token.ToString())
                    End If
                Next
                Salida.Add("")
            End If

            ' Phase 2: Parser
            If DebugMode Then Salida.Add("=== FASE 2: ANÁLISIS SINTÁCTICO ===")
            Dim parser As New Parser()
            Dim programa = parser.Parsear(tokens)
            If DebugMode Then
                Salida.Add("Programa parseado con " & programa.Statements.Count & " sentencias")
                Salida.Add("")
            End If

            ' Phase 3: Semantic Validation
            If DebugMode Then Salida.Add("=== FASE 3: ANÁLISIS SEMÁNTICO ===")
            Dim validator As New SemanticValidator()
            validator.Validar(programa)
            If DebugMode Then
                Salida.Add("Validación semántica exitosa")
                Salida.Add("")
            End If

            ' Phase 4: Interpreter (Execution)
            If DebugMode Then Salida.Add("=== FASE 4: EJECUCIÓN ===")
            Dim interpreter As New Interpreter(validator.ObtenerSymbolTable())
            interpreter.Ejecutar(programa)
            For Each linea In interpreter.Salida
                Salida.Add(linea)
            Next
            If DebugMode Then Salida.Add("")

            Salida.Add("✓ Compilación exitosa")
            Return True

        Catch ex As Exception
            Salida.Add("✗ Error: " & ex.Message)
            Return False
        End Try
    End Function
End Class