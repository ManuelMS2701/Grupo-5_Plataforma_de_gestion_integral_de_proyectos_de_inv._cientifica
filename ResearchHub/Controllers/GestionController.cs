using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchHub.Models;
using System;
using System.Collections.Generic;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class GestionController : Controller
    {
        private static readonly Dictionary<string, EntidadRoadmapViewModel> Catalogo =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Investigador"] = Crear(
                    nombre: "Investigador",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Gestionar perfiles cientificos, especialidad, ORCID y vinculacion institucional.",
                    funciones: new List<string>
                    {
                        "Alta, edicion y baja logica",
                        "Busqueda por especialidad y ORCID",
                        "Asignacion como investigador principal en proyectos"
                    },
                    caracteristicas: new List<string>
                    {
                        "Control de estado activo",
                        "Integridad con Institucion",
                        "Trazabilidad de fecha de registro"
                    },
                    practicas: new List<string>
                    {
                        "OSF prioriza metadatos y perfiles claros para descubribilidad y colaboracion.",
                        "Figshare integra ORCID con salidas publicas para trazabilidad del autor."
                    },
                    brechas: new List<string>
                    {
                        "Falta historial de cambios en perfil y afiliaciones.",
                        "Falta indicador de productividad por investigador."
                    },
                    acciones: new List<string>
                    {
                        "Agregar bitacora de cambios por campo critico.",
                        "Crear panel de produccion: proyectos, publicaciones y datasets por investigador."
                    }),

                ["Proyecto"] = Crear(
                    nombre: "Proyecto",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Centralizar ciclo de vida del proyecto de investigacion.",
                    funciones: new List<string>
                    {
                        "Registro de objetivo, estado y fechas clave",
                        "Asignacion de investigador principal e institucion",
                        "Relacion con linea de investigacion"
                    },
                    caracteristicas: new List<string>
                    {
                        "Entidad pivote para experimentos, muestras y cronograma",
                        "Base para publicaciones y repositorios de datos",
                        "Seguimiento administrativo y cientifico unificado"
                    },
                    practicas: new List<string>
                    {
                        "Benchling usa busqueda de proyectos con enlaces a entidades y entradas de laboratorio.",
                        "OSF usa estructura por componentes con permisos por nivel de proyecto."
                    },
                    brechas: new List<string>
                    {
                        "Falta vista de dependencias entre fases y entregables.",
                        "Falta panel de riesgos y bloqueos por proyecto."
                    },
                    acciones: new List<string>
                    {
                        "Implementar tablero Kanban/Gantt para cronograma y hitos.",
                        "Agregar estado de riesgo con alertas por fecha objetivo."
                    }),

                ["LineaInvestigacion"] = Crear(
                    nombre: "LineaInvestigacion",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Organizar los proyectos por foco cientifico.",
                    funciones: new List<string>
                    {
                        "Catalogo de lineas",
                        "Activacion y desactivacion",
                        "Filtro por linea en proyectos"
                    },
                    caracteristicas: new List<string>
                    {
                        "Clasificacion institucional",
                        "Escalable por areas",
                        "Control de lineas activas"
                    },
                    practicas: new List<string>
                    {
                        "OSF recomienda metadatos y etiquetas para mejorar descubribilidad.",
                        "Figshare usa metadatos estandar para indexacion y recuperacion."
                    },
                    brechas: new List<string>
                    {
                        "Falta taxonomia jerarquica por sublineas o palabras clave.",
                        "Falta tablero comparativo de rendimiento por linea."
                    },
                    acciones: new List<string>
                    {
                        "Agregar etiquetas tematicas y sublineas.",
                        "Crear KPI por linea: proyectos activos, resultados y publicaciones."
                    }),

                ["Institucion"] = Crear(
                    nombre: "Institucion",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Administrar entidades academicas o de investigacion vinculadas.",
                    funciones: new List<string>
                    {
                        "Alta de sede y contactos",
                        "Relacion con investigadores",
                        "Relacion con proyectos"
                    },
                    caracteristicas: new List<string>
                    {
                        "Restriccion de borrado con dependencias",
                        "Datos de ubicacion",
                        "Telefono y correo institucional"
                    },
                    practicas: new List<string>
                    {
                        "OSF usa permisos por proyecto/componente para colaboracion multiinstitucional.",
                        "SciNote promueve gestion de acceso por roles y cumplimiento."
                    },
                    brechas: new List<string>
                    {
                        "Falta catalogo de convenios y vigencia de colaboracion.",
                        "Falta matriz de permisos por institucion y rol."
                    },
                    acciones: new List<string>
                    {
                        "Agregar modulo de convenios y alcance operativo.",
                        "Definir perfiles de acceso por institucion colaboradora."
                    }),

                ["Experimento"] = Crear(
                    nombre: "Experimento",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Gestionar ejecucion experimental por proyecto y protocolo.",
                    funciones: new List<string>
                    {
                        "Estados de experimento y fechas de ejecucion",
                        "Asignacion a laboratorio y protocolo",
                        "Relacion directa con resultados"
                    },
                    caracteristicas: new List<string>
                    {
                        "Trazabilidad completa por proyecto",
                        "Base para analisis y validaciones",
                        "Estructura preparada para flujo de aprobacion"
                    },
                    practicas: new List<string>
                    {
                        "Benchling conecta experimentos con entidades e inventario dentro del mismo flujo.",
                        "Labguru permite captura estructurada en formularios durante la ejecucion."
                    },
                    brechas: new List<string>
                    {
                        "Falta checklist configurable por tipo de experimento.",
                        "Falta alertado de conflicto de recursos por horario."
                    },
                    acciones: new List<string>
                    {
                        "Crear plantillas de ejecucion por tipo de experimento.",
                        "Agregar validadores de consistencia antes de cerrar experimento."
                    }),

                ["Muestra"] = Crear(
                    nombre: "Muestra",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Registrar muestra cientifica, origen y condiciones.",
                    funciones: new List<string>
                    {
                        "Codigo unico y tipo de muestra",
                        "Registro de origen, condicion y fecha de recoleccion",
                        "Vinculo con proyecto"
                    },
                    caracteristicas: new List<string>
                    {
                        "Trazabilidad de ciclo de vida",
                        "Preparada para etiquetado fisico",
                        "Integracion con experimentos"
                    },
                    practicas: new List<string>
                    {
                        "Labguru prioriza escaneo de codigos para mover y actualizar stocks.",
                        "Benchling Inventory enlaza muestras con ubicaciones y contenedores."
                    },
                    brechas: new List<string>
                    {
                        "No hay campo de ubicacion fisica detallada por estante/caja.",
                        "No hay historial de transferencias de muestra."
                    },
                    acciones: new List<string>
                    {
                        "Implementar trazabilidad de movimientos y custodio.",
                        "Agregar soporte para codigo de barras/QR en la UI."
                    }),

                ["Laboratorio"] = Crear(
                    nombre: "Laboratorio",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Administrar recursos fisicos y responsables.",
                    funciones: new List<string>
                    {
                        "Capacidad operativa y ubicacion",
                        "Responsable de laboratorio",
                        "Estado activo/inactivo"
                    },
                    caracteristicas: new List<string>
                    {
                        "Relacion con experimentos",
                        "Relacion con equipos",
                        "Base para tablero de ocupacion"
                    },
                    practicas: new List<string>
                    {
                        "Labguru ofrece calendario de equipos y eventos de mantenimiento.",
                        "SciNote enfatiza control de acceso y auditoria en ambientes regulados."
                    },
                    brechas: new List<string>
                    {
                        "No hay calendario de disponibilidad del laboratorio.",
                        "No hay alerta por sobrecarga operativa."
                    },
                    acciones: new List<string>
                    {
                        "Agregar agenda de ocupacion por ventana horaria.",
                        "Crear alertas de capacidad y conflictos de uso."
                    }),

                ["EquipoLaboratorio"] = Crear(
                    nombre: "EquipoLaboratorio",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Controlar inventario tecnico y uso de equipos.",
                    funciones: new List<string>
                    {
                        "Registro por numero de serie",
                        "Estado operativo",
                        "Asignacion por laboratorio"
                    },
                    caracteristicas: new List<string>
                    {
                        "Historial de adquisicion",
                        "Preparado para mantenimiento preventivo",
                        "Integracion con experimentos"
                    },
                    practicas: new List<string>
                    {
                        "Labguru registra mantenimiento/calibracion y agenda eventos de equipo.",
                        "SciNote recomienda evidencia auditable para actividades criticas."
                    },
                    brechas: new List<string>
                    {
                        "Falta calendario de mantenimiento y proxima calibracion.",
                        "Falta evidencia documental de intervenciones."
                    },
                    acciones: new List<string>
                    {
                        "Implementar eventos de mantenimiento con responsable.",
                        "Adjuntar certificados y bitacora tecnica por equipo."
                    }),

                ["Protocolo"] = Crear(
                    nombre: "Protocolo",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Versionar y estandarizar metodos experimentales.",
                    funciones: new List<string>
                    {
                        "Control por version",
                        "Fecha de aprobacion",
                        "Estado activo"
                    },
                    caracteristicas: new List<string>
                    {
                        "Vinculacion con experimentos",
                        "Base para auditoria",
                        "Reuso de metodos"
                    },
                    practicas: new List<string>
                    {
                        "Labguru expone protocolos para consulta en movilidad durante ejecucion.",
                        "Benchling relaciona protocolos con entidades, experimentos e inventario."
                    },
                    brechas: new List<string>
                    {
                        "Falta ciclo formal de revision/aprobacion/version.",
                        "Falta historial de diffs entre versiones de protocolo."
                    },
                    acciones: new List<string>
                    {
                        "Agregar flujo de aprobacion con firma del responsable.",
                        "Versionar protocolos con comparacion de cambios."
                    }),

                ["Variable"] = Crear(
                    nombre: "Variable",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Definir metricas medibles y rangos de control.",
                    funciones: new List<string>
                    {
                        "Unidad, tipo y descripcion",
                        "Rango minimo y maximo",
                        "Relacion con resultados"
                    },
                    caracteristicas: new List<string>
                    {
                        "Estandarizacion de captura",
                        "Soporte para validaciones automaticas",
                        "Base para analitica de calidad"
                    },
                    practicas: new List<string>
                    {
                        "Plataformas ELN/LIMS aplican formularios estructurados para reducir errores.",
                        "SciNote usa controles de cumplimiento y trazabilidad de cambios."
                    },
                    brechas: new List<string>
                    {
                        "No hay catalogo de unidades validadas por dominio.",
                        "No hay reglas de alerta fuera de rango en captura."
                    },
                    acciones: new List<string>
                    {
                        "Agregar diccionario de unidades y tipos por disciplina.",
                        "Implementar alertas automaticas por outlier y fuera de rango."
                    }),

                ["Resultado"] = Crear(
                    nombre: "Resultado",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Registrar datos generados en experimentos.",
                    funciones: new List<string>
                    {
                        "Valor por variable",
                        "Observaciones y fecha de registro",
                        "Relacion experimento-variable"
                    },
                    caracteristicas: new List<string>
                    {
                        "Base para analisis",
                        "Preparado para exporte",
                        "Trazabilidad por contexto experimental"
                    },
                    practicas: new List<string>
                    {
                        "Labguru prioriza actualizacion en tiempo real de resultados en flujo.",
                        "Figshare promueve metadatos claros para reutilizacion y citacion."
                    },
                    brechas: new List<string>
                    {
                        "No hay versionado de resultado ante correcciones.",
                        "No hay indicador de calidad del dato capturado."
                    },
                    acciones: new List<string>
                    {
                        "Agregar control de version y motivo de correccion.",
                        "Implementar score de completitud y consistencia por registro."
                    }),

                ["Analisis"] = Crear(
                    nombre: "Analisis",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Interpretar resultados y documentar conclusiones.",
                    funciones: new List<string>
                    {
                        "Metodo de analisis",
                        "Conclusiones tecnicas",
                        "Relacion con resultado base"
                    },
                    caracteristicas: new List<string>
                    {
                        "Entrada para validacion",
                        "Soporte de evidencia reproducible",
                        "Seguimiento temporal del analisis"
                    },
                    practicas: new List<string>
                    {
                        "SciNote habilita auditoria detallada para cumplimiento y revisiones.",
                        "OSF incentiva transparencia de metodos y metadatos para reuso."
                    },
                    brechas: new List<string>
                    {
                        "No hay plantilla metodologica por tipo de analisis.",
                        "No hay flujo de aprobacion por pares."
                    },
                    acciones: new List<string>
                    {
                        "Implementar plantillas por metodo estadistico o analitico.",
                        "Agregar revisiones con observaciones y version final aprobada."
                    }),

                ["Publicacion"] = Crear(
                    nombre: "Publicacion",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Dar seguimiento a salida cientifica y DOI.",
                    funciones: new List<string>
                    {
                        "Titulo, resumen y revista",
                        "Control de fecha de publicacion",
                        "Registro DOI"
                    },
                    caracteristicas: new List<string>
                    {
                        "Relacion con proyecto",
                        "Indicadores de impacto",
                        "Historial editorial"
                    },
                    practicas: new List<string>
                    {
                        "Figshare expone DOI y metadatos reutilizables para citacion.",
                        "OSF promueve metadatos para descubribilidad y colaboracion."
                    },
                    brechas: new List<string>
                    {
                        "No hay estado editorial (borrador, enviado, aceptado, publicado).",
                        "No hay enlace automatico a datasets relacionados."
                    },
                    acciones: new List<string>
                    {
                        "Agregar pipeline editorial con fechas por etapa.",
                        "Vincular publicaciones con repositorios de datos y materiales."
                    }),

                ["RepositorioDatos"] = Crear(
                    nombre: "RepositorioDatos",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Gestionar publicacion de datasets y su gobernanza.",
                    funciones: new List<string>
                    {
                        "URL, tipo y fecha de registro",
                        "Descripcion de dataset",
                        "Relacion con proyecto"
                    },
                    caracteristicas: new List<string>
                    {
                        "Base para ciencia abierta",
                        "Trazabilidad de datos publicados",
                        "Punto de auditoria para metadatos"
                    },
                    practicas: new List<string>
                    {
                        "Figshare usa DOI, versionado y licencias para preservacion y reuso.",
                        "Zenodo permite curacion por comunidades y metadatos editables al enviar."
                    },
                    brechas: new List<string>
                    {
                        "Falta registrar licencia, embargo y version del dataset.",
                        "Falta checklist FAIR minimo antes de publicar."
                    },
                    acciones: new List<string>
                    {
                        "Agregar campos de licencia, DOI, embargo y version.",
                        "Aplicar validacion de metadatos obligatorios para publicacion."
                    }),

                ["Colaborador"] = Crear(
                    nombre: "Colaborador",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Registrar participantes internos y externos del proyecto.",
                    funciones: new List<string>
                    {
                        "Tipo y rol",
                        "Datos de contacto",
                        "Asignacion por proyecto"
                    },
                    caracteristicas: new List<string>
                    {
                        "Soporte de equipos multidisciplinarios",
                        "Control de participacion",
                        "Relaciones institucionales"
                    },
                    practicas: new List<string>
                    {
                        "OSF maneja permisos por colaborador y por componente del proyecto.",
                        "SciNote prioriza gestion de usuarios y permisos para cumplimiento."
                    },
                    brechas: new List<string>
                    {
                        "No hay niveles de permiso por modulo/entidad.",
                        "No hay vigencia de colaboracion ni control de expiracion."
                    },
                    acciones: new List<string>
                    {
                        "Implementar roles por dominio (lectura, edicion, aprobacion).",
                        "Agregar fechas de inicio/fin de colaboracion por proyecto."
                    }),

                ["Cronograma"] = Crear(
                    nombre: "Cronograma",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Controlar fases, fechas y estado del plan de trabajo.",
                    funciones: new List<string>
                    {
                        "Registro de fase y estado",
                        "Fechas de inicio y fin",
                        "Relacion directa con proyecto"
                    },
                    caracteristicas: new List<string>
                    {
                        "Visibilidad de avances",
                        "Base para alertas",
                        "Preparado para dependencias entre tareas"
                    },
                    practicas: new List<string>
                    {
                        "Plataformas de laboratorio usan tareas y eventos para no perder hitos.",
                        "OSF permite estructurar trabajo en componentes y plantillas."
                    },
                    brechas: new List<string>
                    {
                        "No hay dependencia entre fases ni ruta critica.",
                        "No hay alertas automaticas por retraso."
                    },
                    acciones: new List<string>
                    {
                        "Implementar dependencias y porcentaje de avance por fase.",
                        "Agregar notificaciones por vencimiento y atraso."
                    }),

                ["Validacion"] = Crear(
                    nombre: "Validacion",
                    estado: "CRUD activo",
                    madurez: "Operativa",
                    objetivo: "Aprobar, observar o rechazar analisis con evidencia.",
                    funciones: new List<string>
                    {
                        "Resultado de validacion",
                        "Validador responsable",
                        "Observaciones"
                    },
                    caracteristicas: new List<string>
                    {
                        "Relacion con analisis",
                        "Control de calidad",
                        "Flujo de aprobaciones"
                    },
                    practicas: new List<string>
                    {
                        "SciNote destaca auditoria detallada y gestion avanzada de usuarios para compliance.",
                        "Zenodo/Figshare muestran trazabilidad de cambios y estados de publicacion."
                    },
                    brechas: new List<string>
                    {
                        "No hay firma digital ni evidencia adjunta obligatoria.",
                        "No hay SLA de validacion por criticidad."
                    },
                    acciones: new List<string>
                    {
                        "Agregar aprobacion con firma/usuario y sello de tiempo.",
                        "Configurar tiempos objetivo y escalamiento de revisiones."
                    })
            };

        public IActionResult Entidad(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return View(new EntidadRoadmapViewModel
                {
                    Nombre = "Modulo",
                    Estado = "Planeado",
                    Madurez = "Inicial",
                    Objetivo = "Selecciona una entidad para ver su diseno funcional y brechas."
                });
            }

            if (Catalogo.TryGetValue(nombre, out var entidad))
            {
                return View(entidad);
            }

            return View(new EntidadRoadmapViewModel
            {
                Nombre = nombre,
                Estado = "Pendiente de definicion",
                Madurez = "Inicial",
                Objetivo = "Entidad no catalogada aun en el roadmap."
            });
        }

        private static EntidadRoadmapViewModel Crear(
            string nombre,
            string estado,
            string madurez,
            string objetivo,
            List<string> funciones,
            List<string> caracteristicas,
            List<string>? practicas = null,
            List<string>? brechas = null,
            List<string>? acciones = null)
        {
            return new EntidadRoadmapViewModel
            {
                Nombre = nombre,
                Estado = estado,
                Madurez = madurez,
                Objetivo = objetivo,
                Funciones = funciones,
                Caracteristicas = caracteristicas,
                PracticasReferencia = practicas ?? new List<string>(),
                BrechasActuales = brechas ?? new List<string>(),
                AccionesPrioritarias = acciones ?? new List<string>()
            };
        }
    }
}

