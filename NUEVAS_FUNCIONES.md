# Nuevas funciones: `if/else` y operadores logicos

Cambios introducidos en el commit `9304326` sobre la rama `agregado_funciones`.
Este documento explica **unicamente lo que se anadio**; las caracteristicas
preexistentes quedan fuera de su alcance.

---

## Perspectiva lexica

El lexer es la primera fase del compilador: convierte el codigo fuente en una
secuencia de tokens. En este commit se anadieron cuatro piezas nuevas.

### 1. `if` y `else` como palabra reservada

Antes, unicamente `print` se reconocia como `PALABRA_RESERVADA`. Cualquier otra
palabra formada por letras -incluidas `if` y `else`- caia en la rama de
`IDENTIFICADOR`. Esto provocaba que el parser nunca pudiese entrar en
`ParsearIf`, asi que el condicional era inalcanzable aunque el codigo
estuviese bien escrito.

Despues del fix, la condicion del lexer queda asi:

```vb
If texto = "int" Or texto = "float" Or texto = "string" Then
    tokens.Add(New Token(TipoToken.TIPO_DATO, texto, inicio))
ElseIf texto = "print" OrElse texto = "if" OrElse texto = "else" Then
    tokens.Add(New Token(TipoToken.PALABRA_RESERVADA, texto, inicio))
Else
    tokens.Add(New Token(TipoToken.IDENTIFICADOR, texto, inicio))
End If
```

### 2. Operadores `&&`, `||`, `!`

Se anadio el token `OP_LOGICO` al enumerado `TipoToken` y se incorporaron tres
nuevas ramas al lexer:

- `&` seguido de `&` -> token `OP_LOGICO` con lexema `&&`.
- `|` seguido de `|` -> token `OP_LOGICO` con lexema `||`.
- `!` solo -> token `OP_LOGICO` con lexema `!` (negacion unaria).
- `!` seguido de `=` sigue siendo `OP_RELACIONAL` con lexema `!=`.

### Ejemplo

Para la entrada `if (x > 5 && y == 0)`, el lexer emite:

```
PALABRA_RESERVADA  if
PARENTESIS_IZQUIERDO  (
IDENTIFICADOR     x
OP_RELACIONAL     >
NUMERO            5
OP_LOGICO         &&
IDENTIFICADOR     y
OP_RELACIONAL     ==
NUMERO            0
PARENTESIS_DERECHO  )
```

---

## Perspectiva sintactica

El parser convierte la lista de tokens en un arbol AST. En este commit se
anadieron las reglas para producir sentencias `if/else` y expresiones
logicas, y se endurezio el tratamiento de comparaciones.

### 1. Nodo nuevo: `LogicalExpression.vb`

Se creo un nodo especifico para `&&`, `||` y `!`, separado del
`ComparisonExpression`. La estructura es:

```vb
Public Class LogicalExpression
    Inherits Expression

    Public Property Operador As String       ' "&&", "||", "!"
    Public Property Izquierda As Expression  ' Nothing cuando es "!"
    Public Property Derecha As Expression    ' operando unico cuando es "!"
End Class
```

### 2. Sentencia `if` y bloques `{ ... }`

Se anadieron `ParsearIf` y `ParsearBloque` al parser. `ParsearIf` consume el
token `if`, exige parentesis alrededor de la condicion y delega en
`ParsearBloque` para leer el cuerpo delimitado por llaves. Si aparece `else`,
se parsea un segundo bloque.

```vb
ElseIf TokenActual().Valor = "if" Then
    Return ParsearIf()
```

### 3. Jerarquia de expresiones

`ParsearExpresion` ahora delega en `ParsearOr`, que es la cabeza de una
cadena de precedencia descendente:

```
ParsearOr       -> ||
ParsearAnd      -> &&
ParsearNot      -> ! unario
ParsearComparacion -> == != < > <= >=
ParsearSuma     -> + -
ParsearMultiplicacion -> * /
ParsearFactor   -> literales, identificador, ( expr )
```

Esto permite escribir expresiones como `if (a > 0 && (b < 10 || c == 0))`,
donde la negacion y los operadores logicos tienen la precedencia esperada.

