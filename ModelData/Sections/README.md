# Cataloghi delle sezioni commerciali e mappatura sulle sezioni di Model

`GPC.Model.Data.Sections` raccoglie i profili commerciali delle norme, ciascuno con la sua fonte, e li
collega alla sezione di Model che li rappresenta.

- `SectionCatalogs`: i cataloghi (letti alla prima richiesta dai CSV incorporati) e la ricerca per
  designazione (`Find`, `FindAll`, con le grafie comuni: `HEB 300` = `HE 300 B` = `HE300B`,
  `L 100 x 10` = `L 100 x 100 x 10`, virgola o punto decimale; per l'AISC anche la designazione metrica,
  `W1100X607` = `W44X408`).
- `CatalogProfile`: designazione, eventuale alias, serie, famiglia geometrica, appartenenza alla norma
  (`IsInStandard`) e i valori pubblicati dalla fonte in mm (mm², mm³, mm⁴, mm⁶), assi y-y forte e z-z debole
  per tutti i cataloghi (per l'AISC x-x diventa y-y, y-y diventa z-z).
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
| `AISC_ShapesV16` | AISC Steel Construction Manual 16th Ed.; ASTM A6, A500/A1085, A53 | AISC Shapes Database v16.0 (agosto 2023) | 2299: W, M, S, HP, C, MC, L, WT, MT, ST, 2L, HSS, PIPE |

Il programma ArcelorMittal segna i profili fuori norma ("Additional section to the standard"): nel
catalogo hanno `IsInStandard = false`. Le serie ASTM del programma sono escluse: i profili americani vengono
dal database AISC, convertito esattamente dalle colonne in pollici (quelle metriche sono arrotondate).

`Find` cerca nei cataloghi europei prima di quelli americani: le designazioni metriche di alcuni profili AISC
coincidono con profili europei (HP 360 x 174), `FindAll` li restituisce entrambi. Gli alias AISC uguali alla
designazione di un altro profilo (il metrico `Pipe20STD`, DN 20, è la designazione del tubo NPS 20) non sono
cercati: sono elencati in `SectionCatalog.AmbiguousAliases`.

## Mappatura

| Famiglia | Sezione di Model | Parametri | Fedeltà |
| --- | --- | --- | --- |
| I/H ad ali parallele | `SectionH` laminata | EN: raccordo r; AISC W: r = kdes − tf; AISC M, HP: r equivalente all'area pubblicata | esatta |
| I ad ali rastremate | `SectionHTaperFlange` | IPN: pendenza 14%, tf a b/4 dalla punta (DIN 1025-1); J: 8°, tf a metà sporgenza; AISC S: 1/6, tf medio, r1 equivalente all'area | IPN, S esatte; J approssimata |
| Canali ad ali parallele | `SectionC` laminata | h, tw, b, tf, r1, r2 | esatta |
| Canali ad ali rastremate | `SectionCTaperFlange` | UPN h ≤ 300: 8%, tf a b/2; h > 300: 5%, tf a metà sporgenza (DIN 1026-1); AISC C: 1/6; AISC MC: pendenza del baricentro pubblicato; r1 equivalente all'area | esatta |
| Angolari | `SectionL` | EN laminata r1, r2 (r2 = r1/2 di EN 10056-1 se manca); AISC a spigoli vivi, come le sue proprietà | esatta |
| T | `SectionT` laminata, `SectionTTaperFlange` | AISC WT: r = kdes − tf; MT: r equivalente all'area; ST: pendenza 1/6 | esatta |
| Doppi angolari | `SectionBuiltUp.DoubleAngle` | l'angolare singolo del catalogo, LLBB/SLBB, distanziatore s | esatta |
| Tubi circolari | `SectionCHS` | D, t (AISC t = tdes; D degli HSS tondi dalla designazione) | esatta |
| Tubi rettangolari | `SectionRHSRoundedCorners` | EN 10210-2: raggi esterno 1,5 t, interno t; AISC: t = tdes, esterno 2 t, interno t | esatta |

"Esatta" indica che area, baricentro, momenti d'inerzia e moduli elastici e plastici sono quelli della geometria
della norma, raccordi e pendenze comprese. Le costanti di torsione e d'ingobbamento e il centro di taglio
di canali, T e sezioni rastremate vengono dalle formule a parete sottile di Model (vedi i limiti).

Quando la fonte non pubblica un parametro geometrico, la mappatura lo ricava dai valori pubblicati e lo dichiara:
- convenzioni delle ali rastremate EN: scelte confrontando le aree (IPN: 0,05% contro 1,2%; UPN: 0,06% contro 1,3%);
- AISC: kdes è una grandezza di progetto delle verifiche locali, non il raggio delle proprietà. Per M, HP, MT, S,
  ST, C, MC il raccordo è quello che riproduce l'area pubblicata (con kdes gli M scartano fino al 7% in Iy e al 47%
  in It, col raggio equivalente 0,5% e 2,8%); la pendenza degli MC, che varia da profilo a profilo, è quella che
  riproduce il baricentro pubblicato (0,04 ÷ 0,19).

## Sezioni composte

