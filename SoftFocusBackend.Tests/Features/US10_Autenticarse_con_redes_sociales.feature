Feature: Autenticarse con redes sociales
  Como usuario,
  quiero iniciar sesión con Google o Facebook,
  para acceder rápidamente sin crear contraseña.

  Scenario Outline: Intento de autenticación con red social
    Given que el usuario selecciona login con "<proveedor>"
    And es primera vez usando social login: "<primera_vez>"
    When autoriza el acceso desde el proveedor con token "<token>"
    Then el sistema procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | proveedor | primera_vez | token              | estado | accion_resultante                                                              |
      | google    | no          | token-valido-google | éxito  | accede automáticamente a la cuenta existente                                   |
      | google    | sí          | token-valido-google | éxito  | solicita información adicional requerida como el tipo de usuario               |
      | facebook  | no          | token-invalido      | error  | mensaje de error indicando que la autenticación con la red social falló        |