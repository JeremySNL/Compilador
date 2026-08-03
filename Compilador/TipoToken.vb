' Tokens del lenguaje
' Agregados sobre la version base:
'   OP_RELACIONAL   -> Para ==, !=, <, >, <=, >=
'   OP_LOGICO       -> Para &&, ||, !
'   LLAVE_IZQUIERDA -> {
'   LLAVE_DERECHA   -> }
Public Enum TipoToken
    IDENTIFICADOR
    NUMERO
    CADENA
    PALABRA_RESERVADA   ' print, if, else
    TIPO_DATO           ' int, float, string
    ASIGNACION          ' =
    OPERADOR_ARITMETICO ' +, -, *, /
    OP_RELACIONAL       ' ==, !=, <, >, <=, >=
    OP_LOGICO           ' &&, ||, !
    PARENTESIS_IZQUIERDO
    PARENTESIS_DERECHO
    LLAVE_IZQUIERDA     ' {
    LLAVE_DERECHA       ' }
    FIN_LINEA           ' ;
    FIN_ARCHIVO
    ERROR_404
End Enum
