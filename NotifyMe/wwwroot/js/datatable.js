$(document).ready(function () {
    var mainTable = $('#table').DataTable({
        "language": {
            emptyTable: "No data is available in the table.",
            zeroRecords: "No records were found",
            info: "Displaying _START_ to _END_ of _TOTAL_",
            infoEmpty: "No records available",
            infoFiltered: "(_MAX_ was filtered out of the total)",
            search: "Search:",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        },
        "paging": true,
        "searching": true,
        "ordering": true,
        "info": false,
        "pagingType": "simple", 
        "pageLength": 15,
    });
    // Apply the search input filter to the DataTable
    $('#searchTable').on('keyup', function () {
        mainTable.search(this.value).draw();
    });
});
