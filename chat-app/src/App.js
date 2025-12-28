import { useState, useEffect } from "react";
import { db } from "./firebase";
import { collection, addDoc, onSnapshot, orderBy, query } from "firebase/firestore";


function App() {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState("");

    useEffect(() => {
        const q = query(collection(db, "messages"), orderBy("createdAt"));

        const unsubscribe = onSnapshot(q, snapshot => {
            const msgs = snapshot.docs.map(doc => doc.data());
            setMessages(msgs);
        });

        return () => unsubscribe();
    }, []);

    const sendMessage = async () => {
        if (!input.trim()) return;

        await addDoc(collection(db, "messages"), {
            text: input,
            sender: "user",
            createdAt: new Date()
        });

        setInput("");
    };



    return (
        <div className="app">
            <h2>University Tuition Chat</h2>

            <div className="chat-box">
                <p><b>🤖 Bot:</b> I'm University Tuition Chat. How can I help you?</p>

                {messages.map((m, index) => (
                    <p key={index}>
                        <b>{m.sender === "user" ? "🧑 You" : "🤖 Bot"}:</b> {m.text}
                    </p>
                ))}
            </div>

            <div className="inputBox">
                <input
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder="Write something..."
                />
                <button onClick={sendMessage}>Send</button>
            </div>
        </div>
    );
}

export default App;
