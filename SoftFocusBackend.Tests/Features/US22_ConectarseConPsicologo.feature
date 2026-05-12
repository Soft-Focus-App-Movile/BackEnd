Feature: Conectarse con psicólogo mediante código
  Como paciente
  quiero ingresar el código de mi psicólogo
  para establecer la conexión terapéutica en la app.

  Scenario Outline: Conectarse exitosamente con un código válido
    Given el <paciente> tiene un código de su psicólogo
    And se encuentra en la sección "Conectar con psicólogo"
    When ingresa el <codigo> en el campo correspondiente
    And hace clic en el botón para conectarse
    Then el sistema muestra <resultado>

    Examples:
      | paciente | codigo      | resultado                                             |
      | Juan     | VALIDO-1234 | vinculación exitosa y aparece el perfil del terapeuta |

  Scenario Outline: Fallar al intentar conectarse con un código inválido
    Given el <paciente> tiene un código de su psicólogo
    And se encuentra en la sección "Conectar con psicólogo"
    When ingresa un <codigo> incorrecto en el campo correspondiente
    And hace clic en el botón para conectarse
    Then el sistema muestra <resultado>

    Examples:
      | paciente | codigo       | resultado                                                   |
      | María    | INVALIDO-999 | mensaje de error y opción de reintentar o contactar soporte |