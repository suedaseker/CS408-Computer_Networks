using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace server2
{
    public partial class Form1 : Form
    {
        //initialize some variables
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<Socket> clientSockets = new List<Socket>();
        static List<Player> playerList = new List<Player>();
        static List<Player> waitingList = new List<Player>();
        List<Player> leftplayer = new List<Player>();
        Dictionary<Player, Socket> playerSocketDict = new Dictionary<Player, Socket>();
        Dictionary<Player, Socket> waitingSocketDict = new Dictionary<Player, Socket>();
        List<string> questions = new List<string>();
        List<int> answers = new List<int>();

        bool isGameStarted = false;
        bool isGameFinished = false;
        bool playerleft = false;
        bool tie = false;
        bool isQuestionAsked = false;

        int numOfQuestions = 0;
        int numOfQuestionsAsked = 0;
        int numOfQuestionsControl = 0;
        string receiveAnswer = "";
        int numAnswers = 0;
        int connectCount = 0;

        static bool terminating = false;
        static bool accept = true;

        delegate void Del(string text);
        Thread acceptThread, receiveThread, gameThread;

        // For storing  informations about players/clients.
        public class Player : IComparable<Player>
        {
            public String name;
            public int id, answer, diff;
            public double score;
            public bool answered = false;

            public int CompareTo(Player other)
            {
                return name.CompareTo(other.name);
            }

            public override bool Equals(object obj)
            {
                var other = obj as Player;
                if (other == null) { return false; }
                return other.name == this.name;
            }
        }

        // To check if the name exists in the a Player list
        public bool nameExists(List<Player> list, string name)
        {
            foreach (Player player in list)
            {
                if (player.name == name)
                    return true;
            }
            return false;
        }

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort;

            if (Int32.TryParse(textBox_port.Text, out serverPort)) // if we can parse the input port number
            {
                if (serverPort <= 65535 && serverPort >= 0)
                {
                    try
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                        serverSocket.Bind(endPoint);
                        serverSocket.Listen(300);
                    }
                    catch (Exception ex)
                    {
                        richTextBox_info.AppendText("Fail: " + ex.ToString() + "\n");
                        richTextBox_info.ScrollToCaret();
                    }

                    button_listen.Enabled = false;
                    button_listen.Text = "Listening";
                    button_listen.BackColor = Color.Green;

                    // Starting Accept Thread 
                    acceptThread = new Thread(Accept);
                    acceptThread.IsBackground = true;
                    acceptThread.Start();

                    richTextBox_info.AppendText("Started listening on port: " + serverPort + "\n");
                    richTextBox_info.ScrollToCaret();
                    string filename = "questions.txt";

                    // Getting the questions and corresponding answers from the file
                    string[] lines = File.ReadAllLines(filename, Encoding.UTF8);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            questions.Add(lines[i]);
                        }
                        else
                        {
                            answers.Add(Int32.Parse(lines[i]));

                        }
                    }
                }
                else
                {
                    richTextBox_info.AppendText("Port number should be between 0 and 65535\n");
                    richTextBox_info.ScrollToCaret();
                }
            }
            else
            {
                richTextBox_info.AppendText("Please check port number\n");
                richTextBox_info.ScrollToCaret();
            }
        }

        // Accepts the client when they request connection
        private void Accept()
        {
            while (accept)
            {
                try
                {
                    clientSockets.Add(serverSocket.Accept()); // Add the new client's socket to clientSockets list

                    if (clientSockets.Count > 1) { this.Invoke((MethodInvoker)delegate { button_startgame.Enabled = true; }); }

                    // Start receiveThread to get inputs from the client
                    receiveThread = new Thread(new ThreadStart(Receive));
                    receiveThread.IsBackground = true;
                    receiveThread.Start();                   
                }
                catch
                {
                    if (terminating)
                    {
                        richTextBox_info.AppendText("Server disconnected...\n");
                        accept = false;
                    }

                    else { richTextBox_info.AppendText("Problem with the accept function...\n"); }
                }
            }
        }

        private void Receive()
        {
            bool connected = true;
            int lenClientSocket = clientSockets.Count();
            Socket thisClient = clientSockets[lenClientSocket - 1];
            Player myplayer = new Player();

            bool isRegistered = false;
      
            //2 -> client disconnected (coming)
            //3 -> answer (coming)
            //5 -> name exists error (going)

            // Continues until disconnection
            while (connected && !terminating)
            {
                try
                {
                    // If this client is not registered yet
                    if (!isRegistered)
                    {
                        Byte[] recname = new byte[20];
                        int rec = thisClient.Receive(recname); // Get the name of the this client
                        Player tempplayer = new Player();
                        string tempname = Encoding.ASCII.GetString(recname);
                        tempname = tempname.Substring(0, tempname.IndexOf("\0"));
                        tempplayer.name = tempname;

                        // If the the received name already exists, send an appropriate message
                        if (playerList.Contains(tempplayer) || nameExists(playerList, tempplayer.name) || nameExists(waitingList, tempplayer.name))
                        {
                            send_message(thisClient, "5&This name already exists in the game.\n");
                        }
                        else
                        {
                           
                            byte[] ok = BitConverter.GetBytes(100);
                            thisClient.Send(ok);
                            
                            if (!isGameStarted) // If the name does not already exist and game not started, save the client as player
                            {
                                richTextBox_info.AppendText(tempname + " connected\n");

                                myplayer.name = tempname;
                                myplayer.id = connectCount;
                                myplayer.score = 0;
                                playerList.Add(myplayer);
                                connectCount++;
                                playerSocketDict.Add(myplayer, thisClient);
                                isRegistered = true;
                            }
                            else // If the game already started, save the client to the waiting list    
                            {
                                richTextBox_info.AppendText(tempname + " connected and waiting for the game to end to join.\n");

                                string started = "The game has already started. To join, wait for the game to end.\n";

                                if (waitingList.Contains(tempplayer) || nameExists(waitingList, tempplayer.name))
                                {
                                    send_message(thisClient, "5&This name already exists in the game.\n");
                                }
                                else
                                {
                                    myplayer.name = tempname;
                                    myplayer.id = connectCount;
                                    myplayer.score = 0;

                                    waitingList.Add(myplayer);
                                    connectCount++;
                                    waitingSocketDict.Add(myplayer, thisClient);
                                    isRegistered = true;
                                    send_message(thisClient, started);
                                }
                            }
                        }
                    }

                    else// If the clients are already registered, receive the incoming messages
                    {
                        Byte[] message = new Byte[2048];
                        thisClient.Receive(message);
                        receiveAnswer = Encoding.Default.GetString(message);
                        string[] incoming = receiveAnswer.Split('&'); // To identify between incoming answers and disconnection issues

                        if (incoming[0] == "3") // Answer condition
                        {
                            if (playerList.Contains(myplayer) && isGameStarted && myplayer.answered == false)
                            {
                                int ans = Int32.Parse(incoming[1]);
                                myplayer.answer = ans;
                                myplayer.answered = true;
                                numAnswers++;
                            }
                        }
                        else if (incoming[0] == "2") // Exit condition, means the player disconnected
                        { 
                            if (playerList.Contains(myplayer)) // If the player is in the list, remove and close sockets
                            {
                                clientSockets.Remove(playerSocketDict[myplayer]);
                                playerSocketDict[myplayer].Shutdown(SocketShutdown.Both);
                                playerSocketDict[myplayer].Close();
                                playerSocketDict.Remove(myplayer);
                                playerList.Remove(myplayer);
                                
                                myplayer.score = 0;
                                if (myplayer.answered == true)
                                    numAnswers--;

                                if (isGameStarted) // If the client disconnected during the game, make appropriate changes
                                {
                                    if (!leftplayer.Contains(myplayer))
                                        leftplayer.Add(myplayer);

                                    if (playerSocketDict.Count == 1) // If one player is left in the game, the game will be finished
                                    {
                                        isGameFinished = true;
                                    }
                                    else
                                    {
                                        playerleft = true;
                                    }
                                }
                            }
                            else if (waitingList.Contains(myplayer)) // If the client was in the waiting list, remove and close sockets
                            {
                                clientSockets.Remove(waitingSocketDict[myplayer]);
                                waitingSocketDict[myplayer].Shutdown(SocketShutdown.Both);
                                waitingSocketDict[myplayer].Close();
                                waitingSocketDict.Remove(myplayer);
                                waitingList.Remove(myplayer);
                            }

                            richTextBox_info.AppendText(myplayer.name + " has disconnected\n");

                            break;
                        }
                    }
                }

                catch
                {
                    if (!terminating)
                    {
                        thisClient.Close();
                    }
                    clientSockets.Remove(thisClient);
                    connected = false;
                }
            }
        }

        private void button_startgame_Click(object sender, EventArgs e)
        {
            //if there are anyone waiting, add them in the player list and delete them from the waiting list
            foreach (Player waits in waitingList)
            {
                if (!playerList.Contains(waits))
                {
                    playerList.Add(waits);
                    playerSocketDict.Add(waits, waitingSocketDict[waits]);
                    waitingSocketDict.Remove(waits);
                }
            }
            waitingList.Clear();

            if (playerList.Count > 1) // Start the game if there are 2 or more players
            {
                if (Int32.TryParse(textBox_num.Text, out numOfQuestions)) // we got the number of questions as input
                {
                    isGameStarted = true;
                    gameThread = new Thread(new ThreadStart(thegame)); // The game function thread is called
                    gameThread.IsBackground = true;
                    gameThread.Start();
                    Int32.TryParse(textBox_num.Text, out numOfQuestions);
                    button_startgame.Enabled = false;
                }
                else
                {
                    richTextBox_info.AppendText("Please enter the number of the questions\n");
                    richTextBox_info.ScrollToCaret();
                }
                
            }
            else
            {
                richTextBox_info.AppendText("There should be at least 2 players to start the game\n");
            }
        }

        // The function where the game is handeled from the moment that "start game" button clicked until the game ends
        private void thegame()
        {
            isQuestionAsked = false;
            
            foreach (Player clientName in playerList)
            {
                send_message(playerSocketDict[clientName], "\nTHE GAME STARTED\n");
            }
            richTextBox_info.AppendText("\nTHE GAME STARTED\n");

            // The game will continue until one player is left in the game or all the questions asked
            while (isGameStarted && (numOfQuestionsControl != numOfQuestions))
            {
                if (!isQuestionAsked) // If the question is not asked yet, ask the question
                {
                    foreach (Player clientName in playerList) // Send the question to all the players
                    {
                        if (numOfQuestionsAsked < questions.Count())
                        {
                            send_message(playerSocketDict[clientName], "\nQuestion is: " + questions[numOfQuestionsAsked] + "\n");
                        }
                        else
                        {
                            numOfQuestionsAsked = numOfQuestionsAsked % questions.Count();
                            send_message(playerSocketDict[clientName], "\nQuestion is: " + questions[numOfQuestionsAsked] + "\n");
                        }

                    }
                    richTextBox_info.AppendText("\nQuestion is: " + questions[numOfQuestionsAsked] + "\n");
                    isQuestionAsked = true;
                }
                else // If the question is asked, wait untill everybody is answered
                {
                    while (numAnswers != (playerList.Count) && !isGameFinished)
                    {}

                    if (playerleft) // If a player leaves during the game
                    {
                        playerleft = false;
                        richTextBox_info.AppendText("A player left the game unexpectedly.Game will continue without that player.\n");
                    }

                    if (isGameFinished) // If there is one player left, isGameFinished will be true, so the game is going to end
                    {
                        break; 
                    } 

                    int correctAnswer = answers[numOfQuestionsAsked];
                    int min = 2147483647; // INT MAX
                    foreach (Player player in playerList) // Check the answers of each client
                    {
                        player.diff = Math.Abs(correctAnswer - player.answer);
                        richTextBox_info.AppendText(player.name + ": " + player.answer + "\n");

                        if (player.diff < min)
                        {
                            min = player.diff; // Found min difference among the players
                        }
                    }

                    List<Player> winner = new List<Player>();
                    List<string> sortedList = new List<string>();
                    foreach (Player player in playerList) // Compare the answer of each client to the min difference
                    {
                        if (player.diff == min)
                            winner.Add(player); // Determine the winners
                    }

                    richTextBox_info.AppendText("The answer is: " + correctAnswer + "\n");
                    richTextBox_info.AppendText("\nScores:\n");

                    if (winner.Count == 1) // If only one winner, give 1 point
                        winner[0].score += 1;
                    else // If there are more than one winner, shre the point among them
                    {
                        foreach (Player win in winner)
                            win.score += 1.0 / winner.Count;
                    }

                    playerList.Sort((a, b) => b.score.CompareTo(a.score)); // Sort the players in descending order wrt their scores

                    foreach (Player client in playerList) // Send the correct answer and other players' answers to each player
                    {
                        send_message(playerSocketDict[client], "The answer is: " + correctAnswer + "\n");

                        foreach (Player client2 in playerList)
                        {
                            if (client.name != client2.name)
                            {
                                send_message(playerSocketDict[client], client2.name + "'s answer is: " + client2.answer + "\n");
                                client.answered = false;
                            }
                        }
                        send_message(playerSocketDict[client], "\nScores:\n");
                        foreach (Player client2 in playerList) // Send the scores
                        {
                            send_message(playerSocketDict[client], client2.name + "(" + client2.score + ")\n");
                        }
                        richTextBox_info.AppendText(client.name + "(" + client.score + ")\n");
                        
                        if (leftplayer.Count != 0) // All the left players' scores will be shown as 0
                        {
                            foreach (Player left in leftplayer)
                                send_message(playerSocketDict[client], left.name + "(" + left.score + ")\n");
                        }
                    }
                    if (leftplayer.Count != 0)
                    {
                        foreach (Player left in leftplayer)
                            richTextBox_info.AppendText(left.name + "(" + left.score + ")\n");
                    }

                    isQuestionAsked = false;
                    numAnswers = 0;
                    numOfQuestionsAsked += 1;
                    numOfQuestionsControl++;
                }

            }

            string winners = "";
            List<string> winnerlist = new List<string>();
            winnerlist = whoIsWinner(playerList);

            try
            {
                if (isGameFinished) // If only one player left in the game, send appropriate message
                {
                    foreach (Player player in playerList)
                    {
                        send_message(playerSocketDict[player], "All other players left the game and " + player.name + " won the game!\n");
                        send_message(playerSocketDict[player], "\nTHE GAME IS OVER.\n\n");
                        richTextBox_info.AppendText("All other players left the game and " + player.name + " won the game!\n");
                    }
                }
                else
                {
                    if (tie) // If there is a tie
                    {
                        foreach (string word in winnerlist) // Get all the players with the same score
                        {
                            winners += word + "\n";
                        }

                        foreach (Player player in playerList)
                        {
                            send_message(playerSocketDict[player], "There was a tie and winners are: \n" + winners);
                            send_message(playerSocketDict[player], "\nTHE GAME IS OVER.\n\n");
                        }
                        richTextBox_info.AppendText("There was a tie and winners are:\n" + winners);
                    }

                    else // no tie
                    {
                        foreach (Player player in playerList)
                        {
                            send_message(playerSocketDict[player], winnerlist[0] + " is the winner\n!");
                            send_message(playerSocketDict[player], "\nTHE GAME IS OVER.\n\n");
                        }
                        richTextBox_info.AppendText(winnerlist[0] + " is the winner\n!");
                    }
                }
            }

            catch
            {
                richTextBox_info.AppendText("ERROR\n");
                terminating = true;
                serverSocket.Close();
            }

            richTextBox_info.AppendText("\nTHE GAME IS OVER.\n\n");
            button_startgame.Enabled = true;
            ResetGame();
        }

        // To reset some variables at the end of the finished game, before the new game starts
        public void ResetGame()
        {
            numOfQuestionsControl = 0;
            numOfQuestionsAsked = 0;
            numAnswers = 0;
            isGameStarted = false;
            isGameFinished = false;
            isQuestionAsked = false;
            playerleft = false;
            tie = false;
            terminating = false;
            accept = true;
            leftplayer.Clear();
            foreach (Player player in playerList)
            {
                player.score = 0;
            }
        }

        // Makes a list of players who won the game
        public List<string> whoIsWinner(List<Player> list)
        {
            List<string> winnerlist = new List<string>();
            int i = (list.Count) - 2;

            playerList.Sort((a, b) => a.score.CompareTo(b.score));
            try
            {
                winnerlist.Add(list[(list.Count) - 1].name);
                double maxscore = list[list.Count - 1].score;

                for (; i >= 0; i--)
                {
                    if (maxscore == list[i].score) // If tied
                    {
                        winnerlist.Add(list[i].name);
                        tie = true;
                    }
                    else { break; }     
                }
            }
            catch { }
            return winnerlist;
        }

        // Takes socket and message then sends the message to that socket
        private void send_message(Socket clientSocket, string message)
        {
            Byte[] buffer = new Byte[10000000];
            buffer = Encoding.Default.GetBytes(message);
            if(clientSockets.Contains(clientSocket))
                clientSocket.Send(buffer);
        }

        // The closing server funtion override
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string terminate = "4$Shutting Down...";
            Byte[] close = Encoding.Default.GetBytes(terminate);

            if (playerSocketDict.Count != 0)
            {
                foreach (Player player in playerList)
                {
                    playerSocketDict[player].Send(close);
                    playerSocketDict[player].Shutdown(SocketShutdown.Both);
                    playerSocketDict[player].Close();
                }
            }

            Application.Exit();
            Environment.Exit(Environment.ExitCode);
            return;
        }

    }
}
