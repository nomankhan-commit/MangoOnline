
$(document).ready(function () {

    loadDataTable();

})


function loadDataTable() {

    dataTable = $('#datatable').dataTable({
        order:[[0,"desc"]],
        "ajax": { url: "/order/getall" },
        "columns": [
            { "data": "orderHeaderId", "width": "5%" },
            { "data": "email", "width": "25%" },
            { "data": "name", "width": "20%" },
            { "data": "phoneNumber", "width": "10%" },
            { "data": "status", "width": "10%" },
            { "data": "orderTotal", "width": "10%" },
            {
                data: "orderHeaderId",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">Edit</a>
                    </div>
                    `;
                }
            }
        ]
    });

}