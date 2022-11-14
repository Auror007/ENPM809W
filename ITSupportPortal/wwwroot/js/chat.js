"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${message}`;
});

connection.start().then(async function () {
    document.getElementById("sendButton").disabled = false;
    //console.log("Connection Established!");

    //Add the connection to case group
    var groupName = document.getElementById("groupName").value;
    //console.log(groupName);
    await connection.invoke("AddToGroup", groupName);
    
}).catch(function (err) {
    return console.error(err.toString());
});


document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var groupName = document.getElementById("groupName").value;
    connection.invoke("SendMessage", user, message,groupName).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("messageInput").value = null;
    event.preventDefault();
});
