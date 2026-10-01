# Sezioni di Model

Le sezioni trasversali di GPCModel: geometria, proprietà meccaniche, torsione e ingobbamento, sezioni composte e
variabili. I cataloghi commerciali (EN, AISC) e la loro mappatura su queste classi sono in
[ModelData/Sections/README.md](../../ModelData/Sections/README.md).

Convenzioni: unità coerenti (mm nei cataloghi), origine nello spigolo in basso a sinistra del rettangolo
d'ingombro (salvo dove indicato), X orizzontale e Y verticale; l'asse 1 è l'asse principale del momento d'inerzia
massimo. Le grandezze che dipendono dalla norma (classificazione, larghezze efficaci, effetti del taglio, perdite di
precompressione, confinamento) non sono in Model: sono dei verificatori.

## Tipologie

| Famiglia | Classe o metodo | Note |
| --- | --- | --- |
| Generiche | `Section(Shape2d)` | Contorno arbitrario con fori e isole (`Childs`) |
| Generiche a più regioni | `SectionBuiltUp.FromRegions` | Regioni separate, una fuori dall'altra |
| Rettangolare, circolare | `SectionRectangular`, `SectionCircular` | |
| Profili laminati e saldati | `SectionH`, `SectionHTaperFlange`, `SectionC`, `SectionCTaperFlange`, `SectionL`, `SectionT`, `SectionTTaperFlange` | Raccordi e cordoni di saldatura esatti |
| Tubi | `SectionCHS`, `SectionRHS`, `SectionRHSRoundedCorners`, `SectionEllipse` (EHS), `SectionStadium`, `SectionRegularPolygon` | Pieni o cavi |
| Travi da ponte in acciaio | `SectionHDoubleBottomFlange`, `SectionHInclinedWeb`, `SectionSteelBox` (cassoncino aperto) | |
| Cassoni chiusi | `SectionBoxGirder` | Mono e multicella, rettangolari o trapezoidali, sbalzi rastremati, raccordi nelle celle |
| Travi PSC e prefabbricate | `SectionHaunchedI` (I, T, bulbo), `SectionUBeam`, `SectionHollowCore`, `SectionMultiWebDeck` (T, TT, nervate, impalcati a più anime) | |
| Forme parametriche | `SectionEllipse`, `SectionStadium`, `SectionRegularPolygon`, `SectionTrapezoid` (e `Triangle`) | |
| Sagomati a freddo | `SectionColdFormed`: `Channel`, `LippedChannel`, `Zed`, `LippedZed`, `Hat`, `Sigma` o linea media qualsiasi | Pieghe con raggio interno r |
| Metalliche composte (collegate per punti) | `SectionBuiltUp`: `DoubleAngle`, `DoubleChannel`, `Cruciform` | Torsione = somma, Iw non disponibile |
| Metalliche saldate | `SectionWelded`: `CoverPlated`, `ChannelBox`, `CruciformPlates` o parti qualsiasi | Torsione dell'insieme (celle chiuse comprese) |
| Acciaio–calcestruzzo | `ReinforcedConcreteSection.CreateFilledTube`, `CreateDoubleSkinTube`, `CreateEncased`, `CreateEncasedCircular`, `CreatePartiallyEncased`, `CreateSlabOnGirders`, `CreateSlabOnSteelBox` | Vedi sotto |
| Variabili | `SectionVariation` | Interpolazione dei contorni lungo l'elemento |

`SectionCHS` e `SectionCircular` hanno le proprietà dalle formule chiuse del cerchio; la forma (32 lati) serve alla mesh, il contorno
esatto (256 lati) alla torsione numerica, alle sovrapposizioni con il calcestruzzo e al nucleo dei tubi riempiti (prima era la forma:
torsione numerica -1,4%, nucleo -0,6%).

Le forme parametriche derivano da `SectionParametric`: il contorno è costruito dai parametri e area, baricentro,
momenti d'inerzia, moduli elastici e plastici sono esatti sul contorno (archi discretizzati con 64 lati ogni 90°,
errore relativo circa 1e-4).

## Disponibilità esplicita delle proprietà

`Section.GetAvailability(SectionProperty)` dichiara come è ottenuta ogni proprietà (area, baricentro, momenti
d'inerzia, moduli elastici, moduli plastici, costante di torsione, costante d'ingobbamento, centro di taglio):

| `PropertyAvailability` | Significato |
| --- | --- |
| `Exact` | Formula chiusa o integrazione sul contorno della geometria della sezione |
| `Numerical` | Elementi finiti sulla sezione reale (errore relativo circa 1e-4, vedi sotto) |
| `Approximate` | Formula approssimata (parete sottile, formule dei produttori): vedi la classe |
| `Given` | Assegnata con la sezione (costruttore per valori) |
| `NotAvailable` | Non definita (NaN) o segnaposto da non usare (per esempio Iw = 0 del cassoncino aperto) |

