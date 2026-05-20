'use strict';

$(document).ready(function () {

    console.log("digitalproductvalidations.js caricato");

    hideFieldsOnCreate();
    filterPlatformByProductType();
    registerEvents();
    addDigitalProductValidators();
});

const typePiattaformaDigital = {
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

const typeProductDigital = {
    VideoGame: 126400000,
    Licenza_Software: 126400001
};

let originalPlatformOptions = null;

function registerEvents() {

    $("#sc_typedigitalproduct").on("change", function () {
        filterPlatformByProductType();
    });
}

function hideFieldsOnCreate() {

    hideField("sc_productdetailsid");
    hideField("sc_parentdigitalproductid");
    hideField("sc_accountclientid");
}

function filterPlatformByProductType() {

    const $platform = $("#sc_typeplatform");

    if ($platform.length === 0) {
        console.log("Campo sc_typeplatform non trovato");
        return;
    }

    if (originalPlatformOptions === null) {
        originalPlatformOptions = $platform.find("option").clone();
    }

    const tipoProduct = getNumberValue("sc_typedigitalproduct");
    const allowedPlatforms = getAllowedPlatforms(tipoProduct);
    const currentValue = getNumberValue("sc_typeplatform");

    $platform.empty();

    originalPlatformOptions.each(function () {

        const optionValue = $(this).attr("value");

        if (optionValue === "") {
            $platform.append($(this).clone());
            return;
        }

        const numericOptionValue = Number(optionValue);

        if (allowedPlatforms.indexOf(numericOptionValue) !== -1) {
            $platform.append($(this).clone());
        }
    });

    if (currentValue !== null && allowedPlatforms.indexOf(currentValue) === -1) {
        $platform.val("");
        $platform.trigger("change");
    }
}

function getAllowedPlatforms(tipoProduct) {

    switch (tipoProduct) {

        case typeProductDigital.VideoGame:
            return [
                typePiattaformaDigital.Steam,
                typePiattaformaDigital.EA,
                typePiattaformaDigital.Ubisoft,
                typePiattaformaDigital.Psn,
                typePiattaformaDigital.Microsoft_Xbox,
                typePiattaformaDigital.Epic_Games,
                typePiattaformaDigital.Nintendo_eShop
            ];

        case typeProductDigital.Licenza_Software:
            return [
                typePiattaformaDigital.Microsoft,
                typePiattaformaDigital.Adobe,
                typePiattaformaDigital.Autodesk,
                typePiattaformaDigital.Norton,
                typePiattaformaDigital.Amazon,
                typePiattaformaDigital.Reseller
            ];

        default:
            return [];
    }
}

function addDigitalProductValidators() {

    if (typeof Page_Validators === "undefined") {
        console.log("Page_Validators non disponibile");
        return;
    }

    addValidator("sc_name", "Il campo Name è obbligatorio.", function () {
        return hasValue("sc_name");
    });


    addValidator("sc_typedigitalproduct", "Il campo Tipo Prodotto Digitale è obbligatorio.", function () {
        return hasValue("sc_typedigitalproduct");
    });

    addValidator("sc_typeplatform", "Il campo Piattaforma è obbligatorio.", function () {
        return hasValue("sc_typeplatform");
    });

    addValidator("sc_typeplatform", "La piattaforma selezionata non è valida per questo tipo di prodotto.", function () {
        return isPlatformValidForProductType();
    });

    addValidator("sc_baseprice", "Il campo Base Price deve essere maggiore di 0.", function () {
        const basePrice = getDecimalValue("sc_baseprice");
        return basePrice !== null && basePrice > 0;
    });
}

function isPlatformValidForProductType() {

    const tipoProduct = getNumberValue("sc_typedigitalproduct");
    const platform = getNumberValue("sc_typeplatform");

    if (tipoProduct === null || platform === null) {
        return false;
    }

    const allowedPlatforms = getAllowedPlatforms(tipoProduct);

    return allowedPlatforms.indexOf(platform) !== -1;
}

function addValidator(fieldName, message, validationFunction) {

    const validator = document.createElement("span");

    validator.style.display = "none";
    validator.id = fieldName + "_custom_validator";
    validator.controltovalidate = fieldName;

    validator.errormessage =
        "<a href='#" + fieldName + "_label' " +
        "onclick='javascript:scrollToAndFocus(\"" + fieldName + "_label\", \"" + fieldName + "\"); return false;'>" +
        message +
        "</a>";

    validator.validationGroup = "";
    validator.initialvalue = "";
    validator.evaluationfunction = validationFunction;

    Page_Validators.push(validator);
}

function hasValue(fieldName) {

    const value = $("#" + fieldName).val();

    return value !== null &&
        value !== undefined &&
        String(value).trim() !== "";
}

function getNumberValue(fieldName) {

    const value = $("#" + fieldName).val();

    if (value === null || value === undefined || value === "") {
        return null;
    }

    const numberValue = Number(value);

    if (isNaN(numberValue)) {
        return null;
    }

    return numberValue;
}

function getDecimalValue(fieldName) {

    let value = $("#" + fieldName).val();

    if (value === null || value === undefined || value === "") {
        return null;
    }

    value = String(value).replace(/[^\d,.-]/g, "");

    if (value.indexOf(",") > -1 && value.indexOf(".") > -1) {
        value = value.replace(/\./g, "").replace(",", ".");
    } else {
        value = value.replace(",", ".");
    }

    const decimalValue = Number(value);

    if (isNaN(decimalValue)) {
        return null;
    }

    return decimalValue;
}

function hideField(fieldName) {

    const $field = $("#" + fieldName);

    if ($field.length === 0) {
        console.log("Campo non trovato:", fieldName);
        return;
    }

    $("#" + fieldName + "_label").hide();

    const $container = $field.closest("td, .cell, .form-group, .control");

    if ($container.length > 0) {
        $container.hide();
    } else {
        $field.hide();
    }
}