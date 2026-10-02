"""Extracts the catalog of the hot finished circular hollow sections of the Tata Steel Celsius range (EN 10210) into the CSV file of
GPCModelData: the published dimensions and properties of the workbook "Celsius CHS, Section properties - Dimensions and properties"
(Eurocode 3, BS EN 10210-2:2006) and the sizes of the availability table of the Celsius brochure (EN 10210 S355NH) that the workbook does not
list, whose properties are calculated from D and t with the formulas of EN 10210-2 and rounded to 3 significant figures as the workbook.
Every row is checked: the area against the geometry and the mass against 7.85 kg/dm3; the rows that do not pass are reported and not written.
InStandard is 1 when the size is in the EN 10210-2 tables reproduced by Fondazione Promozione Acciaio (EN10210_CircularHollow.csv of the
output folder), 0 otherwise.

Usage: python extract_tatasteel_celsius.py <workbook xlsx> <brochure pdf> <output folder>
"""
import csv
import datetime
import hashlib
import math
import os
import re
import sys

import openpyxl
import pdfplumber

SOURCE = ("Tata Steel, Celsius hot finished circular hollow sections: CHS, Section properties - Dimensions and properties (workbook of "
          "01/07/2020) and the availability tables of the brochure Celsius, Hot finished hollow sections to EN 10210")
THICKNESSES = [3.2, 3.6, 4.0, 5.0, 6.3, 8.0, 10.0, 12.5, 14.2, 16.0, 17.5]


def sha256(path):
    return hashlib.sha256(open(path, "rb").read()).hexdigest().upper()


def fmt(v):
    if v is None:
        return ""
    if isinstance(v, float):
        v = round(v, 9)
        return str(int(v)) if v == int(v) else repr(v)
    return str(v)


def sig3(x):
    """Rounds to 3 significant figures, as the published values of the workbook"""
    return round(x, 2 - int(math.floor(math.log10(abs(x)))))


def calculated(D, t):
    """Properties of a circular hollow section from D and t (EN 10210-2): cm2, cm4, cm3 rounded to 3 significant figures"""
    d = D - 2 * t
    I = math.pi / 64 * (D ** 4 - d ** 4)
    return {"A": sig3(math.pi * t * (D - t) / 1e2), "I": sig3(I / 1e4), "Wel": sig3(2 * I / D / 1e3),
            "Wpl": sig3((D ** 3 - d ** 3) / 6 / 1e3), "It": sig3(2 * I / 1e4), "Ct": sig3(4 * I / D / 1e3)}


def brochure_sizes(pdf_path):
    """Sizes of the CHS availability table of the brochure (EN 10210 S355NH): the thickness of each published mass is the one of the
    header whose geometric mass is within 1.2% (the text of the table loses the columns)"""
    sizes = {}
    with pdfplumber.open(pdf_path) as pdf:
        page = next(p for p in pdf.pages if re.search(r"EN 10210 S355NH.*\n.*\nCircular hollow sections \(CHS\)", p.extract_text() or ""))
        for line in page.extract_text().splitlines():
            cells = line.split()
            if not cells or not re.fullmatch(r"\d+\.\d", cells[0]):
                continue
            D = float(cells[0])
            for cell in cells[1:]:
                if "/" in cell:
                    break
                mass = float(cell)
                t = min(THICKNESSES, key=lambda x: abs(7.85e-3 * math.pi * x * (D - x) / mass - 1))
                if abs(7.85e-3 * math.pi * t * (D - t) / mass - 1) > 0.012:
                    raise ValueError(f"brochure: no thickness for the mass {mass} of D {D}")
                sizes[(D, t)] = mass
    return sizes


def main(workbook_path, brochure_path, output):
    sheet = openpyxl.load_workbook(workbook_path, data_only=True).worksheets[0]
    published = {}
    for row in sheet.iter_rows(min_row=11, values_only=True):
        if isinstance(row[0], (int, float)):
            D, t, name, G, A, dt, I, i, Wel, Wpl, It, Ct = row[:12]
            published[(float(D), float(t))] = {"G": G, "A": A, "I": I, "Wel": Wel, "Wpl": Wpl, "It": It, "Ct": Ct}

    standard = set()
    with open(os.path.join(output, "EN10210_CircularHollow.csv"), encoding="utf-8") as stream:
        for row in csv.DictReader(line for line in stream if not line.startswith("#")):
            standard.add((float(row["D"]), float(row["t"])))

    brochure = brochure_sizes(brochure_path)
    added = sorted(k for k in brochure if k not in published)
    rows = dict(published)
    for D, t in added:
        rows[(D, t)] = dict(calculated(D, t), G=brochure[(D, t)])

    records, rejected = [], []
    for (D, t) in sorted(rows):
        v = rows[(D, t)]
        designation = f"CHS {fmt(D)} x {fmt(t)}"
        area = v["A"] * 100
        if abs(math.pi * (D - t) * t / area - 1) > 0.012 or abs(v["G"] / (7.85e-3 * area) - 1) > 0.012:
            rejected.append(f"{designation}: A {area} mass {v['G']}")
            continue
        records.append({"Designation": designation, "Series": "CHS", "InStandard": 1 if (D, t) in standard else 0, "G": float(v["G"]), "D": D,
                        "t": t, "A": area, "I": v["I"] * 1e4, "Wel": v["Wel"] * 1e3, "Wpl": v["Wpl"] * 1e3, "It": v["It"] * 1e4,
                        "Ct": v["Ct"] * 1e3})

    fields = ["Designation", "Series", "InStandard", "G", "D", "t", "A", "I", "Wel", "Wpl", "It", "Ct"]
    path = os.path.join(output, "EN10210_CircularHollowCelsius.csv")
    with open(path, "w", newline="", encoding="utf-8") as stream:
        for line in ["Catalog: Circular hollow sections, hot finished (CHS), Tata Steel Celsius range",
                     "Standard (declared by the source): EN 10210-2:2006 (BS EN 10210-2, Eurocode 3 UK National Annex)",
                     f"Source: {SOURCE}",
                     "Source file: Celsius-CHS-sectionpropertiesdimensionsproperties-Eurocode3-1_7_2020.xlsx, local copy of the user",
                     f"Source SHA256: {sha256(workbook_path)}",
                     "Brochure file: https://www.tatasteel.com/media/14622/celsius-overview-brochure-all.pdf",
                     f"Brochure SHA256: {sha256(brochure_path)}",
                     f"Extracted: {datetime.date.today().isoformat()} by tools/section-catalogs/extract_tatasteel_celsius.py",
                     "Units: mm, mm2, mm3, mm4 (It), mm3 (Ct), kg/m (G)",
                     "D: outside diameter, t: wall thickness; I, Wel, Wpl about any axis; It torsion constant, Ct torsion modulus",
                     "InStandard: 1 for the sizes in the EN 10210-2 tables reproduced by Fondazione Promozione Acciaio (EN10210_CircularHollow), "
                     "0 for the other sizes of the producer range",
                     "Sizes of the brochure without properties in the workbook: " + ", ".join(f"{fmt(D)} x {fmt(t)}" for D, t in added) +
                     "; published mass, properties calculated from D and t with the formulas of EN 10210-2, 3 significant figures"]:
            stream.write("# " + line + "\n")
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(fields)
        for record in records:
            writer.writerow([fmt(record.get(f)) for f in fields])
    print(f"{os.path.basename(path)}: {len(records)} sections ({len(added)} from the brochure)")
    for line in rejected:
        print("REJECTED", line)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3])
