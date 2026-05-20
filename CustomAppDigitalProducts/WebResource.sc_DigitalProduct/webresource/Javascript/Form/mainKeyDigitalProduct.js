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
// CustomApp.mainKeyDigitalProduct.onload

CustomApp.mainKeyDigitalProduct = new function () {
    var _self = this;

    _self.onload = function (executionContext) {
        var formContext = executionContext.getFormContext();

        var keydigitaleValue = formContext.getAttribute("sc_name") != null
            ? formContext.getAttribute("sc_name").getValue()
            : null;


        if (formContext.ui.getFormType() == FormType.Create) {

        } else if (formContext.ui.getFormType() == FormType.Update) {

            _self.HideTypePiattaforma(executionContext);
        }
    };


    _self.HideTypePiattaforma = function (executionContext) {

        var formContext = executionContext.getFormContext();

        var prodottoDigitaleValue = formContext.getAttribute("sc_digitalproductid").getValue();

        if (prodottoDigitaleValue == null || prodottoDigitaleValue.length === 0) {
            return;
        }

        var prodottoDigitaleId = prodottoDigitaleValue[0].id;
        prodottoDigitaleId = prodottoDigitaleId.replace("{", "").replace("}", "");


        Xrm.WebApi.retrieveRecord("sc_digitalproduct", prodottoDigitaleId, "?$select=sc_typedigitalproduct").then(
            function (prodottoDigitale) {

                var tipoprodottodigitale = prodottoDigitale["sc_typedigitalproduct"]; // Choice

                var piattaformaControl = formContext.getControl("sc_typeplatform");

                if (piattaformaControl == null) {
                    return;
                }

                if (tipoprodottodigitale == typeProductDigital.VideoGame) {

                    piattaformaControl.removeOption(typePiattaformaDigital.Netflix);
                    piattaformaControl.removeOption(typePiattaformaDigital.Sportify);
                    piattaformaControl.removeOption(typePiattaformaDigital.Amazon);
                    piattaformaControl.removeOption(typePiattaformaDigital.Microsoft);
                    piattaformaControl.removeOption(typePiattaformaDigital.Adobe);
                    piattaformaControl.removeOption(typePiattaformaDigital.Autodesk);
                    piattaformaControl.removeOption(typePiattaformaDigital.Norton);
                    piattaformaControl.removeOption(typePiattaformaDigital.Telegram);
                    piattaformaControl.removeOption(typePiattaformaDigital.Reseller);

                } else if (tipoprodottodigitale == typeProductDigital.Licenza_Software) {

                    piattaformaControl.removeOption(typePiattaformaDigital.Steam);
                    piattaformaControl.removeOption(typePiattaformaDigital.EA);
                    piattaformaControl.removeOption(typePiattaformaDigital.Ubisoft);
                    piattaformaControl.removeOption(typePiattaformaDigital.Psn);
                    piattaformaControl.removeOption(typePiattaformaDigital.Microsoft_Xbox);
                    piattaformaControl.removeOption(typePiattaformaDigital.Epic_Games);
                    piattaformaControl.removeOption(typePiattaformaDigital.Nintendo_eShop);
                }
            },
            function (error) {
                console.error("Errore nel recupero del Prodotto Digitale: " + error.message);
            }
        );
    };

    _self.VisiblyMessage = function (executionContext) {
        var formContext = executionContext.getFormContext();

        Xrm.Navigation.openAlertDialog({
            title: "Aggiornamento",
            text: "Ciao, questo è un alert!"
        });
    };
};