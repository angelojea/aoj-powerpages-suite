/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

    var ns = XRM.namespace('editable.Attribute.handlers');
    var $ = XRM.jQuery;
    var yuiSkinClass = XRM.yuiSkinClass;

    ns.text = function (attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback) {
        // Build the DOM necessary to support our UI.
        var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
        var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog').appendTo(yuiContainer);

        function completeEdit(dialog) {
            dialog.cancel();
            yuiContainer.remove();

            if ($.isFunction(editCompleteCallback)) {
                editCompleteCallback();
            }
        }

        function handleCancel(dialog) {
            completeEdit(dialog);
        }

        function handleSave(dialog) {
            var dialogInput = $('.xrm-text', dialog.body);
            var dialogInputValue = dialogInput.val();
            var dialogFooter = $(dialog.footer);

            // If the attribute value has been changed, persist the new value.
            if (dialogInputValue != attributeValue) {
                dialogFooter.hide();
                dialogInput.hide();
                dialogContainer.addClass('xrm-editable-wait');
                XRM.data.services.putAttribute(entityServiceUri, attributeName, dialogInputValue, {
                    success: function () {
                        $('.xrm-attribute-value', attributeContainer).html(dialogInputValue);
                        completeEdit(dialog);
                    },
                    error: function (xhr) {
                        dialogContainer.removeClass('xrm-editable-wait');
                        dialogFooter.show();
                        dialogInput.show();
                        XRM.ui.showDataServiceError(xhr);
                    }
                });
            }
            // Otherwise, just dismiss the edit dialog without doing anything.
            else {
                completeEdit(dialog);
            }
        }

        // Create our modal editing dialog.
        var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
            visible: false,
            constraintoviewport: true,
            zindex: XRM.zindex,
            xy: YAHOO.util.Dom.getXY(attributeContainer.get(0)),
            buttons: [
                {
                    text: XRM.localize('editable.save.label'), handler: function () {
                        handleSave(this)
                    }, isDefault: true
                },
                {
                    text: XRM.localize('editable.cancel.label'), handler: function () {
                        handleCancel(this)
                    }
                }]
        });

        dialog.setHeader('Edit ' + (attributeDisplayName || ''));
        dialog.setBody(' ');

        $('<input />').attr('type', 'text').addClass('xrm-text').val(attributeValue || '').appendTo(dialog.body);

        // Add ctrl+s shortcut for saving content.
        $('.xrm-text', dialog.body).keypress(function (e) {
            if (!(e.which == ('s').charCodeAt(0) && e.ctrlKey)) {
                return true;
            }
            handleSave(dialog);
            return false;
        });

        dialog.render();
        dialog.show();

        XRM.ui.registerOverlay(dialog);
        dialog.focus();

        $('.xrm-text', dialog.body).focus();
    }

});
