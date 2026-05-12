Feature: Usar reconocimiento facial de emociones
  Como usuario,
  quiero que la app analice mis expresiones faciales,
  para obtener un análisis objetivo de mi estado emocional.

  Scenario Outline: Intento de análisis facial de emociones
    Given que el usuario inicia un check-in
    And decide activar el análisis facial: "<activa_camara>"
    When envía la imagen con tamaño "<tamano_imagen>"
    Then el sistema procesa la solicitud con un estado de "<estado>"
    And el sistema le muestra "<accion_resultante>"

    Examples:
      | activa_camara | tamano_imagen | estado | accion_resultante                                                                        |
      | sí            | válido        | éxito  | feedback sobre emociones detectadas y sugerencias basadas en el análisis                 |
      | sí            | mayor 5MB     | error  | mensaje de error indicando que la imagen supera el tamaño máximo permitido               |
      | no            | ninguno       | éxito  | el check-in continúa sin análisis facial y sin requerir imagen                          |
