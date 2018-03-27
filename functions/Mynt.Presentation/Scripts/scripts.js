var debug = true;
var formsaved = false;
var loadingbarInterval;
var modifiedContentInDialog = false;
var activeDialogId = null;
var reloadAfterSave = true;

function register() {
    if (debug) console.log("register() called");
    //first register all components, because we trigger some of their events later
    registerComponents();

    //initially load delayed panel
    $(".refreshbutton.delayed").trigger("click");

    //After refresh scroll back to old position
    $(window).unbind("scroll").bind("scroll", function () {
        if ($(window).scrollTop() > 118) {
            $("#topbar").addClass("fixed");
            $("#error-content").addClass("fixed-topbar");
        }
        else {
            $("#topbar").removeClass("fixed");
            $("#error-content").removeClass("fixed-topbar");
        }
    });
    if ($("#caching").is(":checked")) {
        cache($("ul.nav.nav-tabs li a"));
    }
}

//for later async partial stuff checkout repo from https://github.com/twixwix/AsyncPartialExample
//function registerPartials() {
//    $(".partialContents").each(function (index, item) {
//        var url = site.baseUrl + $(item).data("url");
//        if (url && url.length > 0) {
//            $(item).load(url, function () {
//                registerComponents();
//            });
//        }
//    });
//}

//waits with calling registerComponenets till a specific item is loaded
function registerComponentsWaitFor(id) {
    if (debug) console.log("registerComponentsWaitFor(id) called: " + id);
    if ($("#" + id).length > 0 && $("#" + id).html() !== "") {
        if ($("#" + id).html().indexOf("Popup wird geladen...") > -1) {
            if (debug) console.log("object found, but not ready!");
            setTimeout(function () {
                registerComponentsWaitFor(id);
            }, 200);
        } else {
            if (debug) console.log("object found!");
            registerComponents();
        }
    }
    else {
        if (debug) console.log("waiting for object to be created");
        setTimeout(function () {
            registerComponentsWaitFor(id);
        }, 200);
    }
}


