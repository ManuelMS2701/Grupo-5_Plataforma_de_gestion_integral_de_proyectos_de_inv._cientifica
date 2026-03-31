# Benchmark de plataformas similares para ResearchHub

Fecha: 2026-03-31

## Objetivo
Comparar ResearchHub con plataformas cientificas consolidadas y traducir buenas practicas en mejoras concretas para las 17 entidades del dominio.

## Hallazgos clave por plataforma

### Benchling
- Enfatiza busqueda por proyectos, inventario, registry y tipo de entidad.
- Conecta entidades registradas, inventario y resultados en un flujo unificado.
- Incluye workflows, solicitudes e insights para tableros operativos.

Fuentes:
- https://help.benchling.com/hc/en-us/articles/39947872229005-Search-for-data
- https://help.benchling.com/hc/en-us/articles/39953982240397-Create-and-manage-entities-with-the-Registry

### Labguru
- Inventario con ubicacion fisica, codigos (QR/barcode), lotes y stock.
- Gestion de equipos con calendario, mantenimiento y notificaciones.
- Integracion ELN + inventario + protocolos + reportes en un mismo producto.

Fuentes:
- https://www.labguru.com/inventory
- https://help.labguru.com/en/articles/1492432-managing-the-lab-s-equipment-in-labguru
- https://www.labguru.com/labguru-mobile-app-labhandy

### SciNote
- Cumplimiento regulatorio con auditoria detallada, roles/permisos y firmas electronicas.
- Inventario conectado a protocolos, experimentos y resultados.
- Enfoque fuerte en trazabilidad y actividad con timestamp.

Fuentes:
- https://knowledgebase.scinote.net/en/knowledge/can-my-scinote-be-compliant-with-21-cfr-part-11-regulations
- https://knowledgebase.scinote.net/en/knowledge/how-do-i-access-and-export-audit-trail
- https://www.scinote.net/product/inventory-management/

### OSF
- Estructura por proyectos/componentes con permisos separados por nivel.
- Uso de metadatos y tags para descubribilidad y reutilizacion.
- Registro/preregistro de proyectos para fijar estados de investigacion.

Fuentes:
- https://help.osf.io/article/246-understand-contributor-permissions
- https://help.osf.io/article/568-add-metadata-to-your-osf-project
- https://help.osf.io/article/330-welcome-to-registrations

### Figshare + Zenodo
- Publicacion de datasets con DOI, metadatos estandar y versionado.
- Curacion/comunidades y edicion de metadatos en flujos de publicacion.
- Enfasis en trazabilidad de cambios y practicas FAIR.

Fuentes:
- https://info.figshare.com/user-guide/how-figshare-com-meets-the-ostp-and-nih-desirable-characteristics-for-data-repositories/
- https://support.figshare.com/en/articles/11284801
- https://help.zenodo.org/docs/share/submit-to-community/

## Traduccion a ResearchHub
- Sidebar vertical y jerarquia por dominio cientifico (navegacion tipo plataforma ELN/LIMS).
- Login y registro en layout aislado, sin barra global.
- Mapa por entidad con:
  - estado de implementacion
  - madurez
  - practicas de referencia de mercado
  - brechas actuales
  - acciones prioritarias
- Filtros de busqueda en proyectos (por texto y estado), inspirado en patrones de discovery.
- Panel administrativo con benchmark visible para priorizar cierre de brechas.

## Entidades cubiertas
Investigador, Proyecto, LineaInvestigacion, Experimento, Muestra, Laboratorio, EquipoLaboratorio, Protocolo, Variable, Resultado, Analisis, Publicacion, RepositorioDatos, Institucion, Colaborador, Cronograma, Validacion.
