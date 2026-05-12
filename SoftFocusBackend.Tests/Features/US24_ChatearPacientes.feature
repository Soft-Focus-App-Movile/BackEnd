Feature: Chatear con pacientes
  Como psicólogo,
  quiero comunicarme directamente con mis pacientes
  para brindar contención emocional entre sesiones.

  Scenario Outline: Chat en tiempo real con paciente conectado
    Given que el <psicologo> tiene pacientes conectados
    When accede al chat de un <paciente> específico
    Then puede visualizar <resultado_chat> en tiempo real

    Examples:
      | psicologo | paciente | resultado_chat        |
      | Dra. Ruiz | Carlos   | los mensajes enviados |
      | Dr. Smith | Ana      | los mensajes enviados |

  Scenario Outline: Envío de mensajes de contención durante una crisis
    Given que el <paciente> está en crisis
    When el <psicologo> recibe la alerta
    Then puede responder inmediatamente enviando mensajes de apoyo y <resultado_crisis>

    Examples:
      | paciente | psicologo | resultado_crisis      |
      | Carlos   | Dra. Ruiz | los mensajes enviados |
      | Ana      | Dr. Smith | los mensajes enviados |