var conn = new signalR.HubConnectionBuilder().withUrl("/updateHub").withAutomaticReconnect().build();

conn.start().then(function () {
    console.log("Connected");
}).catch(function (err) {
    console.error(err.toString());
});

conn.on('create', function (msg) {
    var myTable = document.getElementById("myTable");

    var row = document.createElement("tr");

    row.innerHTML = "<td>" + msg.time + "</td>" +
        "<td>" + msg.locationName + "</td>" +
        "<td>" + msg.latitude + "</td>" +
        "<td>" + msg.longitude + "</td>" +
        "<td>" + msg.temperature + "</td>" +
        "<td>" + msg.airPressure + "</td>" +
        "<td>" + msg.humidity + "</td>" +
        "<td>" + msg.description + "</td>";

    myTable.appendChild(row);
});
