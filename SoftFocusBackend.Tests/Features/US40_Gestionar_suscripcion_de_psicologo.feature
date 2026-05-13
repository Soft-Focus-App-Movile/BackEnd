Feature: Gestionar suscripción de psicólogo
  Como psicólogo,
  quiero administrar mi plan de suscripción,
  para ajustarlo según las necesidades de mi práctica profesional.

  Scenario Outline: Upgrade de plan profesional para ampliar capacidades
    Given que el <psicologo> tiene el plan básico con <limitacion_actual>
    When accede a "Actualizar plan profesional"
    Then puede upgradar al plan premium con <beneficio_premium> habilitado de forma inmediata

    Examples:
      | psicologo  | limitacion_actual                       | beneficio_premium                                     |
      | Dra. Ruiz  | máximo 3 pacientes conectados           | pacientes ilimitados y asignaciones sin restricción   |
      | Dr. Smith  | 5 asignaciones de contenido por semana  | asignaciones ilimitadas y análisis de progreso        |

  Scenario Outline: Gestión de facturación y configuración de cuenta
    Given que el <psicologo> administra su suscripción desde la configuración de cuenta
    When accede a la sección de facturación
    Then puede <accion_facturacion> sin necesidad de contactar soporte

    Examples:
      | psicologo  | accion_facturacion                                                   |
      | Dra. Ruiz  | ver historial de pagos y descargar facturas en formato PDF           |
      | Dr. Smith  | cambiar método de pago y actualizar datos de facturación             |
