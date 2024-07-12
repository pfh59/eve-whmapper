using Xunit.Abstractions;

namespace WHMapper.Tests;

public class DisplayNameOrderer : ITestCollectionOrderer
        {
            public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
            {
                var orderedCollections = testCollections.OrderBy(collection => collection.DisplayName);
                return orderedCollections;
            }

        }
