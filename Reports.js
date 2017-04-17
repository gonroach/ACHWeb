/*Funciones Globales Reportes*/
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

function buildQueryString(data) {
    var str = '';
    for (var prop in data) {
        if (data.hasOwnProperty(prop)) {
            str += prop + '=' + data[prop] + '&';
        }
    }
    return str.substr(0, str.length - 1);
}

//validar Internet Explorer
function getInternetExplorerVersion() {
    var rv = -1;
    if (navigator.appName == 'Microsoft Internet Explorer') {
        var ua = navigator.userAgent;
        var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
        if (re.exec(ua) != null)
            rv = parseFloat(RegExp.$1);
    }
    else if (navigator.appName == 'Netscape') {
        var ua = navigator.userAgent;
        var re = new RegExp("Trident/.*rv:([0-9]{1,}[\.0-9]{0,})");
        if (re.exec(ua) != null)
            rv = parseFloat(RegExp.$1);
    }
    return rv;
}

//Cambia el tamaño del frame
var ChangeJqxWindowSize = function (id, tamReporte) {
    $(".jqx-window-header").width(tamReporte);

    var styles = {
        width: tamReporte,
        overflow: "auto",
        padding: "0 0 0 10px"
    };

    $(".jqx-window-content").css(styles);


    $(id).css({ "width": tamReporte + 11 })

    $(id).jqxWindow('move', ($(document).width() - $(id).width()) / 2, 50);
}

//+X+1 px al width en internet explorer para visualizacion del iframe - retorna el nuevo tamaño
var ChangeWidthIFrame = function (mas) {
    if (mas == undefined)
        mas = 0;

    var width = $("iframe[id!='exportExcelFrame']").width();
    mas = mas + width + 1;
    if (getInternetExplorerVersion() > 0) // If Internet Explorer, return version number
    {
        $("iframe[id!='exportExcelFrame']").width(mas);
        $("iframe[id!='exportExcelFrame']").css({ "padding": "0 20 0 0", "overflow": "hidden", "margin": "0" });
        //console.log("width: " + (mas) + " IE")
    }
    else {
        $("iframe[id!='exportExcelFrame']").width(width + 1);
        //console.log("width: " + (width + 1) + " No IE");
    }
    return mas;
}

var FixVisualIssuesIE = function () {
    if (getInternetExplorerVersion() > 0) // If Internet Explorer, return version number
    {
        //Corrije visualizacion de los input jqWidget IE
        $('input[type=text]').css({ "top": "-3px" })
    }
}

/*Obtener filter*/
var ObtenerFormularioFiltro = function () {
    var filter = $("#FiltrarForm").serializeObject();
    return filter;
}
//clase javascript utilizada para contener url parametrizadas
var ReporteUrls = function () {
    this.urlFileImportedGrid = $("#urlFileImportedGrid").val();
    this.ProcessingLog = $("#urlProcessingLogGrid").val();
    this.TrandeFiles = $("#urlTrandeFilesGrid").val();
    this.TransactionbyProcess = $("#urlTransactionbyProcessGrid").val();
    this.TransferbyBranch = $("#urlTransferbyBranchGrid").val();
    this.reportProcessingLog = $("#urlreportProcessingLogGrid").val();
    this.TransferbyProduct = $("#urlTransferbyProductGrid").val();
    this.EventLog = $("#urlEventLogGrid").val();
    this.EventLogDetail = $("#urlEventLogDetailView").val();
    this.CloseBatchReport = $("#urlCloseBatchGrid").val();
    this.SettlementReport = $("#urlSettlementGrid").val();
    this.XmlReceiveR2View = $("#urlXmlReceiveR2View").val();
    this.XmlReceiveR2Prototipo = $("#urlXmlReceiveR2Prototipo").val();
    this.XmlSendR2View = $("#urlXmlSendR2View").val();
    this.XmlSendR2Prototipo = $("#urlXmlSendR2Prototipo").val();

    this.XmlReceiveR2ViewApproval = $("#urlXmlReceiveR2ViewApproval").val();
    this.XmlReceiveR2Approval = $("#urlXmlReceiveR2Approval").val();
    this.XmlSendR2ViewApproval = $("#urlXmlSendR2ViewApproval").val();
    this.XmlSendR2Approval = $("#urlXmlSendR2Approval").val();

    //Release 2
    this.UrlMessageReceived = $("#UrlMessageReceived").val();

}

