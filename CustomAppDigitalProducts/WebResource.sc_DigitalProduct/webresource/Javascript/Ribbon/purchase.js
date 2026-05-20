function checkAcquistoStatusVisibility(formContext) {
    var statuspurchase = formContext.getAttribute("sc_statuspurchase").getValue(); // Stato dell'acquisto

    // Verifica se lo stato è "In attesa" (126400001) e rende visibile il pulsante
    if (statuspurchase === 126400001) { // 126400001 è "In attesa"
        return true;  // Rende visibile il pulsante
    }
    return false;  // Rende invisibile il pulsante se lo stato è diverso
}

function completaAcquistoFattura(formContext) {
    var acquistoId = formContext.data.entity.getId();
    acquistoId = acquistoId.replace("{", "").replace("}", "");
    // Esempio: cambiamo lo statuscode a "Effettuato"

    var statusAcquisto = formContext.getAttribute("sc_statuspurchase").getValue();

    // 126400001 è "In attesa"
    if (statusAcquisto !== 126400001) { Xrm.Navigation.openAlertDialog({ text: "Acquisto Carrello non deve essere manuelmente cambiato per rispettare le regole" }); return; }

    var accountidLookup = formContext.getAttribute("sc_accountclientid");

    if (!accountidLookup || !accountidLookup.getValue() || accountidLookup.getValue().length === 0) { Xrm.Navigation.openAlertDialog({ text: "accountidLookup è nullo o vuoto" }); return; }

    var fetchUrl = "<fetch mapping='logical' version='1.0' output-format='xml-platform' distinct='false' >" +
        "<entity name='sc_purchaseorderline'>" +
        "<filter type='and'>" +
        "<condition attribute='sc_purchaseid' operator='eq' value='" + acquistoId + "' />" +
        "</filter>" +
        "<attribute name='sc_keydigitalproduct' />" +
        "</entity>" +
        "</fetch>";

    var path = "?fetchXml=" + fetchUrl;
    var keygamecode = null;
    // Esegui la chiamata asincrona per recuperare i record
    Xrm.WebApi.retrieveMultipleRecords("sc_purchaseorderline", path).then( // sc_ordineacquisto
        function success(result) {
            if (result.entities.length > 0) {
                // Cicla attraverso i record di ordineacquisto recuperati
                for (var i = 0; i < result.entities.length; i++) {
                    var ordineAcquisto = result.entities[i];

                    keygamecode = ordineAcquisto.sc_keydigitalproduct;

                    // Verifica se 'acn_keygamecode' è vuoto o nullo
                    if (!keygamecode || keygamecode.trim() === "") {
                        console.log("KeyGame Name non valorizzato.");
                        Xrm.Navigation.openAlertDialog({ text: "KeyGame Name non valorizzato." });
                        // return;
                    } else {

                        Xrm.Navigation.openAlertDialog({ text: "KeyGame Name:", keygamecode });

                        var updateData = {
                            "sc_statuspurchase": 126400000 // Metti il valore corretto per "Effettuato"
                        };

                        Xrm.WebApi.updateRecord("sc_purchase", acquistoId, updateData).then(
                            function success() {
                                console.log("Acquisto aggiornato, plugin dovrebbe partire.");

                                var fattura = formContext.getControl("sc_invoice");
                                if (fattura) {
                                    fattura.setDisabled(true);
                                }
                                formContext.getControl("sc_name").setDisabled(false);
                                formContext.getControl("sc_code").setDisabled(false);
                                formContext.getControl("sc_accountclientid").setDisabled(false);
                                formContext.getControl("sc_purchasedate").setDisabled(false);
                                formContext.getControl("sc_total").setDisabled(false);
                                Xrm.Navigation.openAlertDialog({ text: "Acquisto completato." });
                            },
                            function (error) {
                                console.error("Errore nell'aggiornamento dell'acquisto: ", error.message);
                                Xrm.Navigation.openAlertDialog({ text: "Errore nell'aggiornamento dell'acquisto:" });
                            }
                        );
                    }
                }
            } else {
                console.log("Nessun ordine di acquisto trovato per l'Acquisto ID: " + acquistoId);
            }
        },
        function (error) {
            console.error("Errore nel recupero degli ordini di acquisto: " + error.message);
        }
    );
}