Una proprietà con valore NaN è sempre `NotAvailable`. Per `SteelSection` e le sezioni in calcestruzzo c'è il
metodo di estensione `ISectionShape.GetAvailability` (per il calcestruzzo: le proprietà del solo calcestruzzo).

| Classe | It | Iw | Centro di taglio |
| --- | --- | --- | --- |
| Generica, parametriche, sagomati a freddo, saldate | numerica | numerica | numerico (sugli assi di simmetria) |
| `SectionEllipse` piena, `SectionCircular`, `SectionCHS` | esatta | esatta | esatto |
| `SectionRectangular` | approssimata (Roark, < 0,5%) | approssimata (0) | esatto |
| `SectionRHS`, `SectionRHSRoundedCorners` | approssimata (Bredt, EN 10210-2) | approssimata (0) | esatto con doppia simmetria, altrimenti non disponibile |
| `SectionH` e derivate | approssimata | approssimata | esatto con doppia simmetria, altrimenti approssimato |
| Altre a parete sottile (C, L, T, HDoubleBottomFlange, HInclinedWeb) | approssimata | approssimata | approssimato |
| `SectionSteelBox` | approssimata | non disponibile | non disponibile |
| `SectionBuiltUp` | approssimata (somma delle parti) | non disponibile | approssimato |
| Sezione per valori | assegnata | assegnata | assegnato (moduli non disponibili) |

## Torsione e ingobbamento numerici

`Section.CalculateTorsionProperties(meshSize)` risolve con gli elementi finiti la funzione d'ingobbamento di
Saint-Venant sul contorno esatto (raccordi e cordoni compresi): triangoli quadratici a 6 nodi, integrazione esatta,
gradiente coniugato. Restituisce costante di torsione, costante d'ingobbamento rispetto al centro di taglio e centro
di taglio (Trefftz). Le celle chiuse e i fori non richiedono trattamenti particolari; per parti separate It è la
somma e Iw e il centro di taglio non sono definiti. Mesh predefinita: metà dello spessore minimo, al più √A / 12, al
più 40000 elementi.

Per le sezioni generiche (e parametriche, sagomate, saldate) Jt, Jw e ShearCenter sono calcolati così al primo
accesso (prima valevano 0, 0 e il baricentro). Le classi con formule mantengono i loro valori: il metodo permette il
confronto.

Precisione (test `SectionTorsionTest`): triangolo equilatero It e Iw esatti entro 1e-5 (convergenza h⁴);
rettangoli entro 2e-4 dalla serie di Saint-Venant; ellisse e corona circolare entro 1e-3 (poligoni di 720 lati);
canale e I sottili entro 1-2% dalla teoria a parete sottile, che il calcolo numerico ritrova al 0,01% per t = b/100.

