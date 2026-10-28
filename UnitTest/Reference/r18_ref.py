"""Independent design law of R18 (decision of the ANTHEA user of 26/10/2026, «R18 ok correggi»; 28/10 «Sì, procedi e misura»).
EN 1992-1-1 3.2.7(2) b and Fig. 3.8 (3.3.6(7) and Fig. 3.10 for the tendons): beyond εyd = fyd/E the design branch goes from fyd at εyd to
k fyk/γs at εu: it is the characteristic curve divided by γs = fy/fyd, read at the strain s = εyk + (|t| − εyd)(εu − εyk)/(εu − εyd).
Characteristic curve: E t up to εyk = fy/E, then linear to (εu, fu); zero beyond εu. Total strain t = ε + εp. No GPC code used.
"""


def characteristic(t, E, fy, fu, eu):
    a = abs(t)
    if a > eu: return 0.0
    s = E * a if a <= fy / E else fy + (fu - fy) / (eu - fy / E) * (a - fy / E)
    return s if t >= 0 else -s


def design(t, E, fy, fu, eu, gamma):
    fyd = fy / gamma; eyd = fyd / E; eyk = fy / E; a = abs(t)
    if a > eu: return 0.0
    if a <= eyd: return E * t
    s = eyk + (a - eyd) * (eu - eyk) / (eu - eyd)
    sign = 1 if t >= 0 else -1
    return sign * characteristic(s, E, fy, fu, eu) / gamma


ep = 1500 / 195000
tendon = (195000, 1670, 1860, 0.035, 1.15)
print('TendonDesignStressUsesTheTotalStrain')
for e in (-0.0005, -0.0001, 0, 0.0001, 0.002, 0.028):
    print(f'  ε = {e}: t = {(e + ep) * 1e3:.4f} ‰, σ = {design(e + ep, *tendon):.6f}')
print('DesignStressFromTheCharacteristicStressUsesTheTotalStrain')
for e in (-0.04, -0.0005, -0.0001, 0, 0.026, 0.0272, 0.028):
    print(f'  ε = {e}: t = {(e + ep) * 1e3:.4f} ‰, σ = {design(e + ep, *tendon):.6f}')
s355 = (210000, 355, 510, 0.15)
print(f'  EN 1993 γM0 1.05 t = -148.5 ‰: {design(-0.1505 + 0.002, *s355, 1.05):.9f}')
print(f'  AISC fyd = fy t = -148.5 ‰: {design(-0.1505 + 0.002, *s355, 1.0):.9f}')
print('B450C (k = 1.15, εuk = 7.5 %, εud = 0.9 εuk not applied here) at εu: ', f'{design(0.075, 200000, 450, 540, 0.075, 1.15):.6f}', '= k fyk / γs =', f'{540 / 1.15:.6f}')
