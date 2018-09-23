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