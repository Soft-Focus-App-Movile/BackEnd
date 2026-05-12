Feature: Iniciar sesión con credenciales
  Como usuario registrado,
  quiero iniciar sesión con email y contraseña,
  para acceder a mi cuenta personal.

  Scenario Outline: Intento de inicio de sesión con credenciales
    Given que el usuario se encuentra en la pantalla de inicio de sesión
    And ingresa el email "<email>" y la contraseña "<contrasena>"
    When intenta iniciar sesión
    Then el sistema procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | email                  | contrasena  | estado | accion_resultante                                                             |
      | usuario@correo.com     | Correcta1!  | éxito  | accede al dashboard correspondiente a su tipo de usuario                      |
      | usuario@correo.com     | Incorrecta1 | error  | mensaje de error sin especificar si el fallo es en el email o la contraseña   |
      | noexiste@correo.com    | Correcta1!  | error  | mensaje de error sin especificar si el fallo es en el email o la contraseña   |