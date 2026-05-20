function apriOrderAcquisto(formContext) {

    var statusPD = formContext.getAttribute("sc_statodigitalproduct").getValue();
    if (statusPD !== 126400000) { Xrm.Navigation.openAlertDialog({ text: "Prodotto Digitale non acquistabile" }); return; }


    var typePiattaforma = formContext.getAttribute("sc_typeplatform").getValue();

    if (typePiattaforma === null) {
        Xrm.Navigation.openAlertDialog({
            text: "Scegli prima una piattaforma d'acquistare"
        });
        return;
    }

    if (formContext.data.entity.getEntityName() !== "sc_digitalproduct") {
        alert("Questo pulsante funziona solo su Prodottto Digitale.");
        return;
    }

    var prodottoDigitaleId = formContext.data.entity.getId();
    prodottoDigitaleId = prodottoDigitaleId.replace("{", "").replace("}", "");


    var prodottoDigitaleName = formContext.getAttribute("sc_key")?.getValue(); // Name di prodotto Digitale
    if (!prodottoDigitaleName) {
        alert("Il nome del prodotto Digitale non è disponibile.");
        return;
    }

    var account = formContext.getAttribute("sc_accountclientid")?.getValue();
    if (!account || account.length === 0) {
        alert("Account non selezionato sul Prodottto Digitale.");
        return;
    }
    var accountId = account[0].id.replace("{", "").replace("}", "");
    function generaNumeroCasuale() {
        return Math.floor(Math.random() * 1000000) + 100000;
    }

    var newOrder = {
        "sc_name": "Order-" + generaNumeroCasuale(),
        "sc_digitalproductid@odata.bind": "/sc_digitalproducts(" + prodottoDigitaleId + ")",
        "sc_accountclientid@odata.bind": "/accounts(" + accountId + ")"
    };

    var valueTypeExpOrLicSoft = RetriveValueTypeExpOrLicSoftCountry(prodottoDigitaleId);

    if (valueTypeExpOrLicSoft === null || typeof valueTypeExpOrLicSoft === "undefined") {
        return;
    }
    var tipoProductAttr = formContext.getAttribute("sc_typedigitalproduct");

    var num = CheckExistKeyProduct(prodottoDigitaleId, typePiattaforma);
    if (num === 2) { // Chiavi di prodotto digitale DISPONIBILI

        if (tipoProductAttr.getValue() == 126400000) // VideoGame
        {

            if (valueTypeExpOrLicSoft !== 126400003) { // diverso da Espansione

                if (valueTypeExpOrLicSoft === 126400000 || valueTypeExpOrLicSoft === 126400002) { // Base Game o Remastered

                    var isBasePriceValid = CheckBasePriceGreaterThanZero(formContext);
                    if (isBasePriceValid === false) { return; }

                    creaOrdineAcquisto(newOrder);
                    Xrm.Navigation.openAlertDialog({ text: "Base Game o Remastered" });
                } else
                    if (valueTypeExpOrLicSoft === 126400001) { // DLC
                        var parentprodottodigitaleid = formContext.getAttribute("sc_parentdigitalproductid") != null
                            ? formContext.getAttribute("sc_parentdigitalproductid").getValue()
                            : null;
                        if (parentprodottodigitaleid !== null) {
                            // DLC indipendente da Padre                         
                            Xrm.Navigation.openAlertDialog({ text: "Esiste il prodotto digitale Padre, non puoi creare l'ordine di acquisto " });
                            return;
                        } else {
                            Xrm.Navigation.openAlertDialog({ text: "Il valore non e' stato impostato di prodotto digitale di parent e di  DLC" });
                            // DLC dipendente da Padre
                            var isBasePriceValid = CheckBasePriceGreaterThanZero(formContext);
                            if (isBasePriceValid === false) { return; }

                            creaOrdineAcquisto(newOrder);
                            Xrm.Navigation.openAlertDialog({ text: "DLC" });
                        }
                    }
            } else {

                // Espansione
                var isBasePriceValid = CheckBasePriceGreaterThanZero(formContext);
                if (isBasePriceValid === false) { return; }

                var existDLCdigitProduct = CheckExpansionHasDlcChild(prodottoDigitaleId);

                if (existDLCdigitProduct === false) {
                    Xrm.Navigation.openAlertDialog({
                        text: "Questa Espansione non ha nessun DLC collegato. Creazione ordine interrotta."
                    });
                    return;
                }
                Xrm.Navigation.openAlertDialog({ text: "Procedi con l'espansione." }); 
                CheckExistParentChildProdottoDigitale(prodottoDigitaleId, typePiattaforma, statusPD, newOrder);
            }
        }
        if (tipoProductAttr.getValue() == 126400001) // Licenza Software
        {
            creaOrdineAcquisto(newOrder);
            Xrm.Navigation.openAlertDialog({ text: "Licenza Software" });
        }

    } else if (num === 1) {
        // Nessuna chiave per la piattaforma richiesta
        Xrm.Navigation.openAlertDialog({ text: "Non ci sono chiavi disponibili per questa piattaforma." });
    } else if (num === -1) {
        Xrm.Navigation.openAlertDialog({ text: "Chiavi disponibili con un prodotto digitale non ci sono." });
    } else if (num === -2) {
        // Errore nella chiamata HTTP
        Xrm.Navigation.openAlertDialog({ text: "Errore durante la verifica delle chiavi." });
    }
}



