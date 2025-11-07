/**
 * SKYWORKS SORA Field Explanations (Greek)
 * Official EASA/JARUS references for each field
 */

const SORA_FIELD_EXPLANATIONS_EL = {
  // === COMMON FIELDS ===
  "operation-type": {
    title: "Τύπος Λειτουργίας",
    explanation: `
      <strong>VLOS (Visual Line of Sight):</strong> Ο τηλεχειριστής διατηρεί άμεση οπτική επαφή με το drone καθ' όλη τη διάρκεια της πτήσης. Απόσταση έως ~500m.<br><br>
      
      <strong>EVLOS (Extended Visual Line of Sight):</strong> Χρήση παρατηρητών (airspace observers) για επέκταση του οπτικού πεδίου. Ο τηλεχειριστής δεν βλέπει απευθείας το drone.<br><br>
      
      <strong>BVLOS (Beyond Visual Line of Sight):</strong> Πτήση εκτός οπτικού πεδίου. Απαιτεί DAA (Detect and Avoid) ή segregated airspace (TSA).
    `,
    reference: "EASA Easy Access Rules UAS, Part-UAS, Article 2 Definitions"
  },
  
  "airspace-class": {
    title: "Κλάση Εναέριου Χώρου (ICAO)",
    explanation: `
      <strong>G (Uncontrolled):</strong> Ανεξέλεγκτος εναέριος χώρος. Χαμηλότερο ύψος, VFR πτήσεις. Το πιο κοινό για drones.<br><br>
      
      <strong>E, D, C, B, A:</strong> Ελεγχόμενος εναέριος χώρος. Απαιτείται άδεια από ATC (Air Traffic Control).<br><br>
      
      <strong>F:</strong> Ειδική κατηγορία (advisory services).<br><br>
      
      Για drone operations στην ΕΕ, συνήθως G ή E. Για ελεγχόμενο χώρο (C/D) απαιτείται ATZ clearance.
    `,
    reference: "ICAO Annex 11, EASA Standardised European Rules of the Air (SERA)"
  },
  
  "typicality": {
    title: "Τυπικότητα Λειτουργίας",
    explanation: `
      <strong>Typical:</strong> Η λειτουργία ακολουθεί τυπικά patterns (π.χ. καθημερινή επιθεώρηση, mapping). Χαμηλότερος κίνδυνος.<br><br>
      
      <strong>Atypical:</strong> Μη τυπική λειτουργία (π.χ. πρώτη φορά, πειραματικό, ειδικές συνθήκες). Υψηλότερος κίνδυνος → πιθανόν +1 στο initial ARC.
    `,
    reference: "JARUS SORA 2.5 Annex C, Table C-1 (Operational Volume)"
  },
  
  "uspace": {
    title: "U-Space",
    explanation: `
      <strong>U-Space:</strong> Ψηφιακό σύστημα διαχείρισης drone traffic (UTM). Παρέχει υπηρεσίες όπως:<br>
      - Flight planning & authorization<br>
      - Traffic information<br>
      - Geofencing<br>
      - Tracking<br><br>
      
      Στην ΕΕ, υποχρεωτικό σε ορισμένες ζώνες από 2023. Αν Yes → μείωση air risk.
    `,
    reference: "EU Regulation 2021/664 on U-Space"
  },
  
  "traffic-density": {
    title: "Πηγή Δεδομένων Πυκνότητας Κυκλοφορίας",
    explanation: `
      <strong>Empirical:</strong> Δεδομένα από πραγματικές παρατηρήσεις/μετρήσεις.<br><br>
      
      <strong>Modelled:</strong> Μοντελοποιημένα δεδομένα (simulations).<br><br>
      
      <strong>ANSP:</strong> Δεδομένα από Air Navigation Service Provider (επίσημα ATC data). Η πιο αξιόπιστη πηγή.
    `,
    reference: "JARUS SORA 2.5 Annex F, Air Risk Assessment"
  },
  
  "airspace-containment": {
    title: "Περιορισμός Εναέριου Χώρου",
    explanation: `
      <strong>None:</strong> Κανένας τεχνικός περιορισμός (μόνο procedural).<br><br>
      
      <strong>Horizontal:</strong> Geofence οριζόντια (π.χ. περίμετρος).<br><br>
      
      <strong>Vertical:</strong> Περιορισμός ύψους (altitude limiter).<br><br>
      
      <strong>Horizontal & Vertical:</strong> Και τα δύο (3D geofence). Μείωση air risk → -1 στο initial ARC.
    `,
    reference: "JARUS SORA 2.5 Annex C, M3 (Technical Issue with UAS)"
  },
  
  "aec": {
    title: "AEC (Adjacent Area Consideration)",
    explanation: `
      Αξιολόγηση του γύρω περιβάλλοντος (Annex C SORA 2.5):<br><br>
      
      <strong>AEC 1-4:</strong> Controlled ground area, low people density.<br>
      <strong>AEC 5-8:</strong> Sparsely/moderately populated.<br>
      <strong>AEC 9-12:</strong> Highly populated areas (urban).<br><br>
      
      Όσο υψηλότερο AEC → υψηλότερο initial GRC.
    `,
    reference: "JARUS SORA 2.5 Annex C, Tables C-2 to C-13 (AEC)"
  },
  
  // === SORA 2.5 FIELDS ===
  "m1a": {
    title: "M1(A) - Sheltering (Προστασία)",
    explanation: `
      <strong>Μείωση κινδύνου μέσω προστασίας ατόμων:</strong><br><br>
      
      <strong>None:</strong> Κανένα μέτρο προστασίας.<br><br>
      
      <strong>Low:</strong> Βασική προστασία (π.χ. κάτω από στέγαστρα, μέσα σε κτίρια). Μείωση -1 credit στο Final GRC.<br><br>
      
      ⚠️ <strong>Σημείωση SORA 2.5:</strong> ΔΕΝ υπάρχει επίπεδο "Medium" για M1(A) στο Annex B (Tables 2-3).
    `,
    reference: "JARUS SORA 2.5 Annex B, Tables 2-3 (M1A Sheltering)"
  },
  
  "m1b": {
    title: "M1(B) - Operational Restrictions (Λειτουργικοί Περιορισμοί)",
    explanation: `
      <strong>Μείωση κινδύνου μέσω επιχειρησιακών περιορισμών:</strong><br><br>
      
      <strong>None:</strong> Κανένας περιορισμός.<br><br>
      
      <strong>Medium:</strong> Βασικοί περιορισμοί (π.χ. πτήση εκτός ωρών αιχμής, αποφυγή συγκεντρώσεων). Μείωση -1 credit.<br><br>
      
      <strong>High:</strong> Αυστηροί περιορισμοί (π.χ. πτήση μόνο νύχτα, CGA με buffer zone). Μείωση -2 credits.<br><br>
      
      Παραδείγματα: Χρόνος ημέρας, καιρικές συνθήκες, buffer zones.
    `,
    reference: "JARUS SORA 2.5 Annex B, Tables 4-5 (M1B Operational Restrictions)"
  },
  
  "m1c": {
    title: "M1(C) - Ground Observation (Επίγεια Παρατήρηση)",
    explanation: `
      <strong>Μείωση κινδύνου μέσω ground observers:</strong><br><br>
      
      <strong>None:</strong> Κανένας παρατηρητής.<br><br>
      
      <strong>Low:</strong> Εκπαιδευμένοι ground observers που μπορούν να προειδοποιήσουν/εκκενώσουν την περιοχή. Μείωση -1 credit.<br><br>
      
      ⚠️ <strong>Σημείωση SORA 2.5:</strong> ΔΕΝ υπάρχει επίπεδο "Medium" για M1(C) στο Annex B (Tables 6-7).
    `,
    reference: "JARUS SORA 2.5 Annex B, Tables 6-7 (M1C Ground Observation)"
  },
  
  "m2-25": {
    title: "M2 - Impact Dynamics Reduced (Μείωση Κινητικής Ενέργειας)",
    explanation: `
      <strong>Μείωση βλάβης από πρόσκρουση:</strong><br><br>
      
      <strong>None:</strong> Κανένα μέτρο.<br><br>
      
      <strong>Low:</strong> Βασικά μέτρα (π.χ. ελαφρύτερο drone, χαμηλότερη ταχύτητα). Μείωση -1 credit.<br><br>
      
      <strong>Medium:</strong> Μέτριας έντασης μέτρα (π.χ. impact-absorbing materials, περιορισμός ταχύτητας <10 m/s). Μείωση -1 credit.<br><br>
      
      <strong>High:</strong> Προηγμένα μέτρα (π.χ. parachute, airbag, break-away parts). Μείωση -2 credits.<br><br>
      
      ✅ <strong>SORA 2.5 διαφορά:</strong> Υπάρχει επίπεδο "Low" (δεν υπήρχε στο SORA 2.0).
    `,
    reference: "JARUS SORA 2.5 Annex B, Table 8+ (M2 Impact Dynamics)"
  },
  
  "small-ua-rule": {
    title: "Small-UA Rule (Κανόνας Μικρού Drone)",
    explanation: `
      <strong>Ειδικός κανόνας για πολύ μικρά/αργά drones:</strong><br><br>
      
      Αν <strong>και τα δύο</strong> ισχύουν:<br>
      - MTOM (μάζα) ≤ 0.25 kg<br>
      - Max Speed ≤ 25 m/s (90 km/h)<br><br>
      
      Τότε: <strong>Initial GRC = 1</strong> (ελάχιστος κίνδυνος), ανεξάρτητα από AEC/population density.<br><br>
      
      Παραδείγματα: DJI Mini 2 (249g), Ryze Tello (80g).
    `,
    reference: "JARUS SORA 2.5 Annex B, Small-UA Rule"
  },
  
  // === SORA 2.0 FIELDS ===
  "m1-20": {
    title: "M1 - Strategic Mitigations (Στρατηγικές Μετριασμοί)",
    explanation: `
      <strong>SORA 2.0 (AMC1 Art.11):</strong><br><br>
      
      <strong>None:</strong> Κανένα μέτρο.<br><br>
      
      <strong>Low:</strong> Βασικά μέτρα (π.χ. operational procedures). Μείωση -1 credit.<br><br>
      
      <strong>Medium:</strong> Μέτριας έντασης μέτρα (π.χ. buffer zones, time restrictions). Μείωση -2 credits.<br><br>
      
      <strong>High:</strong> Αυστηρά μέτρα (π.χ. CGA, night-only ops). Μείωση -3 credits.<br><br>
      
      ⚠️ <strong>SORA 2.0 διαφορά:</strong> Χρησιμοποιεί "column-min clamp" (όχι απλό άθροισμα).
    `,
    reference: "EASA AMC1 Article 11 UAS.SPEC.050, SORA 2.0 Tables"
  },
  
  "m2-20": {
    title: "M2 - Impact Reduction (Μείωση Πρόσκρουσης)",
    explanation: `
      <strong>SORA 2.0 (AMC1 Art.11):</strong><br><br>
      
      <strong>None:</strong> Κανένα μέτρο.<br><br>
      
      <strong>Low:</strong> Βασικά μέτρα (π.χ. propeller guards). Μείωση -1 credit.<br><br>
      
      <strong>High:</strong> Προηγμένα μέτρα (π.χ. parachute, airbag). Μείωση -2 credits.<br><br>
      
      ⚠️ <strong>SORA 2.0 διαφορά:</strong> ΔΕΝ υπάρχει επίπεδο "Medium" (μόνο None/Low/High).
    `,
    reference: "EASA AMC1 Article 11 UAS.SPEC.050, M2 Tables"
  },
  
  "m3-20": {
    title: "M3 - Emergency Response Plan (Σχέδιο Έκτακτης Ανάγκης)",
    explanation: `
      <strong>SORA 2.0 (AMC1 Art.11):</strong><br><br>
      
      <strong>None:</strong> Δεν υπάρχει ERP. <strong>Προσθήκη +1 credit</strong> (penalty).<br><br>
      
      <strong>Adequate:</strong> Βασικό ERP (γραπτό σχέδιο, contact numbers). <strong>0 credits</strong> (neutral).<br><br>
      
      <strong>Validated:</strong> Επικυρωμένο ERP (tested, approved by NAA). <strong>Μείωση -1 credit</strong>.<br><br>
      
      Το ERP περιλαμβάνει: Ενέργειες σε περίπτωση fly-away, technical failure, medical emergency.
    `,
    reference: "EASA AMC1 Article 11 UAS.SPEC.050, M3 ERP Requirements"
  }
};

// Export for use in mission.html
if (typeof module !== 'undefined' && module.exports) {
  module.exports = SORA_FIELD_EXPLANATIONS_EL;
}
