"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

let inGroup = false;

const joinBtn = document.getElementById("joinGroupBtn");
const leaveBtn = document.getElementById("leaveGroupBtn");
const sendBtn = document.getElementById("sendGroupMessageBtn");
const groupNameInput = document.getElementById("groupName");
const messageInput = document.getElementById("messageInput");
const messageBox = document.getElementById("messageBox");
const fileInput = document.getElementById("fileInput");

function updateButtons() {
    joinBtn.style.display = inGroup ? "none" : "inline-block";
    leaveBtn.style.display = sendBtn.style.display = inGroup ? "inline-block" : "none";
}

connection.start()
    .then(updateButtons)
    .catch(err => setTimeout(() => connection.start(), 5000));

connection.on("ReceiveGroupMessage", (user, content) => {
    addMessage(`${user}: ${content.text || ""}` +
        (content.attachment ? `<br><a href="${content.attachment.url}" target="_blank">${content.attachment.fileName}</a>` : ""));
});

connection.on("ReceiveSystemNotification", (notification) => {
    addMessage(notification);
});

function addMessage(text) {
    const div = document.createElement("div");
    div.innerHTML = text;
    messageBox.appendChild(div);
    messageBox.scrollTop = messageBox.scrollHeight;
}

async function uploadFile(file) {
    const formData = new FormData();
    formData.append("file", file);

    try {
        const res = await fetch("/api/files/upload", { method: "POST", body: formData });
        if (!res.ok) throw new Error();
        const { file } = await res.json();
        return { fileName: file, url: `/uploads/${file}` };
    } catch {
        console.error("Upload failed.");
        return null;
    }
}

joinBtn.addEventListener("click", async e => {
    e.preventDefault();
    if (inGroup) return alert("Already in a group. Leave first.");

    const groupName = groupNameInput.value.trim();
    if (!groupName) return alert("Please enter a group name.");

    try {
        await connection.invoke("JoinGroup", groupName);
        inGroup = true;
        updateButtons();
    } catch (err) {
        console.error("Join failed:", err);
    }
});

leaveBtn.addEventListener("click", async e => {
    e.preventDefault();
    if (!inGroup) return alert("Not in a group.");

    const groupName = groupNameInput.value.trim();
    if (!groupName) return alert("Please enter a group name.");

    try {
        await connection.invoke("LeaveGroup", groupName);
        inGroup = false;
        updateButtons();
    } catch (err) {
        console.error("Leave failed:", err);
    }
});

sendBtn.addEventListener("click", async e => {
    e.preventDefault();
    if (!inGroup) return alert("Join a group first.");

    const groupName = groupNameInput.value.trim();
    const message = messageInput.value.trim();
    if (!groupName || (!message && fileInput.files.length === 0)) return alert("Enter group and message or select a file.");

    let attachment = null;
    if (fileInput.files.length > 0) {
        attachment = await uploadFile(fileInput.files[0]);
        if (!attachment) {
            alert("File upload failed. Message not sent.");
            return;
        }
    }

    const content = { text: message, attachment };

    try {
        await connection.invoke("SendToGroup", groupName, content);
        messageInput.value = '';
        fileInput.value = '';
    } catch (err) {
        console.error("Send failed:", err);
    }
});
