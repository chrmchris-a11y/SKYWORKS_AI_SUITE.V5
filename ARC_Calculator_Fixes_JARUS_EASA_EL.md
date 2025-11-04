
# SKYWORKS AI SUITE — ARC Calculator: Λίστα Διορθώσεων (χωρίς κώδικα)
**Στόχος:** Να ευθυγραμμιστεί πλήρως ο υπολογισμός του ARC (Air Risk Class) με JARUS SORA v2.0/2.5 και EASA AMC/GM. Κάθε fix συνοδεύεται από επίσημη αναφορά (JARUS/EASA/SERA).  
**Χρήση:** Δώσε το παρόν αρχείο στον AI agent σου για ανάγνωση/εφαρμογή.

---

## A. Βασικές Εννοιολογικές Διορθώσεις

1) **Αφαίρεση “Mode‑S veil” — Χρήση TMZ/RMZ (Ευρώπη).**  
   - **Τι να αλλάξεις:** Μην χρησιμοποιείς τον όρο *Mode‑S veil* (αμερικανικός όρος γύρω από Class B). Για την Ευρώπη χρησιμοποίησε **TMZ (Transponder Mandatory Zone)** και, όπου χρειάζεται, **RMZ (Radio Mandatory Zone)** ως χαρακτηριστικά αέρα που επηρεάζουν την αρχική ταξινόμηση κινδύνου.  
   - **Γιατί:** Το SORA και το ευρωπαϊκό δίκαιο (SERA) αναφέρονται σε TMZ/RMZ· “Mode‑S veil” δεν είναι ενωσιακός όρος.  
   - **Αναφορές:** SERA ορισμοί TMZ/RMZ (EU No 923/2012 & επ.)· EASA eRules U‑space/Part‑SERA.

2) **Μην “σκληροκωδικοποιείς” αποστάσεις τύπου “3 NM/5 km” για “κοντά σε αεροδρόμιο”.**  
   - **Τι να αλλάξεις:** Αντικατάστησε το ad‑hoc “near aerodrome” με την επίσημη διάσταση **“airport/heliport environment”** του SORA και χρησιμοποίησε το **decision tree** για ARC. Απόφυγε καθολικά thresholds απόστασης χωρίς κρατική/ANSP πηγή.  
   - **Αναφορές:** SORA Main Body — **Step #4** (ARC) decision tree, EASA AMC/GM to Article 11 (Figure για τις “13 aggregated collision‑risk categories”).

3) **Ενοποίηση περιβάλλοντος: “Urban” vs “Rural”.**  
   - **Τι να αλλάξεις:** Για SORA 2.5 χειρίσου το περιβάλλον ως **Urban** ή **Rural**. Το “Suburban” αντιμετωπίζεται ως **Urban** για τον αεροκίνδυνο (ίδια ποιοτική κατηγορία συνάντησης).  
   - **Αναφορές:** EASA SORA workshop (Step #4), SORA v2.5 Main Body.

4) **Όριο ύψους για ARC: 500 ft AGL (= ~150 m).**  
   - **Τι να αλλάξεις:** Χρησιμοποίησε **ένα** επιχειρησιακό όριο για τη γενίκευση του κινδύνου αεροσύγκρουσης: **500 ft AGL (~150 m)**. Απόφυγε υβριδικά ή εθνικά όρια (π.χ. 120 m) μέσα στη λογική ARC.  
   - **Αναφορές:** EASA SORA workshop (Slide για Step #4: 500 ft/150 m), SORA v2.5 Main Body §4.4 (Step #4).

5) **AEC (Airspace Encounter Category): συμπλήρωση πλήρους συνόλου και ορθή χαρτογράφηση.**  
   - **Τι να αλλάξεις:** Μην περιορίζεις σε “AEC 1–9”. Υλοποίησε το **πλήρες σύνολο** των AEC όπως παρουσιάζονται στο decision tree/aggregated categories (π.χ. airport vs non‑airport, controlled vs uncontrolled, urban vs rural, typical vs atypical, και ύψος). Η αντιστοίχιση **AEC → ARC (a–d)** πρέπει να πηγάζει από τον επίσημο πίνακα/διάγραμμα και όχι από custom heuristics.  
   - **Αναφορές:** EASA AMC/GM to Article 11 (κείμενο & Figure για τις **13 aggregated categories**), Eurocontrol/JARUS τεκμηρίωση AEC/ARC.

