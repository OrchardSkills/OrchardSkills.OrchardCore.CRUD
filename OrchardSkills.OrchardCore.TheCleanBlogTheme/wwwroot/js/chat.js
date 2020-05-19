"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");


    var h3 = document.createElement("h3");
    h3.classList.add("chat-log__author");
    h3.textContent = user + " says ";

    var div = document.createElement("div");
    div.classList.add("chat-log__message")
    var encodedMsg = msg;
    div.textContent = encodedMsg

    var li = document.createElement("li");
    li.classList.add("chat-log__item")
    li.appendChild(h3)
    li.appendChild(div)
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});