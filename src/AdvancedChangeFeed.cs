using Microsoft.Azure.Cosmos;
using models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CosmicWorks
{
    public class AdvancedChangeFeed
    {
        private ChangeFeedProcessor _changeFeedProcessor;
        private CosmosClient _cosmosClient;
        private Container _monitoredContainer;
        private Container _leasesContainer;

        public AdvancedChangeFeed(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;

            // Monitor the V5 customer container for comprehensive change tracking
            _monitoredContainer = _cosmosClient.GetContainer("database-v5", "customer");
            _leasesContainer = _cosmosClient.GetContainer("database-v5", "leases");
        }

        /// <summary>
        /// Start Change Feed Processor with enhanced tracking
        /// This demonstrates improved change feed capabilities for V5
        /// </summary>
        public async Task<ChangeFeedProcessor> StartAdvancedChangeFeedProcessorAsync()
        {
            // Create Change Feed Processor for V5 advanced features
            _changeFeedProcessor = _monitoredContainer
                .GetChangeFeedProcessorBuilder<CustomerV5>(
                    "AdvancedCustomerChanges", 
                    HandleAdvancedChangesAsync)
                .WithInstanceName("AdvancedCustomerChanges")
                .WithLeaseContainer(_leasesContainer)
                .WithStartTime(DateTime.UtcNow.AddMinutes(-5)) // Start from 5 minutes ago
                .Build();

            // Start the Change Feed Processor
            await _changeFeedProcessor.StartAsync();

            Console.WriteLine("Advanced Change Feed Processor started - monitoring V5 customer changes");
            return _changeFeedProcessor;
        }

        public async Task StopAdvancedChangeFeedProcessorAsync()
        {
            if (_changeFeedProcessor != null)
            {
                await _changeFeedProcessor.StopAsync();
                Console.WriteLine("Advanced Change Feed Processor stopped");
            }
        }

        /// <summary>
        /// Handle changes with enhanced processing for V5 features
        /// Demonstrates advanced change feed patterns and best practices
        /// </summary>
        private async Task HandleAdvancedChangesAsync(
            IReadOnlyCollection<CustomerV5> changes, 
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"\n=== Advanced Change Feed Event (V5) ===");
            Console.WriteLine($"Batch received: {changes.Count} change(s)");
            Console.WriteLine($"Approximate processing time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            foreach (var customer in changes)
            {
                try
                {
                    await ProcessCustomerChange(customer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing change for customer {customer.id}: {ex.Message}");
                }
            }

            Console.WriteLine("=== End Advanced Change Feed Event ===\n");
        }

        private Task ProcessCustomerChange(CustomerV5 customer)
        {
            Console.WriteLine($"\n📊 Customer Change Detected:");
            Console.WriteLine($"   ID: {customer.customerId}");
            Console.WriteLine($"   Name: {customer.firstName} {customer.lastName}");
            Console.WriteLine($"   Region: {customer.region}");
            Console.WriteLine($"   Email: {customer.emailAddress}");
            Console.WriteLine($"   Order Count: {customer.salesOrderCount}");

            // Demonstrate V5 hierarchical partitioning benefits
            Console.WriteLine($"   Hierarchical Partition: [{customer.region}, {customer.customerId}]");
            
            // Simulate computed property usage (these would be auto-calculated in real V5)
            string computedFullName = $"{customer.firstName} {customer.lastName}";
            int computedYearCreated = DateTime.Parse(customer.creationDate).Year;
            
            Console.WriteLine($"   Computed Properties:");
            Console.WriteLine($"     - Full Name: {computedFullName}");
            Console.WriteLine($"     - Year Created: {computedYearCreated}");

            // Example of advanced processing based on change
            ProcessBusinessRules(customer);
            
            return Task.CompletedTask;
        }

        private Task ProcessBusinessRules(CustomerV5 customer)
        {
            // Example business rules that could be triggered by change feed
            Console.WriteLine($"   🔄 Processing Business Rules:");

            // Rule 1: High-value customer detection
            if (customer.salesOrderCount >= 10)
            {
                Console.WriteLine($"     ⭐ High-value customer detected (>{customer.salesOrderCount} orders)");
                // Could trigger: VIP status update, special offers, account manager assignment
            }

            // Rule 2: Regional analytics update
            Console.WriteLine($"     📈 Updating regional analytics for: {customer.region}");
            // Could trigger: Regional dashboard updates, inventory planning

            // Rule 3: Customer engagement scoring
            var engagementScore = CalculateEngagementScore(customer);
            Console.WriteLine($"     💯 Engagement Score: {engagementScore}/100");
            // Could trigger: Marketing campaigns, retention programs

            // Rule 4: Data consistency checks across hierarchical partitions
            Console.WriteLine($"     ✅ Validating hierarchical partition consistency");
            // Could trigger: Cross-partition validation, data integrity checks
            
            return Task.CompletedTask;
        }

        private int CalculateEngagementScore(CustomerV5 customer)
        {
            // Simple engagement scoring algorithm for demo
            int score = 0;
            
            // Points for order history
            score += Math.Min(customer.salesOrderCount * 10, 50);
            
            // Points for account age (assuming newer accounts get some base points)
            var accountAge = DateTime.UtcNow.Year - DateTime.Parse(customer.creationDate).Year;
            score += Math.Min(accountAge * 5, 30);
            
            // Points for complete profile
            if (!string.IsNullOrEmpty(customer.phoneNumber)) score += 10;
            if (customer.addresses?.Count > 0) score += 10;
            
            return Math.Min(score, 100);
        }
    }
}