function registerComponents() {
    if (debug) console.log("registerComponents() called");
    //register all componets inside the crud area

    
    //Jump back to position after Postback in razor engine
    $(function () {
        var top = parseInt($.cookie("top"));
        if (top) $(document).scrollTop(top);
        $(document).scroll(function () {
            var top = $(document).scrollTop();
            $.cookie("top", top);
        });
    });

   

    // Returns nice log messages
    $(".log-content").has(".alert").each(function () {
        $(".log-content").hide();
        var $child = $(".log-content").children();
        var $state = $child.attr("class").split("-");

        $(".submit").addClass("fadeIn animated");

        // create message box bottom right
        var $text = $(".alert").html();

        var $div = $("<div></div>");
        $div.attr("id", "successmsg");
        $div.attr("class", "logmessage fadeInRight animated");
        var $glyph = "";

        switch ($state[1]) {
            case "success":
                $div.addClass("success");
                $glyph = "glyphicon-ok";
                break;
            case "danger":
                $div.addClass("danger");
                $glyph = "glyphicon-remove";
                break;
            case "warning":
                $div.addClass("warning");
                $glyph = "glyphicon-alert";
                break;
            case "info":
                $div.addClass("info");
                $glyph = "glyphicon-info-sign";
                break;
            default:
        }

        $(".submit").html("<span class=\"glyphicon " + $glyph + "\" style=\"min-width:100px\"></span>");
        var $span = $("<span class=\"glyphicon " + $glyph + "\"></span> " + $text);

        $div.html($span);
        $div.appendTo("body");

        $("#successmsg").delay(3000).fadeOut("slow");

    });
    
    //AJAX für ActionButtons
    $(".actionbutton").unbind("click").bind("click", function (e) {
        if (debug) console.log("'.actionbutton' " + e.type + " event fired");
        e.preventDefault();
        var link = $(this).attr("href") || $(this).data("href");
        if (formsaved === undefined || !formsaved)
            formsaved = $(this).data("refresh");
        var refreshids = [];
        var counter = 1;
        while ($(this).data("refreshid" + counter)) {
            refreshids.push($(this).data("refreshid" + counter));
            counter++;
        }
        $.get(link, function (data) {
            if (data !== "False") {
                if (count(refreshids) > 0) {
                    for (var i = 0; i < refreshids.length; i++) {
                        $("#" + refreshids[i]).trigger("click");
                    }
                }
            }
        }).fail(function (e) {
            $("#error-content").html($("#error-content").html() + "<div class=\"alert alert-danger\" role=\"alert\"><p>" + e.status + ": " + e.statusText + ". Request for " + link + " Failed</p></div>");
        });
    });
    
    
    //Creates the dialog boxes
    $('.dialogbutton').unbind('click').bind("click", function (e) {
        if (debug) console.log("'.dialogbutton' " + e.type + " event fired");
        e.preventDefault();

        var link = $(this).attr("href") || $(this).data("href");

        //Define Titles & Color
        var dialogtitle = $(this).data("dialog-title");
        var dialogkind = $(this).data("dialog-type");
        var dialogtype = BootstrapDialog.TYPE_DEFAULT;
        if (dialogkind === "success") {
            dialogtype = BootstrapDialog.TYPE_SUCCESS;
        }
        else if (dialogkind === "info") {
            dialogtype = BootstrapDialog.TYPE_INFO;
        }
        else if (dialogkind === "primary") {
            dialogtype = BootstrapDialog.TYPE_PRIMARY;
        }
        else if (dialogkind === "danger") {
            dialogtype = BootstrapDialog.TYPE_DANGER;
        }
        else if (dialogkind === "warning") {
            dialogtype = BootstrapDialog.TYPE_WARNING;
        }

        var dialogsiz = $(this).data("dialog-size");
        var dialogsize = BootstrapDialog.SIZE_NORMAL;
        if (dialogsiz === "large") {
            dialogsize = BootstrapDialog.SIZE_LARGE;
        }
        else if (dialogsiz === "small") {
            dialogsize = BootstrapDialog.SIZE_SMALL;
        }
        else if (dialogsiz === "wide") {
            dialogsize = BootstrapDialog.SIZE_WIDE;
        }

        //Create Buttons by using the properties of the linked button. No Maximal number of button, because var idbutton is defined globally.
        //to enable feature for unlimited number of buttons, we must store the id's in the dialog object
        var dialogbuttons = [];
        var counter = 0;
        var nextIdButton = $(this).data("dialog-idbutton" + counter);
        var nextIdButtonClass = $(this).data("dialog-idbutton" + counter + "-type");
        var nextIdButtonGlyphicon = $(this).data("dialog-idbutton" + counter + "-glyphicon");

        while (nextIdButton !== null && nextIdButton !== "") {
            switch (nextIdButton) {
                case "submit":
                    dialogbuttons.push({
                        label: "Speichern",
                        icon: "glyphicon glyphicon-floppy-disk",
                        cssClass: "btn-success btn-md submit",
                        action: function () {
                            document.getElementById("Save").click();
                        }
                    });
                    break;
                case "submitandclose":
                    dialogbuttons.push({
                        label: "Speichern und schliessen",
                        icon: "glyphicon glyphicon-floppy-disk submitandclose",
                        cssClass: "btn-success btn-md",
                        action: function (dialogItself) {
                            document.getElementById("Save").click();
                            setTimeout(function () {
                                dialogItself.close();
                            }, 1000);
                        }
                    });
                    break;
                default:
                    var matches = nextIdButton.match(/\d+/g);
                    if (matches !== null) {
                        var idbutton = $('#' + nextIdButton);
                        if (idbutton.length > 0) {
                            var buttonlabel = idbutton.text();

                            //Class for Glyphicon
                            var buttonglyphicon = "";
                            if (nextIdButtonGlyphicon !== undefined) {
                                if (nextIdButtonGlyphicon.length > 0) {
                                    buttonglyphicon = nextIdButtonGlyphicon;
                                }
                                else {
                                    buttonglyphicon = "glyphicon glyphicon-th-list";
                                }
                            }
                            //Class for button
                            var buttonclass = "";
                            var classes = idbutton.attr('class').split(' ');
                            for (var i = 0; i < classes.length; i++) {
                                if (classes[i].indexOf("btn-") === 0) {
                                    buttonclass = classes[i];
                                    break;
                                }
                            }
                            if (nextIdButtonClass !== undefined) {
                                if (nextIdButtonClass.length > 0) {
                                    buttonclass = nextIdButtonClass;
                                }
                            }

                            var toClickId = idbutton.attr("id");
                            dialogbuttons.push({
                                label: buttonlabel,
                                icon: buttonglyphicon,
                                cssClass: buttonclass,
                                relinkClick: toClickId,
                                action: function (dialogItself) {
                                    var $button = this;
                                    dialogItself.close();

                                    // I'm sorry for this timeout :)
                                    // we need this here so the correct dialog is registered as parent, before opening another one. Otherwise very irritting bugs may happen
                                    setTimeout(function () {
                                        $("#" + $button.data("relink-click")).trigger("click");
                                    }, 500);
                                }
                            });
                        }
                        else if (debug)
                            console.log("button not found: " + '#' + idbutton);

                    }
                    else {
                        if (nextIdButton.indexOf("_") > -1) {
                            var toClickAction = nextIdButton.split("_")[0];
                            dialogbuttons.push({
                                label: 'Speichern und neu',
                                icon: 'glyphicon glyphicon-floppy-disk',
                                cssClass: 'btn-success btn-md',
                                relinkClick: toClickAction,
                                action: function (dialogItself) {
                                    var $button = this;
                                    document.getElementById("Save").click();
                                    setTimeout(function () {
                                        //Set not to reload true
                                        reloadAfterSave = false;
                                        dialogItself.close();

                                        // I'm sorry for this timeout :)
                                        // we need this here so the correct dialog is registered as parent, before opening another one. Otherwise very irritting bugs may happen
                                        setTimeout(function () {
                                            $("#" + $button.data("relink-click")).trigger("click");
                                        }, 500);
                                    }, 1000);
                                }
                            });
                        }
                    }
                    break;
            }
            nextIdButton = $(this).data("dialog-idbutton" + ++counter);
        }

        //Close Button
        dialogbuttons.push({
            label: 'Schliessen',
            action: function (dialogItself) {
                dialogItself.close();
            }
        });

        //Show BootstrapDialog
        BootstrapDialog.show({
            title: dialogtitle,
            type: dialogtype,
            size: dialogsize,
            //close by clicking on the side
            closeByBackdrop: true,
            //you may want to diable this, but it is awesome
            draggable: true,
            message: function (dialog) {
                var link = dialog.getData('pageToLoad');

                var content = $('<div data-refresh-url="' + link + '">wird geladen...</div>');
                var id = "dialog_0";
                if (activeDialogId !== null) {
                    var split = activeDialogId.split("_");
                    id = "dialog_" + ++split[1];
                    content.attr("data-parent-dialog-id", activeDialogId);
                }
                content.attr("id", id);
                activeDialogId = id;

                StartLoadingBar();
                $.ajax({
                    url: link,
                    cache: false,
                    success: function (data) {
                        EndLoadingBar();
                        if (data !== false) {
                            var $dialog = $("#" + id);
                            if ($dialog.length > 0) {
                                $dialog.html(data);
                                registerComponents();
                            }
                            else {
                                var waitInterval = setInterval(function () {
                                    var $dialog = $("#" + id);
                                    if ($dialog.length > 0) {
                                        clearInterval(waitInterval);
                                        $dialog.html(data);
                                        registerComponents();
                                    }
                                }, 50);
                            }
                        }
                    },
                    fail: function (e) {
                        alert(e.status + ': ' + e.statusText + '. Request for ' + link + ' failed');
                        EndLoadingBar();
                    }
                });

                return content;
            },
            onhidden: function (dialogRef) {
                var $dialog = $("#" + activeDialogId);
                var parentid = $dialog.data("parent-dialog-id");
                if (parentid !== "")
                    activeDialogId = parentid;
                else
                    activeDialogId = null;

                if (modifiedContentInDialog) {
                    if (activeDialogId !== null) {
                        var $parentdialog = $("#" + parentid);
                        var link = $parentdialog.data("refresh-url");

                        StartLoadingBar();
                        $.ajax({
                            url: link,
                            cache: false,
                            success: function (data) {
                                EndLoadingBar();
                                if (data !== false) {
                                    $parentdialog.html(data);
                                    registerComponents();
                                }
                            },
                            fail: function (e) {
                                alert(e.status + ": " + e.statusText + ". Request for " + link + " failed");
                                EndLoadingBar();
                            }
                        });

                    }
                    else {
                        if (reloadAfterSave) {
                            modifiedContentInDialog = false;
                            location.reload();
                        }
                        else {
                            modifiedContentInDialog = true;
                        }
                    }
                }
            },
            data: {
                'pageToLoad': link
            },
            buttons: dialogbuttons
        });
    });

    //catches form submit event and replaces the submited form with the response recieved
    $("form:not(.no-ajax)").unbind("submit").bind("submit", function (e) {
        if (debug) console.log("'form' " + e.type + " event fired");
        e.preventDefault();
        $(this).on("submit", function () {
            return false;
        });
        var $form = $(this);
        makeLoadingButton($("input[type=\"submit\"]", this));
        StartPercentageLoadingBar();

        $.ajax({
            type: $(this).attr("method"),
            url: $(this).attr("action"),
            data: new FormData($(this)[0]),
            async: true,
            success: function (data) {
                if ($form.attr("data-replace-id")) {
                    $("#" + $form.data("replace-id")).replaceWith(data);
                } else {
                    $form.parent().replaceWith(data);
                }
                registerComponents();
                formsaved = true;
                modifiedContentInDialog = true;
                EndLoadingBar();
            },
            cache: false,
            contentType: false,
            processData: false,
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                //Upload progress
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        UpdatePercentageLodingBar(evt.loaded / evt.total * 100);
                    }
                }, false);
                //Download progress
                xhr.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        UpdatePercentageLodingBar(evt.loaded / evt.total * 100);
                    }
                }, false);
                return xhr;
            }
        }).fail(function (e) {
            EndLoadingBar();
            $("#error-content").html($("#error-content").html() + "<div class=\"alert alert-danger\" role=\"alert\"><p>" + e.status + ": " + e.statusText + ". Submit of form " + $(this).id + " Failed</p></div>");
        });

        return false; // avoid to execute the actual submit of the form.
    });

    // Add Style for Textboxes, Selects
    $("input").addClass("form-control");
    $("select").addClass("form-control");
    $("textarea").addClass("form-control");
    
    //styling of checked & unchecked checkboxes & radio buttons
    $("input[type=checkbox], input[type=radio]").each(function () {
        if ($(this).is(":checked") === true)
            $(this).parent().addClass("active");
        else
            $(this).parent().removeClass("active");
    });


    $("input[type=checkbox], input[type=radio]").unbind("click").bind("click", function (e) {
        if (debug) console.log("'input[type=checkbox], input[type=radio]' " + e.type + " event fired");
        if ($(this).is(":checked"))
            $(this).parent().addClass("active");
        else
            $(this).parent().removeClass("active");

        $("input[type=radio]").each(function () {
            if ($(this).is(":checked"))
                $(this).parent().addClass("active");
            else
                $(this).parent().removeClass("active");
        });



        $("input[type=radio].enablebutton").each(function () {
            var counter = 0;
            if (!$(this).is(":checked"))
                while ($(this).data("id" + counter) !== null && $(this).data("id" + counter) !== "") {
                    if ($("#" + $(this).data("id" + counter)).is("div"))
                        $("#" + $(this).data("id" + counter++)).hide();
                    else {
                        $("#" + $(this).data("id" + counter++)).attr("disabled", true);
                    }
                }
            else
                while ($(this).data("id" + counter) !== null && $(this).data("id" + counter) !== "") {
                    if ($("#" + $(this).data("id" + counter)).is("div"))
                        $("#" + $(this).data("id" + counter++)).show();
                    else {
                        $("#" + $(this).data("id" + counter++)).removeAttr("disabled");
                    }
                }
        });
    });

    $("textarea.expandonclick").unbind("click").bind("click", function (e) {
        var textarea = $(this);
        textarea.addClass("form-control");
        textarea.css("height", "200px");
        setTimeout(function () {
            textarea.addClass("expanded");
            textarea.removeClass("expandonclick");
            optimizeModals();
            $("textarea.expanded").keyup(function () {
                while ($(this).outerHeight() < this.scrollHeight + parseFloat($(this).css("borderTopWidth")) + parseFloat($(this).css("borderBottomWidth"))) {
                    $(this).height($(this).height() + 1);
                }
                ;
                optimizeModals();
            });
        }, 500);
    });

    $("input[type=radio].enablebutton").each(function () {
        var counter = 0;
        if (!$(this).is(":checked"))
            while ($(this).data("id" + counter) !== null && $(this).data("id" + counter) !== "")
                $("#" + $(this).data("id" + counter++)).attr("disabled", true);
        else
            while ($(this).data("id" + counter) !== null && $(this).data("id" + counter) !== "")
                $("#" + $(this).data("id" + counter++)).removeAttr("disabled");
    });

    //initialize selectpickers
    $(".selectpicker").chosen();
    
    //do lazy loading of certain buttons (like refreshbuttons)
    $(".refreshbutton.lazyload").each(function () {
        $(this).removeClass("lazyload");
        $(this).addClass("lazyloaded");
        $(this).trigger("click");
    });

    //correct user input
    $("input.lowercase").unbind("keyup").bind("keyup", function () {
        $(this).val(($(this).val()).toLowerCase());
    });

    //correct user input
    $("input.uppercase").unbind("keyup").bind("keyup", function () {
        $(this).val(($(this).val()).toUpperCase());
    });
    
    optimizeModals();
}

