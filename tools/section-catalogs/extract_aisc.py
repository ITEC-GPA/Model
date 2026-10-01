"""Extracts the catalog of the American shapes from the AISC Shapes Database v16.0 into the CSV file of GPCModelData.

Source: AISC Shapes Database v16.0 (August 2023), consistent with the AISC Steel Construction Manual, 16th Edition; dimensions of
ASTM A6 (rolled shapes), ASTM A500/A1085 (HSS), ASTM A53 (pipes). The US customary values of the database are converted exactly to mm
(the metric columns of the database are rounded conversions); the metric designation is kept as alias.

The names of the values follow the catalogs of GPCModelData (axis y-y strong, z-z weak): AISC Ix -> Iy, Iy -> Iz, Sx -> Wely,
Zx -> Wply, J -> It, Cw -> Iw; for the single angles AISC z (minor principal) -> Iv, w (major principal) -> Iu.

Usage: python extract_aisc.py <aisc-shapes-database-v160.xlsx> <output folder>
"""
import csv
import datetime
import hashlib
import os
import re
import sys

import openpyxl

IN = 25.4
UNITS = {"L": IN, "A": IN ** 2, "S": IN ** 3, "I": IN ** 4, "C6": IN ** 6, "W": 1.4881639435695537}


def num(v):
    if isinstance(v, (int, float)):
        return float(v)
    if isinstance(v, str):
        try:
            return float(v)
        except ValueError:
            return None
    return None


def fraction(text):
    """1-3/8 -> 1.375, 3/4 -> 0.75, 1.000 -> 1.0"""
    m = re.fullmatch(r"(\d+)-(\d+)/(\d+)", text)
    if m:
        return int(m.group(1)) + int(m.group(2)) / int(m.group(3))
    m = re.fullmatch(r"(\d+)/(\d+)", text)
    if m:
        return int(m.group(1)) / int(m.group(2))
    return float(text)


