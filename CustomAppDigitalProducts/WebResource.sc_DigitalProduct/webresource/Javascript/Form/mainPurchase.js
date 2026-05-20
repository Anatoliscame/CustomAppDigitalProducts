'use strict';

if (typeof CustomApp === "undefined") {
    var CustomApp = { __namespace: true };
}

var FormType =
{
    Create: 1,
    Update: 2
};

const typeAcquisto =
{
    Effetuato: 126400000,
    In_attesa: 126400001,
    Annullato: 126400002
}

CustomApp.mainPurchase = new function () {
    var _self = this;

    _self.onload = function (executionContext) {
        var formContext = executionContext.getFormContext();

        if (formContext.ui.getFormType() == FormType.Create) {


        } if (formContext.ui.getFormType() == FormType.Update) {

            //var acquistoId = formContext.data.entity.getId().replace("{", "").replace("}", "");

            //formContext.getAttribute("sc_statuspurchase").addOnChange(function () { _self.StatusHideEffettuato(executionContext) });

            //formContext.getAttribute("sc_statuspurchase").addOnChange(_self.OnChangeAcquistoDisableEffetuato);
            //_self.StatusHideEffettuato(executionContext);

            _self.OnChangeAcquistoDisableEffetuato(executionContext);

        }
    };

    _self.OnChangeAcquistoDisableEffetuato = function (executionContext) {
        var formContext = executionContext.getFormContext();

        var statuspurchase = formContext.getAttribute("sc_statuspurchase").getValue();

        if (statuspurchase == typeAcquisto.Effetuato) {

            formContext.getControl("sc_name").setDisabled(true);
            formContext.getControl("sc_code").setDisabled(true);
            formContext.getControl("sc_accountclientid").setDisabled(true);
            formContext.getControl("sc_purchasedate").setDisabled(true);
            formContext.getControl("sc_invoice").setDisabled(true);
            formContext.getControl("sc_total").setDisabled(true);

        } else {

            formContext.getControl("sc_name").setDisabled(false);
            formContext.getControl("sc_code").setDisabled(false);
            formContext.getControl("sc_accountclientid").setDisabled(false);
            formContext.getControl("sc_purchasedate").setDisabled(false);
            formContext.getControl("sc_invoice").setDisabled(false);
            formContext.getControl("sc_total").setDisabled(false);
        }
    }
    /*
    _self.StatusHideEffettuato = function (executionContext) {
        var formContext = executionContext.getFormContext();
        var status = formContext.getAttribute("sc_statuspurchase").getValue();

        if (status !== null) {
            var StatusOptionSet = formContext.getControl("sc_statuspurchase");

            if (status !== typeAcquisto.Effetuato) {
                StatusOptionSet.removeOption(typeAcquisto.Effetuato);
                StatusOptionSet.addOption({ text: 'Annullato', value: typeAcquisto.Annullato }, 3);
            } else {
                StatusOptionSet.addOption({ text: 'Effetuato', value: typeAcquisto.Effetuato }, 1);
                StatusOptionSet.removeOption(typeAcquisto.Annullato);
            }
        }
    }*/
}