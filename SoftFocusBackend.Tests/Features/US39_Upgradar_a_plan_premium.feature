Feature: Upgradar a plan premium
  Como usuario,
  quiero actualizar mi suscripción,
  para acceder a funcionalidades avanzadas sin restricciones.

  Scenario Outline: Comparación de planes antes del upgrade
    Given que el <usuario> con plan gratuito encuentra limitaciones al usar "<funcionalidad_limitada>"
    When accede a la pantalla "Actualizar plan"
    Then ve una comparación clara entre el plan gratuito y el premium con los <beneficios_premium>

    Examples:
      | usuario  | funcionalidad_limitada             | beneficios_premium                                             |
      | Carlos   | mensajes de IA agotados            | chats ilimitados, análisis facial ilimitado, sin restricciones |
      | María    | análisis facial semanal alcanzado  | análisis facial ilimitado, recomendaciones sin límite          |

  Scenario Outline: Proceso de pago y activación del plan premium
    Given que el <usuario> decide upgradar al plan premium
    When selecciona el plan premium y completa el pago a través de <plataforma_pago>
    Then obtiene acceso inmediato a todas las funcionalidades avanzadas con <confirmacion_acceso>

    Examples:
      | usuario | plataforma_pago | confirmacion_acceso                                           |
      | Carlos  | App Store       | pantalla de confirmación y funcionalidades desbloqueadas      |
      | María   | Google Play     | notificación de activación y acceso completo desde el momento |
