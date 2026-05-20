'use strict';

if (typeof CustomApp === "undefined") {
    var CustomApp = { __namespace: true };
}

const typePiattaformaDigital =
{
    Steam: 126400000,
    EA: 126400001,
    Ubisoft: 126400002,
    Psn: 126400003,
    Microsoft_Xbox: 126400004,
    Epic_Games: 126400005,
    Nintendo_eShop: 126400006,
    Netflix: 126400007,
    Sportify: 126400008,
    Amazon: 126400009,
    Microsoft: 126400010,
    Adobe: 126400011,
    Autodesk: 126400012,
    Norton: 126400013,
    Telegram: 126400014,
    Reseller: 126400015
};

const typeProductDigital =
{
    VideoGame: 126400000,
    Licenza_Software: 126400001
};

CustomApp.mainDigitalProductEdit = new function () {

    var _self = this;
    var _originalPlatformOptions = null;

    _self.onload = function () {

        if (!isUpdateForm()) {
            return;
        }

        var productDetailsValue = getFieldValue("sc_productdetailsid");

        _self.HideTypePiattaforma();

        if (fieldExists("sc_productdetailsid")) {

            if (productDetailsValue !== null) {
                showField("sc_productdetailsid");
            } else {
                hideField("sc_productdetailsid");
            }
        }

        _self.SetFieldsDisabled();

        var prodottoDigitaleId = getCurrentRecordId();

        if (prodottoDigitaleId === null) {
            alert("Id Prodotto Digitale non trovato.");
            return;
        }

        _self.RetriveValueTypeProduct(prodottoDigitaleId, function (valueTypeProduct) {

            if (valueTypeProduct == 0) {
                alert("Il campo sc_typeexpansion non è entrato nel metodo");
                return;
            }

            if (valueTypeProduct == -1) {
                alert("Il campo type expansion o licenza è vuoto.");
            }

            if (valueTypeProduct == -3) {
                alert("Product Details non è collegato al Prodotto Digitale.");
            }

            var tipoProduct = getNumberValue("sc_typedigitalproduct");

            if (tipoProduct == typeProductDigital.VideoGame) {

                if (valueTypeProduct == 126400000 // BaseGame
                    || valueTypeProduct == 126400002 // Remastered
                    || valueTypeProduct == 126400003 // Espansione
                ) {

                    hideField("sc_parentdigitalproductid");

                } else {

                    _self.HideBasePriceIfDlcWithParent(valueTypeProduct);
                    showField("sc_parentdigitalproductid");
                }
            }
            else if (tipoProduct == typeProductDigital.Licenza_Software) {

                if (valueTypeProduct == 126400000 // Perpetua
                    || valueTypeProduct == 126400001 // Mensile
                    || valueTypeProduct == 126400002 // Annuale
                    || valueTypeProduct == 126400003 // Trial
                    || valueTypeProduct == 126400004 // Lifetime
                    || valueTypeProduct == 126400005 // Enterprise
                ) {

                    hideField("sc_parentdigitalproductid");

                } else {

                    showField("sc_parentdigitalproductid");
                }
            }
        });
    };

    _self.VisiblyMessage = function () {

        alert("Ciao, questo è un alert!");
    };

    _self.HideBasePriceIfDlcWithParent = function (valueTypeProduct) {

        if (!fieldExists("sc_baseprice") || !fieldExists("sc_parentdigitalproductid")) {
            return;
        }

        var parentDigitalProductValue = getFieldValue("sc_parentdigitalproductid");

        if (valueTypeProduct === 126400001) { // DLC

            var hasParentDigitalProduct = parentDigitalProductValue !== null;

            if (hasParentDigitalProduct) {
                hideField("sc_baseprice");
            } else {
                showField("sc_baseprice");
            }

        } else {

            showField("sc_baseprice");
        }
    };

    _self.HideTypePiattaforma = function () {

        if (!fieldExists("sc_typedigitalproduct")
            || !fieldExists("sc_typeplatform")) {

            alert("Due optionset non sono disponibili!");
            return;
        }

        var tipoProduct = getNumberValue("sc_typedigitalproduct");
        var attualeValuePiattaforma = getNumberValue("sc_typeplatform");

        var allowedOptions = [];

        switch (tipoProduct) {

            case typeProductDigital.VideoGame:

                allowedOptions = [
                    { text: "Steam", value: typePiattaformaDigital.Steam },
                    { text: "EA", value: typePiattaformaDigital.EA },
                    { text: "Ubisoft", value: typePiattaformaDigital.Ubisoft },
                    { text: "PSN", value: typePiattaformaDigital.Psn },
                    { text: "Microsoft Xbox", value: typePiattaformaDigital.Microsoft_Xbox },
                    { text: "Epic Games", value: typePiattaformaDigital.Epic_Games },
                    { text: "Nintendo eShop", value: typePiattaformaDigital.Nintendo_eShop }
                ];

                break;

            case typeProductDigital.Licenza_Software:

                allowedOptions = [
                    { text: "Microsoft", value: typePiattaformaDigital.Microsoft },
                    { text: "Adobe", value: typePiattaformaDigital.Adobe },
                    { text: "Autodesk", value: typePiattaformaDigital.Autodesk },
                    { text: "Norton", value: typePiattaformaDigital.Norton },
                    { text: "Amazon", value: typePiattaformaDigital.Amazon },
                    { text: "Reseller", value: typePiattaformaDigital.Reseller }
                ];

                break;

            default:
                break;
        }

        var $piattaforma = $("#sc_typeplatform");

        if (_originalPlatformOptions === null) {
            _originalPlatformOptions = $piattaforma.find("option").clone();
        }

        $piattaforma.empty();

        for (var i = 0; i < allowedOptions.length; i++) {

            var option = $("<option></option>");
            option.val(allowedOptions[i].value);
            option.text(allowedOptions[i].text);

            $piattaforma.append(option);
        }

        var isAttualeValueAllowed = false;

        for (var j = 0; j < allowedOptions.length; j++) {

            if (allowedOptions[j].value === attualeValuePiattaforma) {
                isAttualeValueAllowed = true;
                break;
            }
        }

        if (attualeValuePiattaforma !== null && isAttualeValueAllowed === false) {
            $piattaforma.val("");
            $piattaforma.trigger("change");
        }
    };

    _self.SetFieldsDisabled = function () {

        disableField("sc_typedigitalproduct");
        disableField("sc_name");
        disableField("sc_codice");
    };

    _self.RetriveValueTypeProduct = function (prodottoDigitaleId, callback) {

        var valueTypeProduct = 0;

        prodottoDigitaleId = normalizeGuid(prodottoDigitaleId);

        var url = "/_api/sc_digitalproducts(" + prodottoDigitaleId + ")?$select=sc_digitalproductid&$expand=sc_productdetailsid($select=sc_typeexpansion,sc_typelicense)";

        console.log("RetriveValueTypeProduct URL:", url);

        $.ajax({
            type: "GET",
            url: url,
            contentType: "application/json",
            success: function (resultProdottoDigitale) {

                console.log("Risultato DigitalProduct:", resultProdottoDigitale);

                if (resultProdottoDigitale.sc_productdetailsid != null) {

                    console.log("ProductDetails:", resultProdottoDigitale.sc_productdetailsid);

                    var tipoExpansion = resultProdottoDigitale.sc_productdetailsid.sc_typeexpansion;
                    var tipoLicenza = resultProdottoDigitale.sc_productdetailsid.sc_typelicense;

                    console.log("tipoExpansion:", tipoExpansion);
                    console.log("tipoLicenza:", tipoLicenza);

                    if (tipoExpansion !== null && typeof tipoExpansion !== "undefined") {
                        valueTypeProduct = tipoExpansion;
                    } else if (tipoLicenza !== null && typeof tipoLicenza !== "undefined") {
                        valueTypeProduct = tipoLicenza;
                    } else {
                        valueTypeProduct = -1;
                    }

                } else {
                    valueTypeProduct = -3;
                }

                callback(valueTypeProduct);
            },
            error: function (xhr) {

                console.log("ERRORE Web API RetriveValueTypeProduct");
                console.log("Status:", xhr.status);
                console.log("Response:", xhr.responseText);

                callback(0);
            }
        });
    };

    $(document).ready(function () {

        CustomApp.mainDigitalProductEdit.onload();

        $("#sc_typedigitalproduct").on("change", function () {
            CustomApp.mainDigitalProductEdit.HideTypePiattaforma();
        });

        $("#sc_parentdigitalproductid").on("change", function () {
            CustomApp.mainDigitalProductEdit.HideBasePriceIfDlcWithParent(126400001);
        });
    });

    function fieldExists(fieldName) {

        return $("#" + fieldName).length > 0;
    }

    function getFieldValue(fieldName) {

        var field = $("#" + fieldName);

        if (field.length === 0) {
            return null;
        }

        var value = field.val();

        if (value === null || value === undefined || String(value).trim() === "") {
            return null;
        }

        return value;
    }

    function getNumberValue(fieldName) {

        var value = getFieldValue(fieldName);

        if (value === null) {
            return null;
        }

        var numberValue = Number(value);

        if (isNaN(numberValue)) {
            return null;
        }

        return numberValue;
    }

    function hideField(fieldName) {

        var field = $("#" + fieldName);

        if (field.length === 0) {
            return;
        }

        $("#" + fieldName + "_label").hide();

        var container = field.closest("td, .cell, .form-group, .control");

        if (container.length > 0) {
            container.hide();
        } else {
            field.hide();
        }
    }

    function showField(fieldName) {

        var field = $("#" + fieldName);

        if (field.length === 0) {
            return;
        }

        $("#" + fieldName + "_label").show();

        var container = field.closest("td, .cell, .form-group, .control");

        if (container.length > 0) {
            container.show();
        } else {
            field.show();
        }
    }

    function disableField(fieldName) {

        var field = $("#" + fieldName);

        if (field.length === 0) {
            return;
        }

        field.prop("disabled", true);
    }

    function isUpdateForm() {

        return getCurrentRecordId() !== null;
    }

    function getCurrentRecordId() {

        var params = new URLSearchParams(window.location.search);

        var id = params.get("id") || params.get("entityid");

        if (id === null || id === "") {
            return null;
        }

        return normalizeGuid(id);
    }

    function normalizeGuid(id) {

        if (id === null || id === undefined) {
            return null;
        }

        return String(id).replace("{", "").replace("}", "");
    }