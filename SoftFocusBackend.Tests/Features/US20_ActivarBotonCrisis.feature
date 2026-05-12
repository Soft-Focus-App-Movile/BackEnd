Feature: Activar botón de crisis
  Como usuario en crisis,
  quiero presionar un botón de emergencia
  para recibir ayuda inmediata de mi psicólogo.

  Scenario Outline: Alerta a psicólogo cuando hay uno asignado
    Given que el <usuario> tiene un psicólogo asignado.
    When presiona el botón de crisis.
    Then <resultado_esperado>

    Examples:
      | usuario | resultado_esperado                                          |
      | Carlos  | alerta inmediata al terapeuta con ubicación y estado actual |
      | María   | alerta inmediata al terapeuta con ubicación y estado actual |

  Scenario Outline: Acceso a recursos de autoayuda cuando no hay psicólogo asignado
    Given que el <usuario> no tiene un psicólogo asignado.
    When activa el botón de crisis.
    Then <resultado_esperado>

    Examples:
      | usuario | resultado_esperado                                               |
      | Ana     | recursos de emergencia, líneas de ayuda y técnicas de contención |