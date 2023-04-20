using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiAgentSystemsCW
{
    internal class UPAuctionAgent : Agent
    {
        private List<string> BiddersList = new List<string>();
        private List<int> BidList = new List<int>();
        private int available;
        private int need;
        private int participants;
        private int price;
        private int bidderCount;
        private int numNeed;
        private int done;
        private int numMessages;
        private int profit;

        private List<string> SendersList = new List<string>();
        private List<int> SentList = new List<int>();


        public override void Setup()
        {
            available = 0;
            need = 0;
            participants = 0;
            price = 0;
            numNeed = 0;
            done = 0;
            numMessages = 0;
            profit = 0;
        }

        public override void Act(Message message)
        {
            numMessages += 1;

            message.Parse(out string action, out string parameters);

            switch (action)
            {
                case "excess":
                    //recives amount of energy for sale
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    available += Int32.Parse(parameters);

                    SendersList.Add(message.Sender);
                    SentList.Add(Int32.Parse(parameters));

                    AddPart();

                    break;
                //recives amount of energy demanded
                case "needed":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    need += Int32.Parse(parameters);
                    numNeed += 1; 
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
                
                case "joiningAuction":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    bidderCount += 1;

                    if (bidderCount == numNeed)
                    {
                        OrderBids();
                    }

                    break;

                case "done":
                    done += 1;

                    if (done == Settings.numHousesholds)
                    {
                        Console.WriteLine("Number of messages sent to the auction is " + numMessages);
                        Console.WriteLine("Auction profit is " + profit);
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
                StartAuction();
            }
            else if ((participants >= Settings.numHousesholds) && (need == 0))
            {
                Broadcast("NoAuction");
                Stop();
            }
            else if ((participants >= Settings.numHousesholds) && (available == 0))
            {
                Broadcast("NoAuction");
                Stop();
            }
        }

        private void StartAuction()
        {
            bidderCount = 0;
            Broadcast("AuctionOpen");
        }

        private void HandleBid(int bid, string bidder)
        {
            BiddersList.Add(bidder);
            BidList.Add(bid);         
        }

        private void OrderBids()
        {
            for (int i = 0; i < BidList.Count; i++)
            {
                for (int j = 0; j < BidList.Count - 1; j++)
                {
                    if (BidList[j] > BidList[j + 1])
                    {
                        int tempBid = BidList[j];
                        BidList[j] = BidList[j + 1];
                        BidList[j + 1] = tempBid;

                        string tempBidder = BiddersList[j];
                        BiddersList[j] = BiddersList[j + 1];
                        BiddersList[j + 1] = tempBidder;
                    }
                }
            }
            CalculatePrice();
        }

        private void CalculatePrice()
        {
            int pointer = 0;
            if (need < available)
            {
                price = BidList[need - 1];
                pointer = need;
            }
            else if (available < need)
            {
                price = BidList[available - 1];
                pointer = available;
            }
            else if (need == available)
            {
                price = BidList[need - 1];
                pointer = need;
            }

            Winners(pointer);      
        }

        private void Winners(int pointer)
        {
            for (int i = 0; i < pointer; i++)
            {
                Send(BiddersList[i], "won " + price);
                profit += price;

            }
            int remaining = 0;
            remaining = available - need;
            if (remaining > 0)
            {
                ReturnRemaining(remaining);
            }
            Broadcast("auctionOver");
        }

        private void ReturnRemaining(int remaining)
        {
            if (remaining <= SendersList.Count)
            {
                for (int i = 0; i < remaining; i++)
                {
                    Send(SendersList[i], "returning");
                }
            }
            else if (remaining > SendersList.Count)
            {
                int temp = remaining;
                for (int i = 0; i < SendersList.Count; i++)
                {
                    if (temp > 0)
                    {
                        if (temp > SentList[i])
                        {
                            for (int j = 0; j < SentList[i]; j++)
                            {
                                Send(SendersList[i], "returning");
                                temp -= 1;
                            }
                        }
                        else if(temp < SentList[i])
                        {
                            for (int j = 0; j < temp; j++)
                            {
                                Send(SendersList[i], "returning");
                                temp -= 1;
                            }
                        }
                    }
                }
            }
        }
    }
}
