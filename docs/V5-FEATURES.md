# CosmicWorks V5 - Advanced Cosmos DB Features

This document describes the new features demonstrated in CosmicWorks database-v5, showcasing the latest Azure Cosmos DB capabilities for advanced data modeling.

## New Features in V5

### 1. Hierarchical Partitioning
**Status**: Preview Feature
**Documentation**: [Hierarchical Partition Keys](https://learn.microsoft.com/en-us/azure/cosmos-db/hierarchical-partition-keys)

Hierarchical partitioning allows multiple levels of partition keys for more granular data distribution and efficient querying.

**Benefits:**
- Better query performance for common access patterns
- More granular control over data distribution  
- Improved scalability for multi-tenant scenarios
- Reduced hot partitions in complex data models

**Demo Usage in V5:**
- Customer container: Partitioned by region for geo-distributed queries
- Sales orders: Co-located with customers by region for efficient joins
- Enables both regional and cross-regional analytics

**Real-world Examples:**
```
E-commerce: [/region, /customerId]
IoT Systems: [/deviceType, /location, /deviceId]
Multi-tenant SaaS: [/tenantId, /userId]
Gaming: [/gameId, /playerId]
```

### 2. Global Secondary Indexes (GSI)
**Status**: Preview Feature  
**Documentation**: [Global Secondary Indexes](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/global-secondary-indexes)

GSI enables additional access patterns without duplicating data, providing alternative query paths.

**Benefits:**
- Query data using different partition keys
- Improved query performance for varied access patterns
- Reduced data duplication compared to manual indexing
- Automatic index maintenance

**Planned V5 Usage:**
- Customer email lookups (email-based partition)
- Product price range queries (price-based partition)  
- Order date-based analytics (date-based partition)

*Note: GSI syntax is simplified in this demo due to current API limitations*

### 3. Computed Properties
**Status**: Generally Available
**Documentation**: [Computed Properties](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/computed-properties)

Computed properties automatically calculate and index derived values at ingestion time.

**Benefits:**
- Automatic indexing for fast queries on calculated fields
- Consistent calculations across all queries
- Reduced application complexity
- Better query performance vs. runtime calculations

**V5 Examples:**
```sql
-- Customer computed properties
fullName: CONCAT(c.firstName, " ", c.lastName)
yearCreated: DateTimePart("yyyy", c.creationDate)

-- Product computed properties  
priceRange: c.price < 50 ? "low" : c.price < 200 ? "medium" : "high"
discountedPrice: c.price * 0.9

-- Order computed properties
orderMonth: DateTimePart("yyyy-MM", c.orderDate)
totalValue: SUM(ARRAY(SELECT VALUE (item.price * item.quantity) FROM item IN c.details))
```

### 4. All Versions and Deletes Change Feed
**Status**: Generally Available
**Documentation**: [Change Feed Modes](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/change-feed-modes)

Enhanced change feed provides comprehensive operation tracking including deletes and version history.

**Benefits:**
- Track Create, Update, and Delete operations
- Access to before/after versions for updates
- Comprehensive audit trails
- Better data synchronization capabilities

**V5 Implementation:**
- Advanced change feed processor for customer changes
- Demonstrates operation type detection
- Shows before/after state comparison
- Includes business rule processing examples

## Demo Functions

### Menu Options (V5 Features)
- **[l]** Demo: Hierarchical Partitioning - Shows regional partitioning strategies
- **[m]** Demo: Computed Properties - Demonstrates calculated field indexing  
- **[n]** Demo: Advanced Change Feed - Shows comprehensive change tracking
- **[o]** Demo: Cross-region queries - Compares partition strategies

### Educational Content

Each demo includes:
- Feature explanation and benefits
- Real-world use case examples
- Performance considerations
- Best practice recommendations
- Migration guidance from earlier versions

## Data Model Evolution

### V1 → V2 → V3 → V4 → V5 Progression

1. **V1**: Relational-style (normalized, many containers)
2. **V2**: Basic denormalization (embedded addresses, optimized partition keys)
3. **V3**: Advanced denormalization (embedded product categories)
4. **V4**: Complete denormalization (customers + orders in same container)
5. **V5**: Advanced features (hierarchical partitioning, computed properties, enhanced change feed)

### V5 Data Model Highlights

- **Regional partitioning**: Optimized for geo-distributed applications
- **Computed properties**: Automatic calculation and indexing of derived fields
- **Enhanced monitoring**: Comprehensive change tracking with operation history
- **Performance optimized**: Leverages latest Cosmos DB capabilities

## Getting Started with V5

1. Deploy infrastructure with V5 containers:
   ```bash
   azd up
   ```

2. Load V5 sample data:
   ```bash
   cd src
   dotnet run
   # Press 'k' to load data
   ```

3. Explore V5 demos:
   ```bash
   # Press 'l' for hierarchical partitioning
   # Press 'm' for computed properties  
   # Press 'n' for advanced change feed
   # Press 'o' for cross-region queries
   ```

## Migration Considerations

### From V4 to V5
- Consider regional data distribution requirements
- Identify calculated fields that benefit from computed properties
- Plan for enhanced change feed processing
- Evaluate hierarchical partitioning for your access patterns

### Performance Impact
- V5 features generally improve query performance
- Computed properties reduce runtime calculation overhead
- Hierarchical partitioning can significantly reduce RU consumption
- Enhanced change feed provides better observability

## Learn More

- [Azure Cosmos DB Documentation](https://docs.microsoft.com/azure/cosmos-db/)
- [NoSQL Data Modeling Guide](https://docs.microsoft.com/azure/cosmos-db/nosql/modeling-data)
- [Partitioning Best Practices](https://docs.microsoft.com/azure/cosmos-db/partitioning-overview)
- [Change Feed Best Practices](https://docs.microsoft.com/azure/cosmos-db/change-feed-processor)