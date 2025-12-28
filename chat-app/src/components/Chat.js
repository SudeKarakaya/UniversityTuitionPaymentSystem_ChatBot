import { useState } from "react";

function Chat() {
    const [messages, setMessages] = useState([]);
    const [text, setText] = useState("");

    const sendMessage = async () => {
        if (!text.trim()) return;

        const userMessage = { sender: "user", text };
        setMessages([...messages, userMessage]);

        // Şimdilik fake cevap, sonra Firestore-LLM bağlayacağız
        const botMessage = { sender: "bot", text: "Mesaj alındı 👍" };
        setMessages(m => [...m, userMessage, botMessage]);

        setText("");
    };

    return (
        <div style={styles.container}>
            <div style={styles.chatBox}>
                {messages.map((m, i) => (
                    <div
                        key={i}
                        style={{
                            ...styles.message,
                            alignSelf: m.sender === "user" ? "flex-end" : "flex-start",
                            background: m.sender === "user" ? "#4f46e5" : "#e5e7eb",
                            color: m.sender === "user" ? "white" : "black"
                        }}
                    >
                        {m.text}
                    </div>
                ))}
            </div>

            <div style={styles.inputRow}>
                <input
                    value={text}
                    placeholder="Write message..."
                    onChange={e => setText(e.target.value)}
                    style={styles.input}
                />
                <button onClick={sendMessage} style={styles.button}>Send</button>
            </div>
        </div>
    );
}

const styles = {
    container: {
        width: "400px",
        margin: "50px auto",
        display: "flex",
        flexDirection: "column"
    },
    chatBox: {
        height: "500px",
        border: "1px solid #ddd",
        borderRadius: "10px",
        padding: "10px",
        display: "flex",
        flexDirection: "column",
        gap: "8px",
        overflowY: "auto"
    },
    message: {
        padding: "10px 14px",
        borderRadius: "10px",
        maxWidth: "70%"
    },
    inputRow: {
        marginTop: "10px",
        display: "flex",
        gap: "10px"
    },
    input: {
        flex: 1,
        padding: "10px",
        fontSize: "16px"
    },
    button: {
        padding: "10px 18px",
        cursor: "pointer"
    }
};

export default Chat;