`SectionBuiltUp` compone sezioni di Model spostate e specchiate (`Part`): doppi angolari (`DoubleAngle`), doppi
canali schiena contro schiena o punta contro punta (`DoubleChannel`), cruciformi. Area, baricentro e inerzie sono
quelli delle parti trasportate; i moduli elastici vengono dai vertici dei contorni, quelli plastici sono esatti
sull'unione dei contorni; la costante di torsione è la somma delle parti; la costante d'ingobbamento non è
disponibile (NaN) perché dipende dal collegamento delle parti. Una sezione composta non ha una forma unica né una
mesh (`GetMesh` segnala l'errore): si usano le parti. Verificata con due angolari accostati (= T) e due C punta
contro punta (= tubo rettangolare).

## Fedeltà misurata

Il test `SectionCatalogsTest.PropertiesOfModelAgainstThePublishedOnes` crea la sezione di Model di ogni profilo
e la confronta con i valori pubblicati (arrotondati dalla fonte a 3-4 cifre significative). Scarti massimi
al 01/10/2026:

| Gruppo | A | I | Wel | Wpl | It | Iw / posizioni |
| --- | --- | --- | --- | --- | --- | --- |
| EN I/H ad ali parallele (497 coerenti) | 0,35% | 0,09% | 0,09% | 0,10% | 0,14% | Iw 1,6% |
| EN IPN | 0,36% | 0,50% | 0,49% | 0,92% | 11% | Iw 16% |
| EN J | 1,9% | 2,7% | 2,9% | 2,3% | 20% | Iw 18% |
| EN UPE, PFC | 0,31% | 0,34% | 0,35% | 2,4% (UPE 300, altrimenti 0,3%) | 16% | Iw 7,8%, ym 0,95% |
| EN UPN | 0,36% | 0,87% | 0,69% | 2,0% | 12% | Iw 17%, ym 9% |
| EN angolari | 0,84% | 1,6% | 1,5% | — | — | baricentro 2% (pubblicato al mm) |
| EN CHS | 0,41% | 0,32% | 0,38% | 0,43% | 0,32% | — |
| EN SHS, RHS | 0,42% | 1,3% (pareti spesse, media 0,2%) | 1,2% | 0,56% | 0,33% (Ct 0,37%) | — |
| AISC W | 0,73% | 1,4% | 1,2% | 1,1% | 1,1% | Iw 3,4% |
| AISC M, HP | esatta | 1,3% | 1,0% | 0,8% | 2,8% | Iw 1,8% |
| AISC S, ST | esatta | 3,6% (Iz) | 3,9% | 1,6% | 21% | Iw 23% (S) |
| AISC C, MC | esatta | 4,2% (Iz) | 4,6% | 1,2% | 20% | Iw 2,8%, eo 0,64% |
| AISC L, 2L | 1,35% | 3,1% (Iv 6%) | 3,1% | 3,6% | — | baricentro 2,3%, ro 1,6% |
| AISC WT, MT | 0,73% | 1,7% | 1,2% | 1,3% | 3,7% | Iw 1,4% |
| AISC HSS rettangolari | 0,39% | 0,93% | 0,74% | 0,52% | 4,5% | — |
| AISC HSS tondi, PIPE (33 coerenti) | 0,52% | 2,1% (Pipe1/2XS) | 1,0% | 1,0% | 2,1% | — |

## Incoerenze della fonte

- ArcelorMittal, 127 profili I/H (UB, UC, UBP, HP, HD, HL, HLZ, alcuni IPE): l'area pubblicata non corrisponde alle
  dimensioni pubblicate (oltre 0,35%). Esempio UB 610 x 229 x 101: r = 20 mm pubblicato, area pubblicata di
  r = 12,7 mm (BS 4-1); HP 305 x 79: r = 20 mm, area di r ≈ 15,2 mm. La massa G segue l'area pubblicata.
- ArcelorMittal, angolari: r2 pubblicato solo per L 250 e L 300; altrove si usa r2 = r1/2 di EN 10056-1. Il
  raggio r3 pubblicato per alcune misure non è rappresentato. Angolari a lati disuguali: la colonna Iyz dichiara
  "mm4" ma i valori sono in cm⁴ (verificato con Iu + Iv = Iy + Iz). HE 300 B: Iy = 25160 cm⁴ (25170 nelle
  edizioni precedenti).
- AISC, 18 tubi PIPE: l'area pubblicata non corrisponde allo spessore di progetto pubblicato (tdes = 0,93 tnom):
  gli XS da 12 a 26 in sono il 3% più piccoli, Pipe10STD il 3% più grande. La colonna OD degli HSS tondi è
  arrotondata (HSS10.750: 10,8 in): il diametro esatto è nella designazione.
- AISC: il C degli HSS (costante di torsione per le tensioni) non è il modulo torsionale Ct di EN 10210-2 e non
  viene confrontato.

I dati non sono corretti: il test li elenca e ne controlla il numero.

## Limiti noti

- Costante di torsione, d'ingobbamento e centro di taglio di canali, T, IPN, J, S e UPN: formule a parete sottile
  (It fino al 21% in meno, Iw fino al 23% in più dei valori pubblicati, centro di taglio degli UPN al 9%).
  I valori pubblicati sono in `CatalogProfile`. Correzione prevista: calcolo numerico della torsione e
  dell'ingobbamento sulla sezione reale, utile anche alle sezioni generiche.
- Iz di S, ST, C, MC dell'AISC: circa 2% sistematico (geometria delle ali dell'ASTM A6 non pubblicata).
- EN 10219-2: mancano SHS e RHS a freddo (raggi esterni 2t, 2,5t, 3t); EN 10055 (T laminati) non è coperta.
- Sezioni composte: nessuna mesh unica e nessuna costante d'ingobbamento.

## Rigenerare i cataloghi

```powershell
python tools/section-catalogs/extract_arcelormittal.py "<Sections and Merchant Bars-ArcelorMittal_V2026-1.xlsx>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_promozioneacciaio.py "<EN 10210 CHS.pdf>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_aisc.py "<aisc-shapes-database-v160.xlsx>" ModelData/Sections/Catalogs
```

Servono `openpyxl` e `pdfplumber`. Lo SHA256 della fonte è nell'intestazione del CSV e nel test dei cataloghi.
