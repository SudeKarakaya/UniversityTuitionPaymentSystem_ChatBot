import { initializeApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";

const firebaseConfig = {
    apiKey: "AIzaSyAh8QPVzojS4Cxf9FmeezFni_XMuLgmAYU",
    authDomain: "university-tuition-chat.firebaseapp.com",
    projectId: "university-tuition-chat",
    storageBucket: "university-tuition-chat.firebasestorage.app",
    messagingSenderId: "521821000108",
    appId: "1:521821000108:web:3e2fef971362e0d9c5ea09"
};

const app = initializeApp(firebaseConfig);

export const db = getFirestore(app);
