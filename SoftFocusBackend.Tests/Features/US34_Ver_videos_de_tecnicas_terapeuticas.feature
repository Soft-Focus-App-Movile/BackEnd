Feature: Ver videos de técnicas terapéuticas
  Como usuario,
  quiero acceder a videos educativos sobre técnicas de bienestar,
  para aprender ejercicios de forma visual.

  Scenario Outline: Exploración de biblioteca de videos terapéuticos
    Given que el <usuario> busca técnicas visuales de bienestar
    When accede a la sección de videos en la aplicación
    Then encuentra contenido de YouTube sobre <tecnica_bienestar> disponible para reproducir

    Examples:
      | usuario  | tecnica_bienestar                              |
      | Carlos   | respiración diafragmática y mindfulness        |
      | Ana      | meditación guiada y relajación muscular        |
      | Luis     | yoga terapéutico y técnicas de grounding       |

  Scenario Outline: Reproducción de video según configuración del sistema
    Given que el <usuario> selecciona un video de la biblioteca
    When presiona el botón de reproducción
    Then el sistema <resultado_reproduccion> según la configuración activa de la aplicación

    Examples:
      | usuario | resultado_reproduccion                                          |
      | Carlos  | abre el reproductor integrado dentro de la app                  |
      | Ana     | redirige directamente a YouTube para ver el contenido completo  |
