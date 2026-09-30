# Cataloghi delle sezioni commerciali e mappatura sulle sezioni di Model

`GPC.Model.Data.Sections` raccoglie i profili commerciali delle norme, ciascuno con la sua fonte, e li
collega alla sezione di Model che li rappresenta.

- `SectionCatalogs`: i cataloghi (letti alla prima richiesta dai CSV incorporati) e la ricerca per
  designazione (`Find`, `FindAll`, con le grafie comuni: `HEB 300` = `HE 300 B` = `HE300B`,
  `L 100 x 10` = `L 100 x 100 x 10`, virgola o punto decimale).
- `CatalogProfile`: designazione, serie, famiglia geometrica, appartenenza alla norma
  (`IsInStandard`) e i valori pubblicati dalla fonte in mm (mm², mm³, mm⁴, mm⁶), assi y-y forte e z-z debole.
- `SectionMappings`: per ogni famiglia la classe di Model, come i valori del catalogo la riempiono, la fedeltà
  (`Exact`, `Approximated`, `NotSupported`) e le note; `CreateSection` crea la sezione con le proprietà calcolate,
  `CreateSteelSection` la sezione in acciaio laminata.

I valori pubblicati restano disponibili in `CatalogProfile`: la sezione di Model li ricalcola dalla geometria.

## Fonti

Le fonti grezze non sono nel repository: gli script di `tools/section-catalogs` le trasformano nei CSV
di `Sections/Catalogs`, con in testa norma, fonte, versione e SHA256 del file.

| Catalogo | Norma (dichiarata dalla fonte) | Fonte | Profili |
| --- | --- | --- | --- |
| `EN10365_ParallelFlangeIH` | EN 10365:2017, tolleranze EN 10034 | ArcelorMittal, Sales Programme V2026-1 (Excel, 16/03/2026) | 624: IPE, IPE A/AA/O/V, HE AA/A/B/M, HL, HLZ, HD, HP, UBP, UB, UC |
| `EN10365_TaperFlangeI` | EN 10365:2017, tolleranze EN 10024 | idem | 31: IPN, J |
| `EN10365_Channels` | EN 10365:2017, tolleranze EN 10279 | idem | 48: UPE, PFC, UPN |
| `EN10056_Angles` | EN 10056-1:2017 | idem | 76: angolari a lati uguali e disuguali |
| `EN10210_CircularHollow` | EN 10210-2 | Fondazione Promozione Acciaio, tabelle dei profili cavi (2005) | 237 CHS a caldo |
| `EN10210_RectangularHollow` | EN 10210-2 | idem | 255 SHS e RHS a caldo |
| `EN10219_CircularHollow` | EN 10219-2 | idem | 221 CHS a freddo |

Il programma ArcelorMittal segna i profili fuori norma ("Additional section to the standard"): nel
catalogo hanno `IsInStandard = false`. Le serie ASTM del programma sono escluse: i profili americani
verranno dal database AISC.

## Mappatura

| Famiglia | Sezione di Model | Parametri | Fedeltà |
| --- | --- | --- | --- |
| I/H ad ali parallele | `SectionH` laminata | h, tw, b, tf, raccordi r | esatta |
| I ad ali rastremate | `SectionHTaperFlange` | IPN: pendenza 14%, tf a b/4 dalla punta (DIN 1025-1); J: 8°, tf a metà sporgenza | IPN esatta, J approssimata |
| Canali ad ali parallele | `SectionC` laminata | h, tw, b, tf, r1, r2 | esatta |
| Canali ad ali rastremate | `SectionCTaperFlange` | UPN h ≤ 300: 8%, tf a b/2; h > 300: 5%, tf a metà sporgenza (DIN 1026-1) | esatta |
| Angolari | `SectionL` laminata | b, h, t, r1, r2 (r2 = r1/2 di EN 10056-1 se la fonte non lo dà) | esatta |
| Tubi circolari | `SectionCHS` | D, t | esatta |
| Tubi rettangolari | `SectionRHSRoundedCorners` | h, b, t, raggi di calcolo EN 10210-2: esterno 1,5 t, interno t | esatta |
| T, doppi angolari | — | — | non supportata (cataloghi da aggiungere) |

