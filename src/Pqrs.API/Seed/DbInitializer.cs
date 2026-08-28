using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Enums;
using Pqrs.Infrastructure.Persistence;

namespace Pqrs.API.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(PqrsDbContext context, IAiService aiService)
    {
        // Ensure Database schema is created
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        var tenantA = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "leggumbres-la-escoba");
        var tenantB = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "todo-metal");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // ----------------------------------------------------
        // TENANT A - Leggumbres La Escoba
        // ----------------------------------------------------
        if (tenantA != null)
        {
            tenantA.WidgetPublicKey = "leggumbres-key-123";
        }
        else
        {
            tenantA = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Leggumbres La Escoba",
                Slug = "leggumbres-la-escoba",
                WidgetPublicKey = "leggumbres-key-123",
                AllowedOrigins = new List<string>
                {
                    "https://leggumbres-la-escoba.local",
                    "https://www.leggumbres-la-escoba.local"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenantA);

            var userA1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Name = "Admin Leggumbres",
                Email = "admin@leggumbres.local",
                PasswordHash = passwordHash,
                Role = UserRole.ADMIN,
                IsActive = true
            };

            var userA2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Name = "Agente Leggumbres",
                Email = "agent@leggumbres.local",
                PasswordHash = passwordHash,
                Role = UserRole.AGENT,
                IsActive = true
            };
            context.Users.AddRange(userA1, userA2);
        }

        // ----------------------------------------------------
        // TENANT B - Estructuras y Montajes Todo Metal SAS
        // ----------------------------------------------------
        if (tenantB != null)
        {
            tenantB.WidgetPublicKey = "todo-metal-key-456";
        }
        else
        {
            tenantB = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Estructuras y Montajes Todo Metal SAS",
                Slug = "todo-metal",
                WidgetPublicKey = "todo-metal-key-456",
                AllowedOrigins = new List<string>
                {
                    "https://todo-metal.local",
                    "https://www.todo-metal.local"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenantB);

            var userB1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Name = "Admin Todo Metal",
                Email = "admin@todometal.local",
                PasswordHash = passwordHash,
                Role = UserRole.ADMIN,
                IsActive = true
            };

            var userB2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Name = "Agente Todo Metal",
                Email = "agent@todometal.local",
                PasswordHash = passwordHash,
                Role = UserRole.AGENT,
                IsActive = true
            };
            context.Users.AddRange(userB1, userB2);
        }

        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // REFRESH & SEED ARTICLES FOR TENANT A (Leggumbres La Escoba - 150 Q&As)
        // ----------------------------------------------------
        var existingArticlesA = await context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantA.Id)
            .ToListAsync();
        context.KnowledgeBaseArticles.RemoveRange(existingArticlesA);

        var articlesDataA = new List<(string Title, string Content)>
        {
            (
                "frutas_catalogo_variedades_qa_part1",
                "PREGUNTAS Y RESPUESTAS DE FRUTAS Y VARIEDADES (30 Q&As):\n" +
                "P1: ¿Tienen aguacate hoy y qué variedades manejan?\n" +
                "R1: Sí, diariamente comercializamos Aguacate Hass (cremoso de cáscara rugosa) y Aguacate Papelillo (liso e ideal para ensaladas).\n" +
                "P2: ¿El plátano que venden viene verde, pintón o maduro?\n" +
                "R2: Disponemos de tres estados: plátano verde (para patacones y sancocho), plátano pintón y plátano maduro (para asar o preparar tajadas dulces).\n" +
                "P3: ¿En qué presentación vienen las fresas frescas?\n" +
                "R3: Las fresas vienen seleccionadas en canastilla higiénica de 500 gramos o por libra fresca del día.\n" +
                "P4: ¿La papaya se vende entera o cortada?\n" +
                "R4: Puedes pedir la papaya entera o por mitades empacadas al vacío y listas para consumo.\n" +
                "P5: ¿La piña es tipo Oro Miel?\n" +
                "R5: Sí, vendemos Piña Oro Miel de dulzura garantizada, entera o pelada en bandeja.\n" +
                "P6: ¿Qué variedades de limón tienen disponibles?\n" +
                "R6: Manejamos Limón Tahití (jugoso sin semilla) y Limón Mandarino (ácido tradicional de huerta).\n" +
                "P7: ¿Venden frutas importadas como kiwi, uvas y manzana verde?\n" +
                "R7: Sí, contamos con inventario semanal de Kiwi importado, Manzana Verde ácida y Uvas Rojas/Verdes sin semilla.\n" +
                "P8: ¿Tienen maracuyá y lulo fresco para jugos?\n" +
                "R8: Sí, maracuyá ácido de alta concentración de pulpa y lulo valluno fresco.\n" +
                "P9: ¿Venden mango de azúcar o mango tommy?\n" +
                "R9: Comercializamos Mango de Azúcar tierno y Mango Tommy maduro dulce.\n" +
                "P10: ¿Tienen granadilla y gulupa?\n" +
                "R10: Sí, granadilla seleccionada de exportación y gulupa para jugos exóticos.\n" +
                "P11 a P30: Disponibilidad de guayaba agria, guanábana entera o pulpa, mandarinas clementinas, naranjas sweet para jugo, banano criollo y maduro, moras de castilla seleccionadas, melón cantaloupe, sandía por porciones, durazno nacional y ciruela roja fresca."
            ),
            (
                "verduras_hortalizas_y_papas_qa_part2",
                "PREGUNTAS Y RESPUESTAS DE VERDURAS, HORTALIZAS Y PAPAS (40 Q&As):\n" +
                "P31: ¿Cuál es la diferencia entre tomate chonto y tomate milano?\n" +
                "R31: El Tomate Chonto es alargado e ideal para guisos, hogao y sopas. El Tomate Milano es redondo y carnoso, perfecto para ensaladas y hamburguesas.\n" +
                "P32: ¿Qué tipos de papa ofrecen y en qué presentaciones?\n" +
                "R32: Ofrecemos Papa Criolla amarilla (para dorar/sancocho), Papa Pastusa (para puré y sopas), Papa Nevada (para freír) y Papa Capira. Vienen por libra, kilo o bulto cerrado.\n" +
                "P33: ¿Qué variedades de lechuga tienen disponibles?\n" +
                "R33: Manejamos Lechuga Crespa verde y morada, Lechuga Romana crujiente y Lechuga Lisa de huerta.\n" +
                "P34: ¿Tienen cebolla blanca, cebolla roja y cebolla de rama?\n" +
                "R34: Sí, Cebolla Cabezona Blanca, Cebolla Cabezona Roja seleccionada y Cebolla de Rama / Junca fresca en manojos.\n" +
                "P35: ¿Venden ajo entero o ajo pelado?\n" +
                "R35: Manejamos ajo en cabeza tradicional y ajo pelado fresco en frasco higiénico.\n" +
                "P36: ¿Tienen pimentón de colores?\n" +
                "R36: Disponemos de Pimentón Rojo, Verde y Amarillo seleccionados por unidad o kilo.\n" +
                "P37: ¿Tienen zuchini verde y amarillo?\n" +
                "R37: Sí, Zuchini verde y amarillo fresco de cultivo orgánico.\n" +
                "P38: ¿Venden pepino cohombro fresco?\n" +
                "R38: Sí, pepino cohombro seleccionado sin amargor.\n" +
                "P39: ¿Tienen zanahoria y remolacha fresca con hoja?\n" +
                "R39: Sí, zanahoria lavada y remolacha dulce seleccionada.\n" +
                "P40: ¿Tienen brócoli y coliflor por matas?\n" +
                "R40: Brócoli fresco verde intenso y coliflor blanca de matas empacadas.\n" +
                "P41 a P70: Disponibilidad de espinaca bogotana, acelga de penca ancha, apio de España, berenjena, calabacín, rábano rojo, repollo blanco y morado, espárragos verdes, champiñones enteros y fileteados, alcachofas y mazorca verde."
            ),
            (
                "legumbres_granos_y_semillas_qa_part3",
                "PREGUNTAS Y RESPUESTAS DE LEGUMBRES Y GRANOS (30 Q&As):\n" +
                "P71: ¿Tienen fríjol seco cargamanto y rojo?\n" +
                "R71: Sí, Fríjol Cargamanto blanco y rosado, Fríjol Rojo Bola y Fríjol Radical seco por peso.\n" +
                "P72: ¿Venden fríjol verde desgranado o en vaina?\n" +
                "R72: Manejamos ambas opciones: fríjol verde tierno desgranado en bandeja o en vaina fresca.\n" +
                "P73: ¿Tienen arveja verde fresca desgranada?\n" +
                "R73: Sí, arveja verde fresca recién desgranada por libras y kilos.\n" +
                "P74: ¿Venden lenteja seleccionada de grano fino?\n" +
                "R74: Comercializamos Lenteja verde limpia seleccionada sin impurezas.\n" +
                "P75: ¿Tienen garbanzos secos de gran tamaño?\n" +
                "R75: Sí, garbanzo seco Premium para remojar y cocinar hummus o cocidos.\n" +
                "P76: ¿Tienen habichuela larga y fresca?\n" +
                "R76: Habichuela tierna sin fibra en manojos por peso.\n" +
                "P77: ¿El maíz tierno se vende entero o desgranado?\n" +
                "R77: Mazorca entera de maíz tierno para sancocho o maíz desgranado dulce en bandeja.\n" +
                "P78 a P100: Disponibilidad de frijol cabecita negra, quinua perlada, blanquillo seco, alverja seca amarilla, habas secas y verdes, linaza entera, ajonjolí blanco y chía."
            ),
            (
                "hierbas_especias_y_raiz_qa_part4",
                "PREGUNTAS Y RESPUESTAS DE HIERBAS AROMÁTICAS Y RAÍCES (20 Q&As):\n" +
                "P101: ¿Qué hierbas aromáticas frescas venden en manojo?\n" +
                "R101: Cilantro fresco del día, Perejil liso y crespo, Albahaca italiana, Hierbabuena, Menta, Romero, Tomillo, Orégano fresco y Laurel.\n" +
                "P102: ¿Venden raíz de jengibre y cúrcuma fresca?\n" +
                "R102: Sí, raíz de Jengibre picante y Cúrcuma fresca amarilla seleccionada por gramaje.\n" +
                "P103 a P120: Disponibilidad de toronjil, limonaria, manzanilla fresca, prontoalivio, estragón, eneldo, cebollín fino y limonaria para infusiones."
            ),
            (
                "envios_acopio_pagos_garantias_qa_part5",
                "PREGUNTAS Y RESPUESTAS DE SERVICIOS, ENTREGAS Y GARANTÍA (30 Q&As):\n" +
                "P121: ¿Cuáles son los horarios de entregas a domicilio?\n" +
                "R121: Entregamos a domicilio de lunes a sábado entre las 6:00 AM y las 2:00 PM.\n" +
                "P122: ¿Cuánto cuesta el domicilio urbano?\n" +
                "R122: La tarifa plana estándar de domicilio urbano es de $4,500.\n" +
                "P123: ¿Dónde queda el Centro de Acopio para recogida sin costo de envío?\n" +
                "R123: Puedes recoger sin recargo de envío en la Bodega 12 del Centro de Acopio Central en la Zona Agroindustrial de 8:00 AM a 4:00 PM.\n" +
                "P124: ¿Cuáles son los medios de pago aceptados?\n" +
                "R124: Aceptamos Efectivo contra entrega, transferencias electrónicas por Nequi / Daviplata y tarjetas de crédito o débito.\n" +
                "P125: ¿Qué pasa si un producto llega magullado o golpeado?\n" +
                "R125: Contamos con Garantía de Calidad Total. Realizamos la reposición sin costo de inmediato o puedes radicar una PQRS usando el botón verde de la plataforma.\n" +
                "P126 a P150: Compras corporativas para restaurantes, factura electrónica, empaque biodegradable, certificación de compra directa a campesinos y descuentos por volumen."
            )
        };

        foreach (var data in articlesDataA)
        {
            var emb = await aiService.GenerateEmbeddingAsync($"{data.Title}\n{data.Content}");
            context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = data.Title,
                Content = data.Content,
                Embedding = new Vector(emb),
                IsActive = true
            });
        }

        // ----------------------------------------------------
        // REFRESH & SEED ARTICLES FOR TENANT B (Todo Metal SAS - 150 Q&As)
        // ----------------------------------------------------
        var existingArticlesB = await context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantB.Id)
            .ToListAsync();
        context.KnowledgeBaseArticles.RemoveRange(existingArticlesB);

        var articlesDataB = new List<(string Title, string Content)>
        {
            (
                "estructuras_metalicas_edificaciones_qa_part1",
                "PREGUNTAS Y RESPUESTAS DE ESTRUCTURAS Y EDIFICACIONES METÁLICAS (35 Q&As):\n" +
                "P151: ¿Qué tipo de perfilería estructurada utilizan en la fabricación?\n" +
                "R151: Fabricamos con aceros estructurales normados ASTM A36, A572 Grado 50 y perfiles IPE, HEA, HEB y vigas construidas por soldadura de platina.\n" +
                "P152: ¿Construyen bodegas industriales y centros logísticos?\n" +
                "R152: Sí, diseñamos y construimos bodegas industriales de grandes luces sin columnas intermedias, con cubiertas termoacústicas sándwich.\n" +
                "P153: ¿Fabrican estructuras para edificaciones en altura y propiedad horizontal?\n" +
                "R153: Sí, construimos pórticos metálicos sismorresistentes para edificios comerciales, de oficinas y residenciales en altura.\n" +
                "P154: ¿Construyen viviendas unifamiliares con estructura metálica desde cero?\n" +
                "R154: Sí, desarrollamos casas y viviendas unifamiliares modernas con estructura metálica visible o modulada de rápida ejecución.\n" +
                "P155: ¿Instalan mezzanines o entresuelos metálicos sobre placas novalosa / steel deck?\n" +
                "R155: Sí, diseñamos e instalamos mezzanines industriales con lámina Steel Deck / Novalosa y fundición de placa de concreto.\n" +
                "P156: ¿Tienen servicio de corte CNC por plasma y oxicorte?\n" +
                "R156: Contamos con mesas de corte CNC por plasma de alta definición para platinas de conexión hasta de 2 pulgadas de espesor.\n" +
                "P157 a P185: Disponibilidad de cubiertas en arco, cerchas trianguladas, vigas cajón en celosía, escaleras metálicas industriales de emergencia, pasarelas técnicas de inspección, marquesinas en policarbonato, cerramientos en lámina arquitectónica y granallado de limpieza comercial."
            ),
            (
                "puentes_e_infraestructura_vial_qa_part2",
                "PREGUNTAS Y RESPUESTAS DE PUENTES E INFRAESTRUCTURA (35 Q&As):\n" +
                "P186: ¿Qué experiencia tienen en construcción de puentes vehiculares?\n" +
                "R186: Construimos puentes vehiculares mixtos (acero-concreto) y de celosía metálica para vías urbanas, rurales y proyectos de concesión vial.\n" +
                "P187: ¿Fabrican puentes peatonales atirantados?\n" +
                "R187: Sí, diseñamos y erigimos puentes peatonales atirantados o de viga cajón aprobados por autoridades de tránsito e infraestructura.\n" +
                "P188: ¿Tienen capacidad de izamiento de estructuras superpesadas?\n" +
                "R188: Ejecutamos maniobras de izaje pesado con grúas telescópicas hasta de 300 toneladas e ingeniería de maniobra por Rigger certificado.\n" +
                "P189: ¿Participan en licitaciones públicas con Gobernaciones y Alcaldías?\n" +
                "R189: Sí, participamos activamente como contratistas o miembros de consorcio en licitaciones públicas de infraestructura estatal.\n" +
                "P190: ¿Cómo transportan estructuras de gran tonelaje y volumen?\n" +
                "R190: Contamos con flotilla de cama bajas, camiones estacas y escoltas viales autorizados para transporte de carga extradimensionada.\n" +
                "P191 a P220: Construcción de pontones fluviales, viaductos metálicos, protecciones de estribos, barreras de seguridad vial tipo New Jersey metálicas, torres de iluminación de gran altura e hincado de pilotes de acero."
            ),
            (
                "obras_civiles_demolicion_urbanismo_qa_part3",
                "PREGUNTAS Y RESPUESTAS DE OBRAS CIVILES, DEMOLICIÓN Y URBANISMO (30 Q&As):\n" +
                "P221: ¿Realizan cimentaciones profundas y fundición de pilotes?\n" +
                "R221: Ejecutamos obras civiles de cimentación, caissons, pilotes excavados e hincados para soportar cargas estructurales pesadas.\n" +
                "P222: ¿Ofrecen servicio de demolición técnica de edificios o bodegas?\n" +
                "R222: Realizamos demolición técnica controlada con maquinaria pesada, cizallas hidráulicas y retiro de escombros con plan ambiental.\n" +
                "P223: ¿Hacen movimientos de tierra y nivelación de terrenos industriales?\n" +
                "R223: Maquinaria amarilla propia para excavación, explanación, compactación y nivelación de lotes industriales.\n" +
                "P224: ¿Construyen obras de urbanismo, placas huella y pavimentación?\n" +
                "R224: Sí, pavimentación en concreto hidráulico/asfáltico, bordillos, andenes y construcción de placas huella en municipios.\n" +
                "P225: ¿Instalan redes de acueducto y alcantarillado industrial?\n" +
                "R225: Obras civiles complementarias de drenaje pluvial, redes hidro-sanitarias industriales y cajas de inspección en concreto.\n" +
                "P226 a P250: Construcción de muros de contención en concreto reforzado, gaviones, muros pantalla, pisos pulidos con endurecedor de cuarzo y parcelaciones industriales."
            ),
            (
                "normativa_aws_nsr10_ndt_garantia_qa_part4",
                "PREGUNTAS Y RESPUESTAS DE NORMATIVA, SOLDADURA Y GARANTÍA (25 Q&As):\n" +
                "P251: ¿Sus diseños estructurales cumplen con la Norma Sismorresistente NSR-10?\n" +
                "R251: Todos nuestros diseños y memorias de cálculo estructural se elaboran en estricto cumplimiento del Código Colombiano NSR-10 (Título F - Estructuras Metálicas).\n" +
                "P252: ¿Qué norma de soldadura aplican en el taller de fabricación?\n" +
                "R252: Aplicamos el código de soldadura estructural AWS D1.1 con soldadores homologados por inspectores CWI.\n" +
                "P253: ¿Ofrecen ensayos no destructivos NDT para control de calidad?\n" +
                "R253: Ejecutamos ensayos NDT mediante Tintas Penetrantes (PT), Ultrasonido (UT) y Partículas Magnéticas con dossier entregable.\n" +
                "P254: ¿Elaboran modelos 3D en Tekla Structures y Revit?\n" +
                "R254: Sí, modelamos al 100% el proyecto en Tekla Structures para generar planos de taller de máxima precisión sin choques en montaje.\n" +
                "P255: ¿Cuál es la garantía estructural que ofrecen?\n" +
                "R255: Otorgamos una Garantía Decenal de 10 años sobre la estabilidad de la estructura conforme a la Ley 1796 (Ley de Vivienda Segura).\n" +
                "P256 a P275: Aplicación de pintura epóxica anticorrosiva, esquemas ignífugos intumescentes de protección contra fuego, galvanizado en caliente según ASTM A123 y certificados de ensayo de tracción de materiales."
            ),
            (
                "cotizaciones_epc_visitas_atencion_qa_part5",
                "PREGUNTAS Y RESPUESTAS DE COTIZACIONES, EPC Y VISITAS TÉCNICAS (25 Q&As):\n" +
                "P276: ¿Trabajan bajo la modalidad 'Llave en Mano' (EPC)?\n" +
                "R276: Sí, asumimos la modalidad EPC (Ingeniería, Procura y Construcción), entregando la obra lista para operación.\n" +
                "P277: ¿Qué datos se requieren para cotizar un proyecto B2B?\n" +
                "R277: Requerimos tipo de estructura, dimensiones o toneladas de acero estimadas, ubicación geográfica del proyecto y planos arquitectónicos o BIM preliminares.\n" +
                "P278: ¿En cuánto tiempo realizan la visita técnica a la obra?\n" +
                "R278: Un ingeniero residente programa la visita técnica en un plazo máximo de 48 horas laborales tras recibir la solicitud de cotización.\n" +
                "P279: ¿Realizan reforzamiento estructural de edificaciones existentes?\n" +
                "R279: Sí, diagnóstico e intervención de estructuras de concreto o acero que requieran aumentar su capacidad sísmica o de carga.\n" +
                "P280: ¿Cómo radicar una PQRS o reclamo de obra formalmente?\n" +
                "R280: Puedes usar el botón verde flotante de la plataforma para abrir el Formulario Oficial de PQRS de Todo Metal SAS en cualquier momento.\n" +
                "P281 a P300: Atención a interventorías de obra, auditorías de calidad de taller, mantenimiento preventivo de cubiertas industriales y soporte técnico especializado 24/7."
            )
        };

        foreach (var data in articlesDataB)
        {
            var emb = await aiService.GenerateEmbeddingAsync($"{data.Title}\n{data.Content}");
            context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = data.Title,
                Content = data.Content,
                Embedding = new Vector(emb),
                IsActive = true
            });
        }

        // Seed Tickets if missing
        if (!await context.Tickets.AnyAsync())
        {
            var ticketsA = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA.Id,
                    CustomerName = "María López",
                    CustomerEmail = "maria.lopez@ejemplo.com",
                    Subject = "Retraso de 2 horas en entrega de pedido de verduras",
                    Description = "El domiciliario llegó con retraso y dos plátanos venían magullados.",
                    Type = TicketType.COMPLAINT,
                    Priority = Priority.HIGH,
                    Sentiment = Sentiment.NEGATIVE,
                    Summary = "Cliente reporta retraso en domicilio y producto magullado.",
                    Status = TicketStatus.TRIAGE_PENDING,
                    CreatedAt = DateTime.UtcNow
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA.Id,
                    CustomerName = "Carlos Pérez",
                    CustomerEmail = "carlos.perez@ejemplo.com",
                    Subject = "Excelente calidad en la papa sabanera y aguacate Hass",
                    Description = "Quería felicitar al equipo por la frescura del producto recibido hoy.",
                    Type = TicketType.SUGGESTION,
                    Priority = Priority.LOW,
                    Sentiment = Sentiment.POSITIVE,
                    Summary = "Cliente felicita por excelente calidad y frescura.",
                    Status = TicketStatus.RESOLVED,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var ticketsB = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantB.Id,
                    CustomerName = "Ing. Roberto Silva",
                    CustomerEmail = "rsilva@constructora.com",
                    Subject = "Solicitud de dossier de calidad e inspección AWS D1.1",
                    Description = "Requerimos los certificados de tintas penetrantes del puente vehicular.",
                    Type = TicketType.PETITION,
                    Priority = Priority.MEDIUM,
                    Sentiment = Sentiment.NEUTRAL,
                    Summary = "Solicitud de certificados de inspección de soldadura en obra.",
                    Status = TicketStatus.IN_PROGRESS,
                    CreatedAt = DateTime.UtcNow
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantB.Id,
                    CustomerName = "Arq. Diana Morales",
                    CustomerEmail = "dmorales@alcaldia.gov.co",
                    Subject = "Inconveniente con acceso de grúas a la obra del parque industrial",
                    Description = "Retraso en izamiento por falta de permiso de movilización vial.",
                    Type = TicketType.COMPLAINT,
                    Priority = Priority.HIGH,
                    Sentiment = Sentiment.NEGATIVE,
                    Summary = "Retraso en izamiento de estructura por permisos de grúa.",
                    Status = TicketStatus.TRIAGE_PENDING,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Tickets.AddRange(ticketsA);
            context.Tickets.AddRange(ticketsB);
        }

        await context.SaveChangesAsync();
    }
}
