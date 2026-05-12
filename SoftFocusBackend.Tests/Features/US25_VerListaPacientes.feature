Feature: Ver lista básica de pacientes
  Como psicólogo,
  quiero ver una lista simple de mis pacientes
  para acceder rápidamente a su información.

  Scenario Outline: Visualización de la lista de pacientes
    Given que el <psicologo> accede a su dashboard
    When revisa la sección de sus pacientes
    Then el sistema le muestra una <informacion_mostrada>

    Examples:
      | psicologo | informacion_mostrada                              |
      | Dr. Smith | lista con nombre, último check-in y estado básico |
      | Dra. Ruiz | lista con nombre, último check-in y estado básico |

  Scenario Outline: Acceso directo al perfil desde la lista
    Given que el <psicologo> selecciona a un <paciente> de la lista
    When hace clic en su nombre
    Then accede directamente al perfil básico del paciente

    Examples:
      | psicologo | paciente |
      | Dra. Ruiz | Carlos   |
      | Dr. Smith | Ana      |