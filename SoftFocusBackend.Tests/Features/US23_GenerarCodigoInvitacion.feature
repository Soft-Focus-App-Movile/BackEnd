Feature: US23 Generar código de invitación
  Como psicólogo,
  quiero generar códigos únicos
  para invitar a mis pacientes a conectarse conmigo.

  Scenario Outline: Generación automática de código
    Given que el <psicologo> accede a su dashboard.
    When ve la sección "Invitar pacientes".
    Then encuentra su <codigo> único generado automáticamente.

    Examples:
      | psicologo | codigo      |
      | Dr. Smith | SMITH-5521  |

  Scenario Outline: Compartir código de invitación
    Given que el <psicologo> quiere invitar a un paciente.
    When selecciona la opción "Compartir código".
    Then puede enviarlo por <medio_envio> o mostrarlo en pantalla.

    Examples:
      | psicologo  | medio_envio |
      | Dra. Gómez | email       |
      | Dr. Smith  | mensaje     |