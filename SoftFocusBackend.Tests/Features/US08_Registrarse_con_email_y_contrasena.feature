Feature: Registrarse con email y contraseña
  Como nuevo usuario,
  quiero registrarme con email y contraseña,
  para crear mi cuenta en la aplicación.

  Scenario Outline: Intento de registro con email y contraseña
    Given que el usuario accede a la pantalla de registro
    And completa email "<email>", contraseña "<contrasena>" y acepta términos "<acepta_terminos>"
    When intenta registrarse
    Then el sistema procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | email                | contrasena  | acepta_terminos | estado | accion_resultante                                              |
      | ana@correo.com       | Segura1!    | true            | éxito  | la cuenta es creada y se envía email de verificación          |
      | no-es-un-email       | Segura1!    | true            | error  | mensaje de error indicando que el email no es válido          |
      | ana@correo.com       | abc         | true            | error  | mensaje de error indicando que la contraseña es muy débil     |
      | ana@correo.com       | Segura1!    | false           | error  | mensaje de error indicando que debe aceptar los términos      |