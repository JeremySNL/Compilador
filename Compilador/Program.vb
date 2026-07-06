Imports System
Imports System.IO

Module Program

    Sub Main()
        Console.Clear()
        Console.WriteLine("================================")
        Console.WriteLine("COMPILADOR v1.0")
        Console.WriteLine("================================")
        Console.WriteLine()

        ' Ask for debug mode
        Console.WriteLine("¿Activar modo debug? (s/n):")
        Dim debugInput = Console.ReadLine().ToLower()
        Dim debugMode = (debugInput = "s" Or debugInput = "si" Or debugInput = "yes" Or debugInput = "y")

        Console.WriteLine()
        Console.WriteLine("Ingrese la ruta del archivo (o presione Enter para 'program.txt'):")
        Dim filePath = Console.ReadLine()

        If String.IsNullOrWhiteSpace(filePath) Then
            filePath = "C:\CompiladorLP\Compilador\Compilador\program.txt"
        End If

        ' Check if file exists
        If Not File.Exists(filePath) Then
            Console.WriteLine()
            Console.WriteLine("✗ Error: Archivo no encontrado: " & filePath)
            Console.WriteLine("Presione cualquier tecla para salir...")
            Console.ReadKey()
            Return
        End If

        ' Read file content
        Dim codigo As String
        Try
            codigo = File.ReadAllText(filePath)
        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine("✗ Error al leer el archivo: " & ex.Message)
            Console.WriteLine("Presione cualquier tecla para salir...")
            Console.ReadKey()
            Return
        End Try

        ' Compile and run
        Console.WriteLine()
        Console.WriteLine("================================")
        Console.WriteLine("RESULTADO DE LA COMPILACIÓN")
        Console.WriteLine("================================")
        Console.WriteLine()

        Dim compiler As New Compiler(debugMode)
        Dim exitoso = compiler.Compilar(codigo)

        For Each linea In compiler.Salida
            Console.WriteLine(linea)
        Next

        Console.WriteLine()
        Console.WriteLine("================================")
        Console.WriteLine("Presione cualquier tecla para salir...")
        Console.ReadKey()
    End Sub

End Module