"Esatta" indica che area, baricentro, momenti d'inerzia e moduli elastici e plastici sono quelli della geometria
della norma, raccordi e pendenze comprese. Le costanti di torsione e d'ingobbamento e il centro di taglio
di canali e sezioni rastremate vengono dalle formule a parete sottile di Model (vedi i limiti).

Le convenzioni delle ali rastremate (pendenza e punto di misura di tf) non sono nella fonte: sono state
scelte confrontando le aree pubblicate con le alternative (IPN: 0,05% contro 1,2%; UPN: 0,06% contro 1,3%).

## Fedeltà misurata

Il test `SectionCatalogsTest.PropertiesOfModelAgainstThePublishedOnes` crea la sezione di Model di ogni profilo
e la confronta con i valori pubblicati (arrotondati dalla fonte a 3-4 cifre significative). Scarti massimi
al 30/09/2026:

| Famiglia | A | I | Wel | Wpl | It | Iw / centro di taglio |
| --- | --- | --- | --- | --- | --- | --- |
| I/H ad ali parallele (497 coerenti) | 0,35% | 0,09% | 0,09% | 0,10% | 0,14% | Iw 1,6% |
| IPN | 0,36% | 0,50% | 0,49% | 0,92% | 11% | Iw 16% |
| J | 1,9% | 2,7% | 2,9% | 2,3% | 20% | Iw 18% |
| UPE, PFC | 0,31% | 0,34% | 0,35% | 2,4% (UPE 300, altrimenti 0,3%) | 16% | Iw 7,8%, ym 0,95% |
| UPN | 0,36% | 0,87% | 0,69% | 2,0% | 12% | Iw 17%, ym 9% |
| Angolari | 0,84% | 1,6% | 1,5% | — | — | baricentro 2% (pubblicato al mm) |
| CHS | 0,41% | 0,32% | 0,38% | 0,43% | 0,32% | — |
| SHS, RHS | 0,42% | 1,3% (pareti spesse, media 0,2%) | 1,2% | 0,56% | 0,33% (Ct 0,37%) | — |

## Incoerenze della fonte

- 127 profili I/H (UB, UC, UBP, HP, HD, HL, HLZ, alcuni IPE): l'area pubblicata non corrisponde alle dimensioni
  pubblicate (oltre 0,35%). Esempio UB 610 x 229 x 101: r = 20 mm pubblicato, area pubblicata di r = 12,7 mm
  (BS 4-1); HP 305 x 79: r = 20 mm, area di r ≈ 15,2 mm. La massa G segue l'area pubblicata. I dati non
  sono corretti: il test li elenca e ne controlla il numero.
- Angolari: r2 pubblicato solo per L 250 e L 300; altrove si usa r2 = r1/2 di EN 10056-1. Il raggio r3
  pubblicato per alcune misure non è rappresentato.
- Angolari a lati disuguali: la colonna Iyz dichiara "mm4" ma i valori sono in cm⁴ (verificato con
  Iu + Iv = Iy + Iz).
- HE 300 B: Iy = 25160 cm⁴ nel programma 2026 (25170 nelle edizioni precedenti).

## Limiti noti

- Costante di torsione, d'ingobbamento e centro di taglio di canali, IPN, J e UPN: formule a parete sottile
  (It fino al 20% in meno, Iw fino al 18% in più dei valori pubblicati, centro di taglio degli UPN al 9%).
  I valori pubblicati sono in `CatalogProfile`. Correzione prevista: calcolo numerico della torsione e
  dell'ingobbamento sulla sezione reale, utile anche alle sezioni generiche.
- EN 10219-2: mancano SHS e RHS a freddo (raggi esterni 2t, 2,5t, 3t); EN 10055 (T laminati) non è coperta.
- AISC: in attesa del database AISC v16.0.

## Rigenerare i cataloghi

```powershell
python tools/section-catalogs/extract_arcelormittal.py "<Sections and Merchant Bars-ArcelorMittal_V2026-1.xlsx>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_promozioneacciaio.py "<EN 10210 CHS.pdf>" ModelData/Sections/Catalogs
```

Servono `openpyxl` e `pdfplumber`. Lo SHA256 della fonte è nell'intestazione del CSV e nel test dei cataloghi.