// Video Game Parent Child

function CheckExistParentChildProdottoDigitale(prodottoDigitaleId, typePiattaforma, statusPD, newOrder) {


    var fetchUrl = "<fetch mapping='logical' version='1.0' output-format='xml-platform' distinct='false' >" +
        "<entity name='sc_digitalproduct'>" +
        "<filter type='and'>" +
        "<condition attribute='sc_parentdigitalproductid' operator='eq' value='" + prodottoDigitaleId + "' />" +
        "</filter>" +
        "<attribute name='sc_digitalproductid' />" +
        "<attribute name='sc_typeplatform' />" +
        "<attribute name='sc_statodigitalproduct' />" +
        "</entity>" +
        "</fetch>";
    var path = "?fetchXml=" + encodeURIComponent(fetchUrl);


    Xrm.WebApi.retrieveMultipleRecords("sc_digitalproduct", path).then(
        function success(result) {
            if (result.entities.length > 0) {
                var estensionPDStatus = false;
                var estensionPDTypePiattaf = false;

                for (var i = 0; i < result.entities.length; i++) {
                    var prodotiDigitale = result.entities[i];

                    var statuscodeParentChild = prodotiDigitale.sc_statodigitalproduct;
                    var typepiattaformaParentChild = prodotiDigitale.sc_typeplatform;
                    var idV = prodotiDigitale.sc_digitalproductid;

                    if (statuscodeParentChild !== statusPD) {
                        estensionPDStatus = true;
                        break;
                    }
                    if (typepiattaformaParentChild !== typePiattaforma) {
                        estensionPDTypePiattaf = true;
                        break;
                    }
                }

                if (estensionPDStatus) {
                    Xrm.Navigation.openAlertDialog({
                        text: "I contenuti di epsansioni o uno solo non e' presente nella 'DISPONIBILITA'."
                    });
                    return;

                } else if (estensionPDTypePiattaf) {
                    Xrm.Navigation.openAlertDialog({
                        text: "Piattaforma non corrispondente o mancante per le espansioni di Parent Child."
                    });
                    return;

                } else {

                    var numV = CheckExistKeyProduct(idV, typepiattaformaParentChild);

                    if (numV === 2) {
                        creaOrdineAcquisto(newOrder);
                        Xrm.Navigation.openAlertDialog({ text: "creaOrdineAcquisto e' stato creato" });
                    } else if (numV === -1) {
                        Xrm.Navigation.openAlertDialog({ text: "CHILD: Chiavi disponibili con un video game non ci sono." });
                    } else if (numV === -2) {
                        // Errore nella chiamata HTTP
                        Xrm.Navigation.openAlertDialog({ text: "CHILD: Errore durante la verifica delle chiavi." });
                    }
                }

            } else {
                console.log("Nessuna espansione trovata.");
            }
        },
        function (error) {
            console.error("Errore fetch espansioni: " + error.message);
        }
    );
}

function CheckBasePriceGreaterThanZero(formContext) {

    var basePriceAttr = formContext.getAttribute("sc_baseprice");

    if (basePriceAttr === null)
    {
        Xrm.Navigation.openAlertDialog({ text: "Il campo Base Price non è presente sul form." });
        return false;
    }

    var basePrice = basePriceAttr.getValue();

    if (basePrice === null || basePrice <= 0)
    {
        Xrm.Navigation.openAlertDialog({text: "Base Price deve essere maggiore di 0. Non puoi procedere con l'acquisto."});
        return false;
    }

    return true;
}

function CheckExpansionHasDlcChild(prodottoDigitaleId) {

    var existDLCdigitProduct = false;

    var fetchUrl =
        "<fetch mapping='logical' version='1.0' output-format='xml-platform' distinct='false'>" +
        "  <entity name='sc_digitalproduct'>" +
        "    <attribute name='sc_digitalproductid' />" +
        "    <filter type='and'>" +
        "      <condition attribute='sc_parentdigitalproductid' operator='eq' value='" + prodottoDigitaleId + "' />" +
        "    </filter>" +
        "    <link-entity name='sc_productdetails' from='sc_productdetailsid' to='sc_productdetailsid' alias='pd'>" +
        "      <filter type='and'>" +
        "        <condition attribute='sc_typeexpansion' operator='eq' value='126400001' />" + // DLC
        "      </filter>" +
        "    </link-entity>" +
        "  </entity>" +
        "</fetch>";

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/sc_digitalproducts?fetchXml=" + encodeURIComponent(fetchUrl), false);   req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=*");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                console.log(results);
                if (results.value.length > 0) {
                    existDLCdigitProduct = true;
                } 

            } else {
                console.log(this.responseText);
            }
        }
    };
    req.send();
    return existDLCdigitProduct;
}

