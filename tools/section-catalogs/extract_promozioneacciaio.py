"""Extracts the catalogs of hollow sections (EN 10210-2 hot finished CHS, SHS, RHS; EN 10219-2 cold formed CHS) from the tables of
Fondazione Promozione Acciaio ("Profili cavi per la costruzione", www.promozioneacciaio.it, PDF of 2005 with the tables of the standards)
into the CSV files of GPCModelData. Every row is checked: the published area against the geometry of the standard and the mass against
7.85 kg/dm3; the rows that do not pass are reported and not written.

Usage: python extract_promozioneacciaio.py <pdf> <output folder>
"""
import csv
import datetime
import hashlib
import math
import os
import re
import sys

import pdfplumber

SOURCE = "Fondazione Promozione Acciaio, Profili cavi per la costruzione formati a caldo (EN 10210) e a freddo (EN 10219), tables of the standards"
NUMBER = r"\d+(?:[.,]\d+)?"


def value(text):
    return float(text.replace(",", "."))


def rhs_area(h, b, t, ro, ri):
    """Area of a rectangular hollow section with the corner radii of the standard"""
    return 2 * t * (h + b - 2 * t) - (4 - math.pi) * (ro * ro - ri * ri)


def write_catalog(path, title, standard, sha256, notes, fields, records):
    with open(path, "w", newline="", encoding="utf-8") as stream:
        for line in [f"Catalog: {title}",
                     f"Standard (declared by the source): {standard}",
                     f"Source: {SOURCE}",
                     "Source file: EN 10210 CHS.pdf (Fondazione Promozione Acciaio, 2005), local copy of the user",
                     f"Source SHA256: {sha256}",
                     f"Extracted: {datetime.date.today().isoformat()} by tools/section-catalogs/extract_promozioneacciaio.py",
                     "Units: mm, mm2, mm3, mm4 (It), mm3 (Ct), kg/m (G)"] + notes:
            stream.write("# " + line + "\n")
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(fields)
        for record in records:
            writer.writerow([fmt(record.get(f)) for f in fields])
    print(f"{os.path.basename(path)}: {len(records)} sections")


def fmt(v):
    if v is None:
        return ""
    if isinstance(v, float):
        v = round(v, 9)
        return str(int(v)) if v == int(v) else repr(v)
    return str(v)


def main(pdf_path, output):
    sha256 = hashlib.sha256(open(pdf_path, "rb").read()).hexdigest().upper()
    tables = {"EN 10210 circolare": [], "EN 10210 quadrati": [], "EN 10210 rettangolari": [], "EN 10219 circolare": []}
    with pdfplumber.open(pdf_path) as pdf:
        for number, page in enumerate(pdf.pages, 1):
            text = page.extract_text() or ""
            standard = "EN 10219" if "EN 10219" in text else "EN 10210"
            kind = next(k for k in ("circolare", "quadrati", "rettangolari") if "sezione " + k in text)
            count = 17 if kind == "rettangolari" else 12
            for line in text.splitlines():
                cells = line.split()
                if len(cells) == count and all(re.fullmatch(NUMBER, c) for c in cells):
                    tables[f"{standard} {kind}"].append((number, [value(c) for c in cells]))

    rejected = []

    def check(designation, area, mass, geometric):
        ok = abs(geometric / area - 1) <= 0.012 and abs(mass / (7.85e-3 * area) - 1) <= 0.012
        if not ok:
            rejected.append(f"{designation}: A {area} geometric {geometric:.1f} mass {mass}")
        return ok

    for standard, key in (("EN 10210", "EN 10210 circolare"), ("EN 10219", "EN 10219 circolare")):
        records = []
        for page, v in tables[key]:
            D, t, G, A, I, i, Wel, Wpl, It, Ct = v[:10]
            record = {"Designation": f"CHS {fmt(D)} x {fmt(t)}", "Series": "CHS", "InStandard": 1, "G": G, "D": D, "t": t, "A": A * 100,
                      "I": I * 1e4, "Wel": Wel * 1e3, "Wpl": Wpl * 1e3, "It": It * 1e4, "Ct": Ct * 1e3}
            if check(record["Designation"], record["A"], G, math.pi * (D - t) * t):
                records.append(record)
        name = "EN10210_CircularHollow.csv" if standard == "EN 10210" else "EN10219_CircularHollow.csv"
        forming = "hot finished" if standard == "EN 10210" else "cold formed"
        write_catalog(os.path.join(output, name), f"Circular hollow sections, {forming} (CHS)", f"{standard}-2 (tables reproduced by the source)",
                      sha256, ["D: outside diameter, t: wall thickness; I, Wel, Wpl about any axis; It torsion constant, Ct torsion modulus"],
                      ["Designation", "Series", "InStandard", "G", "D", "t", "A", "I", "Wel", "Wpl", "It", "Ct"], records)

    records = []
    for page, v in tables["EN 10210 quadrati"]:
        b, t, G, A, I, i, Wel, Wpl, It, Ct = v[:10]
        record = {"Designation": f"SHS {fmt(b)} x {fmt(b)} x {fmt(t)}", "Series": "SHS", "InStandard": 1, "G": G, "h": b, "b": b, "t": t,
                  "A": A * 100, "Iy": I * 1e4, "Iz": I * 1e4, "Wely": Wel * 1e3, "Welz": Wel * 1e3, "Wply": Wpl * 1e3, "Wplz": Wpl * 1e3,
                  "It": It * 1e4, "Ct": Ct * 1e3}
        if check(record["Designation"], record["A"], G, rhs_area(b, b, t, 1.5 * t, t)):
            records.append(record)
    for page, v in tables["EN 10210 rettangolari"]:
        h, b, t, G, A, Iy, Iz, iy, iz, Wely, Welz, Wply, Wplz, It, Ct = v[:15]
        record = {"Designation": f"RHS {fmt(h)} x {fmt(b)} x {fmt(t)}", "Series": "RHS", "InStandard": 1, "G": G, "h": h, "b": b, "t": t,
                  "A": A * 100, "Iy": Iy * 1e4, "Iz": Iz * 1e4, "Wely": Wely * 1e3, "Welz": Welz * 1e3, "Wply": Wply * 1e3, "Wplz": Wplz * 1e3,
                  "It": It * 1e4, "Ct": Ct * 1e3}
        if check(record["Designation"], record["A"], G, rhs_area(h, b, t, 1.5 * t, t)):
            records.append(record)
    write_catalog(os.path.join(output, "EN10210_RectangularHollow.csv"), "Square and rectangular hollow sections, hot finished (SHS, RHS)",
                  "EN 10210-2 (tables reproduced by the source)", sha256,
                  ["h: side along z (vertical), b: side along y; t: wall thickness; corner radii of the calculations of EN 10210-2: "
                   "outside 1.5 t, inside 1.0 t", "It torsion constant, Ct torsion modulus"],
                  ["Designation", "Series", "InStandard", "G", "h", "b", "t", "A", "Iy", "Iz", "Wely", "Welz", "Wply", "Wplz", "It", "Ct"], records)

    for line in rejected:
        print("REJECTED", line)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
