'use strict';

if (typeof CustomApp === "undefined") {
    var CustomApp = { __namespace: true };
}

var FormType =
{
    Create: 1,
    Update: 2
};

const typeProductDigital =
{
    VideoGame: 126400000,
    Licenza_Software: 126400001
};
const TypeExpansion =
{
    Base: 126400000,
    DLC: 126400001,
    Remastered: 126400002,
    Espansione: 126400003
};


CustomApp.mainProductDetails = new function () {
    var _self = this;

    _self.onload = function (executionContext) {
        var formContext = executionContext.getFormContext();

        if (formContext.ui.getFormType() == FormType.Create) {


        } else if (formContext.ui.getFormType() == FormType.Update) {

            _self.HideColumnsProductDetails(executionContext);
            var typeProductDetailControl = formContext.getControl("header_sc_typedigitalproduct");

            if (typeProductDetailControl != null) {
                typeProductDetailControl.setDisabled(true);
            }

            var typeExpansionAttr = formContext.getAttribute("sc_typeexpansion");

            if (typeExpansionAttr != null) {
                typeExpansionAttr.addOnChange(_self.HideColumnsTypeExpansion);
            }
            _self.HideColumnsTypeExpansion(executionContext);

            /*Xrm.Navigation.openAlertDialog({
                title: "Benvenuti a Product Details",
                text: "Ciao, siamo disponibili!"
            });*/
        }
    };



    _self.HideColumnsTypeExpansion = function (executionContext) {

        var formContext = executionContext.getFormContext();

        var typeexpansionValue = formContext.getAttribute("sc_typeexpansion").getValue();
        var typeexpansionControl = formContext.getControl("sc_typeexpansion");

        if (typeexpansionControl == null) {
            return;
        }

        if (typeexpansionValue === null) {
            Xrm.Navigation.openAlertDialog({
                text: "Scegli prima una typeexpansion d'acquistare"
            });
            return;
        }

        if (typeexpansionValue == TypeExpansion.Base
            || typeexpansionValue == TypeExpansion.Espansione
              || typeexpansionValue == TypeExpansion.Remastered) {

            typeexpansionControl.removeOption(TypeExpansion.DLC);
        } else {

            typeexpansionControl.removeOption(TypeExpansion.Base);
            typeexpansionControl.removeOption(TypeExpansion.Espansione);
            typeexpansionControl.removeOption(TypeExpansion.Remastered);
        }
    }

    _self.HideColumnsProductDetails = function (executionContext) {

        var formContext = executionContext.getFormContext();

        var tipoProductDetail = formContext.getAttribute("sc_typedigitalproduct");

        if (tipoProductDetail === null) {

            Xrm.Navigation.openAlertDialog({
                title: "tipoProductDetail ",
                text: "optionset non sono disponibili!"
            });
            return;
        }

        switch (tipoProductDetail.getValue()) {

            case typeProductDigital.VideoGame:

                formContext.getControl("sc_typelicense").setVisible(false);
                formContext.getControl("sc_numberofstations").setVisible(false);
                formContext.getControl("sc_durationmonths").setVisible(false);


                break;

            case typeProductDigital.Licenza_Software:

                formContext.getControl("sc_pg").setVisible(false);
                formContext.getControl("sc_typeexpansion").setVisible(false);
                formContext.getControl("sc_genre").setVisible(false);
                formContext.getControl("sc_releasedate").setVisible(false);

                break;

            default:

                formContext.getControl("sc_pg").setVisible(false);
                formContext.getControl("sc_typeexpansion").setVisible(false);
                formContext.getControl("sc_genre").setVisible(false);
                formContext.getControl("sc_releasedate").setVisible(false);
                formContext.getControl("sc_typelicense").setVisible(false);
                formContext.getControl("sc_numberofstations").setVisible(false);
                formContext.getControl("sc_durationmonths").setVisible(false);

                break;
        }
    };
};
