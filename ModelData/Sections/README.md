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
| `EN10210_CircularHollowCelsius` | EN 10210-2:2006 | Tata Steel, Celsius: foglio delle proprietà CHS (Eurocodice 3, 01/07/2020) e tabelle di disponibilità della brochure | 175 CHS a caldo della gamma Celsius |
| `EN10210_RectangularHollow` | EN 10210-2 | idem | 255 SHS e RHS a caldo |
| `EN10219_CircularHollow` | EN 10219-2 | idem | 221 CHS a freddo |
| `AISC_ShapesV16` | AISC Steel Construction Manual 16th Ed.; ASTM A6, A500/A1085, A53 | AISC Shapes Database v16.0 (agosto 2023) | 2299: W, M, S, HP, C, MC, L, WT, MT, ST, 2L, HSS, PIPE |

Il programma ArcelorMittal segna i profili fuori norma ("Additional section to the standard"): nel
catalogo hanno `IsInStandard = false`.

La gamma Celsius di Tata Steel completa i tubi a caldo con le misure del produttore che le tabelle della
norma riprodotte dalla Fondazione Promozione Acciaio non hanno (per esempio CHS 60.3 x 3.6, 88.9 x 10,
193.7 x 14.2: tutti i tubi del catalogo dei micropali di ANTHEA sono ora in ModelData). `IsInStandard` vale
`true` per le 89 misure presenti anche in `EN10210_CircularHollow`, `false` per le altre 86. Le 7 misure della
brochure senza proprietà nel foglio (42.4 x 5 e gli spessori 17,5 da 273 a 508) hanno la massa pubblicata e
le proprietà calcolate da D e t con le formule di EN 10210-2, a 3 cifre significative come il foglio. `Find`
restituisce prima il profilo delle tabelle della norma; `FindAll` restituisce anche quello Celsius. Le serie ASTM del programma sono escluse: i profili americani vengono
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
contro punta (= tubo rettangolare). Le parti saldate lungo i lati (H con piatti, due C saldati punta contro punta,
cruciformi di piatti) sono `SectionWelded`: torsione, ingobbamento e centro di taglio dell'insieme con gli
elementi finiti. Tutte le tipologie di Model sono descritte in [Model/Sections/README.md](../../Model/Sections/README.md).

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
| EN CHS Celsius (3 cifre significative) | 0,39% | 0,45% | 0,49% | 0,44% | 0,46% | — |
| EN SHS, RHS | 0,42% | 1,3% (pareti spesse, media 0,2%) | 1,2% | 0,56% | 0,33% (Ct 0,37%) | — |
| AISC W | 0,73% | 1,4% | 1,2% | 1,1% | 1,1% | Iw 3,4% |
| AISC M, HP | esatta | 1,3% | 1,0% | 0,8% | 2,8% | Iw 1,8% |
| AISC S, ST | esatta | 3,6% (Iz) | 3,9% | 1,6% | 21% | Iw 23% (S) |
| AISC C, MC | esatta | 4,2% (Iz) | 4,6% | 1,2% | 20% | Iw 2,8%, eo 0,64% |
| AISC L, 2L | 1,35% | 3,1% (Iv 6%) | 3,1% | 3,6% | — | baricentro 2,3%, ro 1,6% |
| AISC WT, MT | 0,73% | 1,7% | 1,2% | 1,3% | 3,7% | Iw 1,4% |
| AISC HSS rettangolari | 0,39% | 0,93% | 0,74% | 0,52% | 4,5% | — |
| AISC HSS tondi, PIPE (33 coerenti) | 0,52% | 2,1% (Pipe1/2XS) | 1,0% | 1,0% | 2,1% | — |

## Torsione numerica

`Section.CalculateTorsionProperties` risolve torsione e ingobbamento con gli elementi finiti sul contorno esatto
della sezione mappata (raccordi e ali rastremate comprese). Scarti medi / massimi dai valori pubblicati, per ogni
profilo dei cataloghi con It o Iw pubblicati (01/10/2026; tempo medio 3-74 ms per profilo):

| Gruppo | It formula della classe | It numerica | Iw formula della classe | Iw numerica |
| --- | --- | --- | --- | --- |
| EN I/H ad ali parallele | 0,0% / 0,3% | 0,1-1,8% / 9,1% (HE AA, HD) | 0,1-0,5% / 1,6% | 0,7-2,6% / 5,3% |
| EN IPN | 9,8% / 10,9% | 5,3% / 6,9% | 14,2% / 16,1% | 6,2% / 9,7% |
| EN J | 12,0% / 20,3% | 5,8% / 11,9% | 15,0% / 17,6% | 12,4% / 15,8% |
| EN UPE, PFC | 4,7-9,4% / 16,2% | 0,1-0,2% / 0,5% | 4,5-5,2% / 7,8% | 7,1-8,0% / 13,1% |
| EN UPN | 8,1% / 11,6% | 1,4% / 4,4% | 12,8% / 17,1% | 1,6% / 6,0% |
| EN CHS (EN 10210, EN 10219) | 0,0% / 0,3% | 1,4-1,5% / 1,8% | — | — |
| EN SHS, RHS | 0,0-0,1% / 0,3% | 2,0-2,1% / 4,6% | — | — |
| AISC W | 0,2% / 1,1% | 0,3% / 1,2% | 0,7% / 3,4% | 1,1% / 5,7% |
| AISC M, HP | 0,7-0,8% / 2,8% | 0,8-1,6% / 9,4% | 0,5-0,7% / 1,8% | 1,3-1,8% / 3,7% |
| AISC S | 14,6% / 21,0% | 2,5% / 6,6% | 18,2% / 22,8% | 3,6% / 10,1% |
| AISC C, MC | 9,9-13,9% / 19,5% | 1,0-1,2% / 2,8% | 0,8-1,2% / 2,8% | 11,6-20,6% / 23,7% |
| AISC WT, MT | 0,5-1,3% / 3,7% | 0,3-1,4% / 3,1% | 0,2-0,3% / 1,4% | 1,4-4,3% / 33% |
| AISC ST | 13,7% / 19,0% | 2,4% / 4,6% | 0,2% / 1,2% | 18,4% / 35,6% |
| AISC L | 4,1% / 13,9% | 6,3% / 14,9% | 0,2% / 1,4% | 2,8% / 9,9% |
| AISC HSS rettangolari | 0,5% / 4,5% | 0,7% / 4,3% | — | — |
| AISC HSS tondi, PIPE | 0,1-0,6% / 3,2% | 1,4-1,5% / 4,2% | — | — |

