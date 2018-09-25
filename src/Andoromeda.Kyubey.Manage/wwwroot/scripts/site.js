var replaceInnerText = '![Upload](Uploading...)';
var replaceText = '\r\n' + replaceInnerText + '\r\n';

function DropEnable() {
    $('.markdown-textbox').unbind().each(function () {
        var editor = $(this);
        if (editor[0].smde == undefined) {
            var smde = new SimpleMDE({
                element: editor[0],
                spellChecker: false,
                status: false
            });
            editor[0].smde = smde;
            var begin_pos, end_pos;
            $(this).parent().children().unbind().dragDropOrPaste(function () {
                begin_pos = smde.codemirror.getCursor();
                smde.codemirror.setSelection(begin_pos, begin_pos);
                smde.codemirror.replaceSelection(replaceText);
                begin_pos.line++;
                end_pos = { line: begin_pos.line, ch: begin_pos.ch + replaceInnerText.length };
            },
                function (result) {
                    smde.codemirror.setSelection(begin_pos, end_pos);
                    smde.codemirror.replaceSelection('![' + result.FileName + '](/file/download/' + result.Id + ')');
                });
        }
    });
}

function uploadFile() {
    var formData = new FormData($('#frmAjaxUpload')[0]);
    var editor = $('.markdown-textbox');
    var smde = editor[0].smde;

    var begin_pos, end_pos;

    begin_pos = smde.codemirror.getCursor();
    smde.codemirror.setSelection(begin_pos, begin_pos);
    smde.codemirror.replaceSelection(replaceText);
    begin_pos.line++;
    end_pos = { line: begin_pos.line, ch: begin_pos.ch + replaceInnerText.length };

    $.ajax({
        url: '/file/upload',
        type: 'POST',
        data: formData,
        dataType: 'json',
        async: false,
        cache: false,
        contentType: false,
        processData: false,
        success: function (result) {
            smde.codemirror.setSelection(begin_pos, end_pos);
            smde.codemirror.replaceSelection('![' + result.FileName + '](/file/download/' + result.Id + ')');
        },
        error: function (returndata) {
        }
    });
}

DropEnable();

function renderArgsControl() {
    var type = $('#lstCurveType').val();
    if (!type) {
        $('#curve-step2').hide();
        return;
    } else {
        $('#curve-step2').show();
    }

    $('.preview-buttons').hide();
    if (functions[type]['PriceSupplyFunction']) {
        $('#btn-preview-price-supply').show();
    }
    if (functions[type]['PriceBalanceFunction']) {
        $('#btn-preview-price-balance').show();
    }
    if (functions[type]['BalanceSupplyFunction']) {
        $('#btn-preview-balance-supply').show();
    }
    if (functions[type]['SupplyBalanceFunction']) {
        $('#btn-preview-supply-balance').show();
    }

    if (!args[type]) {
        return;
    }

    var html = '';
    for (var i = 0; i < args[type].length; i++) {
        if (type !== currentCurve) {
            html += `<div class="form-group"><label>${args[type][i].Id}</label> <small class="hint">${args[type][i].Name}</small><br /><input type="text" class="form-control args" /></div>`;
        } else {
            html += `<div class="form-group"><label>${args[type][i].Id}</label> <small class="hint">${args[type][i].Name}</small><br /><input type="text" class="form-control args" value="${values[i]}" /></div>`;
        }
        $('#dynamic-args').html(html);
    }
}

if ($('#lstCurveType').length > 0) {
    renderArgsControl();
    render_chart();
    $('#lstCurveType').change(renderArgsControl);
}

function render_chart(chart_type) {
    if (chart_type) {
        var type = $('#lstCurveType').val();
        var vals = $('.args');
        for (var i = 0; i < vals.length; i++) {
            eval(`var ${args[type][i].Id} = ${$(vals[i]).val()}`);
        }

        eval(`var fn = \`${functions[type][chart_type]}\`;`);

        functionPlot({
            target: document.querySelector("#chart"),
            xAxis: { domain: [0, $('#x-max').val()] },
            yAxis: { domain: [0, $('#y-max').val()] },
            tip: {
                renderer: function () { }
            },
            grid: true,
            data: [
                {
                    fn: fn
                }
            ]
        });
    } else {
        functionPlot({
            target: document.querySelector("#chart"),
            xAxis: { domain: [0, 1] },
            yAxis: { domain: [0, 1] },
            tip: {
                renderer: function () { }
            },
            grid: true,
            data: [
            ]
        });
    }
}

if ($('#frmCurve').length > 0) {
    $('#frmCurve').submit(function () {
        var vals = [];
        var dom = $('.args');
        for (var i = 0; i < dom.length; i++) {
            vals.push(parseFloat($(dom[i]).val()));
        }
        $('#hidArgs').val(JSON.stringify(vals));
    });
}

if ($('#frmBancorBasic').length > 0) {
    var editor = ace.edit("tradeJavascript");
    $('#tradeJavascript')[0].editor = editor;
    editor.setTheme("ace/theme/twilight");
    editor.session.setMode('ace/mode/javascript');
    editor.setOptions({
        enableBasicAutocompletion: true,
        enableSnippets: true
    });

    $('#frmBancorBasic').submit(function () {
        $('#hidTradeJavascript').val($('#tradeJavascript')[0].editor.getValue());
    });
}

if ($('#frmBancorPrice').length > 0) {
    var editor = ace.edit("buyPriceJavascript");
    $('#buyPriceJavascript')[0].editor = editor;
    editor.setTheme("ace/theme/twilight");
    editor.session.setMode('ace/mode/javascript');
    editor.setOptions({
        enableBasicAutocompletion: true,
        enableSnippets: true
    });

    $('#frmBancorPrice').submit(function () {
        $('#hidBuyPriceJavascript').val($('#buyPriceJavascript')[0].editor.getValue());
    });
    
    var editor2 = ace.edit("sellPriceJavascript");
    $('#sellPriceJavascript')[0].editor = editor2;
    editor2.setTheme("ace/theme/twilight");
    editor2.session.setMode('ace/mode/javascript');
    editor2.setOptions({
        enableBasicAutocompletion: true,
        enableSnippets: true
    });

    $('#frmBancorPrice').submit(function () {
        $('#hidSellPriceJavascript').val($('#sellPriceJavascript')[0].editor.getValue());
    });
}