6) **Ατυπικός/διαχωρισμένος εναέριος χώρος (“Atypical/segregated”).**  
   - **Τι να αλλάξεις:** Μην τον χρησιμοποιείς ως “πάτωμα” ex post. Αξιολόγησέ τον **στην αρχική ταξινόμηση** (initial ARC) — σε SORA 2.5 ο “Atypical airspace” μπορεί να οδηγεί σε **ARC‑a** (αμελητέος ρυθμός συναντήσεων).  
   - **Αναφορές:** EASA SORA workshop (αναφορά “Atypical airspace → negligible encounter rate”), SORA v2.5 Main Body.

7) **Διάκριση στρατηγικών (Step #5) και τακτικών (Step #6) μετριασμών.**  
   - **Τι να αλλάξεις:**  
     - **Στρατηγικοί μετριασμοί (Annex C):** Δεν μειώνουν “κατά 1 επίπεδο ανά SM”. Η μείωση είναι **περιορισμένη και υπό όρους** (π.χ. συνήθως **μέχρι 1 επίπεδο**, κάτω από 500 ft/150 m, **με συμφωνία Αρχής**, και **μόνο για συγκεκριμένα AEC** σύμφωνα με τον πίνακα του Annex C).  
     - **Τακτικοί μετριασμοί (Annex D / TMPR):** **Δεν** αλλάζουν το ARC· είναι απαιτήσεις που πρέπει να πληρωθούν **βάσει του residual ARC**.  
   - **Αναφορές:** EASA AMC/GM to Article 11 — Annex C (Table C.2 & κείμενο για προϋποθέσεις/όρια μείωσης ARC), Annex D (TMPR).

8) **Urban σε χαμηλά ύψη: μην βάζεις επιπλέον όριο 120 m.**  
   - **Τι να αλλάξεις:** Σε **Urban** περιβάλλον, η συνάντηση είναι **medium ακόμη και σε χαμηλό ύψος** ⇒ οδηγεί τυπικά σε **ARC‑c** χωρίς τεχνητό κατώφλι 120 m.  
   - **Αναφορές:** EASA SORA workshop (Step #4 slide: “Urban area — medium encounter rate even at low level”).

9) **Airport/heliport environment σε controlled/uncontrolled.**  
   - **Τι να αλλάξεις:** Εφάρμοσε **ρητά** τις περιπτώσεις airport/heliport σε **controlled** (π.χ. AEC υψηλότερου κινδύνου) και **uncontrolled** (ξεχωριστή AEC). Απόφυγε γενίκευση “near aerodrome + controlled ⇒ ARC‑d πάντα”. Η AEC εξαρτάται από **συνδυασμό** περιβάλλοντος/ύψους/δομής εναέριου χώρου.  
   - **Αναφορές:** SORA Annex C (παράδειγμα: airport/heliport σε Class G → συγκεκριμένο AEC), EASA AMC/GM Figure.

10) **Κλάσεις εναέριου χώρου.**  
    - **Τι να αλλάξεις:** Θεώρησε **A–E ως controlled**. F/G συνήθως μη ελεγχόμενοι (με κράτος‑ειδικές εξαιρέσεις). Να υποστηρίζονται **TMZ/RMZ** ως πρόσθετα χαρακτηριστικά του περιβάλλοντος.  
    - **Αναφορές:** SERA (ορισμοί), EASA eRules.

---

## B. SORA 2.0 — Στοχευμένες Διορθώσεις

