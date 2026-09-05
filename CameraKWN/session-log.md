# Session Log — camera_show PowerPoint Report

**Date:** 2026-06-03
**Output file:** `Camera_Show_Modes_Report.pptx` (53.9 KB, 12 slides)
**Audience:** Engineers (technical)
**Language:** English

## Slides generated
1. Title slide — 5 mode badges
2. System Overview — stack + mode summary table
3. Mode 1: Normal — live view, black frame detection
4. Mode 2: Read2D — pipeline overview (4-stage flow)
5. Mode 2: Read2D — Canny algorithm + ZXing decode strategy
6. Mode 3: Compare Image — pipeline + sequence logic
7. Mode 3: Compare Image — KAZE + Flann + Homography details
8. Mode 4: Check LED — color mask + persistence logic
9. Mode 5: Blink LED — state machine + formulas
10. Mode 5: Blink LED — 3-sample stability validation
11. All-modes comparison table
12. Shared infrastructure (ROI, camera config, result output, failure recovery)

## Key content
- 5 modes: Normal, Read2D, Compare Image, Check LED, Blink LED
- KAZE thresholds: Lowe=0.80, min matches=4, reproj=2px, orientation dev=20deg
- Canny: high=180, low=120; angle 70-100deg; area>50px
- Check LED: HsvMask>=10px, HsvTimeout>=100ms (continuous)
- Blink LED: TimeAckMin=2000ms, MaxDiffPercent=5.0%, 3-sample rolling window