Confronto con i valori pubblicati: vedi [ModelData/Sections/README.md](../../ModelData/Sections/README.md#torsione-numerica).

## Sezioni composte acciaio–calcestruzzo

Il contenitore è `ReinforcedConcreteSection` (calcestruzzo, barre, profili `SteelSectionPosition`), usato dal
solutore a fibre del Checker, da CompositeBridge, da CheckerUI e da ANTHEA: le tipologie sono metodi statici che lo
costruiscono, non classi derivate.

- Proprietà omogeneizzate: ogni profilo conta n volte le sue proprietà esatte meno il calcestruzzo che sostituisce,
  calcolato con Clipper sul suo contorno esatto (`ConcreteOverlapArea`): tutto il profilo se è dentro il
  calcestruzzo, nulla se è fuori, la parte sovrapposta se è parzialmente rivestito. Prima un profilo era tutto dentro
  o tutto fuori, secondo la sua prima parete sottile.
- `GetHomogeneizedJ11`/`J22` comprendono i profili (prima no: davano un J11 diverso da
  `GetHomogeneizedMechanicalProperties`).
- Il baricentro omogeneizzato usa area per baricentro del calcestruzzo (prima i momenti statici erano integrati
  sulla mesh: per una `SectionCircular`, mesh di 32 lati, il baricentro si spostava dello 0,6%).
- `CalculateHomogenizedTorsionProperties(phi)`: torsione della sezione composta con gli elementi finiti, calcestruzzo
  senza l'acciaio e profili pesati con Gs/Gc (torsione) ed Es/Ec (centro di taglio, ingobbamento); le celle chiuse
  da acciaio e calcestruzzo (cassoncino chiuso dalla soletta) sono risolte come tali. Barre escluse.
- `ConcreteShape`: la forma del calcestruzzo (anche in `IConcreteSection`). `Shape` è ora solo l'implementazione esplicita di
  `ISectionShape`: chi la usa su una variabile `ReinforcedConcreteSection` va aggiornato (5 file di test di `GPCChecker.Test.Concrete`,
  209 righe; le librerie del Checker usano l'interfaccia e compilano).
- `GetHomogeneizedMechanicalProperties(phi)` senza barre né profili restituisce le proprietà del calcestruzzo, come la versione senza
  phi (prima tutti zeri: CompositeBridge e CheckerUI li aggiravano).
- `SteelSectionPosition.MirrorX`, `MirrorY`: il profilo specchiato attorno all'asse verticale e/o orizzontale per il punto
  d'inserimento, prima della rotazione (tutte le trasformazioni passano da `PositionToGlobal` e `PositionToLocal`; il prodotto
  d'inerzia cambia segno con uno specchio).

| Metodo | Tipologia |
| --- | --- |
| `CreateFilledTube` | Tubo riempito (CHS, RHS, RHS con raccordi, qualsiasi sezione cava con un foro) |
| `CreateDoubleSkinTube` | Doppia pelle: tubo esterno, tubo interno centrato, calcestruzzo tra i due |
| `CreateEncased`, `CreateEncasedCircular` | Profilo inglobato (SRC) in un rettangolo o in un cerchio, ruotabile, barre d'angolo o su circonferenza |
| `CreatePartiallyEncased` | H parzialmente rivestito (calcestruzzo tra le ali) |
| `CreateSlabOnGirders` | Soletta (larghezza efficace data) su una o più travi, raccordi sulle ali superiori |
| `CreateSlabOnSteelBox` | Cassoncino chiuso dalla soletta |
| `AddRebarRow` | Fila di barre tra due limiti a passo dato |

Non coperte: calcestruzzi diversi nella stessa sezione (trave prefabbricata più getto), guaine e cavi aderenti o
no, fasi costruttive. Richiedono una classe nuova (non derivata) e un'interfaccia da concordare con il Checker.

### Dati del taglio

`ReinforcedConcreteSection.ShearData` (`ConcreteShearData`, facoltativo) contiene le staffe (`ConcreteShearReinforcement`:
diametro, passo, inclinazione 45–90°, materiale) e, per ciascuna direzione del taglio (`Axis1` per V1, `Axis2` per V2),
i dati resistenti espliciti (`ConcreteShearDirection`):

- bw e d;
- Asl, con la conferma del suo ancoraggio;
- bracci efficaci e z/d;
- cv, per il limite DIN del braccio;
- Δe, per Model Code 2010.

Contiene anche il diametro massimo dell'aggregato e la provenienza.

Nessun valore è ricavato dal contorno. Una derivazione dalle forme tipiche, come fa ANTHEA, sarebbe un suggerimento da
confermare. I dati fanno parte della revisione di verifica, non di quella di analisi. Serializzazione versione 4: la voce
è scritta solo se presente, quindi le sezioni senza dati del taglio mantengono contenuto e revisione.

### Dati della torsione

`ReinforcedConcreteSection.TorsionData` (`ConcreteTorsionData`, facoltativo) descrive il profilo resistente a parete
sottile:

- Ak, area racchiusa dalla linea media delle pareti (fori compresi), e uk, il suo perimetro;
- tef, spessore delle pareti;
- ΣAsl, barre longitudinali disponibili per la torsione in aggiunta alla flessione;
- la conferma di staffe chiuse e ancorate, con le barre longitudinali nello spessore e una barra in ogni spigolo (una spirale
  non equivale a staffe chiuse);
- l'indicazione di sezione cava con armatura su entrambe le facce delle pareti;
- la provenienza.

Le staffe sono quelle di `ShearData`. Come per il taglio, i dati sono espliciti: `TorsionGeometry.Rectangle` e `Circle` di
GPCChecker.Concrete propongono Ak, uk e tef dal contorno, da confermare. La voce è scritta solo se presente.

## Sezioni variabili

`SectionVariation(start, end, law)` restituisce la sezione in un punto dell'elemento interpolando i vertici dei
contorni delle sezioni di estremità: con la legge lineare gli spigoli dell'elemento sono rettilinei (la geometria
reale delle lamiere tagliate dritte; per le sezioni parametriche con anime verticali coincide con la sezione delle
dimensioni interpolate), con le leggi paraboliche gli spigoli sono parabole con il vertice all'inizio o alla fine.
Le due sezioni devono avere contorni con la stessa struttura e lo stesso numero di vertici. L'integrazione lungo
l'elemento (rigidezza, verifiche) non è in Model.
