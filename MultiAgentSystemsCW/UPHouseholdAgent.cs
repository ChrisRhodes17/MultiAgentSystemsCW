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
    internal class UPHouseholdAgent : Agent
    {

        private int demand;
        private int generation;
        private int priceToBuyFromUtility;
        private int priceToSellToUtility;
        private int needed;
        private int excess;
        private int price;
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
            price = 0;
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
                        CalculatePrice();
                    }

                    break;

                case "NoAuction":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    if (excess > 0)
                    {
                        Console.WriteLine("Sold " + excess + "kWh to Utility for " + (priceToSellToUtility * excess) + "p");
                    }
                    else if (needed > 0)
                    {
                        Console.WriteLine("Bought " + needed + "kWh from Utility for " + (priceToBuyFromUtility * needed) + "p");
                        boughtEnergy = needed;
                    }

                    End();

                    break;                

                case "won":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    needed -= 1;

                    bal -= Int32.Parse(parameters);

                    break;

                case "returning":
                    Console.WriteLine($"\r\n\t{message.Format()}");
                    bal += priceToSellToUtility;

                    break;

                case "auctionOver":
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
                Sell();
            }
            else if(demand > generation)
            {
                needed = demand - generation;
                Request();
            }
            else
            {
                Send("Auction", "selfSufficient");
            }
        }


        private void Sell()
        {
            string content = "excess " + excess;
            Send("Auction", content);
        }

        private void Request()
        {
            string content = "needed " + needed;
            Send("Auction", content);
        }

        private void CalculatePrice()
        {
            price = priceToBuyFromUtility - 1;

            Bid();
        }

        private void Bid()
        {
            for (int i = 0; i < needed; i++)
            {
                string content = "bid " + price;
                Send("Auction", content);
                if (i == needed - 1)
                {
                    Send("Auction", "joiningAuction");
                }
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
