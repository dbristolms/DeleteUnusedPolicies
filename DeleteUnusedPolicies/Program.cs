using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeleteUnusedPolicies
{
    class Program
    {
        private static CloudMediaContext _context = null;

        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                DisplayUsage();
                return;
            }
            string tenantDomain = args[0];
            string RESTendpoint = args[1];

            // Specify your AAD tenant domain, for example "microsoft.onmicrosoft.com"
            AzureAdTokenCredentials tokenCredentials = new AzureAdTokenCredentials(tenantDomain, AzureEnvironments.AzureCloudEnvironment);

            AzureAdTokenProvider tokenProvider = new AzureAdTokenProvider(tokenCredentials);

            // Specify your REST API endpoint, for example "https://accountname.restv2.westcentralus.media.azure.net/API"
            _context = new CloudMediaContext(new Uri(RESTendpoint), tokenProvider);

            List<string> usedPolicyIds = new List<string>();

            int skipSize = 0;
            int batchSize = 1000;
            int currentBatch = 0;

            Console.WriteLine("Collecting list of policies from existing locators...");

            while (true)
            {
                // Loop through all locators (1000 at a time) in the Media Services account
                IQueryable<ILocator> locators = _context.Locators.Skip(skipSize).Take(batchSize);
                foreach (ILocator locator in locators)
                {
                    currentBatch++;
                    usedPolicyIds.Add(locator.AccessPolicyId);
                }

                if (currentBatch == batchSize)
                {
                    skipSize += batchSize;
                    currentBatch = 0;
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("Policies total: " + _context.AccessPolicies.Count());
            Console.WriteLine("Policies currently used by locators: " + usedPolicyIds.Count.ToString());

            // Reuse the same variables
            skipSize = 0;
            batchSize = 1000;
            currentBatch = 0;

            List<IAccessPolicy> policiesToDelete = new List<IAccessPolicy>();
            Console.WriteLine("Collecting full list of access policies...");
            while (true)
            {
                IQueryable<IAccessPolicy> policies = _context.AccessPolicies.Skip(skipSize).Take(batchSize);

                foreach (IAccessPolicy policy in policies)
                {
                    currentBatch++;
                    IEnumerable<string> list = from a in usedPolicyIds
                                               where a == policy.Id
                                               select a;
                    if (list.Count() == 0)
                    {
                        policiesToDelete.Add(policy);
                    }
                }

                if (currentBatch == batchSize)
                {
                    skipSize += batchSize;
                    currentBatch = 0;
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("Remove " + policiesToDelete.Count.ToString() + " unused policies y/n?");
            ConsoleKeyInfo response = Console.ReadKey();
            if (response.Key.ToString().Equals("Y"))
            {
                foreach (IAccessPolicy deletePolicy in policiesToDelete)
                {
                    Console.WriteLine("Deleting policy: " + deletePolicy.Id);
                    deletePolicy.Delete();
                }
            }
        }

        private static void DisplayUsage()
        {
            Console.Out.WriteLine("\nThere is a limit of 1 million access policies in a Media Services account. This application deletes");
            Console.Out.WriteLine("all access policies that are not associated with an existing locator.");
            Console.Out.WriteLine("\n\nDeleteUnusedPolicies.exe <AAD tenant domain> <REST API endpoint>");
            Console.Out.WriteLine("\nExample:\nDeleteUnusedPolicies.exe microsoft.onmicrosoft.com https://accountname.restv2.westcentralus.media.azure.net/API");
        }
    }
}