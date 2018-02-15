$(document).ready(function () {

    var ordersToPrint = [];

    var hideEmptyTables = function () {
        $(".script-tables").each(function () {
            if ($(this).children("tbody").children("tr:visible").length == 0) {
                $(this).hide();
            }
        });
    }

    var hidePrintedScripts = function () {
        // hide all the rows that have been collected
        $(".script-tables tr[data-printed='true']").hide();

        hideEmptyTables();


    }

    var MakeDropdown = function (selector, list, action, classname) {
        $.each(list, function (script) {
            $(selector).append(
                $("<li>").append(
                    $("<a>").attr("href", "#").html(this.name).append(
                        $("<span>").addClass("label pull-right " + classname).html(this.items)
                    ).append(
                        $("<span>").addClass("pull-right").html("items&nbsp;")
                        )
                        .after($("<div>").addClass("clearfix"))
                        .after($("<hr />"))
                )
            );
        });
        if (list.length > 0)
            $(selector).append($("<li>").append($("<div>").addClass("drop-foot").append($("<a>").attr("href", action).html("View All"))));
    }

    var GetOrderTotals = function () {
        $.ajax({
            type: "POST",
            url: "Orders/GetOrderTotals", 
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                $(".script-requested").html(response.ScriptRequested.length);
                MakeDropdown(".script-requested-list", response.ScriptRequested, "@Url.Action("Collections", "Orders")", "label-warning");

                $(".script-ordered").html(response.ScriptOrdered.length);
                MakeDropdown(".script-ordered-list", response.ScriptOrdered, "@Url.Action("Index", "Orders", new { status = 4 })", "label-default");

                $(".collected").html(response.Collected.length);
                MakeDropdown(".collected-list", response.Collected, "@Url.Action("Index", "Orders", new { status = 5 })", "label-success");

                $(".not-collected").html(response.NotCollected.length);
                MakeDropdown(".not-collected-list", response.NotCollected, "@Url.Action("Index", "Orders", new { status = 6 })", "label-info");

                $(".missing-items").html(response.MissingItems.length);
                MakeDropdown(".missing-items-list", response.MissingItems, "@Url.Action("Index", "Orders", new { status = 99 })", "label-important");

            },
            error: function (response) {
                $(".script-requested").html("#");
                $(".script-ordered").html("#");
                $(".collected").html("#");
                $(".not-collected").html("#");
                $(".missing-items").html("#");
            }
        });
    }

    var processOrder = function (postData) {
        $.ajax({
            type: "POST",
            url: "Orders/Process",
            data: JSON.stringify(postData),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            traditional: true,
            success: function (response) {
                if (response.status == 1) {
                    return true;
                } else {
                    alert(response.msg);
                    return false;
                }
            },
            error: function (response) {
                alert("Unknown error!");
                return false;
            }
        });
    }


    var addNote = function (id, note) {
        $.ajax({
            type: "POST",
            url: "Orders/AddNote",
            data: '{ orderId : "' + id + '", noteText : "' + note + '" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            traditional: true,
            success: function (response) {
                if (response.status == 1) {
                    return true;
                } else {
                    alert(response.msg);
                    return false;
                }
            },
            error: function (response) {
                alert("Unknown error!");
                return false;
            }
        });
    }

    $(".select-all").click(function (e) {
        e.preventDefault();
        var practiceid = $(this).attr("data-practiceid");
        $("input[data-practiceid='" + practiceid + "']").attr("checked", "checked");
        $(".script-tables tbody tr[data-practiceid='" + practiceid + "']").removeClass("no-print").addClass("break-after");;
        $("#btnPrint").removeAttr("disabled");
    });

    $(".print-checkbox").click(function (e) {
        // remove class from table row when checkbox is checked
        var orderid = $(this).attr("data-orderid");
        if ($(this).is(':checked')) {
            $(".script-tables tbody tr[data-orderid='" + orderid + "']").removeClass("no-print");
            $(".script-tables thead tr[data-orderid='" + orderid + "']").removeClass("no-print");
        } else {
            $(".script-tables tbody tr[data-orderid='" + orderid + "']").addClass("no-print");
        }

        //hide the whole table if there are no rows to print
        $(".script-tables:visible").each(function () {
            if ($(this).children("tbody").children("tr:not(.no-print)").length > 0) {
                $(this).removeClass("no-print");
                $(this).children("thead").children("tr").removeClass("no-print");
                $(this).addClass("break-after");
            } else {
                $(this).addClass("no-print");
                $(this).children("thead").children("tr").addClass("no-print");
                $(this).removeClass("break-after");
            }
        });



        // dissble the print button if no rows are selected
        if ($(".print-checkbox:visible:checked").length > 0) {
            $("#btnPrint").removeAttr("disabled");

        } else {
            $("#btnPrint").prop("disabled", true);
        }

    });

    $("#btnPrint").click(function (e) {

        $(".print-checkbox").each(function () {
            var orderid = $(this).attr("data-orderid");
            if ($(this).is(':checked')) {
                ordersToPrint.push(orderid);
                $("#frm-print").append($("<input>").attr({ 'type': 'hidden', 'name': 'ordersToPrint' }).val(orderid));
            }
        });

        // remove trailing page break
        //$(".script-tables:visible:not(.no-print)").last().removeClass("break-after");

        var postData = new Object();
        postData.ids = ordersToPrint;
        postData.status = 4;
        /*
        $.ajax({
            type: "POST",
            url: "@Url.Action("Process", "Orders")",
            data: JSON.stringify(postData),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.status == 1) {
                    //window.print();
                    window.location.reload();
                } else {
                    alert(response.msg);
                }
            },
            error: function (response) {
                alert("Unknown error!");
            }
        });*/
    });

    $(".orderline-checkbox").click(function () {
        // get the order id of the check box
        var orderid = $(this).data("orderid");
        // if any of the items on the script are ticked enable the button
        var numChecked = $(".orderline-checkbox[data-orderid='" + orderid + "']:checked").length;
        if (numChecked > 0) {
            $("button[data-orderid='" + orderid + "']").removeAttr("disabled");
        } else {
            $("button[data-orderid='" + orderid + "']").attr("disabled", "disabled");
        }
    });





    var orderid;
    $("#ProcessModal .btn-default").click(function () {

    });

    $("#ProcessModal .btn-primary").click(function () {

        addNote(orderid, $("#noteText").val());

        var postData = new Object();
        postData.ids = [orderid];
        postData.status = 99;

        if (processOrder(postData)) {
            $(".table tbody tr[data-orderid='" + orderid + "']").remove();
        }

        $('#noteText').val("");
        $("#ProcessModal").modal('hide');
    });


    $(".process-order-button").click(function () {
        orderid = $(this).data("orderid");
        var numChecked = $(".orderline-checkbox[data-orderid='" + orderid + "']:checked").length;
        var numTotal = $(".orderline-checkbox[data-orderid='" + orderid + "']").length;

        if (numChecked < numTotal) {
            var missingItemsString = "";
            var missingItems = $(".orderline-checkbox[data-orderid='" + orderid + "']:not(:checked)");
            $.each(missingItems, function (item) {
                missingItemsString += $(this).data("drug") + "\n";
            });

            $('#ProcessModal').modal('show');
            $('#noteText').focus();
            $('#noteText').val("The follow items are missing:\n" + missingItemsString + "\nBecause:");


        } else {
            var postData = new Object();
            postData.ids = [orderid];
            postData.status = 8;
            if (processOrder(postData)) {
                $(".table tr[data-orderid='" + orderid + "']").remove();
            }
        }

    });

    $(".delete-order-button").click(function () {
        var orderid = $(this).data("orderid");
        var postData = new Object();
        postData.ids = [orderid];
        postData.status = 9;
        if (processOrder(postData)) {
            $(".table tbody tr[data-orderid='" + orderid + "']").remove();
        }
    });

    $(".recollect-order-button").click(function () {
        var orderid = $(this).data("orderid");
        var postData = new Object();
        postData.ids = [orderid];
        postData.status = 4;
        if (processOrder(postData)) {
            $(".table tbody tr[data-orderid='" + orderid + "']").remove();
        }
    });

    /* Bootstrap Switch */
    $(".make-switch input").bootstrapSwitch();

    $('input[name="printed-scripts"]').on('switch-change', function (event, state) {
        if ($(this).is(":checked")) {
            $("#tbl-collect-scripts tr").show();
        } else {
            hidePrintedScripts();
        }
    });


    hidePrintedScripts();

    GetOrderTotals();

});
