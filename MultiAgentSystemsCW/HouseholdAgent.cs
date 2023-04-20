using ActressMas;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MultiAgentSystemsCW
{
    internal class HouseholdAgent : Agent
    {

        private int demand;
        private int generation;
        private int priceToBuyFromUtility;
        private int priceToSellToUtility;
        private int needed;
        private int excess;
        private int currentPrice;
        private bool participating;
        private int bal;
        private int boughtEnergy;

        public override void Setup()
        {
            demand = 0;
            generation = 0;
            priceToBuyFromUtility = 0;
            priceToSellToUtility = 0;
            needed = 0;
            excess = 0;
            currentPrice = 0;
            participating = true;
            bal = 0;
            boughtEnergy = 0;

            Send("Environment", "start");
        }

        public override void Act(Message message)
        {
            message.Parse(out string action, out string parameters);
            
            switch (action)
            {
                case "inform":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    string[] data = parameters.Split(' ');

                    demand = Int32.Parse(data[0]);
                    generation = Int32.Parse(data[1]);
                    priceToBuyFromUtility = Int32.Parse(data[2]);
                    priceToSellToUtility = Int32.Parse(data[3]);

                    Usage();

                    break;

                case "AuctionOpen":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    if (needed > 0)
                    {
                        string sender = message.Sender;
                        currentPrice = Int32.Parse(parameters);
                        Bid(sender);
                        Send(message.Sender, "joiningAuction");
                        currentPrice = 0;
                    }
                    else
                    {
                        Send(message.Sender, "doneBiding");
                    }

                    break;

                case "lowBid":
                    if (needed > 0)
                    {
                        Console.WriteLine($"\r\n\t{message.Format()}");
                        string sender = message.Sender;
                        currentPrice = Int32.Parse(parameters) + 1;
                        Bid(sender);
                    }
                                
                    break;

                case "won":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    needed -= 1;
                    bal -= Int32.Parse(parameters);

                    /*
                    if (needed == 0)
                    {
                        Send(message.Sender, "doneBiding");
                    }
                    */
                    break;

                case "sold":
                    bal += Int32.Parse(parameters);

                    break;

                case "returnSenders":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    Console.WriteLine("Sold " + parameters + " kWh for " + (priceToSellToUtility * Int32.Parse(parameters)) + " " + "p");
                    bal = bal + priceToSellToUtility * Int32.Parse(parameters);
                    break;

                case "auctionClosed":
                    Console.WriteLine($"\r\n\t{message.Format()}");

                    if (needed == 0)
                    {
                        End();
                    }
                    else if (needed > 0)
                    {
                        for (int i = 0; i < needed; i++)
                        {
                            bal -= priceToBuyFromUtility;
                            boughtEnergy += 1;
                        }

                        End();
                    }

                    break;

                default:
                    break;

            }
        }

        private void Usage()
        {
            if (demand < generation)
            {
                excess = generation - demand;
                Sell(excess);
            }
            else if(demand > generation)
            {
                needed = demand - generation;
                Request(needed);
            }
            else
            {
                Send("Auction", "selfSufficient");
            }
        }


        private void Sell(int excess)
        {
            string content = "excess " + excess + " " + priceToSellToUtility;
            Send("Auction", content);
        }

        private void Request(int needed)
        {
            string content = "needed " + needed;
            Send("Auction", content);
        }

        private void Bid(string sender)
        {
            if (currentPrice < priceToBuyFromUtility)
            {
                string content = "bid " + currentPrice;
                Send(sender, content);
            }
            else
            {
                Send(sender, "tooExpensive");
            }
        }

        private void End()
        {
            Console.WriteLine(Name + ": " + "Profit is " + bal + "p.");
            Console.WriteLine("Unclean energy bought " + boughtEnergy);
            Send("Auction", "done");
            Stop();
        }
    }   
}
