using System;

namespace ImageProcessing
{
    public class Threshold
    {
        /// <summary>
        /// Threshold based on gray average
        /// </summary>
        /// <param name="histogram">Histogram of grayscale image</param>
        /// <returns></returns>
        public static int GetMeanThreshold(int[] histogram)
        {
            int sum = 0, amount = 0;
            for (int y = 0; y < 256; y++)
            {
                amount += histogram[y];
                sum += y * histogram[y];
            }

            return sum / amount;
        }

        /// <summary>
        /// Otsu's threshold algorithm
        /// Reference:
        /// M. Emre Celebi 6.15.2007, Fourier Library https://sourceforge.net/projects/fourier-ipal/
        /// </summary>
        /// <param name="histogram">Histogram of grayscale image</param>
        /// <returns></returns>
        public static int GetOTSUThreshold(int[] histogram)
        {
            int x, y, amount = 0;
            int pixelBack = 0;
            int pixelIntegralBack = 0;
            int pixelIntegral = 0;
            int minValue, maxValue;
            int threshold = 0;

            for (minValue = 0; minValue < 256 && histogram[minValue] == 0; minValue++);
            for (maxValue = 255; maxValue > minValue && histogram[minValue] == 0; maxValue--);
            if (maxValue == minValue) return maxValue; // There is only one color in the image             
            if (minValue + 1 == maxValue) return minValue; // There are only two colors in the image

            for (y = minValue; y <= maxValue; y++) amount += histogram[y]; //  Total number of pixels

            pixelIntegral = 0;
            for (y = minValue; y <= maxValue; y++) pixelIntegral += histogram[y] * y;
            double sigmaB = -1;
            for (y = minValue; y < maxValue; y++)
            {
                pixelBack += histogram[y];
                var pixelFore = amount - pixelBack;
                var omegaBack = (double) pixelBack / amount; // Variance between classes
                var omegaFore = (double) pixelFore / amount; // Variance between classes
                pixelIntegralBack += histogram[y] * y;
                var pixelIntegralFore = pixelIntegral - pixelIntegralBack;
                var microBack = (double) pixelIntegralBack / pixelBack; // Variance between classes
                var microFore = (double) pixelIntegralFore / pixelFore; // Variance between classes
                var sigma = omegaBack * omegaFore * (microBack - microFore) * (microBack - microFore); // Variance between classes
                if (sigma > sigmaB)
                {
                    sigmaB = sigma;
                    threshold = y;
                }
            }

            return threshold;
        }
        
        public static int GetYenThreshold(int[] histogram)
        {
            int ih, it;
            double[] normHisto = new double[histogram.Length]; /* normalized histogram */
            double[] p1 = new double[histogram.Length]; /* cumulative normalized histogram */
            double[] p1Sq = new double[histogram.Length];
            double[] p2Sq = new double[histogram.Length];

            int total = 0;
            for (ih = 0; ih < histogram.Length; ih++)
                total += histogram[ih];

            for (ih = 0; ih < histogram.Length; ih++)
                normHisto[ih] = (double) histogram[ih] / total;

            p1[0] = normHisto[0];
            for (ih = 1; ih < histogram.Length; ih++)
                p1[ih] = p1[ih - 1] + normHisto[ih];

            p1Sq[0] = normHisto[0] * normHisto[0];
            for (ih = 1; ih < histogram.Length; ih++)
                p1Sq[ih] = p1Sq[ih - 1] + normHisto[ih] * normHisto[ih];

            p2Sq[histogram.Length - 1] = 0.0;
            for (ih = histogram.Length - 2; ih >= 0; ih--)
                p2Sq[ih] = p2Sq[ih + 1] + normHisto[ih + 1] * normHisto[ih + 1];

            /* Find the threshold that maximizes the criterion */
            var threshold = -1;
            var maxCrit = Double.MinValue;
            for (it = 0; it < histogram.Length; it++)
            {
                var crit = -0.61 * ((p1Sq[it] * p2Sq[it]) > 0.0 ? Math.Log(p1Sq[it] * p2Sq[it]) : 0.0) +
                           2 * ((p1[it] * (1.0 - p1[it])) > 0.0 ? Math.Log(p1[it] * (1.0 - p1[it])) : 0.0);
                if (crit > maxCrit)
                {
                    maxCrit = crit;
                    threshold = it;
                }
            }

            return threshold;
        }
    }
}