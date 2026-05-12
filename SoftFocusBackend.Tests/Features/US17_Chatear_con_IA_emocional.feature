Feature: Chatear con IA emocional
  Como usuario,
  quiero conversar con la IA sobre mi estado emocional,
  para recibir apoyo inmediato y técnicas de manejo.

  Scenario Outline: Intento de chat con IA emocional
    Given que el usuario tiene plan "<plan>" y se siente "<estado_emocional>"
    And accede al chat con IA
    When envía el mensaje "<mensaje>"
    Then el sistema procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | plan      | estado_emocional | mensaje                        | estado | accion_resultante                                                                      |
      | premium   | mal              | Me siento muy ansioso hoy      | éxito  | respuesta empática con sugerencias de técnicas de manejo emocional                    |
      | gratuito  | mal              | Necesito hablar con alguien    | éxito  | respuesta empática dentro del límite de 3 conversaciones por semana                   |
      | gratuito  | mal              | Cuarta conversación del plan   | error  | mensaje indicando que alcanzó el límite semanal y opción para mejorar el plan         |
