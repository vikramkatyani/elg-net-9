// Call the dataTables jQuery plugin
$(document).ready(function() {
  $('#dataTable').DataTable({
    "ordering": false,
    "searching":false,
    "pagesize":false
  });
  $('[data-toggle="tooltip"]').tooltip();
});
