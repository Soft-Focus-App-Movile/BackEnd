Feature: Recepción y gestión de notificaciones
  Como usuario de la aplicación,
  quiero recibir notificaciones push en tiempo real
  para mantenerme informado sobre eventos importantes y recordatorios.

  Scenario Outline: Recepción exitosa de notificación push
    Given que el <usuario> tiene las notificaciones de la app activadas
    When el sistema detecta un <evento_importante>
    Then el dispositivo móvil muestra una <alerta_push> inmediata

    Examples:
      | usuario   | evento_importante           | alerta_push                                 |
      | Francisco | sesión a punto de iniciar   | "Tu sesión de terapia inicia en 15 minutos" |
      | Sofía     | nuevo mensaje del terapeuta | "Tienes un nuevo mensaje sin leer"          |

  Scenario Outline: Respeto de las preferencias de notificación
    Given que el <usuario> ha desactivado las alertas push desde su perfil
    When el sistema genera un <evento_importante>
    Then la notificación se guarda en la bandeja interna pero <resultado_dispositivo>

    Examples:
      | usuario | evento_importante            | resultado_dispositivo                                 |
      | Luis    | alerta de chequeo emocional  | el celular no suena ni muestra ventanas emergentes    |
      | Andrea  | nueva recomendación          | el celular no vibra ni muestra banner en la pantalla  |