var EventosUrls = function () {
    this.ProcessingLogClick = $("#ProcessingLogClick").val();
    this.HitValidationReportsClick = $("#HitValidationReportsClick").val();
    this.HitValidationReportsPrototipoClick = $("#HitValidationReportsPrototipoClick").val();
    this.EventLogClick = $("#EventLogClick").val();
    this.AccountingbySummaryClick = $("#AccountingbySummaryClick").val();
    this.RejectedbyCosmosClick = $("#RejectedbyCosmosClick").val();
    this.TransactionbyProcessClick = $("#TransactionbyProcessClick").val();
    this.TrandeFilesClick = $("#TrandeFilesClick").val();
    this.TransferbyProcessClick = $("#TransferbyProcessClick").val();
    this.TransferbyProductClick = $("#TransferbyProductClick").val();
    this.TransferbyBranchClick = $("#TransferbyBranchClick").val();
    this.FilesImportedClick = $("#FilesImportedClick").val();
    this.CloseBatchClick = $("#CloseBatchClick").val();

    //Release 2
    this.MessageReceivedClick = $("#MessageReceivedClick").val();
    this.MessageSendClick = $("#MessageSendClick").val();
}

/*Exportar Grilla Generico*/
var ExportExcelNew = function (idGrilla, tipoExportacion, modulo, titulo, url, action) {

    var data = $('#' + idGrilla).jqxGrid('exportdata', 'JSON', null, 'true', null, 'true');
    $("#ArregloExportacion").val(data);
    $("#TipoExportacion").val(tipoExportacion);
    $("#Modulo").val(modulo);
    $("#TituloReporte").val(titulo);
    //$("#DescargarDocumento").trigger("submit");

    var url = $("#DescargarDocumento").attr('action');//$("#urlExportarExcel").val();
    var form = $("#DescargarDocumento").serializeObject();
    var ua = window.navigator.userAgent;
    var msie = ua.indexOf("MSIE ");

    $.post(url, form, "json")
        .done(function (data) {
            CerrarLoading();
            if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
                exportExcelFrame.document.open("application/vnd.ms-excel", "replace");
                exportExcelFrame.document.write(data);
                exportExcelFrame.document.close();
                exportExcelFrame.focus();
                sa = exportExcelFrame.document.execCommand("SaveAs", true, "Report.xls");
            }
            else
                sa = window.open('data:application/vnd.ms-excel,' + encodeURIComponent(data));

            return (sa);

        })
        .fail(function () {
            CerrarLoading();
        });
}

var ExportarGrillaExcel = function (idGrilla, tipoExportacion, modulo, titulo) {
    var grilla = $('#' + idGrilla).jqxGrid('exportdata', 'JSON', null, 'true', null, false);

    var arregloTable = JSON.stringify([$("#tableHeadResume").jqxDataTable('getRows')[0]]);

    var url = $("#urlExportarExcel").val();

    var ua = window.navigator.userAgent;
    var msie = ua.indexOf("MSIE ");

    $.post(url, { arreglo: grilla, titulo: titulo, arregloTable: arregloTable }, "json")
    .done(function (data) {
        CerrarLoading();
        if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
            exportExcelFrame.document.open("txt/html", "replace");
            exportExcelFrame.document.write(data.html);
            exportExcelFrame.document.close();
            exportExcelFrame.focus();
            sa = exportExcelFrame.document.execCommand("SaveAs", true, "Report.xls");
        }
        else {
            sa = window.open('data:application/vnd.ms-excel,' + encodeURI(data.html));
            return (sa);
        }
    })
    .fail(function () {
        CerrarLoading();
    });
}

var ExportarTableGrillaExcel = function (idGrilla, tipoExportacion, modulo, titulo) {
    var grilla = $('#' + idGrilla).jqxGrid('exportdata', 'JSON', null, 'true', null, false);

    var arregloTable = $('#jqxTableMessageCitiscreening').jqxGrid('exportdata', 'JSON', null, 'true', null, false);

    $("#arreglo").val(grilla);
    $("#titulo").val(titulo);
    $("#arregloTable").val(arregloTable);
    $("#tipo").val(tipoExportacion);
    $("#DownloadReport").trigger("submit");
}

/*Exportar Grilla Generico*/

var ExportarGrilla = function (idGrilla, tipoExportacion, modulo, titulo, url, action) {

    var data = $('#' + idGrilla).jqxGrid('exportdata', 'JSON', null, 'true', null, 'true');
    //JSON.stringify($("#tableHeadResume").jqxDataTable('getRows')[0])
    $("#ArregloExportacion").val(data);
    $("#TipoExportacion").val(tipoExportacion);
    $("#Modulo").val(modulo);
    $("#TituloReporte").val(titulo);
    $("#DescargarDocumento").trigger("submit");
}

