var controlsAccounting = function () {
    var data = [],
        source = {},
        adapter = {}

    var init = function (url) {

        source =
            {
                datatype: "json",
                datafields: [
                    { name: 'Id', type: 'int' },                    
                    { name: 'Process', type: 'string' },
                    { name: 'Currency', type: 'string' },
                    { name: 'Business', type: 'string' },
                    { name: 'DescriptionText', type: 'string' },
                    { name: 'HoldingAccount', type: 'string' },
                    { name: 'DebitText', type: 'string' },
                    { name: 'DebitTransactionId', type: 'string' },
                    { name: 'CreditText', type: 'string' },
                    { name: 'CreditTransactionId', type: 'string' },
                    { name: 'FlagPendiente', type: 'bool' }
                ],
                data: {                    
                    processId: $("#formularioAccounting #jqxDropProcess").jqxDropDownList('val'),
                    currencyId: $("#formularioAccounting #jqxDropCurrency").jqxDropDownList('val'),
                    businessId: $("#formularioAccounting #jqxDropBusiness").jqxDropDownList('val')
                },
                id: 'Id',
                url: url
            };

        $('#formularioAccounting #jqxGridAccounting').on('bindingcomplete', function (event) {
            $('#jqxLoader').jqxLoader('close');
        });

        var dataAdapter = new $.jqx.dataAdapter(source, {
            loadError: function (jqXHR, status, error) {
                if (jqXHR.status === 401) {
                    if (jqXHR.responseJSON != undefined)
                        window.location.href = jqXHR.responseJSON.LogOnUrl;
                    else
                        window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
                }
                else {
                    if (jqXHR.status != 200)
                        desplegarMensaje(cultureName == "en-US" ? "Loading Data Error . Contact your administrator." : "Error al Cargar los datos. Contáctese con su administrador.", 'Error');
                }
            }
        });
        //____________________________

        $("#formularioAccounting #jqxGridAccounting").on("bindingcomplete", function () {
            $(this).jqxGrid("hideloadelement");
            $(this).jqxGrid({ disabled: false });
            $(this).jqxGrid('clearselection');

            $(this).jqxGrid("refreshdata");
        });

        var culture = cultureName == 'es-CL' ? 'es-ES' : 'en-US';

        //____________________________

        $("#formularioAccounting #jqxGridAccounting").jqxGrid(
			{
			    localization: Globalize.culture(culture).gridFormat,
			    autoheight: true,
			    autorowheight: false,
			    rowsheight: 30,
			    filterable: true,
			    groupable: false,
			    source: dataAdapter,
			    pageable: true,
			    statusbarheight: 30,
			    selectionmode: 'singlerow',
			    showaggregates: false,
			    showfilterrow: false,
			    columnsresize: true,
			    sortable: true,
			    width: '99%',
			    enableellipsis: true,
			    theme: 'ui-start',
			    pagesize: 10,
			    editable: false,
			    rendered: function (type) {
			        if (type == "full") {

			        }
			    },
			    columns: [
					{ datafield: 'Id', hidden: 'true', editable: false },
                    { datafield: 'HoldingAccount', text: $("label[for='HoldingAccount']").text(), width: '15%', align: 'center', cellsalign: 'left' },
                    { datafield: 'DescriptionText', text: $("label[for='DescriptionText']").text(), width: '30%', align: 'center', cellsalign: 'left' },
                    { datafield: 'DebitText', text: $("label[for='DebitText']").text(), width: '30%', align: 'center', cellsalign: 'left' },
					{ datafield: 'Process', text: $("label[for='Process']").text(), width: '20%', align: 'center', cellsalign: 'left' },
					{ datafield: 'Currency', text: $("label[for='Currency']").text(), width: '20%', align: 'center', cellsalign: 'left' },
                    { datafield: 'Business', text: $("label[for='Business']").text(), width: '20%', align: 'center', cellsalign: 'left' },   
                    { datafield: 'DebitTransactionId', text: $("label[for='DebitTransactionId']").text(), width: '30%', align: 'center', cellsalign: 'left' },
                    { datafield: 'CreditText', text: $("label[for='CreditText']").text(), width: '30%', align: 'center', cellsalign: 'left' },
                    { datafield: 'CreditTransactionId', text: $("label[for='CreditTransactionId']").text(), width: '30%', align: 'center', cellsalign: 'left' },
			    ]
			});

        $("#formularioAccounting #jqxGridAccounting").on('rowselect', function (event) {

            var args = event.args;
            var rowBoundIndex = args.rowindex;
            var rowData = args.row;

            if (JSON.parse(rowData.FlagPendiente)) {
                setTimeout(function () {
                    $("#formularioAccounting #jqxGridAccounting").jqxGrid("clearselection");
                }, 10);
            }

        });

    };

    var updateData = function () {

        var data = {            
            processId: $("#formularioAccounting #jqxDropProcess").jqxDropDownList('val'),
            currencyId: $("#formularioAccounting #jqxDropCurrency").jqxDropDownList('val'),
            businessId: $("#formularioAccounting #jqxDropBusiness").jqxDropDownList('val')
        };

        source.data = data;

        $("#formularioAccounting #jqxGridAccounting").jqxGrid('updatebounddata');
        $("#formularioAccounting #jqxGridAccounting").jqxGrid('showloadelement');
        $("#formularioAccounting #jqxGridAccounting").jqxGrid({ disabled: true });
    };

    return {
        init: init,
        updateData: updateData
    }
}
