"""Independent EN 1992-1-1:2004 Annex B / 3.1.3(3) reference.
Run with an explicit NEW output path. Never reads production code or replaces expectations.
20 C, normal-weight concrete, constant humidity, linear creep.
"""
import json
import math
import sys

rows = []
for fck, rh, h0, t0, cement in [
    (25, 50, 200, 7, "N"), (30, 80, 500, 28, "N"),
    (35, 70, 560, 10, "N"), (27, 40, 100, 1, "S"),
    (27.0001, 40, 100, 1, "R"), (60, 100, 1000, 28, "S"),
    (90, 50, 100, 3, "R"), (12, 80, 10000, .5, "S"),
]:
    fcm = fck + 8
    s, alpha = {"S": (.38, -1), "N": (.25, 0), "R": (.20, 1)}[cement]
    equivalent = max(.5, t0 * (9 / (2 + t0 ** 1.2) + 1) ** alpha)
    a1, a2, a3 = [(35 / fcm) ** e for e in (.7, .2, .5)]
    rh_term = (1 - rh / 100) / (.1 * h0 ** (1 / 3))
    rh_factor = 1 + rh_term if fcm <= 35 else (1 + rh_term * a1) * a2
    ultimate = rh_factor * 16.8 / math.sqrt(fcm) / (.1 + equivalent ** .2)
    strength_factor = 1 if fcm <= 35 else a3
    beta_h = min(1.5 * (1 + (.012 * rh) ** 18) * h0 + 250 * strength_factor, 1500 * strength_factor)
    e28 = 22000 * (fcm / 10) ** .3
    for t in sorted({.5, t0, 28, t0 + 10, 365, 10000}):
        elapsed = max(0, t - t0)
        phi = ultimate * (elapsed / (elapsed + beta_h)) ** .3
        factor = math.exp(s * (1 - math.sqrt(28 / t))) ** .3
        rows.append(dict(fck=fck, rh=rh, h0=h0, t0=t0, cement=cement, age=t,
                         adjusted=equivalent, betaH=beta_h, phiInfinity=ultimate,
                         phi=phi, e28=e28, e=e28 * factor, factor=factor))
with open(sys.argv[1], "x", encoding="utf-8") as output:
    json.dump({"source": "EN 1992-1-1:2004 B.1-B.9; 3.1.3(3); independent Python", "rows": rows}, output, indent=2)