var ConfigureLoadingForReport = function () {
    $("#jqxLoader").css("z-index", "99999");
    $("#jqxLoaderModal").css("z-index", "90000");
    $("#jqxLoaderModal").css("opacity", 0.9);
}

var ResetLoadingForReport = function () {
    $("#jqxLoader").css("z-index", "999");
    $("#jqxLoaderModal").css("z-index", "999");
    $("#jqxLoaderModal").css("opacity", 0.6);
}

/*<=Controles Reportes=>*/
$(function () {

    var work = cultureName == "en-US" ? "Loading..." : "Cargando..."
    $("#jqxLoader").jqxLoader({ isModal: true, theme: tema, text: work });

    urls = new ReporteUrls();

    controlsFileImported = new ControlsFileImported(urls);
    controlsTransferbyBranch = new ControlsTransferbyBranch(urls);
    controlsTransferbyProduct = new ControlsTransferbyProduct(urls);
    controlsTrandeFiles = new ControlsTrandeFiles(urls);
    controlsTransactionbyProcess = new ControlsTransactionbyProcess(urls);
    controlsProcessingLog = new ControlsProcessingLog(urls);
    controlsEventLog = new ControlsEventLog(urls);
    controlsAuditLog = new controlsAuditLog();
    TransactionbyProcessDetail = new TransactionbyProcessDetail();
    controlsCloseBatchReport = new controlsCloseBatchReport(urls);
    controlsSettlementReport = new controlsSettlementReport(urls);

    reportProcessingLog = new ReportProcessingLog(urls);
    controlsEventDetailLog = new controlsEventDetailLog(urls);
    controlsEventLogR2Prototipo = new controlsEventLogR2Prototipo(urls);
    controlsEventDetailLogR2Prototipo = new controlsEventDetailLogR2Prototipo(urls);
    controlsLogXmlReceiveCitiscreening = new controlsLogXmlReceiveCitiscreening(urls);

    //Release 2
    controlsEventHitValidationReport = new controlsEventHitValidationReport();

    eventLogMessageReceiveCitiscreening = new EventLogMessageReceiveCitiscreening(urls);

    $("#btnSearch").on('click', function (event) {
        $("#Evento").val(event.originalEvent.type);
        $("#Evento").val();
    });

    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (searchString, position) {
            position = position || 0;

            if (this.indexOf != undefined)
                return this.indexOf(searchString, position) === position;
        };
    }

});

//*Funcion encargada de Cerrar el icono de carga*//
var CerrarLoading = function () {
    $('#jqxLoader').jqxLoader('close');
    ResetLoadingForReport();
}

//*Funcion encargada de mostrar un spining de carga al hacer ajax*//
var AbrirLoading = function () {
    $(document).ajaxSend(function (event, xhr, settings) {
        $('#jqxLoader').jqxLoader('open');
    });

}

/*XML de la grilla*/

var processXML = function (txt) {
    var xml = "";
    for (var i = 0, len = txt.length; i < len; i++) {

        xml = xml + txt[i];
        if (txt[i] == ">" && txt[i + 1] == "<") {
            xml = xml + "<br/>"
        }
    }

    return xml;
}

//________________________ Release 2

