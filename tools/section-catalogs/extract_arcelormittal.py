"""Extracts the EN catalogs of rolled sections from the ArcelorMittal sales programme (Excel) into the CSV files of GPCModelData.

Source: ArcelorMittal Europe - Long Products, "Sections and Merchant Bars - Sales Programme", Excel version V2026-1
(https://sections.arcelormittal.com/products_and_solutions/Product_catalogues/EN). The dimensions are the ones of the standards
declared by the source for each sheet (EN 10365:2017, EN 10056-1:2017); the sections marked "Additional section to the standard"
are kept with InStandard = 0 (producer range). The properties are the published ones, converted to mm units.

Usage: python extract_arcelormittal.py <sales programme .xlsx> <output folder>
"""
import csv
import datetime
import hashlib
import os
import re
import sys

import openpyxl

SOURCE_URL = "https://sections.arcelormittal.com/repo/Sections/Sections%20and%20Merchant%20Bars-ArcelorMittal_V2026-1.xlsx"
SOURCE_TITLE = "ArcelorMittal Europe - Long Products, Sections and Merchant Bars, Sales Programme"

# conversion of the published units to the mm units of the catalogs
UNITS = {"mm": 1.0, "cm": 10.0, "cm2": 100.0, "cm3": 1e3, "cm4": 1e4, "cm6": 1e6, "kg/m": 1.0}


def number(value):
    """The numeric value of a cell (numbers stored as text included), None for '-', blanks and text"""
    if isinstance(value, (int, float)):
        return float(value)
    if isinstance(value, str):
        try:
            return float(value.replace(",", "."))
        except ValueError:
            return None
    return None


def write_catalog(path, metadata, fields, records):
    with open(path, "w", newline="", encoding="utf-8") as stream:
        for line in metadata:
            stream.write("# " + line + "\n")
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(fields)
        for record in records:
            writer.writerow([format_value(record.get(f)) for f in fields])
    print(f"{os.path.basename(path)}: {len(records)} sections")


def format_value(value):
    if value is None:
        return ""
    if isinstance(value, float):
        return repr(round(value, 9)).rstrip("0").rstrip(".") if value != int(value) else str(int(value))
    return str(value)


def series_of_designation(designation, heading):
    """The commercial series from the designation: IPE, IPE A, IPE O, HE A, HE B, HE M, HE AA, HL, UB, UC, UPE, UPN, PFC, IPN, J, ..."""
    d = designation.upper()
    m = re.match(r"^HE (\d+) (AA|A|B|M|C)$", d)
    if m:
        return "HE " + m.group(2)
    m = re.match(r"^HE \d+ X [\d.]+$", d)
    if m:
        return "HE"
    m = re.match(r"^IPE (A|AA|O|V) \d+$", d)
    if m:
        return "IPE " + m.group(1)
    m = re.match(r"^([A-Z]+)\b", d)
    return m.group(1) if m else heading


def metadata(sheet, standard, family, version, sha256, notes):
    return [f"Catalog: {family}",
            f"Standard (declared by the source): {standard}",
            f"Source: {SOURCE_TITLE}, version {version}, sheet \"{sheet}\"",
            f"Source file: {SOURCE_URL}",
            f"Source SHA256: {sha256}",
            f"Extracted: {datetime.date.today().isoformat()} by tools/section-catalogs/extract_arcelormittal.py",
            "Units: mm, mm2, mm3, mm4, mm6 (Iw), kg/m (G); properties as published by the source, axes y-y strong, z-z weak",
            "InStandard: 0 for the sections marked 'Additional section to the standard' by the source (producer range)",
            "Designations as published, with the decimal commas written as points (HD 320 x 97,6 -> HD 320 x 97.6)"] + notes


