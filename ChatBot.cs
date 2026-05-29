// ChatBot.cs
// The central brain of the chatbot - routes all input through features
// ProcessInput() is the single entry point called by MainWindow.xaml.cs
// This class coordinates: Memory, Sentiment, Keywords, Follow-ups, Fallbacks


using System;

namespace CybersecurityChatbot
{
    public class ChatBot
    {
        // Feature classes - injected via constructor for loose coupling
        private KeywordResponder _keywordResponder;
        private SentimentDetector _sentimentDetector;
        private MemoryStore _memoryStore;

        // Conversation state tracking
        private bool _awaitingName = true;      // True until user gives their name
        private string _lastTopic = "";         // Remember last topic for follow-ups
        private Random _random = new Random();

        // Constructor: initialise all feature classes
        public ChatBot()
        {
            _keywordResponder = new KeywordResponder();
            _sentimentDetector = new SentimentDetector();
            _memoryStore = new MemoryStore();
        }

        // Returns the opening greeting that asks for user's name
        public string GetGreeting()
        {
            return "Hello! Welcome to the Cybersecurity Awareness Bot.\n\nI'm here to help you stay safe online. To get started, please tell me your name.";
        }

        // MAIN ROUTING METHOD - called by MainWindow.xaml.cs for every user input
        // Handles inputs in strict order: Name → Follow-up → Sentiment → Keywords → Special → Fallback
        public string ProcessInput(string input)
        {
            string lowerInput = input.ToLower().Trim();

            // STEP 1: If we don't have a name yet, capture it
            if (_awaitingName)
            {
                return CaptureName(input);
            }

            // STEP 2: Check for follow-up phrases ("tell me more", etc.)
            if (IsFollowUpQuestion(lowerInput))
            {
                return GetFollowUpResponse();
            }

            // STEP 3: Check for memory storage patterns ("I'm interested in...")
            string memoryResponse = TryStoreMemory(input);
            if (memoryResponse != null)
            {
                return memoryResponse;
            }

            // STEP 4: Detect sentiment and get empathetic opener
            Sentiment sentiment = _sentimentDetector.Detect(input);
            string sentimentOpener = _sentimentDetector.GetSentimentResponse(sentiment);

            // STEP 5: Get keyword-based cybersecurity response
            string keywordResponse = _keywordResponder.GetResponse(lowerInput);

            if (keywordResponse != null)
            {
                // Remember this topic for follow-ups
                _lastTopic = _keywordResponder.LastMatchedKeyword;

                // If sentiment detected, prepend empathetic opener + auto-share tip
                if (!string.IsNullOrEmpty(sentimentOpener))
                {
                    return sentimentOpener + "\n\n" + keywordResponse + "\n\nI hope that helps! Let me know if you want to know more.";
                }

                // Add personalised touch if we know their favourite topic
                string personalTouch = GetPersonalisedTouch();
                if (!string.IsNullOrEmpty(personalTouch))
                {
                    return personalTouch + "\n\n" + keywordResponse;
                }

                return keywordResponse;
            }

            // STEP 6: Handle special conversational phrases
            string specialResponse = HandleSpecialPhrases(lowerInput);
            if (specialResponse != null)
            {
                return specialResponse;
            }

            // STEP 7: Fallback response for unknown input
            return GetFallbackResponse();
        }

        // STEP 1: Captures and stores the user's name
        private string CaptureName(string input)
        {
            string name = input.Trim();

            // Validate: name can't be empty
            if (string.IsNullOrWhiteSpace(name))
            {
                return "I didn't catch that. Could you please tell me your name?";
            }

            // Store in memory
            _memoryStore.UserName = name;
            _awaitingName = false;

            return $"Nice to meet you, {name}! I'm your Cybersecurity Awareness Assistant.\n\n" +
                   $"You can ask me about: password safety, phishing, privacy, scams, malware, " +
                   $"social engineering, two-factor authentication, Wi-Fi safety, or data backup.\n\n" +
                   $"What would you like to learn about?";
        }