var MessageReceivedClick = function (id) {
    eventosUrls = new EventosUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();

    $.get(eventosUrls.MessageReceivedClick, '{}', "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();
                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: "Message Received", theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                    okButton: $('#modalMessageCitiscreening #btnOk'),
                    initContent: function () {
                        $('#modalMessageCitiscreening #btnOk').jqxButton({ width: '100px', theme: 'ui-start' });
                        $('#modalMessageCitiscreening #btnOk').focus();

                        $("#excelExportDetail").on('click', function (event) {
                            if ($('#jqxGridMessageCitiscreening').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarTableGrillaExcel("jqxGridMessageCitiscreening", "Excel", "MessageSent", "Message Citiscreening Report");
                        });

                        $("#pdfExportDetail").on('click', function (event) {
                            if ($('#jqxGridMessageCitiscreening').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarTableGrillaExcel("jqxGridMessageCitiscreening", "Pdf", "MessageSent", "Message Citiscreening Report");
                        });
                    }
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                CerrarLoading();

                eventLogMessageReceiveCitiscreening.init(id, window);

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}


var MessageSendClick = function (id) {

    eventosUrls = new EventosUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    $.get(eventosUrls.MessageSendClick, { eventOnlineLogId: id }, "html")
    .done(function (data) {
        if (data.answer == undefined) {
            var window = $(data);
            window.appendTo(document.body);
            var htmlWidth = $(document).width();
            var ninePorcentWidth = htmlWidth * 0.3;

            $(window).jqxWindow({
                zIndex: 500,
                autoOpen: false, draggable: false, minHeight: '316px', maxHeight: '330px', isModal: true, resizable: false, theme: 'ui-start', height: '430', width: '330', maxWidth: ninePorcentWidth,
                initContent: function () {
                    //$("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                }
            });

            $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
            $(window).jqxWindow('open');

            CerrarLoading();

        } else {
            if (data.error)
                desplegarMensaje(data.msg, "Advertencia");
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', "Advertencia");

            CerrarLoading();
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}


//________________________

var XmlSendClick = function (row) {
    var data = $('#jqxGridEventLogDetail').jqxGrid('getrowdata', row);
    var titulo = cultureName == "en-US" ? "Xml Send." : "Xml Enviado.";
    data.MessageSent = data.MessageSent.replace("<xmp>", "");
    data.MessageSent = data.MessageSent.replace("</xmp>", "");

    $('#preXmlSend').text(data.MessageSent);

    var window = $('#modalWindowXmlSend');
    window.appendTo(document.body);

    $(window).jqxWindow({
        maxWidth: 900, maxHeight: 900, minHeight: 30, minWidth: 250, height: 'auto', width: 'auto', position: 'center, center',
        resizable: false, isModal: true, modalOpacity: 0.3, theme: tema, autoOpen: false, title: titulo,
        initContent: function () {

        }
    });
    window.unbind('open'); window.bind('open');
    window.on('open', function (event) {
        $('#preXmlSend').val("");
        $('#preXmlSend').val(data.MessageSent);
        $('#preXmlSend').format({ method: 'xml' });
    });

    window.jqxWindow('open');

};

//________________________

var XmlSendClickPrototipo = function (row) {
    var data = $('#jqxGridEventLogDetailR2Prototipo').jqxGrid('getrowdata', row);
    var titulo = cultureName == "en-US" ? "Xml Send." : "Xml Enviado.";
    data.MessageSent = data.MessageSent.replace("<xmp>", "");
    data.MessageSent = data.MessageSent.replace("</xmp>", "");

    $('#preXmlSend').text(data.MessageSent);

    var window = $('#modalWindowXmlSend');
    window.appendTo(document.body);

    $(window).jqxWindow({
        maxWidth: 900, maxHeight: 900, minHeight: 30, minWidth: 250, height: 'auto', width: 'auto', position: 'center, center',
        resizable: false, isModal: true, modalOpacity: 0.3, theme: tema, autoOpen: false, title: titulo,
        initContent: function () {

        }
    });
    window.unbind('open'); window.bind('open');
    window.on('open', function (event) { // Some code here.
        $('#preXmlSend').val("");
        $('#preXmlSend').val(data.MessageSent);
        $('#preXmlSend').format({ method: 'xml' });
    });

    window.jqxWindow('open');

};
//________________Link grilla approval________________________________________

var XmlSendClickR2Approval = function (id) {
    eventosUrls = new ReporteUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    alert("approval")
    $.get(eventosUrls.XmlSendR2ViewApproval, { eventOnlineLogId: id }, "html")
    .done(function (data) {
        if (data.answer == undefined) {
            var window = $(data);
            window.appendTo(document.body);

            var htmlWidth = $(document).width();

            var ninePorcentWidth = htmlWidth * 0.3;

            $(window).jqxWindow({
                zIndex: 500,
                autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: "Prototipo", theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                initContent: function () {
                    //$("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                }
            });

            $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
            $(window).jqxWindow('open');

            CerrarLoading();

        } else {
            if (data.error)
                desplegarMensaje(data.msg, "Advertencia");
            else
                desplegarMensaje(data.msg, "Advertencia");

            CerrarLoading();
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}

//........

var XmlReceiveClickR2Approval = function (id) {

    eventosUrls = new ReporteUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';
    console.log(eventosUrls.XmlReceiveR2ViewApproval)
    ConfigureLoadingForReport();
    AbrirLoading();

    $.get(eventosUrls.XmlReceiveR2ViewApproval, '{}', "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();

                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: "Prototipo", theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                    //okButton: $('#btnOk'),
                    initContent: function () {
                        //$("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                        console.log("init")
                        $("#tableHeadResume").jqxDataTable(
                        {
                            altRows: true,
                            sortable: false,
                            editable: false,
                            selectionMode: 'singleRow',
                            theme: tema,
                            columns: [
                              { text: 'TXNID *', dataField: 'TXNID*', width: 200 },
                              { text: 'RULESET NAME', dataField: 'RULESET NAME', width: 200 },
                              { text: 'HIT COUNT', dataField: 'HIT COUNT', width: 250 }
                            ]
                        });


                        $("#excelExportDetail").on('click', function (event) {
                            if ($('#jqxGridCitiscreeningReceiveR2Prototipo').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarGrilla("jqxGridCitiscreeningReceiveR2Prototipo", "Excel", "Prototipo", "Prototipo Report");
                        });

                        $("#pdfExportDetail").on('click', function (event) {
                            if ($('#jqxGridCitiscreeningReceiveR2Prototipo').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarGrilla("jqxGridCitiscreeningReceiveR2Prototipo", "Pdf", "Prototipo", "Prototipo Report");
                        });
                    }
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                CerrarLoading();

                controlsLogXmlReceiveCitiscreening.init(id);

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}



//____________________________________________________________________________



//________________________

var XmlSendClickR2Prototipo = function (id) {
    eventosUrls = new ReporteUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    $.get(eventosUrls.XmlSendR2View, { eventOnlineLogId: id }, "html")
    .done(function (data) {
        if (data.answer == undefined) {
            var window = $(data);
            window.appendTo(document.body);

            var htmlWidth = $(document).width();

            var ninePorcentWidth = htmlWidth * 0.3;

            $(window).jqxWindow({
                zIndex: 500,
                autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: "Prototipo", theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                initContent: function () {
                    //$("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                }
            });

            $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
            $(window).jqxWindow('open');

            CerrarLoading();

        } else {
            if (data.error)
                desplegarMensaje(data.msg, "Advertencia");
            else
                desplegarMensaje(data.msg, "Advertencia");

            CerrarLoading();
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}

//........

var XmlReceiveClickR2Prototipo = function (id) {
    eventosUrls = new ReporteUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    $.get(eventosUrls.XmlReceiveR2View, '{}', "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();

                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: "Prototipo", theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                    //okButton: $('#btnOk'),
                    initContent: function () {
                        //$("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                        console.log("init")
                        $("#tableHeadResume").jqxDataTable(
                        {
                            altRows: true,
                            sortable: false,
                            editable: false,
                            selectionMode: 'singleRow',
                            theme: tema,
                            columns: [
                              { text: 'TXNID *', dataField: 'TXNID*', width: 200 },
                              { text: 'RULESET NAME', dataField: 'RULESET NAME', width: 200 },
                              { text: 'HIT COUNT', dataField: 'HIT COUNT', width: 250 }
                            ]
                        });


                        $("#excelExportDetail").on('click', function (event) {
                            if ($('#jqxGridCitiscreeningReceiveR2Prototipo').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarGrilla("jqxGridCitiscreeningReceiveR2Prototipo", "Excel", "Prototipo", "Prototipo Report");
                        });

                        $("#pdfExportDetail").on('click', function (event) {
                            if ($('#jqxGridCitiscreeningReceiveR2Prototipo').jqxGrid('getdatainformation').rowscount > 0)
                                ExportarGrilla("jqxGridCitiscreeningReceiveR2Prototipo", "Pdf", "Prototipo", "Prototipo Report");
                        });
                    }
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                CerrarLoading();

                controlsLogXmlReceiveCitiscreening.init(id);

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}

//________________________

var XmlReceivedClick = function (row) {

    var data = $('#jqxGridEventLogDetail').jqxGrid('getrowdata', row);
    var titulo = cultureName == "en-US" ? "Xml Received." : "Xml Recibido.";
    data.MessageReceived = data.MessageReceived.replace("<xmp>", "");
    data.MessageReceived = data.MessageReceived.replace("</xmp>", "");

    var window = $('#modalWindowXmlReceived');
    window.appendTo(document.body);

    $(window).jqxWindow({
        maxWidth: 900, maxHeight: 900, minHeight: 30, minWidth: 250, height: 'auto', width: 'auto', position: 'center, center',
        resizable: false, isModal: true, modalOpacity: 0.3, theme: tema, autoOpen: false, title: titulo,
        initContent: function () {

        }
    });
    window.unbind('open'); window.bind('open');
    window.on('open', function (event) { // Some code here. 
        $('#preXmlReceived').val("");
        $('#preXmlReceived').val(data.MessageReceived);
        $('#preXmlReceived').format({ method: 'xml' });
    });

    window.jqxWindow('open');
};

//________________________

var XmlReceivedClickPrototipo = function (row) {

    var data = $('#jqxGridEventLogDetailR2Prototipo').jqxGrid('getrowdata', row);
    var titulo = cultureName == "en-US" ? "Xml Received." : "Xml Recibido.";
    data.MessageReceived = data.MessageReceived.replace("<xmp>", "");
    data.MessageReceived = data.MessageReceived.replace("</xmp>", "");

    var window = $('#modalWindowXmlReceived');
    window.appendTo(document.body);

    $(window).jqxWindow({
        maxWidth: 900, maxHeight: 900, minHeight: 30, minWidth: 250, height: 'auto', width: 'auto', position: 'center, center',
        resizable: false, isModal: true, modalOpacity: 0.3, theme: tema, autoOpen: false, title: titulo,
        initContent: function () {

        }
    });
    window.unbind('open'); window.bind('open');
    window.on('open', function (event) { // Some code here. 
        $('#preXmlReceived').val("");
        $('#preXmlReceived').val(data.MessageReceived);
        $('#preXmlReceived').format({ method: 'xml' });
    });

    window.jqxWindow('open');
};

//funcion que abre el modal del reporte Processing Log
var ProcessingLogClick = function (row, eventosUrls) {
    ConfigureLoadingForReport();

    var tittle = cultureName == "en-US" ? $("#pagetitle").text() + " Detail" : "Detalle" + $("#pagetitle").text();

    $('#jqxLoader').jqxLoader('open');
    AbrirLoading();

    var data = $('#jqxGridProcessingLog').jqxGrid('getrowdata', row);
    $("#FileName").val(data.FileName);
    $("#FileId").val(data.Id);
    eventosUrls = new EventosUrls();

    $('#jqxLoader').jqxLoader('close');

    if (data.Id != null) {
        $.post(eventosUrls.ProcessingLogClick, '{}', "json")
            .done(function (data) {
                if (data.answer == undefined) {
                    var window = $(data);
                    window.appendTo(document.body);

                    var htmlWidth = $(document).width();

                    var ninePorcentWidth = htmlWidth * 0.9;

                    $(window).jqxWindow({
                        zIndex: 500,
                        autoOpen: false, draggable: false, minHeight: '400px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: '100%', maxWidth: ninePorcentWidth,
                        okButton: $('#btnOk'),
                        initContent: function () {
                            $('#btnOk').jqxButton({ width: '100px', theme: 'ui-start' });
                            $('#btnOk').focus();
                            CerrarLoading();
                            reportProcessingLog.init($("#FileName").val(), $("#FileId").val());
                        }
                    });

                    $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });

                    $(window).jqxWindow('open');

                } else {
                    if (data.error)
                        desplegarMensaje(data.msg, "Advertencia");
                    else
                        desplegarMensaje(data.msg, "Advertencia");

                    CerrarLoading();
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                CerrarLoading();

                if (jqXHR.status === 401)
                    if (jqXHR.responseJSON != undefined)
                        window.location.href = jqXHR.responseJSON.LogOnUrl;
                    else
                        window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
                else
                    desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

                CerrarLoading();
            });
    } else {
        $('#jqxLoader').jqxLoader('close');
        CerrarLoading();
        var msg = cultureName == "en-US" ? "There is no Transactions. Detail File Message : " + data.ErrorDetail : 'No Hay Transacciones. Detalle : ' + data.ErrorDetail;
        desplegarMensaje(msg, "Mensajes");
        CerrarLoading();
    }
}

//funcion que abre el modal del reporte Hit Validation Reports
var HitValidationReportsClick = function (filter) {
    eventosUrls = new EventosUrls();
    var tittle = $("#pagetitle").text();

    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.HitValidationReportsClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 1300, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                //$(window).on('open', function (event) {
                //    $(ReportViewerForMvc.getIframeId()).load(function () {
                //        setTimeout(function () {
                //            var size = ChangeWidthIFrame(25);
                //            ChangeJqxWindowSize("#jqxWinHitValidation", size);
                //            CerrarLoading();
                //        }, 5000);
                //    });
                //});

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinHitValidation", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Hit Validation Reports
var HitValidationPrototipoReportsClick = function (filter) {
    eventosUrls = new EventosUrls();
    var tittle = $("#pagetitle").text();

    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.HitValidationReportsPrototipoClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();
                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: ninePorcentWidth, maxWidth: ninePorcentWidth,
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                //$(window).on('open', function (event) {
                //    $(ReportViewerForMvc.getIframeId()).load(function () {
                //        setTimeout(function () {
                //            var size = ChangeWidthIFrame(25);
                //            ChangeJqxWindowSize("#jqxWinHitValidation", size);
                //            CerrarLoading();
                //        }, 5000);
                //    });
                //});

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }

            CerrarLoading();
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Event Log
var EventLogClick = function (row) {
    eventosUrls = new EventosUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    var filtro = ObtenerFormularioFiltro();

    var data = $('#EventLogGrid').jqxGrid('getrowdata', row);

    $("#FileId").val(data.Id);
    $("#FileName").val(data.FileName);

    $.post(eventosUrls.EventLogClick, '{}', "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();

                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: title + $("label[for='ReportDTO_EventLogLabel']").text(), theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                    okButton: $('#btnOk'),
                    initContent: function () {
                        $("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                    }
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                CerrarLoading();

                controlsEventDetailLog.init();

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//___________________________________

//funcion que abre el modal del reporte Event Log prototipo
var EventLogClickR2Prototipo = function (row) {
    eventosUrls = new EventosUrls();
    var title = cultureName == 'en-US' ? 'Detail ' : 'Detalle ';

    ConfigureLoadingForReport();
    AbrirLoading();
    var filtro = ObtenerFormularioFiltro();

    var data = $('#EventLogGrid').jqxGrid('getrowdata', row);

    $("#FileId").val(data.Id);
    $("#FileName").val(data.FileName);

    $.post(eventosUrls.EventLogClick, '{}', "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();

                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: title + $("label[for='ReportDTO_EventLogLabel']").text(), theme: 'ui-start', height: '430', width: '100%', maxWidth: ninePorcentWidth,
                    okButton: $('#btnOk'),
                    initContent: function () {
                        $("#btnOk").jqxButton({ width: 120, height: 25, theme: tema });
                    }
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                CerrarLoading();

                controlsEventDetailLogR2Prototipo.init();

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Accounting by Summary
var AccountingbySummaryClick = function (filter) {
    eventosUrls = new EventosUrls();
    var tittle = $("#pagetitle").text();
    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.AccountingbySummaryClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 655, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinAccountingSummary", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg);

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Transfer by Process
var RejectedbyCosmosClick = function (filter) {
    eventosUrls = new EventosUrls();
    var tittle = $("#pagetitle").text();
    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.RejectedbyCosmosClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 950, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinRejectedbyCosmos", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Transaction by Process
var TransactionbyProcessClick = function (row) {
    eventosUrls = new EventosUrls();
    var tittle = $("#pagetitle").text();
    var data = $('#jqxGridTransactionbyProcess').jqxGrid('getrowdata', row);
    var filter = ObtenerFormularioFiltro();
    filter.FileId = data.Id;
    eventosUrls = new EventosUrls();

    ConfigureLoadingForReport();
    AbrirLoading();
    $("html, body").animate({ scrollTop: 0 }, "slow");

    $.post(eventosUrls.TransactionbyProcessClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 775, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinTransactionbyProcess", size);

                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Trande Files
var TrandeFilesClick = function (row) {
    var data = $('#jqxGridTrandeFiles').jqxGrid('getrowdata', row);
    var tittle = $("#pagetitle").text();
    var filter = ObtenerFormularioFiltro();
    filter.FileId = data.Id;
    eventosUrls = new EventosUrls();

    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.TrandeFilesClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 650, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinTrandeFiles", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Transfer by Process
var TransferbyProcessClick = function (filter) {
    eventosUrls = new EventosUrls();
    ConfigureLoadingForReport();
    AbrirLoading();
    var tittle = $("#pagetitle").text();

    $.post(eventosUrls.TransferbyProcessClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);
                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 855, maxWidth: 1920
                });

                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinTransferbyProcess", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte Transfer by Product
var TransferbyProductClick = function (row) {
    eventosUrls = new EventosUrls();
    var data = $('#jqxGridTransferbyProduct').jqxGrid('getrowdata', row);
    var tittle = $("#pagetitle").text();

    var filter = ObtenerFormularioFiltro();
    filter.FileId = data.Id;

    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.TransferbyProductClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                ConfigureLoadingForReport();
                AbrirLoading();

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 665, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinTransferbyProduct", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte TransferbyBranch
var TransferbyBranchClick = function (row) {
    eventosUrls = new EventosUrls();
    var data = $('#jqxGridTransferbyBranch').jqxGrid('getrowdata', row);
    var tittle = $("#pagetitle").text();

    var filter = ObtenerFormularioFiltro();
    filter.FileId = data.Id;

    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.TransferbyBranchClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);
                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: 645, maxWidth: 1920
                });

                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinTransferByBranch", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);

                    });
                });


            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();
            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}

//funcion que abre el modal del reporte File Imported
var FilesImportedClick = function (row) {
    eventosUrls = new EventosUrls();
    var data = $('#jqxGridFilesImported').jqxGrid('getrowdata', row);
    var tittle = $("#pagetitle").text();

    var filter = ObtenerFormularioFiltro();
    filter.FileId = data.Id;
    ConfigureLoadingForReport();
    AbrirLoading();

    $.post(eventosUrls.FilesImportedClick, filter, "json")
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '360px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', minWidth: 500, maxWidth: 1920
                });
                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                $(window).jqxWindow('open');

                $(window).on('open', function (event) {
                    $(ReportViewerForMvc.getIframeId()).load(function () {
                        var repoI = setInterval(function () {
                            if ($(ReportViewerForMvc.getIframeId()).width() > 302) {
                                var size = ChangeWidthIFrame(25);
                                ChangeJqxWindowSize("#jqxWinFilesImported", size);
                                CerrarLoading();
                                clearInterval(repoI);
                            }
                        }, 1000);
                    });
                });

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }

        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();

            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
}



//funcion que abre el modal del reporte Transaccion por estatus
var TransactionStatusDatailClick = function (row) {
    ConfigureLoadingForReport();

    var tittle = cultureName == "en-US" ? $("#pagetitle").text() + " Detail" : " Detalle" + $("#pagetitle").text();
    var filtro = ObtenerFormularioFiltro();
    var url = $("#urlGetTranStatusDetailView").val();
    var urlReport = $("#urlGetTranStatusDetail").val();

    var dataRow = $('#jqxGridTransactionStatusReport').jqxGrid('getrowdata', row);

    filtro.FileId = dataRow.Id;

    if (dataRow.Id != null) {
        AbrirLoading();
        $.ajax({
            cache: false,
            type: "GET",
            url: url,
            data: '{}',
            dataType: 'html'
        })
        .done(function (data) {
            if (data.answer == undefined) {
                var window = $(data);
                window.appendTo(document.body);

                var htmlWidth = $(document).width();

                var ninePorcentWidth = htmlWidth * 0.9;

                $(window).jqxWindow({
                    zIndex: 500,
                    autoOpen: false, draggable: false, minHeight: '400px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: '100%', maxWidth: ninePorcentWidth,
                    okButton: $('#btnOk'),
                    initContent: function () {
                        $('#btnOk').jqxButton({ width: '100px', theme: 'ui-start' });
                        $('#btnOk').focus();

                        eventTransactionStatusDetail.init(filtro, urlReport);
                    }
                });

                $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
                CerrarLoading();
                $(window).jqxWindow('open');

            } else {
                if (data.error)
                    desplegarMensaje(data.msg, "Advertencia");
                else
                    desplegarMensaje(data.msg, "Advertencia");

                CerrarLoading();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CerrarLoading();

            if (jqXHR.status === 401)
                if (jqXHR.responseJSON != undefined)
                    window.location.href = jqXHR.responseJSON.LogOnUrl;
                else
                    window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
            else
                desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

            CerrarLoading();
        });
    } else {
        desplegarMensaje("no data", "Mensajes");
        CerrarLoading();
    }
}


//funcion que abre el modal del reporte current setting Scheduler 
var CurrentSettingSchedulerClick = function (url) {
    ConfigureLoadingForReport();
    var tittle = cultureName == "en-US" ? $("#pagetitle").text() + " Detail" : " Detalle" + $("#pagetitle").text();

    AbrirLoading();

    $.ajax({
        cache: false,
        type: "GET",
        url: url,
        data: '{}',
        dataType: 'html'
    }).done(function (data) {
        if (data.answer == undefined) {
            var window = $(data);
            window.appendTo(document.body);
            var htmlWidth = $(document).width();
            var ninePorcentWidth = htmlWidth * 0.9;
            $(window).jqxWindow({
                zIndex: 500,
                autoOpen: false, draggable: false, minHeight: '400px', maxHeight: '900px', isModal: true, resizable: false, title: tittle, theme: 'ui-start', width: '100%', maxWidth: ninePorcentWidth,
                okButton: $('#btnOk'),
                initContent: function () {
                    $('#btnOk').jqxButton({ width: '100px', theme: 'ui-start' });
                    $('#btnOk').focus();
                }
            });

            eventSchedulerReport.init();
            CerrarLoading();

            $(window).on('close', function (event) { $(this).jqxWindow('destroy'); });
            $(window).jqxWindow('open');
        } else {
            if (data.error)
                desplegarMensaje(data.msg, "Advertencia");
            else
                desplegarMensaje(data.msg, "Advertencia");

            CerrarLoading();
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        CerrarLoading();

        if (jqXHR.status === 401)
            if (jqXHR.responseJSON != undefined)
                window.location.href = jqXHR.responseJSON.LogOnUrl;
            else
                window.location.href = JSON.parse(jqXHR.responseText).LogOnUrl;
        else
            desplegarMensaje(cultureName == "en-US" ? "Failed to display the report window. Contact your administrator." : 'Se produjo un error al desplegar la ventana del reporte. Contactese con su administrador.', 'Error');

        CerrarLoading();
    });
}


