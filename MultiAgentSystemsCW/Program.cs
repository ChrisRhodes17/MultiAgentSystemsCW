using ActressMas;

namespace MultiAgentSystemsCW
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var env = new EnvironmentMas(randomOrder: false, parallel: false);

            var EnvironmentAgent = new EnvironmentAgent();
            env.Add(EnvironmentAgent, "Environment");

            
            var AuctionAgent = new AuctionAgent();
            env.Add(AuctionAgent, "Auction");

            for (int i = 1; i <= Settings.numHousesholds; i++)
            {
                var HouseholdAgent = new HouseholdAgent();
                env.Add(HouseholdAgent, "Household" + i);
            }

            /*
            var UPAuctionAgent = new UPAuctionAgent();
            env.Add(UPAuctionAgent, "Auction");

            for (int i = 1; i <= Settings.numHousesholds; i++)
            {
                var UPHouseholdAgent = new UPHouseholdAgent();
                env.Add(UPHouseholdAgent, "Household" + i);
            }
            */

            env.Start();
        }
    }
}