11) **Όριο ύψους 500 ft AGL σε Step #4.**  
    - **Τι να αλλάξεις:** Βεβαιώσου ότι όλη η λογική SORA 2.0 για ARC χρησιμοποιεί **500 ft AGL** ως βασικό split και **όχι** προσαρμοσμένους κανόνες (π.χ. 120 m).  
    - **Αναφορές:** SORA v2.0 (AMC/GM προσαρμογή), EASA workshop για Step #4.

12) **Αεροδρόμιο/ελικοδρόμιο — αντιστοιχίσεις AEC.**  
    - **Τι να αλλάξεις:** Επανέλεγξε τις αντιστοιχίσεις τύπου “AEC 5–7” που χρησιμοποιείς για κοντά σε αεροδρόμιο· επιβεβαίωσέ τες έναντι του decision tree/πίνακα SORA 2.0 (και των aggregated categories).  
    - **Αναφορές:** SORA v2.0 Main Body §2.4.2, EASA AMC/GM Figure.

13) **TMZ/RMZ σε SORA 2.0.**  
    - **Τι να αλλάξεις:** Αντί για “AEC 2: Mode‑S veil/TMZ”, τεκμηρίωσε σωστά **πότε** η ύπαρξη TMZ/RMZ ανεβάζει AEC/ARC, σύμφωνα με το decision tree/τοπικούς κανόνες/ANSP.  
    - **Αναφορές:** SERA (TMZ/RMZ), EASA AMC/GM Step #4.

---

## C. SORA 2.5 — Στοχευμένες Διορθώσεις

14) **Μετρικό ύψος & split 150 m.**  
    - **Τι να αλλάξεις:** Χρησιμοποίησε **μέτρα** στη 2.5 και το split **150 m AGL**. Ενοποίησε τη λογική με το Step #4 της 2.5.  
    - **Αναφορές:** SORA v2.5 Main Body §4.4 (Step #4).

15) **Κατάργηση “<120 m AGL” στο uncontrolled/urban.**  
    - **Τι να αλλάξεις:** Μην βάζεις ειδικό όριο 120 m για urban/uncontrolled. Ακολούθησε το decision tree: urban ⇒ συνήθως **ARC‑c** ακόμη και χαμηλά· rural ⇒ **ARC‑b** κάτω από 150 m και **ARC‑c** πάνω από 150 m (γενικευμένη λογική).  
    - **Αναφορές:** EASA SORA workshop (Step #4 slide).

16) **Airport/heliport σε 2.5.**  
    - **Τι να αλλάξεις:** Εφάρμοσε ρητά τις περιπτώσεις airport/heliport (controlled/uncontrolled) σύμφωνα με το decision tree της 2.5· απόφυγε μοτίβο “near aerodrome ⇒ πάντα ARC‑d”.  
    - **Αναφορές:** SORA v2.5 Main Body §4.4 και Annex C παραδείγματα.

17) **Στρατηγικοί μετριασμοί — περιορισμοί μείωσης ARC.**  
    - **Τι να αλλάξεις:** Στη 2.5, η **μείωση του ARC** μέσω Annex C είναι **υπό όρους** (συνήθως **έως 1 επίπεδο** και **με έγκριση Αρχής**) και όχι “1 επίπεδο ανά SM”. Εφάρμοσε ρητά τις προϋποθέσεις (ύψος, AEC range, στοιχεία τοπικού όγκου, κ.λπ.).  
    - **Αναφορές:** EASA AMC/GM Annex C (πίνακας/κείμενο για residual ARC), SORA v2.5 Main Body.

18) **TMPR (Annex D) στη 2.5.**  
    - **Τι να αλλάξεις:** Ορίσε τις **τακτικές** απαιτήσεις (TMPR) **μετά** τον καθορισμό του residual ARC. Μην τις χρησιμοποιείς για να αλλάξεις τον αριθμητικό δείκτη του ARC.  
    - **Αναφορές:** SORA Annex D (TMPR), EASA eRules — Annex D.