### 4. Comparaciones encadenadas prohibidas

`ParsearComparacion` cambio de `While` a `If`: solo admite una comparacion
por nivel. Una expresion como `1 < 2 < 3` ya no se interpreta como
`(1 < 2) < 3`; ahora el parser exige `)` o cualquier otro token que cierre
la condicion del `if`, y si lo siguiente es otra `OP_RELACIONAL` se
produce un error sintactico.

---

## Perspectiva semantica y de ejecucion

Una vez construido el AST, el compilador realiza la validacion semantica y
finalmente ejecuta el programa. Aqui se integran los cambios que cierran
el circulo para que `if/else` y los operadores logicos funcionen de forma
correcta.

### 1. La condicion del `if` debe ser booleana

`ValidarIf` consulta el tipo de la condicion a traves de
`ObtenerTipoExpresion`. Antes se aceptaba cualquier tipo y se confiaba en
una evaluacion por "truthiness" en runtime. Ahora se exige `bool`:

```vb
Dim tipoCond = ObtenerTipoExpresion(ifStmt.Condition)
If tipoCond <> "bool" Then
    Throw New Exception("La condicion del 'if' debe ser booleana, se obtuvo " & tipoCond)
End If
```

### 2. Tipos validos en `LogicalExpression`

`ObtenerTipoExpresion` aprendio a inferir el tipo de las expresiones
logicas:

- `!expr` exige `expr: bool` y devuelve `bool`.
- `expr && expr` y `expr || expr` exigen ambos operandos `bool` y
  devuelven `bool`.

Comparar numeros o cadenas con `&&` o `||` produce un error claro:
"Los operadores '&&' solo se aplican a expresiones booleanas".

### 3. Cortocircuito en el interprete

`EvaluarExpresion` ahora despacha tambien `LogicalExpression`. La
evaluacion de `&&` y `||` es cortocircuitada: si el operando izquierdo ya
determina el resultado, el derecho no se evalua.

```vb
If log.Operador = "&&" Then
    If Not EsVerdadero(valIzqLog) Then Return False
    Dim valDerLog = EvaluarExpresion(log.Derecha)
    Return EsVerdadero(valDerLog)
ElseIf log.Operador = "||" Then
    If EsVerdadero(valIzqLog) Then Return True
    ...
End If
```

La negacion unaria `!` se evalua con `Not EsVerdadero(valor)`.

### 4. Re-declaracion en el ambito del interprete

El validador semantico abre y cierra ambitos (`PushScope` / `PopScope`)
al recorrer cada `if`. Cuando el interprete llega al mismo nodo, el
ambito ya fue descartado y las variables locales del `if` desaparecian.
La solucion aplicada: `EjecutarDeclaracion` vuelve a declarar la variable
en el ambito actual del interprete si aun no existe.

```vb
Private Sub EjecutarDeclaracion(decl As Declaration)
    If Not symbolTable.VariableExiste(decl.Nombre) Then
        symbolTable.DeclararVariable(decl.Nombre, decl.TipoDato)
    End If
    If decl.Valor IsNot Nothing Then
        ...
    End If
End Sub
```

Esto garantiza que una declaracion dentro de un bloque `if` siga siendo
visible para las sentencias que la siguen dentro de ese mismo bloque, y
desaparezca correctamente cuando el bloque termina.

### Ejemplo end-to-end

Entrada:

```
int x;
x = 10;
if (x > 5 && x < 20) {
    print "rango ok";
} else {
    print "fuera de rango";
};
```

- **Lexico**: reconoce `if`, `&&` y los operadores relacionales como
  tokens dedicados.
- **Sintactico**: produce un `IfStatement` con `Condition` igual a
  `LogicalExpression("&&", ComparisonExpression(">", x, 5),
  ComparisonExpression("<", x, 20))`.
- **Semantica**: confirma que la condicion es `bool` y que los
  identificadores `x` estan declarados.
- **Ejecucion**: evalua `10 > 5` (true) y luego `10 < 20` (true);
  entra al bloque `then` e imprime `Salida: rango ok`.
