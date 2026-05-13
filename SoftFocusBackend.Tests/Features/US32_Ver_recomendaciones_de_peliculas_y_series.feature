Feature: Ver recomendaciones de películas y series
  Como usuario,
  quiero recibir sugerencias de contenido audiovisual,
  para mejorar mi estado de ánimo con entretenimiento apropiado.

  Scenario Outline: Recomendaciones de contenido según estado emocional
    Given que el <usuario> completa un check-in con estado emocional "<estado_emocional>"
    When el sistema procesa su estado
    Then ve recomendaciones de <tipo_contenido> con imagen, sinopsis y trailer acorde a su emoción

    Examples:
      | usuario  | estado_emocional | tipo_contenido                      |
      | Carlos   | triste           | películas positivas y motivadoras   |
      | María    | ansioso          | series de comedia y entretenimiento |
      | Sofía    | feliz            | películas de aventura y acción      |

  Scenario Outline: Redirección a plataformas de streaming
    Given que el <usuario> selecciona un contenido recomendado
    When hace clic en "Ver ahora"
    Then es redirigido a la <plataforma_disponible> correspondiente para reproducir el contenido

    Examples:
      | usuario | plataforma_disponible |
      | Carlos  | Netflix               |
      | María   | Prime Video           |
      | Sofía   | Disney+               |