function optimizeModals() {
    //optimization for Modals
    if ($(".modal-backdrop.fade.in").length > 0 && $(".modal-dialog").length > 0) {
        var height = parseInt($(".modal-dialog").css("margin-top"), 10);
        height += parseInt($(".modal-dialog").css("margin-bottom"), 10);
        height += parseInt($(".modal-dialog").css("height"), 10);
        if (height > parseInt($(".modal-backdrop.fade.in").css("height"), 10)) {
            $(".modal-backdrop.fade.in").css("height", height + "px");
        }
    }
}

function insertAtCaret(areaId, text) {
    var txtarea = document.getElementById(areaId);
    if (!txtarea) { return; }

    var scrollPos = txtarea.scrollTop;
    var strPos = 0;
    var br = ((txtarea.selectionStart || txtarea.selectionStart === '0') ?
        "ff" : (document.selection ? "ie" : false));
    if (br === "ie") {
        txtarea.focus();
        var range = document.selection.createRange();
        range.moveStart('character', -txtarea.value.length);
        strPos = range.text.length;
    } else if (br === "ff") {
        strPos = txtarea.selectionStart;
    }

    var front = (txtarea.value).substring(0, strPos);
    var back = (txtarea.value).substring(strPos, txtarea.value.length);
    txtarea.value = front + text + back;
    strPos = strPos + text.length;
    if (br === "ie") {
        txtarea.focus();
        var ieRange = document.selection.createRange();
        ieRange.moveStart('character', -txtarea.value.length);
        ieRange.moveStart('character', strPos);
        ieRange.moveEnd('character', 0);
        ieRange.select();
    } else if (br === "ff") {
        txtarea.selectionStart = strPos;
        txtarea.selectionEnd = strPos;
        txtarea.focus();
    }

    txtarea.scrollTop = scrollPos;
}

