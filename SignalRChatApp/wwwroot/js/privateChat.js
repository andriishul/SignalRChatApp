"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().catch(err => {
    console.error("Connection failed:", err);
    setTimeout(() => connection.start(), 5000);
});

connection.on("ReceivePrivateMessage", (username, content) => {
    const messageBox = document.getElementById("messageBox");
    const div = document.createElement("div");
    div.className = "message";

    div.innerHTML = `<strong>${username}:</strong> ${content.text || ""}` +
        (content.attachment ? `<br><a href="${content.attachment.url}" target="_blank">${content.attachment.fileName}</a>` : "");

    messageBox.appendChild(div);
    messageBox.scrollTop = messageBox.scrollHeight;
});

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

document.getElementById("sendMessage").addEventListener("click", async () => {
    const email = document.getElementById("email").value.trim();
    const messageText = document.getElementById("message").value.trim();
    const fileInput = document.getElementById("fileInput");

    if (!email || (!messageText && fileInput.files.length === 0)) {
        alert("Please provide an email and a message or file.");
        return;
    }

    let attachment = null;
    if (fileInput.files.length > 0) {
        attachment = await uploadFile(fileInput.files[0]);
        if (!attachment) {
            alert("File upload failed. Message not sent.");
            return;
        }
    }

    const content = { text: messageText, attachment };

    try {
        await connection.invoke("SendTo", email, content);
        document.getElementById("message").value = "";
        fileInput.value = "";
    } catch (err) {
        console.error("Send failed:", err);
    }
});
