//----------------------------------------------------------------------------
//  Based on Emgu CV sample code (C) 2004-2020 EMGU Corporation.
//  Reorganised and fixed:
//    • Removed static bool num_object global side-effect channel.
//    • Draw() now returns objectFound via an out parameter.
//    • modelDescriptors and observedDescriptors are properly disposed.
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace camera_show {
    public static class DrawMatches {
        /// <summary>
        /// Detects KAZE features in both images and attempts to find a homography.
        /// </summary>
        public static void FindMatch(
            Mat modelImage, Mat observedImage,
            out long matchTime,
            out VectorOfKeyPoint modelKeyPoints,
            out VectorOfKeyPoint observedKeyPoints,
            VectorOfVectorOfDMatch matches,
            out Mat mask,
            out Mat homography)
        {
            const int    k                   = 2;
            const double uniquenessThreshold = 0.80;

            homography          = null;
            modelKeyPoints      = new VectorOfKeyPoint();
            observedKeyPoints   = new VectorOfKeyPoint();

            using (var uModel    = modelImage.GetUMat(AccessType.Read))
            using (var uObserved = observedImage.GetUMat(AccessType.Read))
            using (var detector  = new KAZE())
            using (var modelDesc    = new Mat())   // FIX: dispose descriptors
            using (var observedDesc = new Mat())   // FIX: dispose descriptors
            {
                detector.DetectAndCompute(uModel, null, modelKeyPoints, modelDesc, false);

                var watch = Stopwatch.StartNew();
                try {
                    detector.DetectAndCompute(uObserved, null, observedKeyPoints, observedDesc, false);
                } catch { /* observed image may be too small; leave observedKeyPoints empty */ }

                using (var ip      = new LinearIndexParams())
                using (var sp      = new SearchParams())
                using (var matcher = new FlannBasedMatcher(ip, sp))
                {
                    matcher.Add(modelDesc);
                    try { matcher.KnnMatch(observedDesc, matches, k, null); } catch { }

                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZero = CvInvoke.CountNonZero(mask);
                    if (nonZero >= 4) {
                        nonZero = Features2DToolbox.VoteForSizeAndOrientation(
                            modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
                        if (nonZero >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(
                                modelKeyPoints, observedKeyPoints, matches, mask, 2);
                    }
                }
                watch.Stop();
                matchTime = watch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Draws feature matches and the projected model outline.
        /// </summary>
        /// <param name="objectFound">
        /// True when a valid homography was found (i.e., the model object is present
        /// in the observed image).
        /// FIX: was previously a static field — now a proper out parameter.
        /// </param>
        public static Mat Draw(Mat modelImage, Mat observedImage,
                               out long matchTime, out bool objectFound)
        {
            objectFound = false;
            VectorOfKeyPoint modelKP, observedKP;
            Mat mask, homography;

            using (var matches = new VectorOfVectorOfDMatch()) {
                FindMatch(modelImage, observedImage, out matchTime,
                          out modelKP, out observedKP, matches,
                          out mask, out homography);

                var result = new Mat();
                Features2DToolbox.DrawMatches(
                    modelImage, modelKP, observedImage, observedKP,
                    matches, result,
                    new MCvScalar(255, 0, 255), new MCvScalar(0, 255, 255), mask);

                if (homography != null) {
                    var rect = new Rectangle(Point.Empty, modelImage.Size);
                    var pts  = new PointF[] {
                        new PointF(rect.Left,  rect.Bottom),
                        new PointF(rect.Right, rect.Bottom),
                        new PointF(rect.Right, rect.Top),
                        new PointF(rect.Left,  rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    var points = new Point[pts.Length];
                    for (int i = 0; i < points.Length; i++)
                        points[i] = Point.Round(pts[i]);
                    using (var vp = new VectorOfPoint(points)) {
                        // outline drawing omitted (was commented out in original)
                    }
                    objectFound = true;
                }
                return result;
            }
        }
    }
}
