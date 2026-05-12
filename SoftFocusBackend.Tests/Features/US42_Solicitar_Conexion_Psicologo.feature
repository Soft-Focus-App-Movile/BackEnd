Feature: Solicitar conexión con psicólogo del directorio
  Como usuario general,
  quiero solicitar conexión con un psicólogo del directorio
  para obtener sus datos de contacto y coordinar una sesión.

  Scenario Outline: Solicitud de datos exitosa
    Given que el <usuario> selecciona el perfil de un psicólogo en el directorio
    When presiona el botón "Solicitar conexión"
    Then el sistema le muestra los <datos_contacto> del especialista

    Examples:
      | usuario | datos_contacto                                 |
      | Carlos  | email corporativo, teléfono y WhatsApp directo |
      | María   | email corporativo y enlace directo a WhatsApp  |

  Scenario Outline: Visualización de instrucciones post-solicitud
    Given que el <usuario> ya visualizó los datos de contacto del psicólogo
    When se encuentra en la pantalla de confirmación
    Then el sistema muestra <instrucciones_claras> sobre el siguiente paso

    Examples:
      | usuario | instrucciones_claras                                                    |
      | Carlos  | "Comunícate por WhatsApp para que el especialista te brinde tu código"  |
      | Ana     | "Escribe un email solicitando el código de vinculación de Soft Focus"   |