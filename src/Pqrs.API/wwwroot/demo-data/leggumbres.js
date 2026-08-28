/**
 * Leggumbres La Escoba - Seed Data Centralizado (Modo Demostración)
 */
window.LEGGUMBRES_DEMO_DATA = {
  mode: "DEMO_DATA",
  bannerMessage: "Modo Demostración - Datos de Ejemplo Centralizados",
  categories: [
    { id: "all", name: "Todos los Productos", icon: "🧺" },
    { id: "tuberculos", name: "Tubérculos y Raíces", icon: "🥔" },
    { id: "verduras", name: "Verduras y Hortalizas", icon: "🥦" },
    { id: "frutas", name: "Frutas Frescas", icon: "🍎" },
    { id: "granos", name: "Legumbres y Granos", icon: "🫘" },
    { id: "complementarios", name: "Lácteos y Abarrotes", icon: "🥚" }
  ],
  products: [
    {
      id: "prod-1",
      name: "Papa Sabanera Campesina",
      category: "tuberculos",
      price: 2400,
      unit: "Libra (500g)",
      image: "https://images.unsplash.com/photo-1518977676601-b53f82aba655?w=500&auto=format&fit=crop&q=60",
      description: "Papa sabanera recién cosechada, ideal para sancocho, caldos y espesar guisos.",
      badge: "Cosecha Hoy",
      farmer: "Don Gonzalo (Finca El Mirador)",
      available: true
    },
    {
      id: "prod-2",
      name: "Aguacate Hass Seleccionado",
      category: "frutas",
      price: 4500,
      unit: "Unidad (~250g)",
      image: "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=500&auto=format&fit=crop&q=60",
      description: "Aguacate Hass cremoso de primera calidad. Puedes solicitar el punto de maduración en tu pedido.",
      badge: "Favorito",
      farmer: "Familia Rodríguez",
      available: true
    },
    {
      id: "prod-3",
      name: "Yuca Fresca de Campo",
      category: "tuberculos",
      price: 2200,
      unit: "Libra (500g)",
      image: "https://images.unsplash.com/photo-1594282486552-05b4d80fbb9f?w=500&auto=format&fit=crop&q=60",
      description: "Yuca garantizada de buena calidad que ablanda fácilmente al cocinar.",
      badge: "Garantizada",
      farmer: "Asociación Campesina del Valle",
      available: true
    },
    {
      id: "prod-4",
      name: "Tomate Chonto de Invernadero",
      category: "verduras",
      price: 1800,
      unit: "Libra (500g)",
      image: "https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=500&auto=format&fit=crop&q=60",
      description: "Tomate chonto firme y jugoso para guisos o ensaladas frescas.",
      badge: "Fresco",
      farmer: "Doña Marina",
      available: true
    },
    {
      id: "prod-5",
      name: "Plátano Verde para Patacón",
      category: "tuberculos",
      price: 1900,
      unit: "Libra (500g)",
      image: "https://images.unsplash.com/photo-1603833665858-e61d17a86224?w=500&auto=format&fit=crop&q=60",
      description: "Plátano verde duro y fresco, seleccionado para patacones crujientes.",
      badge: "Top Ventas",
      farmer: "Parcela La Esperanza",
      available: true
    },
    {
      id: "prod-6",
      name: "Fresas Frescas de Sabana",
      category: "frutas",
      price: 5800,
      unit: "Bandeja (500g)",
      image: "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?w=500&auto=format&fit=crop&q=60",
      description: "Fresas dulces recién recolectadas, empacadas en empaque biodegradable.",
      badge: "Dulce",
      farmer: "Cultivos El Manantial",
      available: true
    },
    {
      id: "prod-7",
      name: "Fríjol Cargamanto Verde",
      category: "granos",
      price: 3800,
      unit: "Libra (500g)",
      image: "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=500&auto=format&fit=crop&q=60",
      description: "Fríjol verde desgranado fresco, suave y de rápido cocimiento.",
      badge: "Orgánico",
      farmer: "Don Mateo",
      available: true
    },
    {
      id: "prod-8",
      name: "Huevos Campesinos AAA",
      category: "complementarios",
      price: 18500,
      unit: "Cubeta (30 und)",
      image: "https://images.unsplash.com/photo-1516448620398-c5f44bf9f441?w=500&auto=format&fit=crop&q=60",
      description: "Huevos de gallina feliz alimentadas libremente con grano natural.",
      badge: "Nutritivo",
      farmer: "Granja La Pradera",
      available: true
    }
  ],
  farmers: [
    {
      name: "Don Gonzalo Martínez",
      location: "Finca El Mirador",
      specialty: "Papa Sabanera y Criolla",
      quote: "Llevo 25 años cultivando la mejor papa del campo sin intermediarios."
    },
    {
      name: "Doña Marina Gómez",
      location: "Parcela La Esperanza",
      specialty: "Tomate y Hortalizas",
      quote: "Cuidamos cada mata con agua limpia para entregar comida sana a las familias."
    }
  ],
  demoOrders: [
    {
      id: "PED-84920",
      date: "2026-08-26",
      items: "2x Papa Sabanera, 1x Aguacate Hass, 1x Fresas",
      total: 15100,
      status: "Entregado",
      deliveryType: "Domicilio Directo"
    },
    {
      id: "PED-84955",
      date: "2026-08-27",
      items: "1x Yuca Fresca, 1x Plátano Verde, 1x Huevos AAA",
      total: 22600,
      status: "En Camino",
      deliveryType: "Recogida Centro de Acopio"
    }
  ]
};
