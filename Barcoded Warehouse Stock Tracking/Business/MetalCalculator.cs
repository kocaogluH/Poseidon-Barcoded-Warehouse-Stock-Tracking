using System;

namespace Barcoded_Warehouse_Stock_Tracking.Business
{
    public static class MetalCalculator
    {
        // Malzeme yoğunluk katsayıları (g/cm³ -> kg/dm³)
        public const double DensitySteel = 7.85;      // Karbon Çelik / Demir
        public const double DensityStainless = 7.93;  // Paslanmaz Çelik (304)
        public const double DensityAluminum = 2.70;   // Alüminyum
        public const double DensityBrass = 8.50;      // Pirinç / Sarı
        public const double DensityCopper = 8.96;     // Bakır

        public static double GetMaterialDensity(string qualityOrMaterial)
        {
            if (string.IsNullOrWhiteSpace(qualityOrMaterial)) return DensitySteel;
            var lower = qualityOrMaterial.ToLower();
            if (lower.Contains("paslanmaz") || lower.Contains("304") || lower.Contains("316") || lower.Contains("inox"))
                return DensityStainless;
            if (lower.Contains("aluminyum") || lower.Contains("alüminyum") || lower.Contains("6063") || lower.Contains("5083"))
                return DensityAluminum;
            if (lower.Contains("pirinç") || lower.Contains("pirinc") || lower.Contains("sarı"))
                return DensityBrass;
            if (lower.Contains("bakır") || lower.Contains("bakir"))
                return DensityCopper;
            return DensitySteel;
        }

        /// <summary>
        /// Kutu / Dikdörtgen Profil Birim Ağırlığı (kg/mt)
        /// width: Genişlik A (mm), height: Yükseklik B (mm), thickness: Et Kalınlığı s (mm)
        /// </summary>
        public static double CalculateBoxProfileWeight(double width, double height, double thickness, double density = DensitySteel)
        {
            if (width <= 0 || height <= 0 || thickness <= 0) return 0;
            // Formül: ((A + B) * 2 - 4 * s) * s * yoğunluk / 1000
            double area = ((width + height) * 2.0 - 4.0 * thickness) * thickness; // mm²
            return Math.Round((area * density) / 1000.0, 3);
        }

        /// <summary>
        /// Yuvarlak Boru Birim Ağırlığı (kg/mt)
        /// outerDiameter: Dış Çap (mm), thickness: Et Kalınlığı (mm)
        /// </summary>
        public static double CalculateRoundPipeWeight(double outerDiameter, double thickness, double density = DensitySteel)
        {
            if (outerDiameter <= 0 || thickness <= 0 || thickness >= outerDiameter) return 0;
            // Formül: (D - s) * s * PI * yoğunluk / 1000
            double factor = (Math.PI * density) / 1000.0;
            return Math.Round((outerDiameter - thickness) * thickness * factor, 3);
        }

        /// <summary>
        /// Sac / Levha Birim Ağırlığı (kg / plaka veya kg/m²)
        /// thickness: Kalınlık (mm), width: En (mm), length: Boy (mm)
        /// </summary>
        public static double CalculateSheetWeight(double thickness, double widthMm, double lengthMm, double density = DensitySteel)
        {
            if (thickness <= 0 || widthMm <= 0 || lengthMm <= 0) return 0;
            // Formül: (Kalınlık * En * Boy * Yoğunluk) / 1.000.000
            return Math.Round((thickness * widthMm * lengthMm * density) / 1000000.0, 2);
        }

        /// <summary>
        /// Yuvarlak Dolu Mil / Çubuk Birim Ağırlığı (kg/mt)
        /// diameter: Çap (mm)
        /// </summary>
        public static double CalculateRoundBarWeight(double diameter, double density = DensitySteel)
        {
            if (diameter <= 0) return 0;
            double area = Math.PI * (diameter / 2.0) * (diameter / 2.0); // mm²
            return Math.Round((area * density) / 1000.0, 3);
        }

        /// <summary>
        /// Lama / Dikdörtgen Dolu Demir Birim Ağırlığı (kg/mt)
        /// width: Genişlik / En (mm), thickness: Kalınlık (mm)
        /// </summary>
        public static double CalculateFlatBarWeight(double width, double thickness, double density = DensitySteel)
        {
            if (width <= 0 || thickness <= 0) return 0;
            return Math.Round((width * thickness * density) / 1000.0, 3);
        }

        /// <summary>
        /// Köşebent Birim Ağırlığı (kg/mt)
        /// a: En (mm), b: Boy (mm), thickness: Et Kalınlığı (mm)
        /// </summary>
        public static double CalculateAngleWeight(double a, double b, double thickness, double density = DensitySteel)
        {
            if (a <= 0 || b <= 0 || thickness <= 0) return 0;
            double area = (a + b - thickness) * thickness;
            return Math.Round((area * density) / 1000.0, 3);
        }
    }
}
