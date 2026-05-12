Feature: Conectarse con psicólogo mediante código
  Como paciente,
  quiero ingresar el código de mi psicólogo
  para establecer la conexión terapéutica en la app.

  Scenario Outline: Intento de vinculación con código de terapeuta
    Given que el paciente se encuentra en la sección "Conectar con psicólogo"
    And tiene un código de conexión provisto
    When ingresa el código "<codigo_ingresado>" e intenta conectarse
    Then la plataforma procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | codigo_ingresado | estado | accion_resultante                                                 |
      | 12342490305      | éxito  | se establece la vinculación y aparece el perfil del terapeuta     |
      | 000000001999     | error  | mensaje de error y opción para reintentar o contactar soporte     |