Lettura:
- It: il calcolo numerico ritrova i valori pubblicati dei canali (UPE, PFC entro 0,5%, UPN e AISC C, MC entro 3-4%)
  e riduce gli scarti di IPN, J, S, ST, che le formule della classe sottostimano fino al 21%. Dove la formula della
  classe è quella del produttore (I/H, tubi) coincide con il pubblicato e il calcolo numerico se ne discosta di
  poco: per i tubi rettangolari la formula di EN 10210-2 è approssimata per le pareti spesse; per i CHS lo scarto
  dell'1,4% viene dal poligono di 32 lati con cui `SectionCHS` disegna il tubo.
- Iw: i valori pubblicati sono della teoria a parete sottile con lo spessore medio delle ali e senza raccordi. Il
  calcolo numerico converge (canale a spigoli vivi: 0,01% dalla teoria per t = b/100, 2,6% per le pareti di un
  C15x50, invariato raffinando la mesh) e gli scarti vengono dalla geometria reale: le ali rastremate sono più sottili
  alle punte, dove la coordinata settoriale è massima (AISC C, MC, S, ST: Iw fino al 20% minore), i raccordi
  aggiungono materiale (UPE, PFC: 7-8% maggiore).
- Le formule delle classi non sono cambiate (i risultati del Checker restano quelli).

Traccia: i profili con It o Iw delle formule di Model oltre il 5% dal valore pubblicato (soglia `TorsionDeviations.Threshold`; i valori
pubblicati hanno 3-4 cifre significative) sono in [TorsionDeviations.csv](TorsionDeviations.csv), incorporato nell'assembly e letto da
`TorsionDeviations.All` e `TorsionDeviations.Find(profilo)`: valore pubblicato, della formula di Model e numerico, per It e Iw. Al
01/10/2026 sono 239: IPN (21), J (10), UPE (14), PFC (6), UPN (18), AISC S (28), C (32), MC (38), L (44), ST (28). Il test
`SectionCatalogsTest.TorsionDeviationsAreTraced` controlla che la traccia sia aggiornata; se formule o cataloghi cambiano, scrive la nuova
traccia in `%TEMP%\TorsionDeviations.csv` da copiare qui.

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

- Costante di torsione, d'ingobbamento e centro di taglio di canali, T, IPN, J, S e UPN: le proprietà della classe
  restano le formule a parete sottile (It fino al 21% in meno, Iw fino al 23% in più dei valori pubblicati, centro
  di taglio degli UPN al 9%); i valori numerici sulla sezione reale sono dati da `CalculateTorsionProperties` (vedi
  [Torsione numerica](#torsione-numerica)). I valori pubblicati sono in `CatalogProfile`.
- Iz di S, ST, C, MC dell'AISC: circa 2% sistematico (geometria delle ali dell'ASTM A6 non pubblicata).
- EN 10219-2: mancano SHS e RHS a freddo (raggi esterni 2t, 2,5t, 3t); EN 10055 (T laminati) non è coperta.
- Sezioni composte collegate per punti (`SectionBuiltUp`): nessuna mesh unica e nessuna costante d'ingobbamento.

## Rigenerare i cataloghi

```powershell
python tools/section-catalogs/extract_arcelormittal.py "<Sections and Merchant Bars-ArcelorMittal_V2026-1.xlsx>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_promozioneacciaio.py "<EN 10210 CHS.pdf>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_aisc.py "<aisc-shapes-database-v160.xlsx>" ModelData/Sections/Catalogs
python tools/section-catalogs/extract_tatasteel_celsius.py "<Celsius-CHS-sectionpropertiesdimensionsproperties-Eurocode3-1_7_2020.xlsx>" "<celsius-overview-brochure-all.pdf>" ModelData/Sections/Catalogs
```

Servono `openpyxl` e `pdfplumber`. Lo SHA256 della fonte è nell'intestazione del CSV e nel test dei cataloghi. Lo script
Celsius legge `EN10210_CircularHollow.csv` della cartella di uscita per `IsInStandard`: va eseguito dopo quello della
Fondazione Promozione Acciaio. La brochure è https://www.tatasteel.com/media/14622/celsius-overview-brochure-all.pdf.
