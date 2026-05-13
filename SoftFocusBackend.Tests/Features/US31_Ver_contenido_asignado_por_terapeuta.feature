Feature: Ver contenido asignado por terapeuta
  Como paciente,
  quiero acceder a recursos específicos de mi psicólogo,
  para seguir mi plan de tratamiento personalizado.

  Scenario Outline: Sección dedicada de contenido asignado
    Given que el <paciente> tiene psicólogo asignado
    When accede a la aplicación
    Then ve claramente la sección "Asignado por tu terapeuta" con el <contenido_especifico> asignado

    Examples:
      | paciente | contenido_especifico                                   |
      | Carlos   | película de bienestar y ejercicio de respiración       |
      | Ana      | video de meditación guiada y música relajante          |

  Scenario Outline: Seguimiento de cumplimiento de tareas
    Given que el <paciente> completa las tareas asignadas por su terapeuta
    When las marca como completadas en la aplicación
    Then el <psicologo> recibe una <notificacion_progreso> sobre el avance del paciente

    Examples:
      | paciente | psicologo | notificacion_progreso                                        |
      | Carlos   | Dra. Ruiz | notificación de tarea completada con detalle del contenido   |
      | Ana      | Dr. Smith | notificación de progreso con porcentaje del plan cumplido    |