def main(workbook_path, output):
    sha256 = hashlib.sha256(open(workbook_path, "rb").read()).hexdigest().upper()
    wb = openpyxl.load_workbook(workbook_path, read_only=True, data_only=True)
    sheets = {ws.title: list(ws.iter_rows(values_only=True)) for ws in wb.worksheets}
    version = str(sheets["EN sections"][0][2])
    flags = {"Additional section to the standard", "New section 2025", "New section 2026"}

    # parallel flange I and H sections; the ASTM series at the end of the sheet are left to the AISC catalog
    rows = sheets["EN sections"]
    names = {"h": "h", "b": "b", "tw": "tw", "tf": "tf", "r": "r", "A": "A", "Iy": "Iy", "Wely": "Wel.y", "Wply": "Wpl.y",
             "Avz": "Avz", "Iz": "Iz", "Welz": "Wel.z", "Wplz": "Wpl.z", "It": "It", "Iw": "Iw"}
    stop = next(i for i, r in enumerate(rows) if isinstance(r[1], str) and "ASTM" in r[1])
    records = extract_with_mass(rows[:stop], 1, 2, 3, 4, 5, names, 4, flags)
    write_catalog(os.path.join(output, "EN10365_ParallelFlangeIH.csv"),
                  metadata("EN sections", "EN 10365:2017 (dimensions), EN 10034:1993 (tolerances)",
                           "Parallel flange I and H sections (IPE, HE, HL, HLZ, HD, HP, UBP, UB, UC)", version, sha256, []),
                  ["Designation", "Series", "InStandard", "G", "h", "b", "tw", "tf", "r", "A", "Iy", "Wely", "Wply", "Avz", "Iz", "Welz", "Wplz",
                   "It", "Iw"], records)

    rows = sheets["EN Taper flange beams"]
    names = {"h": "h", "b": "b", "tw": "tw", "tf": "tf", "r1": "r1", "r2": "r2", "A": "A", "Iy": "Iy", "Wely": "Wel.y", "Wply": "Wpl.y",
             "Avz": "Avz", "Iz": "Iz", "Welz": "Wel.z", "Wplz": "Wpl.z", "It": "It", "Iw": "Iw"}
    records = extract_with_mass(rows, 0, 1, 3, 4, 5, names, 3, flags)
    for record in records:
        # flange slope declared by the source for the IPN series; none for the J series
        record["Slope"] = 0.14 if record["Series"] == "IPN" else None
    write_catalog(os.path.join(output, "EN10365_TaperFlangeI.csv"),
                  metadata("EN Taper flange beams", "EN 10365:2017 (dimensions), EN 10024:1995 (tolerances)",
                           "Taper flange I sections (IPN, J)", version, sha256,
                           ["Slope: flange slope declared by the source (IPN 14%); empty when not declared (J)",
                            "tf: flange thickness as defined by the standard for taper flanges; r1 root radius, r2 toe radius"]),
                  ["Designation", "Series", "InStandard", "G", "h", "b", "tw", "tf", "r1", "r2", "Slope", "A", "Iy", "Wely", "Wply", "Avz", "Iz",
                   "Welz", "Wplz", "It", "Iw"], records)

    rows = sheets["EN Channels"]
    names = {"h": "h", "b": "b", "tw": "tw", "tf": "tf", "r1": "r", "r2": "r2", "A": "A", "Iy": "Iy", "Wely": "Wel,y", "Wply": "Wpl,y",
             "Avz": "Avz", "Iz": "Iz", "Welz": "Wel,z", "Wplz": "Wpl,z", "It": "It", "Iw": "Iw", "ys": "ys", "ym": "ym"}
    records = extract_with_mass(rows, 0, 1, 3, 4, 5, names, 3, flags)
    write_catalog(os.path.join(output, "EN10365_Channels.csv"),
                  metadata("EN Channels", "EN 10365:2017 (dimensions), EN 10279:2000 (tolerances)",
                           "Channels: parallel flanges (UPE, PFC) and taper flanges (UPN)", version, sha256,
                           ["r1: root radius, r2: toe radius (empty: sharp); ys: centroid from the back of the web, ym: shear centre from the "
                            "back of the web",
                            "UPN: taper flanges, slope not declared by the source; tf is the thickness defined by the standard"]),
                  ["Designation", "Series", "InStandard", "G", "h", "b", "tw", "tf", "r1", "r2", "A", "Iy", "Wely", "Wply", "Avz", "Iz", "Welz",
                   "Wplz", "It", "Iw", "ys", "ym"], records)

    rows = sheets["EN Angles"]
    split = next(i for i, r in enumerate(rows) if i > 10 and r[0] == "Series")
    names = {"h": "h = b", "t": "t", "r1": "r1", "r2": "r2", "r3": "r3", "A": "A", "zs": "zs=ys", "Iy": "Iy=Iz", "Wely": "Wel,y=Wel,z",
             "Iu": "Iu", "Iv": "Iv", "Iyz": "Iyz"}
    equal = extract_with_mass(rows[:split], 0, 1, 4, 5, None, names, 2, flags)
    for record in equal:
        record["b"], record["ys"], record["Iz"], record["Welz"] = record["h"], record["zs"], record["Iy"], record["Wely"]
    names = {"h": "h", "b": "b", "t": "t", "r1": "r1", "r2": "r2", "A": "A", "zs": "zs", "ys": "ys", "Iy": "Iy", "Wely": "Wel,y", "Iz": "Iz",
             "Welz": "Wel,z", "Iu": "Iu", "Iv": "Iv", "Iyz": "Iyz", "alpha": "a"}
    unequal = extract_with_mass(rows[split:], 0, 1, 2, 3, None, names, 2, flags, units_override={"Iyz": 1e4, "alpha": 1.0})
    for record in equal + unequal:
        record["Series"] = "L"
    write_catalog(os.path.join(output, "EN10056_Angles.csv"),
                  metadata("EN Angles", "EN 10056-1:2017 (dimensions), EN 10056-2:1994 (tolerances)",
                           "Equal and unequal leg angles (L)", version, sha256,
                           ["h: leg along z (vertical), b: leg along y; r1 root radius, r2 toe radius, r3 as published",
                            "zs, ys: centroid from the back of the horizontal and of the vertical leg; Iu, Iv principal moments; Iyz product",
                            "Iyz of the unequal angles is published with the unit 'mm4' but the values are cm4 (checked with Iu + Iv = Iy + Iz)",
                            "alpha: angle of the principal axis u from y (degrees), unequal angles only"]),
                  ["Designation", "Series", "InStandard", "G", "h", "b", "t", "r1", "r2", "r3", "A", "zs", "ys", "Iy", "Iz", "Wely", "Welz",
                   "Iu", "Iv", "Iyz", "alpha"], equal + unequal)


