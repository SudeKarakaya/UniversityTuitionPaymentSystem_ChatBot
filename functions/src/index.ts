import { onDocumentCreated } from "firebase-functions/v2/firestore";
import * as admin from "firebase-admin";

admin.initializeApp();
const db = admin.firestore();

// Gateway base URL (Gateway açık olmalı)
const GATEWAY_URL = "http://localhost:5157";

// Firestore Trigger
export const handleMessage = onDocumentCreated("messages/{msgId}", async (event) => {
    const snapshot = event.data;
    if (!snapshot) return;

    const data = snapshot.data();
    if (!data || data.sender !== "user") return;

    const userMessage = data.text;
    console.log("User message:", userMessage);

    // Ollama
    const ollamaResponse = await fetch("http://localhost:11434/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            model: "mistral",
            messages: [
                {
                    role: "system",
                    content: `
You are a university tuition assistant.
User may ask:
1) tuition query
2) unpaid tuition
3) payment

Return STRICT JSON ONLY!!!
{
 "intent": "query" | "unpaid" | "pay",
 "studentNumber": "string or null"
}
`
                },
                { role: "user", content: userMessage }
            ]
        }),
    });

    const result = await ollamaResponse.json();
    console.log("Ollama Raw:", result);

    let replyText = "Sorry, I could not understand your request.";

    try {

        const ai = JSON.parse(result.message.content.trim());
        console.log("AI Parsed:", ai);

        if (!ai.intent) throw new Error("AI intent missing");

        switch (ai.intent) {

            case "query":
                replyText =
                    await callGateway(`${GATEWAY_URL}/mobile/query?studentNo=${ai.studentNumber}`);
                break;

            case "unpaid":
                replyText =
                    await callGateway(`${GATEWAY_URL}/mobile/unpaid?studentNo=${ai.studentNumber}`);
                break;

            case "pay":
                replyText =
                    await callGateway(`${GATEWAY_URL}/mobile/pay`, "POST", {
                        studentNo: ai.studentNumber
                    });
                break;

            default:
                replyText = "I am not sure what you want to do.";
        }

    } catch (err) {
        console.error("AI Processing Error:", err);
        replyText = "AI response could not be processed.";
    }

    // LLM Reply writes to firestore
    await db.collection("messages").add({
        text: replyText,
        sender: "bot",
        createdAt: new Date()
    });
});


// Gateway caller
async function callGateway(url: string, method = "GET", body?: any) {
    try {
        const res = await fetch(url, {
            method,
            headers: { "Content-Type": "application/json" },
            body: body ? JSON.stringify(body) : undefined
        });

        if (!res.ok) {
            console.log("Gateway Error", res.status);
            return `Gateway Error (${res.status})`;
        }

        return await res.text();
    }
    catch (e) {
        console.error("Gateway Connection Failed:", e);
        return "Gateway connection failed.";
    }
}