def main(path, output):
    sha256 = hashlib.sha256(open(path, "rb").read()).hexdigest().upper()
    rows = list(openpyxl.load_workbook(path, read_only=True, data_only=True)["Database v16.0"].iter_rows(values_only=True))
    header = rows[0]
    us = {name: i for i, name in enumerate(header[:84])}
    metric_label = [i for i, name in enumerate(header) if name == "AISC_Manual_Label"][1]

    def g(row, name, unit):
        v = num(row[us[name]])
        return None if v is None else v * UNITS[unit]

    records = []
    for row in rows[1:]:
        kind = row[0]
        if not kind:
            continue
        label, alias = row[us["AISC_Manual_Label"]], row[metric_label]
        r = {"Designation": label, "Series": kind, "InStandard": 1, "Alias": alias, "G": g(row, "W", "W"), "A": g(row, "A", "A")}
        common = {"Iy": ("Ix", "I"), "Iz": ("Iy", "I"), "Wely": ("Sx", "S"), "Welz": ("Sy", "S"), "Wply": ("Zx", "S"), "Wplz": ("Zy", "S"),
                  "It": ("J", "I"), "Iw": ("Cw", "C6")}
        if kind in ("W", "M", "S", "HP", "C", "MC", "WT", "MT", "ST"):
            r.update(h=g(row, "d", "L"), b=g(row, "bf", "L"), tw=g(row, "tw", "L"), tf=g(row, "tf", "L"), kdes=g(row, "kdes", "L"))
            if kind in ("C", "MC"):
                r.update(ys=g(row, "x", "L"), eo=g(row, "eo", "L"))
            if kind in ("WT", "MT", "ST"):
                r.update(zs=g(row, "y", "L"))
            r["SlopedFlanges"] = 1 if kind == "M" and row[us["T_F"]] == "T" else None
        elif kind == "L":
            # vertical leg h = the longer leg (AISC b), horizontal leg b = the shorter one (AISC d): x-x horizontal as in the Manual
            r.update(h=g(row, "b", "L"), b=g(row, "d", "L"), t=g(row, "t", "L"), kdes=g(row, "kdes", "L"), ys=g(row, "x", "L"),
                     zs=g(row, "y", "L"))
            common.update(Iu=("Iw", "I"), Iv=("Iz", "I"))
            r["tanAlpha"] = num(row[us["tan(α)"]])
        elif kind == "2L":
            m = re.fullmatch(r"2L([\d./-]+)X([\d./-]+)X([\d./-]+)(?:X([\d./-]+))?(LLBB|SLBB)?", label)
            if not m:
                raise ValueError(label)
            leg1, leg2, t = fraction(m.group(1)) * IN, fraction(m.group(2)) * IN, fraction(m.group(3)) * IN
            r.update(long=max(leg1, leg2), short=min(leg1, leg2), t=t, s=(fraction(m.group(4)) * IN if m.group(4) else 0.0),
                     config={None: 0, "LLBB": 1, "SLBB": 2}[m.group(5)], zs=g(row, "y", "L"), ro=g(row, "ro", "L"))
            del common["It"], common["Iw"]
        elif kind == "HSS" and num(row[us["OD"]]) is None:
            r.update(h=g(row, "Ht", "L"), b=g(row, "B", "L"), t=g(row, "tdes", "L"), tnom=g(row, "tnom", "L"), Ct=g(row, "C", "S"))
        elif kind in ("HSS", "PIPE"):
            r["Series"] = "HSS round" if kind == "HSS" else "PIPE"
            r.update(D=g(row, "OD", "L"), t=g(row, "tdes", "L"), tnom=g(row, "tnom", "L"))
            common = {"I": ("Ix", "I"), "Wel": ("Sx", "S"), "Wpl": ("Zx", "S"), "It": ("J", "I")}
        else:
            raise ValueError(kind)
        for key, (name, unit) in common.items():
            r[key] = g(row, name, unit)
        records.append(r)

    fields = ["Designation", "Series", "InStandard", "Alias", "G", "h", "b", "tw", "tf", "t", "kdes", "D", "tnom", "long", "short", "s",
              "config", "SlopedFlanges", "A", "Iy", "Iz", "Wely", "Welz", "Wply", "Wplz", "It", "Iw", "Ct", "I", "Wel", "Wpl", "Iu", "Iv",
              "tanAlpha", "ys", "zs", "eo", "ro"]
    metadata = ["Catalog: American shapes of the AISC Steel Construction Manual (W, M, S, HP, C, MC, L, WT, MT, ST, 2L, HSS, PIPE)",
                "Standard (declared by the source): AISC Steel Construction Manual, 16th Edition; ASTM A6/A6M (rolled shapes), "
                "ASTM A500/A1085 (HSS), ASTM A53 (pipes)",
                "Source: AISC Shapes Database v16.0, August 2023",
                "Source file: aisc-shapes-database-v160.xlsx, https://www.aisc.org (downloaded by the user)",
                f"Source SHA256: {sha256}",
                f"Extracted: {datetime.date.today().isoformat()} by tools/section-catalogs/extract_aisc.py",
                "Units: mm, mm2, mm3, mm4, mm6 (Iw), kg/m (G), converted exactly from the US customary values of the database",
                "Axes as the other catalogs: y-y strong (AISC x-x), z-z weak (AISC y-y); Designation: AISC Manual label, Alias: metric label",
                "W, M, S, HP, C, MC, WT, MT, ST: h = d, b = bf, kdes = distance from the outer face of the flange to the web toe of the "
                "fillet; tf of S, C, MC is the average flange thickness; SlopedFlanges = 1 for the M shape with sloped flanges",
                "C, MC: ys = x (centroid) and eo (shear centre) from the back of the web; WT, MT, ST: zs = y from the outer face of the flange",
                "L: h = longer leg (vertical), b = shorter leg (horizontal), ys = x, zs = y; Iu, Iv principal moments (AISC w, z), "
                "tanAlpha = tan(alpha) of the Manual",
                "2L: long, short, t legs; s spacing; config 0 equal legs, 1 long legs back to back (LLBB), 2 short legs back to back (SLBB); "
                "zs = y, ro polar radius of gyration about the shear centre",
                "HSS: t = tdes, the design thickness (0.93 tnom for ERW HSS); rectangular h = Ht, b = B, Ct = C; round and PIPE D = OD"]
    with open(os.path.join(output, "AISC_ShapesV16.csv"), "w", newline="", encoding="utf-8") as stream:
        for line in metadata:
            stream.write("# " + line + "\n")
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(fields)
        for r in records:
            writer.writerow([fmt(r.get(f)) for f in fields])
    print(f"AISC_ShapesV16.csv: {len(records)} sections")


def fmt(v):
    if v is None:
        return ""
    if isinstance(v, float):
        v = round(v, 9)
        return str(int(v)) if v == int(v) else repr(v)
    return str(v)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
