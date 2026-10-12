# Section inspection without duplicated mapping

13 October 2026. Converter 2.2.0.0 exposes ReadSectionCatalog (UNIT, SECT and optional SECTIONALL only) and SectionShapeInspection. Both call the existing private section parser and PropertyMapper. The structural importer keeps its existing section/material policy. Model 5.2.0.1, ModelData 0.0.2.11 and Persistence 3.0.0.4 are technical revisions for a coherent source-linked release; no formulas or ordinary section meshes are changed.

## Inventory and reuse

ISectionShape and Section.GetAvailability already expose geometric quantities and their availability. SectionCurveOutline snapshots retain holes, islands and curves; SectionBuiltUp exposes physical outlines. Section inspection uses these APIs. It does not recreate shape equations, section properties, catalog resolution or reference-point calculations in a UI.

The ANTHEA saved abutment sample has only two DBUSER/SB properties (1.2 m and 0.001 m squares). Composite inspection is therefore tested with an independent section having a 1960 mm web, 15 mm web thickness, 600/800 mm flanges of 20 mm and a 3600 x 280 mm slab. This is a synthetic geometry test, not a comparison of solver properties.

Supported physical preview of composite sections: symmetric steel-I type 1 with centred slab, Hh = 0, no stiffeners. Existing SectionH, SectionRectangular and SectionBuiltUp build the physical parts. Elastic, density and long-term ratios remain source metadata. The resulting physical union must not be used as a homogenized analysis section. The structural converter still leaves unsupported composite beam properties unassigned.

A top/bottom bounding reference can be resolved, and horizontal symmetry makes its centre unambiguous. A centroid-based composite vertical reference remains unresolved because the physical centroid is not the elastic centroid. Haunches, stiffeners, type-2 composite shapes, PSC and tapered section variation remain preserved-only. No equivalent rectangle, effective modulus or resistance is invented.

## Corrected reference handling

A preexisting discrepancy was found: CivilNxModelProfile.Reference warned that custom offsets were unresolved but returned null; PropertyMapper treated null as a default centroid reference. SectionReference.IsUnresolved now distinguishes the explicit unknown state. User-defined section offsets remain null on the imported beam assignments and in the inspection. Known references still use the same calculation. For assembled inspection geometry, the mapper reads SectionBuiltUp.GetOutlines because its inherited GetSectionPoints is unimplemented.

Baseline before changes: 158 existing converter, curve and composite tests passed in ANTHEA/supporto/artefatti/refactoring/sections/before. New and existing checks after changes: 167 passed, including independent area, top reference, units, archive/import behavior, absent outlines and unsupported offset handling. One initial new composite test exposed the inherited method exception, then passed after using GetOutlines. Expected values were not changed.

## Source protocol documentation (development only)

- https://support.midasuser.com/hc/en-us/articles/35808653964185-Section-Properties-Common
- https://support.midasuser.com/hc/en-us/articles/35939122737689-Section-Properties-Composite-Steel

These pages establish field meanings and dimension order only. No third-party text or figures belong in ANTHEA's user guides.

Full library verification: 12 core and 1222 Model tests passed; the two documented native-only tests were skipped. Public API compatibility, pinned dependencies and equality audit passed. Evidence: ANTHEA/supporto/artefatti/refactoring/sections/model-full.log. No expected values or API baselines were regenerated.
