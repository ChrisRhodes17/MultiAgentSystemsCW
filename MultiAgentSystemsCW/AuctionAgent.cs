using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAgentSystemsCW
{
    internal class AuctionAgent : Agent
    {
        private string highestBidder;
        private int highestBid;
        private int available;
        private int need;
        private string[,] senders = new string[Settings.numHousesholds, 3];
        private int id;
        private int participants;
        private int price;
        private string lastHighestBidder;
        private int bidderCount;
        private int energyLoc;
        private int numSenders;
        private int numRequest;
        private bool oneBidder;
        private int done;
        private int numMessages;

        public override void Setup()
        {
            highestBidder = "";
            highestBid = 0;
            available = 0;
            need = 0;
            id = 0;
            participants = 0;
            price = 0;
            lastHighestBidder = " ";;
            numSenders = 0;
            numRequest = 0;
            oneBidder = false;
            done = 0;
            numMessages = 0;
        }


        public override void Act(Message message)
        {
            numMessages += 1;

            message.Parse(out string action, out string parameters);

            switch (action)
            {
                case "excess":
                    //recives amount of energy for sale
                    string[] data = parameters.Split(' ');
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    senders[id, 0] = message.Sender;
                    senders[id, 1] = data[0];
                    senders[id, 2] = data[1];
                    available += Int32.Parse(data[0]);
                    Console.WriteLine(available);

                    numSenders += 1;
                    id += 1;

                    AddPart();

                    break;
                    //recives amount of energy demanded
                case "needed":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    need += Int32.Parse(parameters);
                    numRequest += 1;

                    AddPart();

                    break;

                case "selfSufficient":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    AddPart();

                    break;
                    //recives bids from households
                case "bid":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    HandleBid(Int32.Parse(parameters), message.Sender);
                    
                    break;
                    //recives when price is more than househoulds to value 
                case "tooExpensive":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    bidderCount -= 1;

                    if ((bidderCount == 1) && (numRequest > 1))
                    {
                        AuctionOver();
                    }

                    break;
                case "joiningAuction":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    bidderCount += 1;

                    break;
                /*
                case "doneBiding":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                
                numRequest -= 1;

                if (numRequest == 1)
                {
                    oneBidder = true;
                    AuctionOver();
                }

                break;
                */

                case "done":
                    done += 1;

                    if (done == Settings.numHousesholds)
                    {
                        Console.WriteLine("Number of messages sent to the auction is " + numMessages);
                        Stop();
                    }
                    break;



                default:
                    break;
            }
        }


        private void AddPart()
        {
            participants += 1;

            if ((participants >= Settings.numHousesholds) && (need > 0) && (available > 0))
            {
                Broadcast("AuctionOpen");
            }

        }

        private void StartAuction()
        {
            price = Int32.Parse(senders[0, 2]);
            energyLoc = 0;
            for (int i = 1; i < numSenders; i++)
            {
                if ((price > Int32.Parse(senders[i, 2])) && (Int32.Parse(senders[i, 2]) > 0) && (Int32.Parse(senders[i, 1]) > 0))
                {
                    price = Int32.Parse(senders[i, 2]);
                    energyLoc = i;
                }
            }
            bidderCount = 0;
            highestBid = 0;
            lastHighestBidder = " ";
            Broadcast("AuctionOpen " + price);
        }

        private void HandleBid(int bid, string bidder)
        {
            if (bid > highestBid)
            {
                highestBid = bid;
                highestBidder = bidder;

                {
                    if (lastHighestBidder != " ")
                    {
                        Send(lastHighestBidder, "lowBid " + highestBid);
                    }
                    lastHighestBidder = highestBidder;
                }
            }
            else if (bid <= highestBid)
            {
                Send(bidder, "lowBid " + highestBid);
            }               
            if (oneBidder == true)
            {
                AuctionOver();
            }
        }

        private void AuctionOver()
        {
            Send(highestBidder, "won " + highestBid);
            available -= 1;
            need -= 1;
            int temp = Int32.Parse(senders[energyLoc, 1]);
            int temp2 = temp - 1;
            string temp3 = temp2.ToString();
            Send(senders[energyLoc, 0], "sold " + highestBid);
            senders[energyLoc, 1] = temp3;
            if ((need > 0) && (available > 0))
            {
                StartAuction();
                Console.WriteLine("Continuing, need: " + need + " available: " + available);
            }
            else if (available > 0)
            {
                Console.WriteLine("done, need: " + need + " available: " + available);
                Console.WriteLine("available");

                for (int i = 0; i < numSenders; i++)
                {
                    Console.WriteLine(i);

                    if (Int32.Parse(senders[i, 1]) > 0)
                    {
                        Send(senders[i, 0], "returnSenders " + senders[i, 1]);
                    }
                    else if (Int32.Parse(senders[i, 1]) == 0)
                    {
                        Console.WriteLine("yay works");
                    }
                    else if (Int32.Parse(senders[i, 1]) < 0)
                    {
                        Console.WriteLine("error");
                    }
                    else
                    {
                        Console.WriteLine("big error");
                    }

                }

            }
            else if (need > 0)
            {
                Console.WriteLine("done, need: " + need + " available: " + available);
                Console.WriteLine("need");

                Broadcast("auctionClosed");
            }
        }
       
    }
}
    