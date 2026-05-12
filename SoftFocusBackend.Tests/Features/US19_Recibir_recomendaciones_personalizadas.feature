Feature: Recibir recomendaciones personalizadas
  Como usuario,
  quiero recibir sugerencias de contenido según mi estado,
  para mejorar mi bienestar emocional.

  Scenario Outline: Recepción de recomendaciones personalizadas
    Given que el usuario completa un check-in con estado emocional "<estado_emocional>"
    And tiene psicólogo asignado: "<tiene_psicologo>"
    When el sistema procesa su estado y contenido asignado
    Then el resultado de recomendaciones tiene un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | estado_emocional | tiene_psicologo | estado | accion_resultante                                                                                  |
      | bajo             | no              | éxito  | recomendaciones automáticas de ejercicios de respiración, música relajante y películas positivas   |
      | bajo             | sí              | éxito  | sección "Asignado por tu terapeuta" con recursos específicos además de las recomendaciones         |
      | alto             | no              | éxito  | recomendaciones de mantenimiento acorde al estado emocional positivo reportado                     |
