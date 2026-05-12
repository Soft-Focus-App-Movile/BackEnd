Feature: Recibir alertas de crisis
  Como psicólogo,
  quiero ser notificado cuando un paciente está en crisis
  para brindar apoyo inmediato.

  Scenario Outline: Recepción de alerta inmediata
    Given que un <paciente> presiona el botón de crisis
    When se procesa la alerta
    Then el <psicologo> recibe una <alerta_generada>

    Examples:
      | paciente | psicologo | alerta_generada                              |
      | Carlos   | Dra. Ruiz | notificación push con el nombre del paciente |
      | Ana      | Dr. Smith | notificación push con el nombre del paciente |

  Scenario Outline: Acceso rápido al chat desde la alerta
    Given que el <psicologo> recibe una alerta de crisis
    When abre la notificación
    Then accede directamente al <accion_destino>

    Examples:
      | psicologo | paciente | accion_destino       |
      | Dra. Ruiz | Carlos   | chat con el paciente |
      | Dr. Smith | Ana      | chat con el paciente |