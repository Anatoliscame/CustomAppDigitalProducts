'use strict';

if (typeof CustomApp === "undefined") {
    var CustomApp = { __namespace: true };
}

var FormType =
{
    Create: 1,
    Update: 2
};

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

CustomApp.mainDigitalProduct = new function () {
    var _self = this;
    var _redirectAlreadyDone = false;

    _self.onload = function (executionContext) {
        var formContext = executionContext.getFormContext();

        var productDetailsValue = formContext.getAttribute("sc_productdetailsid") != null
            ? formContext.getAttribute("sc_productdetailsid").getValue()
            : null;
        var productDetailsControl = formContext.getControl("sc_productdetailsid");

        if (formContext.ui.getFormType() == FormType.Create) {

            formContext.data.entity.addOnPostSave(_self.OpenSpecificEntityAfterCreate);

            if (productDetailsControl != null) {
                productDetailsControl.setVisible(false);
            }
            formContext.getControl("sc_parentdigitalproductid").setVisible(false);
            formContext.getControl("sc_accountclientid").setVisible(false);

        } else if (formContext.ui.getFormType() == FormType.Update) {


            _self.HideTypePiattaforma(executionContext);

            if (productDetailsControl != null) {

                if (productDetailsValue != null && productDetailsValue.length > 0) {
                    productDetailsControl.setVisible(true);
                } else {
                    productDetailsControl.setVisible(false);
                }
            }

            _self.SetFieldsDisabled(executionContext);

            var prodottoDigitaleId = formContext.data.entity.getId();
            prodottoDigitaleId = prodottoDigitaleId.replace("{", "").replace("}", "");
            var valueTypeProduct = _self.RetriveValueTypeProduct(executionContext, prodottoDigitaleId);
            if (valueTypeProduct == 0) {
                Xrm.Navigation.openAlertDialog({ text: "Il campo sc_typeexpansion non e' entrato nel metodo" });
                return;
            }
            if (valueTypeProduct == -1) {
                Xrm.Navigation.openAlertDialog({ text: "Il campo type expansion o licenza e' vuoto." });
            }  

            if (valueTypeProduct == -3) {
                Xrm.Navigation.openAlertDialog({
                    text: "Product Details non è collegato al Prodotto Digitale."
                });
            }

            var tipoProductAttr = formContext.getAttribute("sc_typedigitalproduct");

            if (tipoProductAttr.getValue() == typeProductDigital.VideoGame) {

                if (valueTypeProduct == 126400000 //BaseGame
                    || valueTypeProduct == 126400002 //Remastered
                    || valueTypeProduct == 126400003 //Espansione
                ) {

                    formContext.getControl("sc_parentdigitalproductid").setVisible(false);
                    //formContext.getControl("sc_accountcliente").setVisible(true);
                } else {
                    _self.HideBasePriceIfDlcWithParent(executionContext, valueTypeProduct);
                    formContext.getControl("sc_parentdigitalproductid").setVisible(true);
                    //formContext.getControl("sc_accountcliente").setVisible(false);
                }
            }
            else if (tipoProductAttr.getValue() == typeProductDigital.Licenza_Software) {
                if (valueTypeProduct == 126400000 //Perpetua
                    || valueTypeProduct == 126400001 //Mensile
                    || valueTypeProduct == 126400002 //Annuale
                    || valueTypeProduct == 126400003 //Trial
                    || valueTypeProduct == 126400004 //Lifetime
                    || valueTypeProduct == 126400005 //Enterprise                 
                ) {
                    formContext.getControl("sc_parentdigitalproductid").setVisible(false);
                } else {
                    formContext.getControl("sc_parentdigitalproductid").setVisible(true);
                }
            }
        }
    };

    _self.VisiblyMessage = function (executionContext) {
        var formContext = executionContext.getFormContext();

        Xrm.Navigation.openAlertDialog({
            title: "Aggiornamento",
            text: "Ciao, questo è un alert!"
        });
    };



    _self.HideBasePriceIfDlcWithParent = function (executionContext, valueTypeProduct) {
        var formContext = executionContext.getFormContext();

        var basePriceControl = formContext.getControl("sc_baseprice");
        var parentDigitalProductAttr = formContext.getAttribute("sc_parentdigitalproductid");

        if (basePriceControl === null || parentDigitalProductAttr === null) {
            return;
        }

        var parentDigitalProductValue = parentDigitalProductAttr.getValue();

        if (valueTypeProduct === 126400001)// DLC
        {
            var hasParentDigitalProduct = parentDigitalProductValue !== null && parentDigitalProductValue.length > 0;

            if (hasParentDigitalProduct) {
                basePriceControl.setVisible(false);
            } else {
                basePriceControl.setVisible(true);
            }
        } else {
            basePriceControl.setVisible(true); // In rari casi
        }
    };


    _self.HideTypePiattaforma = function (executionContext) {

        var formContext = executionContext.getFormContext();

        var tipoProductAttr = formContext.getAttribute("sc_typedigitalproduct");
        var piattaformaAttr = formContext.getAttribute("sc_typeplatform");
        var piattaformaControl = formContext.getControl("sc_typeplatform");

        if (tipoProductAttr === null || piattaformaAttr === null || piattaformaControl === null) {

            Xrm.Navigation.openAlertDialog({
                title: "tipoProductAttr e piattaformaAttr",
                text: "Due optionset non sono disponibili!"
            });

            return;
        }

        var tipoProduct = tipoProductAttr.getValue();
        var attualeValuePiattaforma = piattaformaAttr.getValue();

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

        piattaformaControl.clearOptions();

        for (var i = 0; i < allowedOptions.length; i++) {
            piattaformaControl.addOption(allowedOptions[i]);
        }

        var isAttualeValueAllowed = false;

        for (var j = 0; j < allowedOptions.length; j++) {

            if (allowedOptions[j].value === attualeValuePiattaforma) {
                isAttualeValueAllowed = true;
                break;
            }
        }

        if (attualeValuePiattaforma !== null && isAttualeValueAllowed === false) {
            piattaformaAttr.setValue(null);
            piattaformaAttr.fireOnChange();
        }
    };

    _self.SetFieldsDisabled = function (executionContext) {
        var formContext = executionContext.getFormContext();

        formContext.getControl("sc_typedigitalproduct").setDisabled(true);
        formContext.getControl("sc_name").setDisabled(true);
        formContext.getControl("sc_codice").setDisabled(true);

    };


    _self.RetriveValueTypeProduct = function (executionContext, prodottoDigitaleId) {
        var formContext = executionContext.getFormContext();

        var valueTypeProduct = 0;

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/sc_digitalproducts(" + prodottoDigitaleId + ")?$select=sc_digitalproductid&$expand=sc_productdetailsid($select=sc_typeexpansion,sc_typelicense)", false);
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

                        var tipoExpansion = resultProdottoDigitale.sc_productdetailsid.sc_typeexpansion;
                        var tipoLicenza = resultProdottoDigitale.sc_productdetailsid.sc_typelicense;

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
                } else {
                    console.log(this.responseText);
                }
            }
        };
        req.send();
        return valueTypeProduct;
    }



    _self.OpenSpecificEntityAfterCreate = function (executionContext) {
        var formContext = executionContext.getFormContext();

        if (_redirectAlreadyDone === true) {
            return;
        }

        var tipoProductAttr = formContext.getAttribute("sc_typedigitalproduct");


        if (tipoProductAttr == null) {
            Xrm.Navigation.openAlertDialog({
                title: "Campo mancanto",
                text: "Tipo prodotto digitale non disponibile."
            });
            return;
        }

        var tipoProduct = tipoProductAttr.getValue();

        if (tipoProduct == null) {
            return;
        }
    };
};