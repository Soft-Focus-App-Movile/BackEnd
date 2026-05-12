Feature: Ver perfil básico de paciente
  Como psicólogo,
  quiero ver información esencial del paciente
  para conocer su estado actual sin complejidad.

  Scenario Outline: Revisión de información básica
    Given que el <psicologo> accede al perfil de un <paciente>
    When revisa la información
    Then puede visualizar <datos_basicos>

    Examples:
      | psicologo | paciente | datos_basicos                                |
      | Dr. Smith | Ana      | check-ins recientes, estado emocional actual |
      | Dra. Ruiz | Carlos   | check-ins recientes, estado emocional actual |

  Scenario Outline: Revisión del historial simple
    Given que el <psicologo> revisa el historial del <paciente>
    When ve los datos
    Then encuentra una <datos_historial>

    Examples:
      | psicologo | paciente | datos_historial                                                 |
      | Dra. Ruiz | Carlos   | lista cronológica de check-ins con fechas y estados emocionales |
      | Dr. Smith | Ana      | lista cronológica de check-ins con fechas y estados emocionales |