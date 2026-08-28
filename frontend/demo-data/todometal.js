/**
 * Estructuras y Montajes Todo Metal SAS - Seed Data Centralizado (Modo Demostración)
 */
window.TODOMETAL_DEMO_DATA = {
  mode: "DEMO_DATA",
  bannerMessage: "Modo Demostración - Datos de Ejemplo Centralizados",
  services: [
    {
      id: "srv-estructuras",
      title: "Fabricación y Montaje de Estructuras Metálicas",
      icon: "🏗️",
      summary: "Diseño, fabricación en taller y montaje en sitio de estructuras pesadas de acero sismorresistente bajo norma NSR-10.",
      details: [
        "Voguería y perfilería estructural ASTM A36 / A572",
        "Ingeniería de detalle en software BIM 3D / Tekla Structures",
        "Soldadura especializada certificada bajo norma AWS D1.1",
        "Pintura anticorrosiva e ignífuga de alta durabilidad"
      ]
    },
    {
      id: "srv-puentes",
      title: "Puentes Vehiculares y Peatonales",
      icon: "🌉",
      summary: "Construcción integral de puentes de estructura metálica y mixta para vías urbanas, rurales e infraestructura pública.",
      details: [
        "Puentes de vigas cajón y celosía metálica",
        "Puentes peatonales atirantados y de tablero de concreto",
        "Inspección de ensayos no destructivos (Tintas y Ultrasonido)",
        "Montaje con grúas de gran tonelaje"
      ]
    },
    {
      id: "srv-montajes",
      title: "Montaje Industrial y Cubiertas",
      icon: "🏢",
      summary: "Montaje técnico de naves industriales, centros logísticos, bodegas comerciales y cubiertas autoportantes.",
      details: [
        "Izamiento seguro con plan de izaje Rigger",
        "Cubiertas en teja sándwich termoacústica y policarbonato",
        "Ingeniero residente permanente en obra",
        "Garantía de calidad sismorresistente Ley 1796 (10 años)"
      ]
    },
    {
      id: "srv-infraestructura",
      title: "Obras Civiles e Infraestructura Pública",
      icon: "🛣️",
      summary: "Ejecución de obras complementarias de cimentación profunda, pilotes, placas de concreto y urbanismo.",
      details: [
        "Cimentaciones especiales para soporte de estructuras",
        "Placas de contrapiso industrial pulido",
        "Adecuación de vías de acceso y movimiento de tierras",
        "Licitaciones públicas y contratos gubernamentales"
      ]
    }
  ],
  projects: [
    {
      id: "proj-1",
      title: "Puente Vehicular Metálico Río Claro",
      category: "Puentes",
      client: "Gobernación / Infraestructura Vial (Licitación Demo)",
      tonnage: "185 Toneladas de Acero",
      status: "Finalizado",
      year: "2025",
      image: "https://images.unsplash.com/photo-1545558014-8692077e9b5c?w=600&auto=format&fit=crop&q=60",
      description: "Puente vehicular de celosía metálica de 45 metros de luz libre sismorresistente bajo norma CCP-14 y NSR-10."
    },
    {
      id: "proj-2",
      title: "Nave Logística y Bodega Industrial Norte",
      category: "Estructuras",
      client: "Centro Logístico Industrial (Sector Privado Demo)",
      tonnage: "320 Toneladas de Acero",
      status: "En Ejecución (85%)",
      year: "2026",
      image: "https://images.unsplash.com/photo-1581094794329-c8112a89af12?w=600&auto=format&fit=crop&q=60",
      description: "Fabricación e instalación de nave industrial de 12.000 m² con luces libres de 30 metros y puente grúa de 10 toneladas."
    },
    {
      id: "proj-3",
      title: "Puente Peatonal Urbano Atirantado",
      category: "Puentes",
      client: "Alcaldía Municipal (Obras Públicas Demo)",
      tonnage: "65 Toneladas de Acero",
      status: "Finalizado",
      year: "2025",
      image: "https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600&auto=format&fit=crop&q=60",
      description: "Estructura peatonal atirantada de diseño moderno con rampas de acceso para movilidad reducida."
    }
  ],
  demoRequests: [
    {
      id: "COT-9401",
      projectType: "Nave Industrial y Cubierta",
      dimensions: "40m x 80m (3.200 m²)",
      location: "Parque Industrial Norte",
      status: "En Revisión Técnica",
      date: "2026-08-25"
    },
    {
      id: "COT-9418",
      projectType: "Puente Peatonal Metálico",
      dimensions: "Luz de 28m x 2.5m de ancho",
      location: "Municipio San Mateo",
      status: "Cotización Enviada",
      date: "2026-08-27"
    }
  ]
};
