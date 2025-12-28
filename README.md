# University Tuition Payment Chat Assistant

This project extends the University Tuition Payment System by adding an AI-powered conversational assistant that allows students to interact with the university tuition system using natural language.

The chatbot understands user intent using a Local LLM (Ollama + Mistral), routes the request through an **API Gateway**, communicates with the **ASP.NET Core Tuition Backend**, and returns responses to the chat UI using Firestore.

## Features
-  Query tuition information
-  Check unpaid tuition
-  Simulated tuition payment
-  AI-based intent detection
-  Firebase Firestore real-time messaging
-  Secure API Gateway routing
-  PostgreSQL + Entity Framework backend


## System Architecture

- React Chat UI
- Firestore (messages)
- Cloud Function triggered on user message
- Ollama + Mistral (Intent + Parameters)
- API Gateway (YARP)
- ASP.NET Tuition API (PostgreSQL + EF Core)
- Backend Response
- Bot Reply written to Firestore
- UI updates in real-time

## Technologies Used

**Frontend**
- React  
- Firebase Firestore  
- Realtime Listener

**AI Layer**
- Ollama  
- Mistral Model (local)  

**Backend**
- ASP.NET Core Web API  
- Entity Framework Core  
- PostgreSQL (Railway)  
- JWT Authentication  

**Gateway**
- YARP Reverse Proxy

**Cloud**
- Firebase Cloud Functions

## Assumptions
- Users mainly request:
  - Tuition info  
  - Unpaid tuition  
  - Payment
- Student number is provided or inferred
- Payment is simulated, not real
- AI always returns JSON in this structure:

json
{
 "intent": "query | unpaid | pay",
 "studentNumber": "string or null"
}

## Issues Encountered
- Firestore permissions blocked writes, solved the issue with rule changes in Firestore Console.
- Cloud Functions v1 vs v2 mismatch, solved by switching to:
import { onDocumentCreated }
from "firebase-functions/v2/firestore";
- API / Gateway Port
API Port: https://localhost:7127
Gateway Port: http://localhost:5157
- Emulator UI sometimes fails

## How to Run the Project (Step-By-Step)

Follow the steps below in order:

### Start the Tuition Backend API
This runs the ASP.NET Core University Tuition Payment System.

1. Open solution in Visual Studio
2. Right-click UniversityTuitionPaymentSystem
3. Select Set as Startup Project
4. Click Run

Final API URLs generally look like:
- http://localhost:5157
- https://localhost:7127

### Start the API Gateway
This enables routing + rate limiting + logging.

1. In Solution Explorer  
Right-click UniversityTuitionPaymentGateway
2. Select Set as Startup Project
3. Run

Gateway will start and forward requests to API.

### Start Local LLM (Ollama + Mistral)

Make sure Ollama is installed.

Pull model (one time only): ollama pull mistral
Start server: ollama serve
Must run at: http://localhost:11434

### Start Firebase Cloud Function
Go to `functions` folder and run: firebase emulators:start

## And Finally
### Run the React Chat UI

Go to `chat-app` folder:
cd chat-app
npm install
npm start

