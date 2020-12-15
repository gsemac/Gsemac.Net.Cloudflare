function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function openAndClose(uri) {
    var handle = window.open(uri);
    sleep(5000).then(() => handle.close());
}

function heartbeat() {
    fetch("heartbeat", { method: "POST" })
        .then(response => response.text())
        .then(nextUri => {
            if (!!nextUri) {
                var li = document.createElement("li");
                var ul = document.getElementById("items");
                li.appendChild(document.createTextNode("Solving challenge for " + nextUri + "..."));
                ul.appendChild(li);
                openAndClose(nextUri);
            }
            setTimeout(heartbeat, 1500);
        })
        .catch(err => console.log(err))
}

heartbeat();