        // STEP 2: Checks if input is a follow-up question
        private bool IsFollowUpQuestion(string input)
        {
            if (string.IsNullOrEmpty(_lastTopic))
                return false;

            string[] followUpPhrases = {
                "tell me more", "explain more", "more info", "elaborate",
                "what else", "anything else", "more", "give me another tip",
                "another one", "continue", "go on"
            };

            foreach (string phrase in followUpPhrases)
            {
                if (input.Contains(phrase))
                    return true;
            }

            return false;
        }

        // STEP 2: Returns additional info about the last discussed topic
        private string GetFollowUpResponse()
        {
            return _keywordResponder.GetFollowUpResponse(_lastTopic);
        }

        // STEP 3: Tries to detect and store memory patterns from natural language
        // Examples: "I'm interested in privacy", "I like passwords", "My favourite is malware"
        private string TryStoreMemory(string input)
        {
            string lowerInput = input.ToLower();

            // Pattern 1: "I'm interested in [topic]"
            if (lowerInput.Contains("interested in") || lowerInput.Contains("like"))
            {
                string topic = _keywordResponder.ExtractTopicFromInput(lowerInput);
                if (topic != null)
                {
                    _memoryStore.FavouriteTopic = topic;
                    string name = _memoryStore.UserName;
                    return $"Great, {name}! I'll remember that you're interested in {topic}. " +
                           $"It's a crucial part of staying safe online. Ask me anything about it!";
                }
            }

            // Pattern 2: "My favourite topic is [topic]"
            if (lowerInput.Contains("favourite") || lowerInput.Contains("favorite"))
            {
                string topic = _keywordResponder.ExtractTopicFromInput(lowerInput);
                if (topic != null)
                {
                    _memoryStore.FavouriteTopic = topic;
                    string name = _memoryStore.UserName;
                    return $"Noted, {name}! I'll remember that {topic} is your favourite topic. " +
                           $"I'll make sure to share relevant tips about it during our conversation.";
                }
            }

            return null; // No memory pattern detected
        }

        // Adds personalised touch based on stored memory
        private string GetPersonalisedTouch()
        {
            string favouriteTopic = _memoryStore.FavouriteTopic;
            string name = _memoryStore.UserName;

            if (!string.IsNullOrEmpty(favouriteTopic) && _random.Next(3) == 0) // 33% chance
            {
                return $"As someone interested in {favouriteTopic}, {name}, here's something relevant:";
            }

            if (!string.IsNullOrEmpty(name) && _random.Next(5) == 0) // 20% chance
            {
                return $"Hey {name}, here's a tip for you:";
            }

            return null;
        }

        // STEP 6: Handles special conversational phrases
        private string HandleSpecialPhrases(string input)
        {
            if (input.Contains("how are you") || input.Contains("how r u"))
            {
                string name = _memoryStore.UserName;
                return $"I'm doing great, {name}! All my security systems are operational. Ready to help you stay safe online!";
            }

            if (input.Contains("purpose") || input.Contains("what do you do") || input.Contains("who are you"))
            {
                return "I am a Cybersecurity Awareness Bot. I educate South African citizens about online threats like phishing, malware, and social engineering. How can I help you today?";
            }

            if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "You can ask me about: password safety, phishing, privacy, scams, malware, social engineering, two-factor authentication, Wi-Fi safety, or data backup. What interests you?";
            }

            if (input.Contains("bye") || input.Contains("exit") || input.Contains("quit"))
            {
                string name = _memoryStore.UserName;
                return $"Goodbye, {name}! Remember to stay vigilant online. Stay safe!";
            }

            return null;
        }

        // STEP 7: Fallback response for unrecognised input
        private string GetFallbackResponse()
        {
            string[] fallbacks = {
                "I'm not sure I understand. Could you try rephrasing? You can ask about passwords, phishing, privacy, scams, malware, and more.",
                "I didn't catch that. Try asking about a cybersecurity topic like 'password safety' or 'phishing tips'.",
                "Hmm, I'm not familiar with that. I can help with: passwords, phishing, malware, privacy, scams, 2FA, Wi-Fi safety, or backups. What would you like to know?",
                "I specialise in cybersecurity topics. Ask me about staying safe online!"
            };

            return fallbacks[_random.Next(fallbacks.Length)];
        }
    }
}