function RetriveValueTypeExpOrLicSoftCountry(prodottoDigitaleId) {

    var valueTypeExpOrLicSoft = null;
    var valueCountry = false;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/sc_digitalproducts(" + prodottoDigitaleId + ")?$select=sc_digitalproductid&$expand=sc_productdetailsid($select=sc_typeexpansion,sc_typelicense,_sc_country_value)", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=*");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var resultProdottoDigitale = JSON.parse(this.response);

                if (resultProdottoDigitale.sc_productdetailsid != null) {

                    var productDetails = resultProdottoDigitale.sc_productdetailsid;

                    var tipoPDParent = productDetails["sc_typeexpansion"];
                    var tipoLicenzaSoftParent = productDetails["sc_typelicense"];
                    var country = productDetails["_sc_country_value"];

                    if (tipoPDParent !== null && typeof tipoPDParent !== "undefined") {

                        Xrm.Navigation.openAlertDialog({ title: "Type Expansion", text: "Valore sc_typeexpansion: " + tipoPDParent });
                        valueTypeExpOrLicSoft = tipoPDParent;
                    }
                    else if (tipoLicenzaSoftParent !== null && typeof tipoLicenzaSoftParent !== "undefined") {
                        Xrm.Navigation.openAlertDialog({ title: "Tipo di Licenza Software", text: "Valore sc_typelicense: " + tipoLicenzaSoftParent });
                        valueTypeExpOrLicSoft = tipoLicenzaSoftParent;
                    }
                    else {
                        Xrm.Navigation.openAlertDialog({ text: "Il campo sc_typelicense è vuoto o sc_typeexpansion." });
                    }
                    if (country !== null && typeof country !== "undefined") {
                        Xrm.Navigation.openAlertDialog({ title: "country", text: "Country e' valorizzato: " + country });
                        valueCountry = true;
                    } else {
                        Xrm.Navigation.openAlertDialog({ text: "Il campo country è vuoto." });
                    }

                    if (valueCountry === false) { return null; }

                } else {
                    Xrm.Navigation.openAlertDialog({ text: "Product Details non è collegato al Prodotto Digitale." });
                }
            } else {
                console.log(this.responseText);
            }
        }
    };
    req.send();
    return valueTypeExpOrLicSoft;
}



function CheckExistKeyProduct(prodottoDigitaleId, typePiattaforma) {

    var num = 0;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/sc_keydigitalproducts?$select=_sc_digitalproductid_value,sc_typeplatform&$filter=_sc_digitalproductid_value eq " + prodottoDigitaleId + "  and sc_statuspresentkey eq 126400000", false); // Disponibile
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=*");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                console.log(results);
                if (results.value.length <= 0) {
                    num = -1;
                } else {
                    var keyTrovata = false;
                    for (var i = 0; i < results.value.length; i++) {
                        var keyGame = results.value[i];
                        if (keyGame["sc_typeplatform"] === typePiattaforma) {
                            keyTrovata = true;
                            break;
                        }
                    }
                    if (!keyTrovata) {
                        num = 1;
                    } else {
                        num = 2; // Chiave trovata
                    }
                }

            } else {
                console.log(this.responseText);
                num = -2;
            }
        }
    };
    req.send();
    return num;
}


function creaOrdineAcquisto(newOrder) {

    // Step 1: crea OrderAcquisto
    Xrm.WebApi.createRecord("sc_purchaseorderline", newOrder).then(
        function (result) {
            var orderId = result.id;
            // Step 2: recupera l’OrderAcquisto appena creato (con il campo acn_acquistoid popolato dal plugin)
            Xrm.WebApi.retrieveRecord("sc_purchaseorderline", orderId, "?$select=sc_purchaseorderlineid&$expand=sc_purchaseid($select=sc_purchaseid)").then(
                function (order) {

                    if (order.sc_purchaseid && order.sc_purchaseid.sc_purchaseid) {
                        var acquistoId = order.sc_purchaseid.sc_purchaseid;
                        // Step 3: naviga verso la pagina dell’Acquisto
                        Xrm.Navigation.openForm({
                            entityName: "sc_purchase",
                            entityId: acquistoId
                        });
                        Xrm.Navigation.openAlertDialog({ text: "Ordine creato" });

                    } else {
                        Xrm.Navigation.openAlertDialog({ text: "Ordine creato, ma non è stato possibile identificare l'Acquisto associato." });
                    }
                },
                function (error) {
                    console.error("Errore nel recupero dell’OrderAcquisto: " + error.message);
                }
            );
        },
        function (error) {
            console.error("Errore nella creazione dell'OrderAcquisto: " + error.message);
        }
    );
}