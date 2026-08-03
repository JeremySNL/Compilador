' Agregados:
'   OP_RELACIONAL   -> Para ==, !=, <, >, <=, >=
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
    PARENTESIS_IZQUIERDO
    PARENTESIS_DERECHO
    LLAVE_IZQUIERDA     ' {
    LLAVE_DERECHA       ' }
    FIN_LINEA           ' ;
    FIN_ARCHIVO
    ERROR_404
End Enum