---

## D. Γενικά για Υλοποίηση & Δοκιμές

19) **Aντιμετώπιση “Suburban”.**  
    - **Τι να αλλάξεις:** Map‑άρισέ το ως **Urban** για SORA. Μην φτιάχνεις ξεχωριστή κατηγορία κινδύνου.

20) **Unit tests: αντικατάσταση προσδοκιών με επίσημα σενάρια.**  
    - **Τι να αλλάξεις:** Δομήσε test cases που προκύπτουν **από το decision tree και τα παραδείγματα** (π.χ. airport/heliport σε Class G, Urban low‑level, Rural >150 m, Atypical airspace). Απόφυγε invented labels τύπου “AEC 8 = controlled <500 ft urban/suburban” αν δεν επιβεβαιώνονται ρητά.  
    - **Αναφορές:** SORA v2.5 Main Body §4.4, Annex C παραδείγματα, EASA AMC/GM Figure (aggregated categories).

21) **Τεκμηρίωση πηγών στο UI/Log.**  
    - **Τι να αλλάξεις:** Πρόσθεσε στο “notes/source” ακριβή παραπομπή (π.χ. “JARUS SORA v2.5 Main Body §4.4; EASA AMC/GM to Art.11 — Annex C/D; SERA TMZ/RMZ”).

---

## Ενδεικτική Χαρτογράφηση (για καθοδήγηση του agent, όχι ως “πίνακας αλήθειας”)

- **Urban (οπουδήποτε, <150 m):** τείνει σε **ARC‑c**.  
- **Rural <150 m (εκτός airport/heliport, typical):** τείνει σε **ARC‑b**.  
- **Rural ≥150 m:** τείνει σε **ARC‑c**.  
- **Airport/heliport environment:** οδηγεί σε **υψηλότερα AEC/ARC** (ιδίως σε controlled).  
- **Atypical/segregated:** μπορεί να καταλήγει σε **ARC‑a** ήδη από την αρχική ταξινόμηση.  
*(Πάντοτε με βάση το επίσημο decision tree και τοπικούς κανόνες ANSP/Αρχής.)*

---

## Επίσημες Αναφορές (για τεκμηρίωση)

- **JARUS SORA v2.5 — Main Body (JAR_doc_25), §4.4 Step #4.**  
  https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-v2.5-Main-Body-Release-JAR_doc_25.pdf

- **JARUS Annex C — Strategic Mitigation Collision Risk Assessment (v1.0).**  
  https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-Annex-C-v1.0.pdf

- **JARUS Annex D — Tactical Mitigation Performance Requirements (TMPR).**  
  (π.χ. εθνικές φιλοξενίες/αναφορές) https://www.transpordiamet.ee/sites/default/files/documents/2024-08/SORA.pdf

- **EASA AMC/GM to Article 11 — Easy Access Rules for UAS (Ιούλιος 2024).**  
  (περιλαμβάνει Figure με **13 aggregated collision‑risk categories** & guidance για Steps #4–#6)  
  https://www.easa.europa.eu/en/document-library/easy-access-rules/online-publications/easy-access-rules-unmanned-aircraft-systems

- **SERA — Ορισμοί TMZ/RMZ (EU No 923/2012 & τροπ.).**  
  Π.χ. EASA/Eurocontrol eRules & Skybrary σύνοψη: https://skybrary.aero/articles/transponder-mandatory-zone-tmz

- **EASA SORA Workshop — Step #4 slides (500 ft / 150 m, Urban vs Rural).**  
  https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

### Σημείωση
- Οι παραπάνω διορθώσεις αφορούν **λογική/κανόνες**. Η υλοποίηση (κώδικας) πρέπει να αντλεί αποφάσεις από **επίσημο πίνακα/διάγραμμα** και, όπου απαιτείται, από **κρατικές/ANSP** πηγές για ζώνες (π.χ. TMZ/RMZ) και **airport/heliport environments**.