def extract_with_mass(rows, series_column, designation_column, header_row, units_row, multiplier_row, names, mass_column, flags,
                      units_override=None):
    """The sections of a sheet (see extract): the mass is read from its column (its unit is in the header rows)"""
    cols = {}
    for key, name in names.items():
        index = next((j for j, v in enumerate(rows[header_row]) if isinstance(v, str) and v.strip() == name), None)
        if index is None:
            raise ValueError(f"column {name} not found")
        if units_override and key in units_override:
            factor = units_override[key]
        else:
            unit = str(rows[units_row][index] or "").strip()
            factor = UNITS[unit]
            multiplier = rows[multiplier_row][index] if multiplier_row is not None else None
            if isinstance(multiplier, str) and multiplier.strip() == "x103":
                factor *= 1e3
        cols[key] = (index, factor)
    records, heading = [], None
    for row in rows[header_row + 1:]:
        label = row[series_column] if series_column is not None else None
        designation = row[designation_column]
        if isinstance(label, str) and label.strip() and label.strip() not in flags:
            heading = label.strip()
        if not isinstance(designation, str) or number(row[mass_column]) is None:
            continue
        # decimal commas of the source (e.g. "HD 320 x 97,6") written with the point: the CSV has no quoted values
        designation = re.sub(r"(\d),(\d)", r"\1.\2", re.sub(r"\s+", " ", designation.strip()))
        record = {"Designation": designation, "Series": series_of_designation(designation, heading),
                  "InStandard": 0 if isinstance(label, str) and label.strip() == "Additional section to the standard" else 1,
                  "G": number(row[mass_column])}
        for key, (index, factor) in cols.items():
            value = number(row[index])
            record[key] = None if value is None else value * factor
        records.append(record)
    return records


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