//the next to functions display the loading bar
var loadingbarInterval;
function StartLoadingBar() {
    $("#loadingbar").css("height", "5px");
    $("#loadingbar").css("width", "30%");
    loadingbarInterval = setInterval(function () {
        var stillavailable = $(document).width() - $("#loadingbar").width();
        var factor = 1 / ($(document).width() - stillavailable);
        var newwidth = (stillavailable * factor * 100) + $("#loadingbar").width();
        $("#loadingbar").css("width", newwidth + "px");
    }, 200);
}

function StartPercentageLoadingBar() {
    $("#loadingbar").css("height", "5px");
    $("#loadingbar").css("width", "0");
}

function UpdatePercentageLodingBar(percentage) {
    $("#loadingbar").css("width", percentage + "%");
    if (percentage === 100)
        StartLoadingBar();
}

function EndLoadingBar() {
    try {
        window.clearInterval(loadingbarInterval);
    }

    $("#loadingbar").css("width", "100%");

    setTimeout(function () {
        $("#loadingbar").css("height", "0");
        $("#loadingbar").css("width", "0");
    }, 500);
}

//replaces html inside button to show a nice loading symbol
function makeLoadingButton($button) {
    $button.addClass("locked");
}

//reverses makeLoadingButton()
function reverseMakeLoadingButton($button) {
    $button.removeClass("locked");
}

//counts up till the element reached the max number defined in a data attribute
function countIt($element) {
    var realnumber = parseInt($element.html());
    var targetnumber = parseInt($element.data("end"));
    var nextcount = 1;
    if (targetnumber === realnumber) {
        if (debug) console.log("nextvalue == maxvalue " + $element.html() + "  " + $element.data("count"));
        return;
    }
    else if (targetnumber > realnumber) {
        $element.html(realnumber + 1);
        nextcount = 1000 / (targetnumber - realnumber);
    } else {
        $element.html(realnumber - 1);
        nextcount = 1000 / (realnumber - targetnumber);
    }

    if (nextcount < 1)
        nextcount = 1;
    setTimeout(function () {
        countIt($element);
    }, nextcount);
}